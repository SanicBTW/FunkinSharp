﻿using System;
using System.Collections.Generic;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Conductors;
using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Funkin.Song;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osuTK;
using static FunkinSharp.Game.Core.Utils.EventDelegates;

// https://github.com/SanicBTW/Just-Another-FNF-Engine/blob/master/source/funkin/notes/StrumLine.hx and some testing code that probably works fine now
namespace FunkinSharp.Game.Funkin.Notes
{
    // The StrumLine looks correctly positioned when using Anchor & Origin to Center or if its inside a Container that already has those set, any position starts from the center
    // TODO: Look for another way of setting downscroll or a mult to set the scroll positioning for more dynamic shi
    // TODO: Increase the miss region diff
    // TODO: Fix sustain downscroll
    // TODO: Change the sustain input logic (make it that if the diff between the clip region and the sustain y is less than 0 it can be pressed)
    public partial class StrumLine : Container
    {
        // Receptors shi & input
        public Container<Receptor> Receptors { get; private set; } = [];
        private Container<Container> hitRegions { get; set; } = [];
        public Camera Camera; // in order to make input work, a camera is needed to check the screen positions of the notes & sustains, i might look for a better way tho
        public float SustainTimer = 0;
        public readonly List<Note> HittableNotes = [];
        public readonly List<Sustain> HittableSustains = [];

        // Stuff that scrolls lmao
        public Container<Note> NotesGroup { get; private set; } = [];
        public List<Sustain> SustainGroup { get; private set; } = [];
        private Container<ClippedContainer<Sustain>> sustainClip = []; // this will get added to the render rather than sustain group

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

        // Audio Samples
        // instead of using a list of raw samples, i should use drawable samples for the sake of not creating audio channels and probably make a memory leak
        private List<Sample> missSamples = []; // we store the 3 miss sounds

        public StrumLine(float x = 0, float y = 0, string strumType = "funkin", float customSize = -1, int keyAmount = 4)
        {
            Position = new Vector2(x, y);

            KeyAmount = keyAmount;
            overrideSize = customSize;

            for (int i = 0; i < KeyAmount; i++)
            {
                Receptor receptor = new Receptor(i, strumType);
                receptor.OnLoadComplete += receptor_OnLoadComplete;
                Receptors.Add(receptor);

                // Allocate a new receptor hit region
                hitRegions.Add(new()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                });
            }

            Add(hitRegions);
            Add(Receptors);
            Add(sustainClip);
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
            Container hitRegion = hitRegions[receptor.NoteData];
            // 1px more cuz it fits the whole strum width
            hitRegion.Size = new Vector2((receptor.CurrentFrame.DisplayWidth * receptor.Scale.X) + 1, receptor.CurrentFrame.DisplayHeight);
            hitRegion.Position = new Vector2(receptor.X, receptor.Y);
        }

