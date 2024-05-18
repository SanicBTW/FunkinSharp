using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;
using FunkinSharp.Game.Core.Sprites;
using osu.Framework.Graphics.Sprites;

namespace FunkinSharp.Game.Core.Containers
{
    // https://github.com/ppy/osu-framework/blob/master/osu.Framework/Graphics/Visualisation/ToolWindow.cs
    public abstract partial class UIWindow : OverlayContainer
    {
        public const float WIDTH = 500;
        public const float HEIGHT = 600;

        private const float button_width = 140;
        private const float button_height = 40;

        protected readonly FillFlowContainer ToolbarContent;

        protected readonly FillFlowContainer MainHorizontalContent;

        protected ScrollContainer<Drawable> ScrollContent;

        protected readonly SearchContainer SearchContainer;

        protected UIWindow(string title, string keyHelpText, IconUsage icon, bool supportsSearch = false)
        {
            AutoSizeAxes = Axes.X;
            Height = HEIGHT;
            CornerRadius = 15;

            Masking = true; // for cursor masking

            BasicTextBox queryTextBox;

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    Colour = Colour4.SteelBlue.Darken(2f),
                    RelativeSizeAxes = Axes.Both,
                    Depth = 0
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Distributed),
                    },
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new TitleBar(title, keyHelpText, this, icon),
                                    new Container //toolbar
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                Colour = Colour4.SteelBlue,
                                                RelativeSizeAxes = Axes.Both,
                                            },
                                            ToolbarContent = new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Spacing = new Vector2(5),
                                                Padding = new MarginPadding(5),
                                            },
                                        },
                                    }
                                }
                            },
                        },
                        new Drawable[]
                        {
                            new TooltipContainer
                            {
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                                Child = MainHorizontalContent = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Direction = FillDirection.Horizontal,
                                    Child = new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        Width = WIDTH,
                                        RowDimensions = new[]
                                        {
                                            new Dimension(GridSizeMode.AutoSize),
                                            new Dimension(GridSizeMode.Distributed)
                                        },
                                        Content = new[]
                                        {
                                            new Drawable[]
                                            {
                                                queryTextBox = new BasicTextBox
                                                {
                                                    Width = WIDTH,
                                                    Height = 30,
                                                    PlaceholderText = "Search...",
                                                    Alpha = supportsSearch ? 1 : 0,
                                                    Colour = Colour4.LightSteelBlue
                                                }
                                            },
                                            new Drawable[]
                                            {
                                                ScrollContent = new BasicScrollContainer<Drawable>
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Child = SearchContainer = new SearchContainer
                                                    {
                                                        AutoSizeAxes = Axes.Y,
                                                        RelativeSizeAxes = Axes.X,
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                        }
                    },
                },
            });

            queryTextBox.Current.BindValueChanged(term => SearchContainer.SearchTerm = term.NewValue, true);
        }

        protected void AddButton(string text, Action action)
        {
            ToolbarContent.Add(new BasicButton
            {
                Size = new Vector2(button_width, button_height),
                Text = text,
                Action = action,
                Colour = Colour4.LightSteelBlue,
            });
        }

        protected override void PopIn() => this.FadeIn(100);

        protected override void PopOut() => this.FadeOut(100);
    }
}
