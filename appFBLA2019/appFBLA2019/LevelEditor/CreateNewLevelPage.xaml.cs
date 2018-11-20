﻿using System;
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
        private int FrameIDCounter;
		public CreateNewLevelPage ()
		{           
			InitializeComponent ();
            this.FrameIDCounter = 0;
            AddNewQuestion();
        }

        private void AddNewQuestion()
        {
            Frame frame = new Frame()
            {
                ClassId = (this.FrameIDCounter++).ToString(),
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10,
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

            SwipeGestureRecognizer swipe = new SwipeGestureRecognizer();
            swipe.Direction = SwipeDirection.Left;
            swipe.Swiped += (object sender, SwipedEventArgs e) =>
            {
                // "sender" is a reference to the object that was swiped
                // "sender" was tested and it returned the correct Frame
                // It is throwing an exception when removed, I don't know why
                this.StackLayoutQuestionStack.Children.Remove(((Frame)sender));
            };

            frame.GestureRecognizers.Add(swipe);
            frame.Content = frameStack;
            this.StackLayoutQuestionStack.Children.Add(frame);
        }

        private void ButtonAddQuestion_Clicked(object sender, EventArgs e)
        {
            AddNewQuestion();
        }

        private void ButtonCreateLevel_Clicked(object sender, EventArgs e)
        {
            List<Question> toAdd = new List<Question>();
            foreach (Frame frame in this.StackLayoutQuestionStack.Children)
            {
                string question = "";
                string answerCorrect = "";
                string answerOne = "";
                string answerTwo = "";
                string answerThree = "";
                int counter = 0;
                foreach (Entry entry in ((StackLayout)(frame.Content)).Children)
                {
                    switch (counter)
                    {
                        case 0:
                            question = entry.Text;
                            break;
                        case 1:
                            answerCorrect = entry.Text;
                            break;
                        case 2:
                            answerOne = entry.Text;
                            break;
                        case 3:
                            answerTwo = entry.Text;
                            break;
                        case 4:
                            answerThree = entry.Text;
                            break;
                    }
                    counter++;
                }
                toAdd.Add(new Question(question, "c/" + answerCorrect,
                    "x/" + answerOne,
                    "x/" + answerTwo,
                    "x/" + answerThree));
            }
            DBHandler.SelectDatabase(this.EntryLevelName.Text);
            DBHandler.Database.AddQuestions(toAdd);
        }
    }
}