//BizQuiz App 2019

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using appFBLA2019.Droid;
using Plugin.CurrentActivity;
using Plugin.FacebookClient;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.IO.Compression;

[assembly: Dependency(typeof(MainActivity))]

namespace appFBLA2019.Droid
{
    /// <summary>
    /// The main activity of the app
    /// </summary>
    [Activity(Label = "appFBLA2019", Icon = "@mipmap/icon", Theme = "@style/MainTheme", 
        MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, 
        WindowSoftInputMode =Android.Views.SoftInput.AdjustResize, ScreenOrientation =ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IGetStorage, IGetImage, IErrorLogger, ICloseApplication
    {
        /// <summary>
        /// the current window of the App
        /// </summary>
        private Android.Views.Window currentWindow;

        /// <summary>
        /// the path where the app keeps internal files
        /// </summary>
        private static string fileDirectory;

        /// <summary>
        /// Called when the app is launched
        /// </summary>
        /// <param name="bundle"></param>
        protected async override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            //Stream input = this.Assets.Open("my_asset.txt");

            CrossCurrentActivity.Current.Init(this, bundle);
            CrossCurrentActivity.Current.Activity = this;
            await CrossPermissions.Current.RequestPermissionsAsync(Plugin.Permissions.Abstractions.Permission.Storage);
            fileDirectory = this.GetExternalFilesDir(null).ToString();

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
        public async override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            PermissionStatus status = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage);
            if (status != PermissionStatus.Granted)
            {
                await this.AlertAsync();
                this.CloseApplication();
            }
            else
            {
                await SetupDefaultQuizzesAsync(GetStorage() + "/dflt");
            }
        }

        /// <summary>
        /// Setup an Android dialog box informing the user that they must grant the app permissions.
        /// </summary>
        /// <returns>A bool on whether or not they have pressed the OK button</returns>
        private Task<bool> AlertAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            using (var dialog = new AlertDialog.Builder(this))
            {
                dialog.SetTitle("Storage Permissions Required");
                dialog.SetMessage("Please enable storage permissions for BizQuiz to function. " +
                        "Storage is needed to save and download levels.");
                dialog.SetNeutralButton("OK", (sender, args) => { tcs.TrySetResult(true); });
                dialog.SetCancelable(false);
                dialog.Show();
            }

            return tcs.Task;
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
        /// Close BizQuiz
        /// </summary>
        public void CloseApplication()
        {
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }

        /// <summary>
        /// Logs an error to the Android system log (and force closes the app)
        /// </summary>
        /// <param name="error"></param>
        public void LogError(string error)
        {
            Android.Util.Log.Error("Crash Report", error);
        }



        /// <summary>
        /// Copies the default levels from the assets to the user folder
        /// </summary>
        /// <param name="userpath"></param>
        /// <returns></returns>
        public async Task SetupDefaultQuizzesAsync(string userpath)
        {
            if (await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage) == PermissionStatus.Granted)
            {

                try
                {
                    using (Stream dbAssetStream = Android.App.Application.Context.Assets.Open("dflt.zip"))
                    {
                        MemoryStream memStream = new MemoryStream();
                        dbAssetStream.CopyTo(memStream);
                        memStream.Position = 0;
                        File.WriteAllBytes(userpath + "dflt.zip", memStream.ToArray());
                        ZipFile.ExtractToDirectory(userpath + "dflt.zip", userpath, true);
                        File.Delete(userpath + "dflt.zip");
                    }
                }
                catch (Exception ex)
                {
                    BugReportHandler.SaveReport(ex, "SetupDefaultLevels");
                }
            }
        }
    }
}