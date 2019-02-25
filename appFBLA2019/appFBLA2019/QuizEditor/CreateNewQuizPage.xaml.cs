//BizQuiz App 2019

using Plugin.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateNewQuizPage : ContentPage
    {
        private string originalAuthor;
        private string originalName;
        private string originalCategory;

        private ToolbarItem Done = new ToolbarItem
        {
            Icon = "ic_done_white_48dp.png",
            Text = "Done",
        };

        /// <summary>
        /// Constructing from an already existing quiz
        /// </summary>
        /// <param name="originalName">    </param>
        /// <param name="originalAuthor">  </param>
        public CreateNewQuizPage(string originalCategory, string originalName, string originalAuthor)
        {
            this.InitializeComponent();
            this.SetUpBar();
            this.originalCategory = originalCategory;
            this.originalAuthor = originalAuthor;
            this.originalName = originalName;
            this.PickerCategory.SelectedItem = originalCategory;
        }

        /// <summary>
        /// Constructing a brand new quiz
        /// </summary>
        public CreateNewQuizPage()
        {
            this.InitializeComponent();
            this.SetUpBar();
            this.AddNewQuestion();
            this.AddNewQuestion();
        }

        private void SetUpBar()
        {
            this.ToolbarItems.Add(this.Done);
            this.Done.Clicked += this.ButtonCreateQuiz_Clicked;
        }

        private bool canClose = true;

        /// <summary>
        /// Overrides the backbutton to make sure the user really wants to leave
        /// </summary>
        /// <returns>  </returns>
        protected override bool OnBackButtonPressed()
        {
            if (this.canClose)
            {
                this.ShowExitDialogue();
            }

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

        private async Task PickImage(object sender)
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
                    {
                        ((Button)sender).IsVisible = false;
                    }
                }
                else
                {
                    await this.DisplayAlert("Couldn't use Picture", "Pictures must be under 3 MB", "Back");
                }
                file.Dispose();
            }
        }

        private object x;

        /// <summary>
        /// Called when the user presses the Add Image button on a question eiditor
        /// </summary>
        /// <param name="sender">  </param>
        /// <param name="e">       </param>
        private async void ButtonAddImage_Clicked(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                ((Button)sender).IsEnabled = false;
                await this.PickImage(sender);
                ((Button)sender).IsEnabled = true;
            }
            else
            {
                ((ImageButton)sender).IsEnabled = false;
                await this.Navigation.PushAsync(new PhotoPage(((ImageButton)sender)));
                this.x = sender;
                ((ImageButton)sender).IsEnabled = true;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.ButtonAddQuestion.Scale = 0;
            this.ButtonAddDrop.Scale = 0;
            this.ButtonAddQuestion.ScaleTo(1, 250, Easing.CubicInOut);
            this.ButtonAddDrop.ScaleTo(1.3, 250, Easing.CubicInOut);
            if (this.x is ImageButton)
            {
                if (((ImageButton)this.x).StyleId == "change")
                {
                    this.PickImage(this.x);
                }
                else if (((ImageButton)this.x).StyleId == "delete")
                {
                    ((ImageButton)this.x).IsVisible = false;
                    ((StackLayout)((ImageButton)this.x).Parent).Children[7].IsVisible = true;
                }
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.ButtonAddQuestion.ScaleTo(0, 250, Easing.CubicInOut);
            this.ButtonAddDrop.ScaleTo(0, 250, Easing.CubicInOut);
        }

        /// <summary>
        /// Called when the add question button is clicked and adds a new question
        /// </summary>
        /// <param name="sender">  </param>
        /// <param name="e">       </param>
        private async void ButtonAddQuestion_Clicked(object sender, EventArgs e)
        {
            if (this.StackLayoutQuestionStack.Children.Count() <= 100)
            {
                Frame frame = this.AddNewQuestion();
                double x = frame.X;
                frame.TranslationX = this.Width;
                // Scroll to bottom
                this.ScrollViewQuestions.ScrollToAsync(this.stkMain, ScrollToPosition.End, true);

                //animate in frame
                await frame.TranslateTo(x - 10, 0, 500, Easing.CubicOut);
            }
            else
            {
                await this.DisplayAlert("Couldn't add question", "Question limit reached", "Back");
            }
        }

        /// <summary>
        /// Removes the Question Frame when remove button is clicked
        /// </summary>
        /// <param name="sender">  </param>
        /// <param name="e">       </param>
        private async void ButtonRemove_Clicked(object sender, EventArgs e)
        {
            bool answer = await this.DisplayAlert("Warning", "Are you sure you would like to delete this question?", "Yes", "No");
            if (answer == true)
            {
                //this.StackLayoutQuestionStack.Children.Remove((((Frame)((StackLayout)((ImageButton)sender).Parent).Parent))); // Removes the question
                Frame frame = (Frame)((StackLayout)((StackLayout)((ImageButton)sender).Parent).Parent).Parent;
                //Animate A deletion
                await frame.TranslateTo(-this.Width, 0, 500, Easing.CubicInOut);
                // There has to be a better way to do this, it looks very rough. but hours have been spent on trying to make this look good
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
        /// Saves the user created quiz
        /// </summary>
        /// <param name="sender">  </param>
        /// <param name="e">       </param>
        private async void ButtonCreateQuiz_Clicked(object sender, EventArgs e)
        {
            this.Done.IsEnabled = false;
            if (string.IsNullOrWhiteSpace(this.EditorQuizName.Text))
            {
                await this.DisplayAlert("Couldn't Create Quiz", "Please give your quiz a name.", "OK");
            }
            else if (this.StackLayoutQuestionStack.Children.Count < 2)
            {
                await this.DisplayAlert("Couldn't Create Quiz", "Please create at least two questions", "OK");
            }
            else if (this.PickerCategory.SelectedIndex == -1)
            {
                await this.DisplayAlert("Couldn't Create Quiz", "Please give your quiz a category", "OK");
            }
            else
            {
                // Set previousQuestions to the correct previous questions
                List<Question> previousQuestions = new List<Question>(); // A list of questions already in the database

                if (!string.IsNullOrWhiteSpace(this.originalName)) // Edit
                {
                    DBHandler.SelectDatabase(this.originalCategory, this.originalName, this.originalAuthor);
                    previousQuestions = DBHandler.Database.GetQuestions();
                }

                List<Question> NewQuestions = new List<Question>();  // A list of questions the user wants to add to the database

                // Now open the database the user just made, might be the same as the one already open
                DBHandler.SelectDatabase(this.PickerCategory.Items[this.PickerCategory.SelectedIndex], this.EditorQuizName.Text.Trim(), CredentialManager.Username);
                // Loops through each question frame on the screen
                foreach (Frame frame in this.StackLayoutQuestionStack.Children)
                {
                    // A list of all the children of the current frame
                    IList<View> children = ((StackLayout)frame.Content).Children;

                    Question addThis;
                    //The answers to the question
                    string[] answers = {((Editor)children[2]).Text, //Correct answer
								((Editor)children[3]).Text, // Incorect answer
								((Editor)children[4]).Text, // Incorect answer
								((Editor)children[5]).Text}; // Incorect answer

                    // Checks if there is a question set
                    if (string.IsNullOrWhiteSpace(((Editor)children[1]).Text))
                    {
                        await this.DisplayAlert("Couldn't Create Quiz", "Every question must have a question set", "OK");
                        goto exit;
                    }

                    if (((ImageButton)children[6]).IsVisible) // if needs image
                    {
                        addThis = new Question(
                                ((Editor)children[1]).Text, // The Question
                                ((ImageButton)children[6]).Source.ToString().Substring(6), // adds image using the image source
                                answers)
                        { NeedsPicture = true };
                    }
                    else // if not needs picture
                    {
                        addThis = new Question(
                                ((Editor)children[1]).Text,
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
                            {
                                size++;
                            }
                        }
                        if (size < 2 || string.IsNullOrWhiteSpace(answers[0]))
                        {
                            await this.DisplayAlert("Couldn't Create Quiz", "Mulitple choice questions must have a correct answer and at least one wrong answer", "OK");
                            goto exit;
                        }

                        addThis.QuestionType = 0;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(answers[0]))
                        {
                            await this.DisplayAlert("Couldn't Create Quiz", "Text answer questions must have an answer", "OK");
                            goto exit;
                        }

                        if (questionType == "Question Type: Text answer")
                        {
                            addThis.QuestionType = 1;
                        }
                        else
                        {
                            addThis.QuestionType = 2;
                        }
                    }

                    addThis.QuestionId = frame.StyleId; // Set the dbid

                    NewQuestions.Add(addThis);
                }

                // Add if it doesn't already exist, delete if it doesn't exist anymore, update the ones that need to be updated, and do nothing to the others Work in progress, algorithm might be off.
                if (previousQuestions.Count() == 0)
                {
                    // if the user created this for the first time

                    // Save a new QuizInfo into the quiz database, which also adds this QuizInfo to the device quiz roster
                    DBHandler.Database.NewQuizInfo(CredentialManager.Username,
                        this.EditorQuizName.Text.Trim(),
                        this.PickerCategory.Items[this.PickerCategory.SelectedIndex]);
                    DBHandler.Database.AddQuestions(NewQuestions);
                }
                else // edit
                {
                    QuizInfo updatedQuizInfo = new QuizInfo(DBHandler.Database.GetQuizInfo())
                    {
                        QuizName = this.EditorQuizName.Text.Trim(),
                        LastModifiedDate = DateTime.Now.ToString()
                    };
                    DBHandler.Database.EditQuizInfo(updatedQuizInfo);

                    for (int i = 0; i <= previousQuestions.Count() - 1; i++)
                    {
                        bool DBIdSame = true;
                        // test each old question with each new question
                        foreach (Question newQuestion in NewQuestions)
                        {
                            if (previousQuestions[i].QuestionId == newQuestion.QuestionId)
                            {
                                DBIdSame = true;
                                // the same question, but changed, so update
                                DBHandler.Database.EditQuestion(newQuestion);
                                NewQuestions.Remove(newQuestion);
                                break;
                            }
                            else
                            {
                                DBIdSame = false;
                            }
                        }

                        if (!DBIdSame) // if the question doesn't exist in the new list. delete it
                        {
                            DBHandler.Database.DeleteQuestions(previousQuestions[i]);
                        }
                    }

                    // Add all the questions that aren't eddited
                    DBHandler.Database.AddQuestions(NewQuestions);
                }

                File.Create(DBHandler.Database.DBFolderPath + ".nomedia");

                // If they renamed the quiz, delete the old one
                if (this.originalName != this.EditorQuizName.Text.Trim() && this.originalAuthor == CredentialManager.Username)
                {
                    Directory.Delete(App.UserPath + "/" + this.originalName + "`" + this.originalAuthor, true);
                }

                // Returns user to front page of QuizEditor and refreshed database
                await this.Navigation.PopAsync(true);
            }
        exit:;
            this.Done.IsEnabled = true;
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
        /// <param name="question">  the Question to answer </param>
        /// <param name="imagePath"> the path for the image corrosponding to the question </param>
        /// <param name="answers">   the first is the correct answer, the rest are incorrect answers </param>
        public Frame AddNewQuestion(Question question)
        {
            bool isMultipleChoice = true;
            if (question != null)
            {
                isMultipleChoice = question.QuestionType == 0;
            }

            Frame frame = new Frame() // The frame that holds everything
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
            };
            if (question != null)
            {
                frame.StyleId = question.QuestionId;
            }

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
                buttonQuestionType.Clicked += this.OnButtonQuestionTypeClicked;
                buttonQuestionType.HorizontalOptions = LayoutOptions.StartAndExpand;
                buttonQuestionType.VerticalOptions = LayoutOptions.Start;
                buttonQuestionType.BackgroundColor = Color.LightGray;
                buttonQuestionType.TextColor = Color.Black;
                buttonQuestionType.CornerRadius = 25;
            }

            if (question == null || question.QuestionType == 0)
            {
                buttonQuestionType.Text = "Question Type: Multiple choice";
            }
            else if (question.QuestionType == 1)
            {
                buttonQuestionType.Text = "Question Type: Text answer";
            }
            else
            {
                buttonQuestionType.Text = "Question Type: Case sensitive text answer";
            }

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
            Editor EditorQuestion = new Editor // The question
            {
                Placeholder = "Enter question",
                FontSize = 20,
                MaxLength = 150,
                AutoSize = EditorAutoSizeOption.TextChanges
                //ReturnType=ReturnType.Next
            };
            if (question != null)
            {
                EditorQuestion.Text = question.QuestionText;
            }

            frameStack.Children.Add(EditorQuestion);

            // 2
            Editor EditorAnswerCorrect = new Editor // The correct answer
            {
                Placeholder = "Enter correct answer",
                MaxLength = 150,
                AutoSize = EditorAutoSizeOption.TextChanges
                //ReturnType = ReturnType.Next
            };
            if (question != null)
            {
                EditorAnswerCorrect.Text = question.CorrectAnswer;
            }
            frameStack.Children.Add(EditorAnswerCorrect);

            // 3
            Editor EditorAnswerWrongOne = new Editor // A wrong answer
            {
                Placeholder = "Enter a possible answer",
                MaxLength = 150,
                AutoSize = EditorAutoSizeOption.TextChanges
                //ReturnType = ReturnType.Next
            };
            if (question != null)
            {
                EditorAnswerWrongOne.Text = question.AnswerOne;
            }
            frameStack.Children.Add(EditorAnswerWrongOne);

            // 4
            Editor EditorAnswerWrongTwo = new Editor// A wrong answer
            {
                Placeholder = "Enter a possible answer",
                MaxLength = 150,
                AutoSize = EditorAutoSizeOption.TextChanges

                //ReturnType = ReturnType.Next
            };
            if (question != null)
            {
                EditorAnswerWrongTwo.Text = question.AnswerTwo;
            }
            frameStack.Children.Add(EditorAnswerWrongTwo);

            // 5
            Editor EditorAnswerWrongThree = new Editor// A wrong answer
            {
                Placeholder = "Enter a possible answer",
                AutoSize = EditorAutoSizeOption.TextChanges,
                MaxLength = 150,
                //ReturnType = ReturnType.Next,
                VerticalOptions = LayoutOptions.StartAndExpand
            };
            if (question != null)
            {
                EditorAnswerWrongThree.Text = question.AnswerThree;
            }
            frameStack.Children.Add(EditorAnswerWrongThree);

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
            {
                needsPicture = question.NeedsPicture;
            }
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
            {
                AddImage.IsVisible = true;
            }

            //EditorQuestion.ReturnCommand = new Command(() => EditorAnswerCorrect.Focus());
            if (isMultipleChoice)
            {
                //EditorAnswerCorrect.ReturnCommand = new Command(() => EditorAnswerWrongOne.Focus());
                //EditorAnswerWrongOne.ReturnCommand = new Command(() => EditorAnswerWrongTwo.Focus());
                //EditorAnswerWrongTwo.ReturnCommand = new Command(() => EditorAnswerWrongThree.Focus());
            }
            else
            {
                EditorAnswerWrongOne.Opacity = 0;
                EditorAnswerWrongTwo.Opacity = 0;
                EditorAnswerWrongThree.Opacity = 0;
            }

            // Dissable extra answers if its not mulitple choice
            EditorAnswerWrongOne.IsVisible = isMultipleChoice;
            EditorAnswerWrongTwo.IsVisible = isMultipleChoice;
            EditorAnswerWrongThree.IsVisible = isMultipleChoice;

            frame.Content = frameStack;
            // and add the frame to the the other stacklaout.
            this.StackLayoutQuestionStack.Children.Add(frame);

            return frame;
        }

        /// <summary>
        /// When the user clicks the button to change question type: changes to the next question type
        /// </summary>
        /// <param name="sender">  </param>
        /// <param name="e">       </param>
        private async void OnButtonQuestionTypeClicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            // Disable button so user can't spam -- for some reasong button.IsEnabled doesn't want to work. IsVisible makes the X jump location. Translating it away works just fine (Although it seems impractical). Note: This should probably be figured out.
            button.TranslationX += this.Width * 2;
            await button.FadeTo(0, 150, Easing.CubicInOut);
            switch (button.Text)
            {
                case "Question Type: Multiple choice":
                {
                    // Do animation stuff - doesn't look the greatest, but the best I can do without downloading customs or doing something too time intesive
                    StackLayout stack = ((StackLayout)((StackLayout)(button).Parent).Parent);
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

                    // Change the button to the next question type and change the return command so users can't access the other editors
                    //((Editor)stack.Children[2]).ReturnCommand = new Command(() => ((Editor)stack.Children[2]).Unfocus());
                    // change question type
                    button.Text = "Question Type: Text answer";
                    break;
                }
                case "Question Type: Text answer":
                    button.Text = "Question Type: Case sensitive text answer";
                    break;

                case "Question Type: Case sensitive text answer":
                {
                    StackLayout stack = ((StackLayout)((StackLayout)(button).Parent).Parent);
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

                    //((Editor)stack.Children[2]).ReturnCommand = new Command(() => ((Editor)stack.Children[2]).Focus());
                    button.Text = "Question Type: Multiple choice";
                    break;
                }
            }

            // Re-enable (Translate back) the button and fade it into view - for some reason this breaks it.
            button.TranslationX -= this.Width * 2;
            await button.FadeTo(1, 150, Easing.CubicInOut);
        }


        public void SetQuizName(string quizName)
        {
            this.EditorQuizName.Text = quizName;
        }
    }
}