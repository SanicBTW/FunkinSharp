namespace FunkinSharp.Game.Funkin.Events
{
    // Used to be able to pass different types of events into the generic
    public interface ISongEvent
    {
        public int Duration { get; }
    }
}
