using System.Collections.Generic;
using FunkinSharp.Game.Core.Sparrow;
using osu.Framework.Graphics.Animations;
using osu.Framework.Logging;
using osuTK;

namespace FunkinSharp.Game.Core.Animations
{
    // Holds the logic for playing frame based animations using SparrowAnimations
    // This code is somewhat legacy but works, so I'll keep it like this
    // TODO: Proper documentation :sob:
    // REWRITE SOON :smiling_imp:
    public partial class FrameAnimatedSprite : TextureAnimation
    {
        public const double DEFAULT_FRAME_DURATION = 24;

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
        private protected float FrameTimer = 0.0f; // For indices

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

                if (realAnim.Indices != null)
                {
                    // This handles a custom frame timer to play animations properly I should override the animation texture stuff with a new one that already handles this
                    // NOTE: THIS TOOK ME A FUCKING LOT OF TIME AND I BELIEVE IT STILL DOESN'T WORK AS EXPECTED 
                    // Only the indices are allowed to force loop, no need to set the Loop variable which affects the rest of the animations
                    FrameTimer += (float)(Clock.ElapsedFrameTime * Clock.Rate);
                    while (FrameTimer > realAnim.FrameRate && !IsFinished)
                    {
                        FrameTimer -= realAnim.FrameRate;

                        if (CurrentFrameIndex >= realAnim.Indices[^1] && CurFrame >= realAnim.Frames && !realAnim.Loop)
                            IsFinished = true;

                        if (CurrentFrameIndex >= realAnim.Indices[^1])
                            CurFrame = 0;

                        if (IsFinished)
                        {
                            CurFrame = 0;
                            GotoFrame(realAnim.Indices[CurFrame]);
                        }
                        else
                        {
                            CurFrame = (CurFrame + 1) % (realAnim.Frames + 1);
                            GotoFrame(realAnim.Indices[CurFrame]);
                        }
                    }
                }
                else
                {
                    if (CurrentFrameIndex >= realAnim.EndFrame && CurFrame >= realAnim.Frames && !Loop)
                        IsFinished = true;

                    if (CurrentFrameIndex >= realAnim.EndFrame)
                        GotoFrame(realAnim.StartFrame);

                    if (IsFinished)
                    {
                        CurFrame = 0;
                        GotoFrame(realAnim.EndFrame);
                    }
                    else
                        CurFrame++;
                }
            }

            if (!IsFinished && Alive)
                base.Update();
        }

        // Can be overriden to customize the "Play" behaviour
        // THIS is no longer legacy code :fire:
        public virtual bool CanPlayAnimation(bool Force)
        {
            return Force && !IsFinished || IsFinished;
        }

        public virtual void Play(string animName, bool force = true)
        {
            if (Animations.TryGetValue(animName, out var realAnim) && CanPlayAnimation(force))
            {
                if (!force && CurAnimName == animName)
                    return;

                IsFinished = false;
                CurFrame = 0;
                CurAnimName = animName;
                FrameTimer = 0.0f;

                GotoFrame(realAnim.Indices != null ? realAnim.Indices[0] : realAnim.StartFrame);
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

        // Add Frames from the provided range
        // I'm not really convinced with the name lol
        protected void AddFrameRange(int startFrame, int endFrame, double frameDuration = DEFAULT_FRAME_DURATION)
        {
            for (var frame = startFrame; frame < endFrame + 1; frame++)
            {
                AddFrame(Atlas.Frames[frame], frameDuration);
            }
        }

        // Indices stuff, since I didn't really know how to implement them I looked for the easiest way possible

        // https://github.com/HaxeFlixel/flixel/blob/27c47e5cb5780238eacef0171d9f19325b6fcd24/flixel/animation/FlxAnimationController.hx#L405
        protected void AddByIndices(string name, string prefix, int[] indices, string postfix, double frameDuration = DEFAULT_FRAME_DURATION, bool loop = false)
        {
            if (Atlas.Frames.Count > 0)
            {
                List<int> frameIndices = [];
                pushIndicesHelper(frameIndices, prefix, indices, postfix);

                // Replace the existing animation with the indices one
                if (frameIndices.Count > 0)
                    Atlas.Animations[name] = new AnimationFrame(frameIndices.ToArray(), (int)frameDuration, loop);
            }
        }

        // https://github.com/HaxeFlixel/flixel/blob/27c47e5cb5780238eacef0171d9f19325b6fcd24/flixel/animation/FlxAnimationController.hx#L456
        protected int FindSpriteFrame(string prefix, int index, string postfix)
        {
            var i = 0;
            foreach (var name in Atlas.FrameNames)
            {
                if (name.StartsWith(prefix) && name.EndsWith(postfix))
                {
                    var endIndex = name.Length - postfix.Length;
                    if (int.TryParse(name[prefix.Length..endIndex], out var frameIndex))
                    {
                        if (frameIndex == index)
                            return i;
                    }
                }

                i++;
            }

            return -1;
        }

        // https://github.com/HaxeFlixel/flixel/blob/27c47e5cb5780238eacef0171d9f19325b6fcd24/flixel/animation/FlxAnimationController.hx#L715
        private void pushIndicesHelper(in List<int> target, string prefix, int[] indices, string suffix)
        {
            foreach (var index in indices)
            {
                var indexToAdd = FindSpriteFrame(prefix, index, suffix);
                if (indexToAdd != -1)
                    target.Add(indexToAdd);
            }
        }

        // jus like haxeflixel fr fr
        public void SetGraphicSize(float width = 0, float height = 0)
        {
            if (width <= 0 && height <= 0)
                return;

            var newScaleX = width / CurrentFrame.Width;
            var newScaleY = height / CurrentFrame.Height;
            var scale = new Vector2(newScaleX, newScaleY);

            if (width <= 0)
                scale.X = newScaleY;
            else if (height <= 0)
                scale.Y = newScaleX;

            Scale = scale;
        }
    }
}
