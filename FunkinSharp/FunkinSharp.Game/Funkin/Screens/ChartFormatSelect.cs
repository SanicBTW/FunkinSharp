using System;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Funkin.Song;
using FunkinSharp.Game.Funkin.Text;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens;
using osuTK;

namespace FunkinSharp.Game.Funkin.Screens
{
    // TODO: Decouple the selected object tweening into another screen that holds a creation of that object automatically
    public partial class ChartFormatSelect : FunkinScreen
    {
        private Sprite bg;
        private Camera camera = new(false); // World camera
        private Container<MenuAtlasText> grpOptions = new();
        private AtlasText indicator;
        private bool selected = false;
        private double accum = 0;
        private double timerLimit = 100D;
        private Colour4 baseColor = Colour4.Snow;
        private Colour4 flashColor = Colour4.SteelBlue;
        private int limit = 10;
        private int flashed = 0;
        private bool introTween = false;
        private bool canPress = true;
        private bool blocked = false;
        private double textFadeTime = 250D;
        private AtlasText verIndicator = new($"You are running {GameConstants.TITLE} {GameConstants.VERSION}{GameConstants.VER_PREFIX} {GameConstants.TESTING_STATE}")
        {
            Anchor = Anchor.BottomRight,
            Origin = Anchor.BottomRight,
            Margin = new MarginPadding(16),
            Scale = new Vector2(0.5f)
        };

        private static int ccselect = 0;
        private int curSelected
        {
            get => ccselect;
            set
            {
                ccselect += value;

                if (ccselect < 0)
                    ccselect = grpOptions.Count - 1;
                if (ccselect >= grpOptions.Count)
                    ccselect = 0;

                int tf = 0;
                foreach (MenuAtlasText item in grpOptions)
                {
                    item.TargetY = tf - ccselect;
                    tf++;

                    item.Alpha = 0.6f;

                    if (item.TargetY == 0)
                        item.Alpha = 1;
                }
            }
        }

        public ChartFormatSelect(bool fromTween = false)
        {
            OnActionPressed += actionPressed;
            OnActionReleased += actionReleased;
            introTween = fromTween;
            blocked = fromTween;
            canPress = !fromTween;
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            OnActionPressed -= actionPressed;
            OnActionReleased -= actionReleased;
            return base.OnExiting(e);
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore store)
        {
            CursorVisible = false;

            Add(bg = new Sprite()
            {
                Alpha = 0.5f,
                Texture = store.Get("General/BGS/menuDesat"),
                Size = new Vector2(GameConstants.WIDTH, GameConstants.HEIGHT),
                Colour = baseColor
            });

            Add(camera);

            camera.Add(grpOptions);

            camera.Add(indicator = new AtlasText(">", 5, 85)
            {
                Alpha = introTween ? 0 : 1
            });

            camera.Add(verIndicator);
            verIndicator.Alpha = (introTween) ? 0 : 1;
        }

        protected override void LoadComplete()
        {
            regenMenu([.. ChartRegistry.SUPPORTED_FORMATS, "Note Skin Selection", "Settings"]);
            base.LoadComplete();
        }

        private void actionPressed(FunkinAction action)
        {
            if (!canPress)
                return;

            switch (action)
            {
                case FunkinAction.UI_UP:
                    curSelected = -1;
                    break;

                case FunkinAction.UI_DOWN:
                    curSelected = 1;
                    break;

                case FunkinAction.CONFIRM:
                    if (selected)
                        return;

                    indicator.FadeOut(500D, Easing.OutQuint);
                    verIndicator.FadeOut(500D, Easing.OutQuint);
                    bg.Colour = flashColor;
                    selected = true;

                    foreach (MenuAtlasText item in grpOptions)
                    {
                        if (item.TargetY == 0)
                        {
                            item.ChangeY = false;
                            Vector2 endTarget = new Vector2((camera.DrawWidth / 2) - (item.DrawWidth / 2), (camera.DrawHeight / 2) - (item.DrawHeight / 2));
                            item.MoveTo(endTarget, 500D, Easing.InSine);
                        }
                        else
                            item.FadeOut(textFadeTime * Math.Abs(item.TargetY), Easing.OutQuint);
                    }
                    break;

                default:
                    break;
            }
        }

        private void actionReleased(FunkinAction action)
        {
            if (blocked)
                return;

            canPress = true;
        }

        protected override void Update()
        {
            if (selected)
            {
                MenuAtlasText curItem = grpOptions[curSelected];

                if (flashed >= limit)
                {
                    Schedule(() =>
                    {
                        switch (curItem.Text.ToLower())
                        {
                            case "note skin selection":
                                Game.ScreenStack.Push(new NoteSkinSelect());
                                break;

                            case "settings":
                                Game.ScreenStack.Push(new SettingsScreen());
                                break;

                            default:
                                Game.ScreenStack.Push(new SongSelector(GetTransObjContent(curItem.Text.ToLower()), curItem.Text));
                                break;
                        }
                    });
                    
                    selected = false;
                }

                if (accum > timerLimit && flashed < limit)
                {
                    if (bg.Colour == baseColor)
                        bg.FadeColour(flashColor, timerLimit, Easing.InQuint);
                    else
                        bg.FadeColour(baseColor, timerLimit, Easing.InQuint);

                    if (curItem.Alpha == 1)
                        curItem.FadeOut(timerLimit, Easing.InQuint);
                    else
                        curItem.FadeIn(timerLimit, Easing.InQuint);

                    accum = 0;
                    flashed++;
                }
                else
                    accum += Clock.ElapsedFrameTime;
            }

            base.Update();
        }

        private void regenMenu(string[] array)
        {
            grpOptions.Clear();

            int i = 0;

            foreach (string str in array)
            {
                MenuAtlasText item = new MenuAtlasText(50, 80, str, true)
                {
                    ChangeX = false,
                    X = 50,
                    TargetY = i - curSelected,
                    ID = i
                };

                if (introTween)
                {
                    item.OnLoadComplete += delegate (Drawable obj)
                    {
                        if (item.ID == ccselect)
                        {
                            indicator.FadeIn(500D, Easing.InQuint);
                            verIndicator.FadeIn(500D, Easing.InQuint);
                            item.ChangeY = false;
                            item.Alpha = 1;
                            item.Position = new Vector2((camera.DrawWidth / 2) - (obj.DrawWidth / 2), (camera.DrawHeight / 2) - (obj.DrawHeight / 2));
                            item.MoveTo(new Vector2(item.StartPosition.X, item.GetYPos()), 500D, Easing.OutSine).OnComplete((_) =>
                            {
                                blocked = false;
                                canPress = true;
                                item.ChangeY = true;
                            });
                        }
                        else
                        {
                            item.Alpha = 0;
                            item.FadeIn(textFadeTime * Math.Abs(item.TargetY), Easing.InQuint);
                        }
                    };
                }

                grpOptions.Add(item);
                i++;
            }

            curSelected = (introTween) ? 0 : grpOptions.Count + 1;
        }

        public static string GetTransObjContent(string format)
        {
            switch (format)
            {
                case "fnf vslice":
                    return "VSlice Charts";

                case "fnf legacy":
                    return "Legacy Charts";

                case "quaver":
                    return "Quaver Maps";

                case "osu!":
                    return "OSU! Beatmaps";
            }

            return "";
        }
    }
}
