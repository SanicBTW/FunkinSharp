using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace FunkinSharp.API.Animation;

// only serves as renderer
public partial class SparrowSprite : Sprite
{
    protected override DrawNode CreateDrawNode() => new SparrowSpriteDrawNode(this);

    public SparrowFrame? CurrentFrame { get; private set; }

    // we dont want an invalidation everytime a property changes... but we might actually need it
    public bool FlipHorizontal { get; set; }
    public bool FlipVertical { get; set; }

    public void Apply(SparrowAnimationData animation, SparrowFrame frame)
    {
        Texture = frame.Atlas;
        CurrentFrame = frame;
        FlipHorizontal = animation.FlipX;
        FlipVertical = animation.FlipY;
    }

    public void Clean()
    {
        Texture = null;
        CurrentFrame = null;
        FlipHorizontal = FlipVertical = false;
    }

    // https://github.com/SanicBTW/FunkinSharp/blob/legacy/FunkinSharp/FunkinSharp.Game/Core/ReAnimationSystem/ReAnimatedSpriteNode.cs
    private partial class SparrowSpriteDrawNode(SparrowSprite source) : SpriteDrawNode(source)
    {
        protected new SparrowSprite Source => (SparrowSprite)base.Source;

        private SparrowFrame? currentFrame;

        public override void ApplyState()
        {
            base.ApplyState();

            currentFrame = Source.CurrentFrame;
        }

        protected override void Blit(IRenderer renderer)
        {
            if (currentFrame == null)
                return;

            Vector2 topLeft = ScreenSpaceDrawQuad.TopLeft;
            Vector2 topRight = ScreenSpaceDrawQuad.TopRight;
            Vector2 bottomLeft = ScreenSpaceDrawQuad.BottomLeft;
            Vector2 bottomRight = ScreenSpaceDrawQuad.BottomRight;

            if (Source.FlipHorizontal)
            {
                (topRight.X, topLeft.X) = (topLeft.X, topRight.X);
                (bottomRight.X, bottomLeft.X) = (bottomLeft.X, bottomRight.X);
            }

            if (Source.FlipVertical)
            {
                (bottomLeft.Y, topLeft.Y) = (topLeft.Y, bottomLeft.Y);
                (bottomRight.Y, topRight.Y) = (topRight.Y, bottomRight.Y);
            }

            RectangleF sourceRect = currentFrame.SourceRect;
            Quad drawQuad = new Quad(topLeft, topRight, bottomLeft, bottomRight);
            renderer.DrawQuad(Texture, drawQuad, DrawColourInfo.Colour, sourceRect, null,
                new Vector2(InflationAmount.X / sourceRect.Width, InflationAmount.Y / sourceRect.Height),
                null, null); // should batch...
        }
    }
}
