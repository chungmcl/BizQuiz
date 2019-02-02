using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plugin.Media;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CreateNewLevelPage : ContentPage
	{
		public CreateNewLevelPage()
		{           
			this.InitializeComponent();
        }

        /// <summary>
        /// Called when the user presses the Add Image button on a question eiditor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonAddImage_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            Plugin.Media.Abstractions.MediaFile file = await CrossMedia.Current.PickPhotoAsync();

;
            // Couldn't think of a proper way, but just used the StyleId property to store the file path as a string
            // CURRENT: NEED TO SET THE STYLEID AND SOURCE OF THE CURRENT QUESTION TO FILE.PATH


            //((Image)StackLayoutQuestionStack.Children[6]).StyleId = file.Path;
            //((Image)StackLayoutQuestionStack.Children[6]).Source = file.Path;

            // Enables the image
            ((Image)StackLayoutQuestionStack.Children[6]).IsEnabled = true;
        }

        /// <summary>
        /// Called when the add question button is clicked and adds a new question
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAddQuestion_Clicked(object sender, EventArgs e)
        {
            this.AddNewQuestion();
        }

        /// <summary>
        /// Removes the Question Frame when remove button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonRemove_Clicked(object sender, EventArgs e)
        {
            this.StackLayoutQuestionStack.Children.Remove((((Frame)((StackLayout)((ImageButton)sender).Parent).Parent)));
        }

        /// <summary>
        /// Creates the Level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCreateLevel_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.EntryLevelName.Text))
            {
                DisplayAlert("Couldn't Create Level", "Please give your level a name.", "OK");
            }
            else if (this.StackLayoutQuestionStack.Children.Count < 2)
            {
                DisplayAlert("Couldn't Create Level", "Please create at least two questions", "OK");
            }
            else
            {
                // Later, need to impliment username to pass through
                DBHandler.SelectDatabase(this.EntryLevelName.Text.Trim(), "testAuthor");
                List<Question> questionsToAdd = new List<Question>();  // A list of questions to add to the database \
                // Loops through each question frame on the screen 
                foreach (Frame frame in this.StackLayoutQuestionStack.Children)
                {

                    for (int i = 0; i < ((StackLayout)frame.Content).Children.Count; i++)
                    {

                        IList<View> children = ((StackLayout)frame.Content).Children;
                        Question addThis;

                        string[] answers = { "c/" + ((Entry)children[2]).Text,
                                    "x/" + ((Entry)children[3]).Text,
                                    "x/" + ((Entry)children[4]).Text,
                                    "x/" + ((Entry)children[5]).Text};

                    if (((Image)children[6]).IsEnabled) // if needs picture
                        {
                            addThis = new Question(
                                    ((Entry)children[1]).Text,
                                    children[6].StyleId, // The image path
                                    answers);
                        }
                        else // if not needs picture
                        {
                            addThis = new Question(
                                    ((Entry)children[1]).Text,
                                    answers);
                        }

                        questionsToAdd.Add(addThis);

                    }
                }
                DBHandler.Database.AddQuestions(questionsToAdd);

                // Returns user to front page of LevelEditor
                this.Navigation.PushAsync(new LevelEditorPage());
            }
        }

        /// <summary>
        /// a private AddNewQuestions for when the user creates a brand new question
        /// </summary>
        private void AddNewQuestion()
        {
            string[] noAnswers = new string[0];
            AddNewQuestion(null, "", noAnswers);
        }

        /// <summary>
        /// This AddNewQuestions is for if the question contains no image
        /// </summary>
        /// <param name="question">the Question to answer</param>
        /// <param name="answers">the first is the correct answer, the rest are incorrect answers</param>
        public void AddNewQuestion(string question, params string[] answers)
        {
            AddNewQuestion(question, null, answers);
        }

        /// <summary>
        /// This add New Questions contains parameters for images for when a question contains an image.
        /// </summary>
        /// <param name="question">the Question to answer</param>
        /// <param name="imagePath">the path for the image corrosponding to the question</param>
        /// <param name="answers">the first is the correct answer, the rest are incorrect answers</param>
        public void AddNewQuestion(string question, string imagePath, params string[] answers)
        {
            Frame frame = new Frame()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10
            };

            StackLayout frameStack = new StackLayout
            {
                FlowDirection = FlowDirection.LeftToRight,
                Orientation = StackOrientation.Vertical
            };

            ImageButton Remove = new ImageButton();
            {
                Remove.Source = "ic_delete_black_48dp.png";
                Remove.Clicked += new EventHandler(ButtonRemove_Clicked);               
            }
            frameStack.Children.Add(Remove);

            Entry Question = new Entry
            {
                Placeholder = "Enter question",
                
            };
            if (question != null) 
                Question.Text = question;
            frameStack.Children.Add(Question);

            Entry AnswerCorrect = new Entry
            {
                Placeholder = "Enter correct answer",
                
            };
            if (answers.Count() != 0)
                AnswerCorrect.Text = answers[0];
            frameStack.Children.Add(AnswerCorrect);

            Entry AnswerWrongOne = new Entry
            {
                Placeholder = "Enter a possible answer",
            };
            if (answers.Count() != 0)
                AnswerWrongOne.Text = answers[1];
            frameStack.Children.Add(AnswerWrongOne);

            Entry AnswerWrongTwo = new Entry
            {
                Placeholder = "Enter a possible answer",
            };
            if (answers.Count() != 0)
                AnswerWrongTwo.Text = answers[2];
            frameStack.Children.Add(AnswerWrongTwo);

            Entry AnswerWrongThree = new Entry
            {
                Placeholder = "Enter a possible answer",
            };
            if (answers.Count() != 0)
                AnswerWrongThree.Text = answers[2];
            frameStack.Children.Add(AnswerWrongThree);

            Button AddImage = new Button();
            {
                AddImage.Text = "Add Image";
                AddImage.Clicked += new EventHandler(ButtonAddImage_Clicked);
                AddImage.CornerRadius = 20;
                AddImage.IsEnabled = false;
            }

            bool needsPicture = false;
            if (imagePath != "")
                needsPicture = true;
            Image image = new Image
            {
                IsEnabled = needsPicture,          
            };
            frameStack.Children.Add(image);
            frameStack.Children.Add(AddImage);
            //Gets the image from the imagePath
            if (image.IsEnabled)
            {
                image.Source = imagePath;
            }
            else // or adds the add image button
                AddImage.IsEnabled = true;

            frame.Content = frameStack;
            this.StackLayoutQuestionStack.Children.Add(frame);
        }

        public void SetLevelName(string levelName)
        {
            EntryLevelName.Text = levelName;
        }
    }
}