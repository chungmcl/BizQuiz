//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LevelEditorPage : ContentPage
    {
        public LevelEditorPage()
        {
            this.InitializeComponent();
        }

        // Get the list of Quizes the user can edit
        private List<string> FindDatabase()
        {
            // get a list of all databases the user has on the device
            List<string> databaseList = new List<string>();
            string[] subFolderNames = Directory.GetDirectories(App.Path);
            foreach (string levelName in subFolderNames)
            {
                if (levelName.Contains('`'))
                {
                    // Might have to change this up when it comes to release as Levels will be stored somewhere else!
                    string level = levelName.Remove(levelName.IndexOf('`'),
                        levelName.Count() - levelName.IndexOf('`')).Substring(30);
                    string username = (levelName.Substring(30)).Remove(0, level.Count() + 1);
                    databaseList.Add(level + " By " + username);
                }
            }

            return databaseList;
        }

        private void ButtonCreate_Clicked(object sender, EventArgs e)
        {
            if (CredentialManager.IsLoggedIn)
            {
                this.Navigation.PushAsync(new CreateNewLevelPage());
            }
            else
            {
                this.DisplayAlert("Hold on!", "Before you can create your own custom levels, you have to create your own account.", "Ok");
            }
        }

        async private void ButtonLevelSelect_Clicked(object sender, EventArgs e)
        {
            if (CredentialManager.IsLoggedIn)
            {
                string level = await this.DisplayActionSheet("Select a quiz to edit", "Cancel", null, this.FindDatabase().ToArray());

                if (!string.IsNullOrWhiteSpace(level) && level != "Cancel") // If the user has selected something then open the page
                {
                    string levelTitle = level.Remove(level.IndexOf(" By "));
                    string levelAuthor = level.Substring(level.IndexOf(" By ") + 4);
                    DBHandler.SelectDatabase(levelTitle, levelAuthor);
                    CreateNewLevelPage levelPage = new CreateNewLevelPage(levelTitle, levelAuthor); //Create the levelPage
                                                                                                    // Add the questions from the database to the page to edit
                    List<Question> test = DBHandler.Database.GetQuestions();
                    foreach (Question question in DBHandler.Database.GetQuestions())
                    {
                        levelPage.SetLevelName(levelTitle);
                        levelPage.AddNewQuestion(question);
                    }
                    await this.Navigation.PushAsync(levelPage);
                }
            }
            else
            {
                await this.DisplayAlert("Hold on!", "Before you can create your own custom levels, you have to create your own account.", "Ok");
            }
        }
    }
}