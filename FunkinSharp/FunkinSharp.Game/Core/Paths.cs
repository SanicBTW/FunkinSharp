using System.IO;

namespace FunkinSharp.Game.Core
{
    public static class Paths
    {
        // replaces '\' and the directory separator character with '/'
        public static string SanitizeForResources(string path) =>
            path.Replace('\\', '/').Replace(Path.DirectorySeparatorChar, '/');
    }
}
