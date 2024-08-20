using FunkinSharp.Game.Core;
using FunkinSharp.Game.Funkin;
using FunkinSharp.Game.Funkin.Screens;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace FunkinSharp.Game
{
    public partial class MainScreen : FunkinScreen
    {
        private Sprite bg;
        private SpriteText curLoad;
        private string lastFormat;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                bg = new Sprite
                {
                    Texture = Paths.GetTexture("Textures/General/preloaderArt.png"), // we use the Paths cache so the texture can get cleaned
                    Size = new osuTK.Vector2(GameConstants.WIDTH, GameConstants.HEIGHT),
                    Alpha = 0f
                },
                curLoad = new SpriteText
                {
                    Font = new FontUsage(family: "RedHatDisplay", size: 40, weight: "Bold"),
                    Text = "Please wait, loading...",
                    Margin = new MarginPadding(16),
                    Alpha = 0f
                }
            };

            bg.FadeIn(1500D, Easing.InQuint).OnComplete((_) =>
            {
                curLoad.FadeIn(500D, Easing.InQuint).OnComplete((_) =>
                {
                    ChartRegistry.Scan((newFormat) =>
                    {
                        if (lastFormat != newFormat)
                            lastFormat = newFormat;
                    }, progress, changeScreen);
                });
            });
        }

        private void progress(int cur, int total)
        {
            string concat = "";
            
            if (Game.FunkinConfig.Get<bool>(FunkinSetting.ShowPercentageOnBootup))
                concat = $"{float.Round(((float)cur / total) * 100, 0)}%"; // I have to cast the cur number to a float to be able to get the percentage
            else
                concat = $"({cur}/{total})";

            Schedule(() =>
            {
                curLoad.Text = $"Currently loading {lastFormat} charts... {concat}";
            });
        }

        private void changeScreen()
        {
            Schedule(() =>
            {
                curLoad.Text = $"Finished loading!";

                bg.FadeOut(1500D, Easing.OutQuint).OnComplete((_) =>
                {
                    Game.ScreenStack.Push(new ChartFormatSelect());
                });
            });
        }
    }
}
