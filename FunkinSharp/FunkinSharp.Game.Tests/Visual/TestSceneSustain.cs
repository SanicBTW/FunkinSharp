using FunkinSharp.Game.Funkin.Notes;
using NUnit.Framework;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneSustain : FunkinSharpTestScene
    {
        private Note head;
        private Sustain sustain;
        private string noteType = "funkin";
        private int noteData = 0;

        public TestSceneSustain()
        {
            head = new Note(0, noteData, noteType)
            {
                Y = -100,
            };

            sustain = new Sustain(head);
            Add(head);
            Add(sustain);

            AddSliderStep("Sustain Length", 100, 400, 100, (t) =>
            {
                if (sustain == null) return;

                sustain.MaxHeight = t;
                sustain.TargetHeight.Value = t;
            });

            AddToggleStep("Sustain Downscroll", (e) =>
            {
                sustain.Downscroll.Value = e;
            });

            AddSliderStep("Note Data", 0, 3, 0, (v) =>
            {
                if (noteData == v || head == null || sustain == null) return;

                noteData = v;

                Remove(head, true);
                Remove(sustain, true);

                head = new Note(0, noteData, noteType)
                {
                    Y = -100
                };

                Sustain prev = sustain;
                sustain = new Sustain(head)
                {
                    TargetHeight = prev.TargetHeight,
                    MaxHeight = prev.TargetHeight.Value
                };
                prev = null;

                Add(head);
                Add(sustain);
            });
        }
    }
}
