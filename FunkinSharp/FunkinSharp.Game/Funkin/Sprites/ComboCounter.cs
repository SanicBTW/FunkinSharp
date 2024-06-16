using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Sparrow;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;

namespace FunkinSharp.Game.Funkin.Sprites
{
    // TODO: Search for a better way to update the numbers
    public partial class ComboCounter : FillFlowContainer<ComboNum>
    {
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
            AddNumber();
            AddNumber();
            AddNumber();
        }

        // fast call for adding a new number when it exceeds the maximum available
        public void AddNumber() => Add(new() { Alpha = 0f });

        private void updateNumbers()
        {
            int combo = Current.Value;
            string concat = "";

            if (combo >= 1000 && Count < 4)
                AddNumber();

            if (combo < 100)
                concat = "0";

            if (combo < 10)
                concat = "00";

            if (combo < 1000 && Count > 3)
                Remove(this[^1], true);

            char[] nums = $"{concat}{combo}".ToString().ToCharArray();

            this.MoveToY(InitialY).MoveToY(InitialY + -(DrawHeight / 4), 150D).Delay(100D).MoveToY(InitialY, 150D);

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

        public void ChangeNum(int newNum)
        {
            Play($"{newNum}");

            // setting it to 0.01 acts like its still visible and doesnt resize the parent container (fill flow container)
            this.FadeIn().Delay(Conductor.Instance.BeatLengthMs).FadeOut(250D);
        }
    }
}
