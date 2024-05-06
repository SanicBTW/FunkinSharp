using FunkinSharp.Game.Funkin.Notes;

namespace FunkinSharp.Game.Core.Utils
{
    // Holds static delegates to be used on event definitions
    public static class EventDelegates
    {
        // aint no senders cuz we aint no instance bro
        public delegate void IntValueUpdate(int e);
        public delegate void BPMValueUpdate(double last, double current);

        public delegate void NoteEvent(Note note);
    }
}
