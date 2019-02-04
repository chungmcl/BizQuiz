using System;
using System.Collections.Generic;
using System.IO;
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
            /*REMOVE DURING RELEASE*/
            Directory.CreateDirectory(DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug");
            App.Path = DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug";

            ServerConnector.Server = "50.106.17.86";
            
            this.MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart ()
		{
            
            // Handle when your app starts
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
