using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using osu.Framework.Timing;

namespace FunkinSharp.Game.Core.ReAnimationSystem
{
    public partial class ReAnimation
    {
        public const double DEFAULT_FRAMERATE = 24;

        public List<ReAnimationFrame> Frames { get; protected set; } = [];

        public double FrameRate { get; set; } = DEFAULT_FRAMERATE;

        public bool Loop { get; set; } = true;

        public bool Reverse { get; set; } = false;
        public bool Paused { get; set; } = false;

        public bool FlipHorizontal { get; set; } = false;
        public bool FlipVertical { get; set; } = false;

        public int CurrentFrameIndex { get; private set; }

        public bool Finished { get; private set; }
        public event Action OnFinish;

        private double elapsedTime;

        public void Update(IFrameBasedClock clock)
        {
            if (Paused || Finished) return;

            elapsedTime += (clock.ElapsedFrameTime * clock.Rate);
            while (elapsedTime > FrameRate && !Finished)
            {
                elapsedTime -= FrameRate;
                advanceFrame();
            }
        }

        private void advanceFrame()
        {
            if (Reverse)
            {
                CurrentFrameIndex--;
                if (CurrentFrameIndex < 0)
                {
                    if (Loop)
                        CurrentFrameIndex = Frames.Count - 1;
                    else
                        finish();
                }
            }
            else
            {
                CurrentFrameIndex++;
                if (CurrentFrameIndex >= Frames.Count)
                {
                    if (Loop)
                        CurrentFrameIndex = 0;
                    else
                        finish();
                }
            }
        }

        private void finish()
        {
            CurrentFrameIndex = (Reverse) ? 0 : Frames.Count - 1;
            Paused = true;
            Finished = true;
            OnFinish?.Invoke();
        }

        public void Reset()
        {
            Paused = false;
            Finished = false;
            CurrentFrameIndex = Reverse ? Frames.Count - 1 : 0;
            elapsedTime = 0;
        }

        public static string GetAnimationName(string frameName)
        {
            // Use regex to remove digits from the end of the frame name to get the base animation name.
            return NoNumbersRegex().Replace(frameName, string.Empty);
        }

        [GeneratedRegex(@"\d+$")]
        private static partial Regex NoNumbersRegex();
    }
}
