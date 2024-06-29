using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace FunkinSharp.Game.Core.ReAnimationSystem
{
    internal partial class ReAnimatedSpriteNode : SpriteDrawNode
    {
        protected new ReAnimatedSprite Source => (ReAnimatedSprite)base.Source;

        public ReAnimatedSpriteNode(Sprite source) : base(source) { }

        protected override void Blit(IRenderer renderer)
        {
            ReAnimation anim = Source.CurAnim;
            if (anim != null)
            {
                Texture.Bind();

                Vector2 topLeft = ScreenSpaceDrawQuad.TopLeft;
                Vector2 topRight = ScreenSpaceDrawQuad.TopRight;
                Vector2 bottomLeft = ScreenSpaceDrawQuad.BottomLeft;
                Vector2 bottomRight = ScreenSpaceDrawQuad.BottomRight;

                if (anim.FlipHorizontal)
                {
                    /* just like
                     float temp1 = topLeft.X;
                    topLeft.X = topRight.X;
                    topRight.X = temp1;
                     */

                    (topRight.X, topLeft.X) = (topLeft.X, topRight.X);
                    (bottomRight.X, bottomLeft.X) = (bottomLeft.X, bottomRight.X);
                }

                if (anim.FlipVertical)
                {
                    (bottomLeft.Y, topLeft.Y) = (topLeft.Y, bottomLeft.Y);
                    (bottomRight.Y, topRight.Y) = (topRight.Y, bottomRight.Y);
                }

                Quad drawQuad = new Quad(
                    topLeft,
                    topRight,
                    bottomLeft,
                    bottomRight
                );

                // ok so for some fucking reason this sometimes crashes but not too often, like its extremely rare AND I DONT KNOW WHY
                ReAnimationFrame currentFrame = anim.Frames[anim.CurrentFrameIndex % anim.Frames.Count];
                RectangleF drawRect = new RectangleF(
                    currentFrame.Frame.Location,
                    Source.ApplyFrameOffsets ? currentFrame.SourceSize : currentFrame.Frame.Size
                );

                if (Source.ApplyFrameOffsets)
                {
                    // apply the position offset before interesecting the rect
                    drawRect.Location -= currentFrame.Offset;
                    // this fixes an issue where it draws outside of the current frame but breaks some offsets
                    // also because of this, some sizes might be incorrect
                    drawRect.Intersect(currentFrame.Frame);
                }

                renderer.DrawQuad(Texture, drawQuad, DrawColourInfo.Colour, textureRect: drawRect);
            }
        }
    }
}
