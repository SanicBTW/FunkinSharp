using System;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    // Currently in FNF 0.3 Prototype, the smallest metadata version is 2.2.0
    public class SongMetadata
    {
        [JsonProperty("version")]
        public readonly Version Version;

        [JsonProperty("songName")]
        public readonly string SongName;

        [JsonProperty("artist")]
        public readonly string Artist;

        [JsonProperty("timeFormat")]
        public readonly string TimeFormat;

        [JsonProperty("divisions")]
        public readonly int Divisions; // idk if its an int but its most likely is

        [JsonProperty("offsets")]
        public readonly SongOffsets Offsets;

        [JsonProperty("timeChanges")]
        public readonly SongTimeChange[] TimeChanges; // i could set it to list but i think a fixed array is way better

        [JsonProperty("looped")]
        public bool Looped; // the fuck is this for??

        [JsonProperty("playData")]
        public readonly SongPlayData PlayData;

        [JsonProperty("generatedBy")]
        public readonly string GeneratedBy;

        [JsonIgnore]
        public readonly string Variation; // this is set in runtime and not parsed in metadata

        public SongMetadata(string songName, string artist, string variation = null)
        {
            Version = SongConstants.METADATA_FORMAT_VERSION;
            SongName = songName;
            Artist = artist;
            TimeFormat = "ms";
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
    }
}
