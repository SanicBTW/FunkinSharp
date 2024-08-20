using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace FunkinSharp.Game.Funkin.Screens
{
    public partial class SongSelector
    {
        private partial class PlayButton : BasicButton
        {
            public PlayButton()
            {
                // Setting the properties here to not clutter the functions
                Text = "PLAY";
                Colour = Colour4.White;
                RelativeSizeAxes = Axes.Y;
                HoverColour = Colour4.Green.Darken(5f);
                BackgroundColour = Colour4.Green;
                DisabledColour = Colour4.DarkRed;
            }

            protected override SpriteText CreateText() => new SpriteText
            {
                Depth = -1,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Font = new FontUsage(family: "RedHatDisplay", size: 32f, weight: "Bold"),
                Colour = Colour4.White
            };
        }
        
    }
}
