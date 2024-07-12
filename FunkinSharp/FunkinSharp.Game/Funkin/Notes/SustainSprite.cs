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
    // TODO: Fix window resizing affecting the sustain width and with that the fill ratio too
    public partial class SustainSprite : FrameAnimatedSprite
    {
        protected readonly Note Head;

        private SpriteInternal textureHolder;

        protected BindableBool Legacy;

        public new float Rotation { get => textureHolder.Rotation; set => textureHolder.Rotation = value; }

        public override Drawable CreateContent()
        {
            textureHolder = new(Legacy, Head.NoteData)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            var field = ReflectionUtils.GetField<TextureAnimation>("textureHolder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(this, textureHolder);
            return textureHolder;
        }

        public SustainSprite(Note head, BindableBool loadLegacy)
        {
            Legacy = loadLegacy;
            Head = head;
            Anchor = Origin = Anchor.TopCentre;
            Loop = true;
            Depth = -1; // in front of the sustain end
        }

        [BackgroundDependencyLoader]
        private void load(SparrowAtlasStore sparrowStore)
        {
            if (Head.NoteType == null)
                return;

            if (Legacy.Value)
            {
                // We expect that the parent note "Head" has existing ReceptorData
                Atlas = sparrowStore.GetSparrow($"NoteTypes/{Head.NoteType}/{Head.ReceptorData.Texture}");

                string key = $"{Head.GetNoteColor()} hold piece";
                if (Animations.TryGetValue(key, out AnimationFrame anim))
                {
                    AddFrameRange(anim.StartFrame, anim.EndFrame);
                    CurAnim = anim;
                    CurAnimName = key;
                }
            }
            else
            {
                AddFrame(Paths.GetTexture($"NoteTypes/{Head.NoteType}/NOTE_hold_assets.png", false));
                Width = CurrentFrame.DisplayHeight / 2;
            }

            // note for my dumb self!!
            // instead of trying to apply the scale to the draw node just apply the scale to THIS sprite
            // since it will reflect on the texture holder and the draw node gets the "ScreenSpaceDrawQuad"
            // which its already scaled from this sprite
            float scaleMult = (Legacy.Value) ? 1 : 1.15f;
            Scale = new Vector2(Head.Scale.X * scaleMult, 1);
        }

        private partial class SpriteInternal : Sprite
        {
            protected override DrawNode CreateDrawNode() => new SustainDrawNode(this);

            protected BindableBool Legacy;
            protected int NoteData;

            // NoteData is passed through the bound head from this parent
            public SpriteInternal(BindableBool legacy, int noteData)
            {
                Legacy = legacy;
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
                    else
                    {
                        // an attempt to fix texture fading on legacy sheets lol
                        var aspectRatio = Source.Texture.DisplayWidth / Source.Texture.DisplayHeight;
                        TextureRegion.Y = aspectRatio;
                        TextureRegion.Height = Source.Texture.DisplayHeight - (aspectRatio * 2);
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

                        Quad tiledQuad = new Quad(
                            rotatePoint(new Vector2(ScreenSpaceDrawQuad.TopLeft.X, tilePosY)),
                            rotatePoint(new Vector2(ScreenSpaceDrawQuad.TopLeft.X + ScreenSpaceDrawQuad.Width, tilePosY)),
                            rotatePoint(new Vector2(ScreenSpaceDrawQuad.TopLeft.X, tilePosY + tileHeight)),
                            rotatePoint(new Vector2(ScreenSpaceDrawQuad.TopLeft.X + ScreenSpaceDrawQuad.Width, tilePosY + tileHeight))
                        );

                        RectangleF rect = TextureRegion;
                        if (rect.Width <= -1)
                            rect.Width = (Source.Legacy.Value) ? TextureCoords.Width : Source.DrawWidth;

                        if (rect.Height <= -1)
                            rect.Height = tileHeight;

                        renderer.DrawQuad(Texture, tiledQuad, DrawColourInfo.Colour, textureRect: rect);
                    }
                }

                // I should probably improve this but whatever
                private Vector2 rotatePoint(Vector2 point)
                {
                    float rotation = MathHelper.DegreesToRadians(Source.Rotation);
                    Vector2 relativePosition = point - ScreenSpaceDrawQuad.Centre;
                    return ScreenSpaceDrawQuad.Centre
                        + new Vector2(
                            relativePosition.X * (float)Math.Cos(rotation) - relativePosition.Y * (float)Math.Sin(rotation),
                            relativePosition.X * (float)Math.Sin(rotation) + relativePosition.Y * (float)Math.Cos(rotation)
                        );
                }
            }
        }
    }
}
