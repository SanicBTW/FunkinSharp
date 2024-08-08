using FunkinSharp.Game;
using FunkinSharp.Game.Core;
using osu.Framework.Allocation;

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
        }
    }
}
