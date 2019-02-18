//BizQuiz App 2019

using System.Collections.Generic;
using System;
using System.IO;
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
            Xamarin.Forms.DependencyService.Register<IGetImage>();

            Directory.CreateDirectory(DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug");

            Path = DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug" + "/";

            ServerConnector.Server = "50.106.17.86";

            this.MainPage = new NavigationPage(new MainPage());
        }

        public static string Path;

        protected override void OnResume()
        {

        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override async void OnStart()
        {
            await ThreadTimer.RunServerChecks();
        }
    }
}