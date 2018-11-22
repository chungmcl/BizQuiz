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
	public partial class CreateNewLevelPage : ContentPage
	{
		public CreateNewLevelPage ()
		{           
			this.InitializeComponent ();
            this.AddNewQuestion();
        }

        private void AddNewQuestion()
        {
            Frame frame = new Frame()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10
            };

            StackLayout frameStack = new StackLayout
            {
                FlowDirection = FlowDirection.LeftToRight,
                Orientation = StackOrientation.Vertical
            };

            Entry Question = new Entry
            {
                Placeholder = "Enter question",
            };
            frameStack.Children.Add(Question);

            Entry AnswerCorrect = new Entry
            {
                Placeholder = "Enter correct answer"
            };
            frameStack.Children.Add(AnswerCorrect);

            Entry AnswerWrongOne = new Entry
            {
                Placeholder = "Enter a possible answer"
            };
            frameStack.Children.Add(AnswerWrongOne);

            Entry AnswerWrongTwo = new Entry
            {
                Placeholder = "Enter a possible answer"
            };
            frameStack.Children.Add(AnswerWrongTwo);

            Entry AnswerWrongThree = new Entry
            {
                Placeholder = "Enter a possible answer"
            };
            frameStack.Children.Add(AnswerWrongThree);

            frame.Content = frameStack;
            this.StackLayoutQuestionStack.Children.Add(frame);
        }

        private void ButtonAddQuestion_Clicked(object sender, EventArgs e)
        {
            this.AddNewQuestion();
        }

        private void ButtonCreateLevel_Clicked(object sender, EventArgs e)
        {
            foreach(Frame frame in this.StackLayoutQuestionStack.Children)
            {
                
            }
            DBHandler.SelectDatabase(this.EntryLevelName.Text);
        }
    }
}