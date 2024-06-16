using FunkinSharp.Game.Funkin.Sprites;
using NUnit.Framework;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneCombo : FunkinSharpTestScene
    {
        private ComboCounter comboCounter;
        public TestSceneCombo()
        {
            Add(comboCounter = []);

            AddSliderStep("Current Number", 0, 1000, 0, (ev) =>
            {
                comboCounter.Current.Value = ev;
            });

            AddStep("Add Number to the Counter", () =>
            {
                comboCounter.AddNumber();
            });
        }
    }
}
