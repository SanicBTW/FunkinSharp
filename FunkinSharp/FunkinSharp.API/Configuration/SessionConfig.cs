using osu.Framework;
using osu.Framework.Configuration;

namespace FunkinSharp.API.Configuration;

// https://github.com/SanicBTW/LivinOnSweets/blob/qrewrite/LivinOnSweets/LivinOnSweets.API/Configuration/SessionConfig.cs
public class SessionConfig : ConfigManager<SessionSetting>
{
    public SessionConfig()
    {
        InitialiseDefaults();
    }

    protected override void InitialiseDefaults()
    {
        SetDefault(SessionSetting.TouchInputActive, RuntimeInfo.IsMobile);
        SetDefault(SessionSetting.ShowingScreenshot, false);
        SetDefault(SessionSetting.ScreenshotCursorVisibility, true);
    }

    protected override void PerformLoad() { }

    protected override bool PerformSave() => true;
}
