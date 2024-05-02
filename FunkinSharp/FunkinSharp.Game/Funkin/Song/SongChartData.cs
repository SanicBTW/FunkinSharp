using System;
using System.Collections.ObjectModel;
using FunkinSharp.Game.Funkin.Events;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    public class SongChartData
    {
        [JsonProperty("version")]
        public readonly Version Version;

        [JsonProperty("scrollSpeed")]
        public readonly ReadOnlyDictionary<string, double> ScrollSpeeds;

        // The event value inside the event data is set through the converter
        [JsonProperty("events")]
        [JsonConverter(typeof(EventTypeConverter))]
        public readonly SongEventData<ISongEvent>[] Events = [];

        [JsonProperty("notes")]
        public readonly ReadOnlyDictionary<string, SongNoteData[]> Notes;

        [JsonProperty("generatedBy")]
        public readonly string GeneratedBy;
    }
}
