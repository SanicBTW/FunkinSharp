using FunkinSharp.Game.Funkin.Song;

namespace FunkinSharp.Game.Funkin.Events
{
    public class SetCameraBopSongEvent : SongEvent
    {
        public int Rate { get; private set; }
        public float Intensity { get; private set; }

        public SetCameraBopSongEvent() : base("SetCameraBop") { }

        public override void HandleEvent(SongEventData eventData)
        {
            Rate = eventData.GetInt("rate") ?? 4;
            Intensity = eventData.GetFloat("intensity") ?? 1.0f;
        }

        public override string GetTitle() => "Set Camera Bop";
    }
}
