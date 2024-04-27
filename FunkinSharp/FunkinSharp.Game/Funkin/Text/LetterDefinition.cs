using osuTK;

namespace FunkinSharp.Game.Funkin.Text
{
    // Psych stuff lmao
    public readonly struct LetterDefinition
    {
        public readonly string Animation;
        public readonly Vector2 Offset;
        public readonly Vector2 BoldOffset;
        public readonly bool HasBold;

        public LetterDefinition(string animation, Vector2 offset, Vector2 boldOffset, bool hasBold = true)
        {
            Animation = animation;
            Offset = offset;
            BoldOffset = boldOffset;
            HasBold = hasBold;
        }
    }
}
