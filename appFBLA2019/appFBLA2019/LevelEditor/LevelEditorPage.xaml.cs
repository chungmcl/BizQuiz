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
            this.categoryPaths = Directory.GetDirectories(App.UserPath);

            this.categories = new string[this.categoryPaths.Count()];
            for (int i = 0; i < this.categories.Length; i++)
            {
                this.categories[i] = this.categoryPaths[i].Split('/').Last();
            }
        }

        private string[] categoryPaths;
        private string[] categories;

        // Get the list of Quizes the user can edit
        private List<string> FindDatabase()
        {
            // get a list of all databases the user has on the device
            List<string> databaseList = new List<string>();
            foreach (string category in this.categoryPaths)
            {
                databaseList.Add(category.Split('/').Last() + ":");
                string[] subFolderNames = Directory.GetDirectories(category);
                foreach (string levelPath in subFolderNames)
                {
                    if (levelPath.Contains('`'))
                    {
                        string levelName = levelPath.Split('/').Last();
                        // Might have to change this up when it comes to release as Levels will be stored somewhere else!
                        string level = levelName.Split('`').First();
                        string username = levelName.Split('`').Last();
                        databaseList.Add(level + " by " + username);
                    }
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

        private async void ButtonLevelSelect_Clicked(object sender, EventArgs e)
        {
            if (CredentialManager.IsLoggedIn)
            {
                string[] selections = this.FindDatabase().ToArray();
                string level = await this.DisplayActionSheet("Select a quiz to edit", "Cancel", null, selections);

                if (!string.IsNullOrWhiteSpace(level) && level != "Cancel" && !this.categories.Contains(level.Split(':').First())) // If the user has selected something then open the page
                {
                    string levelTitle = level.Remove(level.IndexOf(" by "));
                    string levelAuthor = level.Substring(level.IndexOf(" by ") + 4);
                    string category = this.GetCategory(selections.ToList(), level);
                    DBHandler.SelectDatabase(category, levelTitle, levelAuthor);
                    CreateNewLevelPage levelPage = new CreateNewLevelPage(category, levelTitle, levelAuthor); //Create the levelPage

                    levelPage.SetLevelName(levelTitle);
                    foreach (Question question in DBHandler.Database.GetQuestions())
                    {
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

        private string GetCategory(List<String> selections, string choice)
        {
            int choiceIndex = selections.FindIndex(x => x == choice);
            for (int i = choiceIndex; i < 0; i--)
            {
                if (this.categoryPaths.Contains(selections[i]))
                {
                    return selections[i];
                }
            }
            return "FBLA General";
        }
    }
}