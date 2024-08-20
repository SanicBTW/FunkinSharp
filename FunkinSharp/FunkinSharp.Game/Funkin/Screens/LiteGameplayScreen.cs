using System;
using System.Collections.Generic;
using System.Linq;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Conductors;
using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Core.Input;
using FunkinSharp.Game.Funkin.Data.Event;
using FunkinSharp.Game.Funkin.Events;
using FunkinSharp.Game.Funkin.Notes;
using FunkinSharp.Game.Funkin.Skinnable.Notes;
using FunkinSharp.Game.Funkin.Song;
using FunkinSharp.Game.Funkin.Sprites;
using FunkinSharp.Game.Funkin.Sprites.Touch;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Platform;

namespace FunkinSharp.Game.Funkin.Screens
{
    // copy of gameplay screen to test out mobile support without too much shit in it
    public partial class LiteGameplayScreen : FunkinScreen
    {
        private float bopMult = 2.0f;
        private int bopRate = 4;
        private float spawnTime = 3500;

        private Camera uiCamera;
        private BindableFloat uiZoom = new(1);
        private bool canLerp = true;

        private Container<StrumLine> strumLines;
        private StrumLine oppLine;
        private StrumLine plyLine;

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

        private Hitbox hitbox;
        private FunkinInputManager inputManager;

        public LiteGameplayScreen(SongChartData chart, SongMetadata meta, List<Track> tracks, string diff, string format)
        {
            chartData = chart;
            metaData = meta;
            this.tracks = tracks;
            this.diff = diff;
            this.format = format;

            Conductor.Instance = conductor;

            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.SlateGray
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
            uiCamera.AddRange([strumLines]);
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

            uiCamera.Add(hitbox = new(this));

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
        private void load(GameHost host, FunkinConfig config)
        {
            host.AvailableInputHandlers.OfType<MouseHandler>().FirstOrDefault().UseRelativeMode.Value = false;

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = (FunkinInputManager)GetContainingInputManager();
            inputManager.LocalUserPlaying.Value = true;
        }

        public override void OnEntering(osu.Framework.Screens.ScreenTransitionEvent e)
        {
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

            base.OnEntering(e);
        }

        public override void BeatHit()
        {
            if (CurBeat % bopRate == 0 && canLerp)
            {
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
                uiZoom.Value = Lerp(uiZoom.Default, uiZoom.Value, zoomLerp);
            }

            if ((uiCamera != null && uiCamera.IsLoaded) &&
                chartData != null && conductor.Instrumental != null)
            {
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
        }

        private void gameplayScreen_OnActionPressed(FunkinAction action)
        {
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
        }

        private void onBotHit(Note note)
        {
            StrumLine line = strumLines[note.StrumLine];
            Receptor receptor = line.Receptors[note.NoteData];

            receptor.Play("confirm");
            if (note.BoundToSustain)
                receptor.HoldTimer = 150;
            else
                receptor.HoldTimer = 175;

            getVoice(note.StrumLine == 1).Volume.SetDefault();

            if (!note.BoundToSustain)
                line.DestroyNote(note);
            else
            {
                note.Alpha = 0; // Hide the note
            }
        }

        private void noteHit(Note note)
        {
            if (!note.GoodHit)
            {
                Receptor strum = plyLine.Receptors[note.NoteData];

                note.GoodHit = true;
                strum.Play("confirm", true);

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
            plyLine.PlayMiss();
            getVoice().Volume.Value = 0;
        }

        private void endSong()
        {
            uiZoom.SetDefault();
            canLerp = false;
            inputManager.LocalUserPlaying.Value = true;

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
    }
}
