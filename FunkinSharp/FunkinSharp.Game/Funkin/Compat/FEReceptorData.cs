using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Compat
{
    // Forever Engine : Rewrite Receptor Data https://github.com/SanicBTW/Just-Another-FNF-Engine/blob/master/source/funkin/notes/Note.hx#L140
    // The reason behind I'm using this is to make it somewhat easy to add custom note skins instead of trying to code them (WIP Mod support??)
    public struct FEReceptorData
    {
        [JsonProperty("keyAmount")]
        public int KeyAmount { get; private set; }
        [JsonProperty("actions")]
        public string[] Actions { get; private set; }
        [JsonProperty("colors")]
        public string[] Colors { get; private set; }
        [JsonProperty("separation")]
        public float Separation { get; private set; }
        [JsonProperty("size")]
        public float Size { get; private set; }
        // There's also antialiasing but we ain't adding it

        // The sparrow we want to use in the skin, this is propietary to this engine
        [JsonProperty("texture")]
        public string Texture { get; private set; }
    }
}
