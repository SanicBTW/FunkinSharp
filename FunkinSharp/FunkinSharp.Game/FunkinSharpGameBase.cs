using System.Collections.Generic;
using System.Drawing;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Game.Funkin;
using FunkinSharp.Game.Funkin.Data.Event;
using FunkinSharp.Resources;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osuTK;

namespace FunkinSharp.Game
{
    // This is shared across the Testing Browser and Standalone Game
    public partial class FunkinSharpGameBase : osu.Framework.Game
    {
        public DependencyContainer GameDependencies { get; protected set; }
        public FunkinConfig FunkinConfig { get; protected set; }
        public FunkinKeybinds FunkinKeybinds { get; protected set; }
        private List<string> fonts => ["Fonts/RedHatDisplay/RedHatDisplay-Regular", "Fonts/RedHatDisplay/RedHatDisplay-Bold"];

        public virtual ScreenStack ScreenStack { get; protected set; }

        // yeah we using the amazing basic camera to give the game black bars hehe
        protected override Camera Content { get; }

        protected FunkinSharpGameBase()
        {
            base.Content.Add(Content = []);
        }

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config)
        {
            Paths.Initialize(Host, Audio, Resources);
            SongEventRegistry.LoadEventCache();
            Resources.AddStore(new DllResourceStore(typeof(FunkinSharpResources).Assembly));
            setupDependencies();
            loadFonts();

            Paths.Initialize(Host, Audio, Resources);
            SongEventRegistry.LoadEventCache();

            // We listen to resizes to properly set the camera size
            ResizeCamera();
            Window.Resized += ResizeCamera;

            // Force the window size to our desired window size
            config.SetValue(FrameworkSetting.WindowedSize, new Size(GameConstants.WIDTH, GameConstants.HEIGHT));

            // Force the window position to the center on startup
            config.SetValue(FrameworkSetting.WindowedPositionX, 0.5);
            config.SetValue(FrameworkSetting.WindowedPositionY, 0.5);

            if (ScreenStack == null)
                return;

            ScreenStack.ScreenPushed += ScreenPushed;
            ScreenStack.ScreenExited += ScreenExited;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            GameDependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        private void setupDependencies()
        {
            GameDependencies.CacheAs(this);
            GameDependencies.CacheAs(Dependencies);
            GameDependencies.CacheAs(FunkinConfig = new FunkinConfig(Host.Storage));
            GameDependencies.CacheAs(FunkinKeybinds = new FunkinKeybinds(Host.Storage));
            GameDependencies.CacheAs(new SparrowAtlasStore(Resources, Host.Renderer));
            GameDependencies.CacheAs(new JSONStore(Resources));
        }

        private void loadFonts()
        {
            foreach (string font in fonts)
            {
                AddFont(Resources, font);
            }
        }

        protected virtual void ScreenPushed(IScreen lastScreen, IScreen newScreen)
        {
            ScreenChanged(lastScreen, newScreen, false);
        }

        protected virtual void ScreenExited(IScreen lastScreen, IScreen newScreen)
        {
            ScreenChanged(lastScreen, newScreen, true);

            if (newScreen == null)
                RequestExit();
        }

        // When overriding this method, there's no need to call the base method (this)
        public virtual void ScreenChanged(IScreen lastScreen, IScreen newScreen, bool isExit)
        {
            string pref = isExit ? "exited" : "pushed";
            Logger.Log($"Screen {lastScreen} {pref} to {newScreen}");
        }

        // This makes the game container properly resize to match da classic haxeflixel 1280x720 black bars feel hehe
        // It only makes the needed calculations and it sets the ratio to the camera size (since the camera size is relative to the full size, setting a number gets multiplied by the axis, yknow what i mean)

        // TODO: Fix crash when trying to resize the window when a child (inside of the camera clip container) is selected in the draw visualizer
        public void ResizeCamera()
        {
            float wWidth = Window.ClientSize.Width;
            float wHeight = Window.ClientSize.Height;

            float ratioX = wWidth / GameConstants.WIDTH;
            float ratioY = wHeight / GameConstants.HEIGHT;
            float zoom = float.Min(ratioX, ratioY);

            Content.Zoom = zoom;
            Content.Size = new Vector2(1 / ratioX, 1 / ratioY);
        }
    }
}
