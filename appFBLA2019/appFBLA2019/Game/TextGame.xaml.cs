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
        private Level topic;
		public TextGame (Level topic)
		{
            this.InitializeComponent ();
            this.topic = topic;
            Task.Run(() => this.RunGame());
		}

        private void RunGame()
        {
            while (this.topic.QuestionsAvailable) //questions remain
            {
                Question question = this.topic.GetQuestion();

                Task getAnswer = Task.Run(() => this.GetAnswer(question));
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
            foreach (Button button in this.ButtonGrid.Children)
            {
                button.IsEnabled = false;
            }
            for (int i = 0; i < question.Answers.Count(); i++)
            {
                this.ButtonGrid.Children[i].IsEnabled = true;
                ((Button)this.ButtonGrid.Children[i]).Text = question.Answers[i];
            }
            return true;
        }
	}
}