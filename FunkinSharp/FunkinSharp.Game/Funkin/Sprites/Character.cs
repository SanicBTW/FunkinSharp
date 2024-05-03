using System.Collections.Generic;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Game.Funkin.Compat;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osuTK;

namespace FunkinSharp.Game.Funkin.Sprites
{
    // GF Support soon ( i fucking hate indices ) 14-04-2024
    // Bet (idk how to do it yet bruh) 27-04-2024
    // This is legacy code that will get rewritten very soon probably
    public partial class Character : FrameAnimatedSprite
    {
        public Dictionary<string, string> Aliases { get; private set; } = []; // Holds the aliases of the Sparrow Animations, they are set through the Psych Character JSON File
        public PsychCharacterFile CFile { get; private set; } // Save the character file

        public readonly string CharacterName = "";
        public readonly bool IsPlayer = false;

        public double HoldTimer = 0;

        public bool WillBop = false;

        public Character(string name, bool isPlayer = false)
        {
            CharacterName = name;
            IsPlayer = isPlayer;
            Anchor = Origin = osu.Framework.Graphics.Anchor.Centre;
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

                AnimationFrame newAnim = Animations[realAnim];
                GotoFrame(newAnim.StartFrame);
                CurAnim = newAnim;
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
                if (!Animations.ContainsKey(anim.Name))
                {
                    Logger.Log($"Missing {anim.Name} required by the Character JSON as {anim.Animation}", level: LogLevel.Important);
                    continue;
                }

                Aliases[anim.Animation] = anim.Name; // Funky ref to the animations Dictionary lol
            }

            if (Aliases.ContainsKey("idle"))
                Play("idle");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Logger.Log($"Loaded {CharacterName}");
        }
    }
}
