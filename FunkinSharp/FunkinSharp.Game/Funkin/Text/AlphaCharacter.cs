using System.Collections.Generic;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Stores;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osuTK;

namespace FunkinSharp.Game.Funkin.Text
{
    // Base Game 0.2.7.1 mixed with some Psych code
    // Might rewrite it a little bit
    // Unfinished
    public partial class AlphaCharacter : FrameAnimatedSprite
    {
        // Only contains the necessary definitions
        public static readonly Dictionary<string, LetterDefinition> LETTERS = new()
        {
            // Symbols
            { "&", new LetterDefinition("amp", Vector2.Zero, new Vector2(0, 2)) },
            { "(", new LetterDefinition(string.Empty, Vector2.Zero, new Vector2(0, 5)) },
            { ")", new LetterDefinition(string.Empty, Vector2.Zero, new Vector2(0, 5)) },
            { "*", new LetterDefinition(string.Empty, new Vector2(0, 28), Vector2.Zero) },

            { "+", new LetterDefinition(string.Empty, new Vector2(0, 7), new Vector2(0, -12)) },
            { "-", new LetterDefinition(string.Empty, new Vector2(0, 16), new Vector2(0, -30)) },
            { "<", new LetterDefinition("lt", Vector2.Zero, new Vector2(0, 4)) },
            { ">", new LetterDefinition("gt", Vector2.Zero, new Vector2(0, 4)) },

            { "'", new LetterDefinition("apostrophe", new Vector2(0, 32), Vector2.Zero) },
            { "\"", new LetterDefinition("quote", new Vector2(0, 32), Vector2.Zero) },
            { "!", new LetterDefinition("exclamation", Vector2.Zero, new Vector2(0, 10)) },
            { "?", new LetterDefinition("question", Vector2.Zero, new Vector2(0, 4)) },

            { ".", new LetterDefinition("period", Vector2.Zero, new Vector2(0, -44)) },
            { "❝", new LetterDefinition("start quote", new Vector2(0, 24), new Vector2(0, -5)) },
            { "❞", new LetterDefinition("end quote", new Vector2(0, 24), new Vector2(0, -5)) },

            // No bold
            { "#", new LetterDefinition("hashtag", Vector2.Zero, Vector2.Zero, false) },
            { "$", new LetterDefinition("dollarsign", Vector2.Zero, Vector2.Zero, false) },

            { ":", new LetterDefinition(string.Empty, new Vector2(0, 2), Vector2.Zero, false) },
            { ";", new LetterDefinition(string.Empty, new Vector2(0, -2), Vector2.Zero, false) },
            { "]", new LetterDefinition(string.Empty, new Vector2(0, -1), Vector2.Zero, false) },
            { "^", new LetterDefinition(string.Empty, new Vector2(0, 28), Vector2.Zero, false) },

            { ",", new LetterDefinition("comma", new Vector2(0, -6), Vector2.Zero, false) },
            { "\\", new LetterDefinition("back slash", Vector2.Zero, Vector2.Zero, false) },
            { "/", new LetterDefinition("forward slash", Vector2.Zero, Vector2.Zero, false) },
            { "~", new LetterDefinition(string.Empty, new Vector2(0, 16), Vector2.Zero, false) },

        };

        public const double FRAME_DURATION = 48; // Increase the duration of the frames so it looks better?

        public readonly string Letter = "?";
        public bool IsBold { get; private set; } = false;

        public Vector2 LetterOffset { get; private set; } = Vector2.Zero;

        // We setting deez stuff on constructor since we need em and cannot wait for baddie to loadhehe
        private LetterDefinition def = LETTERS["?"];
        private string suffix;

        // We set the letter info on creation since the animation is setup on Load which is async and trying to run "CreateBold" results on nothing since the sprite didnt load yet
        public AlphaCharacter(Vector2 position, string letter, bool isBold = false)
        {
            Position = position;
            Letter = letter;
            IsBold = isBold;
            Loop = true;

            string lowercased = Letter.ToLower();
            if (LETTERS.ContainsKey(lowercased))
                def = LETTERS[lowercased];

            if (IsBold)
            {
                suffix = " bold";
                if (def.BoldOffset != Vector2.Zero)
                    LetterOffset = def.BoldOffset;
            }
            else
            {
                if (isTypeAlphabet(lowercased[0]))
                    suffix = (lowercased != Letter) ? " uppercase" : " lowercase";
                else
                {
                    suffix = " normal";
                    if (def.Offset != Vector2.Zero)
                        LetterOffset = def.Offset;
                }
            }

            if (IsBold)
            {
                Origin = osu.Framework.Graphics.Anchor.Centre;
            }
            else
            {
                Origin = osu.Framework.Graphics.Anchor.BottomCentre;
            }
        }

        // Somehow it works but I think it's not gonna last long lmao
        [BackgroundDependencyLoader]
        private void load(SparrowAtlasStore sparrowStore)
        {
            Atlas = sparrowStore.GetSparrow("Textures/alphabet");

            string animation = Letter.ToLower();
            if (!Equals(def, LETTERS["?"]) && def.Animation != string.Empty)
                animation = def.Animation;

            if (suffix == " bold" && !def.HasBold)
            {
                suffix = " normal";
                IsBold = false;
            }

            string key = $"{animation}{suffix}";
            if (Animations.TryGetValue(key, out AnimationFrame anim))
            {
                AddFrameRange(anim.StartFrame, anim.EndFrame, FRAME_DURATION);
                CurAnim = anim;
                CurAnimName = key;
            }
            else
            {
                if (suffix != " bold")
                    suffix = " normal";

                key = $"question{suffix}";

                AnimationFrame qanim = Animations[key];
                AddFrameRange(qanim.StartFrame, qanim.EndFrame, FRAME_DURATION);
                CurAnim = qanim;
                CurAnimName = key;

                Logger.Log($"Missing Letter ({Letter}/{animation}){suffix} Animation", level: LogLevel.Error);
            }
        }

        private bool isTypeAlphabet(char c)
        {
            return (c >= 65 && c <= 90) || (c >= 97 && c <= 122);
        }
    }
}
