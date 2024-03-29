//BizQuiz App 2019

using Plugin.Permissions;
using Realms;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace appFBLA2019
{
    public partial class App : Application
    {
        /// <summary>
        /// Main app constructor:
        /// this registers dependencies and starts background processes that are 
        /// required for the app to function
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            Xamarin.Forms.DependencyService.Register<IGetStorage>();
            Xamarin.Forms.DependencyService.Register<IErrorLogger>();
            Xamarin.Forms.DependencyService.Register<IGetImage>();
            Xamarin.Forms.DependencyService.Register<ICloseApplication>();

            App.random = new Random();
            App.Path = DependencyService.Get<IGetStorage>().GetStorage() + "/";

            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
            this.MainPage = new NavigationPage(new MainPage());
        }

        /// <summary>
        /// a random to be used for shuffling and other functions
        /// </summary>
        public static Random random;

        /// <summary>
        /// the Path to the app's main directory
        /// </summary>
        public static string Path;

        /// <summary>
        /// The path to the current user's folder within
        /// the app's main directory.
        /// </summary>
        public static string UserPath
        {
            get
            {
                string path = Path + CredentialManager.Username + "/";
                Directory.CreateDirectory(path);
                return path;
            }
        }

        public static RealmConfiguration realmConfiguration(string path)
        {
            return new RealmConfiguration(path) { SchemaVersion = 2 };
        }

        /// <summary>
        /// When the app comes back into main focus
        /// </summary>
        protected override void OnResume()
        {
        }

        /// <summary>
        /// When the app is switched away from
        /// </summary>
        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        /// <summary>
        /// Sets up stuff that needs storage/network permissions
        /// </summary>
        protected override async void OnStart()
        {
            string tempPicturesDir = App.Path + "/" + "Pictures";
            if (Directory.Exists(tempPicturesDir))
                Directory.Delete(App.Path + "/" + "Pictures", true);

            CredentialManager.Username = "dflt";
            Directory.CreateDirectory(App.Path + $"dflt");

            if (!App.Current.Properties.ContainsKey("Tutorial"))
            {
                App.Current.Properties.Add("Tutorial", "Yes");
                await this.SavePropertiesAsync();
            }

            BugReportHandler.Setup();
            await ThreadTimer.RunServerChecksAsync();
            if (Directory.GetDirectories(App.UserPath).Length < 5)
            {
                await DependencyService.Get<IGetStorage>().SetupDefaultQuizzesAsync(App.UserPath);
            }
            BugReportHandler.ProcessCrashLog();
        }

        /// <summary>
        /// Handles exceptions from unobserved tasks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="unobservedTaskExceptionEventArgs"></param>
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

        /// <summary>
        /// Handles exceptions from the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="unhandledExceptionEventArgs"></param>
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

        /// <summary>
        /// Saves unhandled exceptions to file and logs them with device diagnostics
        /// </summary>
        /// <param name="exception"></param>
        private static void LogUnhandledException(Exception exception)
        {
            try
            {
                string logPath = App.Path + "/CrashReport.txt";
                var errorText = String.Format($"Error (Unhandled Exception): {exception.ToString()}");
                File.WriteAllText(logPath, errorText);

                DependencyService.Get<IErrorLogger>().LogError(errorText);
            }
            catch
            {
                // just suppress any error logging exceptions
            }
        }
    }
}