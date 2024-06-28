using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osuTK;

namespace FunkinSharp.Game.Core.ReAnimationSystem
{
    // TODO: Add methods to manipulate the current animation playback
    public partial class ReAnimatedSprite : Sprite
    {
        protected override DrawNode CreateDrawNode() => new ReAnimatedSpriteNode(this);

        protected Dictionary<string, ReAnimation> Animations { get; set; } = [];

        public ReAnimation CurAnim { get; protected set; } = null;
        public string CurAnimName { get; protected set; } = "";

        protected override void Update()
        {
            base.Update();
            CurAnim?.Update(Clock);
            if (CurAnim != null)
            {
                // behaviour from CustomisableSizeCompositeDrawable

                if (RelativeSizeAxes == Axes.Both) return;

                Vector2 frameSize = CurAnim.Frames[CurAnim.CurrentFrameIndex].SourceSize;
                if ((RelativeSizeAxes & Axes.X) == 0)
                    Width = frameSize.X;

                if ((RelativeSizeAxes & Axes.Y) == 0)
                    Height = frameSize.Y;
            }
        }

        public virtual bool CanPlayAnimation(bool Force)
        {
            // wtf
            return Force && ((CurAnim != null && (!CurAnim.Finished || CurAnim.Finished)) || CurAnim == null);
        }

        // base function to make extending play function somewhat cleaner and shorter
        protected void ApplyNewAnim(string animName, ReAnimation newAnim)
        {
            if (CurAnimName == animName)
            {
                CurAnim?.Reset(); // if the animation we want to play is the current one, only reset its properties
                return;
            }

            CurAnim = newAnim;
            CurAnim.Reset(); // just in case the animation was used before
            CurAnimName = animName;
        }

        public virtual void Play(string animName, bool force = true)
        {
            if (Animations.TryGetValue(animName, out ReAnimation realAnim) && CanPlayAnimation(force))
            {
                if (!force && CurAnimName == animName)
                    return;

                ApplyNewAnim(animName, realAnim);
            }
            else
                Logger.Log($"Animation Name ({animName}) not found", level: LogLevel.Error);
        }

        // HaxeFlixel type shi
        public void SetGraphicSize(float width = 0, float height = 0)
        {
            if (width <= 0 && height <= 0)
                return;

            var newScaleX = width / DrawWidth;
            var newScaleY = height / DrawHeight;
            var scale = new Vector2(newScaleX, newScaleY);

            if (width <= 0)
                scale.X = newScaleY;
            else if (height <= 0)
                scale.Y = newScaleX;

            Scale = scale;
        }

        public List<string> GetAnimations() => [..Animations.Keys];
    }
}
