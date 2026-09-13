using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace FunkinSharp.API.Screens.Navigation;

// https://github.com/SanicBTW/LivinOnSweets/blob/qrewrite/LivinOnSweets/LivinOnSweets.API/Screens/SweetScreenStack.cs
// i def should improve this...
public partial class FunkinScreenStack : ScreenStack
{
    private readonly FunkinScreenStack? subStack;
    public bool IsSubStack { get; protected set; }
    public bool IsSubScreenOpen => !IsSubStack && subStack!.CurrentScreen != null;

    public new MarginPadding Padding
    {
        get => base.Padding;
        set => base.Padding = value;
    }

    public FunkinScreenStack(bool isSubStack = false) : base(false)
    {
        IsSubStack = isSubStack;
        if (!isSubStack)
            InternalChild = subStack = new FunkinScreenStack(true) { Depth = -1, RelativeSizeAxes = Axes.Both };
    }

    public void PushSynchronously(FunkinScreen screen)
    {
        LoadComponent(screen);
        Push(screen);
    }

    public void PushSubScreen(FunkinSubScreen subScreen)
    {
        if (IsSubStack || IsSubScreenOpen)
            return;

        subStack!.Push(subScreen);
    }

    public void ExitSubScreen()
    {
        // If its a sub stack do not call sub stack exit since the sub stack is null
        if (IsSubStack || !IsSubScreenOpen)
            return;

        subStack!.Exit();
    }
}
