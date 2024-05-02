using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    // this had alt instrumentals and instrumentals but idfk why so i aint adding them here lmao
    // sanco here like about 8 hours later, so the instrumental fields is to indicate which instrumental to use
    // for ex: the normal meta inst will be an empty string while the erect meta will have "erect" in the field
    // I will add the alt instrumentals field just in case but they dont seem to be used at all (maybe it works like the new voices system)
    public class SongCharacterData
    {
        [JsonProperty("player")]
        public readonly string Player;

        [JsonProperty("girlfriend")]
        public readonly string Girlfriend;

        [JsonProperty("opponent")] // i have opps in da block
        public readonly string Opponent;

        [JsonProperty("instrumental")]
        public readonly string Instrumental;

        [JsonProperty("altInstrumentals")]
        public readonly string[] AltInstrumentals; // Most likely to be an array of strings, trust me bro

        public SongCharacterData(string player, string girlfriend, string opponent, string instrumental = null)
        {
            Player = player;
            Girlfriend = girlfriend;
            Opponent = opponent;

            // we check if the provided instrumental is null since we are letting the JSON parser get the fields automatically (if the class is being passed thru a parser)
            if (instrumental != null)
                Instrumental = instrumental;

            AltInstrumentals ??= []; // if jit is null, set it to an empty array 
        }
    }
}
