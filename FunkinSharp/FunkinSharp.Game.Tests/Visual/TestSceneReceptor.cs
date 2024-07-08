using System.Collections.Generic;
using FunkinSharp.Game.Funkin.Notes;
using NUnit.Framework;
using osu.Framework.Testing.Drawables.Steps;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneReceptor : FunkinSharpTestScene
    {
        private List<StepButton> anims = [];
        private string lastAnim = "static";

        private Receptor receptor;
        private string noteType = "funkin";
        private int noteData = 0;

        public TestSceneReceptor()
        {
            AddSliderStep("Receptor Note Data", 0, 3, 0, (v) =>
            {
                if (noteData == v || receptor == null) return;

                noteData = v;
                Remove(receptor, true);
                clean();

                receptor = new Receptor(noteData, noteType: noteType);
                receptor.OnLoadComplete += receptor_OnLoadComplete;
                Add(receptor);
            });

            receptor = new Receptor(noteData, noteType: noteType);
            receptor.OnLoadComplete += receptor_OnLoadComplete;
            Add(receptor);
        }

        private void receptor_OnLoadComplete(osu.Framework.Graphics.Drawable obj)
        {
            foreach (var alias in receptor.Aliases)
            {
                StepButton step = AddStep($"Play {alias.Key}", () =>
                {
                    receptor.Play(alias.Key);
                    lastAnim = alias.Key;
                });

                if (lastAnim == alias.Key)
                {
                    step.TriggerClick();
                }

                anims.Add(step);
            }

            AddToggleStep("Loop Animation", (state) =>
            {
                receptor.CurAnim.Loop = state;
            });
        }

        private void clean()
        {
            foreach (StepButton step in anims)
            {
                StepsContainer.Remove(step, true);
            }

            StepsContainer.Remove(StepsContainer[^1], true);
            anims = [];
        }
    }
}
