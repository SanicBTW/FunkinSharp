using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Sparrow;
using FunkinSharp.Game.Core.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace FunkinSharp.Game.Funkin.Sprites
{
    public partial class ComboCounter : FillFlowContainer<ComboNum>, ICameraComponent
    {
        public Vector2 ScrollFactor { get; set; } = Vector2.Zero;
        public bool FollowScale { get; set; } = false;

        public readonly BindableInt Current = new();
        public float InitialY = 0;

        public ComboCounter()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Horizontal;

            Current.BindValueChanged((ev) => updateNumbers());

            // always gonna show 3
            Add(new() { Alpha = 0f });
            Add(new() { Alpha = 0f });
            Add(new() { Alpha = 0f });
        }

        private void updateNumbers()
        {
            int combo = Current.Value;
            string concat = "";

            if (combo >= 1000 && Count < 4)
                Add(new() { Alpha = 0f });

            if (combo < 100)
                concat = "0";

            if (combo < 10)
                concat = "00";

            if (combo < 1000 && Count > 3)
                Remove(this[^1], true);

            char[] nums = $"{concat}{combo}".ToString().ToCharArray();

            // sanco here, since this is a fill flow container, when theres no visible children, the container will set its height to 0
            // since its a fill flow container, thus making the next combo update play the animation at Y 0 since the current draw height is 0
            // because theres not any number visible (the numbers are still mid tween!), basically depending on draw height
            // depends on a race condition, so im just setting a fixed value now
            this.MoveToY(InitialY).MoveToY(InitialY + (-10), 120D).Then().Delay(Conductor.Instance.StepLengthMS).MoveToY(InitialY, 120D);

            for (int i = 0; i < Count; i++)
            {
                if (nums.Length > i)
                    this[i].ChangeNum(int.Parse(nums[i].ToString()));
                else
                    this[i].ChangeNum(int.Parse(this[i].CurAnimName));
            }
        }
    }

    public partial class ComboNum : FrameAnimatedSprite
    {
        public ComboNum()
        {
            Loop = true;
            Anchor = Origin = Anchor.Centre;
        }

        // Gotta use TextureStore so it gets added to a backing texture atlas
        [BackgroundDependencyLoader]
        private void load(TextureStore store)
        {
            Atlas = new SparrowAtlas("Combo/num");
            for (int i = 0; i < 10; i++)
            {
                Atlas.FrameNames.Add($"{i}");
                Atlas.Frames.Add(store.Get($"{Atlas.TextureName}{i}"));
                Atlas.SetFrame($"{i}", new AnimationFrame([i], 1, true));
                AddFrame(Atlas.Frames[i], 1);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Play("0");
        }

        public virtual void ChangeNum(int newNum)
        {
            Play($"{newNum}");

            this.FadeIn().Delay(Conductor.Instance.BeatLengthMs).FadeOut(120D);
        }
    }
}
