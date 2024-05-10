using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FunkinSharp.Game.Funkin.Events;
using FunkinSharp.Game.Funkin.Song;
using osu.Framework.Logging;

namespace FunkinSharp.Game.Funkin.Data.Event
{
    // no macros!! so we gotta hardcode the built in events https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/event/SongEventRegistry.hx
    // TODO: Fix docs
    /// <summary>
    ///     This class statically handles the parsing of internal and scripted song event handlers.
    /// </summary>
    public static class SongEventRegistry
    {
        // Have to manually set the types of the events in the list
        private static List<Type> builtIn_Events = [typeof(FocusCameraSongEvent), typeof(ZoomCameraSongEvent), typeof(SetCameraBopSongEvent)];

        private static Dictionary<string, SongEvent> eventCache = [];

        public static void LoadEventCache()
        {
            eventCache.Clear();

            // Base Events
            registerBaseEvents();
        }

        private static void registerBaseEvents()
        {
            // I did the same constructor trick in the assets stuff, not published yet but exists on the latest patches/versions
            Logger.Log($"Instantiating {builtIn_Events.Count} built-in song events...");
            foreach (Type evType in builtIn_Events)
            {
                // We dont pass any arguments since it extends SongEvent and the events already call super with the event name
                // Also these logs are only shown when running thru a console since in this moment the game logger hasn't been initialized yet
                ConstructorInfo evCtor = evType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, [], null);
                if (evCtor != null)
                {
                    try
                    {
                        SongEvent newEvent = (SongEvent)evCtor.Invoke([]);
                        Logger.Log($"Loaded built-in song event: {newEvent.ID}");
                        eventCache[newEvent.ID] = newEvent;
                    }
                    catch (Exception)
                    {
                        Logger.Log($"Failed to load built-in song event: {evType.FullName}", LoggingTarget.Runtime, LogLevel.Error);
                    }
                }
                else
                {
                    Logger.Log($"Failed to instantiate event constructor for {evType.FullName}", LoggingTarget.Runtime, LogLevel.Error);
                }
            }
        }

        public static string[] ListEventIDs()
        {
            string[] ret = new string[eventCache.Count];
            eventCache.Keys.CopyTo(ret, 0);
            return ret;
        }

        public static SongEvent[] ListEvents()
        {
            SongEvent[] ret = new SongEvent[eventCache.Count];
            eventCache.Values.CopyTo(ret, 0);
            return ret;
        }

        public static SongEvent GetEvent(string id)
        {
            if (eventCache.TryGetValue(id, out SongEvent ev))
                return ev;
            return null;
        }

        // event schema

        public static void HandleEvent(SongEventData data)
        {
            SongEvent handler = GetEvent(data.Kind);
            if (handler != null)
                handler.HandleEvent(data);
            else
                Logger.Log($"No event handler for event with kind: {data.Kind}", LoggingTarget.Runtime, LogLevel.Important);
            data.Activated = true;
        }

        public static void HandleEvents(SongEventData[] events)
        {
            foreach (SongEventData data in events)
                HandleEvent(data);
        }

        /// <summary>
        ///     Given a list of song events and the current timestamp,
        ///     <para/>
        ///     return a list of events that should be handled.
        /// </summary>
        /// <param name="events">List of song events.</param>
        /// <param name="currentTime">The current timestamp.</param>
        /// <returns>List of events that should be handled.</returns>
        public static SongEventData[] QueryEvents(SongEventData[] events, float currentTime) => events.Where(e => !e.Activated && e.Time <= currentTime).ToArray();

        /// <summary>
        ///     The currentTime has jumped far ahead or back.
        ///     <para/>
        ///     If we moved back in time, we need to reset all the events in that space.
        ///     <para/>
        ///     If we moved forward in time, we need to skip all the events in that space.
        /// </summary>
        /// <param name="events">List of song events.</param>
        /// <param name="currentTime">The current timestamp.</param>
        public static void HandleSkippedEvents(SongEventData[] events, float currentTime)
        {
            foreach (SongEventData data in events)
            {
                if (data.Time > currentTime)
                    data.Activated = false;

                if (data.Time < currentTime)
                    data.Activated = true;
            }
        }

        /// <summary>
        ///     Reset activation of all the provided events.
        /// </summary>
        /// <param name="events">List of song events.</param>
        public static void ResetEvents(SongEventData[] events)
        {
            foreach (SongEventData data in events)
                data.Activated = false;
        }
    }
}
