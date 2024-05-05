using FunkinSharp.Game.Funkin.Notes;
using NUnit.Framework;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneScrollNote : FunkinSharpTestScene
    {
        private Note note;
        private string noteType = "funkin";
        private int noteData = 0;

        public TestSceneScrollNote()
        {
            AddSliderStep("Note Data", 0, 3, 0, (v) =>
            {
                if (noteData == v || note == null) return;

                noteData = v;
                Remove(note, true);

                note = new Note(0, noteData, noteType)
                {
                    Y = 0
                };
                Add(note);
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            note = new Note(0, noteData, noteType)
            {
                Y = 0
            };
            Add(note);
        }
    }
}
