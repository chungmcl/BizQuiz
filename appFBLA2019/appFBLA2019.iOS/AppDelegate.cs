//BizQuiz App 2019

using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using Plugin.Permissions;
using System.IO.Compression;
using System.Reflection;

[assembly: Dependency(typeof(appFBLA2019.iOS.AppDelegate))]
namespace appFBLA2019.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IGetImage, IGetStorage, IErrorLogger
    {
        // This method is invoked when the application has loaded and is ready to run. In this method you should instantiate the window, load the UI into it and then make the window visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            this.LoadApplication(new App());
            return base.FinishedLaunching(app, options);
        }

        public Stream GetJPGStreamFromByteArray(byte[] image)
        {
            // test
            UIKit.UIImage images = new UIKit.UIImage(Foundation.NSData.FromArray(image));
            byte[] bytes = images.AsJPEG(100).ToArray();
            Stream imgStream = new MemoryStream(bytes);
            return imgStream;
        }

        public string GetStorage()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Resources);
        }

        public void LogError(string error)
        {
            //throw new NotImplementedException();
        }

        public Task SetupDefaultQuizzesAsync(string userpath)
        {
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(AppDelegate)).Assembly;
                using (Stream dbAssetStream = assembly.GetManifestResourceStream("appFBLA2019.dflt.zip"))
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
            return Task.CompletedTask;
        }

        public bool IsPackageInstalled(string packageName)
        {
            string url = "";
            switch (packageName)
            {
                case "com.twitter.android":
                    url = "www.twitter.com";
                    break;
                case "com.instagram.android":
                    url = "www.instagram.com";
                    break;
            }
            
            NSUrl nsurl = new NSUrl(url);
            if (UIApplication.SharedApplication.CanOpenUrl(nsurl))
                return true;
            else
                return false;
        }
    }
}