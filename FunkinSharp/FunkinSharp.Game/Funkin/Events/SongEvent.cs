using System;
using System.Threading;
using FunkinSharp.Game.Funkin.Song;

namespace FunkinSharp.Game.Funkin.Events
{
    /// <summary>
    ///     This class represents a handler for a type of song event.
    /// </summary>
    public abstract partial class SongEvent
    {
        /// <summary>
        ///     The internal song event ID that this handler is responsible for.
        /// </summary>
        public string ID;

        public SongEvent(string id)
        {
            ID = id;
        }

        /// <summary>
        ///     Handles a song event that matches this handler's ID.
        /// </summary>
        /// <param name="eventData">The data associated with the event.</param>
        public abstract void HandleEvent(SongEventData eventData);

        /// <summary>
        ///     Retrieves the chart editor schema for this song event type.
        /// </summary>
        /// <exception cref="NotImplementedException">Charting Screen not implemented</exception>
        public void GetEventSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Retrieves the asset path to the icon this event type should use in the chart editor.
        ///     To customize this, override GetIconPath().
        /// </summary>
        /// <returns>This event icon path</returns>
        public virtual string GetIconPath() => "ui/chart-editor/events/default";

        // TODO: Use the frameworks culture
        /// <summary>
        ///     Retrieves the human readable title of this song event type.
        ///     Used for chart editor.
        /// </summary>
        /// <returns>The title.</returns>
        public virtual string GetTitle() => Thread.CurrentThread.CurrentUICulture.TextInfo.ToTitleCase(ID);

        public new string ToString() => $"SongEvent({ID})";
    }
}
