using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Net;
using Android.Graphics;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using System.IO;

namespace UltimateHoopers
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Register WebP handler on Android
            Microsoft.Maui.Controls.Handlers.Compatibility.ImageRenderer.Init();

            // Enable hardware acceleration for video playback
            Window.SetFlags(Android.Views.WindowManagerFlags.HardwareAccelerated,
                            Android.Views.WindowManagerFlags.HardwareAccelerated);
        }
    }
}