//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TextGame : ContentPage
    {
        public TextGame(Level level)
        {
            this.InitializeComponent();
            this.level = level;
            this.score = 0;
            this.random = new Random();

            this.LayoutRefresh();
            this.CycleQuestion();
        }

        public async void OnFinished(object source, EventArgs args)
        {
            this.level.ResetLevel();
            await this.Navigation.PopAsync();
        }

        protected override async void OnAppearing()
        {
            await this.NextBanner.TranslateTo(this.NextBanner.Width * -2, this.Height * 2 / 3, 200);
            this.LayoutRefresh();
        }


        private void LayoutRefresh()
        {
            this.StackLayoutMain.WidthRequest = this.RelativeLayout.Width;
            this.StackLayoutMain.HeightRequest = this.RelativeLayout.Height;

            this.QuestionImage.Aspect = Aspect.AspectFit;
            if (!this.QuestionImage.IsEnabled)
            {
                this.LayoutGrid.RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition() { Height = Xamarin.Forms.GridLength.Star }
                };
            }
            else
            {
                //this.QuestionImage.HeightRequest = this.StackLayoutMain.Height * 1 / 3;
                this.LayoutGrid.RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition() { Height = Xamarin.Forms.GridLength.Auto },
                    new RowDefinition() { Height = Xamarin.Forms.GridLength.Star }
                };
            }
            this.LayoutGrid.HeightRequest = this.StackLayoutMain.Height;

            this.UpdateChildrenLayout();
            this.ForceLayout();
        }

        private string correct;
        private Question currentQuestion;
        private Level level;
        private Random random;
        private int score;

        private async Task AnimateNextBanner()
        {
            await this.NextBanner.TranslateTo((this.Width - this.NextBanner.Width) / 2, this.Height * 2 / 3, 500, Easing.BounceOut);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            this.CycleQuestion();
        }

        private async void CheckButtonAnswer(string answer)
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
                ((Button)this.InputGrid.Children.Where(x => (x as Button).Text == this.currentQuestion.CorrectAnswer).First()).BackgroundColor = Color.Green;

                await this.CorrectAnswer();
            }
            else
            {
                ((Button)this.InputGrid.Children.Where(x => (x as Button).Text == answer).First()).BackgroundColor = Color.Red;

                await this.IncorrectAnswer();
            }
        }

        private void CheckTextAnswer(string answer, bool checkCase)
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
                this.CorrectAnswer();
            }
            else
            {
                this.IncorrectAnswer();
            }
        }

        private async Task CorrectAnswer()
        {
            this.LabelFeedback.Text = "Correct!";
            this.NextBanner.BackgroundColor = Color.Green;
            this.LabelFeedback.TextColor = Color.White;
            //add 2 points for getting it right first time, 1 point for getting it right a second time or later
            if (this.currentQuestion.Status == 0)
            {
                this.score += 2; this.LabelFeedback.Text += " First try!";
            }
            else
            {
                this.score++;
            }

            // 2 represents 'correct'
            DBHandler.Database.realmDB.Write(() =>
                this.currentQuestion.Status = 2
            );

            await this.AnimateNextBanner();
        }

        private async Task CycleQuestion()
        {
            await this.NextBanner.TranslateTo(this.NextBanner.Width * -2, this.Height * 2 / 3, 0);
            if (this.level.QuestionsAvailable)
            {
                // Save as reference
                this.currentQuestion = this.level.GetQuestion();
                this.SetUpNextQuestion(this.currentQuestion);
            }
            else // Finished level
            {
                LevelEndPage levelEndPage = (new LevelEndPage(this.score, this.level.Questions.Count));
                levelEndPage.Finished += this.OnFinished;
                await this.Navigation.PushModalAsync(levelEndPage);
            }
        }

        private async Task IncorrectAnswer()
        {
            this.LabelFeedback.Text = "Incorrect!";
            this.LabelFeedback.TextColor = Color.White;
            this.NextBanner.BackgroundColor = Color.Red;

            // 1 represents 'failed'
            DBHandler.Database.realmDB.Write(() =>
                this.level.Questions[0].Status = 1
            );

            await this.AnimateNextBanner();
        }

        private void SetUpNextQuestion(Question question)
        {
            if (question.QuestionText == "" || question.CorrectAnswer == "")
            {
                DBHandler.Database.realmDB.Write(() =>
                {
                    question.Status = 3;
                });
                this.CycleQuestion();
                return;
            }

            this.LabelQuestion.Text = question.QuestionText;
            this.correct = question.CorrectAnswer;
            List<string> answers = question.Answers;

            this.InputGrid.Children.Clear();
            this.InputGrid.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition() { Height = Xamarin.Forms.GridLength.Star },
                new RowDefinition() { Height = Xamarin.Forms.GridLength.Star }
            };

            this.ProgressBar.ProgressTo(((double)this.level.Questions.Count() - (double)this.level.QuestionsRemaining) / (double)this.level.Questions.Count(), 500, Easing.SpringOut);

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
                        FontSize = 45,
                        CornerRadius = 25,
                        Padding = 10,
                        BackgroundColor = Color.Accent,
                        TextColor = Color.White,
                        VerticalOptions = LayoutOptions.FillAndExpand
                    };

                    button.Clicked += (object sender, EventArgs e) =>
                    {
                        this.CheckButtonAnswer(((Button)sender).Text);
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
                }
            }
            else if (question.QuestionType == 1 || question.QuestionType == 2) // if text response
            {
                this.InputGrid.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition() { Height = Xamarin.Forms.GridLength.Star},
                new RowDefinition() { Height = Xamarin.Forms.GridLength.Star }
            };
                Entry entry = new Entry()
                {
                    FontSize = 35,
                    FontAttributes = FontAttributes.Italic,
                    TextColor = Color.Gray,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.End,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
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
                    TextColor = Color.White,
                    HeightRequest = 100
                };
                buttonCheckAnswer.Clicked += (object sender, EventArgs e) =>
                {
                    // Can we do this with null-conditional operators? yes we can
                    this.CheckTextAnswer(entry.Text ?? "", (question.QuestionType == 2));
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