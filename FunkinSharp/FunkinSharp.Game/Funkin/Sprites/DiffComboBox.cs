using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;

namespace FunkinSharp.Game.Funkin.Screens
{
    public partial class SongSelector
    {
        // TODO: Add slide in animations / slide out animations for the menu
        // TODO: Add color fade animations for selection
        // TODO: Modify Search Bar colors
        private partial class DiffComboBox : Dropdown<string>
        {
            protected override DropdownMenu CreateMenu() => new DiffComboMenu();
            protected override DropdownHeader CreateHeader() => new DiffComboHeader();

            private Box roundBox;
            private DiffComboHeader header => (DiffComboHeader)Header;

            // these are to properly change the rounded bg color
            // forced values since i dont ever think i will be changing these
            protected override bool OnHover(HoverEvent e)
            {
                roundBox.FadeColour(Colour4.Gray);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                roundBox.FadeColour(cbColor);
                base.OnHoverLost(e);
            }

            public DiffComboBox(Box bg) : base()
            {
                roundBox = bg;
                Menu.StateChanged += menu_StateChanged;
                Current.BindValueChanged((ev) =>
                {
                    header.LabelSpr.Colour = diffColors[ev.NewValue];
                });
            }

            private void menu_StateChanged(MenuState obj)
            {
                header.Indicator.Icon = (obj == MenuState.Open) ? FontAwesome.Regular.CaretSquareUp : FontAwesome.Regular.CaretSquareDown;
            }

            private partial class DiffComboHeader : DropdownHeader
            {
                private static FontUsage font => new FontUsage(family: "RedHatDisplay", size: 32f, weight: "Bold");

                public readonly SpriteText LabelSpr;

                protected override LocalisableString Label
                {
                    get => LabelSpr.Text;
                    set => LabelSpr.Text = value;
                }

                public readonly SpriteIcon Indicator;

                public DiffComboHeader() : base()
                {
                    Foreground.Padding = new MarginPadding(5);
                    BackgroundColour = cbColor;
                    BackgroundColourHover = Colour4.Gray;

                    Children = new Drawable[]
                    {
                        Indicator = new SpriteIcon
                        {
                            Size = new Vector2(32),
                            Icon = FontAwesome.Regular.CaretSquareDown
                        },
                        LabelSpr = new SpriteText
                        {
                            AlwaysPresent = true,
                            Font = font,
                            Height = font.Size,
                            Margin = new MarginPadding() { Left = 40 }
                        },
                    };
                }

                protected override DropdownSearchBar CreateSearchBar() => new DiffSearchBar();

                private partial class DiffSearchBar : DropdownSearchBar
                {
                    protected override void PopIn() => this.FadeIn();

                    protected override void PopOut() => this.FadeOut();

                    protected override TextBox CreateTextBox() => new BasicTextBox
                    {
                        PlaceholderText = "type to search diff",
                        FontSize = font.Size,
                    };
                }
            }

            private partial class DiffComboMenu : DropdownMenu
            {
                protected override Menu CreateSubMenu() => new BasicMenu(Direction.Vertical);

                protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new DrawableBasicDropdownMenuItem(item);

                protected override ScrollContainer<Drawable> CreateScrollContainer(Direction direction) => new BasicScrollContainer(direction);

                private partial class DrawableBasicDropdownMenuItem : DrawableDropdownMenuItem
                {
                    public DrawableBasicDropdownMenuItem(MenuItem item)
                        : base(item)
                    {
                        Foreground.Padding = new MarginPadding(6);
                        BackgroundColour = cbColor;
                        BackgroundColourHover = Colour4.Gray;
                        BackgroundColourSelected = Colour4.DarkGray;

                        // for some reason the item value is not ready before completion
                        OnLoadComplete += delegate
                        {
                            ForegroundColour = ForegroundColourHover = ForegroundColourSelected = diffColors[item.Text.Value.ToString()];
                        };
                    }

                    protected override Drawable CreateContent() => new SpriteText
                    {
                        Font = new FontUsage(family: "RedHatDisplay", size: 32f, weight: "Bold")
                    };
                }
            }
        }
        
    }
}
