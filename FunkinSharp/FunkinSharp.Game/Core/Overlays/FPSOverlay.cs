using System;
using System.Diagnostics;
using FunkinSharp.Game.Funkin;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Framework.Timing;
using osu.Framework.Utils;

namespace FunkinSharp.Game.Core.Overlays
{
    // I'm really sorry I got extremely lazy to code this by myself so most of the code is adapted from below :pray:
    // https://github.com/ppy/osu/blob/master/osu.Game/Graphics/UserInterface/FPSCounter.cs#L22
    public partial class FPSOverlay : VisibilityContainer
    {
        private readonly BindableBool showOverlay = new BindableBool(true);

        private double timePerUpdate = 10;
        private double spikeTimeMS = 20;

        private double displayedFpsCount;
        private double displayedFrameTime;
        private bool isDisplayed;

        private double aimDrawFPS;
        private double aimUpdateFPS;
        private double memPeak;

        private double lastUpdate;
        private ThrottledFrameClock drawClock = null!;
        private ThrottledFrameClock updateClock = null!;
        private ThrottledFrameClock inputClock = null!;

        /// <summary>
        /// The last time value where the display was required (due to a significant change or hovering).
        /// </summary>
        private double lastDisplayRequiredTime;

        private SpriteText fpsText;
        private SpriteText memText;

        public FPSOverlay()
        {
            AutoSizeAxes = Axes.Both;
            Margin = new MarginPadding(8);
        }

        [BackgroundDependencyLoader]
        private void load(GameHost gameHost, FunkinConfig config)
        {
            config.BindWith(FunkinSetting.ShowFPSOverlay, showOverlay);

            FontUsage defaultFont = new FontUsage(family: "OpenSans");

            InternalChild = new FillFlowContainer<SpriteText>()
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Children = new SpriteText[]
                {
                    fpsText = new SpriteText()
                    {
                        Font = defaultFont,
                        Text = "FPS: ? ?ms"
                    },
                    memText = new SpriteText()
                    {
                        Font = defaultFont,
                        Text = "RAM: ? / ?"
                    }
                }
            };

            drawClock = gameHost.DrawThread.Clock;
            updateClock = gameHost.UpdateThread.Clock;
            inputClock = gameHost.InputThread.Clock;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            requestDisplay();

            showOverlay.BindValueChanged((ev) =>
            {
                State.Value = ev.NewValue ? Visibility.Visible : Visibility.Hidden;
                if (ev.NewValue)
                    requestDisplay();
            }, true);
        }

        protected override void Update()
        {
            base.Update();

            double elapsedDrawFrameTime = drawClock.ElapsedFrameTime;
            double elapsedUpdateFrameTime = updateClock.ElapsedFrameTime;

            // If the game goes into a suspended state (ie. debugger attached or backgrounded on a mobile device)
            // we want to ignore really long periods of no processing.
            if (elapsedUpdateFrameTime > 10000)
                return;

            // Handle the case where the window has become inactive or the user changed the
            // frame limiter (we want to show the FPS as it's changing, even if it isn't an outlier).
            bool aimRatesChanged = updateAimFPS();

            bool hasUpdateSpike = displayedFrameTime < spikeTimeMS && elapsedUpdateFrameTime > spikeTimeMS;
            // use elapsed frame time rather then FramesPerSecond to better catch stutter frames.
            bool hasDrawSpike = displayedFpsCount > (1000 / spikeTimeMS) && elapsedDrawFrameTime > spikeTimeMS;

            const float damp_time = 100;

            displayedFrameTime = Interpolation.DampContinuously(displayedFrameTime, elapsedUpdateFrameTime, hasUpdateSpike ? 0 : damp_time, elapsedUpdateFrameTime);

            if (hasDrawSpike)
                // show spike time using raw elapsed value, to account for `FramesPerSecond` being so averaged spike frames don't show.
                displayedFpsCount = 1000 / elapsedDrawFrameTime;
            else
                displayedFpsCount = Interpolation.DampContinuously(displayedFpsCount, drawClock.FramesPerSecond, damp_time, Time.Elapsed);

            if (Time.Current - lastUpdate > timePerUpdate)
            {
                updateCounters();
                lastUpdate = Time.Current;
            }

            bool hasSignificantChanges = aimRatesChanged
                                         || hasDrawSpike
                                         || hasUpdateSpike
                                         || displayedFpsCount < aimDrawFPS * 0.8
                                         || 1000 / displayedFrameTime < aimUpdateFPS * 0.8;

            if (hasSignificantChanges)
                requestDisplay();
            else if (isDisplayed && Time.Current - lastDisplayRequiredTime > 2000 && !IsHovered)
            {
                this.FadeTo(0.7f, 300, Easing.OutQuint);
                isDisplayed = false;
            }
        }

        private void updateCounters()
        {
            fpsText.Colour = getColour(displayedFpsCount / aimDrawFPS);
            fpsText.Text = $"FPS: {(int)Math.Ceiling(displayedFpsCount)} {Math.Round(displayedFrameTime, 2)}ms";

            double curMem = getMemory();
            if (curMem > memPeak) memPeak = Math.Round(curMem, 2);
            memText.Text = $"RAM: {Math.Round(curMem, 2)}mb / {memPeak}mb"; // mem peak is already rounded from before
        }

        private void requestDisplay()
        {
            lastDisplayRequiredTime = Time.Current;
            if (!isDisplayed)
            {
                this.FadeTo(1, 300D, Easing.OutQuint);
                isDisplayed = true;
            }
        }

        private bool updateAimFPS()
        {
            if (updateClock.Throttling)
            {
                double newAimDrawFPS = drawClock.MaximumUpdateHz;
                double newAimUpdateFPS = updateClock.MaximumUpdateHz;

                if (aimDrawFPS != newAimDrawFPS || aimUpdateFPS != newAimUpdateFPS)
                {
                    aimDrawFPS = newAimDrawFPS;
                    aimUpdateFPS = newAimUpdateFPS;
                    return true;
                }
            }
            else
            {
                double newAimFPS = inputClock.MaximumUpdateHz;

                if (aimDrawFPS != newAimFPS || aimUpdateFPS != newAimFPS)
                {
                    aimUpdateFPS = aimDrawFPS = newAimFPS;
                    return true;
                }
            }

            return false;
        }

        private ColourInfo getColour(double performanceRatio)
        {
            if (performanceRatio < 0.5f)
                return Interpolation.ValueAt(performanceRatio, Colour4.Red, Colour4.Orange, 0, 0.5);

            return Interpolation.ValueAt(performanceRatio, Colour4.Orange, Colour4.Lime, 0.5, 0.9);
        }

        private double getMemory()
        {
            float retMem = (Process.GetCurrentProcess().PrivateMemorySize64);

            while (retMem > 1024)
            {
                retMem /= 1024;
            }

            return Math.Round(retMem * 100) / 100;
        }

        protected override void PopIn() => this.FadeIn(100);

        protected override void PopOut() => this.FadeOut(100);
    }
}
