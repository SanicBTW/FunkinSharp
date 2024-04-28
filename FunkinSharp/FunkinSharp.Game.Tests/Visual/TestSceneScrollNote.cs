using System.Collections.Generic;
using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Game.Funkin.Compat;
using FunkinSharp.Game.Funkin.Notes;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing.Drawables.Steps;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneScrollNote : FunkinSharpTestScene
    {
        private Note note;
        private string noteType = "default";
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
