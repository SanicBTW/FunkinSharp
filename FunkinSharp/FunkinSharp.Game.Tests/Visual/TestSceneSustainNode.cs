using System;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Game.Core.Utils;
using FunkinSharp.Game.Funkin.Notes;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneSustainNode : FunkinSharpTestScene
    {
        public TestSceneSustainNode()
        {
            AddToggleStep("Animated", (s) =>
            {
                regen(s);
            });
        }

        private void regen(bool fromGlobalSheet)
        {
            Clear();

            int prevPos = -250;
            for (int i = 0; i < 4; i++)
            {
                Add(new SustainBody(i, fromGlobalSheet)
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    X = prevPos += 100
                });
            }
        }

        private partial class SustainBody : FrameAnimatedSprite
        {
            private BufferedContainer<BodyInternal> bufferedBody;

            // Reference to the private textureHolder from TextureAnimation
            private BodyInternal textureHolder;

            public override Drawable CreateContent()
            {
                textureHolder = new()
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };

                var field = ReflectionUtils.GetField<TextureAnimation>("textureHolder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(this, textureHolder);

                if (animated)
                {
                    bufferedBody = new BufferedContainer<BodyInternal>(null, false, true)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = textureHolder,
                        FrameBufferScale = new Vector2(1, 0)
                    };
                    return bufferedBody;
                }

                return textureHolder;
            }

            private int nnotedata;
            private bool animated;

            public SustainBody(int noteData, bool fromGlobalSheet) : base()
            {
                nnotedata = noteData;
                animated = fromGlobalSheet;

                Alpha = 0.8f;
                Height = Sustain.SustainHeight(350, 2.5f);
            }

            [BackgroundDependencyLoader]
            private void load(SparrowAtlasStore sparrowStore)
            {
                if (animated)
                {
                    Atlas = sparrowStore.GetSparrow("NoteTypes/funkin/NOTE_assets");

                    string[] colors = ["purple", "blue", "green", "red"];
                    string key = $"{colors[nnotedata]} hold piece";
                    if (Animations.TryGetValue(key, out AnimationFrame anim))
                    {
                        AddFrameRange(anim.StartFrame, anim.EndFrame);
                        CurAnim = anim;
                        CurAnimName = key;
                    }
                }
                else
                    AddFrame(Paths.GetTexture("NoteTypes/funkin/NOTE_hold_assets.png"));
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                textureHolder.Nnotedata = nnotedata;
                textureHolder.Animated = animated;

                if (!animated)
                    Width = CurrentFrame.DisplayHeight / 2;
            }

            private partial class BodyInternal : Sprite
            {
                protected override DrawNode CreateDrawNode() => new SustainDrawNode(this);

                public int Nnotedata;
                public bool Animated;

                public BodyInternal() { }

                // TODO: Might move the base blitting/drawing logic into a core node "TiledDrawNode" in order to simplify code
                private partial class SustainDrawNode : SpriteDrawNode
                {
                    protected new BodyInternal Source => (BodyInternal)base.Source;

                    // -1 to automatically set the value (only for sizes cuz im cool)
                    public RectangleF TextureRegion = new(0, 0, -1, -1);

                    public SustainDrawNode(Sprite source) : base(source)
                    {
                        if (!Source.Animated)
                        {
                            // 52 is the max width of the singular sustain sprite
                            TextureRegion.Width = 52;
                            if (Source.Nnotedata > 0)
                                TextureRegion.X += TextureRegion.Width * Source.Nnotedata * 2;
                        }
                        else
                        {
                            float aspectRatio = Source.Texture.DisplayWidth / Source.Texture.DisplayHeight;
                            Source.Scale = new Vector2(1 / aspectRatio, 1);
                        }
                    }

                    protected override void Blit(IRenderer renderer)
                    {
                        int tileCountY = (int)Math.Ceiling(ScreenSpaceDrawQuad.Height / TextureCoords.Height);

                        for (float y = 0; y < tileCountY; y++)
                        {
                            float tilePosY = ScreenSpaceDrawQuad.TopLeft.Y + y * TextureCoords.Height;

                            float tileHeight = Math.Min(TextureCoords.Height, ScreenSpaceDrawQuad.BottomRight.Y - tilePosY);

                            Quad tiledQuad = new Quad(
                                new Vector2(ScreenSpaceDrawQuad.TopLeft.X, tilePosY),
                                new Vector2(ScreenSpaceDrawQuad.TopLeft.X + ScreenSpaceDrawQuad.Width, tilePosY),
                                new Vector2(ScreenSpaceDrawQuad.TopLeft.X, tilePosY + tileHeight),
                                new Vector2(ScreenSpaceDrawQuad.TopLeft.X + ScreenSpaceDrawQuad.Width, tilePosY + tileHeight)
                            );

                            RectangleF rect = TextureRegion;
                            if (rect.Width <= -1)
                                rect.Width = Source.DrawWidth;

                            if (rect.Height <= -1)
                                rect.Height = tileHeight;

                            renderer.DrawQuad(Texture, tiledQuad, DrawColourInfo.Colour, textureRect: rect);
                        }
                    }
                }
            }
        }
    }
}
