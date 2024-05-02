using System;

namespace FunkinSharp.Game.Funkin.Song
{
    // Used to expose constant values just like the "Contants" file in 0.3 Prototype FNF
    public static class SongConstants
    {
        public static Version CHART_FORMAT_VERSION { get; private set; } = new Version(2, 2, 2); // Latest known chart format
        public const string DEFAULT_DIFFICULTY = "normal";
        public const string DEFAULT_CHARACTER = "bf";
        public const string DEFAULT_STAGE = "mainStage";
        public const string DEFAULT_SONG = "test";
        public const string DEFAULT_VARIATION = "default";
        public const int DEFAULT_TIME_SIGNATURE_NUM = 4;
        public const int DEFAULT_TIME_SIGNATURE_DEN = 4;
        public const double DEFAULT_BPM = 100;
        public const string DEFAULT_NOTE_STYLE = "funkin";
    }
}
