using System;
using System.Collections.Generic;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Conductors;
using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Funkin.Song;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using static FunkinSharp.Game.Core.Utils.EventDelegates;

// https://github.com/SanicBTW/Just-Another-FNF-Engine/blob/master/source/funkin/notes/StrumLine.hx and some testing code that probably works fine now
namespace FunkinSharp.Game.Funkin.Notes
{
    // The StrumLine looks correctly positioned when using Anchor & Origin to Center or if its inside a Container that already has those set, any position starts from the center
    public partial class StrumLine : Container
    {
        // Receptors shi & input
        public Container<Receptor> Receptors { get; private set; } = [];
        public Container<Box> HitRegions { get; private set; } = [];
        public Camera Camera; // in order to make input work, a camera is needed to check the screen positions of the notes & sustains, i might look for a better way tho
        public float SustainTimer = 0;
        public readonly List<Note> HittableNotes = [];
        public readonly List<Sustain> HittableSustains = [];

        // Stuff that scrolls lmao
        public Container<Note> NotesGroup { get; private set; } = [];
        public Container<Sustain> SustainGroup { get; private set; } = [];

        // Queue to delete notes next frame
        private List<Drawable> removeQueue = [];

        // Events
        public event NoteEvent OnBotHit; // Used to dispatch Bot Hits, when the bot hits a sustain, it will pass the head
        public event NoteEvent OnMiss; // Dispatched when theres a missed note, usually it goes offscreen or cannot be hit anymore

        // Settings
        public BindableFloat Speed = new();
        public BindableBool BotPlay = new();
        public BindableBool DownScroll = new();
        public readonly int KeyAmount = 4;
        private readonly float overrideSize; // Will be applied once the receptors finish loading

        // Conductor lololol
        private BaseConductor conductorInUse;
        public BaseConductor ConductorInUse
        {
            get
            {
                if (conductorInUse == null) return Conductor.Instance;
                return conductorInUse;
            }
            set => conductorInUse = value;
        }

        public StrumLine(float x = 0, float y = 0, string strumType = "funkin", float customSize = -1, int keyAmount = 4)
        {
            Position = new Vector2(x, y);

            KeyAmount = keyAmount;
            overrideSize = customSize;

            Speed.BindValueChanged((v) =>
            {
                if (Speed.Value == v.NewValue) return;

                //Speed.Value = float.Round(SongConstants.PIXELS_PER_MS * v.NewValue, 2);
            });

            SustainGroup.Anchor = SustainGroup.Origin = Anchor.Centre;

            for (int i = 0; i < KeyAmount; i++)
            {
                Receptor receptor = new Receptor(i, strumType);
                receptor.OnLoadComplete += receptor_OnLoadComplete;
                Receptors.Add(receptor);

                // Allocate a new receptor hit region
                HitRegions.Add(new()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                });
            }

            Add(HitRegions);
            Add(Receptors);
            Add(SustainGroup);
            Add(NotesGroup);
        }

        private void receptor_OnLoadComplete(Drawable obj)
        {
            // I have to set the positions here since the receptor is fully loaded here and thus have the necessary stuff
            Receptor receptor = (Receptor)obj;
            float receptorWidth = receptor.CurrentFrame.Width;
            receptor.SetGraphicSize(receptorWidth * receptor.ReceptorData.Size);

            if (overrideSize != -1)
            {
                receptor.SetGraphicSize((receptorWidth / receptor.ReceptorData.Size) * overrideSize);
                receptor.SwagWidth = receptor.ReceptorData.Separation * overrideSize;
            }

            receptor.Position = new Vector2(X - receptor.SwagWidth / 2, Y - receptor.SwagWidth / 2);
            receptor.X += (receptor.NoteData - ((KeyAmount - 1) / 2)) * receptor.SwagWidth;

            receptor.InitialX = float.Floor(receptor.X);
            receptor.InitialY = float.Floor(receptor.Y);
            receptor.Play("static");

            setupHitRegion(receptor);
        }

        private void setupHitRegion(Receptor receptor)
        {
            Box hitRegion = HitRegions[receptor.NoteData];
            // 1px more cuz it fits the whole strum width
            hitRegion.Size = new Vector2((receptor.CurrentFrame.DisplayWidth * receptor.Scale.X) + 1, receptor.CurrentFrame.DisplayHeight);
            hitRegion.Position = new Vector2(receptor.X, receptor.Y);
#if DEBUG
            hitRegion.Alpha = 0.25f;
            Colour4 endCol;
            switch (receptor.GetNoteColor())
            {
                case "purple":
                    endCol = Colour4.Purple;
                    break;
                case "blue":
                    endCol = Colour4.Cyan;
                    break;
                case "green":
                    endCol = Colour4.Green;
                    break;
                case "red":
                    endCol = Colour4.Red;
                    break;
                default:
                    endCol = Colour4.Gray;
                    break;
            }
            hitRegion.Colour = endCol;
#endif
        }

