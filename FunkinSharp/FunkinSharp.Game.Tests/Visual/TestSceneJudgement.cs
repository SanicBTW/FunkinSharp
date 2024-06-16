using FunkinSharp.Game.Funkin.Sprites;
using NUnit.Framework;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneJudgement : FunkinSharpTestScene
    {
        private JudgementDisplay judgementDisplay;

        public TestSceneJudgement()
        {
            Add(judgementDisplay = new());

            string[] judgements = ["sick", "good", "bad", "shit"];
            foreach (string judgement in judgements)
            {
                AddStep($"Play {judgement}", () =>
                {
                    judgementDisplay.Play(judgement);
                });
            }
        }
    }
}
