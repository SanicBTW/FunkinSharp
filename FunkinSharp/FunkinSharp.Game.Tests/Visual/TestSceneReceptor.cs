using System.Collections.Generic;
using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Game.Funkin.Compat;
using FunkinSharp.Game.Funkin.Notes;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing.Drawables.Steps;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneReceptor : FunkinSharpTestScene
    {
        private List<StepButton> anims = [];
        private FEReceptorData data;
        private string lastAnim = "static";

        private Receptor receptor;
        private string noteType = "default";
        private int noteData = 0;

        public TestSceneReceptor()
        {
            AddSliderStep("Receptor Note Data", 0, 3, 0, (v) =>
            {
                if (noteData == v || receptor == null) return;

                noteData = v;
                Remove(receptor, true);
                clean();

                receptor = new Receptor(data, noteData, noteType);
                receptor.OnLoadComplete += receptor_OnLoadComplete;
                Add(receptor);
            });
        }

        [BackgroundDependencyLoader]
        private void load(JSONStore jsonStore)
        {
            data = jsonStore.Get<FEReceptorData>($"NoteTypes/{noteType}/{noteType}");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            receptor = new Receptor(data, noteData, noteType);
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
                receptor.Loop = state;
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
