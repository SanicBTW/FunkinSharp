using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace FunkinSharp.Game.Core.Cursor
{
    public partial class Cursor : Container
    {
        public const float DEFAULT_SCALE = 0.75f;

        public Bindable<float> CursorScale;
        public AnimatedCursor CursorDrawable;
        public bool MouseState;

        public Cursor()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            CursorDrawable = new();
            Children = new Drawable[]
            {
                CursorDrawable
            };

            CursorScale = new BindableFloat(DEFAULT_SCALE);
            CursorScale.BindValueChanged(x => CursorDrawable.Scale = new Vector2(x.NewValue * DEFAULT_SCALE), true);
        }

        protected override void Update()
        {
            base.Update();

            if (CursorDrawable is null)
                return;

            if (MouseState)
                CursorDrawable.Play("arrow click");
            else
                CursorDrawable.Play("arrow jiggle", false);
        }

        public virtual void SetState(bool heldDown)
        {
            MouseState = heldDown;
            if (CursorDrawable is null)
                return;

            if (!MouseState)
                CursorDrawable.Play("arrow jiggle");
        }
    }
}
