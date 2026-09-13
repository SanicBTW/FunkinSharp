using FunkinSharp.API.Configuration;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Framework.Logging;
using osuTK;

namespace FunkinSharp.API.Input;

// https://github.com/SanicBTW/LivinOnSweets/blob/qrewrite/LivinOnSweets/LivinOnSweets.API/Input/TouchInputInterceptor.cs

/// <summary>
/// Intercepts all positional input events and sets the appropriate <see cref="SessionSetting.TouchInputActive"/> value
/// for consumption by particular game screens.
/// </summary>
public partial class TouchInputInterceptor : Component
{
    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

    private readonly BindableBool touchInputActive = new(RuntimeInfo.IsMobile);

    [BackgroundDependencyLoader]
    private void load(SessionConfig sesConf)
    {
        sesConf.BindWith(SessionSetting.TouchInputActive, touchInputActive);
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
                        Logger.Log($"Touch input deactivated due to received {e.GetType().ReadableName()}", LoggingTarget.Input);
                    touchInputActive.Value = false;
                }

                break;

            case TouchEvent:
                if (!touchInputWasActive)
                    Logger.Log($"Touch input activated due to received {e.GetType().ReadableName()}", LoggingTarget.Input);
                touchInputActive.Value = true;

                break;
        }

        return false;
    }
}
