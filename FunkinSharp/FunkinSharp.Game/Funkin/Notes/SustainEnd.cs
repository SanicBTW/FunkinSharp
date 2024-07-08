using System;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Stores;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;

namespace FunkinSharp.Game.Funkin.Notes
{
    // For anyone wondering, I cannot use the new ReAnimation System, might as well implement it once SustainSprite supports it too
    // or once everything else supports it
    public partial class SustainEnd : FrameAnimatedSprite
    {
        protected readonly Note Head;
        protected readonly BindableBool Legacy;
        public SustainEnd(Note head, BindableBool loadLegacy)
        {
            Head = head;
            Legacy = loadLegacy;
            Anchor = Origin = Anchor.BottomCentre;
            Loop = true;
            RelativeSizeAxes = Axes.X;
            Depth = 1; // behind the sustain body
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

                // AlphaCharacter stuff, basically add only the frames inside the range
                string key = $"{Head.GetNoteColor()} hold end";
                if (Animations.TryGetValue(key, out AnimationFrame anim))
                {
                    AddFrameRange(anim.StartFrame, anim.EndFrame);
                    CurAnim = anim;
                    CurAnimName = key;
                }
            }
            else
            {
                Texture sustainSheet = Paths.GetTexture($"NoteTypes/{Head.NoteType}/NOTE_hold_assets.png", false);
                AddFrame(sustainSheet.Crop(GetCropRect()), DEFAULT_FRAME_DURATION);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Thanks to swordcube for telling me about mismatching height
            // I think I still probably need to look into this
            float textureWidth = DrawWidth;
            float textureHeight = CurrentFrame.DisplayHeight;

            Height = (float)Math.Ceiling(textureHeight / 2);
            Margin = new MarginPadding() { Top = (textureWidth / Height) };
        }

        // function made to return the sustain end rect for the new sustain sheet
        protected RectangleF GetCropRect()
        {
            // manual stuff, mostly coming from SustainDrawNode
            float bodyWidth = 52;
            float endHeight = 65;
            float cropX = bodyWidth;
            if (Head.NoteData > 0)
                cropX += bodyWidth * Head.NoteData * 2;

            return new RectangleF(cropX, 0, bodyWidth, endHeight);
        }
    }
}
