using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Conductors;
using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Core.ReAnimationSystem;
using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Game.Core.Utils;
using FunkinSharp.Game.Funkin.Compat;
using FunkinSharp.Game.Funkin.Data.Event;
using FunkinSharp.Game.Funkin.Events;
using FunkinSharp.Game.Funkin.Notes;
using FunkinSharp.Game.Funkin.Skinnable.Notes;
using FunkinSharp.Game.Funkin.Song;
using FunkinSharp.Game.Funkin.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Video;
using osu.Framework.Logging;
using osuTK;

namespace FunkinSharp.Game.Funkin.Screens
{
    // Forced to FNF Gameplay style since its the first chart format to get supported - what i dont know what i was doin when i wrote this bruh
    // It still uses some old code adapted to this one, so it can easily break
    // Although I would love to rewrite this properly and decouple the input logic a lil bit and chart formats etc

    // TODO: Rewrite some handling
    public partial class GameplayScreen : FunkinScreen
    {
        private float bopMult = 2.0f;
        private int bopRate = 4;
        private float spawnTime = 3500;

        private Camera worldCamera;
        private BindableFloat worldZoom = new(0.7f);
        private Vector2 worldPos = Vector2.Zero;
        private Bindable<Vector2> wposBind = new(Vector2.Zero);
        private bool canLerpPos = true;

        private Camera uiCamera;
        private BindableFloat uiZoom = new(1);
        private bool canLerp = true;

        private PauseMenuContainer pauseMenu; // should probably cache this instance but fuck it
        private bool canPause = true;

        private Container<StrumLine> strumLines;
        private StrumLine oppLine;
        private StrumLine plyLine;

        private SpriteText scoreText = new SpriteText()
        {
            Y = -30,
            X = 0,
            Text = "",
            Anchor = Anchor.BottomCentre,
            Origin = Anchor.BottomCentre,
            Font = new FontUsage(family: "OpenSans", size: 38)
        };

        private ComboCounter comboCounter;
        private JudgementDisplay judgementDisplay;

        private List<Drawable> unspawnNotes = [];

        // I would like to move these to a judgement class
        private int misses = 0;
        private int hits = 0;
        private float notesAccuracy = 0.0f;
        private int score = 0;
        private float accuracy = 0;
        private bool ghostTapAllowed;

        private FunkinConductor conductor = new FunkinConductor();

        private SongChartData chartData;
        private SongMetadata metaData;
        private List<Track> tracks;
        private string diff;
        private string format;

        // TODO: Move to judgement display?
        private float safeZoneOffset = (10.0f / 60.0f) * 1000.0f;
        private Dictionary<string, float> weights = new()
        {
            { "sick", 100.0f },
            { "good", 75.0f },
            { "bad", 50.0f },
            { "shit", 25.0f },
            { "miss", -100.0f }
        };

        private Character dad;
        private Character bf;
        private Character gf;
        private bool danced = false;

        private Box bg;
        private Video open;
        private Video part;

        public GameplayScreen(SongChartData chart, SongMetadata meta, List<Track> tracks, string diff, string format)
        {
            chartData = chart;
            metaData = meta;
            this.tracks = tracks;
            this.diff = diff;
            this.format = format;

            Conductor.Instance = conductor;

            Add(bg = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.SlateGray
            });

            Add(worldCamera = new Camera(false)
            {
                Alpha = 0,
                CameraPosition = { BindTarget = wposBind },
            });

            worldZoom.BindValueChanged((ev) => worldCamera.Zoom = ev.NewValue, true);
            string oppChar = "dad";
            if (meta.SongName == "Silly Billy")
                oppChar = "evilLookalike";
            if (meta.SongName == "Test")
                oppChar = "bf";
            worldCamera.Add(gf = new("gf")
            {
                X = -150,
                Y = -50,
                ScrollFactor = new Vector2(0.95f),
                Alpha = (oppChar == "evilLookalike") ? 0 : 1
            });
            worldCamera.Add(dad = new(oppChar)
            {
                X = -550,
            });
            worldCamera.Add(bf = new("bf", true)
            {
                X = 250,
                Y = -175,
            });

