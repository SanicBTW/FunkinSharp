using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osuTK;
using osu.Framework.Graphics.Shapes;

namespace FunkinSharp.Game.Core.Sprites
{
    // https://github.com/ppy/osu-framework/blob/master/osu.Framework/Graphics/Visualisation/TitleBar.cs
    internal partial class TitleBar : CompositeDrawable
    {
        private readonly Drawable movableTarget;

        public const float HEIGHT = 40;

        public TitleBar(string title, string keyHelpText, Drawable movableTarget, IconUsage icon)
        {
            this.movableTarget = movableTarget;

            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, HEIGHT);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.SteelBlue.Lighten(0.2f),
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Size = new Vector2(20),
                            Margin = new MarginPadding(10) { Right = 0 },
                            Icon = icon,
                        },
                        new SpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = title,
                            Font = FrameworkFont.Condensed.With(weight: "Bold"),
                            Colour = Colour4.Snow,
                        },
                        new SpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = keyHelpText,
                            Font = FrameworkFont.Condensed,
                            Colour = Colour4.Snow,
                            Alpha = 0.5f
                        },
                    }
                },
            };
        }

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override void OnDrag(DragEvent e)
        {
            movableTarget.Position += e.Delta;
            base.OnDrag(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e) => true;
    }
}
