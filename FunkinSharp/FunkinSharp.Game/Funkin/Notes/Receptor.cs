using osu.Framework.Graphics;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Funkin.Compat;
using System.Collections.Generic;
using osu.Framework.Logging;
using osu.Framework.Allocation;
using FunkinSharp.Game.Core.Stores;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace FunkinSharp.Game.Funkin.Notes
{
    // Legacy code but wit JSON stuff hehe
    // TODO: In order to add soft modding (?) I would need to add some basic interp or sum shit
    // I'm thinking about Wren or Luau?¿ probably wren
    // https://github.com/SanicBTW/Just-Another-FNF-Engine/blob/master/source/funkin/notes/Receptor.hx
    public partial class Receptor : FrameAnimatedSprite
    {
        public Dictionary<string, string> Aliases { get; private set; } = []; // Holds the aliases of the Sparrow Animations

        public float SwagWidth { get; private set; }

        public readonly int NoteData;
        public readonly string NoteType;
        public readonly bool IsPlayer;

        public FEReceptorData ReceptorData { get; private set; }
        // public WrenModule NoteModule { get; private set; } - soon hehe

        public float InitialX;
        public float InitialY;

        public float SetAlpha = 0.8f;

        public double HoldTimer = 0;
        public float Direction = 90;

        public Receptor(int noteData = 0, bool isPlayer = false, string noteType = "funkin")
        {
            NoteData = noteData;
            NoteType = noteType;
            IsPlayer = isPlayer;

            Anchor = Origin = Anchor.Centre;
        }

        protected override void Update()
        {
            if (HoldTimer > 0)
            {
                HoldTimer -= (Clock.ElapsedFrameTime / 1000); // Flixel Elapsed like
                if (HoldTimer <= 0)
                {
                    Play("static", false);
                    HoldTimer = 0;
                }
            }

            base.Update();
        }

        public override void Play(string animName, bool Force = true)
        {
            if (Aliases.TryGetValue(animName, out string realAnim) && CanPlayAnimation(Force))
            {
                if (!Force && CurAnimName == animName)
                    return;

                IsFinished = false;
                CurFrame = 0;
                CurAnimName = animName;

                AnimationFrame newAnim = Animations[realAnim];
                GotoFrame(newAnim.StartFrame);
                CurAnim = newAnim;

                Alpha = (animName == "confirm") ? 1 : SetAlpha;
            }

            if (!Aliases.ContainsKey(animName))
            {
                Logger.Log($"Animation Alias ({animName}) not found on Receptor {NoteData} with skin {NoteType}", level: LogLevel.Error);
                base.Play(animName, Force);
            }
        }

        [BackgroundDependencyLoader]
        private void load(JSONStore jsonStore, SparrowAtlasStore sparrowStore)
        {
            if (Note.DataCache.ContainsKey(NoteType))
                ReceptorData = Note.DataCache[NoteType];
            else
                ReceptorData = Note.DataCache[NoteType] = jsonStore.Get<FEReceptorData>($"NoteTypes/{NoteType}/{NoteType}");

            SwagWidth = ReceptorData.Separation * ReceptorData.Size;

            Atlas = sparrowStore.GetSparrow($"NoteTypes/{NoteType}/{ReceptorData.Texture}");
            foreach (Texture frame in Atlas.Frames)
            {
                AddFrame(frame, DEFAULT_FRAME_DURATION);
            }

            string stringSect = GetNoteDirection();
            Aliases["static"] = $"arrow{stringSect.ToUpper()}";
            Aliases["pressed"] = $"{stringSect} press";
            Aliases["confirm"] = $"{stringSect} confirm";
            Scale = new Vector2(ReceptorData.Size);
            Play("static", false);
        }

        public string GetNoteDirection()
        {
            return ReceptorData.Actions[NoteData];
        }

        public string GetNoteColor()
        {
            return ReceptorData.Colors[NoteData];
        }
    }
}
