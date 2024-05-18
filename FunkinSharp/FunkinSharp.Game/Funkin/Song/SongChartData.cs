using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/song/SongData.hx#L537
    public class SongChartData
    {
        [JsonProperty("version")]
        public Version Version;

        [JsonProperty("scrollSpeed")]
        public Dictionary<string, double> ScrollSpeeds;

        [JsonProperty("events")]
        public SongEventData[] Events = [];

        [JsonProperty("notes")]
        public Dictionary<string, SongNoteData[]> Notes;

        [JsonProperty("generatedBy")]
        public string GeneratedBy; // See SongMetadata.GeneratedBy

        [JsonIgnore]
        public string Variation;

        public SongChartData(Dictionary<string, double> scrollSpeeds, SongEventData[] events, Dictionary<string, SongNoteData[]> notes)
        {
            Version = SongConstants.CHART_FORMAT_VERSION;
            Events = events;
            Notes = notes;
            ScrollSpeeds = scrollSpeeds;
            GeneratedBy = SongConstants.DEFAULT_GENERATED_BY;
        }

        public double GetScrollSpeed(string diff = "default")
        {
            if (ScrollSpeeds.TryGetValue(diff, out var scrollSpeed))
                return (scrollSpeed == 0.0) ? 1.0 : scrollSpeed;
            else if (diff != "default")
                return GetScrollSpeed("default");

            return 1.0;
        }

        public double SetScrollSpeed(double value, string diff = "default")
        {
            ScrollSpeeds[diff] = value;
            return value;
        }

        public SongNoteData[] GetNotes(string diff)
        {
            if (Notes.TryGetValue(diff, out var notes))
                return notes;
            else if (diff != "normal")
                return GetNotes("normal");

            return [];
        }

        public SongNoteData[] SetNotes(SongNoteData[] value, string diff)
        {
            Notes[diff] = value;
            return value;
        }

        public new string ToString() => $"SongChartData({Events.Length} events, {Notes.Count} difficulties, {GeneratedBy})";
    }
}
