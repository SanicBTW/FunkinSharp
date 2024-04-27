using FunkinSharp.Game.Funkin.Sprites;
using NUnit.Framework;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneBFChar : FunkinSharpTestScene
    {
        public TestSceneBFChar()
        {
            Character bf = new Character("bf", true)
            {
                Anchor = osu.Framework.Graphics.Anchor.Centre,
                Origin = osu.Framework.Graphics.Anchor.Centre
            };
            Add(bf);
            foreach (var alias in bf.Aliases)
            {
                AddStep($"Play {alias.Key}", () =>
                {
                    bf.Play(alias.Key);
                });
            }
        }
    }
}
