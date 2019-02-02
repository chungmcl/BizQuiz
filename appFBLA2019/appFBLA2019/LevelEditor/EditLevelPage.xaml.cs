using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditLevelPage : ContentPage
    {
        public EditLevelPage()
        {
            InitializeComponent();
            FindDatabase();
            
        }

        private void OnPickerSelectedIndexChanged(Object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                ButtonEditLevel.IsEnabled = true;
            }
        }
        
        // Now the user can edit the questions, essentially the same as create new level but with everything filled out already.

        // Get the list of Quizes the user can edit
        private void FindDatabase()
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

            PickerLevelSelect.ItemsSource = databaseList;
        }

        // Called when the user presses the button to edit a quiz, Sets up the edit page
        private void ButtonEditLevel_Clicked(object sender, EventArgs e)
        {
            if(PickerLevelSelect.SelectedIndex != -1) // If the user has selected something then open the page
            {
                DBHandler.SelectDatabase((string)PickerLevelSelect.SelectedItem, "testAuthor");
                CreateNewLevelPage levelPage = new CreateNewLevelPage(); //Create the levelPage
                // Add the questions from the database to the page to edit
                foreach (Question question in DBHandler.Database.GetQuestions()) 
                {
                    string[] answers = new string[] {
                           question.CorrectAnswer,
                           question.AnswerOne,
                           question.AnswerTwo,
                           question.AnswerThree };

                    levelPage.SetLevelName((string)PickerLevelSelect.SelectedItem);

                    if (question.NeedsPicture) // Check if the question needs an image or not
                    {
                        levelPage.AddNewQuestion(question.QuestionText, question.ImageByteArray, answers);
                    }
                    else
                    {
                        levelPage.AddNewQuestion(question.QuestionText, answers);
                    }
                }
                this.Navigation.PushAsync(levelPage);
            }
            
        }
    }
}