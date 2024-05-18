using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/song/SongData.hx#L16
    public class SongMetadata
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

        [JsonProperty("offsets", NullValueHandling = NullValueHandling.Ignore)]
        public SongOffsets Offsets;

        [JsonProperty("playData")]
        public SongPlayData PlayData;

        [JsonProperty("generatedBy")]
        public string GeneratedBy; // this should be set as a default value but i cannot pass SongConstants.GENERATED_BY because it aint a real const!!!

        [JsonProperty("timeFormat")]
        public SongTimeFormat TimeFormat;

        [JsonProperty("timeChanges")]
        public SongTimeChange[] TimeChanges; // i could set it to list but i think a fixed array is way better

        [JsonIgnore]
        public string Variation; // this is set in runtime and not parsed in metadata

        public SongMetadata(string songName, string artist, string variation = null)
        {
            Version = SongConstants.METADATA_FORMAT_VERSION;
            SongName = songName;
            Artist = artist;
            TimeFormat = SongTimeFormat.MS;
            Divisions = -1;
            Offsets = new SongOffsets();
            TimeChanges = [new SongTimeChange(0, SongConstants.DEFAULT_BPM)];
            Looped = false;
            PlayData = new SongPlayData
            {
                SongVariations = [],
                Difficulties = [],
                Characters = new SongCharacterData("bf", "gf", "dad"),
                Stage = "mainStage",
                NoteStyle = SongConstants.DEFAULT_NOTE_STYLE
            };

            // We gettin lit with this one
            GeneratedBy = SongConstants.DEFAULT_GENERATED_BY;
            Variation = variation ?? SongConstants.DEFAULT_VARIATION;
        }

        public new string ToString() => $"SongMetadata({SongName} by {Artist}, variation {Variation})";
    }
}
