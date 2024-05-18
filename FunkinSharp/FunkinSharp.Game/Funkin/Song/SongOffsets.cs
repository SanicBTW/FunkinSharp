using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/song/SongData.hx#L225
    public class SongOffsets
    {
        [JsonProperty("instrumental", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(0.0)]
        public double Instrumental;

        [JsonProperty("altInstrumentals", DefaultValueHandling = DefaultValueHandling.Populate)]
        public Dictionary<string, double> AltInstrumentals = [];

        [JsonProperty("vocals", DefaultValueHandling = DefaultValueHandling.Populate)]
        public Dictionary<string, double> Vocals = [];

        public SongOffsets(double instrumental = 0.0, Dictionary<string, double> altInstrumentals = null, Dictionary<string, double> vocals = null)
        {
            Instrumental = instrumental;
            AltInstrumentals = altInstrumentals ?? [];
            Vocals = vocals ?? [];
        }

        public double GetInstrumentalOffset(string instrumental = null)
        {
            if (instrumental == null || instrumental == "") return Instrumental;
            if (!AltInstrumentals.TryGetValue(instrumental, out double value)) return Instrumental;
            return value;
        }

        public double SetInstrumentalOffset(double value, string instrumental = null)
        {
            if (instrumental == null || instrumental == "")
                Instrumental = value;
            else
                AltInstrumentals[instrumental] = value;
            return value;
        }

        public double GetVocalOffset(string charId)
        {
            if (!Vocals.TryGetValue(charId, out double value)) return 0.0;
            return value;
        }

        public double SetVocalOffset(string charId, double value)
        {
            Vocals[charId] = value;
            return value;
        }

        public new string ToString() => $"SongOffsets({Instrumental}ms, {AltInstrumentals}, {Vocals})";
    }
}
