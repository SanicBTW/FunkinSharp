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
        public TestSceneClipping()
        {
            Character bf = new Character("bf", true)
            {
                X = 100,
                Scale = new Vector2(1.5f)
            };

            ClippedContainer clipp = new ClippedContainer
            {
                Width = 0.55f,
                Height = 0.25f
            };

            clipp.Add(bf);

            Children = new Drawable[]
            {
                clipp,
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
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Size = clipp.Size,
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

            AddStep("Play", () =>
            {
                bf.Play("idle", true);
            });
        }
    }
}
