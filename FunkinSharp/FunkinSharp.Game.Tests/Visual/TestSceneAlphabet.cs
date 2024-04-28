using FunkinSharp.Game.Funkin.Text;
using NUnit.Framework;

namespace FunkinSharp.Game.Tests.Visual
{
    // Unfinished
    [TestFixture]
    public partial class TestSceneAlphabet : FunkinSharpTestScene
    {
        private Alphabet text;
        public TestSceneAlphabet()
        {
            text = new Alphabet(50, 50, "", false);
            Add(text);
            AddStep("Text 1", () =>
            {
                text.Text = "Hello World!";
            });
            AddStep("Text 2", () =>
            {
                text.Text = "Hello osu!Framework";
            });
            AddToggleStep("Bold", (state) =>
            {
                text.Bold = state;
            });
        }
    }
}
