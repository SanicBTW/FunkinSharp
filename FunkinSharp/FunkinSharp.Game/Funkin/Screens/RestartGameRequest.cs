using System.Diagnostics;
using System.IO;
using FunkinSharp.Game.Core;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace FunkinSharp.Game.Funkin.Screens
{
    public partial class SettingsScreen
    {
        // Jus like PauseMenuContainer
        // TODO: Rounded buttons
        // TODO: Find a way to replace the GameHost on runtime without needing to actually restart the whole application
        private partial class RestartGameRequest : VisibilityContainer
        {
            private FillFlowContainer content = [];
            private double gameCamTime = 250D;
            private double slideInTime = 750D;
            private float defaultMargin = 8;
            private float defaultWidth = 520f;

            private SettingsScreen invoker;

            public RestartGameRequest(SettingsScreen caller, string changedOption)
            {
                invoker = caller;
                Position = new osuTK.Vector2(GameConstants.WIDTH + defaultWidth + (defaultMargin * 2), defaultMargin * 2);

                Add(new Container
                {
                    Masking = true,
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 15,
                    Child = new Box
                    {
                        Size = new osuTK.Vector2(defaultWidth, 190f),
                        Colour = Colour4.SteelBlue
                    }
                });

                Add(content);

                content.Add(new SpriteText
                {
                    Text = $"You've changed the following option: \"{changedOption}\"",
                    Margin = new MarginPadding(defaultMargin),
                    Font = new FontUsage(family: "RedHatDisplay", size: 24, weight: "Bold")
                });

                content.Add(new SpriteText
                {
                    Text = $"and the game needs to restart in order to apply the changes.",
                    Margin = new MarginPadding(defaultMargin),
                    Font = new FontUsage(family: "RedHatDisplay", size: 24)
                });

                content.Add(new SpriteText
                {
                    Text = $"You can always restart later if you do not wish to restart now.",
                    Margin = new MarginPadding(defaultMargin),
                    Font = new FontUsage(family: "RedHatDisplay", size: 24)
                });

                content.Add(new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Margin = new MarginPadding(defaultMargin),
                    Children = new Drawable[]
                    {
                        new BasicButton
                        {
                            Text = "Ok, restart now",
                            Margin = new MarginPadding(defaultMargin / 4),
                            Size = new osuTK.Vector2(defaultWidth / 3, 50f),
                            Action = (() =>
                            {
                                invoker.camera.FadeTo(0f, gameCamTime, Easing.OutQuint).OnComplete((_) =>
                                {
                                    this.MoveToX(GameConstants.WIDTH + (defaultWidth + (defaultMargin * 2)), slideInTime, Easing.OutSine).OnComplete((_) =>
                                    {
                                        invoker.Game.Exit();
                                        Process.Start(Path.Join(Path.GetDirectoryName(invoker.GameHost.FullPath), $"{GameConstants.TITLE}.exe"));
                                    });
                                });
                            })
                        },
                        new BasicButton
                        {
                            Text = "Continue anyway",
                            Margin = new MarginPadding(defaultMargin / 4),
                            Size = new osuTK.Vector2(defaultWidth / 3, 50f),
                            Action = ToggleVisibility
                        }
                    }
                });

                ToggleVisibility();
            }

            protected override void PopIn()
            {
                invoker.camera.FadeTo(0.35f, gameCamTime, Easing.OutQuint);
                this.MoveToX(GameConstants.WIDTH - (defaultWidth + (defaultMargin * 2)), slideInTime, Easing.InSine);
            }

            protected override void PopOut()
            {
                invoker.camera.FadeTo(1f, gameCamTime, Easing.InQuint);
                this.MoveToX(GameConstants.WIDTH + (defaultWidth + (defaultMargin * 2)), slideInTime, Easing.OutSine).OnComplete((_) =>
                {
                    Schedule(() =>
                    {
                        invoker.Game.ScreenStack.Push(new ChartFormatSelect(true));
                    });
                });
            }
        }
    }
}
