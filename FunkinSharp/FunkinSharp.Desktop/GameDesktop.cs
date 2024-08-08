using FunkinSharp.Game;
using FunkinSharp.Game.Core;

namespace FunkinSharp.Desktop
{
    public partial class GameDesktop : FunkinSharpGame
    {
        protected override void LoadComplete()
        {
            GameDependencies.CacheAs(typeof(IStorageService), new DesktopStorageService(Host));

            base.LoadComplete();
        }
    }
}
