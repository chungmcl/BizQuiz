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
        private Topic topic;
		public TextGame (Topic topic)
		{
			InitializeComponent ();
            this.topic = topic;
            Task.Run(() => RunGame());
		}

        private void RunGame()
        {
            while (topic.freshQuestions.Count > 0 || topic.failedQuestions.Count > 0) //questions remain
            {
                Question question = topic.GetQuestion();

                Task getAnswer = Task.Run(() => GetAnswer(question));
            }
        }

        private void FinishGame()
        {
            //save progress
        }

        private void AskQuestion()
        {

        }

        //display the question and async wait for a reply
        private async Task<bool> GetAnswer(Question question)
        {
            this.Question.Text = question.QuestionText;
            foreach (Button button in ButtonGrid.Children)
            {
                button.IsEnabled = false;
            }
            for (int i = 0; i < question.answers.Count(); i++)
            {
                ButtonGrid.Children[i].IsEnabled = true;
                ((Button)ButtonGrid.Children[i]).Text = question.answers[i];
            }
            return true;
        }
	}
}