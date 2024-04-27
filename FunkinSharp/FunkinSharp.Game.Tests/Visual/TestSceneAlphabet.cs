using FunkinSharp.Game.Funkin.Text;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneAlphabet : FunkinSharpTestScene
    {
        private Container<AlphaCharacter> alphabet;

        public TestSceneAlphabet()
        {
            alphabet = [];
            string str = "Hello World!";
            bool lastSpace = false;

            foreach (char character in str.ToCharArray())
            {
                if (character.ToString() == " ")
                {
                    lastSpace = true;
                    continue;
                }

                Vector2 pos = (lastSpace) ? new Vector2(40, 0) : Vector2.Zero;
                AlphaCharacter letter = new AlphaCharacter(pos, character.ToString(), true);
                letter.OnLoadComplete += letter_OnLoadComplete;
                letter.Origin = osu.Framework.Graphics.Anchor.TopLeft;
                letter.Anchor = osu.Framework.Graphics.Anchor.TopLeft;
                alphabet.Add(letter);
                lastSpace = false;
            }
            Add(alphabet);
        }

        private void letter_OnLoadComplete(osu.Framework.Graphics.Drawable obj)
        {
            AlphaCharacter character = (AlphaCharacter)obj;
            if (alphabet.IndexOf(character) != 0)
            {
                AlphaCharacter prevChar = alphabet[alphabet.IndexOf(character) - 1];
                character.X += prevChar.X + prevChar.DrawWidth;
            }
        }
    }
}
