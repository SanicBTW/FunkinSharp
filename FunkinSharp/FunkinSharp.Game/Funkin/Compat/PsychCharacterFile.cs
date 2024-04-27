using Newtonsoft.Json;

// Legacy code I wrote back when testing, it's still the same since I cannot really use most of the stuff here yet
namespace FunkinSharp.Game.Funkin.Compat
{
    public struct PsychCharacterFile
    {
        [JsonProperty("animations")]
        public PsychAnimArray[] Animations { get; private set; }
        [JsonProperty("image")] // The JSON gets parsed before the XML so we use this property as the XML file target
        public string Image { get; private set; }
        [JsonProperty("scale")]
        public float Scale { get; private set; }
        [JsonProperty("sing_duration")]
        public float SingDuration { get; private set; }
        [JsonProperty("healthicon")]
        public string HealthIcon { get; private set; }

        [JsonProperty("position")]
        public float[] Position { get; private set; }
        [JsonProperty("camera_position")]
        public float[] CameraPosition { get; private set; }
        [JsonProperty("flip_x")]
        public bool FlipX { get; private set; }
        [JsonProperty("no_antialiasing")] // Not used in the Framework (yet - 27/04/2024, probably about 2/3 weeks later since last test)
        public bool NoAntialiasing { get; private set; }
        [JsonProperty("healthbar_colors")]
        public int[] HealthBar_Colors { get; private set; }
    }

    public struct PsychAnimArray
    {
        [JsonProperty("anim")]
        public string Animation { get; private set; }
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("fps")] // Should be taken into account
        public int FPS { get; private set; }
        [JsonProperty("loop")] // Should be taken into account
        public bool Loop { get; private set; }
        [JsonProperty("indices")] // Not Supported by the XML Parser but maybe could override the existing animation with the provided indices¿
        public int[] Indices { get; private set; }
        [JsonProperty("offsets")]
        public int[] Offsets { get; private set; }
    }
}
