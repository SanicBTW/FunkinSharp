using Android.Content.PM;
using FunkinSharp.Game;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace FunkinSharp.Android
{
    // https://github.com/ppy/osu/blob/master/osu.Android/GameplayScreenRotationLocker.cs
    public partial class GameplayScreenRotationLocker : Component
    {
        private Bindable<bool> localUserPlaying = new(false);

        [Resolved]
        private MainActivity gameActivity { get; set; } = null!;

        // ok so for some reason i never cache funkinsharpgame but rather the base implementation, this happens probably since all the important stuff is in there :skull:
        [BackgroundDependencyLoader]
        private void load(FunkinSharpGameBase game)
        {
            // Apparently this is an indicator for when the user is playing a song and it should lock the rotation
            // Will implement it eventually for good measure to avoid fucking up
            localUserPlaying.BindValueChanged(updateLock, true);
        }

        private void updateLock(ValueChangedEvent<bool> userPlaying)
        {
            gameActivity.RunOnUiThread(() =>
            {
                gameActivity.RequestedOrientation = userPlaying.NewValue ? ScreenOrientation.Locked : gameActivity.DefaultOrientation;
            });
        }
    }
}
