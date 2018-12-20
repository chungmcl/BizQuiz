using System;
using System.Collections.Generic;
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
                this.GridButtons.Children.Clear();

                if (question.QuestionType == 0) // If multiple-choice button question
                {
                    int topDimension = (int)Math.Ceiling(Math.Sqrt(question.Answers.Count()));
                    int sideDimension = (int)Math.Ceiling(question.Answers.Count() / (double)topDimension);

                    int currentRow = 0;
                    int currentColumn = 0;
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
                        this.GridButtons.Children.Add(button, currentColumn, currentRow);
                    }
                }
                else if (question.QuestionType == 1 || question.QuestionType == 2) // if text response
                {
                    Entry entry = new Entry();
                    this.StackLayoutMain.Children.Add(entry);
                    Button buttonCheckAnswer = new Button
                    {
                        Text = "Check Answer"
                    };
                    this.StackLayoutMain.Children.Add(buttonCheckAnswer);
                    buttonCheckAnswer.Clicked += async (object sender, EventArgs e) =>
                    {
                        await this.CheckTextAnswer(entry.Text, (question.QuestionType == 2));
                    };
                }
            }
            else // Finished level
            {
                DBHandler.Database.AddScore(new ScoreRecord(this.score));
                await this.Navigation.PushModalAsync(new LevelEndPage(this.score, this.level.Questions.Count));
                await this.Navigation.PopAsync();
            }
        }

        private async Task CheckButtonAnswer(string answer)
        {
            if (answer == this.currentQuestion.CorrectAnswer)
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

                // Get the next question
                this.ResetButtons();

                // Save as reference
                this.currentQuestion = this.level.GetQuestion();
                await this.GetNextQuestion(this.currentQuestion);
            }
            else
            {
                this.LabelDebug.Text = "Incorrect!";
                // 1 represents 'failed'

                DBHandler.Database.realmDB.Write(() =>
                    this.level.Questions[0].Status = 1
                );

                // Get the next question
                this.ResetButtons();

                // Save as reference
                this.currentQuestion = this.level.GetQuestion();
                await this.GetNextQuestion(this.level.GetQuestion());
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

            if (answer == this.currentQuestion.CorrectAnswer)
            {

            }
        }

        private void ResetButtons()
        {
            foreach (Button button in this.GridButtons.Children)
            {
                button.IsEnabled = false;
            }
        }
    }
}