using System;
using FunkinSharp.Game.Funkin;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace FunkinSharp.Game.Core.Overlays
{
    // https://github.com/ppy/osu/blob/318598730b0fea6c2fa66217a6c39b9998771486/osu.Game/Overlays/VolumeOverlay.cs
    public partial class VolumeOverlay : VisibilityContainer
    {
        private BindableBool useFlixelTray = new BindableBool(true);

        // Cache these bad boys
        private FlixelTray tray = new();

        private double maxTime = 1000D;
        private double visibleTime = 0D;

        public VolumeOverlay() { }

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig, FunkinConfig config)
        {
            useFlixelTray.BindTo(config.GetBindable<bool>(FunkinSetting.UseFlixelTray));

            if (useFlixelTray.Value)
            {
                Anchor = Origin = Anchor.TopCentre;
                Add(tray);

                // make current volume bars visible 
                tray.UpdateBars((int)Math.Round(frameworkConfig.Get<double>(FrameworkSetting.VolumeUniversal) * 10));
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ToggleVisibility(); // make it show for some time when loaded so the user knows :money_mouth:
        }

        protected override void PopIn()
        {
            visibleTime = 0D;
            if (useFlixelTray.Value)
                tray.SlideIn();
        }

        protected override void PopOut()
        {
            if (useFlixelTray.Value)
                tray.SlideOut();
        }

        public void UpdateVol(bool up = false, bool mute = false)
        {
            visibleTime = 0D;
            if (useFlixelTray.Value)
                tray.UpdateVol(up, mute);
        }

        protected override void Update()
        {
            if (State.Value == Visibility.Visible)
            {
                if (visibleTime >= maxTime)
                    ToggleVisibility();

                visibleTime += Clock.ElapsedFrameTime;
            }

            base.Update();
        }
    }
}
