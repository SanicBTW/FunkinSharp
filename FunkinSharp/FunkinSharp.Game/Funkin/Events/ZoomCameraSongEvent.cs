using FunkinSharp.Game.Core;
using FunkinSharp.Game.Funkin.Song;

namespace FunkinSharp.Game.Funkin.Events
{
    public class ZoomCameraSongEvent : SongEvent
    {
        public float Zoom { get; private set; }
        public float Duration { get; private set; }
        public string Mode { get; private set; }
        public string Ease { get; private set; }
        public bool IsDirectMode { get; private set; }

        public ZoomCameraSongEvent() : base("ZoomCamera") { }

        public override void HandleEvent(SongEventData eventData)
        {
            Zoom = eventData.GetFloat("zoom") ?? 1.0f;

            float duration = eventData.GetFloat("duration") ?? 4.0f;
            Duration = (float)Conductor.StepCrochet * duration / 1000;

            Mode = eventData.GetString("mode") ?? "direct";
            IsDirectMode = Mode == "direct";

            Ease = eventData.GetString("ease") ?? "CLASSIC";
        }

        public override string GetTitle() => "Zoom Camera";
    }
}
