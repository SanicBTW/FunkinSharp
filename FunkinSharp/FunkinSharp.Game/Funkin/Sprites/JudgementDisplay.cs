using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Sparrow;
using FunkinSharp.Game.Core;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics;
using FunkinSharp.Game.Core.Sprites;

namespace FunkinSharp.Game.Funkin.Sprites
{
    // Just like combo num
    // TODO: Add the possibility of adding judgements or sum, gotta see it on modding update lol
    public partial class JudgementDisplay : FrameAnimatedSprite, ICameraComponent
    {
        public osuTK.Vector2 ScrollFactor { get; set; } = osuTK.Vector2.Zero;
        public bool FollowScale { get; set; } = false;

        public JudgementDisplay()
        {
            Loop = true;
            Anchor = Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore store)
        {
            Atlas = new SparrowAtlas("Judgements/");
            string[] judgements = ["sick", "good", "bad", "shit"];
            int i = 0;
            foreach (string judgement in judgements)
            {
                Atlas.FrameNames.Add(judgement);
                Atlas.Frames.Add(store.Get($"{Atlas.TextureName}{judgement}"));
                Atlas.SetFrame(judgement, new AnimationFrame([i], 1, true));
                AddFrame(Atlas.Frames[i], 1);

                i++;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            base.Play("sick"); // so it doesnt play the scale transform
        }

        public void Play(string newJudgement)
        {
            base.Play(newJudgement);
            this.FadeIn().ScaleTo(1).ScaleTo(1.1f, 100D).Delay(Conductor.Instance.BeatLengthMs).ScaleTo(1, 100D).FadeOut(120D);
        }
    }
}
