//BizQuiz App 2019

using System;
using System.Collections.Generic;
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

            Directory.CreateDirectory(DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug");
            App.Path = DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug";

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
    }
}