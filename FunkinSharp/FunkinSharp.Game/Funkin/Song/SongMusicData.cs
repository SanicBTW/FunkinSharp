using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/song/SongData.hx#L317
    public class SongMusicData
    {
        [JsonProperty("version")]
        public readonly Version Version;

        [JsonProperty("songName", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("Unknown")]
        public string SongName;

        [JsonProperty("artist", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("Unknown")]
        public string Artist;

        [JsonProperty("divisions", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(96)]
        public int Divisions;

        [JsonProperty("looped", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(false)]
        public bool Looped;

        [JsonProperty("generatedBy")]
        public string GeneratedBy;

        [JsonProperty("timeFormat")]
        public SongTimeFormat TimeFormat;

        [JsonProperty("timeChanges")]
        public SongTimeChange[] TimeChanges; // i could set it to list but i think a fixed array is way better

        [JsonIgnore]
        public string Variation; // this is set in runtime and not parsed in metadata

        public SongMusicData(string songName, string artist, string variation = "default")
        {
            Version = SongConstants.CHART_FORMAT_VERSION;
            SongName = songName;
            Artist = artist;
            TimeFormat = SongTimeFormat.MS;
            Divisions = -1;
            TimeChanges = [new SongTimeChange(0, SongConstants.DEFAULT_BPM)];
            Looped = false;

            // We gettin lit with this one
            GeneratedBy = SongConstants.DEFAULT_GENERATED_BY;
            Variation = variation ?? SongConstants.DEFAULT_VARIATION;
        }

        public new string ToString() => $"SongMusicData({SongName} by {Artist}, variation {Variation})";
    }
}
