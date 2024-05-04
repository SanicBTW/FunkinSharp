using System.ComponentModel;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    // so the decompiled source says it uses step time but nah i'd win
    // ok yeah they are steps but just in case imma use strum time cuz why not
    // both time and length are step time :sob:
    // decompiled mentions note "kind" but in charts theres nothing
    // nvm i found a chart that uses kind, basically is the note type shi from psych and yknow the drill
    public class SongNoteData
    {
        [JsonProperty("t")]
        public readonly float Time;

        [JsonProperty("d")]
        public readonly int Data;

        [JsonProperty("l")]
        public readonly float Length;

        [JsonProperty("k")]
        [DefaultValue("normal")]
        public readonly string Kind;

        public SongNoteData(float time, int data, float length, string kind = "normal")
        {
            Time = time;
            Data = data;
            Length = length;
            Kind = kind;
        }
    }
}
