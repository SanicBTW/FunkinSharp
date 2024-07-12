using System;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Tests.Visual;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace FunkinSharp.Game.Tests.Sustains
{
    // First ever testing and implementation of a DrawNode in my life, keeping it as the base for future stuff, just a reference in case I forget something
    [TestFixture]
    public partial class TestSceneRawDrawing : FunkinSharpTestScene
    {
        private readonly TestNodeOverride spr;

        public TestSceneRawDrawing()
        {
            Add(spr = new TestNodeOverride()
            {
                Texture = Paths.GetTexture("NoteTypes/funkin/NOTE_hold_assets.png", false),
                Width = 52,
            });

            AddSliderStep("Sustain Height", 0, 500, 50, (e) =>
            {
                spr.Height = e;
            });
        }

        private partial class TestNodeOverride : Sprite
        {
            protected override DrawNode CreateDrawNode() => new TestDrawNode(this);
        }

        private partial class TestDrawNode : SpriteDrawNode
        {
            public TestDrawNode(Sprite source) : base(source) { }

            protected override void Blit(IRenderer renderer)
            {
                // We use ScreenSpaceDrawQuad to get the whole rendering area that the sprite needs
                // The TextureCoords are to get the texture sizes and shi

                // Here we get how many times we are going to repeat the texture based on the texture rect provided on draw quad
                var tileCountY = (int)Math.Ceiling(ScreenSpaceDrawQuad.Height / TextureCoords.Height);

                for (float y = 0; y < tileCountY; y++)
                {
                    // we get the y position of the tile
                    var tilePosY = ScreenSpaceDrawQuad.TopLeft.Y + y * TextureCoords.Height;

                    // we get the height of the tile (in order to keep filling the space with a partial tile instead of trying to fill full tiles)
                    var tileHeight = Math.Min(TextureCoords.Height, ScreenSpaceDrawQuad.BottomRight.Y - tilePosY);

                    // we get the "vertex quad" or the space of the sprite we are gonna render or the quad for the current tile, i actually dont know what this is for
                    var tiledQuad = new Quad(
                        new Vector2(ScreenSpaceDrawQuad.TopLeft.X, tilePosY),
                        new Vector2(ScreenSpaceDrawQuad.TopLeft.X + ScreenSpaceDrawQuad.Width, tilePosY),
                        new Vector2(ScreenSpaceDrawQuad.TopLeft.X, tilePosY + tileHeight),
                        new Vector2(ScreenSpaceDrawQuad.TopLeft.X + ScreenSpaceDrawQuad.Width, tilePosY + tileHeight)
                    );

                    // we draw the "vertex quad" using the sprite colour info AND we get the region of the texture that we want to draw
                    // usually the width is bound to the sprites width in order to keep it simple and not that hardcoded
                    // and the height is bound to tile height to properly fill the space until a full tile
                    renderer.DrawQuad(Texture, tiledQuad, DrawColourInfo.Colour, textureRect: new RectangleF(0, 0, Source.DrawWidth, tileHeight));
                }
            }

        }
    }
}
