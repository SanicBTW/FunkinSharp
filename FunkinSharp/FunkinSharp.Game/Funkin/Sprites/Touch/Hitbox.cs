using System;
using System.Collections.Generic;
using FunkinSharp.Game.Core.ReAnimationSystem;
using FunkinSharp.Game.Core.Stores;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using static FunkinSharp.Game.Funkin.Sprites.Touch.Hitbox;

namespace FunkinSharp.Game.Funkin.Sprites.Touch
{
    public partial class Hitbox : FillFlowContainer<HitboxButton>
    {
        private FunkinScreen invoker;

        private static Dictionary<string, Colour4> colors => new()
        {
            { "left", Colour4.FromARGB(0xFFFF00FF) },
            { "down", Colour4.FromARGB(0xFF00FFFF) },
            { "up", Colour4.FromARGB(0xFF00FF00) },
            { "right", Colour4.FromARGB(0xFFFF0000) },
        };

        private ReAnimatedSprite loader = new();

        public Hitbox(FunkinScreen caller)
        {
            invoker = caller;

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
                HitboxButton btn = new HitboxButton(loader, anim.Key, colors[anim.Key]);

                btn.Pressed += () => invoker.RaiseActionPressed(btn.Action);
                btn.Released += () => invoker.RaiseActionReleased(btn.Action);

                Add(btn);
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

            public FunkinAction Action { get; }
            public event Action Pressed;
            public event Action Released;

            public HitboxButton(ReAnimatedSprite loader, string anim, Colour4 colour)
            {
                Name = anim;
                Action = (FunkinAction)Enum.Parse(typeof(FunkinAction), $"NOTE_{anim.ToUpper()}");
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
                Pressed?.Invoke();
            }

            public void SetIdle()
            {
                this.FadeTo(IdleAlpha, IdleTime, Easing.InOutCirc);
                Released?.Invoke();
            }
        }
    }
}
