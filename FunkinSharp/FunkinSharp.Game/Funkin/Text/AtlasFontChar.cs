using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Sparrow;
using osu.Framework.Logging;

namespace FunkinSharp.Game.Funkin.Text
{
    // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/ui/AtlasText.hx#L165
    // TODO: Pooling
    public partial class AtlasFontChar : FrameAnimatedSprite
    {
        private string curchar;
        public string Char
        {
            get => curchar;
            set
            {
                if (curchar != value)
                {
                    string prefix = GetAnimPrefix(value);
                    if (FrameCount > 0)
                        ClearFrames(); // clears the previous frames

                    if (Animations.TryGetValue(prefix, out AnimationFrame anim))
                    {
                        AddFrameRange(anim.StartFrame, anim.EndFrame, DEFAULT_FRAME_DURATION * 2);
                        CurAnim = anim;
                        CurAnimName = prefix;
                    }
                    else
                    {
                        Logger.Log($"Couldn't find animation for char \"{value}\"", LoggingTarget.Runtime, LogLevel.Debug);
                    }
                }

                curchar = value;
            }
        }

        public AtlasFontChar(SparrowAtlas atlas, string @char, float x = 0f, float y = 0f)
        {
            Position = new osuTK.Vector2(x, y);
            Atlas = atlas;
            Char = @char;
            Loop = true;
        }

        public string GetAnimPrefix(string @char)
        {
            string prefix;

            switch (@char)
            {
                case "&": prefix = "-andpersand-"; break;
                case "😠": prefix = "-angry faic-"; break; // TODO: Do multi-flag characters work?
                case "'": prefix = "-apostraphie-"; break;
                case "\\": prefix = "-back slash-"; break;
                case ",": prefix = "-comma-"; break;
                case "-": prefix = "-dash-"; break;
                case "↓": prefix = "-down arrow-"; break; // U+2193
                case "”": prefix = "-end quote-"; break; // U+0022
                case "!": prefix = "-exclamation point-"; break; // U+0021
                case "/": prefix = "-forward slash-"; break; // U+002F
                case ">": prefix = "-greater than-"; break; // U+003E
                case "♥": prefix = "-heart-"; break; // U+2665
                case "♡": prefix = "-heart-"; break;
                case "←": prefix = "-left arrow-"; break; // U+2190
                case "<": prefix = "-less than-"; break; // U+003C
                case "*": prefix = "-multiply x-"; break;
                case ".": prefix = "-period-"; break; // U+002E
                case "?": prefix = "-question mark-"; break;
                case "→": prefix = "-right arrow-"; break; // U+2192
                case "“": prefix = "-start quote-"; break;
                case "↑": prefix = "-up arrow-"; break; // U+2191
                default: prefix = @char; break; // Default to getting the character itself.
            }

            return prefix;
        }
    }
}
