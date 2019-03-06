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
            this.Title = quiz.Title;
            this.inGame = false;

            this.LabelQuestion.SizeChanged += (object sender, EventArgs e) => { this.RelativeLayoutImageAnswer.HeightRequest = this.StackLayoutMain.Height - this.LabelQuestion.Height - 75; };
            this.SizeChanged += (object sender, EventArgs e) => { this.RelativeLayoutImageAnswer.HeightRequest = this.StackLayoutMain.Height - this.LabelQuestion.Height - 75; };
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
            base.OnAppearing();
            if (!this.inGame)
            {
                await this.NextBanner.TranslateTo(this.NextBanner.Width * -2, this.Height * 2 / 3, 0);
                await this.ActivityBanner.TranslateTo(this.ActivityBanner.Width * -2, this.Height * 2 / 3, 0);
                this.inGame = true;
                await this.CycleQuestionAsync();
            }
        }

        private bool inGame;

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
        /// the current score of the user
        /// </summary>
        private int score;

        /// <summary>
        /// brings the Next Question banner to the center of the screen
        /// </summary>
        /// <returns>  </returns>
        private async Task AnimateNextBannerAsync()
        {
            while (this.LabelFeedback.FontSize / 2 * this.LabelFeedback.Text.Length > (this.NextBanner.Width - 5))
            {
                this.LabelFeedback.FontSize--;
            }
            this.NextBanner.ForceLayout();
            await this.NextBanner.TranslateTo((this.Width - this.NextBanner.Width) / 2, this.Height * 2 / 3, 500, Easing.SpringOut);
        }

        /// <summary>
        /// checks the answer for multiple choice type questions
        /// </summary>
        /// <param name="answer"> the string of the button that was pressed </param>
        private async Task CheckButtonAnswerAsync(string answer)
        {
            foreach (View view in this.InputGrid.Children)
            {
                Button button = view as Button;
                if (button != null)
                {
                    button.IsEnabled = false;
                    if (button.Text != answer)
                    {
                        button.BackgroundColor = Color.Accent.AddLuminosity(-.05);
                    }
                }
            }
            if (answer == this.correct)
            {
                //if user answered right, color their selection green
                //(if for some reason this predicate returns null, an imaginary button is created and assigned a color to prevent null issues)
                Button selectedButton = (Button)this.InputGrid.Children?.Where(x => { return x is Button && ((Button)x).Text == answer; })?.First() ?? new Button();
                selectedButton.BackgroundColor = Color.Green;

                await this.CorrectAnswerAsync();
            }
            else
            {
                //if user answered wrong, color their selection red 
                //(if for some reason this predicate returns null, an imaginary button is created and assigned a color to prevent null issues)
                Button selectedButton = (Button)this.InputGrid.Children?.Where(x => {return x is Button && ((Button)x).Text == answer;} )?.First() ?? new Button();
                selectedButton.BackgroundColor = Color.Red;
                await this.IncorrectAnswerAsync();
            }
        }

        /// <summary>
        /// checks the answer for text type questions
        /// </summary>
        /// <param name="answer">    the answer that was typed </param>
        /// <param name="checkCase"> whether or not to check the case </param>
        private async Task CheckTextAnswerAsync(string answer, bool checkCase)
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
                await this.CorrectAnswerAsync();
            }
            else
            {
                await this.IncorrectAnswerAsync();
            }
        }

        /// <summary>
        /// triggered when the answer is correct, modifies score and shows the next question banner
        /// </summary>
        /// <returns> an awaitable task </returns>
        private async Task CorrectAnswerAsync()
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
            Question copyQuestion = new Question(this.currentQuestion)
            {
                Status = 2
            };
            DBHandler.Database.EditQuestion(copyQuestion);
            await this.AnimateNextBannerAsync();
        }

        /// <summary>
        /// The core of the game loop, triggered when the user presses "Next Question". This method will either load the next question or end the game if appropriate
        /// </summary>
        /// <returns>  </returns>
        private async Task CycleQuestionAsync()
        {
            _ = this.ProgressBar.ProgressTo(((double)this.quiz.Questions.Count() - (double)this.quiz.QuestionsRemaining) / (double)this.quiz.Questions.Count(), 500, Easing.SpringOut);
            _ = this.NextBanner.TranslateTo(this.NextBanner.Width * -2, this.Height * 2 / 3, 0);
            if (this.quiz.QuestionsRemaining > 0)
            {
                // Save as reference
                this.ScoreLabel.Text = $"{this.score}/{this.quiz.Questions.Count * 2}";
                this.currentQuestion = this.quiz.GetQuestion();
                await this.SetUpQuestionAsync(this.currentQuestion);
            }
            else // Finished quiz
            {
                QuizEndPage quizEndPage = (new QuizEndPage(this.score, this.quiz.Questions.Count, this.quiz.Title));
                quizEndPage.Finished += this.OnFinished;
                await this.Navigation.PushModalAsync(quizEndPage);
            }
        }

        /// <summary>
        /// triggered when the answer is wrong, modifies score and shows the next question banner
        /// </summary>
        /// <returns>  </returns>
        private async Task IncorrectAnswerAsync()
        {
            this.LabelFeedback.Text = "Incorrect!";
            this.LabelFeedback.TextColor = Color.White;
            this.NextBanner.BackgroundColor = Color.Red;

            // 1 represents 'failed'
            Question copyQuestion = new Question(this.currentQuestion)
            {
                Status = 1
            };
            DBHandler.Database.EditQuestion(copyQuestion);

            await this.AnimateNextBannerAsync();
        }

        /// <summary>
        /// Forces a refresh of the layout
        /// </summary>
        private void LayoutRefresh()
        {
            this.QuestionImage.Aspect = Aspect.AspectFit;
            this.StackLayoutMain.HeightRequest = this.Height;
            this.UpdateChildrenLayout();
            this.ForceLayout();
        }

        /// <summary>
        /// Triggered when the next question button is clicked
        /// </summary>
        /// <param name="sender">  </param>
        /// <param name="e">       </param>
        private async void NextButton_Clicked(object sender, EventArgs e)
        {
            await this.NextBanner.TranslateTo(this.Width + this.NextBanner.Width * 2, this.Height * 2 / 3, 500, Easing.CubicIn);
            await this.CycleQuestionAsync();
        }

        /// <summary>
        /// Sets up the layout and graphics for the next question
        /// </summary>
        /// <param name="question"> the question to be displayed </param>
        private async Task SetUpQuestionAsync(Question question)
        {
            await this.ActivityBanner.TranslateTo((this.Width - this.ActivityBanner.Width) / 2, this.Height * 2 / 3, 500, Easing.CubicOut);
            this.ActivityIndicatorLoading.IsRunning = true;
            if (question.QuestionText == "" || question.CorrectAnswer == "")
            {
                Question copyQuestion = new Question(this.currentQuestion)
                {
                    Status = 3
                };
                DBHandler.Database.EditQuestion(copyQuestion);
                await this.CycleQuestionAsync();
                return;
            }

            this.LabelQuestion.Text = question.QuestionText;
            this.LabelQuestion.VerticalTextAlignment = TextAlignment.Center;
            this.LabelQuestion.VerticalOptions = LayoutOptions.Center;

            this.LabelQuestion.FontSize = 40;
            while (this.LabelQuestion.FontSize / 2 * this.LabelQuestion.Text.Length > (this.Width - 10) * 2.5)
            {
                this.LabelQuestion.FontSize--;
            }
            this.LabelQuestion.LineBreakMode = LineBreakMode.WordWrap;

            this.correct = question.CorrectAnswer;
            List<string> answers = question.Answers;

            this.InputGrid.Children.Clear();

            if (question.QuestionType == 0) // If multiple-choice button question
            {

                //remove empty answers
                answers.RemoveAll(x => x == "");

                //convert answers to be usable by A, B, C, D buttons
                this.ShuffleAndLabel(answers);
                foreach(string answer in answers)
                {
                    if (answer.Contains(this.correct))
                    {
                        this.correct = Char.ToString(answer[0]);
                        break;
                    }
                }

                this.InputGrid.ColumnDefinitions.Clear();
                this.InputGrid.RowDefinitions.Clear();

                //print the possible answers as labels to avoid scaling issues with buttons
                int answerLabelFontSize = 30;
                for (int i = 0; i<answers.Count;i++)
                {
                    if (answers[i] != null)
                    {
                        Label answerLabel = new Label()
                        {
                            Text = answers[i],
                            TextColor = Color.Gray,
                            WidthRequest = this.Width - 10
                        };
                        while (answerLabelFontSize / 2 * answerLabel.Text.Length > (this.Width - 10) * 2)
                        {
                            answerLabelFontSize--;
                        }
                        this.InputGrid.RowDefinitions.Add(new RowDefinition() { Height = Xamarin.Forms.GridLength.Auto });
                        this.InputGrid.Children.Add(answerLabel, 0, i);
                    }
                }
                foreach (Label label in this.InputGrid.Children)
                {
                    label.FontSize = answerLabelFontSize;
                }

                this.InputGrid.RowDefinitions.Add(new RowDefinition() { Height = Xamarin.Forms.GridLength.Star });
                for (int i = 0; i < answers.Count(); i++)
                {
                    string answer = answers[i];
                    Button button = new Button
                    {
                        Text = Char.ToString(answer[0]),
                        CornerRadius = 25,
                        Padding = 5,
                        BackgroundColor = Color.Accent,
                        TextColor = Color.White,
                        MinimumHeightRequest = 50
                    };
                    button.Clicked += async (object sender, EventArgs e) =>
                    {
                        await this.CheckButtonAnswerAsync(((Button)sender).Text);
                    };

                    this.InputGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = Xamarin.Forms.GridLength.Star });
                    this.InputGrid.Children.Add(button, i, this.InputGrid.RowDefinitions.Count - 1);
                }

                foreach(View view in this.InputGrid.Children)
                {
                    if (view is Label)
                        Grid.SetColumnSpan(view, this.InputGrid.ColumnDefinitions.Count);
                }
            }
            else if (question.QuestionType == 1 || question.QuestionType == 2) // if text response
            {
                this.InputGrid.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition() { Height = new GridLength(.75, GridUnitType.Star) },
                new RowDefinition() { Height = new GridLength(1, GridUnitType.Star)}
            };
                this.InputGrid.ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition() { Width = Xamarin.Forms.GridLength.Star } };
                Entry entry = new Entry()
                {
                    FontSize = 35,
                    FontAttributes = FontAttributes.Italic,
                    TextColor = Color.Gray,
                    HorizontalTextAlignment = TextAlignment.Center,
                    WidthRequest = this.Width,
                    Placeholder = "Answer Here",
                    PlaceholderColor = Color.LightGray,
                    Margin = 0,
                };
                entry.HeightRequest = entry.FontSize * 3;
                Button buttonCheckAnswer = new Button
                {
                    Text = "Check Answer",
                    FontSize = 45,
                    CornerRadius = 25,
                    Padding = 10,
                    BackgroundColor = Color.Accent,
                    TextColor = Color.White
                };
                buttonCheckAnswer.Clicked += async (object sender, EventArgs e) =>
                {
                    // Can we do this with null-conditional operators? yes we can
                    await this.CheckTextAnswerAsync(entry.Text ?? "", (question.QuestionType == 2));
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
            await this.ActivityBanner.TranslateTo(this.Width + this.ActivityBanner.Width * 2, this.Height * 2 / 3, 500, Easing.CubicIn);
            await this.ActivityBanner.TranslateTo(this.ActivityBanner.Width * -2, this.Height * 2 / 3, 0);
        }

        /// <summary>
        /// given a list of strings, shuffles it (used for randomizing buttons)
        /// </summary>
        /// <param name="answers"> the list of answers to be shuffled </param>
        private void ShuffleAndLabel(List<String> answers)
        {
            int n = answers.Count;
            while (n > 1)
            {
                n--;
                int k = App.random.Next(n + 1);
                String value = answers[k];
                answers[k] = answers[n];
                answers[n] = value;
            }
            string[] prefixes = {
            "A: ",
            "B: ",
            "C: ",
            "D: ",
            };
            for (int i = 0; i < answers.Count; i++)
            {
                answers[i] = prefixes[i] + answers[i];
            }
        }
    }
}