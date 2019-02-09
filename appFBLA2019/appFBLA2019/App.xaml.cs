using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace appFBLA2019
{
	public partial class App : Application
	{
        public static string Path;
            
        public App ()
		{
            this.InitializeComponent();
            Xamarin.Forms.DependencyService.Register<IGetStorage>();

            Directory.CreateDirectory(DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug");
            App.Path = DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug";

            CredentialManager.StartTimedCheckLoginStatus();
            
            /*REMOVE DURING RELEASE*/
            ServerConnector.Server = "50.106.17.86";
            this.MainPage = new NavigationPage(new MainPage());
        }

        protected override async void OnStart ()
		{
            // Handle when your app starts
            await CredentialManager.CheckLoginStatus();
        }

		protected override void OnSleep ()
		{
            // Handle when your app sleeps
        }

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
    }
}
