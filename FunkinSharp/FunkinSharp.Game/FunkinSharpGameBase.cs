using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Resources;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osuTK;

namespace FunkinSharp.Game
{
    public partial class FunkinSharpGameBase : osu.Framework.Game
    {
        public DependencyContainer GameDependencies { get; protected set; }
        // Anything in this class is shared between the test browser and the game implementation.
        // It allows for caching global dependencies that should be accessible to tests, or changing
        // the screen scaling for all components including the test browser and framework overlays.

        protected override Container<Drawable> Content { get; }

        protected FunkinSharpGameBase()
        {
            // Ensure game and tests scale with window size and screen DPI.
            base.Content.Add(Content = new DrawSizePreservingFillContainer
            {
                // You may want to change TargetDrawSize to your "default" resolution, which will decide how things scale and position when using absolute coordinates.
                TargetDrawSize = new Vector2(1366, 768)
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Resources.AddStore(new DllResourceStore(typeof(FunkinSharpResources).Assembly));
            GameDependencies.CacheAs(this);
            GameDependencies.CacheAs(Dependencies);
            GameDependencies.CacheAs(new SparrowAtlasStore(Resources, Host.Renderer));
            GameDependencies.CacheAs(new JSONStore(Resources));
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            GameDependencies = new DependencyContainer(base.CreateChildDependencies(parent));
    }
}
