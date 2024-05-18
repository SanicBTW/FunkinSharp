using System;
using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Funkin.Text;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneDebugWindow : FunkinSharpTestScene
    {
        private TestWindow window = new TestWindow();

        public TestSceneDebugWindow()
        {
            window.AddButton("Hello 1", () =>
            {

            });
            window.AddButton("Hello 2", () =>
            {

            });
            window.AddButton("Hello 3", () =>
            {

            });

            window.Add(new AtlasText("Content"));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            window.ToggleVisibility();
            Add(window);
        }

        private partial class TestWindow : UIWindow
        {
            private FillFlowContainer content;

            public TestWindow() : base("Debug Window", "placeholder", FontAwesome.Regular.Angry)
            {
                ScrollContent.Child = content = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(5)
                };
            }

            public new void Add(Drawable drawable)
            {
                content.Add(drawable);
            }

            public new void AddButton(string text, Action action)
            {
                base.AddButton(text, action);
            }
        }
    }
}
