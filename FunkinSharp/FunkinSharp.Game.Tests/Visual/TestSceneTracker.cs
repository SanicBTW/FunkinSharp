using FunkinSharp.Game.Core.Windows;
using FunkinSharp.Game.Funkin.Text;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneTracker : FunkinSharpTestScene
    {
        private TrackerWindow tracker;

        public TestSceneTracker()
        {
            tracker = new TrackerWindow();
            Add(tracker);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            tracker.ToggleVisibility();

            tracker.Add(new TestTrackable());
            tracker.Add(new TestTrackable());
            tracker.Add(new TestTrackable2());
        }

        internal partial class TestTrackable : ITrackableComponent
        {
            private AtlasText test;
            private double waitTime = 0;

            bool ITrackableComponent.DAdded { get; set; }
            bool ITrackableComponent.DScheduled { get; set; }

            string ITrackableComponent.Name => "Test Trackable";

            FillFlowContainer ITrackableComponent.Parent { get; set; }

            void ITrackableComponent.Init(FillFlowContainer content)
            {
                content.Add(new SpriteText()
                {
                    Text = $"Hello {RNG.Next(0, 15)}",
                    Font = FontUsage.Default,
                });
                content.Add(test = new AtlasText("0", 0, 0, AtlasFontType.FREEPLAY_CLEAR));
            }

            bool ITrackableComponent.Refresh(double deltaTime)
            {
                if (waitTime > 3000)
                {
                    waitTime = 0;
                    test.Text = $"{RNG.Next(0, 1000)}";
                }
                else
                    waitTime += deltaTime;

                return true;
            }
        }

        internal partial class TestTrackable2 : ITrackableComponent
        {
            bool ITrackableComponent.DAdded { get; set; }
            bool ITrackableComponent.DScheduled { get; set; }

            string ITrackableComponent.Name => "Test Trackable 2";

            FillFlowContainer ITrackableComponent.Parent { get; set; }

            void ITrackableComponent.Init(FillFlowContainer content)
            {
                content.Add(new SpriteText()
                {
                    Text = $"Hello {RNG.Next(0, 15)}",
                    Font = FontUsage.Default,
                });
            }

            bool ITrackableComponent.Refresh(double deltaTime)
            {
                return true;
            }
        }
    }
}
