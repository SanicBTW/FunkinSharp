using System;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using FunkinSharp.Game;
using osu.Framework.Android;
using Debug = System.Diagnostics.Debug;

namespace FunkinSharp.Android
{
    // https://github.com/ppy/osu/blob/master/osu.Android/OsuGameActivity.cs

    [Activity(ConfigurationChanges = DEFAULT_CONFIG_CHANGES, Exported = true, LaunchMode = DEFAULT_LAUNCH_MODE, MainLauncher = true)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate the compatibility of the platform", Justification = "The API target is >=21")]
    public class MainActivity : AndroidGameActivity
    {
        /// <summary>
        /// The default screen orientation.
        /// </summary>
        /// <remarks>Adjusted on startup to match expected UX for the current device type (phone/tablet).</remarks>
        public ScreenOrientation DefaultOrientation = ScreenOrientation.Unspecified;

        protected override osu.Framework.Game CreateGame() => new FunkinSharpGame();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Debug.Assert(Window != null);

            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            Window.AddFlags(WindowManagerFlags.HardwareAccelerated);

            Debug.Assert(WindowManager?.DefaultDisplay != null);
            Debug.Assert(Resources?.DisplayMetrics != null);

            Point displaySize = new Point();
#pragma warning disable CA1422 // GetSize is deprecated
            WindowManager.DefaultDisplay.GetSize(displaySize);
#pragma warning restore CA1422
            float smallestWidthDp = Math.Min(displaySize.X, displaySize.Y) / Resources.DisplayMetrics.Density;
            bool isTablet = smallestWidthDp >= 600f;

            RequestedOrientation = DefaultOrientation = isTablet ? ScreenOrientation.FullUser : ScreenOrientation.SensorLandscape;
        }
    }
}
