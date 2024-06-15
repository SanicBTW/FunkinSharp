using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace FunkinSharp.Game.Funkin
{
    public class FunkinConfig : IniConfigManager<FunkinSetting>
    {
        protected override string Filename => @"FunkinConfig.ini";

        protected override void InitialiseDefaults()
        {
            SetDefault(FunkinSetting.DownScroll, false);
            SetDefault(FunkinSetting.MiddleScroll, false); // Enforced on non fnf charts
            SetDefault(FunkinSetting.GhostTapping, true);
            SetDefault(FunkinSetting.ScrollSpeed, 0.0f); // 0 to use the map speed
            SetDefault(FunkinSetting.ShowPercentageOnBootup, false);
            SetDefault(FunkinSetting.UseFlixelTray, true);
            SetDefault(FunkinSetting.UseLegacyNoteSpritesheet, false); // Set to false for sustains, the rest of the sprites will not apply this setting until i port the new textures over
        }

        public FunkinConfig(Storage storage, IDictionary<FunkinSetting, object> defaultOverrides = null)
            : base(storage, defaultOverrides)
        {
            Save();
        }
    }

    public enum FunkinSetting
    {
        DownScroll,
        MiddleScroll,
        GhostTapping,
        ScrollSpeed,
        ShowPercentageOnBootup,
        UseFlixelTray,
        UseLegacyNoteSpritesheet
    }
}
