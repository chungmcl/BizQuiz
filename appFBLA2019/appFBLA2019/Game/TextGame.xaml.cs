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
		public TextGame (Level level)
		{
            this.InitializeComponent();
            this.level = level;

            // Save as reference
            this.currentQuestion = level.GetQuestion();

            if (currentQuestion != null)
                this.GetNextQuestion(currentQuestion);
        }
        
        private void GetNextQuestion(Question question)
        {
            if (this.level.QuestionsAvailable)
            {
                this.LabelQuestion.Text = question.QuestionText;
                this.correct = question.CorrectAnswer;
                this.GridButtons.Children.Clear();

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
                    button.Clicked += (object sender, EventArgs e) =>
                    {
                        this.CheckAnswer((sender as Button).Text);
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
            else
            {
                level.SaveState();
                // Display a completion page?
            }
        }

        //private void ButtonOptionA_Clicked(object sender, EventArgs e)
        //{
        //    this.CheckAnswer(AnswerButton.optionA);
        //}

        //private void ButtonOptionB_Clicked(object sender, EventArgs e)
        //{
        //    this.CheckAnswer(AnswerButton.optionB);
        //}

        //private void ButtonOptionC_Clicked(object sender, EventArgs e)
        //{
        //    this.CheckAnswer(AnswerButton.optionC);
        //}

        //private void ButtonOptionD_Clicked(object sender, EventArgs e)
        //{
        //    this.CheckAnswer(AnswerButton.optionD);
        //}

        private void CheckAnswer(string answer)
        {
            if (answer == this.currentQuestion.CorrectAnswer)
            {
                this.LabelDebug.Text = "Correct!";
                // 2 represents 'correct'
                this.currentQuestion.Status = 2;

                // Get the next question
                this.ResetButtons();

                // Save as reference
                this.currentQuestion = level.GetQuestion();
                this.GetNextQuestion(currentQuestion);
            }
            else
            {
                this.LabelDebug.Text = "Incorrect!";
                // 1 represents 'failed'
                this.level.Questions[0].Status = 1;

                // Get the next question
                this.ResetButtons();

                // Save as reference
                this.currentQuestion = level.GetQuestion();
                this.GetNextQuestion(this.level.GetQuestion());
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