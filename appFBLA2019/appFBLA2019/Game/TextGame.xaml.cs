//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TextGame : ContentPage
    {
        public TextGame(Level level)
        {
            this.InitializeComponent();
            this.level = level;
            this.score = 0;
            this.random = new Random();

            // Save as reference
            this.currentQuestion = level.GetQuestion();

            if (this.currentQuestion != null)
            {
                this.SetUpNextQuestion(this.currentQuestion);
            }
        }

        public async void OnFinished(object source, EventArgs args)
        {
            this.level.ResetLevel();
            await this.Navigation.PopAsync();
        }

        private string correct;
        private Question currentQuestion;
        private Level level;
        private Random random;
        private int score;

        private void CheckButtonAnswer(string answer)
        {
            if (answer == this.currentQuestion.CorrectAnswer)
            {
                this.CorrectAnswer(true);
            }
            else
            {
                this.IncorrectAnswer(true);
            }
        }

        private void CheckTextAnswer(string answer, bool checkCase)
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
                this.CorrectAnswer(false);
            }
            else
            {
                this.IncorrectAnswer(false);
            }
        }

        private void CorrectAnswer(bool isMultipleChoice)
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
                this.ResetButtons();
            }
            else
            {
                this.ResetTextEntry();
            }

            this.CycleQuestion();
        }

        private void CycleQuestion()
        {
            if (this.level.QuestionsAvailable)
            {
                // Save as reference
                this.currentQuestion = this.level.GetQuestion();
                this.SetUpNextQuestion(this.currentQuestion);
            }
            else // Finished level
            {
                LevelEndPage levelEndPage = (new LevelEndPage(new ScoreRecord(this.score), this.level.Questions.Count));
                levelEndPage.Finished += this.OnFinished;
                this.Navigation.PushModalAsync(levelEndPage);
            }
        }

        private void IncorrectAnswer(bool isMultipleChoice)
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

            this.CycleQuestion();
        }

        private void ResetButtons()
        {
            // Grab reference of object, then remove
            foreach (Button button in this.ButtonGrid.Children.ToArray())
            {
                //button.TranslateTo(button.X, 1000, 10000, Easing.CubicIn);
                this.ButtonGrid.Children.Remove(button);
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

        private void SetUpNextQuestion(Question question)
        {
            if (question.QuestionText == "" || question.CorrectAnswer == "")
            {
                question.Status = 3;
                this.CycleQuestion();
                return;
            }

            this.LabelQuestion.Text = question.QuestionText;
            this.correct = question.CorrectAnswer;
            List<string> answers = question.Answers;

            this.ButtonGrid.Children.Clear();
            this.ButtonGrid.HeightRequest = 320;
            this.ButtonGrid.RowDefinitions = new RowDefinitionCollection
                {
                new RowDefinition() { Height = Xamarin.Forms.GridLength.Star },
                new RowDefinition() { Height = Xamarin.Forms.GridLength.Star }
                };

            //this.QuestionImage.IsEnabled = question.NeedsPicture;
            // The image will ALWAYS be named after the DBId
            this.QuestionImage.Source = ImageSource.FromFile(question.DBId + ".jpg"); // Add cases for all JPG file extensions(for example, ".jpeg")
            this.QuestionImage.Aspect = Aspect.AspectFit;
            this.ProgressBar.ProgressTo(((double)this.level.Questions.Count() - (double)this.level.QuestionsRemaining) / (double)this.level.Questions.Count(), 500, Easing.SpringOut);

            if (question.QuestionType == 0) // If multiple-choice button question
            {
                int currentRow = 0;
                int currentColumn = 0;

                //remove empty answers
                answers.RemoveAll(x => x == "");

                //if there are only 2 answers there are only two rows
                if (answers.Count() < 3)
                {
                    this.ButtonGrid.HeightRequest = 160;
                    this.ButtonGrid.RowDefinitions =
                        new RowDefinitionCollection
                        {
                                new RowDefinition() { Height = Xamarin.Forms.GridLength.Star }
                        };
                }

                this.Shuffle(answers);

                for (int i = 0; i < answers.Count(); i++)
                {
                    string answer = answers[i];
                    Button button = new Button
                    {
                        Text = answer,
                        CornerRadius = 25
                    };

                    button.Clicked += async (object sender, EventArgs e) =>
                    {
                        this.CheckButtonAnswer((sender as Button).Text);
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
                    if (i == 2 && answers.Count() == 3)
                    {
                        Grid.SetColumnSpan(button, 2);
                    }
                    this.StackLayoutMain.ForceLayout();
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
                        this.CheckTextAnswer("", (question.QuestionType == 2));
                    }
                    else
                    {
                        this.CheckTextAnswer(entry.Text, (question.QuestionType == 2));
                    }
                };
                this.ButtonGrid.Children.Add(entry, 0, 0);
                this.ButtonGrid.Children.Add(buttonCheckAnswer, 0, 1);

                Grid.SetColumnSpan(entry, 2);
                Grid.SetColumnSpan(buttonCheckAnswer, 2);
            }
        }

        private void Shuffle(List<String> answers)
        {
            int n = answers.Count;
            while (n > 1)
            {
                n--;
                int k = this.random.Next(n + 1);
                String value = answers[k];
                answers[k] = answers[n];
                answers[n] = value;
            }
        }
    }
}