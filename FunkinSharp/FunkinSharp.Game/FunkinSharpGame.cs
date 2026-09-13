using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace FunkinSharp.Game
{
    // https://github.com/SanicBTW/LivinOnSweets/blob/qrewrite/LivinOnSweets/LivinOnSweets.Game/LivinOnSweetsGame.cs
    // i should really, REALLY add scaling containers, but since its just fnf, im not trying to overcomplicate this
    // the content its gonna get rescaled to match the dpi either way so its fineeee, if i end up needing that ill just add it
    public partial class FunkinSharpGame : FunkinSharpGameBase
    {
        private Bindable<bool> applySafeAreaConsiderations;

        private ScreenStack screenStack;

        [BackgroundDependencyLoader]
        private void load()
        {
            // Add your top-level game components here.
            // A screen stack and sample screen has been provided for convenience, but you can replace it if you don't want to use screens.
            Child = screenStack = new ScreenStack { RelativeSizeAxes = Axes.Both };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            screenStack.Push(new StartupScreen());
        }
    }
}
