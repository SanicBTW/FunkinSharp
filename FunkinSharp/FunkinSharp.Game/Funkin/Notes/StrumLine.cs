using System;
using System.Collections.Generic;
using FunkinSharp.Game.Core;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using static FunkinSharp.Game.Core.Utils.EventDelegates;

// https://github.com/SanicBTW/Just-Another-FNF-Engine/blob/master/source/funkin/notes/StrumLine.hx and some testing code that probably works fine now
// TODO: Pooling
// TODO: Proper SustainClip Masking detection (if the player misses then unmask and after its done mask again)
namespace FunkinSharp.Game.Funkin.Notes
{
    // The StrumLine looks correctly positioned when using Anchor & Origin to Center or if its inside a Container that already has those set, any position starts from the center
    public partial class StrumLine : Container
    {
        public Container<Receptor> Receptors { get; private set; }

        public Container<Note> NotesGroup { get; private set; }

        // Each container will have the hit region of the receptor, thus making the possibility to hit notes vary between receptors position
        public List<Container> HitRegions { get; private set; } = [];

        // The reason we store clipped containers on sustain group is because we are clipping each lane sustain
        public Container<ClippedContainer<Sustain>> SustainGroup {  get; private set; }

        private List<Drawable> removeQueue = [];
        private readonly float overrideSize; // Will be applied once the receptors finish loading

        // I should make these as bindable

        // You can set these while making a new StrumLine, e.g: new StrumLine() { BotPlay = true };
        public bool BotPlay = false;

        public int KeyAmount = 4;

        private float speed;

        public float Speed
        {
            get => speed;
            set => speed = float.Round(Conductor.RATE * value, 2);
        }

        // Input shi
        public Camera Camera; // in order to make input work, a camera is needed to check the screen positions of the notes & sustains, i might look for a better way tho
        public float SustainTimer = 0;

        public readonly List<Note> HittableNotes = [];
        public readonly List<Sustain> HittableSustains = [];

        // Events
        public event NoteEvent OnBotHit; // Used to dispatch Bot Hits, when the bot hits a sustain, it will pass the head
        public event NoteEvent OnMiss;

        public StrumLine(float x = 0, float y = 0, string strumType = "funkin", float customSize = -1)
        {
            Position = new Vector2(x, y);

            Receptors = [];
            NotesGroup = [];
            SustainGroup = new() // I spent the last 2 hours trying to fix the position of the sustains while all I had to do was set these to centre bru
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };

            overrideSize = customSize;

            for (int i = 0; i < KeyAmount; i++)
            {
                Receptor receptor = new Receptor(i, strumType);
                receptor.OnLoadComplete += receptor_OnLoadComplete;
                Receptors.Add(receptor);

                // Allocate a new clipped container, we manually control the size
                SustainGroup.Add(new()
                {
                    RelativeSizeAxes = Axes.None,
                });

                // Allocate a new receptor hit region
                HitRegions.Add(new()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            }

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
            setupSustainRegion(receptor);
        }

        private void setupHitRegion(Receptor receptor)
        {
            Container hitRegion = HitRegions[receptor.NoteData];
            // 1px more cuz it fits the whole strum width
            hitRegion.Size = new Vector2((receptor.CurrentFrame.DisplayWidth * receptor.Scale.X) + 1, receptor.CurrentFrame.DisplayHeight);
            hitRegion.Position = new Vector2(receptor.X, receptor.Y);
        }

        private void setupSustainRegion(Receptor receptor)
        {
            // In the test screen I made for gameplay, we used the UICamera DrawHeight but since we don't have access to it, we use the Game Height
            ClippedContainer<Sustain> susRegion = SustainGroup[receptor.NoteData];
            susRegion.Width = GetHitRegion(receptor.NoteData).Width;
            susRegion.Height = (receptor.CurrentFrame.DisplayHeight / 2) + GameConstants.HEIGHT;
            susRegion.Position = new Vector2(receptor.X, receptor.Y + (susRegion.Height / 2));
            susRegion.Masking = BotPlay;
        }

        protected override void Update()
        {
            if (Camera == null && !BotPlay)
                throw new ArgumentException("Input can't work without any camera", "StrumLine.Camera");

            base.Update();

            if (SustainTimer >= Conductor.StepCrochet)
                SustainTimer = 0;
            else
                SustainTimer += (float)Clock.ElapsedFrameTime;

            int downscrollMult = 1;
            float realSpeed = (Speed / Conductor.RATE);

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
                float dist = ((Conductor.RATE * downscrollMult) * -((float)Conductor.Time - strumNote.StrumTime) * realSpeed);

                strumNote.Rotation = recD - 90 + recA;
                strumNote.X = recX + (float)Math.Cos(angleDir) * dist;
                strumNote.Y = recY + (float)Math.Sin(angleDir) * dist;

                if (!BotPlay && Camera.Contains(strumNote.RelativeToAbsoluteFactor) && !HittableNotes.Contains(strumNote))
                    HittableNotes.Add(strumNote);

                if (BotPlay && strumNote.StrumTime <= Conductor.Time && !strumNote.GoodHit)
                    OnBotHit?.Invoke(strumNote);

                if (!strumNote.BoundToSustain && -strumNote.Y > GameConstants.HEIGHT)
                    removeQueue.Add(strumNote);
            }

            // BRO THIS IS GONNA KILL PERFORMANCE REALLY FUCKING BAD - we dont care!!!
            foreach (ClippedContainer<Sustain> susClip in SustainGroup)
            {
                //susClip.Masking = BotPlay; // If the player sets BotPlay mid song, make sure to clip sustains

                foreach (Sustain strumSus in susClip)
                {
                    if (!strumSus.IsAlive)
                        continue;

                    if (-(strumSus.Y + strumSus.Height) > GameConstants.HEIGHT + strumSus.Height)
                    {
                        removeQueue.Add(strumSus);
                    }

                    Note head = strumSus.Head;
                    // Made it to offset the sustain to properly position it on the center of the note
                    strumSus.Margin = new MarginPadding() { Top = head.DrawHeight };

                    if (!BotPlay && Camera.Contains(head.RelativeToAbsoluteFactor) && Camera.Contains(strumSus.DrawPosition) && !HittableSustains.Contains(strumSus))
                        HittableSustains.Add(strumSus);

                    // TODO: Long ahh sustains might not finish on time
                    if (BotPlay && head.GoodHit && strumSus.Holded < strumSus.Length)
                    {
                        // Hit
                        if (SustainTimer >= Conductor.StepCrochet)
                        {
                            strumSus.Holding = true;
                            strumSus.Holded += (float)(Conductor.StepCrochet + Clock.ElapsedFrameTime);
                            OnBotHit?.Invoke(strumSus.Head);
                        }
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
                    SustainGroup[delSustain.Head.NoteData].Remove(delSustain, true);
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
            SustainGroup[newSustain.Head.NoteData].Add(newSustain);
        }

        // This function queues the provided sprite (Note / Sustain) to be deleted next frame
        public void DestroyNote(Drawable sprite)
        {
            removeQueue.Add(sprite);
        }

        // simple wrapper
        public Container GetHitRegion(int noteData)
        {
            return HitRegions[noteData];
        }

        public ClippedContainer<Sustain> GetSustainRegion(int noteData)
        {
            return SustainGroup[noteData];
        }
    }
}
