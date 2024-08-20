using System.Linq;
using FunkinSharp.Game;
using FunkinSharp.Game.Core;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Handlers.Mouse;

namespace FunkinSharp.Android
{
    // https://github.com/ppy/osu/blob/master/osu.Android/OsuGameAndroid.cs
    public partial class GameAndroid : FunkinSharpGame
    {
        [Cached]
        private readonly MainActivity gameActivity;

        public GameAndroid(MainActivity activity) : base()
        {
            gameActivity = activity;
        }

        protected override void LoadComplete()
        {
            GameDependencies.CacheAs(typeof(IStorageService), new AndroidStorageService(gameActivity, Host));

            base.LoadComplete();

            LoadComponentAsync(new GameplayScreenRotationLocker(), Add);
            MouseHandler mouseSupport = Host.AvailableInputHandlers.OfType<MouseHandler>().FirstOrDefault();

            mouseSupport.UseRelativeMode.Value = false;
            mouseSupport.Enabled.Value = false;
        }

        // This makes it so instead of using a Camera as the Game Content it uses another container
        // This is made in order to try to increase performance by avoiding masking the whole game
        public override Container CreateContent() => new DrawSizePreservingFillContainer
        {
            TargetDrawSize = new osuTK.Vector2(GameConstants.WIDTH, GameConstants.HEIGHT)
        };
    }
}
