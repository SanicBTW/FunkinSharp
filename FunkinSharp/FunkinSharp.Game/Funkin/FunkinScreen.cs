using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;

namespace FunkinSharp.Game.Funkin
{
    // This g is full of questionable choices but so far works properly
    // TODO: Add required methods for manipulating the Container (Add, Remove, etc)
    public partial class FunkinScreen : Screen
    {
        private Container content;
        protected virtual Container Content => GenerateContainer();

        public virtual bool CursorVisible
        {
            get
            {
                FunkinSharpGame game = (FunkinSharpGame)Game;
                return game.Cursor.Cursor.State.Value == Visibility.Visible;
            }

            set
            {
                FunkinSharpGame game = (FunkinSharpGame)Game;
                game.Cursor.Cursor.State.Value = (value) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public virtual Container GenerateContainer()
        {
            // There's already an existing container for the screen!
            if (content != null)
                return content;

            content = new()
            {
                Name = "FunkinScreenContent",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
            };
            InternalChild = content;
            return content;
        }

        public virtual void Add(Drawable drawable)
        {
            Content.Add(drawable);
        }
    }
}