        [BackgroundDependencyLoader]
        private void load(ISampleStore sampleStore)
        {
            for (int i = 1; i < 4; i++)
            {
                Sample miss = sampleStore.Get($"Miss/missnote{i}.ogg");
                miss.Volume.Value = 0.2;
                missSamples.Add(miss);
            }
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

                if (!BotPlay.Value)
                {
                    // Camera based input check
                    if (Camera.Contains(strumNote.RelativeToAbsoluteFactor) && !HittableNotes.Contains(strumNote))
                        HittableNotes.Add(strumNote);

                    // TODO: Move this to the new system
                    // Hit Region based check
                    if (!strumNote.GoodHit && !strumNote.Missed && HittableNotes.Contains(strumNote))
                    {
                        Container hitRegion = getHitRegion(strumNote.NoteData);
                        float posY = (DownScroll.Value) ? (hitRegion.Y + hitRegion.DrawHeight) : (hitRegion.Y - hitRegion.DrawHeight);
                        Vector2 hitReg = new Vector2(posY, hitRegion.DrawHeight);
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

                    if (DownScroll.Value && strumNote.Y < -GameConstants.HEIGHT + -strumNote.Height)
                        removeQueue.Add(strumNote);
                }
            }

            // Although whats visible on screen is sustainClip group, we manipulate the sustains through SustainGroup
            foreach (Sustain strumSus in SustainGroup)
            {
                if (!strumSus.IsAlive)
                    continue;

                if (strumSus.Missed || strumSus.Hit)
                {
                    if (!DownScroll.Value && -(strumSus.Y + strumSus.Length) > GameConstants.HEIGHT + strumSus.Height)
                        removeQueue.Add(strumSus);

                    if (DownScroll.Value && (strumSus.Y + strumSus.Length) < -GameConstants.HEIGHT + -strumSus.Height)
                        removeQueue.Add(strumSus);
                }

                Note head = strumSus.Head;
                // Made it to offset the sustain to properly position it on the center of the note
                if (DownScroll.Value)
                    strumSus.Margin = new MarginPadding() { Top = head.DrawHeight - (head.DrawHeight / 2f) };
                else
                    strumSus.Margin = new MarginPadding() { Top = head.DrawHeight - (head.DrawHeight / 8) };

                if (!BotPlay.Value)
                {
                    // TODO: proper sustain miss
                    if (!head.GoodHit && head.Missed && !strumSus.Missed)
                    {
                        strumSus.Clipper.Masking = false;
                        strumSus.Missed = true;
                        OnMiss?.Invoke(strumSus.Head);
                    }

                    // The head note was hit, the sustain can be pressed now
                    if (head.GoodHit && !HittableSustains.Contains(strumSus))
                        HittableSustains.Add(strumSus);
                }

                // TODO: Properly use the Hit flag
                if (BotPlay.Value && head.GoodHit && !strumSus.Hit)
                {
                    // Finished hold
                    if (strumSus.StrumTime < conductorInUse.SongPosition
                        && strumSus.Y < strumSus.Clipper.AnchorPosition.Y + strumSus.Height)
                    {
                        strumSus.Hit = true;
                        strumSus.Holding = false;
                    }

                    // Can be hit
                    if (strumSus.StrumTime >= conductorInUse.SongPosition
                        && strumSus.Y < strumSus.Clipper.AnchorPosition.Y)
                    {
                        // Hit
                        if (SustainTimer >= ConductorInUse.StepLengthMS)
                        {
                            strumSus.Holding = true;
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
                    SustainGroup.Remove(delSustain);
                    if (delSustain.Clipper != null)
                        sustainClip.Remove(delSustain.Clipper, true);
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
            newSustain.Downscroll.BindTo(DownScroll);
            SustainGroup.Add(newSustain);
            // Create a clip region that this sustain will get clipped to
            if (newSustain.Clipper == null)
            {
                Receptor receptor = Receptors[newSustain.Head.NoteData];
                ClippedContainer<Sustain> clip = new()
                {
                    RelativeSizeAxes = Axes.None,
                    Width = getHitRegion(newSustain.Head.NoteData).Width,
                    Height = (getHitRegion(newSustain.Head.NoteData).Height / 2) + GameConstants.HEIGHT,
                    Child = newSustain,
                    Masking = true, // start clipped, when missed it stops being clipped
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Position = new Vector2(receptor.X, getHitRegion(newSustain.Head.NoteData).Y),
                    Rotation = (DownScroll.Value) ? 180 : 0
                };
                newSustain.Clipper = clip;
                sustainClip.Add(clip);
            }
        }

        // This function queues the provided sprite (Note / Sustain) to be deleted next frame
        public void DestroyNote(Drawable sprite) => removeQueue.Add(sprite);

        // since most of the input stuff doesnt depend on the hit region any more, theres no need to expose it
        private Container getHitRegion(int noteData) => hitRegions[noteData];

        // plays a cached miss sample within the range of 1-3
        public void PlayMiss() => missSamples[RNG.Next(1, 3)].Play();
    }
}
