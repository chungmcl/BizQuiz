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



        // Called when the user presses the Add Image button on a question eiditor
        private async void ButtonAddImage_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            Plugin.Media.Abstractions.MediaFile file = await CrossMedia.Current.PickPhotoAsync();

;
            // Couldn't think of a proper way, but just used the StyleId property to store the file path as a string
            ((Image)StackLayoutQuestionStack.Children[6]).StyleId = file.Path;
            ((Image)StackLayoutQuestionStack.Children[6]).Source = file.Path;

            // Enables the image
            ((Image)StackLayoutQuestionStack.Children[6]).IsEnabled = true;
        }

        // Called when the add question button is clicked
        private void ButtonAddQuestion_Clicked(object sender, EventArgs e)
        {
            this.AddNewQuestion();
        }

        // Removes the Question Frame
        private void ButtonRemove_Clicked(object sender, EventArgs e)
        {
            this.StackLayoutQuestionStack.Children.Remove((((Frame)((StackLayout)((ImageButton)sender).Parent).Parent)));
        }

        // Creates the Level
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
                            byte[] b = File.ReadAllBytes(((Image)children[6]).StyleId);
                            addThis = new Question(
                                    ((Entry)children[1]).Text,
                                    b, // The image
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

        // This is a private AddNewQuestions for when the user creates a brand new question
        private void AddNewQuestion()
        {
            byte[] noImage = new byte[0];
            AddNewQuestion(null, noImage, null);
        }

        // This AddNewQuestions is for if the question contains no image
        public void AddNewQuestion(string question, params string[] answers)
        {
            AddNewQuestion(question, null, answers);
        }

        // This add New Questions contains parameters for images for when a question contains an image.
        public void AddNewQuestion(string question, byte[] imageAsBytes, params string[] answers)
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
            if (answers != null)
                AnswerCorrect.Text = answers[0];
            frameStack.Children.Add(AnswerCorrect);

            Entry AnswerWrongOne = new Entry
            {
                Placeholder = "Enter a possible answer",
            };
            if (answers != null)
                AnswerWrongOne.Text = answers[1];
            frameStack.Children.Add(AnswerWrongOne);

            Entry AnswerWrongTwo = new Entry
            {
                Placeholder = "Enter a possible answer",
            };
            if (answers != null)
                AnswerWrongTwo.Text = answers[2];
            frameStack.Children.Add(AnswerWrongTwo);

            Entry AnswerWrongThree = new Entry
            {
                Placeholder = "Enter a possible answer",
            };
            if (answers != null)
                AnswerWrongThree.Text = answers[2];
            frameStack.Children.Add(AnswerWrongThree);

            Button AddImage = new Button();
            {
                AddImage.Text = "Add Image";
                AddImage.Clicked += new EventHandler(ButtonAddImage_Clicked);
                AddImage.CornerRadius = 20;
            }

            bool needsPicture = true;
            if (imageAsBytes.Length == 0)
                needsPicture = false;
            Image image = new Image
            {
                IsEnabled = needsPicture,            
            };

            //Gets the imagesource from they byte array
            if (IsEnabled)
            {
                image.Source = ImageSource.FromStream(() => new MemoryStream(imageAsBytes));
                frameStack.Children.Add(image);
            }
            else
                frameStack.Children.Add(AddImage);

            frame.Content = frameStack;
            this.StackLayoutQuestionStack.Children.Add(frame);
        }

        public void SetLevelName(string levelName)
        {
            EntryLevelName.Text = levelName;
        }
    }
}