            Add(uiCamera = new Camera()
            {
                Alpha = 0,
            });

            uiZoom.BindValueChanged((ev) => uiCamera.Zoom = ev.NewValue);

            strumLines = new Container<StrumLine>()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
            uiCamera.AddRange([strumLines, scoreText]);
            uiCamera.Add(comboCounter = new()
            {
                InitialY = 100,
                Y = 100
            });
            uiCamera.Add(judgementDisplay = new()
            {
                Alpha = 0,
                Y = 0
            });

            // camera isnt ready here so we use the game width instead
            // GameConstants.WIDTH / 8 sets the strumline into bf position
            // Multiplying it by -1 sets it to the dad position

            oppLine = new StrumLine(-(GameConstants.WIDTH / 8), -80)
            {
                BotPlay = { Value = true },
                ConductorInUse = conductor
            };
            oppLine.OnBotHit += onBotHit;        

            plyLine = new StrumLine(GameConstants.WIDTH / 8, -80)
            {
                BotPlay = { Value = false },
                ConductorInUse = conductor
            };
            plyLine.OnBotHit += onBotHit;
            plyLine.OnMiss += noteMiss;

            strumLines.AddRange([oppLine, plyLine]);

            OnActionPressed += gameplayScreen_OnActionPressed;
            OnActionReleased += gameplayScreen_OnActionReleased;
            TargetActions = Actors.NOTE;
        }

        [BackgroundDependencyLoader]
        private void load(FunkinConfig config)
        {
            ghostTapAllowed = config.Get<bool>(FunkinSetting.GhostTapping);

            float savedSpeed = config.Get<float>(FunkinSetting.ScrollSpeed);
            float speed = (float)chartData.ScrollSpeeds[diff];
            if (savedSpeed > 1)
                speed = savedSpeed;

            foreach (StrumLine lane in strumLines)
            {
                lane.DownScroll.BindTo(config.GetBindable<bool>(FunkinSetting.DownScroll));
                if (lane.DownScroll.Value)
                {
                    lane.Y *= -1; // sets it to 80
                    lane.Y += 50;
                }

                lane.Speed.Value = speed;
            }

            if (config.Get<bool>(FunkinSetting.MiddleScroll))
            {
                oppLine.Alpha = 0;
                plyLine.X -= GameConstants.WIDTH / 8;
            }

            if (dad.CharacterName == "evilLookalike")
            {
                dad.Scale = new Vector2(0.75f);
                dad.Y += 200;
                bg.Colour = Colour4.Black;
            }

            if (dad.CharacterName == "bf")
            {
                dad.Y = -180;
            }

            SongNoteData[] cNotes = chartData.Notes[diff];
            foreach (SongNoteData daNote in cNotes)
            {
                // 0 is dad, 1 is bf
                SkinnableNote newNote = new SkinnableNote((float)daNote.Time, daNote.Data % 4, strumLine: (daNote.Data >= 4 || daNote.MustHit) ? 0 : 1, skin: config.Get<string>(FunkinSetting.CurrentNoteSkin));
                unspawnNotes.Add(newNote);
                if (daNote.Length > 0)
                {
                    SkinnableSustain tail = new SkinnableSustain(newNote)
                    {
                        FullLength = (float)daNote.Length,
                        Length = (float)daNote.Length,
                    };
                    unspawnNotes.Add(tail);
                    newNote.BoundToSustain = true;
                }
            }

            tracks[0].Completed += endSong;
            conductor.Bind(tracks[0], [.. tracks[1..]], metaData.TimeChanges);
            conductor.Instrumental.Stop();

            foreach (Track voice in conductor.Voices)
            {
                voice.Stop();
            }
        }

        public override void OnEntering(osu.Framework.Screens.ScreenTransitionEvent e)
        {
            if (dad.CharacterName != "evilLookalike")
            {
                worldCamera.FadeIn(800D, Easing.InQuint);
                uiCamera.FadeIn(1250D, Easing.InQuint).OnComplete((_) =>
                {
                    Scheduler.AddDelayed(() =>
                    {
                        conductor.Instrumental.Start();

                        foreach (ITrack voice in conductor.Voices)
                        {
                            voice.Start();
                        }

                        conductor.Resync();
                    }, 500D);
                });
            }
            else
            {
                canPause = false;
                Add(open = new Video(Paths.GetStream("Videos/open.mp4"))
                {
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(GameConstants.WIDTH, GameConstants.HEIGHT)
                });
            }

            base.OnEntering(e);
        }

