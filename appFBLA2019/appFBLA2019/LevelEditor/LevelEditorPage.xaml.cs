using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Timers;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LevelEditorPage : ContentPage
	{
	    public LevelEditorPage ()
	    {
		    this.InitializeComponent ();
            this.SetUp();
        }

        private void ButtonCreate_Clicked(object sender, EventArgs e)
        {
            if (CredentialManager.IsLoggedIn)
                this.Navigation.PushAsync(new CreateNewLevelPage());
            else
                DisplayAlert("Hold on!", "Before you can create your own custom levels, you have to create your own accout.", "Ok");
        }
        

        private void OnPickerSelectedIndexChanged(Object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;
            if (CredentialManager.IsLoggedIn)
            {
                if (selectedIndex != -1) // If the user has selected something then open the page
                {
                    DBHandler.SelectDatabase((string)PickerLevelSelect.SelectedItem, "testAuthor");
                    CreateNewLevelPage levelPage = new CreateNewLevelPage(); //Create the levelPage
                                                                             // Add the questions from the database to the page to edit
                    List<Question> test = DBHandler.Database.GetQuestions();
                    foreach (Question question in DBHandler.Database.GetQuestions())
                    {

                        levelPage.SetLevelName((string)PickerLevelSelect.SelectedItem);
                        levelPage.AddNewQuestion(question);
                    }
                    this.Navigation.PushAsync(levelPage);
                }
            }
            else
                DisplayAlert("Hold on!", "Before you can create your own custom levels, you have to create your own accout.", "Ok");
            // Reset the picker value
            picker.SelectedIndex = -1;
        }

        // Now the user can edit the questions, essentially the same as create new level but with everything filled out already.

        private void SetUp()
        {
            this.FindDatabase();

            //Timer timer = new Timer(15000);
            //timer.Elapsed += this.OnTimedEvent;
            //timer.AutoReset = true;
            //timer.Enabled = true;
        }


        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            this.FindDatabase();
        }



        // Get the list of Quizes the user can edit
        public void FindDatabase()
        {
            List<string> databaseList = new List<string>();


            // get a list of all databases the user has on the device

        
            string[] subFolderNames = Directory.GetDirectories(App.Path);
            foreach (string levelName in subFolderNames)
            {
                if (levelName.Contains('`'))
                {
                    // Might have to change this up when it comes to release as Levels will be stored somewhere else!
                    databaseList.Add(levelName.Remove(levelName.IndexOf('`'),
                        levelName.Count() - levelName.IndexOf('`')).Substring(30));
                }

            }

            this.PickerLevelSelect.ItemsSource = databaseList;
        }

    }
}