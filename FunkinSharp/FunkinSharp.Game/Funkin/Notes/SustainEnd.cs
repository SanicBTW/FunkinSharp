using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Stores;
using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace FunkinSharp.Game.Funkin.Notes
{
    // This is just a copy of SustainSprite but displays the hold end frame
    public partial class SustainEnd : FrameAnimatedSprite
    {
        private readonly Note head;
        public SustainEnd(Note Head)
        {
            head = Head;
            Anchor = Origin = Anchor.BottomCentre;
            Loop = true;
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(SparrowAtlasStore sparrowStore)
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
    }
}
