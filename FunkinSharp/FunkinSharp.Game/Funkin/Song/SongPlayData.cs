using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    // This class on the decompiled source is not complete and thus I'm trying to improvise
    // I think perfectly fits
    public class SongPlayData
    {
        [JsonProperty("album", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("unknown")] // Since it mostly returns null for some songs
        public string Album;

        [JsonProperty("previewStart")]
        public double PreviewStart;

        [JsonProperty("previewEnd")]
        public double PreviewEnd;

        [JsonProperty("ratings")]
        public Dictionary<string, int> Ratings; // idfk what this is - sanco here like 8 hours later dunno, i think its for the freeplay menu or sum

        // the reason behind these are lists, its because the play data seems to be modified on runtime
        [JsonProperty("songVariations")]
        public List<string> SongVariations;

        [JsonProperty("difficulties")]
        public List<string> Difficulties;

        [JsonProperty("characters")]
        public SongCharacterData Characters;

        [JsonProperty("stage")]
        public string Stage;

        [JsonProperty("noteStyle")]
        public string NoteStyle;

        public SongPlayData() { }
    }
}
