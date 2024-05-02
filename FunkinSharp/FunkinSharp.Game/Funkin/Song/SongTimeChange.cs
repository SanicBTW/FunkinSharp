using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    // didnt add the beatTime/Tuples shi cuz i didnt know how to
    // ok so bt (beatTime) is apparently an array of ints?
    public class SongTimeChange
    {
        [JsonProperty("t")]
        public readonly double TimeStamp;

        [JsonProperty("bpm")]
        public readonly double BPM;

        [JsonProperty("n")]
        public readonly int TimeSignatureNum;

        [JsonProperty("d")]
        public readonly int TimeSignatureDen;

        public SongTimeChange(double timeStamp, double bpm, int timeSignatureNum = -1, int timeSignatureDen = -1)
        {
            TimeStamp = timeStamp;
            BPM = bpm;

            if (timeSignatureNum == -1)
                timeSignatureNum = SongConstants.DEFAULT_TIME_SIGNATURE_NUM;
            TimeSignatureNum = timeSignatureNum;

            if (timeSignatureDen == -1)
                timeSignatureDen = SongConstants.DEFAULT_TIME_SIGNATURE_DEN;
            TimeSignatureDen = timeSignatureDen;
        }
    }
}
