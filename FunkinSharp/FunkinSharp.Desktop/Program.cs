using FunkinSharp.Game.Core;
using osu.Framework;
using osu.Framework.Platform;

namespace FunkinSharp.Desktop
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableDesktopHost(GameConstants.TITLE, new HostOptions() { FriendlyGameName = GameConstants.TITLE }))
            using (osu.Framework.Game game = new GameDesktop())
                host.Run(game);
        }
    }
}
