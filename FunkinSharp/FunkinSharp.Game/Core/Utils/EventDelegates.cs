namespace FunkinSharp.Game.Core.Utils
{
    // Holds static delegates to be used on event definitions
    public static class EventDelegates
    {
        // aint no senders cuz we aint no instance bro
        public delegate void IntValueUpdate(int e);
        public delegate void FloatValueUpdate(double last, double current);
    }
}
