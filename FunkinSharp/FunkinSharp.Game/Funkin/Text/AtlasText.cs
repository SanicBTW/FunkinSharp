using System;
using System.Collections.Generic;
using FunkinSharp.Game.Core.Sparrow;
using osu.Framework.Graphics.Containers;

namespace FunkinSharp.Game.Funkin.Text
{
    // TODO: Pooling
    public partial class AtlasText : Container<AtlasFontChar>
    {
        private static Dictionary<AtlasFontType, AtlasFontData> fonts = [];

        private string text = "";

        public string Text
        {
            get => text;
            set
            {
                value ??= "";

                string caseValue = restrictCase(value);
                string caseText = restrictCase(text);

                text = value;
                if (caseText == caseValue) return; // Cancel redraw

                if (caseValue.IndexOf(caseText) == 0)
                {
                    // New text is just old text with additions at the end, only append the difference
                    appendTextCased(caseValue[caseText.Length..]);
                    return;
                }

                value = caseValue;
                Clear();
                if (value == "") return;
                appendTextCased(caseValue);
            }
        }

        private AtlasFontData font;

        public SparrowAtlas Atlas => font.Atlas;
        public FontCase CaseAllowed => font.CaseAllowed;
        public float MaxHeight => font.MaxHeight;

        public AtlasText(string text, float x = 0f, float y = 0f, AtlasFontType fontName = AtlasFontType.DEFAULT)
        {
            if (!fonts.ContainsKey(fontName)) fonts[fontName] = new AtlasFontData(fontName);
            font = fonts[fontName];

            Position = new osuTK.Vector2(x, y);
            Text = text;
        }

        public void AppendText(string text)
        {
            if (text == null) throw new ArgumentNullException();
            if (text == "") return;

            Text = this.text + text;
        }

        /// <summary>
        ///     Converts all characters to fit the font's <see cref="AtlasFontData.CaseAllowed"/>.
        /// </summary>
        /// <param name="text">The text to be converted</param>
        /// <returns>The converted text to be used</returns>
        private string restrictCase(string text)
        {
            string ret = text;

            switch (CaseAllowed)
            {
                case FontCase.BOTH: ret = text; break;
                case FontCase.UPPER: ret = text.ToUpper(); break;
                case FontCase.LOWER: ret = text.ToLower(); break;
            }

            return ret;
        }

        /// <summary>
        ///     Adds new text on top of the existing text. Helper for other methods; Doesn't change <see cref="Text"/>.
        /// </summary>
        /// <param name="text">The text to add assumed to match the font's <see cref="AtlasFontData.CaseAllowed"/></param>
        private void appendTextCased(string text)
        {
            int charCount = AliveChildren.Count;
            float xPos = 0f;
            float yPos = 0f;

            // Evaluate char count behaviour
            if (charCount == 0 || charCount == -1)
                charCount = 0;
            else if (charCount > 0)
            {
                AtlasFontChar lastChar = this[charCount - 1];
                xPos = lastChar.X + lastChar.Width - X;
                yPos = lastChar.Y + lastChar.Height - MaxHeight - Y;
            }

            foreach (char rawChar in text.ToCharArray())
            {
                string character = rawChar.ToString();
                switch (character)
                {
                    case " ":
                        xPos += 40;
                        break;

                    case "\n":
                        xPos = 0;
                        yPos += MaxHeight;
                        break;

                    default:
                        // Pooling here soon
                        // NOTE: When loading a new character, the sizes are not available until the sprite has fully loaded,
                        // a way to fix this would be to add an event listener and increase the positions according to the real shi but whatver it works decently now
                        AtlasFontChar charSpr = new AtlasFontChar(Atlas, character, xPos);
                        charSpr.Y = yPos + MaxHeight - charSpr.CurrentFrame.DisplayHeight;
                        Add(charSpr);

                        xPos += charSpr.CurrentFrame.DisplayWidth;
                        charCount++;
                        break;
                }
            }
        }
    }
}
