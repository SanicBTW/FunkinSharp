namespace FunkinSharp.Game.Funkin.Events
{
    public class ZoomCameraEvent : ISongEvent
    {
        public int Duration { get; }
        public readonly string Ease;
        public readonly float Zoom;
        public readonly string Mode;

        public ZoomCameraEvent(int duration, string ease, float zoom, string mode)
        {
            Duration = duration;
            Ease = ease;
            Zoom = zoom;
            Mode = mode;
        }
    }
}
