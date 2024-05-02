using System;
using System.Reflection;

namespace FunkinSharp.Game.Core
{
    // omg jus like 0.3 FNF :surprised:
    public static class GameConstants
    {
        public static string TITLE => Assembly.GetExecutingAssembly().GetName().Name.Replace(".Game", ""); // lmao
        public static Version VERSION => Assembly.GetExecutingAssembly().GetName().Version;
        public static string VER_PREFIX => " ALPHA";
    }
}
