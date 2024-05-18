using System;
using FunkinSharp.Game.Core;

namespace FunkinSharp.Game.Funkin.Song
{
    // Used to expose constant values just like the "Contants" file in 0.3 Prototype FNF
    public static class SongConstants
    {
        // Meta stuff
        public static Version METADATA_FORMAT_VERSION => new Version(2, 2, 2); // Latest known metadata format
        public const string DEFAULT_DIFFICULTY = "normal";
        public const string DEFAULT_CHARACTER = "bf";
        public const string DEFAULT_STAGE = "mainStage";
        public const string DEFAULT_SONG = "test";
        public const string DEFAULT_VARIATION = "default";
        public const int DEFAULT_TIME_SIGNATURE_NUM = 4;
        public const int DEFAULT_TIME_SIGNATURE_DEN = 4;
        public const int SECS_PER_MIN = 60;
        public const int MS_PER_SEC = 1000;
        public const int STEPS_PER_BEAT = 4;
        public const double DEFAULT_BPM = 100;
        public const string DEFAULT_NOTE_STYLE = "funkin";
        public const float PIXELS_PER_MS = 0.45f; // This was Conductor.RATE, its a float so it doesnt break existing logic and doesnt require type conversion (float -> double) but might move to double sometime
        public static string DEFAULT_GENERATED_BY => $"{GameConstants.TITLE} - {GameConstants.VERSION} {GameConstants.VER_PREFIX}";
        public const string METADATA_SEMVER_RULE = "2.2.x";
        // Chart stuff
        public static Version CHART_FORMAT_VERSION => new Version(2, 0, 0); // Latest known chart format (all of the 0.3 songs use it)
    }
}
