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
            // Scroll to bottom
            this.ScrollViewQuestions.ScrollToAsync(this.stkMain, ScrollToPosition.End, true);
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
                // Open Database or create it
                DBHandler.SelectDatabase(this.EntryLevelName.Text.Trim(), CredentialManager.Username);

                List<Question> NewQuestions = new List<Question>();  // A list of questions the user wants to add to the database
                List<Question> previousQuestions = DBHandler.Database.GetQuestions(); // A list of questions already in the database

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

                    NewQuestions.Add(addThis);
                }

                // Add if it doesn't already exist,
                // delete if it doesn't exist anymore, 
                // update the ones that need to be updated, 
                // and do nothing to the others

                // It exists/changed if the DBId exists in both old and new list
                //     - contents are the same: Exists and same
                //     - contents changed: changed
                // It's new if DBId isn't present in new list
                // It's deleted if DBId exists in old questions and not new questions

                // Work in progress, algorithm might be off.
                if (previousQuestions.Count() == 0) // if the user created this for the first time
                    DBHandler.Database.AddQuestions(NewQuestions);
                else
                {
                    for (int i = 0; i <= previousQuestions.Count() - 1; i++)
                    {
                        bool DBIdSame = true;
                        // test each old question with each new question
                        foreach (Question newQuestion in NewQuestions)
                        {
                            if (newQuestion.DBId == "")
                            {
                                // this is a new question. add it then remove it from the list     
                                // AddQuestions should be able to take params of questions 
                                // because It doesn't I have to do this less effective method
                                DBHandler.Database.AddQuestions(new List<Question> { newQuestion });
                                NewQuestions.Remove(newQuestion);
                            }
                            if (previousQuestions[i] == newQuestion)
                            {
                                DBIdSame = true;
                                NewQuestions.Remove(newQuestion);
                                // contents are the same, so don't delete or change
                            }
                            else if (previousQuestions[i].DBId == newQuestion.DBId)
                            {
                                DBIdSame = true;
                                // the same question, but changed, so update
                                DBHandler.Database.realmDB.Write(() =>
                                {
                                    previousQuestions[i] = newQuestion;
                                });
                                NewQuestions.Remove(newQuestion);
                            }
                            else
                                DBIdSame = false;
                        }

                        if (!DBIdSame) // if the question doesn't exist in the new list
                        {
                            // delete it from the database
                            DBHandler.Database.DeleteQuestions(previousQuestions[i]);
                        }

                    }
                }

                // Returns user to front page of LevelEditor and refreshed database
                var stack = this.Navigation.NavigationStack;
                if (stack[stack.Count - 2] is MainPage)
                    ((MainPage)stack[stack.Count - 2]).RefreshDatabase();

                this.Navigation.PopAsync(true);
            }
        }

        /// <summary>
        /// a private AddNewQuestions for when the user creates a brand new question
        /// </summary>
        private void AddNewQuestion()
        {
            this.AddNewQuestion(null);
        }

        /// <summary>
        /// This add New Questions contains parameters for images for when a question contains an image.
        /// </summary>
        /// <param name="question">the Question to answer</param>
        /// <param name="imagePath">the path for the image corrosponding to the question</param>
        /// <param name="answers">the first is the correct answer, the rest are incorrect answers</param>
        public void AddNewQuestion(Question question)
        {
            Frame frame = new Frame() // The frame that holds everything
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
                
            };
            if (question != null)
                frame.StyleId = question.DBId;

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
                Question.Text = question.QuestionText;
            frameStack.Children.Add(Question);

            Entry AnswerCorrect = new Entry // The correct answer
            {
                Placeholder = "Enter correct answer",
                
            };
            if (question != null)
                AnswerCorrect.Text = question.CorrectAnswer;

            frameStack.Children.Add(AnswerCorrect);

            Entry AnswerWrongOne = new Entry // A wrong answer
            {
                Placeholder = "Enter a possible answer",
            };
            if (question != null)
                AnswerWrongOne.Text = question.AnswerOne;

            frameStack.Children.Add(AnswerWrongOne);

            Entry AnswerWrongTwo = new Entry// A wrong answer
            {
                Placeholder = "Enter a possible answer",
            };
            if (question != null)
                AnswerWrongTwo.Text = question.AnswerTwo;
            frameStack.Children.Add(AnswerWrongTwo);

            Entry AnswerWrongThree = new Entry// A wrong answer
            {
                Placeholder = "Enter a possible answer",
            };
            if (question != null)
                AnswerWrongThree.Text = question.AnswerThree;
            frameStack.Children.Add(AnswerWrongThree);


            Button AddImage = new Button(); // The add Image button
            {
                AddImage.Text = "Add Image";
                AddImage.Clicked += new EventHandler(ButtonAddImage_Clicked);
                AddImage.CornerRadius = 20;
                AddImage.IsVisible = false;              
            }

            bool needsPicture = false;
            if (question != null)
                needsPicture = question.NeedsPicture;
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
                image.Source = question.ImagePath;
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