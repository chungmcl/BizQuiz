using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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

            if (file != null) // if the user actually picked an image
            {
                ImageButton currentImage;

                currentImage = ((ImageButton)((StackLayout)((View)sender).Parent).Children[6]);
 

                currentImage.Source = file.Path;

                // Enables the image
                currentImage.IsEnabled = true;
                if (sender is Button)
                    ((Button)sender).IsVisible = false;
            }

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
        async private void ButtonRemove_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Warning", "Are you sure you would like to delete this question?", "Yes", "No");
            if (answer == true)
            {
                //this.StackLayoutQuestionStack.Children.Remove((((Frame)((StackLayout)((ImageButton)sender).Parent).Parent))); // Removes the question

                Frame frame = ((Frame)((StackLayout)((ImageButton)sender).Parent).Parent);
                //Animate A deletion
                await frame.TranslateTo(-Application.Current.MainPage.Width, 0, 250, Easing.CubicIn);
                this.StackLayoutQuestionStack.Children.Remove(frame);
            }
        }

        /// <summary>
        /// Saves the user created level
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

                // clear the database to prevent duplication
                DBHandler.Database.DeleteQuestions(DBHandler.Database.GetQuestions().ToArray());

                List<Question> questionsToAdd = new List<Question>();  // A list of questions to add to the database
                // Loops through each question frame on the screen 
                foreach (Frame frame in this.StackLayoutQuestionStack.Children)
                {
                    // A list of all the children of the current frame
                    IList<View> children = ((StackLayout)frame.Content).Children;
                    
                    Question addThis;

                    //The answers to the question
                    string[] answers = {((Entry)children[2]).Text, //Correct answer
                                ((Entry)children[3]).Text, // Incorect answer
                                ((Entry)children[4]).Text, // Incorect answer
                                ((Entry)children[5]).Text}; // Incorect answer

                    if (((ImageButton)children[6]).IsEnabled) // if needs image
                    {
                        addThis = new Question(
                                ((Entry)children[1]).Text, // The Question
                                ((ImageButton)children[6]).Source.ToString().Substring(6), // adds image using the image source
                                answers);
                        addThis.NeedsPicture = true;
                    }
                    else // if not needs picture
                    {
                        addThis = new Question(
                                ((Entry)children[1]).Text,
                                answers);
                    }

                        questionsToAdd.Add(addThis);
                }
                
                // Adds the list of questions to the database
                DBHandler.Database.AddQuestions(questionsToAdd);

                // Returns user to front page of LevelEditor
                this.Navigation.PopAsync(true);
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
            AddNewQuestion(question, "", answers);
        }

        /// <summary>
        /// This add New Questions contains parameters for images for when a question contains an image.
        /// </summary>
        /// <param name="question">the Question to answer</param>
        /// <param name="imagePath">the path for the image corrosponding to the question</param>
        /// <param name="answers">the first is the correct answer, the rest are incorrect answers</param>
        public void AddNewQuestion(string question, string imagePath, params string[] answers)
        {
            Frame frame = new Frame() // The frame that holds everything
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10
            };

            StackLayout frameStack = new StackLayout //the stack that holds all the info in the frame
            {
                FlowDirection = FlowDirection.LeftToRight,
                Orientation = StackOrientation.Vertical
            };

            ImageButton Remove = new ImageButton(); // the button to remove the question
            {
                Remove.Source = "ic_delete_black_48dp.png";
                Remove.Clicked += new EventHandler(ButtonRemove_Clicked);
                Remove.BackgroundColor = Color.Transparent;
                Remove.HeightRequest = 40;
                Remove.WidthRequest = 40;
                Remove.HorizontalOptions = LayoutOptions.End;
                Remove.VerticalOptions = LayoutOptions.Start;
            }
            frameStack.Children.Add(Remove);

            Entry Question = new Entry // The question
            {
                Placeholder = "Enter question",
                
            };
            if (question != null) 
                Question.Text = question;
            frameStack.Children.Add(Question);

            Entry AnswerCorrect = new Entry // The correct answer
            {
                Placeholder = "Enter correct answer",
                
            };
            if (answers.Count() != 0)
                AnswerCorrect.Text = answers[0];
            frameStack.Children.Add(AnswerCorrect);

            Entry AnswerWrongOne = new Entry // A wrong answer
            {
                Placeholder = "Enter a possible answer",
            };
            if (answers.Count() != 0)
                AnswerWrongOne.Text = answers[1];
            frameStack.Children.Add(AnswerWrongOne);

            Entry AnswerWrongTwo = new Entry// A wrong answer
            {
                Placeholder = "Enter a possible answer",
            };
            if (answers.Count() != 0)
                AnswerWrongTwo.Text = answers[2];
            frameStack.Children.Add(AnswerWrongTwo);

            Entry AnswerWrongThree = new Entry// A wrong answer
            {
                Placeholder = "Enter a possible answer",
            };
            if (answers.Count() != 0)
                AnswerWrongThree.Text = answers[2];
            frameStack.Children.Add(AnswerWrongThree);

            bool needsPicture = false;
            if (imagePath != "")
                needsPicture = true;

            Button AddImage = new Button(); // The add Image button
            {
                AddImage.Text = "Add Image";
                AddImage.Clicked += new EventHandler(ButtonAddImage_Clicked);
                AddImage.CornerRadius = 20;
                AddImage.IsVisible = false;
                
            }


            ImageButton image = new ImageButton(); // The image itself
            {
                image.IsEnabled = needsPicture;
                image.Clicked += new EventHandler(ButtonAddImage_Clicked);
                image.BackgroundColor = Color.Transparent;
            }

            frameStack.Children.Add(image);
            frameStack.Children.Add(AddImage);
            //Gets the image from the imagePath
            if (image.IsEnabled)
            {
                image.Source = imagePath;
            }
            else // or adds the add image button
                AddImage.IsVisible = true;
            
            frame.Content = frameStack;
            // and add the frame to the the other stacklaout.
            this.StackLayoutQuestionStack.Children.Add(frame);
        }


        public void SetLevelName(string levelName)
        {
            EntryLevelName.Text = levelName;
        }
    }
}