        protected override void Update()
        {
            if (Camera == null && !BotPlay.Value)
                throw new ArgumentException("Input can't work without any camera", "StrumLine.Camera");

            base.Update();

            if (SustainTimer >= ConductorInUse.StepLengthMS)
                SustainTimer = 0;
            else
                SustainTimer += (float)Clock.ElapsedFrameTime;

            int downscrollMult = (DownScroll.Value ? 1 : -1);

            foreach (Note strumNote in NotesGroup)
            {
                if (!strumNote.IsAlive)
                    continue;

                Receptor receptor = Receptors[strumNote.NoteData];
                float recX = receptor.X;
                float recY = receptor.Y;
                float recA = receptor.Rotation;
                float recD = receptor.Direction;

                float angleDir = recD * ((float)Math.PI / 180);
                float dist = SongConstants.PIXELS_PER_MS * ((float)ConductorInUse.SongPosition - strumNote.StrumTime - ConductorInUse.InputOffset) * Speed.Value * downscrollMult;

                strumNote.Rotation = recD - 90 + recA;
                strumNote.X = recX + (float)Math.Cos(angleDir) * dist;
                strumNote.Y = recY + (float)Math.Sin(angleDir) * dist;

                // Camera/HitBox based inputs
                // TODO: Check if it works as intended on downscroll
                if (!BotPlay.Value)
                {
                    if (Camera.Contains(strumNote.RelativeToAbsoluteFactor) && !HittableNotes.Contains(strumNote))
                        HittableNotes.Add(strumNote);

                    if (!strumNote.GoodHit && !strumNote.Missed && HittableNotes.Contains(strumNote))
                    {
                        Box hitRegion = GetHitRegion(strumNote.NoteData);
                        Vector2 hitReg = new Vector2(hitRegion.Y - hitRegion.DrawHeight, hitRegion.DrawHeight);
                        Vector2 noteReg = new Vector2(strumNote.Y, strumNote.DrawHeight);
                        float hitDist = Vector2.Distance(hitReg, noteReg);
                        if (hitDist < 5)
                        {
                            strumNote.Missed = true;
                            OnMiss?.Invoke(strumNote);
                        }
                    }
                }

                // Automatically set good hit on botplay 
                if (BotPlay.Value && strumNote.StrumTime <= ConductorInUse.SongPosition && !strumNote.GoodHit)
                {
                    strumNote.GoodHit = true;
                    OnBotHit?.Invoke(strumNote);
                }

                if (!strumNote.BoundToSustain && (strumNote.Missed || strumNote.GoodHit))
                {
                    if (!DownScroll.Value && -strumNote.Y > GameConstants.HEIGHT)
                        removeQueue.Add(strumNote);

                    if (DownScroll.Value && strumNote.Y > -GameConstants.HEIGHT + strumNote.Height)
                        removeQueue.Add(strumNote);
                }
            }

            foreach (Sustain strumSus in SustainGroup)
            {
                if (!strumSus.IsAlive)
                    continue;

                // TODO: DownScroll
                if (-(strumSus.Y + strumSus.Height) > GameConstants.HEIGHT + strumSus.Height)
                {
                    removeQueue.Add(strumSus);
                }

                Note head = strumSus.Head;
                strumSus.X = head.X;

                if (!BotPlay.Value && head.GoodHit && Camera.Contains(strumSus.RelativeToAbsoluteFactor) && !HittableSustains.Contains(strumSus))
                    HittableSustains.Add(strumSus);

                if (BotPlay.Value && head.GoodHit && strumSus.Holded < strumSus.Length)
                {
                    // Hit
                    if (SustainTimer >= ConductorInUse.StepLengthMS)
                    {
                        strumSus.Holding = true;
                        strumSus.Holded += (float)(ConductorInUse.StepLengthMS);
                        OnBotHit?.Invoke(strumSus.Head);
                    }
                }
            }

            while (removeQueue.Count > 0)
            {
                Drawable sprite = removeQueue[0];

                if (sprite is Note delNote)
                {
                    HittableNotes.Remove(delNote);
                    NotesGroup.Remove(delNote, true);
                }

                if (sprite is Sustain delSustain)
                {
                    HittableSustains.Remove(delSustain);
                    SustainGroup.Remove(delSustain, true);
                }

                removeQueue.Remove(sprite);
            }
        }

        public void Push(Note newNote)
        {
            NotesGroup.Add(newNote);
        }

        public void Push(Sustain newSustain)
        {
            // Push the note into the clipped container
            newSustain.Speed.BindTo(Speed);
            SustainGroup.Add(newSustain);
        }

        // This function queues the provided sprite (Note / Sustain) to be deleted next frame
        public void DestroyNote(Drawable sprite) => removeQueue.Add(sprite);

        // simple wrapper
        public Box GetHitRegion(int noteData) => HitRegions[noteData];
    }
}
