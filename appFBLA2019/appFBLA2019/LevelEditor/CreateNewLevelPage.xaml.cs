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
            this.AddNewQuestion();
        }

        private void AddNewQuestion()
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

            Button Remove = new Button();
            {
                Remove.Text = "Remove Question";
                Remove.Clicked += new EventHandler(ButtonRemove_Clicked);
            }
            frameStack.Children.Add(Remove);

            Entry Question = new Entry
            {
                Placeholder = "Enter question",
            };
            frameStack.Children.Add(Question);

            Entry AnswerCorrect = new Entry
            {
                Placeholder = "Enter correct answer"
            };
            frameStack.Children.Add(AnswerCorrect);

            Entry AnswerWrongOne = new Entry
            {
                Placeholder = "Enter a possible answer"
            };
            frameStack.Children.Add(AnswerWrongOne);

            Entry AnswerWrongTwo = new Entry
            {
                Placeholder = "Enter a possible answer"
            };
            frameStack.Children.Add(AnswerWrongTwo);

            Entry AnswerWrongThree = new Entry
            {
                Placeholder = "Enter a possible answer"
            };
            frameStack.Children.Add(AnswerWrongThree);      

            Button AddImage = new Button();
            {
                AddImage.Text = "Add Image";
                AddImage.Clicked += new EventHandler(ButtonAddImage_Clicked);

            }

            Image image = new Image
            {
                IsEnabled = false,
            };
            frameStack.Children.Add(image);
            frameStack.Children.Add(AddImage);

            frame.Content = frameStack;
            this.StackLayoutQuestionStack.Children.Add(frame);
        }

        // Called when the user presses the Add Image button on a question eiditor
        private async void ButtonAddImage_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            Plugin.Media.Abstractions.MediaFile file = await CrossMedia.Current.PickPhotoAsync();

            //Stream stream = file.GetStream();

            //MemoryStream ms = new MemoryStream();
            //stream.CopyTo(ms);
            //byte[] Image = ms.ToArray();
            
            // Couldn't think of a proper way, but just used the StyleId property to store the file path as a string
            ((Image)StackLayoutQuestionStack.Children[6]).StyleId = file.Path;
            ((Image)StackLayoutQuestionStack.Children[6]).Source = file.Path;
        }

        // Called when the add question button is clicked
        private void ButtonAddQuestion_Clicked(object sender, EventArgs e)
        {
            this.AddNewQuestion();
        }

        private void ButtonRemove_Clicked(object sender, EventArgs e)
        {
            // This just removed the first question, Not the question the button is on.
            this.StackLayoutQuestionStack.Children.RemoveAt(0);
        }

        private void ButtonCreateLevel_Clicked(object sender, EventArgs e)
        {
            // Later, need to impliment username to pass through
            DBHandler.SelectDatabase(this.EntryLevelName.Text, "testAuthor");
            List<Question> questionsToAdd = new List<Question>();  // A list of questions to add to the database \
            // Loops through each question frame on the screen 
            foreach (Frame frame in this.StackLayoutQuestionStack.Children)
            {
                for (int i = 0; i < ((StackLayout)frame.Content).Children.Count; i++)
                {
                    
                    IList<View> children = ((StackLayout)frame.Content).Children;

                    byte[] b = File.ReadAllBytes(((Image)children[6]).StyleId);
                    Question addThis = new Question(
                        ((Entry)children[1]).Text,
                        b,
                        "c/" + ((Entry)children[2]).Text,
                        "x/" + ((Entry)children[3]).Text,
                        "x/" + ((Entry)children[4]).Text,
                        "x/" + ((Entry)children[5]).Text);

                    questionsToAdd.Add(addThis);

                }
            }
            DBHandler.Database.AddQuestions(questionsToAdd);

            // Returns user to front page of LevelEditor
            this.Navigation.PushAsync(new LevelEditorPage());
        }

        public void AddNewQuestion(string question, params string[] answers)
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

            Button Remove = new Button();
            {
                Remove.Text = "Remove Question";
                Remove.Clicked += new EventHandler(ButtonRemove_Clicked);
            }
            frameStack.Children.Add(Remove);

            Entry Question = new Entry
            {
                Placeholder = "Enter question",
                Text = question,
            };
            frameStack.Children.Add(Question);

            Entry AnswerCorrect = new Entry
            {
                Placeholder = "Enter correct answer",
                Text = answers[0],
            };
            frameStack.Children.Add(AnswerCorrect);

            Entry AnswerWrongOne = new Entry
            {
                Placeholder = "Enter a possible answer",
                Text = answers[1],
            };
            frameStack.Children.Add(AnswerWrongOne);

            Entry AnswerWrongTwo = new Entry
            {
                Placeholder = "Enter a possible answer",
                Text = answers[2],
            };
            frameStack.Children.Add(AnswerWrongTwo);

            Entry AnswerWrongThree = new Entry
            {
                Placeholder = "Enter a possible answer",
                Text = answers[3],
            };
            frameStack.Children.Add(AnswerWrongThree);

            Button AddImage = new Button();
            {
                AddImage.Text = "Add Image";
                AddImage.Clicked += new EventHandler(ButtonAddImage_Clicked);

            }

            Image image = new Image
            {
                IsEnabled = false,
            };
            frameStack.Children.Add(image);
            frameStack.Children.Add(AddImage);

            frame.Content = frameStack;
            this.StackLayoutQuestionStack.Children.Add(frame);
        }
    }

    
}