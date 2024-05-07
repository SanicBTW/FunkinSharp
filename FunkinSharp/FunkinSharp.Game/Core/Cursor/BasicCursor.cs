using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osuTK;

namespace FunkinSharp.Game.Core.Cursor
{
    public partial class BasicCursor : CursorContainer
    {
        public new Cursor ActiveCursor;
        protected override Drawable CreateCursor() => ActiveCursor = new Cursor();

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (ActiveCursor is null || ActiveCursor is not null && !ActiveCursor.IsPresent)
                return base.OnMouseDown(e);

            ActiveCursor.Scale = new Vector2(1f);
            ActiveCursor.ScaleTo(0.9f, 800D, Easing.OutQuint);
            ActiveCursor.FadeTo(0.8f, 800D, Easing.OutQuint);
            ActiveCursor.SetState(true);

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (!e.HasAnyButtonPressed && ActiveCursor is not null && ActiveCursor.IsPresent)
            {
                ActiveCursor.FadeTo(1f, 500D, Easing.OutQuint);
                ActiveCursor.ScaleTo(1f, 500D, Easing.OutElastic);
                ActiveCursor.SetState(false);
            }

            base.OnMouseUp(e);
        }

        // Now they on schedule since it might crash because of "Cannot apply transforms if not in the draw thread" which is fair
        protected override void PopIn()
        {
            if (ActiveCursor is null)
            {
                base.PopIn();
                return;
            }

            // this is scheduled to run after children since popout runs before this, so to properly run transforms we have to schedule them transforms
            ScheduleAfterChildren(() =>
            {
                ActiveCursor.FadeTo(1f, 250D, Easing.OutQuint);
                ActiveCursor.ScaleTo(1f, 400D, Easing.OutQuint);
            });
        }

        protected override void PopOut()
        {
            if (ActiveCursor is null)
            {
                base.PopOut();
                return;
            }

            Schedule(() =>
            {
                ActiveCursor.FadeTo(0f, 250D, Easing.OutQuint);
                ActiveCursor.ScaleTo(0.6f, 250D, Easing.OutQuint);
            });
        }
    }
}
