using FunkinSharp.API.Input;
using FunkinSharp.API.Screens.Navigation;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osuTK.Graphics;

namespace FunkinSharp.Game
{
    public partial class StartupScreen : FunkinScreen, IKeyBindingHandler<FunkinAction>
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Violet,
                    RelativeSizeAxes = Axes.Both,
                },
                new SpriteText
                {
                    Y = 20,
                    Text = "Main Screen",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FontUsage.Default.With(size: 40)
                }
            };
        }

        public bool OnPressed(KeyBindingPressEvent<FunkinAction> e)
        {
            if (e.Action == FunkinAction.Confirm)
            {
                Push(new Startup2Screen());
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<FunkinAction> e)
        {
        }
    }

    public partial class Startup2Screen : FunkinScreen, IKeyBindingHandler<FunkinAction>
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren =
            [
                new Box
                {
                    Colour = Color4.MidnightBlue,
                    RelativeSizeAxes = Axes.Both,
                },
                new SpriteText
                {
                    Y = 20,
                    Text = "Cock Screen",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FontUsage.Default.With(size: 40)
                }
            ];
        }

        public bool OnPressed(KeyBindingPressEvent<FunkinAction> e)
        {
            if (e.Action == FunkinAction.Back)
            {
                Exit();
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<FunkinAction> e)
        {
        }
    }
}
