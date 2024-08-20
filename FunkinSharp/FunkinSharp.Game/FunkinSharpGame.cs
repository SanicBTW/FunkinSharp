using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Cursor;
using FunkinSharp.Game.Funkin.Data.Event;
using FunkinSharp.Game.Funkin.Data;
using FunkinSharp.Game.Funkin;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Screens;

namespace FunkinSharp.Game
{
    public partial class FunkinSharpGame : FunkinSharpGameBase
    {
        public BasicCursorContainer Cursor { get; protected set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            // This makes the OS Cursor hide and leaves the Game Cursor be the pointer in game
            Window.CursorState |= CursorState.Hidden;
            Children = [ScreenStack, Cursor = [], PerfOverlay = [], Volume = []];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // when overriding game to setup custom dependencies, always call base.LoadComplete AFTER adding the custom dependencies,
            // so that if any class or instance that needs it wont throw a null reference exception!!

            SongEventRegistry.LoadEventCache();
            ChartRegistry.Initialize(GameDependencies.Get<IStorageService>());
            NoteSkinRegistry.Initialize(GameDependencies.Get<IStorageService>());

            ScreenStack.Push(new MainScreen());
        }
    }
}
