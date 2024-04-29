using FunkinSharp.Game.Core.Cursor;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Screens;

namespace FunkinSharp.Game
{
    public partial class FunkinSharpGame : FunkinSharpGameBase
    {
        private ScreenStack screenStack = new() { RelativeSizeAxes = Axes.Both };
        public override ScreenStack ScreenStack => screenStack;

        public BasicCursorContainer Cursor { get; protected set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            // This makes the OS Cursor hide and leaves the Game Cursor be the pointer in game
            Window.CursorState |= CursorState.Hidden;
            Children = [screenStack, Cursor = new()];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScreenStack.Push(new MainScreen());
        }
    }
}