        public override void BeatHit()
        {
            if (CurBeat % 2 == 0)
            {
                string animSide = danced ? "Right" : "Left";
                gf.Play($"dance{animSide}");
                danced = !danced;

                if (!dad.CurAnimName.StartsWith("sing"))
                    dad.Play("idle");

                if (!bf.CurAnimName.StartsWith("sing"))
                    bf.Play("idle");
            }

            if (CurBeat % bopRate == 0 && canLerp)
            {
                worldZoom.Value += 0.015f * bopMult;
                uiZoom.Value += 0.0125f * bopMult;
            }
        }

        private double startTime = 0;
        protected override void Update()
        {
            base.Update();

            float elapsed = (float)(Clock.ElapsedFrameTime / 1000);
            if (canLerp)
            {
                float zoomLerp = BoundTo(1 - (float)(elapsed * 3.125), 0, 1);
                worldZoom.Value = Lerp(worldZoom.Default, worldZoom.Value, zoomLerp);
                uiZoom.Value = Lerp(uiZoom.Default, uiZoom.Value, zoomLerp);
            }

            if (canLerpPos)
            {
                float posLerp = BoundTo(elapsed * 2.4f, 0, 1);
                Vector2 newPos = Vector2.Lerp(wposBind.Value, worldPos, posLerp);
                wposBind.Value = newPos;
            }

            if ((worldCamera != null && worldCamera.IsLoaded) &&
                (uiCamera != null && uiCamera.IsLoaded) &&
                chartData != null && conductor.Instrumental != null)
            {
                if (dad.CharacterName == "evilLookalike")
                {
                    if (open != null && open.Buffering && !conductor.Instrumental.IsRunning)
                    {
                        conductor.Instrumental.Start();

                        foreach (ITrack voice in conductor.Voices)
                        {
                            voice.Start();
                        }

                        conductor.Resync();
                    }

                    if (open != null && !open.Buffering && open.PlaybackPosition >= open.Duration)
                    {
                        if (worldCamera.Alpha == 0)
                        {
                            bg.FadeColour(Colour4.SlateGray, conductor.StepLengthMS);
                            worldCamera.FadeIn(conductor.StepLengthMS, Easing.InQuint);
                            uiCamera.FadeIn(conductor.StepLengthMS, Easing.InQuint);
                        }
                        Content.Remove(open, true);
                        open = null;
                        canPause = true;
                    }

                    if (conductor.SongPosition >= 290870 && startTime == 0 && uiCamera.Alpha == 1)
                    {
                        plyLine.MoveToX(plyLine.X - (GameConstants.WIDTH / 4), conductor.BeatLengthMs, Easing.OutSine);
                        uiCamera.FadeOut(conductor.BeatLengthMs, Easing.OutQuint);
                    }

                    if (conductor.SongPosition >= 303033.682080926 && part == null && startTime <= 0)
                    {
                        canLerp = false;
                        worldZoom.SetDefault();
                        startTime = conductor.SongPosition;

                        worldCamera.FadeOut(conductor.StepLengthMS, Easing.InQuint);

                        Add(part = new Video(Paths.GetStream("Videos/SO_STAY_FINAL.mp4"))
                        {
                            RelativeSizeAxes = Axes.None,
                            Size = new Vector2(GameConstants.WIDTH, GameConstants.HEIGHT),
                            Depth = 0,
                        });
                        Content.ChangeChildDepth(uiCamera, -1);
                    }
                    else if (conductor.SongPosition >= 303033.682080926 && startTime > 0 && part != null)
                    {
                        if (part.Buffering && worldZoom.IsDefault)
                        {
                            bg.FadeColour(Colour4.Black, conductor.StepLengthMS);
                            startTime = conductor.SongPosition;
                        }
                        else
                        {
                            if (conductor.SongPosition >= 313959.537572256 && uiCamera.Alpha == 0)
                            {
                                uiCamera.FadeIn(conductor.BeatLengthMs, Easing.InQuint);
                                oppLine.Alpha = 0;
                            }

                            if (part.PlaybackPosition >= part.Duration)
                            {
                                canLerp = true;
                                worldZoom.SetDefault();
                                oppLine.FadeIn(conductor.BeatLengthMs, Easing.InQuint);
                                worldCamera.FadeIn(conductor.StepLengthMS, Easing.InQuint);
                                bg.FadeColour(Colour4.SlateGray, conductor.StepLengthMS);
                                plyLine.MoveToX(plyLine.X + (GameConstants.WIDTH / 4), conductor.MeasureLengthMs, Easing.InSine);
                                Content.Remove(part, true);
                                part = null;
                                Content.ChangeChildDepth(uiCamera, 0);
                            }
                        }
                    }
                }

                conductor.Update();
                processSongEvents();
            }

            if (unspawnNotes.Count > 0 && unspawnNotes[0] != null && unspawnNotes[0] is Note nextNote)
            {
                StrumLine lane = strumLines[nextNote.StrumLine];
                float time = spawnTime;
                if (lane.Speed.Value < 1)
                    time /= lane.Speed.Value;

                if ((nextNote.StrumTime - conductor.SongPosition) < time)
                {
                    lane.Push(nextNote);
                    unspawnNotes.Remove(nextNote);

                    if (nextNote.BoundToSustain)
                    {
                        lane.Push(unspawnNotes[0] as Sustain);
                        unspawnNotes.Remove(unspawnNotes[0] as Sustain);
                    }
                }
            }

            handleSustains();

            scoreText.Text = $"Misses {misses} | Accuracy {float.Floor(accuracy * 100) / 100}% | Score {score}";
        }

