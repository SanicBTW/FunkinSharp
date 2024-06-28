using System;
using System.Text.RegularExpressions;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Sparrow;
using FunkinSharp.Game.Core.Utils;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;

namespace FunkinSharp.Game.Funkin.Text
{
    public partial class AtlasFontData
    {
        public static Regex UpperChar => UpperCharRegex();
        public static Regex LowerChar => LowerCharRegex();
        public static Regex DigitsOnly => DigitsOnlyRegex();

        public SparrowAtlas Atlas;
        public float MaxHeight = 0.0f;
        public FontCase CaseAllowed = FontCase.BOTH;

        public AtlasFontData(AtlasFontType name)
        {
            string fontName = EnumExtensions.GetString(name);

            Atlas = Paths.GetSparrowLegacy($"Textures/Fonts/{fontName.ToLower()}");
            if (Atlas == null)
            {
                Logger.Log($"Couldn't find font atlas for font \"{fontName}\".");
                return;
            }

            bool containsUpper = false;
            bool containsLower = false;

            for (int i = 0; i < Atlas.Frames.Count; i++)
            {
                Texture frame = Atlas.Frames[i];
                string frameName = Atlas.FrameNames[i];

                MaxHeight = Math.Max(MaxHeight, frame.DisplayHeight);

                if (!containsUpper) containsUpper = UpperChar.IsMatch(frameName);
                if (!containsLower) containsLower = LowerChar.IsMatch(frameName);
            }

            if (containsUpper != containsLower) CaseAllowed = containsUpper ? FontCase.UPPER : FontCase.LOWER;
        }

        [GeneratedRegex(@"^[A-Z]\d+$")]
        private static partial Regex UpperCharRegex();
        [GeneratedRegex(@"^[a-z]\d+$")]
        private static partial Regex LowerCharRegex();
        [GeneratedRegex(@"^\d+(\s\d+)*$")]
        private static partial Regex DigitsOnlyRegex();
    }
}
