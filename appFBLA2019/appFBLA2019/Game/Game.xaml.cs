﻿//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    /// <summary>
    /// The core of the game functionality
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Game : ContentPage
    {
        /// <summary>
        /// creates the game, sets up the quiz and begins the game loop
        /// </summary>
        /// <param name="quiz">  </param>
        public Game(Quiz quiz)
        {
            this.InitializeComponent();
            this.quiz = quiz;
            this.score = 0;
            this.random = new Random();
            this.Title = quiz.Title;

            this.LayoutRefresh();
        }

        /// <summary>
        /// Triggered when the endquizpage closes, resets the quiz and returns the user to the mainpage
        /// </summary>
        /// <param name="source">  </param>
        /// <param name="args">    </param>
        public async void OnFinished(object source, EventArgs args)
        {
            this.quiz.ResetQuiz();
            await this.Navigation.PopAsync();
        }

        /// <summary>
        /// triggered right before the page appears, sets the next question banner to offscreen and updates layout
        /// </summary>
        protected override async void OnAppearing()
        {
            await this.NextBanner.TranslateTo(this.NextBanner.Width * -2, this.Height * 2 / 3, 0);
            this.LayoutRefresh();
            await this.CycleQuestion();
        }

        /// <summary>
        /// the correct answer
        /// </summary>
        private string correct;

        /// <summary>
        /// the current question
        /// </summary>
        private Question currentQuestion;

        /// <summary>
        /// the current quiz
        /// </summary>
        private Quiz quiz;

        /// <summary>
        /// a random to be used for shuffling
        /// </summary>
        private Random random;

        /// <summary>
        /// the current score of the user
        /// </summary>
        private int score;

        /// <summary>
        /// brings the Next Question banner to the center of the screen
        /// </summary>
        /// <returns>  </returns>
        private async Task AnimateNextBanner()
        {
            while (this.LabelFeedback.FontSize / 2 * LabelFeedback.Text.Length > (NextBanner.Width - 5))
            {
                this.LabelFeedback.FontSize--;
            }
            this.NextBanner.ForceLayout();
            await this.NextBanner.TranslateTo((this.Width - 300) / 2, this.Height * 2 / 3, 500, Easing.SpringOut);
        }

        /// <summary>
        /// checks the answer for multiple choice type questions
        /// </summary>
        /// <param name="answer"> the string of the button that was pressed </param>
        private async Task CheckButtonAnswer(string answer)
        {
            foreach (Button button in this.InputGrid.Children)
            {
                button.IsEnabled = false;
                if (button.Text != answer)
                {
                    button.BackgroundColor = Color.Accent.AddLuminosity(-.05);
                }
            }
            if (answer == this.currentQuestion.CorrectAnswer)
            {
                foreach (View button in this.InputGrid.Children.Where(x => (x as Button).Text == this.currentQuestion.CorrectAnswer))
                {
                    (button as Button).BackgroundColor = Color.Green;
                }

                await this.CorrectAnswer();
            }
            else
            {
                ((Button)this.InputGrid.Children.Where(x => (x as Button).Text == answer).First()).BackgroundColor = Color.Red;

                await this.IncorrectAnswer();
            }
        }

        /// <summary>
        /// checks the answer for text type questions
        /// </summary>
        /// <param name="answer">    the answer that was typed </param>
        /// <param name="checkCase"> whether or not to check the case </param>
        private async Task CheckTextAnswer(string answer, bool checkCase)
        {
            ((Button)this.InputGrid.Children.Where(x => x.GetType() == typeof(Button)).First()).IsEnabled = false;
            answer = answer.Trim();
            string correctAnswer = this.currentQuestion.CorrectAnswer;
            if (!checkCase)
            {
                answer = answer.ToLower();
                correctAnswer = correctAnswer.ToLower();
            }

            if (answer == correctAnswer)
            {
                await this.CorrectAnswer();
            }
            else
            {
                await this.IncorrectAnswer();
            }
        }

        /// <summary>
        /// triggered when the answer is correct, modifies score and shows the next question banner
        /// </summary>
        /// <returns> an awaitable task </returns>
        private async Task CorrectAnswer()
        {
            this.LabelFeedback.Text = "Correct!";
            this.LabelFeedback.FontSize = 40;
            this.NextBanner.BackgroundColor = Color.Green;
            this.LabelFeedback.TextColor = Color.White;
            //add 2 points for getting it right first time, 1 point for getting it right a second time or later
            if (this.currentQuestion.Status == 0)
            {
                this.score += 2;
                this.LabelFeedback.Text += " First try! + 2 points";
            }
            else
            {
                this.score++;
                this.LabelFeedback.Text += " + 1 point";
            }

            // 2 represents 'correct'
            Question copyQuestion = new Question(this.currentQuestion);
            copyQuestion.Status = 2;
            DBHandler.Database.EditQuestion(copyQuestion);
            await this.AnimateNextBanner();
        }

        /// <summary>
        /// The core of the game loop, triggered when the user presses "Next Question". This method will either load the next question or end the game if appropriate
        /// </summary>
        /// <returns>  </returns>
        private async Task CycleQuestion()
        {
            await this.ProgressBar.ProgressTo(((double)this.quiz.Questions.Count() - (double)this.quiz.QuestionsRemaining) / (double)this.quiz.Questions.Count(), 500, Easing.SpringOut);
            await this.NextBanner.TranslateTo(this.NextBanner.Width * -2, this.Height * 2 / 3, 500);
            if (this.quiz.QuestionsRemaining > 0)
            {
                // Save as reference
                this.ScoreLabel.Text = $"{this.score}/{this.quiz.Questions.Count * 2}";
                this.currentQuestion = this.quiz.GetQuestion();
                await this.SetUpQuestion(this.currentQuestion);
            }
            else // Finished quiz
            {
                QuizEndPage quizEndPage = (new QuizEndPage(this.score, this.quiz.Questions.Count));
                quizEndPage.Finished += this.OnFinished;
                await this.Navigation.PushModalAsync(quizEndPage);
            }
        }

        /// <summary>
        /// triggered when the answer is wrong, modifies score and shows the next question banner
        /// </summary>
        /// <returns>  </returns>
        private async Task IncorrectAnswer()
        {
            this.LabelFeedback.Text = "Incorrect!";
            this.LabelFeedback.TextColor = Color.White;
            this.NextBanner.BackgroundColor = Color.Red;

            // 1 represents 'failed'
            Question copyQuestion = new Question(this.currentQuestion);
            copyQuestion.Status = 1;
            DBHandler.Database.EditQuestion(copyQuestion);

            await this.AnimateNextBanner();
        }

        /// <summary>
        /// Forces a refresh of the layout
        /// </summary>
        private void LayoutRefresh()
        {
            this.ActivityIndicatorLoading.IsVisible = true;
            this.ActivityIndicatorLoading.IsRunning = true;
            this.QuestionImage.Aspect = Aspect.AspectFit;
            this.UpdateChildrenLayout();
            this.ForceLayout();
            this.ActivityIndicatorLoading.IsVisible = false;
            this.ActivityIndicatorLoading.IsRunning = false;
        }

        /// <summary>
        /// Triggered when the next question button is clicked
        /// </summary>
        /// <param name="sender">  </param>
        /// <param name="e">       </param>
        private async void NextButton_Clicked(object sender, EventArgs e)
        {
            await this.CycleQuestion();
        }

        /// <summary>
        /// Sets up the layout and graphics for the next question
        /// </summary>
        /// <param name="question"> the question to be displayed </param>
        private async Task SetUpQuestion(Question question)
        {
            if (question.QuestionText == "" || question.CorrectAnswer == "")
            {
                Question copyQuestion = new Question(this.currentQuestion);
                copyQuestion.Status = 3;
                DBHandler.Database.EditQuestion(copyQuestion);
                await this.CycleQuestion();
                return;
            }

            int fontSize = 30;
            this.LabelQuestion.Text = question.QuestionText;
            this.LabelQuestion.VerticalTextAlignment = TextAlignment.Center;
            this.LabelQuestion.VerticalOptions = LayoutOptions.Center;
            this.correct = question.CorrectAnswer;
            List<string> answers = question.Answers;

            this.InputGrid.Children.Clear();
            this.InputGrid.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition() { Height = Xamarin.Forms.GridLength.Star },
                new RowDefinition() { Height = Xamarin.Forms.GridLength.Star }
            };

            if (question.QuestionType == 0) // If multiple-choice button question
            {
                int currentRow = 0;
                int currentColumn = 0;

                //remove empty answers
                answers.RemoveAll(x => x == "");

                //if there are only 2 answers there are only two rows
                if (answers.Count() < 3)
                {
                    this.InputGrid.RowDefinitions =
                        new RowDefinitionCollection
                        {
                                new RowDefinition() { Height = Xamarin.Forms.GridLength.Star }
                        };
                }
                this.Shuffle(answers);

                for (int i = 0; i < answers.Count(); i++)
                {
                    string answer = answers[i];
                    Button button = new Button
                    {
                        Text = answer,
                        CornerRadius = 25,
                        Padding = 10,
                        BackgroundColor = Color.Accent,
                        TextColor = Color.White,
                        VerticalOptions = LayoutOptions.FillAndExpand
                    };
                    button.Clicked += async(object sender, EventArgs e) =>
                    {
                        await this.CheckButtonAnswer(((Button)sender).Text);
                    };

                    this.InputGrid.Children.Add(button, 0, i);
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

                    this.InputGrid.Children.Add(button, currentColumn, currentRow);
                    if (i == 2 && answers.Count() == 3)
                    {
                        Grid.SetColumnSpan(button, 2);
                    }
                    while (fontSize / 2 * button.Text.Length > (button.Width - 10) * 3.5)
                    {
                        double testSize = fontSize / 2 * button.Text.Length ;
                        double buttonWidth = (button.Width - 10) * 3.5;
                        fontSize--;
                    }
                }
                foreach(View button in this.InputGrid.Children)
                {
                    (button as Button).FontSize = fontSize;
                }
            }
            else if (question.QuestionType == 1 || question.QuestionType == 2) // if text response
            {
                Entry entry = new Entry()
                {
                    FontSize = 35,
                    FontAttributes = FontAttributes.Italic,
                    TextColor = Color.Gray, 
                    HorizontalTextAlignment = TextAlignment.Center,
                    WidthRequest = this.Width,
                    Placeholder = "Answer Here",
                    PlaceholderColor = Color.LightGray
                }
                ;
                Button buttonCheckAnswer = new Button
                {
                    Text = "Check Answer",
                    FontSize = 45,
                    CornerRadius = 25,
                    Padding = 10,
                    BackgroundColor = Color.Accent,
                    TextColor = Color.White
                };
                buttonCheckAnswer.Clicked += async(object sender, EventArgs e) =>
                {
                    // Can we do this with null-conditional operators? yes we can
                    await this.CheckTextAnswer(entry.Text ?? "", (question.QuestionType == 2));
                };
                this.InputGrid.Children.Add(entry, 0, 0);
                this.InputGrid.Children.Add(buttonCheckAnswer, 0, 1);

                Grid.SetColumnSpan(entry, 2);
                Grid.SetColumnSpan(buttonCheckAnswer, 2);
            }

            this.QuestionImage.Source = ImageSource.FromFile(question.ImagePath); // Add cases for all JPG file extensions(for example, ".jpeg")
            this.QuestionImage.IsEnabled = question.NeedsPicture;

            // The image will ALWAYS be named after the DBId

            this.LayoutRefresh();
        }

        /// <summary>
        /// given a list of strings, shuffles it (used for randomizing buttons)
        /// </summary>
        /// <param name="answers"> the list of answers to be shuffled </param>
        private void Shuffle(List<String> answers)
        {
            int n = answers.Count;
            while (n > 1)
            {
                n--;
                int k = this.random.Next(n + 1);
                String value = answers[k];
                answers[k] = answers[n];
                answers[n] = value;
            }
        }
    }
}