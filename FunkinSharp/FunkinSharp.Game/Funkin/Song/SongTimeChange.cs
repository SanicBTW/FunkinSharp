using System.ComponentModel;
using FunkinSharp.Game.Core.Utils;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/song/SongData.hx#L132
    public enum SongTimeFormat
    {
        [StringValue("ticks")]
        TICKS,
        [StringValue("float")]
        FLOAT,
        [StringValue("ms")]
        MS
    }

    // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/song/SongData.hx#L139
    public class SongTimeChange
    {
        [JsonProperty("t")]
        public double TimeStamp;

        // Defaults to -1 since i cannot use null properly (fuck c#)
        [JsonProperty("b", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(-1)]
        public double BeatTime;

        [JsonProperty("bpm")]
        public double BPM;

        [JsonProperty("n", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(SongConstants.DEFAULT_TIME_SIGNATURE_NUM)]
        public int TimeSignatureNum;

        [JsonProperty("d", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(SongConstants.DEFAULT_TIME_SIGNATURE_DEN)]
        public int TimeSignatureDen;

        [JsonProperty("bt", NullValueHandling = NullValueHandling.Ignore)]
        public int[] BeatTuplets;

        public SongTimeChange(double timeStamp, double bpm, int timeSignatureNum = 4, int timeSignatureDen = 4, double? beatTime = null, int[] beatTuplets = null)
        {
            TimeStamp = timeStamp;
            BPM = bpm;

            TimeSignatureNum = timeSignatureNum;
            TimeSignatureDen = timeSignatureDen;

            if (beatTime is not null)
                BeatTime = (double)beatTime;

            BeatTuplets = beatTuplets ?? [4, 4, 4, 4];
        }

        public new string ToString() => $"SongTimeChange({TimeStamp}ms, {BPM}bpm)";
    }
}
