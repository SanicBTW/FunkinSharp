using System;

namespace FunkinSharp.Game.Funkin
{
    // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/play/scoring/Scoring.hx
    public static class Scoring
    {
        public const int PBOT1_MAX_SCORE = 500;
        public const float PBOT1_SCORING_OFFSET = 54.99f;
        public const float PBOT1_SCORING_SLOPE = 0.080f;
        public const int PBOT1_MIN_SCORE = 9;
        public const int PBOT1_MISS_SCORE = 0;

        public const float PBOT1_PERFECT_THRESHOLD = 5.0f;
        public const float PBOT1_MISS_THRESHOLD = 160.0f;

        public const float PBOT1_KILLER_THRESHOLD = 12.5f; // although it isnt being used atm, im gonna port it anyways in case it gets used on another update
        public const float PBOT1_SICK_THRESHOLD = 45.0f;
        public const float PBOT1_GOOD_THRESHOLD = 90.0f;
        public const float PBOT1_BAD_THRESHOLD = 135.0f;
        public const float PBOT1_SHIT_THRESHOLD = 160.0f; // why not call it a miss already?

        public const float LEGACY_HIT_WINDOW = ((10.0f / 60.0f) * 1000.0f) / PBOT1_MISS_THRESHOLD;

        public static int ScoreNote(float timing)
        {
            float absTiming = Math.Abs(timing);

            if (absTiming > PBOT1_MISS_THRESHOLD)
                return PBOT1_MISS_SCORE;
            else if (absTiming < PBOT1_PERFECT_THRESHOLD)
                return PBOT1_MAX_SCORE;
            else
            {
                float factor = 1.0f - (1.0f / (1.0f + (float)Math.Exp(-PBOT1_SCORING_SLOPE * (absTiming - PBOT1_SCORING_OFFSET))));
                return (int)(PBOT1_MAX_SCORE * factor * PBOT1_MIN_SCORE);
            }
        }

        public static string JudgeNote(float timing)
        {
            float absTiming = Math.Abs(timing);

            /* if (absTiming < PBOT1_KILLER_THRESHOLD)
                return "killer";
            else */
            if (absTiming < PBOT1_SICK_THRESHOLD)
                return "sick";
            else if (absTiming < PBOT1_GOOD_THRESHOLD)
                return "good";
            else if (absTiming < PBOT1_BAD_THRESHOLD)
                return "bad";
            else if (absTiming < PBOT1_SHIT_THRESHOLD)
                return "shit";
            else // not meant to happen apparently
                return "miss";
        }
    }
}
