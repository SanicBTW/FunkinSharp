using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osu.Framework.Graphics.Containers;
using FunkinSharp.Game.Funkin.Sprites;
using FunkinSharp.Game.Core;

namespace FunkinSharp.Game.Tests.Visual
{
    public partial class TestSceneClipping : FunkinSharpTestScene
    {
        private readonly Character bf;
        private readonly ClippedContainer clip;
        private readonly Container clipBounds;

        public TestSceneClipping()
        {
            bf = new Character("bf", true);
            clip = [bf];

            Children = new Drawable[]
            {
                clip,
                new Container
                {
                    Name = "Info Overlay",
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderColour = Color4.Red,
                    BorderThickness = 4,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true,
                            Colour = Colour4.White
                        },
                        new SpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Out of bounds",
                            Colour = Color4.Red,
                            Font = FontUsage.Default.With(size: 36)
                        },
                        clipBounds = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Size = clip.Size,
                            Masking = true,
                            BorderColour = Color4.Green,
                            BorderThickness = 4,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true
                                },
                                new SpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Text = "Masked area",
                                    Colour = Color4.Green,
                                    Font = FontUsage.Default.With(size: 36)
                                }
                            }
                        }
                    }
                },
            };

        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddLabel("BF Control");

            AddStep("Play Idle", () =>
            {
                bf.Play("idle", true);
            });

            AddSliderStep("BF X", -DrawWidth, DrawWidth, 0, (t) =>
            {
                bf.X = t;
            });

            AddSliderStep("BF Y", -DrawHeight, DrawHeight, 0, (t) =>
            {
                bf.Y = t;
            });

            AddSliderStep("BF Scale", 0.15f, 2.5f, 1f, (t) =>
            {
                bf.Scale = new Vector2(t);
            });

            // TODO: The content inside the clipping container should not move alongside its parent (clipping container) - im actually smokin fent
            AddLabel("Clipping Control");

            AddSliderStep("Clip X", -DrawWidth, DrawWidth, 0, (t) =>
            {
                clipBounds.X = clip.X = t;
            });

            AddSliderStep("Clip Y", -DrawHeight, DrawHeight, 0, (t) =>
            {
                clipBounds.Y = clip.Y = t;
            });

            AddSliderStep("Clip Width", 0f, Width, 1, (t) =>
            {
                clip.Width = t;
                clipBounds.Size = clip.Size;
            });

            AddSliderStep("Clip Height", 0f, Height, 1, (t) =>
            {
                clip.Height = t;
                clipBounds.Size = clip.Size;
            });
        }
    }
}
