using FunkinSharp.API.Screens.Navigation;
using FunkinSharp.API.Screens.Presentation;
using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace FunkinSharp.Game
{
    // https://github.com/SanicBTW/LivinOnSweets/blob/qrewrite/LivinOnSweets/LivinOnSweets.Game/LivinOnSweetsGame.cs
    // i should really, REALLY add scaling containers, but since its just fnf, im not trying to overcomplicate this
    // the content its gonna get rescaled to match the dpi either way so its fineeee, if i end up needing that ill just add it
    public partial class FunkinSharpGame : FunkinSharpGameBase
    {
        // private Bindable<bool> applySafeAreaConsiderations;

        private FunkinPresentation presentation;

        [BackgroundDependencyLoader]
        private void load()
        {
            GameDependencies.Cache(this);

            // should apply safe area considerations here
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(presentation = new FunkinPresentation() { Anchor = Anchor.Centre, Origin = Anchor.Centre, RelativeSizeAxes = Axes.Both });
            presentation.ScreenStack.Push(new StartupScreen());

            // overlay stuff
        }
    }
}
