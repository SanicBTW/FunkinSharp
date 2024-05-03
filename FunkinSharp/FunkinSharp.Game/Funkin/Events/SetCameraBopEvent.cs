namespace FunkinSharp.Game.Funkin.Events
{
    public class SetCameraBopEvent : ISongEvent
    {
        public int Duration { get; } = 0;
        public readonly float Rate;
        public readonly float Intensity;

        public SetCameraBopEvent(float rate, float intensity)
        {
            Rate = rate;
            Intensity = intensity;
        }
    }
}
