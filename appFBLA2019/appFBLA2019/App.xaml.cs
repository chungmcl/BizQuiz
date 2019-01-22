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
            // Initialize Live Reload.
#if DEBUG
            //LiveReload.Init();
#endif

            this.InitializeComponent();
            Xamarin.Forms.DependencyService.Register<IGetStorage>();
            /*REMOVE DURING RELEASE*/
            Directory.CreateDirectory(DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug");
            App.Path = DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug";
            
            this.MainPage = new NavigationPage(new MainPage());

            /*REMOVE DURING RELEASE*/
            // If you want to generate a testLevel 
            //DBHandler.SelectDatabase("testLevel", "testAuthor");
            //List<Question> questions = new List<Question>();
            //for (int i = 0; i < 10; i++)
            //{
            //    questions.Add(new Question());
            //}
            //DBHandler.Database.AddQuestions(questions);
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
