using FunkinSharp.Game.Core;
using FunkinSharp.Game.Funkin.Song;

namespace FunkinSharp.Game.Funkin.Events
{
    public partial class FocusCameraSongEvent : SongEvent
    {
        public float Duration { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public int Character { get; private set; }
        public string Ease { get; private set; }

        public FocusCameraSongEvent() : base("FocusCamera") { }

        // Since I don't have the "Gameplay Screen" yet (not commited) I won't be trying to make any static stuff, once its done I will surely make all of the stuff static
        // For now the HandleEvent function will work as just setting the data to be accessed on the "Gameplay Screen"
        // WAIT I just realized the class is cached so any modifications made to the instance variables are kept for the next time :skull:
        public override void HandleEvent(SongEventData eventData)
        {
            X = eventData.GetFloat("x") ?? 0.0f;
            Y = eventData.GetFloat("y") ?? 0.0f;

            Character = eventData.GetInt("char") ?? (int)eventData.Value;
            Ease = eventData.GetString("ease") ?? "CLASSIC";

            float duration = eventData.GetFloat("duration") ?? 4.0f;
            Duration = (float)Conductor.StepCrochet * duration / 1000.0f;
        }

        public override string GetTitle() => "Focus Camera";
    }
}
