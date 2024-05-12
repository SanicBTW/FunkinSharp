using FunkinSharp.Game.Funkin.Text;
using NUnit.Framework;
using osu.Framework.Graphics.UserInterface;

namespace FunkinSharp.Game.Tests.Visual
{
    // Unfinished
    [TestFixture]
    public partial class TestSceneAtlasText : FunkinSharpTestScene
    {
        private AtlasText defaultFont;
        private AtlasText boldFont;
        private AtlasText fpFont;

        private BasicTextBox textBox;

        public TestSceneAtlasText()
        {
            AtlasText infoText = new AtlasText("Type here to modify\nthe current font:", fontName: AtlasFontType.BOLD);
            infoText.Anchor = infoText.Origin = osu.Framework.Graphics.Anchor.TopLeft;
            infoText.Scale = new osuTK.Vector2(0.6f);
            infoText.Margin = new osu.Framework.Graphics.MarginPadding() { Left = 50, Top = 50 };
            Add(infoText);

            textBox = new BasicTextBox();
            textBox.Anchor = textBox.Origin = osu.Framework.Graphics.Anchor.TopCentre;
            textBox.Text = "Hello World!";
            textBox.Size = new osuTK.Vector2(220, 42);
            textBox.Position = new osuTK.Vector2(140, 85);
            textBox.CommitOnFocusLost = true;
            textBox.OnCommit += (s, n) =>
            {
                if (n)
                {
                    if (defaultFont.IsPresent || boldFont.IsPresent)
                    {
                        if (defaultFont.IsPresent)
                            defaultFont.Text = s.Text;
                        else
                            boldFont.Text = s.Text;
                    }
                    else if (fpFont.IsPresent)
                    {
                        if (AtlasFontData.DigitsOnly.IsMatch(s.Text))
                            fpFont.Text = s.Text;
                    }
                }
            };
            Add(textBox);

            // why is bro using NEGATIVE margin :sob:
            defaultFont = new AtlasText(textBox.Text);
            defaultFont.Anchor = defaultFont.Origin = osu.Framework.Graphics.Anchor.CentreLeft;
            defaultFont.Margin = new osu.Framework.Graphics.MarginPadding() { Left = 25, Top = -380 };
            Add(defaultFont);

            boldFont = new AtlasText(textBox.Text, fontName: AtlasFontType.BOLD);
            boldFont.Anchor = boldFont.Origin = osu.Framework.Graphics.Anchor.CentreLeft;
            boldFont.Margin = new osu.Framework.Graphics.MarginPadding() { Left = 25, Top = -380 };
            Add(boldFont);

            fpFont = new AtlasText("1", fontName: AtlasFontType.FREEPLAY_CLEAR);
            fpFont.Anchor = fpFont.Origin = osu.Framework.Graphics.Anchor.CentreLeft;
            fpFont.Margin = new osu.Framework.Graphics.MarginPadding() { Left = 25, Top = -380 };
            Add(fpFont);

            AddStep("Set Default Font", () =>
            {
                textBox.Text = "Hello World!";
                defaultFont.Show();
                boldFont.Hide();
                fpFont.Hide();
            });

            AddStep("Set Bold Font", () =>
            {
                textBox.Text = "Hello World!";
                defaultFont.Hide();
                boldFont.Show();
                fpFont.Hide();
            });

            AddStep("Set Freeplay Font", () =>
            {
                textBox.Text = "1";
                defaultFont.Hide();
                boldFont.Hide();
                fpFont.Show();
            });
        }
    }
}
