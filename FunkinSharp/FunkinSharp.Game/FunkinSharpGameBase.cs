using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Resources;
using osu.Framework.Allocation;
using osu.Framework.IO.Stores;
using osuTK;

namespace FunkinSharp.Game
{
    public partial class FunkinSharpGameBase : osu.Framework.Game
    {
        public DependencyContainer GameDependencies { get; protected set; }

        // yeah we using the amazing basic camera to give the game black bars hehe
        protected override Camera Content { get; }

        private float gameWidth = 1280f;
        private float gameHeight = 720f;

        protected FunkinSharpGameBase()
        {
            base.Content.Add(Content = new());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // We listen to resizes to properly set the camera size
            ResizeCamera();
            Window.Resized += ResizeCamera;

            Resources.AddStore(new DllResourceStore(typeof(FunkinSharpResources).Assembly));
            GameDependencies.CacheAs(this);
            GameDependencies.CacheAs(Dependencies);
            GameDependencies.CacheAs(new SparrowAtlasStore(Resources, Host.Renderer));
            GameDependencies.CacheAs(new JSONStore(Resources));
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            GameDependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        // This makes the game container properly resize to match da classic haxeflixel 1280x720 black bars feel hehe
        // It only makes the needed calculations and it sets the ratio to the camera size (since the camera size is relative to the full size, setting a number gets multiplied by the axis, yknow what i mean)

        // TODO: Fix crash when trying to resize the window when a child (inside of the camera clip container) is selected in the draw visualizer
        public void ResizeCamera()
        {
            float wWidth = Window.ClientSize.Width;
            float wHeight = Window.ClientSize.Height;

            float ratioX = wWidth / gameWidth;
            float ratioY = wHeight / gameHeight;
            float zoom = float.Min(ratioX, ratioY);

            Content.Zoom = zoom;
            Content.Size = new Vector2(1 / ratioX, 1 / ratioY);
        }
    }
}
