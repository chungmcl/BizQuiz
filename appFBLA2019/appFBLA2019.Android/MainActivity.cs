//BizQuiz App 2019

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using appFBLA2019.Droid;
using Plugin.FacebookClient;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(MainActivity))]

namespace appFBLA2019.Droid
{
    /// <summary>
    /// The main activity of the app
    /// </summary>
    [Activity(Label = "appFBLA2019", Icon = "@mipmap/icon", Theme = "@style/MainTheme", 
        MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, 
        WindowSoftInputMode =Android.Views.SoftInput.AdjustResize, ScreenOrientation =ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IGetStorage, IGetImage, IErrorLogger
    {
        private Android.Views.Window currentWindow;
        private static string fileDirectory;
        /// <summary>
        /// Called when the app is launched
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            fileDirectory = this.GetExternalFilesDir(null).ToString();
            //Stream input = this.Assets.Open("my_asset.txt");



            FacebookClientManager.Initialize(this);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            this.LoadApplication(new App());
            this.currentWindow = this.Window;
            this.Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);
        }

        /// <summary>
        /// When the activity needs to broadcast an intent to the system
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="resultCode"></param>
        /// <param name="intent"></param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);
            FacebookClientManager.OnActivityResult(requestCode, resultCode, intent);
        }

        /// <summary>
        /// An interface to get the local storage path
        /// </summary>
        /// <returns></returns>
        public string GetStorage()
        {
            return fileDirectory;
        }

        /// <summary>
        /// When the app requests permissions, take action based on the results
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="permissions"></param>
        /// <param name="grantResults"></param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /// <summary>
        /// Load a jpg stream from a given bytearray
        /// </summary>
        /// <param name="image">The byte array to make into a stream</param>
        /// <returns>The properly formatted stream</returns>
        public Stream GetJPGStreamFromByteArray(byte[] image)
        {
            BitmapFactory.Options bitopt = new BitmapFactory.Options
            {
                InMutable = true
            };
            Bitmap resultBitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length, bitopt);

            MemoryStream outStream = new MemoryStream();
            resultBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, outStream);
            return outStream;
        }

        /// <summary>
        /// Logs an error to the Android system log (and force closes the app)
        /// </summary>
        /// <param name="error"></param>
        public void LogError(string error)
        {
            Android.Util.Log.Error("Crash Report", error);
        }
    }
}