namespace FunkinSharp.Game.Funkin.Events
{
    public class FocusCameraEvent : ISongEvent
    {
        public int Duration { get; }
        public readonly float X;
        public readonly float Y;
        public readonly int Character;
        public readonly string Ease;

        public FocusCameraEvent(int duration, float x, float y, int character, string ease)
        {
            Duration = duration;
            X = x;
            Y = y;
            Character = character;
            Ease = ease;
        }

    }
}
