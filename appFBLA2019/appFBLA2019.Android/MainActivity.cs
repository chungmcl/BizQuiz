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
    [Activity(Label = "appFBLA2019", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IGetStorage, IGetImage
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            FacebookClientManager.Initialize(this);

            this.Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);

            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;

            global::Xamarin.Forms.Forms.Init(this, bundle);
            this.LoadApplication(new App());
        }

        private static void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            var newExc = new Exception("UnobservedTaskException", unobservedTaskExceptionEventArgs.Exception);
            LogUnhandledException(newExc);
        }

        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var newExc = new Exception("UnhandledException", unhandledExceptionEventArgs.ExceptionObject as Exception);
            LogUnhandledException(newExc);
        }

        internal static void LogUnhandledException(Exception exception)
        {
            try
            {
                string errorFilePath = System.IO.Path.Combine(App.Path, "/CrashReport.log");
                var errorMessage = String.Format($"Error (Unhandled Exception): {exception.ToString()}");
                File.WriteAllText(errorFilePath, errorMessage);

                BugReportHandler.SubmitReport(new BugReport("Unhandled Exception", "Exceptions", errorMessage));

                // Log to Android Device Logging.
                Android.Util.Log.Error("Crash Report", errorMessage);
            }
            catch
            {
                // just suppress any error logging exceptions
            }
        }

        private void DisplayCrashReport()
        {
            string logPath = System.IO.Path.Combine(App.Path, "/CrashReport.log");
            if (File.Exists(logPath))
            {
                var errorText = File.ReadAllText(logPath);
                if (!BugReportHandler.SubmitReport(new BugReport("Unhandled Exception", "Exceptions", errorText)))
                {
                    new AlertDialog.Builder(this)
                      .SetPositiveButton("Clear", (sender, args) =>
                      {
                      })
                      .SetNegativeButton("Close", (sender, args) =>
                      {
                          // User pressed Close.
                      })
                      .SetMessage(errorText)
                      .SetTitle("Would you like to send this crash report?")
                      .Show();
                }
                File.Delete(logPath);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);
            FacebookClientManager.OnActivityResult(requestCode, resultCode, intent);
        }

        public string GetStorage()
        {
            return Android.OS.Environment.ExternalStorageDirectory.ToString();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

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
    }
}