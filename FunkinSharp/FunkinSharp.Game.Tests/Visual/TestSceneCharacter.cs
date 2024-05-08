using System.Collections.Generic;
using FunkinSharp.Game.Funkin.Sprites;
using NUnit.Framework;
using osu.Framework.Testing.Drawables.Steps;

namespace FunkinSharp.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneCharacter : FunkinSharpTestScene
    {
        private string characterName = "bf";
        private List<StepButton> anims = [];

        public TestSceneCharacter()
        {
            Character character = new Character(characterName, true);
            character.OnLoadComplete += char_OnLoadComplete;
            Add(character);
            AddStep("Set BF", () =>
            {
                if (characterName == "bf")
                    return;

                characterName = "bf";
                Remove(character, true);
                clean();

                character = new Character(characterName, true);
                character.OnLoadComplete += char_OnLoadComplete;
                Add(character);
            });
            AddStep("Set DAD", () =>
            {
                if (characterName == "dad")
                    return;

                characterName = "dad";
                Remove(character, true);
                clean();

                character = new Character(characterName, true);
                character.Anchor = character.Origin = osu.Framework.Graphics.Anchor.Centre;
                character.OnLoadComplete += char_OnLoadComplete;
                Add(character);
            });
            AddStep("Set GF", () =>
            {
                if (characterName == "gf")
                    return;

                characterName = "gf";
                Remove(character, true);
                clean();

                character = new Character(characterName, true);
                character.OnLoadComplete += char_OnLoadComplete;
                Add(character);
            });
        }

        private void char_OnLoadComplete(osu.Framework.Graphics.Drawable obj)
        {
            Character character = (Character)obj;
            character.X -= character.CFile.Position[0];
            character.Y -= character.CFile.Position[1];
            foreach (var alias in character.Aliases)
            {
                anims.Add(AddStep($"Play {alias.Key}", () =>
                {
                    character.Play(alias.Key);
                }));
            }

            AddToggleStep("Loop Animation", (state) =>
            {
                character.Loop = state;
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
