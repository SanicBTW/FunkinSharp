using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Stores;
using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace FunkinSharp.Game.Funkin.Notes
{
    // The sprite logic is separate from the clipping logic for a couple of reasons
    // Just like ScrollNote & AlphaCharacter, it only adds the necessary frames
    // Only displays the hold piece
    public partial class SustainSprite : FrameAnimatedSprite
    {
        private readonly Note head;

        public SustainSprite(Note Head)
        {
            head = Head;
            Anchor = Origin = Anchor.TopCentre;
            Loop = true;
            RelativeSizeAxes = Axes.Both; // Now set to both axes since we are using a buffered container which handles the sizing
        }

        [BackgroundDependencyLoader]
        private void load(SparrowAtlasStore sparrowStore)
        {
            // We expect that the parent note "Head" has existing ReceptorData
            Atlas = sparrowStore.GetSparrow($"NoteTypes/{head.NoteType}/{head.ReceptorData.Texture}");

            // AlphaCharacter stuff, basically add only the frames inside the range
            string key = $"{head.GetNoteColor()} hold piece";
            if (Animations.TryGetValue(key, out AnimationFrame anim))
            {
                AddFrameRange(anim.StartFrame, anim.EndFrame);
                CurAnim = anim;
                CurAnimName = key;
            }
        }
    }
}
