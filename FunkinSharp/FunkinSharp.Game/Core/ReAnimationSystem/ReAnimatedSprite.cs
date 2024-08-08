using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osuTK;

namespace FunkinSharp.Game.Core.ReAnimationSystem
{
    // TODO: When finishing an animation, play it in reverse for other animations to look correctly (?)
    public partial class ReAnimatedSprite : Sprite
    {
        protected override DrawNode CreateDrawNode() => new ReAnimatedSpriteNode(this);

        public Dictionary<string, ReAnimation> Animations { get; protected set; } = [];
        public List<ReAnimationFrame> Frames { get; protected set; } = [];

        public ReAnimation CurAnim { get; protected set; } = null;
        public string CurAnimName { get; protected set; } = "";

        protected override void Update()
        {
            base.Update();
            CurAnim?.Update(Clock);
            if (CurAnim != null)
            {
                ReAnimationFrame curFrame = Frames[CurAnim.Frames[CurAnim.CurrentFrameIndex]];
                Texture = curFrame.TextureFrame;

                // behaviour from CustomisableSizeCompositeDrawable

                if (RelativeSizeAxes == Axes.Both) return;

                // instead of accessing RectangleF.Size (creates a new Vector2 per accessor call) we access the property directly
                if ((RelativeSizeAxes & Axes.X) == 0)
                    Width = curFrame.Rect.Width;

                if ((RelativeSizeAxes & Axes.Y) == 0)
                    Height = curFrame.Rect.Height;
            }
        }

        // useful to avoid overlapping animations
        public virtual bool CanPlayAnimation(bool Force)
        {
            return (Force && (!CurAnim?.Finished ?? true)) || (CurAnim?.Finished ?? true);
        }

        // base function to make extending play function somewhat cleaner and shorter
        protected virtual void ApplyNewAnim(string animName, ReAnimation newAnim, bool force, bool reversed, int frame)
        {
            bool oldFlipX = false;
            bool oldFlipY = false;

            if (CurAnim != null && animName != CurAnimName)
            {
                oldFlipX = CurAnim.FlipHorizontal;
                oldFlipY = CurAnim.FlipVertical;
                CurAnim.Stop();
            }

            CurAnim = newAnim;
            CurAnimName = animName;
            CurAnim.Play(force, reversed, frame);

            if (oldFlipX != CurAnim.FlipHorizontal || oldFlipY != CurAnim.FlipVertical)
            {
                Invalidate(); // ?
                Logger.Log($"Mismatching flip flags, invalidating");
            }
        }

        // Made them virtual so the extended classes can override the default behaviour

        public virtual void Play(string animName, bool force = true, bool reversed = false, int frame = 0)
        {
            if (Animations.TryGetValue(animName, out ReAnimation realAnim) && CanPlayAnimation(force))
                ApplyNewAnim(animName, realAnim, force, reversed, frame);
            else
                Logger.Log($"Animation Name ({animName}) not found", level: LogLevel.Error);
        }

        public virtual void Reset() => CurAnim?.Reset();

        public virtual void Finish() => CurAnim?.Finish();

        public virtual void Stop() => CurAnim?.Stop();

        public virtual void Pause() => CurAnim?.Pause();

        public virtual void Resume() => CurAnim?.Resume();

        public virtual void Reverse() => CurAnim?.Reverse();

        public virtual bool Paused
        {
            get => CurAnim?.Paused ?? false;
            set
            {
                if (value)
                    CurAnim?.Pause();
                else
                    CurAnim?.Resume();
            }
        }

        // HaxeFlixel type shi
        public void SetGraphicSize(float width = 0, float height = 0)
        {
            if (width <= 0 && height <= 0)
                return;

            var newScaleX = width / Texture.DisplayWidth;
            var newScaleY = height / Texture.DisplayHeight;
            var scale = new Vector2(newScaleX, newScaleY);

            if (width <= 0)
                scale.X = newScaleY;
            else if (height <= 0)
                scale.Y = newScaleX;

            Scale = scale;
        }
    }
}
