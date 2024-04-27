using System.Collections.Generic;
using FunkinSharp.Game.Core.Sparrow;
using osu.Framework.Graphics.Animations;
using osu.Framework.Logging;

namespace FunkinSharp.Game.Core
{
    // Holds the logic for playing frame based animations using SparrowAnimations
    // This code is somewhat legacy but works, so I'll keep it like this

    public partial class FrameAnimatedSprite : TextureAnimation
    {
        // Holds available animations
        private protected Dictionary<string, AnimationFrame> Animations => Atlas != null ? Atlas.Animations : [];

        // The reference of "this" SparrowAtlas
        private protected SparrowAtlas Atlas;

        // Animation controller
        public AnimationFrame? CurAnim { get; private protected set; } = null;
        public string CurAnimName { get; private protected set; } = "";
        public int CurFrame { get; private protected set; } = 0;
        public bool IsFinished { get; private protected set; } = false;

        // Lifetime control
        public bool Alive = true;

        public FrameAnimatedSprite()
        {
            Loop = false;
        }

        // Handles the custom SparrowAnimations system
        protected override void Update()
        {
            if (CurAnim != null)
            {
                var realAnim = (AnimationFrame)CurAnim; // Make it a not-null variable

                // this thing is complicated, idk if i should use CurrentFrameIndex >= realAnim.EndFrame or CurFrame >= realAnim.Frames
                if (CurrentFrameIndex >= realAnim.EndFrame && CurFrame >= realAnim.Frames)
                //if (CurFrame >= realAnim.Frames)
                {
                    IsFinished = true;
                }

                if (CurrentFrameIndex >= realAnim.EndFrame)
                {
                    GotoFrame(realAnim.StartFrame);
                }

                if (IsFinished)
                {
                    CurFrame = 0;
                    GotoFrame(realAnim.EndFrame);
                }
                else
                    CurFrame++;
            }

            if (!IsFinished && Alive)
            {
                base.Update();
            }
        }

        // Can be overriden to customize the "Play" behaviour
        // THIS is no longer legacy code :fire:
        public virtual bool CanPlayAnimation(bool Force)
        {
            return (Force && !IsFinished || IsFinished);
        }

        public virtual void Play(string animName, bool force = true)
        {
            if (Animations.TryGetValue(animName, out AnimationFrame realAnim) && CanPlayAnimation(force))
            {
                if (!force && CurAnimName == animName)
                    return;

                IsFinished = false;
                CurFrame = 0;
                CurAnimName = animName;

                GotoFrame(realAnim.StartFrame);
                CurAnim = realAnim;
            }

            if (!Animations.ContainsKey(animName))
            {
                Logger.Log($"Animation Name ({animName}) not found", level: LogLevel.Error);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            Alive = false;
            base.Dispose(isDisposing);
        }

        /*
        // Some virtuals so they can be overriden on extending classes
        // Its done like this so I know what I can use when loading the sprites lmao

        [BackgroundDependencyLoader]
        public virtual void Load(SparrowAtlasStore sparrowStore)
        {
        }*/
    }
}
