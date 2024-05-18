using System.Collections.Generic;
using FunkinSharp.Game.Core.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

// https://github.com/SanicBTW/NEOPlayer/blob/master/src/core/DebugUI.hx
// This might get reworked since I'm not a huge fan of this (The naming lmao) but its just the basics for a window that "tracks" some internal values
namespace FunkinSharp.Game.Core.Windows
{
    // Interface to be implemented so the Tracker can properly set up the added components
    // I DONT KNOW WHERE TO MOVE THIS TO :Sob:
    public interface ITrackableComponent
    {
        // Metadata
        public bool DAdded { get; set; } // The component was added to the view
        public bool DScheduled { get; set; } // The component was scheduled to be removed
        public string Name { get; } // The name of THIS component

        // Setup shi
        public FillFlowContainer Parent { get; set; } // The container which was added to the view, only accessible once Init was called
        public void Init(FillFlowContainer content); // The content is the container that THIS component will use
        public bool Refresh(double deltaTime); // Refreshes the component to update values which are being tracked, returning false will schedule the removal of THIS component
    }

    public partial class TrackerWindow : UIWindow
    {
        private FillFlowContainer content;

        private BasicCheckbox paused;
        private List<ITrackableComponent> comp = [];
        private List<ITrackableComponent> pending = [];

        public TrackerWindow() : base("Tracker", "(F2 to Toggle)", FontAwesome.Regular.Eye)
        {
            ScrollContent.Child = content = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y
            };

            ToolbarContent.Add(paused = new BasicCheckbox
            {
                LabelText = "Pause Tracking"
            });
        }

        public void Add(ITrackableComponent app)
        {
            if (app.DAdded)
                return;

            comp.Add(app);
            app.DAdded = true;

            FillFlowContainer compContainer;
            app.Parent = new FillFlowContainer()
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Text = app.Name,
                        Padding = new MarginPadding(5),
                        Font = FontUsage.Default.With(weight: "Bold")
                    },
                    compContainer = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new osuTK.Vector2(22),
                        Padding = new MarginPadding(10),
                    },
                    new Box
                    {
                        Colour = Colour4.Snow,
                        RelativeSizeAxes = Axes.X,
                        Height = 2
                    }
                }
            };

            app.Init(compContainer);
            content.Add(app.Parent);
        }

        public void Remove(ITrackableComponent app)
        {
            if (!app.DAdded || app.DScheduled) // This component was already scheduled
                return;

            app.DScheduled = true; // Schedule for deletion on next frame
            pending.Add(app);
        }

        protected override void Update()
        {
            base.Update();

            if (paused != null && !paused.Current.Value)
            {
                foreach (ITrackableComponent comp in comp)
                {
                    if (comp.DScheduled)
                        continue;

                    if (!comp.Refresh(Clock.ElapsedFrameTime))
                        Remove(comp);
                }
            }

            while (pending.Count > 0)
            {
                ITrackableComponent comp = pending[0];

                comp.DAdded = false;
                Remove(comp.Parent, true);

                pending.Remove(comp);
            }
        }
    }
}
