//BizQuiz App 2019

using Plugin.Permissions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
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
            Xamarin.Forms.DependencyService.Register<IErrorLogger>();
            Xamarin.Forms.DependencyService.Register<IGetImage>();
            Xamarin.Forms.DependencyService.Register<ICloseApplication>();

            App.random = new Random();
            //broken cause of userpath moving
            ////if there's nothing in the user's folder
            //if (Directory.GetFiles(UserPath).Length == 0)
            //{
            //    Task.WaitAll(Task.Run(() => DependencyService.Get<IGetStorage>().SetupDefaultLevels(App.UserPath)));
            //}


            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
            this.MainPage = new NavigationPage(new MainPage());
        }

        /// <summary>
        /// a random to be used for shuffling and other functions
        /// </summary>
        public static Random random;

        public static string debugFolder = "/FBLADebug/";

        public static string Path;

        public static string UserPath
        {
            get
            {
                string path = Path + CredentialManager.Username + "/";
                Directory.CreateDirectory(path);
                return path;
            }
        }

        protected override void OnResume()
        {
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override async void OnStart()
        {
            // Handle when your app starts
            await CrossPermissions.Current.RequestPermissionsAsync(Plugin.Permissions.Abstractions.Permission.Storage);
            string tempPicturesDir = App.Path + "/" + "Pictures";
            if (Directory.Exists(tempPicturesDir))
                Directory.Delete(App.Path + "/" + "Pictures", true);

            CredentialManager.Username = "dflt";
            Directory.CreateDirectory(App.Path + $"dflt");
            
            BugReportHandler.Setup();

            // If app's server IP has not been changed, default to IP in else statement.
            // If it has been changed, use the new IP.
            string serverIp = await SecureStorage.GetAsync("serverIP");
            if (serverIp != null)
                ServerConnector.Server = serverIp;
            else
                ServerConnector.Server = "50.106.17.86"; // Valid as of March 3rd, 2019

            await ThreadTimer.RunServerChecks();
            BugReportHandler.ProcessCrashLog();
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