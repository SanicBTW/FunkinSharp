using System;
using System.Text.RegularExpressions;

namespace FunkinSharp.Game.Core.Utils
{
    // Just like VersionUtil from 0.3 Prototype FNF but without the thx semver lib lmao
    public static class VersionUtil
    {
        // The rule must have a .x at the end
        public static bool ValidateVersion(Version version, string rule)
        {
            string verStr = version.ToString();
            string semRule = $"^{Regex.Escape(rule).Replace("\\.x", "\\.\\d+")}$"; // After a couple of gens, ChatGPT finally gave me the good code bru
            Regex regex = new Regex(semRule);
            return regex.IsMatch(verStr);
        }
    }
}
