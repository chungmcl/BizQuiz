//BizQuiz App 2019

using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms.Platform.iOS;

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
            throw new NotImplementedException();
        }

        public string GetStorage()
        {
            throw new NotImplementedException();
        }

        public void LogError(string error)
        {
            throw new NotImplementedException();
        }

        public Task SetupDefaultLevels(string userpath)
        {
            throw new NotImplementedException();
        }
    }
}