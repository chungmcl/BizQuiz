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
                // just used the StyleId property to store the file path as a string However Im not sure I need
                Image currentImage = ((Image)((StackLayout)((Button)sender).Parent).Children[6]);

                currentImage.StyleId = file.Path;
                currentImage.Source = file.Path;

                // Enables the image
                currentImage.IsEnabled = true;
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
                                    children[6].StyleId, // adds image using The image path in StyleId
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
                Remove.IsEnabled = false;
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

            Button AddImage = new Button(); // The add Image button
            {
                AddImage.Text = "Add Image";
                AddImage.Clicked += new EventHandler(ButtonAddImage_Clicked);
                AddImage.CornerRadius = 20;
                AddImage.IsVisible = false;
            }

            bool needsPicture = false;
            if (imagePath != "")
                needsPicture = true;
            Image image = new Image // The image itself
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
                AddImage.IsVisible = true;

            // This part deals with swiping Not quite working
            SwipeGestureRecognizer swipe = new SwipeGestureRecognizer{ Direction = SwipeDirection.Left };
            swipe.Swiped += (object sender, SwipedEventArgs e) =>
            {
                System.Timers.Timer deleteTimer = new System.Timers.Timer(1000);
                deleteTimer.Elapsed += (object source, ElapsedEventArgs f) =>
                {
                    ((Frame)sender).Content = null;
                    ((Frame)sender).IsVisible = false;
                };

                System.Timers.Timer swipeTimer = new System.Timers.Timer(30);          
                swipeTimer.Elapsed += (object source, ElapsedEventArgs f) =>
                {
                    ((Frame)sender).TranslationX -= 50;
                };

                swipeTimer.Enabled = true;
                // "sender" is a reference to the object that was swiped
                // "sender" was tested and it returned the correct Frame
                //this.StackLayoutQuestionStack.Children.Remove(((Frame)sender));
                // It is throwing an exception when this code is called because 
                // To my best guess, it's trying to delete the object that it's in while being actively in use
                //this.StackLayoutQuestionStack.Children.Remove(((Frame)sender));
                // This techinically works but doesn't genuinely remove the object from memory

                
            };

            frame.GestureRecognizers.Add(swipe);

            // finally add the stacklayout we just set up to the frame
            frame.Content = frameStack;
            // and add the frame to the the other stacklaout.
            this.StackLayoutQuestionStack.Children.Add(frame);
        }

        void OnSwiped(object sender, SwipedEventArgs e)
        {
            System.Timers.Timer timer = new System.Timers.Timer(20);

            timer.Elapsed += (object source, ElapsedEventArgs f) =>
            {
                ((Frame)((StackLayout)((SwipeGestureRecognizer)sender).Parent).Parent).TranslationX++;
            };

            timer.Enabled = true;

            this.StackLayoutQuestionStack.Children.Remove((Frame)(sender));
        }


            public void SetLevelName(string levelName)
        {
            EntryLevelName.Text = levelName;
        }
    }
}