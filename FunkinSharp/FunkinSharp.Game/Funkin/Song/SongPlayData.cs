﻿using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/song/SongData.hx#L393
    public class SongPlayData
    {
        [JsonProperty("songVariations", DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> SongVariations = [];

        [JsonProperty("difficulties")]
        public List<string> Difficulties;

        [JsonProperty("characters")]
        public SongCharacterData Characters;

        [JsonProperty("stage")]
        public string Stage;

        [JsonProperty("noteStyle")]
        public string NoteStyle;

        [JsonProperty("ratings", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, int> Ratings = new Dictionary<string, int>()
            {
                { "normal", 0 }
            };

        [JsonProperty("album", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("Unknown")]
        public string Album;

        [JsonProperty("previewStart", DefaultValueHandling = DefaultValueHandling.Populate, NullValueHandling = NullValueHandling.Ignore)]
        [DefaultValue(0.0)]
        public double PreviewStart;

        [JsonProperty("previewEnd", DefaultValueHandling = DefaultValueHandling.Populate, NullValueHandling = NullValueHandling.Ignore)]
        [DefaultValue(15000.0)]
        public double PreviewEnd;

        public SongPlayData() { }

        public new string ToString() => $"SongPlayData({SongVariations}, {Difficulties})";
    }
}