        private void gameplayScreen_OnActionPressed(FunkinAction action)
        {
            if (action == FunkinAction.PAUSE && canPause && pauseMenu == null)
            {
                canPause = false;
                AddInternal(pauseMenu = new PauseMenuContainer(this));
                return;
            }

            if (plyLine.BotPlay.Value)
                return;

            foreach (Receptor strum in plyLine.Receptors)
            {
                if (strum.BoundAction == action)
                {
                    int strumIdx = strum.NoteData;
                    List<Note> possibleNotes = [];
                    List<Note> dumbNotes = [];
                    List<int> directionList = [];

                    foreach (Note note in plyLine.NotesGroup)
                    {
                        if ((note.NoteData == strum.NoteData) && note.CanBeHit && !note.TooLate && !note.GoodHit)
                        {
                            if (directionList.Contains(strum.NoteData))
                            {
                                foreach (Note coolNote in possibleNotes)
                                {
                                    if (coolNote.NoteData == note.NoteData
                                            && Math.Abs(note.StrumTime - coolNote.StrumTime) < 10)
                                    {
                                        dumbNotes.Add(note);
                                        break;
                                    }
                                    else if (coolNote.NoteData == note.NoteData && note.StrumTime < coolNote.StrumTime)
                                    {
                                        possibleNotes.Remove(coolNote);
                                        possibleNotes.Add(note);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                possibleNotes.Add(note);
                                directionList.Add(strum.NoteData);
                            }
                        }
                    }

                    foreach (Note note in dumbNotes)
                    {
                        plyLine.DestroyNote(note);
                    }

                    bool dontCheck = false;
                    if (HoldingActions[action] && !directionList.Contains(strumIdx))
                        dontCheck = true;

                    if (possibleNotes.Count > 0 && !dontCheck)
                    {
                        if (!ghostTapAllowed && HoldingActions[action] && !directionList.Contains(strumIdx))
                        {
                            missPress(strumIdx);
                        }

                        foreach (Note coolNote in possibleNotes)
                        {
                            if (HoldingActions[action])
                                noteHit(coolNote);
                        }
                    }
                    else if (!ghostTapAllowed && HoldingActions[action] && !directionList.Contains(strumIdx))
                    {
                        missPress(strumIdx);
                    }

                    if (strum.CurAnimName != "confirm")
                        strum.Play("pressed", false);
                }
            }
        }

        private void gameplayScreen_OnActionReleased(FunkinAction action)
        {
            if (plyLine.BotPlay.Value)
                return;

            foreach (Receptor strum in plyLine.Receptors)
            {
                if (strum.BoundAction == action)
                {
                    strum.Play("static");
                }
            }
        }

        private void handleSustains()
        {
            // This code comes from BotPlay sustain hold logic, just adapted to inputs
            foreach (Sustain sus in plyLine.HittableSustains)
            {
                Note note = sus.Head;
                // no need to check if the head note was a good hit, if the sustain is in hittable sustains, it means that the parent note was hit

                // Finished hold
                if (sus.StrumTime < conductor.SongPosition
                    && sus.Y < sus.Clipper.AnchorPosition.Y + sus.Body.Height // to be more friendly, only count the body height rather than the full sustain
                    && (!sus.Hit && !sus.Missed))
                {
                    sus.Hit = true;
                }

                // Can be hit
                if (sus.StrumTime >= conductor.SongPosition
                    && sus.Y < sus.Clipper.AnchorPosition.Y
                    && (!sus.Hit && !sus.Missed))
                {
                    sus.Holding = HoldingActions[note.BoundAction];

                    Receptor receptor = plyLine.Receptors[note.NoteData];

                    if (plyLine.SustainTimer >= conductor.StepLengthMS)
                    {
                        if (sus.Holding && !sus.Missed)
                        {
                            sus.Holded += (float)conductor.StepLengthMS;
                            receptor.Play("confirm");
                            bf.Play($"sing{receptor.GetNoteDirection().ToUpper()}");
                            // make a function that processes the sustain hit (from sustain head) n shit
                        }
                        else
                        {
                            // Miss
                            sus.Missed = true;
                            sus.Clipper.Masking = false;
                            // make a function that processes the missed sustain (from sustain head) n shit
                        }
                    }
                }
            }

            if ((bf != null && bf.IsLoaded)
                && (!HoldingActions.ContainsValue(true) || plyLine.BotPlay.Value)
                && (bf.CurAnim != null)
                && (bf.HoldTimer > conductor.StepLengthMS * 0.001 * bf.CFile.SingDuration)
                && (bf.CurAnimName.StartsWith("sing"))
                && (!bf.CurAnimName.EndsWith("miss")))
            {
                bf.Play("idle");
            }
        }

        private void onBotHit(Note note)
        {
            StrumLine line = strumLines[note.StrumLine];
            Receptor receptor = line.Receptors[note.NoteData];
            Character curChar = note.StrumLine == 0 ? dad : bf;

            receptor.Play("confirm");
            if (note.BoundToSustain)
                receptor.HoldTimer = 150;
            else
                receptor.HoldTimer = 175;

            curChar.Play($"sing{receptor.GetNoteDirection().ToUpper()}");
            curChar.HoldTimer = 0;
            getVoice(note.StrumLine == 1).Volume.SetDefault();

            if (!note.BoundToSustain)
                line.DestroyNote(note);
            else
            {
                note.Alpha = 0; // Hide the note

                float targetHold = (float)conductor.StepLengthMS * 0.001f * curChar.CFile.SingDuration;
                if (curChar.HoldTimer + 0.2 > targetHold)
                    curChar.HoldTimer = targetHold - 0.2;
            }
        }

        private void noteHit(Note note)
        {
            if (!note.GoodHit)
            {
                Receptor strum = plyLine.Receptors[note.NoteData];

                note.GoodHit = true;
                strum.Play("confirm", true);
                bf.Play($"sing{strum.GetNoteDirection().ToUpper()}");
                bf.HoldTimer = 0;

                getVoice().Volume.SetDefault();
                judgementDisplay.Play(judge((note.StrumTime - (float)conductor.SongPosition)));
                comboCounter.Current.Value++;

                if (!note.BoundToSustain)
                    plyLine.DestroyNote(note);
                else
                    note.Alpha = 0;
            }
        }

        private void noteMiss(Note note)
        {
            StrumLine line = strumLines[note.StrumLine];
            misses++;
            if (note.StrumLine == 1 && comboCounter.Current.Value != 0)
                judgementDisplay.Play("shit");

            bf.Play($"sing{note.GetNoteDirection().ToUpper()}miss");

            judge(Scoring.PBOT1_SHIT_THRESHOLD);
            line.PlayMiss();
            // we are most likely playing as bf but we dont care!!
            getVoice(note.StrumLine == 1).Volume.Value = 0;
            if (note.StrumLine == 1)
                comboCounter.Current.Value = 0;
        }

        private void missPress(int direction)
        {
            misses++;
            judge(Scoring.PBOT1_SHIT_THRESHOLD);
            bf.Play($"sing{plyLine.Receptors[direction].GetNoteDirection().ToUpper()}miss");
            plyLine.PlayMiss();
            getVoice().Volume.Value = 0;
        }

        private void endSong()
        {
            worldZoom.SetDefault();
            uiZoom.SetDefault();
            canLerp = false;
            canPause = false;

            Schedule(() =>
            {
                conductor.Instrumental.Stop();
                foreach (ITrack voice in conductor.Voices)
                {
                    voice.Stop();
                }

                // To clean tracks & sounds
                Paths.ClearStoredMemory();
                Paths.ClearUnusedMemory();

                this.FadeOut(1800D, Easing.OutQuint);
                this.TransformBindableTo(uiZoom, 3.15f, 1250D, Easing.InQuint).OnComplete((_) =>
                {
                    Game.ScreenStack.Push(new SongSelector(ChartFormatSelect.GetTransObjContent(format.ToLower()), format, true));
                });
            });
        }

        public float Lerp(float a, float b, float ratio)
        {
            return a + ratio * (b - a);
        }

        public float BoundTo(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private Track getVoice(bool isPlayer = true)
        {
            // Voice 1 is dad, voice 0 is bf or the default one (from legacy charts)
            if (!isPlayer)
            {
                if (conductor.Voices.Length > 1 && conductor.Voices[1] != null)
                    return conductor.Voices[1];
                else
                    return conductor.Voices[0];
            }
            else
                return conductor.Voices[0];
        }

        private string judge(float ms)
        {
            string judgement = Scoring.JudgeNote(ms);
            notesAccuracy += weights[judgement];
            score += Scoring.ScoreNote(ms);
            hits++;

            // OK SO, i was casting hits as a float so that shi would be like half (100 -> 30) and it got me playin around for almost half an hour bru
            accuracy = Math.Min(100.0f, Math.Max(0.0f, notesAccuracy / hits));

            return judgement;
        }

        private void processSongEvents()
        {
            if (chartData != null && chartData.Events != null && chartData.Events.Length > 0)
            {
                SongEventData[] songEventsToActivate = SongEventRegistry.QueryEvents(chartData.Events, (float)conductor.SongPosition);
                if (songEventsToActivate.Length > 0)
                {
                    foreach (SongEventData data in songEventsToActivate)
                    {
                        float eventAge = (float)(conductor.SongPosition - data.Time);
                        if (eventAge > 1000)
                        {
                            data.Activated = true;
                            continue;
                        }

                        // some custom event cancellation shi here
                        SongEventRegistry.HandleEvent(data);
                        switch (data.Kind)
                        {
                            case "FocusCamera":
                                FocusCameraSongEvent focusEv = (FocusCameraSongEvent)SongEventRegistry.GetEvent(data.Kind);
                                float targetX = focusEv.X;
                                float targetY = focusEv.Y;
                                bool isBeginning = (conductor.SongPosition <= 10); // grace time to avoid breaking the camera lmao!!

                                switch (focusEv.Character)
                                {
                                    case -1:
                                        Logger.Log("Focusing camera on static position");
                                        break;
                                        
                                    case 0:
                                        Vector2 bfCenter = bf.OriginPosition;
                                        bfCenter.Y -= bf.DrawHeight / 4;

                                        if (!isBeginning)
                                        {
                                            targetX += bfCenter.X;
                                            targetY += bfCenter.Y;
                                        }

                                        targetX += bf.CFile.CameraPosition[0];
                                        targetY += bf.CFile.CameraPosition[1];
                                        break;

                                    case 1:
                                        Vector2 dadCenter = dad.OriginPosition;
                                        dadCenter.Y -= dad.DrawHeight / 2;
                                        //dadCenter = -dadCenter;

                                        if (!isBeginning)
                                        {
                                            targetX -= dadCenter.X;
                                            targetY -= dadCenter.Y;
                                        }

                                        targetX += dad.CFile.CameraPosition[0];
                                        targetY += dad.CFile.CameraPosition[1];
                                        break;

                                    case 2:
                                        targetX -= gf.OriginPosition.X / 4;
                                        targetY -= gf.OriginPosition.Y / 4;

                                        targetX -= gf.CFile.CameraPosition[0];
                                        targetY -= gf.CFile.CameraPosition[1];
                                        break;

                                    default:
                                        Logger.Log($"Unknown camera focus {focusEv.Character}");
                                        break;
                                }

                                // apparently unbinding the camera position was the trick, just the same as https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/play/PlayState.hx#L3163
                                switch (focusEv.Ease)
                                {
                                    case "CLASSIC":
                                        canLerpPos = true;
                                        worldPos = new Vector2(targetX, targetY);
                                        break;
                                    case "INSTANT":
                                        canLerpPos = false;
                                        worldPos = new Vector2(targetX, targetY);
                                        worldCamera.CameraPosition.UnbindBindings();
                                        // set target pos so when we can lerp theres no need to make the camera move
                                        this.TransformBindableTo(wposBind, worldPos, 0, Easing.None).OnComplete((_) =>
                                        {
                                            worldCamera.CameraPosition.BindTo(wposBind);
                                            canLerpPos = true;
                                        });
                                        break;
                                    default:
                                        // duration should be already calculated
                                        canLerpPos = false;

                                        if (EasingUtils.FLX_CONVERSION.TryGetValue(focusEv.Ease, out Easing res))
                                        {
                                            worldPos = new Vector2(targetX, targetY);
                                            worldCamera.CameraPosition.UnbindBindings();
                                            worldCamera.TransformBindableTo(worldCamera.CameraPosition, worldPos, focusEv.Duration, res).OnComplete((_) =>
                                            {
                                                worldCamera.CameraPosition.BindTo(wposBind);
                                                canLerpPos = true;
                                            });
                                        }
                                        else
                                        {
                                            Logger.Log($"Couldn't parse {focusEv.Ease} into a valid ease function");
                                            canLerpPos = true;
                                            return;
                                        }
                                        break;
                                }
                                break;

                            case "ZoomCamera":
                                ZoomCameraSongEvent zoomEv = (ZoomCameraSongEvent)SongEventRegistry.GetEvent(data.Kind);
                                switch (zoomEv.Ease)
                                {
                                    case "INSTANT":
                                        cameraZoomHelper(zoomEv.Zoom, 0, zoomEv.IsDirectMode, Easing.None);
                                        break;
                                    default:
                                        // duration should be already calculated

                                        if (EasingUtils.FLX_CONVERSION.TryGetValue(zoomEv.Ease, out Easing res))
                                            cameraZoomHelper(zoomEv.Zoom, zoomEv.Duration, zoomEv.IsDirectMode, res);
                                        else
                                        {
                                            Logger.Log($"Couldn't parse {zoomEv.Ease} into a valid ease function");
                                            return;
                                        }
                                        break;
                                }
                                break;

                            case "SetCameraBop":
                                SetCameraBopSongEvent bopEv = (SetCameraBopSongEvent)SongEventRegistry.GetEvent(data.Kind);
                                bopMult = bopEv.Intensity;
                                bopRate = bopEv.Rate;
                                break;
                        }
                    }
                }
            }
        }

        private void cameraZoomHelper(float zoom, double duration, bool direct, Easing ease)
        {
            float targetZoom = zoom * (direct ? worldZoom.Value : worldZoom.Default);
            if (duration == 0)
                worldZoom.Value = targetZoom;
            else
            {
                canLerp = false;
                this.TransformBindableTo(worldZoom, targetZoom, duration, ease).OnComplete((_) =>
                {
                    canLerp = true;
                });
            }
        }
    }
}
