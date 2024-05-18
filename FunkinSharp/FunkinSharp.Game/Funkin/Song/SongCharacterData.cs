using System.ComponentModel;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/song/SongData.hx#L490
    public class SongCharacterData
    {
        [JsonProperty("player", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string Player;

        [JsonProperty("girlfriend", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string Girlfriend;

        [JsonProperty("opponent", DefaultValueHandling = DefaultValueHandling.Populate)] // i have opps in da block
        [DefaultValue("")]
        public string Opponent;

        [JsonProperty("instrumental", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("")]
        public string Instrumental;

        [JsonProperty("altInstrumentals", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string[] AltInstrumentals = []; 

        public SongCharacterData(string player, string girlfriend, string opponent, string instrumental = "")
        {
            Player = player;
            Girlfriend = girlfriend;
            Opponent = opponent;
            Instrumental = instrumental;
        }

        public new string ToString() => $"SongCharacterData({Player}, {Girlfriend}, {Opponent}, {Instrumental}, [{AltInstrumentals}])";
    }
}
