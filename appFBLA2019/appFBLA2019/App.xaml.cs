//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace appFBLA2019
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            Xamarin.Forms.DependencyService.Register<IGetStorage>();

            Directory.CreateDirectory(DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug");
            App.Path = DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug";

            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
            BugReportHandler.Setup();
            this.SendCrashLog();

            this.MainPage = new NavigationPage(new MainPage());
        }

        public static string Path;

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override async void OnStart()
        {
            // Handle when your app starts

            /*REMOVE DURING RELEASE*/
            ServerConnector.Server = "50.106.17.86";

            await CredentialManager.CheckLoginStatus();
            CredentialManager.StartTimedCheckLoginStatus();
        }

        private void SendCrashLog()
        {
            string logPath = App.Path + "/CrashReport.log";
            if (File.Exists(logPath))
            {
                var errorText = File.ReadAllText(logPath);
                BugReportHandler.SubmitReport(new BugReport("Unhandled Exception", "Exceptions", errorText));
                File.Delete(logPath);
            }
        }

        private static void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            if (unobservedTaskExceptionEventArgs.Exception.InnerException == null)
            {
                LogUnhandledException(unobservedTaskExceptionEventArgs.Exception);
            }
            else
            {
                LogUnhandledException(unobservedTaskExceptionEventArgs.Exception.InnerException);
            }
        }

        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            if ((unhandledExceptionEventArgs.ExceptionObject as Exception).InnerException == null)
            {
                LogUnhandledException(unhandledExceptionEventArgs.ExceptionObject as Exception);
            }
            else
            {
                LogUnhandledException((unhandledExceptionEventArgs.ExceptionObject as Exception).InnerException);
            }
        }

        private static void LogUnhandledException(Exception exception)
        {
            try
            {
                string logPath = App.Path + "/CrashReport.log";
                var errorText = String.Format($"Error (Unhandled Exception): {exception.ToString()}");
                File.WriteAllText(logPath, errorText);

                BugReportHandler.SubmitReport(new BugReport("Unhandled Exception", "Exceptions", errorText));

                DependencyService.Get<IErrorLogger>().LogError(errorText);
            }
            catch
            {
                // just suppress any error logging exceptions
            }
        }
    }
}