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
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        // This method is invoked when the application has loaded and is ready to run. In this method you should instantiate the window, load the UI into it and then make the window visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            this.LoadApplication(new App());

            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;

            return base.FinishedLaunching(app, options);
        }

        private static void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            Exception ex = new Exception("UnobservedTaskException", unobservedTaskExceptionEventArgs.Exception);
            LogUnhandledException(ex);
        }

        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Exception ex = new Exception("UnhandledException", unhandledExceptionEventArgs.ExceptionObject as Exception);
            LogUnhandledException(ex);
        }

        internal static void LogUnhandledException(Exception exception)
        {
            try
            {
                string logPath = System.IO.Path.Combine(App.Path, "/CrashReport.log");
                var errorText = String.Format($"Error (Unhandled Exception): {exception.ToString()}");
                File.WriteAllText(logPath, errorText);

                BugReportHandler.SubmitReport(new BugReport("Unhandled Exception", "Exceptions", errorText));

                //writing actual crash logs in IOS is difficult and we wouldn't use them anyway
            }
            catch
            {
                // just suppress any error logging exceptions
            }
        }
    }
}