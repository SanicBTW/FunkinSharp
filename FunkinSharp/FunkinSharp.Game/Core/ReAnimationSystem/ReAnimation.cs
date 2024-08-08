using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using osu.Framework.Timing;
using osu.Framework.Utils;

namespace FunkinSharp.Game.Core.ReAnimationSystem
{
    public partial class ReAnimation
    {
        public const double DEFAULT_FRAMERATE = 24;

        protected ReAnimatedSprite Controller;

        public string Name { get; private set; } = "unset";

        public List<int> Frames { get; protected set; } = [];

        private double frameRate = DEFAULT_FRAMERATE;
        public double FrameRate
        {
            get => frameRate;
            set
            {
                frameRate = value;
                frameDuration = value > 0 ? 1.0 / value : 0;
            }
        }

        public bool Loop { get; set; } = true;

        public bool Reversed { get; set; } = false;
        public bool Paused { get; set; } = false;

        public bool FlipHorizontal { get; set; } = false;
        public bool FlipVertical { get; set; } = false;

        public int CurrentFrameIndex { get; private set; }
        public int LoopPoint { get; set; } = 0;

        public bool Finished { get; private set; }
        public event Action OnFinish;

        private double elapsedTime;
        private double frameDuration = 1.0 / DEFAULT_FRAMERATE; // we calculate the frame duration based on the default anim fps so it isnt 0 on class creation

        public ReAnimation(ReAnimatedSprite parent, string animation)
        {
            Controller = parent;
            Name = animation;
        }

        public void Play(bool Force = false, bool Reversed = false, int Frame = 0)
        {
            if (!Force && !Finished && this.Reversed == Reversed)
            {
                Paused = false;
                return;
            }

            this.Reversed = Reversed;
            Paused = false;
            elapsedTime = 0;
            Finished = frameDuration == 0;

            int maxFrameIndex = Frames.Count - 1;
            if (Frame < 0)
                setFrame(RNG.Next(0, maxFrameIndex));
            else
            {
                if (Frame > maxFrameIndex)
                    Frame = maxFrameIndex;
                if (this.Reversed)
                    Frame = (maxFrameIndex - Frame);
                setFrame(Frame);
            }

            if (Finished)
                OnFinish?.Invoke();
        }

        public void Restart() => Play(true, Reversed);

        public void Stop()
        {
            Finished = true;
            Paused = true;
        }

        public void Reset()
        {
            Stop();
            setFrame(Reversed ? Frames.Count - 1 : 0);
        }

        public void Finish()
        {
            Stop();
            setFrame(Reversed ? 0 : Frames.Count - 1);
        }

        public void Pause() => Paused = true;

        public void Resume() => Paused = false;

        public void Reverse()
        {
            Reversed = !Reversed;
            if (Finished)
                Play(false, Reversed);
        }

        public virtual void Update(IFrameBasedClock clock)
        {
            // we check for the frame duration but we use the framerate to advance frames since using frameDuration slows the animation
            double curFrameDuration = frameDuration;
            if (curFrameDuration == 0 || Finished || Paused)
                return;

            elapsedTime += (clock.ElapsedFrameTime * clock.Rate);
            while (elapsedTime > FrameRate && !Finished)
            {
                elapsedTime -= FrameRate;
                if (Reversed)
                {
                    if (Loop && CurrentFrameIndex == LoopPoint)
                        setFrame(Frames.Count - 1);
                    else
                        setFrame(CurrentFrameIndex - 1);
                }
                else
                {
                    if (Loop && CurrentFrameIndex == Frames.Count - 1)
                        setFrame(LoopPoint);
                    else
                        setFrame(CurrentFrameIndex + 1);
                }

                if (Finished)
                    break;
            }
        }
        
        private void setFrame(int frame)
        {
            int maxFrameIndex = Frames.Count - 1;
            int tempFrame = Reversed ? maxFrameIndex - frame : frame;

            if (tempFrame >= 0)
            {
                if (!Loop && tempFrame > maxFrameIndex)
                {
                    Finished = true;
                    CurrentFrameIndex = Reversed ? 0 : maxFrameIndex;
                }
                else
                    CurrentFrameIndex = frame;
            }
            else
                CurrentFrameIndex = RNG.Next(0, maxFrameIndex);

            if (Finished)
                OnFinish?.Invoke();
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
