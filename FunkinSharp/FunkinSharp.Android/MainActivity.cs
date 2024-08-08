using System;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using osu.Framework.Android;
using Debug = System.Diagnostics.Debug;

namespace FunkinSharp.Android
{
    // https://github.com/ppy/osu/blob/master/osu.Android/OsuGameActivity.cs

    [Activity(ConfigurationChanges = DEFAULT_CONFIG_CHANGES, Exported = true, LaunchMode = DEFAULT_LAUNCH_MODE, MainLauncher = true)]
    public class MainActivity : AndroidGameActivity
    {
        /// <summary>
        /// The default screen orientation.
        /// </summary>
        /// <remarks>Adjusted on startup to match expected UX for the current device type (phone/tablet).</remarks>
        public ScreenOrientation DefaultOrientation = ScreenOrientation.Landscape;

        protected override osu.Framework.Game CreateGame() => new GameAndroid(this);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Debug.Assert(Window != null);

            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            Debug.Assert(WindowManager?.DefaultDisplay != null);
            Debug.Assert(Resources?.DisplayMetrics != null);

            Point displaySize = new Point();
#pragma warning disable CA1422 // GetSize is deprecated
            WindowManager.DefaultDisplay.GetSize(displaySize);
#pragma warning restore CA1422
            float smallestWidthDp = Math.Min(displaySize.X, displaySize.Y) / Resources.DisplayMetrics.Density;
            bool isTablet = smallestWidthDp >= 600f;

            RequestedOrientation = DefaultOrientation = isTablet ? ScreenOrientation.FullUser : ScreenOrientation.SensorLandscape;

#pragma warning disable CA1416 // Validate the compatibility of the platform
            RequestPermissions(["android.permission.WRITE_EXTERNAL_STORAGE", "android.permission.READ_EXTERNAL_STORAGE"], 21404);
#pragma warning restore CA1416 // Validate the compatibility of the platform
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
#pragma warning disable CA1416 // Validate the compatibility of the platform
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
#pragma warning restore CA1416 // Validate the compatibility of the platform

            // dont ask me lol
            // maybe i did this to only support 5 tries? if the user keeps rejecting the perms just cope with it and crash it ig
            if (requestCode >= 21404 && requestCode <= 21409)
            {
                foreach (string permission in permissions)
                {
                    if (grantResults[Array.IndexOf(permissions, permission)] == Permission.Denied)
                    {
#pragma warning disable CA1416 // Validate the compatibility of the platform
                        RequestPermissions([permission], requestCode + 1);
#pragma warning restore CA1416 // Validate the compatibility of the platform
                    }
                }
            }
        }
    }
}
