using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    public class SongOffsets
    {
        [JsonProperty("instrumental", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(0.0)] // most of the metadata already defines the instrumental offset but just in case its missing
        public readonly double Instrumental;

        [JsonProperty("altInstrumentals")]
        public readonly Dictionary<string, double> AltInstrumentals; // Idk how this one will look

        // the structure is easy, { bf: offsetTime, dad: offsetTime }
        [JsonProperty("vocals")]
        public readonly Dictionary<string, double> Vocals;

        public SongOffsets(double instrumental = -1, Dictionary<string, double> altInstrumentals = null, Dictionary<string, double> vocals = null)
        {
            if (instrumental == -1)
                Instrumental = 0.0;

            AltInstrumentals = altInstrumentals ?? [];
            Vocals = vocals ?? [];
        }
    }
}
