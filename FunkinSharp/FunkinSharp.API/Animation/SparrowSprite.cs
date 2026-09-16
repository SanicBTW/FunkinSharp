using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace FunkinSharp.API.Animation;

// only serves as renderer
// for a future c# based modding most of these will be protected and overridable
public partial class SparrowSprite : Sprite
{
    protected override DrawNode CreateDrawNode() => new SparrowSpriteDrawNode(this);

    private SparrowFrame? currentFrame { get; set; }

    // we dont want an invalidation everytime a property changes... but we might actually need it
    private bool flipHorizontal { get; set; }
    private bool flipVertical { get; set; }

    public void Apply(SparrowAnimationData animation, SparrowFrame frame)
    {
        Texture = frame.Atlas;
        currentFrame = frame;
        flipHorizontal = animation.FlipX;
        flipVertical = animation.FlipY;
    }

    public void Clean()
    {
        Texture = null;
        currentFrame = null;
        flipHorizontal = flipVertical = false;
    }

    // https://github.com/SanicBTW/FunkinSharp/blob/legacy/FunkinSharp/FunkinSharp.Game/Core/ReAnimationSystem/ReAnimatedSpriteNode.cs
    private partial class SparrowSpriteDrawNode(SparrowSprite source) : SpriteDrawNode(source)
    {
        private SparrowSprite source => (SparrowSprite)Source;

        private SparrowFrame? currentFrame;
        private RectangleF textureCoords;

        public override void ApplyState()
        {
            base.ApplyState();

            currentFrame = source.currentFrame;
            if (Texture != null)
            {
                textureCoords = new RectangleF(
                    0,
                    0,
                    Texture.DisplayWidth,
                    Texture.DisplayHeight
                );
            }
        }

        protected override void Blit(IRenderer renderer)
        {
            if (currentFrame == null)
                return;

            RectangleF sourceRect = currentFrame.SourceRect;

            Vector2 origin = ScreenSpaceDrawQuad.TopLeft;

            Vector2 xAxis =
                (ScreenSpaceDrawQuad.TopRight - origin)
                / DrawRectangle.Width;

            Vector2 yAxis =
                (ScreenSpaceDrawQuad.BottomLeft - origin)
                / DrawRectangle.Height;

            Vector2 topLeft =
                origin
                - xAxis * sourceRect.X
                - yAxis * sourceRect.Y;

            Vector2 topRight =
                topLeft + xAxis * Texture.DisplayWidth;

            Vector2 bottomLeft =
                topLeft + yAxis * Texture.DisplayHeight;

            Vector2 bottomRight =
                topRight + yAxis * Texture.DisplayHeight;

            if (source.flipHorizontal)
            {
                (topLeft, topRight) = (topRight, topLeft);
                (bottomLeft, bottomRight) = (bottomRight, bottomLeft);
            }

            if (source.flipVertical)
            {
                (topLeft, bottomLeft) = (bottomLeft, topLeft);
                (topRight, bottomRight) = (bottomRight, topRight);
            }

            Quad drawQuad = new Quad(
                topLeft,
                topRight,
                bottomLeft,
                bottomRight
            );

            renderer.DrawQuad(
                Texture,
                drawQuad,
                DrawColourInfo.Colour,
                null,
                null,
                new Vector2(
                    InflationAmount.X / DrawRectangle.Width,
                    InflationAmount.Y / DrawRectangle.Height),
                null,
                textureCoords
            );
        }
    }
}
