using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Framework.Logging;
using osuTK;
using osuTK.Input;

namespace FunkinSharp.Game.Core.Input
{
    // https://github.com/ppy/osu/blob/master/osu.Game/Input/TouchInputInterceptor.cs
    // yes im gonna copy the code 1:1 because im too fucking lazy
    public partial class TouchInputInterceptor : Component
    {
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        private readonly BindableBool touchInputActive = new BindableBool();

        [BackgroundDependencyLoader]
        private void load()
        {
            touchInputActive.Value = RuntimeInfo.IsMobile;
        }

        protected override bool Handle(UIEvent e)
        {
            bool touchInputWasActive = touchInputActive.Value;

            switch (e)
            {
                case MouseEvent:
                    if (e.CurrentState.Mouse.LastSource is not ISourcedFromTouch)
                    {
                        if (touchInputWasActive)
                            Logger.Log($@"Touch input deactivated due to received {e.GetType().ReadableName()}", LoggingTarget.Input);
                        touchInputActive.Value = false;
                    }

                    break;

                case TouchEvent:
                    if (!touchInputWasActive)
                        Logger.Log($@"Touch input activated due to received {e.GetType().ReadableName()}", LoggingTarget.Input);
                    touchInputActive.Value = true;
                    break;

                case KeyDownEvent keyDown:
                    if (keyDown.Key == Key.T && keyDown.ControlPressed && keyDown.ShiftPressed)
                        debugToggleTouchInputActive();
                    break;
            }

            return false;
        }

        private void debugToggleTouchInputActive()
        {
            Logger.Log($@"Debug-toggling touch input to {(touchInputActive.Value ? @"inactive" : @"active")}", LoggingTarget.Information, LogLevel.Important);
            touchInputActive.Toggle();
        }
    }
}
