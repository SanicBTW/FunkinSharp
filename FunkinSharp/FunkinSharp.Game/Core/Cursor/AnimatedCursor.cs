using FunkinSharp.Game.Core.ReAnimationSystem;
using FunkinSharp.Game.Core.Stores;
using osu.Framework.Allocation;

namespace FunkinSharp.Game.Core.Cursor
{
    // Holds the logic of the cursor animation n shit
    public partial class AnimatedCursor : ReAnimatedSprite
    {
        public AnimatedCursor()
        {
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load(SparrowAtlasStore sparrowStore)
        {
            sparrowStore.GetSparrowNew(this, "Textures/General/Cursors/Cursor");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Play("arrow jiggle");
        }
    }
}
