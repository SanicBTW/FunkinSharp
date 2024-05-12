using System.Collections.Generic;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Sprites;
using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Game.Funkin.Compat;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osuTK;

namespace FunkinSharp.Game.Funkin.Sprites
{
    // GF Support soon ( i fucking hate indices ) 14-04-2024
    // Bet (idk how to do it yet bruh) 27-04-2024
    // I finally added the GF Support but I'm not conviced enough :sob: 08-05-2024
    // This is legacy code that will get rewritten very soon probably
    public partial class Character : FrameAnimatedSprite, ICameraScrollable
    {
        public Dictionary<string, string> Aliases { get; private set; } = []; // Holds the aliases of the Sparrow Animations, they are set through the Psych Character JSON File
        private Dictionary<string, Vector2> animOffsets = [];

        public PsychCharacterFile CFile { get; private set; } // Save the character file

        public readonly string CharacterName = "";
        public bool IsPlayer = false; // You can set the flag on runtime that indicates whether the player is botplay or not, TODO: make this a bindable

        public double HoldTimer = 0;

        public bool WillBop = false;

        private Vector2 scrollFactor = Vector2.One;
        public Vector2 ScrollFactor { get => scrollFactor; set => scrollFactor = value; }

        public Character(string name, bool isPlayer = false)
        {
            CharacterName = name;
            IsPlayer = isPlayer;
            Anchor = Origin = Anchor.Centre;
        }

        // hehe some old code, its just the same as my fnf engine so uhhhh i hope it works¿
        protected override void Update()
        {
            if (CurAnim != null)
            {
                if (!IsPlayer)
                {
                    if (CurAnimName.StartsWith("sing"))
                        HoldTimer += Clock.ElapsedFrameTime / 1000; // to mimic haxeflixel elapsed

                    if (HoldTimer >= Conductor.StepCrochet * (CFile.SingDuration / 1000))
                    {
                        Play("idle");
                        HoldTimer = 0;
                    }
                }
                else
                {
                    if (CurAnimName.StartsWith("sing"))
                        HoldTimer += Clock.ElapsedFrameTime / 1000;
                    else
                        HoldTimer = 0;

                    if (CurAnimName.EndsWith("miss") && IsFinished)
                        Play("idle");
                }

                // Bop next frame its possible
                if (WillBop)
                {
                    Play("idle");
                    WillBop = false;
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
                FrameTimer = 0.0f;

                AnimationFrame newAnim = Animations[realAnim];
                GotoFrame(newAnim.Indices != null ? newAnim.Indices[0] : newAnim.StartFrame);
                CurAnim = newAnim;

                if (animOffsets.ContainsKey(animName))
                    Margin = new MarginPadding()
                    {
                        Right = animOffsets[animName].X,
                        Bottom = animOffsets[animName].Y
                    };
                else
                    Margin = new MarginPadding(0);
            }

            if (!Aliases.ContainsKey(animName))
            {
                Logger.Log($"Animation Alias ({animName}) not found for character {CharacterName}", level: LogLevel.Error);
                base.Play(animName, Force);
            }
        }

        //  I kinda want to change this since it requires a lot of stuff
        [BackgroundDependencyLoader]
        private void load(JSONStore jsonStore, SparrowAtlasStore sparrowStore)
        {
            // Thanks to stores now the parsing process was reduced by a lot
            CFile = jsonStore.Get<PsychCharacterFile>($"Characters/{CharacterName}/{CharacterName}");
            Atlas = sparrowStore.GetSparrow($"Characters/{CharacterName}/{CFile.Image}");

            foreach (Texture frame in Atlas.Frames)
            {
                AddFrame(frame, DEFAULT_FRAME_DURATION);
            }

            if (CFile.Scale != 1)
                Scale = new Vector2(CFile.Scale);

            // We set the aliases for the JSON Animations declarations (anim -> name) / (Animation -> Name)
            foreach (PsychAnimArray anim in CFile.Animations)
            {
                if (anim.Indices != null && anim.Indices.Length > 0)
                {
                    AddByIndices(anim.Animation, anim.Name, anim.Indices, "", anim.FPS, anim.Loop);
                    Aliases[anim.Animation] = anim.Animation; // Set the alias to its own name since we added an animation as that name yknow
                }
                else
                    Aliases[anim.Animation] = anim.Name; // Funky ref to the animations Dictionary lol

                if (!Animations.ContainsKey(anim.Name))
                {
                    Logger.Log($"Missing {anim.Name} required by the Character JSON as {anim.Animation}", level: LogLevel.Important);
                    continue;
                }

                if (anim.Offsets != null && anim.Offsets.Length > 1)
                    addOffset(anim.Animation, new Vector2(anim.Offsets[0], anim.Offsets[1]));
            }

            if (Aliases.ContainsKey("idle"))
                Play("idle");
            if (Aliases.ContainsKey("danceRight"))
                Play("danceRight");

            X += CFile.Position[0];
            Y += CFile.Position[1];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Logger.Log($"Loaded {CharacterName}");
        }

        // TODO: Make a separate class that has these, and try to insert this class in the middle of sprite inheritances
        private void addOffset(string name, Vector2 offset) => animOffsets[name] = offset;

        public void ResizeOffsets(float newScale = -1)
        {
            newScale = newScale == -1 ? Scale.X : newScale;

            Dictionary<string, Vector2> end = [];
            foreach (var entries in animOffsets)
            {
                end[entries.Key] = new Vector2(entries.Value.X * newScale, entries.Value.Y * newScale);
            }
            animOffsets = end;
        }
    }
}
