using FunkinSharp.Game;
using osu.Framework;
using osu.Framework.Platform;

namespace FunkinSharp.Desktop
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableDesktopHost(@"FunkinSharp", new HostOptions()))
            using (osu.Framework.Game game = new FunkinSharpGame())
                host.Run(game);
        }
    }
}
