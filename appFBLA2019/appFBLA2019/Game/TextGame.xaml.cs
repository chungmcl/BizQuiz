//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TextGame : ContentPage
    {
        #region Public Constructors

        public TextGame(Level level)
        {
            this.score = 0;
            this.InitializeComponent();
            this.level = level;

            // Save as reference
            this.currentQuestion = level.GetQuestion();

            if (this.currentQuestion != null)
            {
                this.SetUpNextQuestion(this.currentQuestion);
            }
        }

        #endregion Public Constructors

        #region Public Methods

        public async void OnFinished(object source, EventArgs args)
        {
            this.level.ResetLevel();
            await this.Navigation.PopAsync();
        }

        #endregion Public Methods

        #region Private Properties + Fields

        private string correct;
        private Question currentQuestion;
        private Level level;
        private int score;

        #endregion Private Properties + Fields

        #region Private Methods

        private async Task CheckButtonAnswer(string answer)
        {
            if (answer == this.currentQuestion.CorrectAnswer)
            {
                await this.CorrectAnswer(true);
            }
            else
            {
                await this.IncorrectAnswer(true);
            }
        }

        private async Task CheckTextAnswer(string answer, bool checkCase)
        {
            answer = answer.Trim();
            string correctAnswer = this.currentQuestion.CorrectAnswer;
            if (!checkCase)
            {
                answer = answer.ToLower();
                correctAnswer = correctAnswer.ToLower();
            }

            if (answer == correctAnswer)
            {
                await this.CorrectAnswer(false);
            }
            else
            {
                await this.IncorrectAnswer(false);
            }
        }

        private async Task CorrectAnswer(bool isMultipleChoice)
        {
            this.LabelFeedback.Text = "Correct!";
            //add 2 points for getting it right first time, 1 point for getting it right a second time or later
            if (this.currentQuestion.Status == 0)
            { this.score += 2; this.LabelFeedback.Text += " First try!"; }
            else
            {
                this.score++;
            }

            // 2 represents 'correct'
            DBHandler.Database.realmDB.Write(() =>
                this.currentQuestion.Status = 2
            );

            if (isMultipleChoice)
            {
                // Get the next question
                this.ResetButtons();
            }
            else
            {
                this.ResetTextEntry();
            }

            // Save as reference
            this.currentQuestion = this.level.GetQuestion();
            await this.SetUpNextQuestion(this.currentQuestion);
        }

        private async Task IncorrectAnswer(bool isMultipleChoice)
        {
            this.LabelFeedback.Text = "Incorrect!";

            // 1 represents 'failed'
            DBHandler.Database.realmDB.Write(() =>
                this.level.Questions[0].Status = 1
            );

            if (isMultipleChoice)
            {
                this.ResetButtons();
            }
            else
            {
                this.ResetTextEntry();
            }

            // Save as reference
            this.currentQuestion = this.level.GetQuestion();
            await this.SetUpNextQuestion(this.level.GetQuestion());
        }

        private void ResetButtons()
        {
            // Grab reference of object, then remove

            List<Button> toRemove = new List<Button>();
            foreach (Button button in this.ButtonGrid.Children.ToList())
            {
                this.ButtonGrid.Children.Remove(button);
            }
            for (int i = 0; i < toRemove.Count; i++)
            {
                this.ButtonGrid.Children.Remove(toRemove[i]);
            }
        }

        private void ResetTextEntry()
        {
            // Grab reference of object, then remove

            // Define List of type View due to multiple types inheriting from view in GridEntryObjects
            List<View> toRemove = new List<View>();
            foreach (View entryObject in this.ButtonGrid.Children)
            {
                toRemove.Add(entryObject);
            }
            for (int i = 0; i < toRemove.Count; i++)
            {
                this.ButtonGrid.Children.Remove(toRemove[i]);
            }
        }

        private async Task SetUpNextQuestion(Question question)
        {
            if (this.level.QuestionsAvailable)
            {
                this.LabelQuestion.Text = question.QuestionText;
                this.correct = question.CorrectAnswer;
                this.ButtonGrid.Children.Clear();
                this.ButtonGrid.HeightRequest = 320;
                this.ButtonGrid.RowDefinitions = new RowDefinitionCollection
                {
                new RowDefinition() { Height = Xamarin.Forms.GridLength.Star },
                new RowDefinition() { Height = Xamarin.Forms.GridLength.Star }
                };
                this.QuestionImage.IsEnabled = question.NeedsPicture;

                // The image will ALWAYS be named after the DBId
                this.QuestionImage.Source = question.DBId + ".jpg"; // Add cases for all JPG file extensions(for example, ".jpeg")
                await this.ProgressBar.ProgressTo((this.level.Questions.Count() - this.level.QuestionsRemaining) / this.level.Questions.Count, 200, Easing.SpringOut);
                if (question.QuestionType == 0) // If multiple-choice button question
                {
                    int currentRow = 0;
                    int currentColumn = 0;
                    int span = 1;
                    //remove empty answers
                    question.Answers.RemoveAll(x => x == "");

                    //if there are only 2 answers there are only two rows
                    if (question.Answers.Count() < 3)
                    {
                        ButtonGrid.HeightRequest = 160;
                        ButtonGrid.RowDefinitions =
                            new RowDefinitionCollection
                {
                new RowDefinition() { Height = Xamarin.Forms.GridLength.Star }
                };
                    }

                    for (int i = 0; i < question.Answers.Count(); i++)
                    {
                        string answer = question.Answers[i];
                        Button button = new Button
                        {
                            Text = answer,
                            CornerRadius = 25
                        };
                        if (answer == "")
                        { button.IsEnabled = false; break; }

                        button.Clicked += async (object sender, EventArgs e) =>
                        {
                            await this.CheckButtonAnswer((sender as Button).Text);
                        };

                        //BROKEN
                        ////2 is a magic number here for the number of columns
                        //currentColumn = Math.Abs((i % 2) - 1);
                        ////row is determined (basically) by being greater than 2
                        //currentRow = Math.Max(Math.Sign(i - 2), 0);

                        //this is gross and messy, need to find a better way to place buttons correctly with math and stuff
                        switch (i)
                        {
                            case 0:
                                currentColumn = 0;
                                currentRow = 0;
                                break;

                            case 1:
                                currentColumn = 1;
                                currentRow = 0;
                                break;

                            case 2:
                                currentColumn = 0;
                                currentRow = 1;
                                break;

                            case 3:
                                currentColumn = 1;
                                currentRow = 1;
                                break;
                        }

                        this.ButtonGrid.Children.Add(button, currentColumn, currentRow);
                        Grid.SetColumnSpan(button, span);
                        if (i == 2 && question.Answers.Count() == 3)
                        {
                            Grid.SetRowSpan(button, 2);
                        }
                    }
                }
                else if (question.QuestionType == 1 || question.QuestionType == 2) // if text response
                {
                    Entry entry = new Entry();
                    Button buttonCheckAnswer = new Button
                    {
                        Text = "Check Answer",
                        CornerRadius = 25
                    };
                    buttonCheckAnswer.Clicked += async (object sender, EventArgs e) =>
                    {
                        // Can we do this with null-conditional operators?
                        if (entry.Text == null)
                        {
                            await this.CheckTextAnswer("", (question.QuestionType == 2));
                        }
                        else
                        {
                            await this.CheckTextAnswer(entry.Text, (question.QuestionType == 2));
                        }
                    };
                    this.ButtonGrid.Children.Add(entry, 0, 0);
                    this.ButtonGrid.Children.Add(buttonCheckAnswer, 0, 1);

                    Grid.SetColumnSpan(entry, 2);
                    Grid.SetColumnSpan(buttonCheckAnswer, 2);
                }
            }
            else // Finished level
            {
                LevelEndPage levelEndPage = (new LevelEndPage(new ScoreRecord(this.score), this.level.Questions.Count));
                levelEndPage.Finished += this.OnFinished;
                await this.Navigation.PushModalAsync(levelEndPage);
            }
        }

        #endregion Private Methods
    }
}