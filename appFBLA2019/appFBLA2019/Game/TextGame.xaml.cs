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
        private enum AnswerButton
        {
            optionA,
            optionB,
            optionC,
            optionD
        }
        private Level level;
        private AnswerButton correctAnswer;
        private Question currentQuestion;
		public TextGame (Level level)
		{
            this.InitializeComponent();
            this.level = level;

            // Save as reference
            this.currentQuestion = level.GetQuestion();
            this.GetNextQuestion(currentQuestion);
        }
        
        private void GetNextQuestion(Question question)
        {
            if (this.level.QuestionsAvailable)
            {
                this.LabelQuestion.Text = question.QuestionText;
                for (int i = 0; i < question.Answers.Count(); i++)
                {
                    string answer = question.Answers[i];
                    if (answer == question.CorrectAnswer)
                    {
                        this.correctAnswer = (AnswerButton)(i);
                    }
                    this.GridButtons.Children[i].IsEnabled = true;
                    ((Button)this.GridButtons.Children[i]).Text = answer;
                }
            }
            else
            {
                //level.SaveState();
            }
        }

        private void ButtonOptionA_Clicked(object sender, EventArgs e)
        {
            this.CheckAnswer(AnswerButton.optionA);
        }

        private void ButtonOptionB_Clicked(object sender, EventArgs e)
        {
            this.CheckAnswer(AnswerButton.optionB);
        }

        private void ButtonOptionC_Clicked(object sender, EventArgs e)
        {
            this.CheckAnswer(AnswerButton.optionC);
        }

        private void ButtonOptionD_Clicked(object sender, EventArgs e)
        {
            this.CheckAnswer(AnswerButton.optionD);
        }

        private void CheckAnswer(AnswerButton answer)
        {
            if (answer == this.correctAnswer)
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