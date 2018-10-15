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
        private enum answerButton
        {
            optionA,
            optionB,
            optionC,
            optionD
        }
        private Level level;
        private answerButton correctAnswer;
		public TextGame (Level level)
		{
            this.InitializeComponent();
            this.level = level;
            this.GetAnswer(level.GetQuestion());
        }

        private void FinishGame()
        {
            //save progress
            level.SaveState();
        }
        
        private void GetAnswer(Question question)
        {
            this.LabelQuestion.Text = question.QuestionText;
            for (int i = 0; i < question.Answers.Count(); i++)
            {
                string answer = question.Answers[i];
                if (answer == question.CorrectAnswer)
                {
                    correctAnswer = (answerButton)(i);
                }
                this.GridButtons.Children[i].IsEnabled = true;
                ((Button)this.GridButtons.Children[i]).Text = answer;
            }
        }

        private void ButtonOptionA_Clicked(object sender, EventArgs e)
        {
            CheckAnswer(answerButton.optionA);
        }

        private void ButtonOptionB_Clicked(object sender, EventArgs e)
        {
            CheckAnswer(answerButton.optionB);
        }

        private void ButtonOptionC_Clicked(object sender, EventArgs e)
        {
            CheckAnswer(answerButton.optionC);
        }

        private void ButtonOptionD_Clicked(object sender, EventArgs e)
        {
            CheckAnswer(answerButton.optionD);
        }

        private void CheckAnswer(answerButton answer)
        {
            if (answer == correctAnswer)
            {
                this.LabelDebug.Text = "Correct!";
                // 2 represents 'correct'
                level.Questions[0].Status = 2;
                ResetButtons();
                this.GetAnswer(level.GetQuestion());
            }
            else
            {
                this.LabelDebug.Text = "Incorrect!";
                // 1 represents 'failed'
                level.Questions[0].Status = 1;
                ResetButtons();
                this.GetAnswer(level.GetQuestion());
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