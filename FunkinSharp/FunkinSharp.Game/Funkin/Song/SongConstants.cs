using System;
using FunkinSharp.Game.Core;

namespace FunkinSharp.Game.Funkin.Song
{
    // Used to expose constant values just like the "Contants" file in 0.3 Prototype FNF
    public static class SongConstants
    {
        // Meta stuff
        public static Version METADATA_FORMAT_VERSION { get; private set; } = new Version(2, 2, 2); // Latest known metadata format
        public const string DEFAULT_DIFFICULTY = "normal";
        public const string DEFAULT_CHARACTER = "bf";
        public const string DEFAULT_STAGE = "mainStage";
        public const string DEFAULT_SONG = "test";
        public const string DEFAULT_VARIATION = "default";
        public const int DEFAULT_TIME_SIGNATURE_NUM = 4;
        public const int DEFAULT_TIME_SIGNATURE_DEN = 4;
        public const double DEFAULT_BPM = 100;
        public const string DEFAULT_NOTE_STYLE = "funkin";
        public static string DEFAULT_GENERATED_BY { get; private set; } = $"{GameConstants.TITLE} - {GameConstants.VERSION} {GameConstants.VER_PREFIX}";
        public const string METADATA_SEMVER_RULE = "2.2.x";
        // Chart stuff
        public static Version CHART_FORMAT_VERSION { get; private set; } = new Version(2, 0, 0); // Latest known chart format (all of the 0.3 songs use it)
    }
}
