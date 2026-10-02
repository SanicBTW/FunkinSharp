using System.Collections.Generic;
using System.IO;
using System.Linq;
using FunkinSharp.API.Animation;
using FunkinSharp.API.Input;
using FunkinSharp.API.Screens.Navigation;
using FunkinSharp.API.Sparrow;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.IO.Stores;
using osuTK.Graphics;

namespace FunkinSharp.Game
{
    public partial class StartupScreen : FunkinScreen, IKeyBindingHandler<FunkinAction>
    {
        private Container<Character> sparrows;

        [BackgroundDependencyLoader]
        private void load()
        {
            var gf = new Character("gf") { X = 400, Y = 130, Origin = Anchor.Centre };
            var dad = new Character("dad") { X = 100, Y = 100, Origin = Anchor.Centre };
            var bf = new Character("bf", true) { X = 770, Y = 100, Origin = Anchor.Centre };

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
                },
                sparrows = new Container<Character>()
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = [gf, dad, bf]
                }
            };
        }

        public bool OnPressed(KeyBindingPressEvent<FunkinAction> e)
        {
            switch (e.Action)
            {
                /*
                case FunkinAction.Back:
                    animation.Play(fix[0]);
                    return true;

                case FunkinAction.NoteLeft:
                case FunkinAction.NoteDown:
                case FunkinAction.NoteUp:
                case FunkinAction.NoteRight:
                    var idx = (int)e.Action;
                    var anim = fix[idx + 1];
                    animation.Play(anim);
                    return true;*/

                case FunkinAction.Confirm:
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
