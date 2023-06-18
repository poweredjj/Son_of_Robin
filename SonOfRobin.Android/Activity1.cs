using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Microsoft.Xna.Framework;
using System.IO;

namespace SonOfRobin.Android
{
    [Activity(
        Label = "SonOfRobin",
        MainLauncher = true,
        Icon = "@drawable/icon",
        Theme = "@style/Theme.Splash",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden

    )]
    public class Activity1 : AndroidGameActivity
    {
        private SonOfRobinGame _game;
        private View _view;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Xamarin.Essentials.Platform.Init(this, bundle);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            { Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges; }

            _game = new SonOfRobinGame();

            SonOfRobinGame.platform = Platform.Mobile;
            SonOfRobinGame.os = OS.Android;
            SonOfRobinGame.fakeMobileMode = false;

            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.WriteExternalStorage }, 1);
            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage }, 1);
            SonOfRobinGame.downloadsPath = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDownloads).AbsolutePath;

            _view = _game.Services.GetService(typeof(View)) as View;

            SetContentView(_view);


            _game.Run();
        }

        protected void OnWindowFocusChanged()
        {
            base.OnWindowFocusChanged(true);
            GoTrueFullScreen();
        }

        protected override void OnResume()
        {
            base.OnResume();
            GoTrueFullScreen();
        }

        private void GoTrueFullScreen()
        {
            _view.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.LayoutStable | SystemUiFlags.LayoutHideNavigation | SystemUiFlags.LayoutFullscreen | SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen | SystemUiFlags.ImmersiveSticky);
        }
    }
}