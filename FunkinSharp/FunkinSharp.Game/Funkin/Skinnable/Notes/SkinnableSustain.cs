using FunkinSharp.Game.Funkin.Data;
using FunkinSharp.Game.Funkin.Notes;
using osu.Framework.Bindables;

namespace FunkinSharp.Game.Funkin.Skinnable.Notes
{
    public partial class SkinnableSustain : Sustain
    {
        public new readonly SkinnableNote Head;

        protected override SustainSprite GetSustainBody() => new SkinnableSustainSprite(Head, UseLegacySpritesheet);
        protected override SustainEnd GetSustainEnd() => new SkinnableSustainEnd(Head, UseLegacySpritesheet);

        public SkinnableSustain(SkinnableNote head) : base(head)
        {
            // because we override the Head variable in this class we need to re assign it since calling the base function does nothing
            Head = head;
            UseLegacySpritesheet = new BindableBool(!NoteSkinRegistry.SupportsSustainSheet(head.Skin));
        }
    }
}
