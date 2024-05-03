using FunkinSharp.Game.Core.Stores;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;

namespace FunkinSharp.Game.Core.Cursor
{
    // Holds the logic of the cursor animation n shit
    public partial class AnimatedCursor : FrameAnimatedSprite
    {
        public AnimatedCursor()
        {
            AlwaysPresent = true;
            IsPlaying = true;
            Loop = true;
        }

        [BackgroundDependencyLoader]
        private void load(SparrowAtlasStore sparrowStore)
        {
            Atlas = sparrowStore.GetSparrow("Textures/General/Cursors/Cursor");
            foreach (Texture frame in Atlas.Frames)
            {
                AddFrame(frame, DEFAULT_FRAME_DURATION);
            }
            Play("arrow jiggle");
        }
    }
}
