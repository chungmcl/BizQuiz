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
		}

        private void RunGame()
        {
            while (topic.freshQuestions.Count > 0) //questions remain
            {

            }
        }

        private void FinishGame()
        {
            //save progress
        }

        private void AskQuestion()
        {

        }
	}
}