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
		private string originalAuthor;
		private string originalName;
		private string originalCategory;

        ToolbarItem Done = new ToolbarItem
        {
            Icon = "ic_done_white_48dp.png",
            Text = "Done",
        };
        

		/// <summary>
		/// Constructing from an already existing level
		/// </summary>
		/// <param name="originalName"></param>
		/// <param name="originalAuthor"></param>
		public CreateNewLevelPage(string originalCategory, string originalName, string originalAuthor)
		{
			this.InitializeComponent();
            setUpBar();
            this.originalCategory = originalCategory;
			this.originalAuthor = originalAuthor;
			this.originalName = originalName;
            PickerCategory.SelectedItem = originalCategory;

        }

		/// <summary>
		/// Constructing a brand new level
		/// </summary>
		public CreateNewLevelPage(string Category)
		{
			this.InitializeComponent();
            setUpBar();
            PickerCategory.SelectedItem = Category;
            AddNewQuestion();
			AddNewQuestion();
		}

        private void setUpBar()
        {
            this.ToolbarItems.Add(this.Done);
            Done.Clicked += ButtonCreateLevel_Clicked;
        }

		private bool canClose = true;

		/// <summary>
		/// Overrides the backbutton to make sure the user really wants to leave
		/// </summary>
		/// <returns></returns>
		protected override bool OnBackButtonPressed()
		{
			if (this.canClose)
				this.ShowExitDialogue();
			return this.canClose;
		}

		/// <summary>
		/// Shows the exit dialogue to confirm if the user wants to leave without saving
		/// </summary>
		private async void ShowExitDialogue()
		{
			var answer = await this.DisplayAlert("Exit Creation", "Are you sure you want to leave? Your progress will not be saved", "Yes, leave", "No, keep working");
			if (answer)
			{
				this.canClose = false;
				this.OnBackButtonPressed();
				await this.Navigation.PopAsync(true);
			}
		}

		async private Task PickImage(object sender)
		{
			await CrossMedia.Current.Initialize();
			Plugin.Media.Abstractions.MediaFile file = await CrossMedia.Current.PickPhotoAsync();

			if (file != null) // if the user actually picked an image
			{
				MemoryStream memoryStream = new MemoryStream();
				file.GetStream().CopyTo(memoryStream);    

				if (memoryStream.Length < 3000000)
				{
					ImageButton currentImage;

					currentImage = ((ImageButton)((StackLayout)((View)sender).Parent).Children[6]);


					currentImage.Source = file.Path;

					// Enables the image
					currentImage.IsVisible = true;
					if (sender is Button)
						((Button)sender).IsVisible = false;
				}
				else
				{
					await DisplayAlert("Couldn't use Picture", "Pictures must be under 3 MB", "Back");
				}
				file.Dispose();
			}
		}
		private object x;
		/// <summary>
		/// Called when the user presses the Add Image button on a question eiditor
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ButtonAddImage_Clicked(object sender, EventArgs e)
		{
			if (sender is Button)
				await PickImage(sender);
			else
			{
				await this.Navigation.PushAsync(new LevelEditor.PhotoPage(((ImageButton)sender)));
				x = sender;

			}

		}
		
		protected override void OnAppearing()
		{
			base.OnAppearing();
			ButtonAddQuestion.Scale = 0;
			ButtonAddDrop.Scale = 0;
			ButtonAddQuestion.ScaleTo(1, 250, Easing.CubicInOut);
			ButtonAddDrop.ScaleTo(1.3, 250, Easing.CubicInOut);
			if (x is ImageButton)
			{
				if (((ImageButton)x).StyleId == "change")
					PickImage(x);
				else if (((ImageButton)x).StyleId == "delete")
				{
					((ImageButton)x).IsVisible = false;
					((StackLayout)((ImageButton)x).Parent).Children[7].IsVisible = true;
				}

			}
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			ButtonAddQuestion.ScaleTo(0, 250, Easing.CubicInOut);
			ButtonAddDrop.ScaleTo(0, 250, Easing.CubicInOut);
		}


		/// <summary>
		/// Called when the add question button is clicked and adds a new question
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ButtonAddQuestion_Clicked(object sender, EventArgs e)
		{
			Frame frame = this.AddNewQuestion();
			double x = frame.X;
			frame.TranslationX = this.Width;
			// Scroll to bottom
			this.ScrollViewQuestions.ScrollToAsync(this.stkMain, ScrollToPosition.End, true);

			//animate in frame           
			await frame.TranslateTo(x - 10, 0, 500, Easing.CubicOut);

		}

		/// <summary>
		/// Removes the Question Frame when remove button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		async private void ButtonRemove_Clicked(object sender, EventArgs e)
		{
			bool answer = await this.DisplayAlert("Warning", "Are you sure you would like to delete this question?", "Yes", "No");
			if (answer == true)
			{
				//this.StackLayoutQuestionStack.Children.Remove((((Frame)((StackLayout)((ImageButton)sender).Parent).Parent))); // Removes the question
				Frame frame = (Frame)((StackLayout)((StackLayout)((ImageButton)sender).Parent).Parent).Parent;
				//Animate A deletion
				await frame.TranslateTo(-this.Width, 0, 500, Easing.CubicInOut);
				// There has to be a better way to do this, it looks very rough. 
				// but hours have been spent on trying to make this look good
				IList<View> children = ((StackLayout)frame.Children[0]).Children;
				uint i = 0;
				foreach (View child in children)
				{
					await child.LayoutTo(new Rectangle(child.X, child.Y, child.Width, 0), 20 - i, Easing.CubicInOut);
					child.IsVisible = false;
					i += 2;
				}

				this.StackLayoutQuestionStack.Children.Remove(frame);
			}
		}

		/// <summary>
		/// Saves the user created level
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ButtonCreateLevel_Clicked(object sender, EventArgs e)
		{
            Done.IsEnabled = false;
			if (string.IsNullOrWhiteSpace(this.EntryLevelName.Text))
			{
				await this.DisplayAlert("Couldn't Create Level", "Please give your level a name.", "OK");
			}
			else if (this.StackLayoutQuestionStack.Children.Count < 2)
			{
				await this.DisplayAlert("Couldn't Create Level", "Please create at least two questions", "OK");
			}
			else if (this.PickerCategory.SelectedIndex == -1)
				await this.DisplayAlert("Couldn't Create Level", "Please give your level a category", "OK");
			else
			{
				// Set previousQuestions to the correct previous questions
				List<Question> previousQuestions = new List<Question>(); ; // A list of questions already in the database

				if (!string.IsNullOrWhiteSpace(this.originalName))
				{
					DBHandler.SelectDatabase(this.originalCategory, this.originalName, this.originalAuthor);
					previousQuestions = DBHandler.Database.GetQuestions();
				}

				List<Question> NewQuestions = new List<Question>();  // A list of questions the user wants to add to the database


				// Now open the database the user just made, might be the same as the one already open
				DBHandler.SelectDatabase(this.PickerCategory.Items[this.PickerCategory.SelectedIndex], this.EntryLevelName.Text.Trim(), CredentialManager.Username);
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

					// Checks if there is a question set
					if (string.IsNullOrWhiteSpace(((Entry)children[1]).Text))
					{
						await this.DisplayAlert("Couldn't Create Level", "Every question must have a question set", "OK");
						goto exit;
					}


					if (((ImageButton)children[6]).IsVisible) // if needs image
					{
						addThis = new Question(
								((Entry)children[1]).Text, // The Question
								((ImageButton)children[6]).Source.ToString().Substring(6), // adds image using the image source
								answers)
						{ NeedsPicture = true };
					}
					else // if not needs picture
					{
						addThis = new Question(
								((Entry)children[1]).Text,
								answers);
					}

					string questionType = ((Button)((StackLayout)children[0]).Children[0]).Text;

					// Sets the question type

					if (questionType == "Question Type: Multiple choice")
					{
						int size = 0;
						foreach (string answer in answers)
						{
							if (!string.IsNullOrWhiteSpace(answer))
								size++;
						}
						if (size < 2 || string.IsNullOrWhiteSpace(answers[0]))
						{
							await this.DisplayAlert("Couldn't Create Level", "Mulitple choice questions must have a correct answer and at least one wrong answer", "OK");
							goto exit;
						}

						addThis.QuestionType = 0;
					}
					else
					{
						if (string.IsNullOrWhiteSpace(answers[0]))
						{
							await this.DisplayAlert("Couldn't Create Level", "Text answer questions must have an answer", "OK");
							goto exit;
						}

						if (questionType == "Question Type: Text answer")
							addThis.QuestionType = 1;
						else
							addThis.QuestionType = 2;
					}

					addThis.DBId = frame.StyleId; // Set the dbid

					NewQuestions.Add(addThis);
				}

				// Add if it doesn't already exist,
				// delete if it doesn't exist anymore, 
				// update the ones that need to be updated, 
				// and do nothing to the others
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

							if (previousQuestions[i].DBId == newQuestion.DBId)
							{
								DBIdSame = true;
								// the same question, but changed, so update
								DBHandler.Database.EditQuestion(newQuestion);
								NewQuestions.Remove(newQuestion);
								break;
							}
							else
								DBIdSame = false;
						}

						if (!DBIdSame) // if the question doesn't exist in the new list. delete it
							DBHandler.Database.DeleteQuestions(previousQuestions[i]);

					}

					// Add all the questions that aren't eddited
					DBHandler.Database.AddQuestions(NewQuestions);


				}

				// If they renamed the level, delete the old one
				if (this.originalName != this.EntryLevelName.Text.Trim() && this.originalAuthor == CredentialManager.Username)
				{
					Directory.Delete(App.Path + "/" + this.originalName + "`" + this.originalAuthor, true);
				}

				// Returns user to front page of LevelEditor and refreshed database
				await this.Navigation.PopAsync(true);
                
            }
        exit:;
        Done.IsEnabled = true;

        }

		/// <summary>
		/// a private AddNewQuestions for when the user creates a brand new question
		/// </summary>
		private Frame AddNewQuestion()
		{
			return this.AddNewQuestion(null);
		}

		/// <summary>
		/// This add New Questions contains parameters for images for when a question contains an image.
		/// </summary>
		/// <param name="question">the Question to answer</param>
		/// <param name="imagePath">the path for the image corrosponding to the question</param>
		/// <param name="answers">the first is the correct answer, the rest are incorrect answers</param>
		public Frame AddNewQuestion(Question question)
		{
			bool isMultipleChoice = true;
			if (question != null)
				isMultipleChoice = question.QuestionType == 0;

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

			// 0
			StackLayout topStack = new StackLayout
			{
				FlowDirection = FlowDirection.LeftToRight,
				Orientation = StackOrientation.Horizontal
			};
			frameStack.Children.Add(topStack);


			// 0 - 1
			Button buttonQuestionType = new Button();
			{
				buttonQuestionType.Clicked += OnButtonQuestionTypeClicked;
				buttonQuestionType.HorizontalOptions = LayoutOptions.StartAndExpand;
				buttonQuestionType.VerticalOptions = LayoutOptions.Start;
				buttonQuestionType.BackgroundColor = Color.LightGray;
				buttonQuestionType.TextColor = Color.Black;
				buttonQuestionType.CornerRadius = 25;
			}

			if (question == null || question.QuestionType == 0)
				buttonQuestionType.Text = "Question Type: Multiple choice";
			else if (question.QuestionType == 1)
				buttonQuestionType.Text = "Question Type: Text answer";
			else
				buttonQuestionType.Text = "Question Type: Case sensitive text answer";

			topStack.Children.Add(buttonQuestionType);

			// 0 - 2
			ImageButton Remove = new ImageButton(); // the button to remove the question 
			{
				Remove.Source = "ic_close_black_48dp.png";
				Remove.Clicked += new EventHandler(this.ButtonRemove_Clicked);
				Remove.BackgroundColor = Color.Transparent;
				Remove.HeightRequest = 40;
				Remove.WidthRequest = 40;
				Remove.HorizontalOptions = LayoutOptions.End;
				Remove.VerticalOptions = LayoutOptions.Start;
			}
			topStack.Children.Add(Remove);

			// 1
			Entry entryQuestion = new Entry // The question
			{
				Placeholder = "Enter question",
				FontSize = 20

			};
			if (question != null)
				entryQuestion.Text = question.QuestionText;
			frameStack.Children.Add(entryQuestion);
			entryQuestion.TextChanged += this.OnTextChanged;

			// 2
			Entry entryAnswerCorrect = new Entry // The correct answer
			{
				Placeholder = "Enter correct answer",
			};
			if (question != null)
				entryAnswerCorrect.Text = question.CorrectAnswer;
			entryAnswerCorrect.TextChanged += this.OnTextChanged;
			frameStack.Children.Add(entryAnswerCorrect);

			// 3
			Entry entryAnswerWrongOne = new Entry // A wrong answer
			{
				Placeholder = "Enter a possible answer",
			};
			if (question != null)
				entryAnswerWrongOne.Text = question.AnswerOne;
			entryAnswerWrongOne.TextChanged += this.OnTextChanged;
			frameStack.Children.Add(entryAnswerWrongOne);

			// 4
			Entry entryAnswerWrongTwo = new Entry// A wrong answer
			{
				Placeholder = "Enter a possible answer",
			};
			if (question != null)
				entryAnswerWrongTwo.Text = question.AnswerTwo;
			entryAnswerWrongTwo.TextChanged += this.OnTextChanged;
			frameStack.Children.Add(entryAnswerWrongTwo);

			// 5
			Entry entryAnswerWrongThree = new Entry// A wrong answer
			{
				Placeholder = "Enter a possible answer",
				VerticalOptions = LayoutOptions.StartAndExpand
			};
			if (question != null)
				entryAnswerWrongThree.Text = question.AnswerThree;
			entryAnswerWrongThree.TextChanged += this.OnTextChanged;
			frameStack.Children.Add(entryAnswerWrongThree);

			// 7
			Button AddImage = new Button(); // The add Image button
			{
				AddImage.Text = "Add Image";
				AddImage.Clicked += new EventHandler(this.ButtonAddImage_Clicked);
				AddImage.CornerRadius = 25;
				AddImage.IsVisible = false;
				AddImage.HeightRequest = 50;
				AddImage.BackgroundColor = Color.Accent;
				AddImage.TextColor = Color.White;
				AddImage.VerticalOptions = LayoutOptions.End;
			}

            bool needsPicture = false;
            if (question != null)
                needsPicture = question.NeedsPicture;
            // 6
            ImageButton image = new ImageButton(); // The image itself
            {
                image.IsVisible = needsPicture;
                image.Clicked += new EventHandler(this.ButtonAddImage_Clicked);
                image.BackgroundColor = Color.Transparent;
                image.VerticalOptions = LayoutOptions.End;
                image.HeightRequest = Application.Current.MainPage.Height / 5;
                image.Aspect = Aspect.AspectFit;
                image.HorizontalOptions = LayoutOptions.CenterAndExpand;
            }

			frameStack.Children.Add(image);
			frameStack.Children.Add(AddImage);
			//Gets the image from the imagePath
			if (image.IsVisible)
			{
				image.Source = question.ImagePath;
			}
			else // or adds the add image button
				AddImage.IsVisible = true;


			entryQuestion.ReturnCommand = new Command(() => entryAnswerCorrect.Focus());
			if (isMultipleChoice)
			{
				entryAnswerCorrect.ReturnCommand = new Command(() => entryAnswerWrongOne.Focus());
				entryAnswerWrongOne.ReturnCommand = new Command(() => entryAnswerWrongTwo.Focus());
				entryAnswerWrongTwo.ReturnCommand = new Command(() => entryAnswerWrongThree.Focus());
			}
			else
			{
				entryAnswerWrongOne.Opacity = 0;
				entryAnswerWrongTwo.Opacity = 0;
				entryAnswerWrongThree.Opacity = 0;
			}

			// Dissable extra answers if its not mulitple choice
			entryAnswerWrongOne.IsVisible = isMultipleChoice;
			entryAnswerWrongTwo.IsVisible = isMultipleChoice;
			entryAnswerWrongThree.IsVisible = isMultipleChoice;

			frame.Content = frameStack;
            // and add the frame to the the other stacklaout.
            this.StackLayoutQuestionStack.Children.Add(frame);


            return frame;


		}

		/// <summary>
		/// When the user clicks the button to change question type: changes to the next question type
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		async void OnButtonQuestionTypeClicked(object sender, EventArgs e)
		{
			if (((Button)sender).Text == "Question Type: Multiple choice")
			{
                // Do animation stuff - doesn't look the greatest, but the best I can do without downloading customs or doing something too time intesive
				StackLayout stack = ((StackLayout)((StackLayout)((Button)sender).Parent).Parent);
				Frame frame = ((Frame)stack.Parent);
				await Task.WhenAll(
					stack.Children[3].FadeTo(0, 150, Easing.CubicInOut),
					stack.Children[4].FadeTo(0, 150, Easing.CubicInOut),
					stack.Children[5].FadeTo(0, 150, Easing.CubicInOut),
					frame.LayoutTo(new Rectangle(frame.X, frame.Y, frame.Width, frame.Height - (stack.Children[3].Height + stack.Children[4].Height + stack.Children[5].Height)), 200, Easing.CubicInOut)
					);

				stack.Children[3].IsVisible = false;
				stack.Children[4].IsVisible = false;
				stack.Children[5].IsVisible = false;

                // Change the button to the next question type and change the return command so users can't access the other entries
				((Entry)stack.Children[2]).ReturnCommand = new Command(() => ((Entry)stack.Children[2]).Unfocus());
				((Button)sender).Text = "Question Type: Text answer";
			}
			else if (((Button)sender).Text == "Question Type: Text answer")
			{
				((Button)sender).Text = "Question Type: Case sensitive text answer";
			}
			else
			{
				StackLayout stack = ((StackLayout)((StackLayout)((Button)sender).Parent).Parent);
				Frame frame = ((Frame)stack.Parent);
				stack.Children[3].IsVisible = true;
				stack.Children[4].IsVisible = true;
				stack.Children[5].IsVisible = true;
				await Task.WhenAll(
					stack.Children[3].FadeTo(1, 250, Easing.CubicInOut),
					stack.Children[4].FadeTo(1, 250, Easing.CubicInOut),
					stack.Children[5].FadeTo(1, 250, Easing.CubicInOut),
					frame.LayoutTo(new Rectangle(frame.X, frame.Y, frame.Width,
					frame.Height + (stack.Children[3].Height + stack.Children[4].Height + stack.Children[5].Height)), 200, Easing.CubicInOut)
				);
			   
				((Entry)stack.Children[2]).ReturnCommand = new Command(() => ((Entry)stack.Children[2]).Focus());
				((Button)sender).Text = "Question Type: Multiple choice";
			}
		}

		private const int restrictCount = 64;
        /// <summary>
        /// Sets a limit to how much the user can put in an entry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextChanged(object sender, EventArgs e)
		{
			Entry entry = sender as Entry;
			string val = entry.Text; //Get Current Text

			if (val.Length > restrictCount)//If it is more than your character restriction
			{
				val = val.Remove(restrictCount);// Remove Everything past the restriction length
				entry.Text = val; //Set the Old value
			}
		}

		public void SetLevelName(string levelName)
		{
			this.EntryLevelName.Text = levelName;
		}
	}
}