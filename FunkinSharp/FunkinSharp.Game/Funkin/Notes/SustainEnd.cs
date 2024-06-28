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
        private readonly Note head;
        private readonly BindableBool legacy;
        public SustainEnd(Note Head, BindableBool loadLegacy)
        {
            head = Head;
            legacy = loadLegacy;
            Anchor = Origin = Anchor.BottomCentre;
            Loop = true;
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(SparrowAtlasStore sparrowStore)
        {
            if (legacy.Value)
            {
                // We expect that the parent note "Head" has existing ReceptorData
                Atlas = sparrowStore.GetSparrow($"NoteTypes/{head.NoteType}/{head.ReceptorData.Texture}");

                // AlphaCharacter stuff, basically add only the frames inside the range
                string key = $"{head.GetNoteColor()} hold end";
                if (Animations.TryGetValue(key, out AnimationFrame anim))
                {
                    AddFrameRange(anim.StartFrame, anim.EndFrame);
                    CurAnim = anim;
                    CurAnimName = key;
                }
            }
            else
            {
                // manual stuff, mostly coming from SustainDrawNode
                float bodyWidth = 52;
                float endHeight = 65;
                float cropX = bodyWidth;
                if (head.NoteData > 0)
                    cropX += bodyWidth * head.NoteData * 2;

                RectangleF cropRect = new RectangleF(cropX, 0, bodyWidth, endHeight);

                Texture sustainSheet = Paths.GetTexture($"NoteTypes/{head.NoteType}/NOTE_hold_assets.png", false);
                AddFrame(sustainSheet.Crop(cropRect), DEFAULT_FRAME_DURATION);
            }

            // Thanks to swordcube for telling me about mismatching height
            // I think I still probably need to look into this
            Height = (float)Math.Floor(CurrentFrame.DisplayHeight / 2);
            Margin = new MarginPadding() { Bottom = DrawWidth / Height };
        }
    }
}
