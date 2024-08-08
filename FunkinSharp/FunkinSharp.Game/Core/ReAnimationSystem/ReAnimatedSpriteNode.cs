using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace FunkinSharp.Game.Core.ReAnimationSystem
{
    // Now this node works only as a manipulation node, basically modifies the drawing quad, not the texture
    internal partial class ReAnimatedSpriteNode : SpriteDrawNode
    {
        protected new ReAnimatedSprite Source => (ReAnimatedSprite)base.Source;

        public ReAnimatedSpriteNode(Sprite source) : base(source) { }

        protected override void Blit(IRenderer renderer)
        {
            ReAnimation anim = Source.CurAnim;
            if (anim != null)
            {
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

                if (Source.Frames[anim.CurrentFrameIndex].Rotated)
                    Source.Rotation = -90;

                Quad drawQuad = new Quad(
                    topLeft,
                    topRight,
                    bottomLeft,
                    bottomRight
                );

                Texture.Bind();
                // even though specifying a texture rect (which should be inside the frame) it draws in the whole texture, so I don't really know what to do?
                renderer.DrawQuad(Texture, drawQuad, DrawColourInfo.Colour/*, anim.Frames[anim.CurrentFrameIndex].Rect*/);
            }
        }
    }
}
