using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunkinSharp.Game.Core.ReAnimationSystem;
using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Game.Tests.Visual;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using static FunkinSharp.Game.Tests.Touch_Components.Hitbox.HitboxSprite;

namespace FunkinSharp.Game.Tests.Touch_Components
{
    [TestFixture]
    public partial class Hitbox : FunkinSharpTestScene
    {
        private HitboxSprite hitboxhehe;

        public Hitbox() { }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(hitboxhehe = new()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        // TODO: Make the textures in here, maybe using BufferedContainer with BlurSigma to mask a CircularBlob
        // so it looks like this https://github.com/FunkinDroidTeam/Funkin/blob/cb36d23c5ce303b1e5d6e449d99ea75ad1539065/source/funkin/mobile/ui/FunkinHitbox.hx
        public partial class HitboxSprite : FillFlowContainer<HitboxButton>
        {
            private static Dictionary<string, Colour4> colors => new()
            {
                { "left", Colour4.FromARGB(0xFFFF00FF) },
                { "down", Colour4.FromARGB(0xFF00FFFF) },
                { "up", Colour4.FromARGB(0xFF00FF00) },
                { "right", Colour4.FromARGB(0xFFFF0000) },
            };

            private ReAnimatedSprite loader = new();

            public HitboxSprite()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Direction = FillDirection.Horizontal;
                RelativeSizeAxes = Axes.Both; // Fills up the parent container which most of the times should have a size set
            }

            [BackgroundDependencyLoader]
            private void load(SparrowAtlasStore sparrowStore)
            {
                sparrowStore.GetSparrowNew(loader, "Textures/Touch/hitbox");
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                foreach (var anim in loader.Animations)
                {
                    Add(new HitboxButton(loader, anim.Key, colors[anim.Key]));
                }
            }

            protected override void UpdateAfterAutoSize()
            {
                base.UpdateAfterAutoSize();

                // Made it to always fit THIS container and the amount of animations in the loader
                foreach (HitboxButton child in AliveChildren)
                {
                    child.Size = new osuTK.Vector2(DrawSize.X / loader.Animations.Count, DrawSize.Y);
                }
            }

            public partial class HitboxButton : Sprite
            {
                public float IdleAlpha = 0.1f;
                public float ActiveAlpha = 0.6f;

                public double ActiveTime = 60;
                public double IdleTime = 150;

                public HitboxButton(ReAnimatedSprite loader, string anim, Colour4 colour)
                {
                    Name = anim;
                    Texture = loader.Frames[loader.Animations[anim].Frames[0]].TextureFrame;
                    Colour = colour;
                    Alpha = IdleAlpha;
                }

                // Touch support

                protected override bool OnTouchDown(TouchDownEvent e)
                {
                    SetActive();
                    return base.OnTouchDown(e);
                }
                
                protected override void OnTouchUp(TouchUpEvent e)
                {
                    SetIdle();
                    base.OnTouchUp(e);
                }

                // Mouse support (only for desktop testing)

                protected override bool OnMouseDown(MouseDownEvent e)
                {
                    SetActive();
                    return base.OnMouseDown(e);
                }

                protected override void OnMouseUp(MouseUpEvent e)
                {
                    SetIdle();
                    base.OnMouseUp(e);
                }

                // Base functions
                public void SetActive()
                {
                    this.FadeTo(ActiveAlpha, ActiveTime, Easing.InOutCirc);
                }

                public void SetIdle()
                {
                    this.FadeTo(IdleAlpha, IdleTime, Easing.InOutCirc);
                }
            }
        }
    }
}
