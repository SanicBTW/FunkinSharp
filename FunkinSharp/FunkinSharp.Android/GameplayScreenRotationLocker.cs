using Android.Content.PM;
using FunkinSharp.Game.Core.Input;
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // GetContainingInputManager is available after loading

            // Apparently this is an indicator for when the user is playing a song and it should lock the rotation
            localUserPlaying = (GetContainingInputManager() as FunkinInputManager)?.LocalUserPlaying.GetBoundCopy();
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
