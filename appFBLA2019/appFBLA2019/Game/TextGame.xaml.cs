using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TextGame : ContentPage
	{
        private Level level;
        private Question currentQuestion;
        private string correct;
        private int score;
		public TextGame (Level level)
		{
            this.score = 0;
            this.InitializeComponent();
            this.level = level;

            // Save as reference
            this.currentQuestion = level.GetQuestion();
            
            if (this.currentQuestion != null)
                this.GetNextQuestion(this.currentQuestion);
        }
        
        private async Task GetNextQuestion(Question question)
        {
            if (this.level.QuestionsAvailable)
            {
                this.LabelQuestion.Text = question.QuestionText;
                this.correct = question.CorrectAnswer;
                this.GridEntryObjects.Children.Clear();
                this.QuestionImage.IsEnabled = question.NeedsPicture;
                this.QuestionImage.Source = App.Path + question.PictureName;

                if (question.QuestionType == 0) // If multiple-choice button question
                {
                    int topDimension = (int)Math.Ceiling(Math.Sqrt(question.Answers.Count()));
                    int sideDimension = (int)Math.Ceiling(question.Answers.Count() / (double)topDimension);

                    int currentRow = 0;
                    int currentColumn = 0;
                    int span = 1;
                    for (int i = 0; i < question.Answers.Count(); i++)
                    {
                        string answer = question.Answers[i];
                        Button button = new Button
                        {
                            Text = answer
                        };
                        button.Clicked += async (object sender, EventArgs e) =>
                        {
                            await this.CheckButtonAnswer((sender as Button).Text);
                        };
                        //if there are only 2 answers the buttons are tall
                        if (question.Answers.Count() < 3)
                            span = 2;
                        //column is determined by even / odd status
                        currentColumn = Math.Abs((i % 2) - 1);
                        //row is determined (basically) by being greater than 2
                        currentRow = Math.Max(Math.Sign(i - 2), 0);

                        this.GridEntryObjects.Children.Add(button, currentColumn, currentRow);
                        Grid.SetColumnSpan(button, span);
                    }
                }
                else if (question.QuestionType == 1 || question.QuestionType == 2) // if text response
                {
                    Entry entry = new Entry();
                    Button buttonCheckAnswer = new Button
                    {
                        Text = "Check Answer"
                    };
                    buttonCheckAnswer.Clicked += async (object sender, EventArgs e) =>
                    {
                        // Can we do this with null-conditional operators?
                        if (entry.Text == null)
                            await this.CheckTextAnswer("", (question.QuestionType == 2));
                        else
                            await this.CheckTextAnswer(entry.Text, (question.QuestionType == 2));
                    };
                    this.GridEntryObjects.Children.Add(entry, 0, 0);
                    this.GridEntryObjects.Children.Add(buttonCheckAnswer, 0, 1);

                    Grid.SetColumnSpan(entry, 2);
                    Grid.SetColumnSpan(buttonCheckAnswer, 2);
                }
            }
            else // Finished level
            {
                DBHandler.Database.AddScore(new ScoreRecord(this.score));
                LevelEndPage levelEndPage = (new LevelEndPage(this.score, this.level.Questions.Count));
                levelEndPage.Finished += this.OnFinished;
                await this.Navigation.PushModalAsync(levelEndPage);
            }
        }

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
            this.LabelDebug.Text = "Correct!";
            //add 2 points for getting it right first time, 1 point for getting it right a second time or later
            if (this.currentQuestion.Status == 0)
                this.score += 2;
            else
                this.score++;

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
            await this.GetNextQuestion(this.currentQuestion);
        }

        private async Task IncorrectAnswer(bool isMultipleChoice)
        {
            this.LabelDebug.Text = "Incorrect!";
            // 1 represents 'failed'

            DBHandler.Database.realmDB.Write(() =>
                this.level.Questions[0].Status = 1
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
            await this.GetNextQuestion(this.level.GetQuestion());
        }

        private void ResetButtons()
        {
            // Lists are immutable in a foreach loop in C#.NET
            // Grab reference of object, then remove

            List<Button> toRemove = new List<Button>();
            foreach (Button button in this.GridEntryObjects.Children.ToList())
            {
                this.GridEntryObjects.Children.Remove(button);
            }
            for (int i = 0; i < toRemove.Count; i++)
            {
                this.GridEntryObjects.Children.Remove(toRemove[i]);
            }
        }

        private void ResetTextEntry()
        {
            // Lists are immutable in a foreach loop in C#.NET
            // Grab reference of object, then remove

            // Define List of type View due to multiple types inheriting from view in GridEntryObjects
            List<View> toRemove = new List<View>();
            foreach (View entryObject in this.GridEntryObjects.Children)
            {
                toRemove.Add(entryObject);
            }
            for (int i = 0; i < toRemove.Count; i++)
            {
                this.GridEntryObjects.Children.Remove(toRemove[i]);
            }
        }

        public async void OnFinished(object source, EventArgs args)
        {
            await this.Navigation.PopAsync();
        }
    }
}