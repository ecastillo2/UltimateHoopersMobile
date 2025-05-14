using Android.App;
using Android.Content.PM;
using Android.OS;

namespace UltimateHoopers
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Initialize the MAUI Android Handlers
            // The ImageRenderer.Init() call was removed as this type no longer exists in .NET MAUI
            // If you need WebP support, it should be handled differently in newer MAUI versions

            // Enable hardware acceleration for video playback
            Window.SetFlags(Android.Views.WindowManagerFlags.HardwareAccelerated,
                            Android.Views.WindowManagerFlags.HardwareAccelerated);
        }
    }
}