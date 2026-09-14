using System.Collections.Generic;
using System.Linq;
using FunkinSharp.API.Animation;
using FunkinSharp.API.Input;
using FunkinSharp.API.Screens.Navigation;
using FunkinSharp.API.Sparrow;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.IO.Stores;
using osuTK.Graphics;

namespace FunkinSharp.Game
{
    public partial class StartupScreen : FunkinScreen, IKeyBindingHandler<FunkinAction>
    {
        private SparrowAnimation animation;
        private string[] fix = ["idle", "singLEFT", "singDOWN", "singUP", "singRIGHT"];


        [BackgroundDependencyLoader]
        private void load(FunkinSharpGame game, TextureStore largeTextureStore)
        {
            // populate this through other way, maybe the resource pack system from livin' on sweets???
            var sheet = largeTextureStore.Get("Characters/BOYFRIEND.png");
            var str = game.Resources.GetStream("Textures/Characters/BOYFRIEND.xml");

            var sparrow = SparrowAtlas.Parse("boyfriend", sheet, str, parseSparrowFrames: true)!;
            List<SparrowFrame> aggr = [];
            Dictionary<string, SparrowAnimationData> anims = [];
            string[] list = ["BF idle dance", "BF NOTE LEFT", "BF NOTE DOWN", "BF NOTE UP", "BF NOTE RIGHT"];
            for (int i = 0; i < list.Length; i++)
            {
                var id = list[i];
                var animn = fix[i];

                var cur = aggr.Count;
                var frames = sparrow.AdvFrames[id];
                aggr.AddRange(frames);
                var last = aggr.Count;

                var animData = new SparrowAnimationData()
                {
                    Name = id,
                    Frames = Enumerable.Range(cur, last - cur).ToArray(),
                    FrameRate = 24,
                };
                anims[animn] = animData;
            }

            animation = new SparrowAnimation(aggr)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
            foreach (var entry in anims)
                animation.Animations.Add(entry.Key, entry.Value);

            animation.Play("idle");

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Violet,
                    RelativeSizeAxes = Axes.Both,
                },
                new SpriteText
                {
                    Y = 20,
                    Text = "Main Screen",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FontUsage.Default.With(size: 40)
                },
                animation
            };
        }

        public bool OnPressed(KeyBindingPressEvent<FunkinAction> e)
        {
            switch (e.Action)
            {
                case FunkinAction.NoteLeft:
                case FunkinAction.NoteDown:
                case FunkinAction.NoteUp:
                case FunkinAction.NoteRight:
                    var idx = (int)e.Action;
                    var anim = fix[idx];
                    animation.Play(anim);
                    return true;

                case FunkinAction.Confirm:
                    Push(new Startup2Screen());
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<FunkinAction> e)
        {
        }
    }

    public partial class Startup2Screen : FunkinScreen, IKeyBindingHandler<FunkinAction>
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren =
            [
                new Box
                {
                    Colour = Color4.MidnightBlue,
                    RelativeSizeAxes = Axes.Both,
                },
                new SpriteText
                {
                    Y = 20,
                    Text = "Cock Screen",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FontUsage.Default.With(size: 40)
                }
            ];
        }

        public bool OnPressed(KeyBindingPressEvent<FunkinAction> e)
        {
            if (e.Action == FunkinAction.Back)
            {
                Exit();
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<FunkinAction> e)
        {
        }
    }
}
