using System;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Game.Core.Utils;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace FunkinSharp.Game.Funkin.Notes
{
    // New code adapted from the SustainNode Test Scene, for more and better explanation check that class
    // Also see RawDrawing Test Scene for the in-depth explanation of the draw process
    // Some spritesheets (mostly legacy ones) might present the fading texture issue
    // For anyone wondering, I cannot use the new ReAnimation System because this overrides the draw node and some other custom animation handling
    // I COULD support it someday but it would be too much fuss, including I would need to extend the base tiling or animation draw node and do some other stuff
    // including changing textures every now and then and more framing support etc
    // TODO: Better naming :pray:
    // TODO: Fix downscroll
    public partial class SustainSprite : FrameAnimatedSprite
    {
        private readonly Note head;

        private SpriteInternal textureHolder;

        private BindableBool legacy;
        private BindableBool downscroll;

        public override Drawable CreateContent()
        {
            textureHolder = new(legacy, downscroll, head.NoteData)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            var field = ReflectionUtils.GetField<TextureAnimation>("textureHolder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(this, textureHolder);
            return textureHolder;
        }

        public SustainSprite(Note Head, BindableBool loadLegacy, BindableBool isDownscroll)
        {
            legacy = loadLegacy;
            downscroll = isDownscroll;
            head = Head;
            Anchor = Origin = Anchor.TopCentre;
            Loop = true;
        }

        [BackgroundDependencyLoader]
        private void load(SparrowAtlasStore sparrowStore)
        {
            if (legacy.Value)
            {
                // We expect that the parent note "Head" has existing ReceptorData
                Atlas = sparrowStore.GetSparrow($"NoteTypes/{head.NoteType}/{head.ReceptorData.Texture}");

                string key = $"{head.GetNoteColor()} hold piece";
                if (Animations.TryGetValue(key, out AnimationFrame anim))
                {
                    AddFrameRange(anim.StartFrame, anim.EndFrame);
                    CurAnim = anim;
                    CurAnimName = key;
                }
            }
            else
                AddFrame(Paths.GetTexture($"NoteTypes/{head.NoteType}/NOTE_hold_assets.png", false));

            // note for my dumb self!!
            // instead of trying to apply the scale to the draw node just apply the scale to THIS sprite
            // since it will reflect on the texture holder and the draw node gets the "ScreenSpaceDrawQuad"
            // which its already scaled from this sprite
            float scaleMult = (legacy.Value) ? 1 : 1.15f;
            Scale = new Vector2(head.Scale.X * scaleMult, 1);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (!legacy.Value)
                Width = CurrentFrame.DisplayHeight / 2;
        }

        private partial class SpriteInternal : Sprite
        {
            protected override DrawNode CreateDrawNode() => new SustainDrawNode(this);

            protected BindableBool Legacy;
            protected BindableBool Downscroll;
            protected int NoteData;

            // NoteData is passed through the bound head from this parent
            public SpriteInternal(BindableBool legacy, BindableBool downscroll, int noteData)
            {
                Legacy = legacy;
                Downscroll = downscroll;
                NoteData = noteData;
            }

            private partial class SustainDrawNode : SpriteDrawNode
            {
                protected new SpriteInternal Source => (SpriteInternal)base.Source;

                public RectangleF TextureRegion = new(0, 0, -1, -1);

                public SustainDrawNode(Sprite source) : base(source)
                {
                    if (!Source.Legacy.Value)
                    {
                        TextureRegion.Width = 52;
                        if (Source.NoteData > 0)
                            TextureRegion.X += TextureRegion.Width * Source.NoteData * 2;
                    }
                }

                protected override void Blit(IRenderer renderer)
                {
                    int tileCountY = (int)Math.Ceiling(ScreenSpaceDrawQuad.Height / TextureCoords.Height);
                    Texture.Bind();

                    for (float y = 0; y < tileCountY; y++)
                    {
                        float tilePosY = ScreenSpaceDrawQuad.BottomRight.Y - (y + 1) * TextureCoords.Height;
                        float tileHeight = Math.Min(TextureCoords.Height, tilePosY + TextureCoords.Height - ScreenSpaceDrawQuad.TopLeft.Y);

                        if (tileHeight < TextureCoords.Height)
                            tilePosY = ScreenSpaceDrawQuad.TopLeft.Y;

                        float spaceX = ScreenSpaceDrawQuad.TopLeft.X;

                        Quad tiledQuad = new Quad(
                            new Vector2(spaceX, tilePosY),
                            new Vector2(spaceX + ScreenSpaceDrawQuad.Width, tilePosY),
                            new Vector2(spaceX, tilePosY + tileHeight),
                            new Vector2(spaceX + ScreenSpaceDrawQuad.Width, tilePosY + tileHeight)
                        );

                        RectangleF rect = TextureRegion;
                        if (rect.Width <= -1)
                            rect.Width = (Source.Legacy.Value) ? TextureCoords.Width : Source.DrawWidth;

                        if (rect.Height <= -1)
                            rect.Height = tileHeight;

                        renderer.DrawQuad(Texture, tiledQuad, DrawColourInfo.Colour, textureRect: rect);
                    }
                }
            }
        }
    }
}
