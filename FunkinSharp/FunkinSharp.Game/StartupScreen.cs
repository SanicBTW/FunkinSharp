using System.Collections.Generic;
using System.IO;
using System.Linq;
using FunkinSharp.API.Animation;
using FunkinSharp.API.Input;
using FunkinSharp.API.Screens.Navigation;
using FunkinSharp.API.Sparrow;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
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
        private Container<SparrowAnimation> sparrows;
        private string[] fix = ["idle", "singLEFT", "singDOWN", "singUP", "singRIGHT"];


        [BackgroundDependencyLoader]
        private void load(FunkinSharpGame game, TextureStore largeTextureStore)
        {
            // populate this through other way, maybe the resource pack system from livin' on sweets???
            var sheet = largeTextureStore.Get("Characters/BOYFRIEND.png");
            var str = game.Resources.GetStream("Textures/Characters/BOYFRIEND.xml");

            var bf = loadBf(sheet, str);

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
                sparrows = new Container<SparrowAnimation>()
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = [bf]
                }
            };
        }

        public bool OnPressed(KeyBindingPressEvent<FunkinAction> e)
        {
            switch (e.Action)
            {
                /*
                case FunkinAction.Back:
                    animation.Play(fix[0]);
                    return true;

                case FunkinAction.NoteLeft:
                case FunkinAction.NoteDown:
                case FunkinAction.NoteUp:
                case FunkinAction.NoteRight:
                    var idx = (int)e.Action;
                    var anim = fix[idx + 1];
                    animation.Play(anim);
                    return true;*/

                case FunkinAction.Confirm:
                    Push(new Startup2Screen());
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<FunkinAction> e)
        {
        }

        private SparrowAnimation loadBf(Texture texture, Stream stream)
        {
            var sparrow = SparrowAtlas.Parse("boyfriend", texture, stream, parseSparrowFrames: true)!;
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

            var spAnim = new SparrowAnimation()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
            spAnim.AddFrames(aggr);
            foreach (var entry in anims)
                spAnim.Animations.Add(entry.Key, entry.Value);

            spAnim.Play("idle");
            return spAnim;
        }

        /*
        private SparrowAnimation loadDad(Texture texture, Stream stream)
        {
            var sparrow = SparrowAtlas.Parse("dad", texture, stream, parseSparrowFrames: true)!;
            List<SparrowFrame> aggr = [];

        }*/
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
