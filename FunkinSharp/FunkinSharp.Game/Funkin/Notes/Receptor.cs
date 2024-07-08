using osu.Framework.Graphics;
using FunkinSharp.Game.Funkin.Compat;
using System.Collections.Generic;
using osu.Framework.Logging;
using osu.Framework.Allocation;
using FunkinSharp.Game.Core.Stores;
using System;
using FunkinSharp.Game.Core.ReAnimationSystem;

namespace FunkinSharp.Game.Funkin.Notes
{
    // Forever Engine:Rewrite receptor and some other JAFE stuff (which is mostly based off FE:R)
    public partial class Receptor : ReAnimatedSprite
    {
        public Dictionary<string, string> Aliases { get; protected set; } = []; // Aliases for the Animations

        public readonly int NoteData;
        public readonly string NoteType;

        public float InitialX;
        public float InitialY;

        public float SwagWidth;
        public FEReceptorData ReceptorData { get; protected set; }

        public float SetAlpha = 0.8f;

        public double HoldTimer = 0;
        public float Direction = 90;

        public FunkinAction BoundAction;

        public Receptor(int noteData = 0, string noteType = "funkin")
        {
            NoteData = noteData;
            NoteType = noteType;
            Anchor = Origin = Anchor.Centre;
        }

        protected override void Update()
        {
            if (HoldTimer > 0)
            {
                HoldTimer -= Clock.ElapsedFrameTime;
                if (HoldTimer <= 0)
                {
                    Play("static");
                    HoldTimer = 0;
                }
            }

            base.Update();
        }

        // Look into this someday, basically its for animated sheets
        public override bool CanPlayAnimation(bool Force)
        {
            return base.CanPlayAnimation(Force) || (CurAnim?.Loop ?? true);
        }

        public override void Play(string animName, bool force = true)
        {
            if (Aliases.TryGetValue(animName, out string realAnim) && CanPlayAnimation(force))
            {
                if (!force && CurAnimName == animName)
                    return;

                ApplyNewAnim(animName, Animations[realAnim]);

                Alpha = (animName == "confirm") ? 1 : SetAlpha;
            }
            else
            {
                Logger.Log($"Animation Alias ({animName}) not found on Receptor {NoteData} with skin {NoteType}", level: LogLevel.Error);
                base.Play(animName, force);
            }
        }

        [BackgroundDependencyLoader]
        private void load(JSONStore jsonStore, SparrowAtlasStore sparrowStore)
        {
            if (NoteType == null)
                return;

            // The JSONStore cache should've already cached the string content but it doesn't cache the object
            // So we do some magic stuff here because my dumb ahh decided to cache the content but not the object

            if (Note.DataCache.ContainsKey(NoteType))
                ReceptorData = Note.DataCache[NoteType];
            else
                ReceptorData = Note.DataCache[NoteType] = jsonStore.Get<FEReceptorData>($"NoteTypes/{NoteType}/{NoteType}");

            BoundAction = (FunkinAction)Enum.Parse(typeof(FunkinAction), "NOTE_" + GetNoteDirection().ToUpper());
            SwagWidth = ReceptorData.Separation * ReceptorData.Size;

            string stringSect = GetNoteDirection();
            Aliases["static"] = $"arrow{stringSect.ToUpper()}";
            Aliases["pressed"] = $"{stringSect} press";
            Aliases["confirm"] = $"{stringSect} confirm";

            sparrowStore.GetSparrowNew(this, $"NoteTypes/{NoteType}/{ReceptorData.Texture}");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Play("static");

            ReAnimation staticAnim = Animations[Aliases["static"]];
            if (staticAnim.Frames.Count > 1)
                staticAnim.Loop = true; // needed for animated sheets
        }

        public string GetNoteDirection() => ReceptorData.Actions[NoteData];

        public string GetNoteColor() => ReceptorData.Colors[NoteData];
    }
}
