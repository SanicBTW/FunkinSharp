﻿using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace FunkinSharp.Game.Core.Containers
{
    // Basic container made for clipping the children inside of it
    // It's recommended that the sprites that will be added doesn't have the relative size axes set
    // Since it will fit the whole container

    // TODO: Debug overlay??
    public partial class ClippedContainer : Container
    {
        protected override Container<Drawable> Content => ClippedContent;

        protected readonly Container ClippedContent;

        // This sets the masking ON ClippedContent Container, not THIS Container
        public new bool Masking
        {
            get => Content == null || Content.Masking;
            set
            {
                if (Content != null)
                    Content.Masking = value;

                return;
            }
        }

        public ClippedContainer()
        {
            Name = "Clipper Mask";
            Anchor = Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Masking = true;

            AddInternal(ClippedContent = new Container
            {
                Name = "Clipped Content",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true
            });
        }
    }

    // Generic one

    public partial class ClippedContainer<T> : Container<T> where T : Drawable
    {
        protected override Container<T> Content => ClippedContent;

        protected readonly Container<T> ClippedContent;

        // This sets the masking ON ClippedContent Container, not THIS Container
        public new bool Masking
        {
            get => Content == null || Content.Masking;
            set
            {
                if (Content != null)
                    Content.Masking = value;

                return;
            }
        }

        public ClippedContainer()
        {
            Name = "Clipper Mask";
            Anchor = Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Masking = true;

            AddInternal(ClippedContent = new Container<T>
            {
                Name = "Clipped Content",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true
            });
        }
    }
}
