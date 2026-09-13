using osu.Framework.Screens;
using osu.Framework.Graphics;

namespace FunkinSharp.API.Screens.Navigation;

// https://github.com/SanicBTW/LivinOnSweets/blob/qrewrite/LivinOnSweets/LivinOnSweets.API/Screens/SweetScreen.cs
public partial class FunkinScreen : Screen
{
    protected FunkinScreenStack ScreenStack => (FunkinScreenStack)Parent!;

    protected bool IsSubScreenOpen => ScreenStack.IsSubScreenOpen;

    protected FunkinScreen()
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Anchor = Origin = Anchor.Centre;
    }

    public override bool OnExiting(ScreenExitEvent e)
    {
        if (ScreenStack is not { IsSubScreenOpen: true }) return base.OnExiting(e);

        ScreenStack.ExitSubScreen();
        return true;
    }

    protected void Exit() => ScreenStack.Exit();

    protected void Push(FunkinScreen screen) => ScreenStack.Push(screen);

    protected void PushSynchronously(FunkinScreen screen) => ScreenStack.PushSynchronously(screen);

    protected void PushSubScreen(FunkinSubScreen subScreen) => ScreenStack.PushSubScreen(subScreen);

    protected void ExitSubScreen() => ScreenStack.ExitSubScreen();
}
