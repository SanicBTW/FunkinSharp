using System;
using System.Collections.ObjectModel;
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
        public readonly SongEventData[] Events = [];

        [JsonProperty("notes")]
        public readonly ReadOnlyDictionary<string, SongNoteData[]> Notes;

        [JsonProperty("generatedBy")]
        public readonly string GeneratedBy;
    }
}
