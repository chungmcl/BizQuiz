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
using System.IO.Compression;

[assembly: Dependency(typeof(MainActivity))]

namespace appFBLA2019.Droid
{
    /// <summary>
    /// The main activity of the app
    /// </summary>
    [Activity(Label = "appFBLA2019", Icon = "@mipmap/icon", Theme = "@style/MainTheme",
        MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        WindowSoftInputMode = Android.Views.SoftInput.AdjustResize, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IGetStorage, IGetImage, IErrorLogger
    {
        private Android.Views.Window currentWindow;

        /// <summary>
        /// Called when the app is lauched
        /// </summary>
        /// <param name="bundle">  </param>
        protected override async void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(bundle);
            FacebookClientManager.Initialize(this);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            this.LoadApplication(new App());
            this.currentWindow = this.Window;
            this.Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);
        }

        /// <summary>
        /// When the activity needs to broadcast an intent to the system
        /// </summary>
        /// <param name="requestCode">  </param>
        /// <param name="resultCode">   </param>
        /// <param name="intent">       </param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);
            FacebookClientManager.OnActivityResult(requestCode, resultCode, intent);
        }

        /// <summary>
        /// An interface to get the local storage path
        /// </summary>
        /// <returns>  </returns>
        public string GetStorage()
        {
            return Android.OS.Environment.ExternalStorageDirectory.ToString();
        }

        /// <summary>
        /// When the app requests permissions, take action based on the results
        /// </summary>
        /// <param name="requestCode">   </param>
        /// <param name="permissions">   </param>
        /// <param name="grantResults">  </param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /// <summary>
        /// Load a jpg stream from a given bytearray
        /// </summary>
        /// <param name="image"> The byte array to make into a stream </param>
        /// <returns> The properly formatted stream </returns>
        public Stream GetJPGStreamFromByteArray(byte[] image)
        {
            BitmapFactory.Options bitopt = new BitmapFactory.Options
            {
                InMutable = true
            };
            Bitmap resultBitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length, bitopt);
            //int width = resultBitmap.Width;
            //int height = resultBitmap.Height;
            //for (int x = 0; x < width; x++)
            //{
            //    for (int y = 0; y < height; y++)
            //    {
            //        int argb = resultBitmap.GetPixel(x, y);
            //        if (argb == Android.Graphics.Color.Transparent)
            //        {
            //            resultBitmap.SetPixel(x, y, Android.Graphics.Color.White);
            //        }
            //    }
            //}

            MemoryStream outStream = new MemoryStream();
            resultBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, outStream);
            return outStream;
        }

        /// <summary>
        /// Logs an error to the Android system log (and force closes the app)
        /// </summary>
        /// <param name="error">  </param>
        public void LogError(string error)
        {
            Android.Util.Log.Error("Crash Report", error);
        }

        /// <summary>
        /// Copies the default levels from the assets to the user folder
        /// </summary>
        /// <param name="userpath"></param>
        /// <returns></returns>
        public async Task SetupDefaultLevels(string userpath)
        {
            try
            {
                using (Stream dbAssetStream = Android.App.Application.Context.Assets.Open("dflt.zip"))
                using (Stream dbFileStream = new FileStream(userpath + "dflt.zip", FileMode.OpenOrCreate))
                {
                    var buffer = new byte[1024];

                    int b = buffer.Length;
                    int length;

                    while ((length = await dbAssetStream.ReadAsync(buffer, 0, b)) > 0)
                    {
                        await dbFileStream.WriteAsync(buffer, 0, length);
                    }

                    dbFileStream.Flush();
                    dbFileStream.Close();
                    dbAssetStream.Close();
                }
                //this keeps throwing a stupid exception and i cant tell why
                  //at System.IO.File.SetLastWriteTime (System.String path, System.DateTime lastWriteTime) [0x0002a] in <d4a23bbd2f544c30a48c44dd622ce09f>:0 
                  //at System.IO.Compression.ZipFileExtensions.ExtractToFile(System.IO.Compression.ZipArchiveEntry source, System.String destinationFileName, System.Boolean overwrite)[0x00067] in < 1a0f8b38772b44ca85cc92de9ff0e2e6 >:0
                  //at System.IO.Compression.ZipFileExtensions.ExtractToDirectory(System.IO.Compression.ZipArchive source, System.String destinationDirectoryName, System.Boolean overwrite)[0x0009d] in < 1a0f8b38772b44ca85cc92de9ff0e2e6 >:0
                  //at System.IO.Compression.ZipFile.ExtractToDirectory(System.String sourceArchiveFileName, System.String destinationDirectoryName, System.Text.Encoding entryNameEncoding, System.Boolean overwrite)[0x00017] in < 1a0f8b38772b44ca85cc92de9ff0e2e6 >:0
                  //at System.IO.Compression.ZipFile.ExtractToDirectory(System.String sourceArchiveFileName, System.String destinationDirectoryName, System.Text.Encoding entryNameEncoding)[0x00000] in < 1a0f8b38772b44ca85cc92de9ff0e2e6 >:0
                  //at System.IO.Compression.ZipFile.ExtractToDirectory(System.String sourceArchiveFileName, System.String destinationDirectoryName)[0x00000] in < 1a0f8b38772b44ca85cc92de9ff0e2e6 >:0
                  //at appFBLA2019.Droid.MainActivity +< SetupDefaultLevels > d__7.MoveNext()[0x00206] in C: \Users\tomel\Source\Repos\appFBLA20192\appFBLA2019\appFBLA2019.Android\MainActivity.cs:154
                ZipFile.ExtractToDirectory(userpath + "dflt.zip", userpath);
                File.Delete(userpath + "dflt.zip");
            }
            catch (Exception ex)
            {
                BugReportHandler.SaveReport(ex, "SetupDefaultLevels");
            }
        }
    }
}