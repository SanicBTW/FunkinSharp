using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Conductors;
using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Game.Funkin.Notes;
using FunkinSharp.Game.Funkin.Song;
using Microsoft.Diagnostics.Runtime.DacInterface;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Stores;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestScenePooling : FunkinSharpTestScene
    {
        public Container<Receptor> Receptors { get; private set; }
        public SongNoteData[] SongNotes { get; private set; }
        public SongChartData Chart { get; private set; }
        public SongMetadata Metadata { get; private set; }

        private DrawablePool<PoolableNote> notePool;
        private static int displayCount;
        private SpriteText? count;
        private Container<PoolableNote> notes;
        private int nextNoteIndex = -1;

        private List<SongNoteData> spawnedNotes = [];
        private FunkinConductor conductor = new FunkinConductor();
        private ConductorWindow debug = new ConductorWindow();

        public TestScenePooling()
        {
            Conductor.Instance = conductor;

            Receptors = new()
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Y = -150
            };
            Add(Receptors);

            for (int i = 0; i < 4; i++)
            {
                Receptor receptor = new Receptor(i);
                receptor.OnLoadComplete += receptor_OnLoadComplete;
                Receptors.Add(receptor);
            }

            notePool = new DrawablePool<PoolableNote>(15, 55);
            Add(notePool);

            notes = new Container<PoolableNote>();
            Add(notes);

            debug.Scale = new Vector2(0.75f);
            debug.State.Value = Visibility.Visible;
            Add(debug);

            count = new SpriteText();
            Add(count);
        }

        private void receptor_OnLoadComplete(Drawable obj)
        {
            // I have to set the positions here since the receptor is fully loaded here and thus have the necessary stuff
            Receptor receptor = (Receptor)obj;
            float receptorWidth = receptor.CurrentFrame.Width;
            receptor.SetGraphicSize(receptorWidth * receptor.ReceptorData.Size);

            receptor.Position = new Vector2(X - receptor.SwagWidth / 2, Y - receptor.SwagWidth / 2);
            receptor.X += (receptor.NoteData - ((4 - 1) / 2)) * receptor.SwagWidth;

            receptor.InitialX = float.Floor(receptor.X);
            receptor.InitialY = float.Floor(receptor.Y);
            receptor.Play("static");
        }

        [BackgroundDependencyLoader]
        private void load(ITrackStore store, JSONStore jStore)
        {
            Chart = jStore.Get<SongChartData>("fresh/fresh-chart-erect");
            Metadata = jStore.Get<SongMetadata>("fresh/fresh-metadata-erect");

            SongNotes = Chart.Notes["nightmare"];

            conductor.Bind(Paths.GetTrack("fresh/Inst-erect.ogg"), [Paths.GetTrack("fresh/Voices-bf-erect.ogg"), Paths.GetTrack("fresh/Voices-dad-erect.ogg")], Metadata.TimeChanges);
            conductor.Instrumental.Start();

            foreach (Track voice in conductor.Voices)
            {
                voice.Start();
            }
        }

        private PoolableNote consumeDrawable(SongNoteData data)
        {
            var drawable = notePool.Get(d =>
            {
                d.Data = data;
            });

            notes.Add(drawable);

            return drawable;
        }

        protected override void Update()
        {
            base.Update();
            conductor.Update();

            if (count != null)
            {
                count.Text =
                    $"available: {notePool.CountAvailable} poolSize: {notePool.CurrentPoolSize} inUse: {notePool.CountInUse} excessConstructed: {notePool.CountExcessConstructed}";
            }

            if (SongNotes != null)
            {
                if (SongNotes.Length > 0 && SongNotes[0] != null && SongNotes[0] is SongNoteData note)
                {
                    /*
                    if ((nextNote.StrumTime - Conductor.Time) < 3500)
                    {
                        strumLines[nextNote.StrumLine].Push(nextNote);
                        unspawnNotes.Remove(nextNote);
                    }
                    if (note == null) return;
                    if (note.Time < Conductor.Instance.SongPosition - 160.0) // GameConstants.HIT_WINDOW_MS
                    {
                        nextNoteIndex = noteIndex + 1;
                        continue;
                    }

                    if (note.Time > Conductor.Time + (GameConstants.HEIGHT / SongConstants.PIXELS_PER_MS)) // RENDER_DISTANCE_MS
                        break;

                    consumeDrawable(note);

                    nextNoteIndex = noteIndex + 1;*/
                    if ((note.Time - Conductor.Instance.SongPosition) < 3500 && !spawnedNotes.Contains(note))
                    {
                        consumeDrawable(note);
                        spawnedNotes.Add(note);
                    }
                }

                /*
                if (unspawnNotes.Count > 0 && unspawnNotes[0] != null && unspawnNotes[0] is Sustain nextSus)
                {
                    float time = (float)(nextSus.Head.StrumTime + (nextSus.Height / Conductor.StepCrochet));
                    if ((time - Conductor.Time) < 3500)
                    {
                        strumLines[nextSus.Head.StrumLine].Push(nextSus);
                        unspawnNotes.Remove(nextSus);
                    }
                }
                /*
                for (int noteIndex = 0; nextNoteIndex < SongNotes.Length; noteIndex++)
                {
                    SongNoteData note = SongNotes[noteIndex];
                    if (note == null) continue;
                    if (note.Time < Conductor.Time - 160.0) // GameConstants.HIT_WINDOW_MS
                    {
                        nextNoteIndex = noteIndex + 1;
                        continue;
                    }

                    if (note.Time > Conductor.Time + (GameConstants.HEIGHT / Conductor.RATE)) // RENDER_DISTANCE_MS
                        break;

                    consumeDrawable(note);

                    nextNoteIndex = noteIndex + 1;
                }*/
            }

            foreach (PoolableNote strumNote in notes)
            {
                if (!strumNote.IsAlive || strumNote.Cock.StrumLine != 1)
                    continue;

                Receptor receptor = Receptors[strumNote.Cock.NoteData];
                float recX = receptor.X;
                float recY = receptor.Y;
                float recA = receptor.Rotation;
                float recD = receptor.Direction;

                float angleDir = recD * ((float)Math.PI / 180);
                float dist = ((SongConstants.PIXELS_PER_MS * 1) * -((float)Conductor.Instance.SongPosition - strumNote.Cock.StrumTime) * ((float)Chart.GetScrollSpeed("nightmare") / SongConstants.PIXELS_PER_MS));

                strumNote.Rotation = recD - 90 + recA;
                strumNote.X = recX + (float)Math.Cos(angleDir) * dist;
                strumNote.Y = recY + (float)Math.Sin(angleDir) * dist;

                if (strumNote.Cock.StrumTime <= Conductor.Instance.SongPosition && !strumNote.Cock.GoodHit)
                {
                    receptor.Play("confirm");
                    receptor.HoldTimer = 0.15;
                    strumNote.Expire();
                }

                if (-strumNote.Y > GameConstants.HEIGHT || strumNote.Cock.StrumTime >= conductor.Instrumental.Length)
                {
                    strumNote.Expire();
                }
            }
        }

        private partial class PoolableNote : PoolableDrawable
        {
            public Note Cock;
            private SongNoteData data;
            public SongNoteData Data
            {
                get => data;
                set
                {
                    data = value;
                    Cock = new Note((float)Data.Time, Data.Data % 4, strumLine: Data.Data >= 4 ? 0 : 1);
                }
            }

            public PoolableNote()
            {
                AutoSizeAxes = Axes.Both;
                InternalChild = Cock = new Note(0, 0);
            }

            public void AddChild(Drawable drawable) => AddInternal(drawable);

            public new bool IsDisposed => base.IsDisposed;

            public int PreparedCount { get; private set; }
            public int FreedCount { get; private set; }

            protected override void PrepareForUse()
            {
                Y = -9999;
                PreparedCount++;
            }

            protected override void FreeAfterUse()
            {
                base.FreeAfterUse();
                FreedCount++;
            }
        }
    }
}
