using osu.Framework.Graphics.Containers;
using osuTK;

namespace FunkinSharp.Game.Funkin.Text
{
    // Unfinished
    public partial class Alphabet : Container<AlphaCharacter>
    {
        public Vector2 DistancePerItem = new(20, 120);
        public Vector2 StartPosition = Vector2.Zero;

        private bool lastWasSpace = false;

        private string text;

        public string Text
        {
            get => text;
            set
            {
                if (text != "" || text == value)
                    return;

                string v = value.Replace("\\n", "\n");

                Clear();
                createLetters(v);

                text = v;
            }
        }

        private bool bold = true;

        public bool Bold
        {
            get => bold;
            set
            {
                if (bold == value)
                    return;

                Clear();
                createLetters(text);

                bold = value;
            }
        }

        public Alphabet(float x, float y, string text = "", bool bold = true)
        {
            StartPosition = Position = new Vector2(x, y);
            Text = text;
            Bold = bold;
        }

        private void createLetters(string newText)
        {
            int consecutiveSpaces = 0;
            foreach (char rawChar in newText.ToCharArray())
            {
                string character = rawChar.ToString();
                bool isSpace = (character == " " || (Bold && character == "_"));
                if (isSpace)
                {
                    consecutiveSpaces++;
                    lastWasSpace = true;
                    continue;
                }

                Vector2 pos = (lastWasSpace) ? new Vector2((DistancePerItem.X * 3) * consecutiveSpaces, 0) : Vector2.Zero;
                AlphaCharacter letter = new AlphaCharacter(pos, character, bold);
                letter.OnLoadComplete += letter_OnLoadComplete;
                Add(letter);

                lastWasSpace = false;
            }
        }

        private void letter_OnLoadComplete(osu.Framework.Graphics.Drawable obj)
        {
            AlphaCharacter letter = (AlphaCharacter)obj;

            if (IndexOf(letter) != 0)
            {
                AlphaCharacter prevChar = this[IndexOf(letter) - 1];
                float horizontalGap = DistancePerItem.X / 2;
                float xPos = (prevChar.X + prevChar.DrawWidth) + horizontalGap;
                letter.X += xPos;
                if (!bold)
                    letter.Y = prevChar.Y;
            }
            else
            {
                letter.X = (StartPosition.X - letter.DrawWidth) / 2;
                if (!bold)
                    letter.Y = (StartPosition.Y / 2);
            }

            letter.X += letter.LetterOffset.X;
            letter.Y += (letter.LetterOffset.Y / 2);
            if (!bold) // sleep deprived sory
                letter.Y += (letter.LetterOffset.Y / 2);
        }
    }
}
