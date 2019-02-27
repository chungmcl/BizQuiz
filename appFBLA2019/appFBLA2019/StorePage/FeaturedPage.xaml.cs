using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FeaturedPage : ContentPage
	{
        private string category;
        private List<SearchInfo> quizzesFeatured;

        public FeaturedPage()
        {
            InitializeComponent();
            this.currentChunk = 1;
            quizzesFeatured = new List<SearchInfo>();
        }

        protected async override void OnAppearing()
        {
            this.quizzesRemaining = true;
            this.category = "All";
            await this.Refresh();
        }

        private bool quizzesRemaining;
        private int currentChunk;

        private async Task Refresh()
        {
            this.LabelNoQuiz.IsVisible = false;
            this.SearchedStack.Children.Clear();
            this.quizzesFeatured.Clear();
            try
            {
                Device.BeginInvokeOnMainThread(() => {
                    this.SearchedStack.Children.Clear();
                    this.ActivityIndicator.IsVisible = true;
                    this.ActivityIndicator.IsRunning = true;
                });
                await Task.Run(() => this.AddQuizzes(SearchUtils.GetQuizzesByAuthorChunked("BizQuiz", 1)));
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.ActivityIndicator.IsVisible = false;
                    this.ActivityIndicator.IsRunning = false;
                });
            }
            catch (Exception ex)
            {
                BugReportHandler.SaveReport(ex, nameof(FeaturedPage));
                await this.DisplayAlert("Error", "Couldn't get quizzes", "Ok");
            }
        }

        private async Task Search()
        {
            List<SearchInfo> chunk = new List<SearchInfo>();
            chunk = SearchUtils.GetQuizzesByAuthorChunked("BizQuiz", this.currentChunk);
            if (chunk.Count < 20)
                this.quizzesRemaining = false;
            await Task.Run(() => this.AddQuizzes(chunk));
        }

        private async void ScrollSearch_Scrolled(object sender, ScrolledEventArgs e)
        {
            ScrollView scrollView = sender as ScrollView;
            double scrollingSpace = scrollView.ContentSize.Height - scrollView.Height;

            if (scrollingSpace <= e.ScrollY && this.quizzesRemaining)
            {
                try
                {
                    this.currentChunk++;
                    await Task.Run(() => this.Search());
                    this.LabelNoQuiz.IsVisible = false;
                }
                catch
                {
                    await this.DisplayAlert("Error", "Couldn't get quizzes", "Ok");
                }
            }
        }

        /// <summary>
        /// Adds a quiz to the search stack given a QuizInfo
        /// </summary>
        /// <param name="quizzes"></param>
        private void AddQuizzes(List<SearchInfo> quizzes)
        {
            List<QuizInfo> currentlySubscribed = QuizRosterDatabase.GetRoster();
            foreach (SearchInfo quiz in quizzes)
            {
                if (this.category == "All" || quiz.Category == this.category) // Only add quiz if the category is what user picked
                {
                    this.quizzesFeatured.Add(quiz);
                    Frame quizFrame = new Frame
                    {
                        VerticalOptions = LayoutOptions.Start,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        CornerRadius = 10
                    };

                    StackLayout frameStack = new StackLayout
                    {
                        FlowDirection = FlowDirection.LeftToRight,
                        Orientation = StackOrientation.Vertical
                    };

                    StackLayout topStack = new StackLayout
                    {
                        FlowDirection = FlowDirection.LeftToRight,
                        Orientation = StackOrientation.Horizontal
                    };

                    Label quizName = new Label
                    {
                        Text = quiz.QuizName,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                        HorizontalOptions = LayoutOptions.StartAndExpand
                    };
                    topStack.Children.Add(quizName);

                    ImageButton ImageButtonSubscribe = new ImageButton
                    {
                        StyleId = quiz.DBId,
                        HeightRequest = 30,
                        BackgroundColor = Color.White,
                        HorizontalOptions = LayoutOptions.End
                    };

                    // If not already subscribed
                    if (!(currentlySubscribed.Where(quizInfo => quizInfo.DBId == quiz.DBId).Count() > 0))
                    {
                        // source is add if not subscribed and if they are then source is check
                        ImageButtonSubscribe.Source = "ic_playlist_add_black_48dp.png";
                    }
                    else
                    {
                        ImageButtonSubscribe.Source = "ic_playlist_add_check_black_48dp.png";
                    }

                    ImageButtonSubscribe.Clicked += this.ImageButtonSubscribe_Clicked;
                    topStack.Children.Add(ImageButtonSubscribe);

                    frameStack.Children.Add(topStack);

                    Label quizAuthor = new Label
                    {
                        Text = "Created by: " + quiz.Author,
                    };
                    frameStack.Children.Add(quizAuthor);

                    Label quizCategory = new Label
                    {
                        Text = "Category: " + quiz.Category,
                    };
                    frameStack.Children.Add(quizCategory);

                    quizFrame.Content = frameStack;
                    Device.BeginInvokeOnMainThread(() =>
                    this.SearchedStack.Children.Add(quizFrame));
                }
            }
            if (this.quizzesFeatured.Count() == 0)   
            {
                Device.BeginInvokeOnMainThread(() =>
                this.LabelNoQuiz.IsVisible = true);
            }
        }

        /// <summary>
        /// When a user wants to subscribe to a quiz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void ImageButtonSubscribe_Clicked(object sender, EventArgs e)
        {
            ImageButton button = (sender as ImageButton);
            string dbId = button.StyleId;
            if (button.Source.ToString() == "File: ic_playlist_add_check_black_48dp.png") // unsubscribe
            {
                bool answer = await this.DisplayAlert("Are you sure you want to unsubscribe?", "You will no longer get updates of this quiz", "Yes", "No");
                if (answer)
                {
                    await button.FadeTo(1, 150, Easing.CubicInOut);
                    OperationReturnMessage returnMessage = await Task.Run(async() => await ServerOperations.UnsubscribeToQuiz(dbId));
                    if (returnMessage == OperationReturnMessage.True)
                    {
                        await button.FadeTo(0, 150, Easing.CubicInOut);
                        button.Source = "ic_playlist_add_black_48dp.png";
                        button.HeightRequest = 30;
                    }
                    else if (returnMessage == OperationReturnMessage.FalseInvalidCredentials)
                    {
                        await DisplayAlert("Invalid Credentials", "Your current login credentials are invalid. Please try logging in again.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Subscribe Failed", "The subscription request could not be completed. Please try again.", "OK");
                    }
                }
            }
            else // subscribe
            {
                OperationReturnMessage returnMessage = await SubscribeUtils.SubscribeToLevel(dbId, this.quizzesFeatured);
                if (returnMessage == OperationReturnMessage.True)
                {
                    await button.FadeTo(0, 150, Easing.CubicInOut);
                    button.Source = "ic_playlist_add_check_black_48dp.png";
                    button.HeightRequest = 30;
                    await button.FadeTo(1, 150, Easing.CubicInOut);
                }
                else if (returnMessage == OperationReturnMessage.FalseInvalidCredentials)
                {
                    await DisplayAlert("Invalid Credentials", "Your current login credentials are invalid. Please try logging in again.", "OK");
                }
                else
                {
                    await DisplayAlert("Subscribe Failed", "The unsubscription request could not be completed. Please try again.", "OK");
                }
            }

        }

        private void Search_Activated(object sender, EventArgs e)
        {
            this.Navigation.PushModalAsync(new StorePage());
        }

        private async void PickerCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.category = this.PickerCategory.Items[this.PickerCategory.SelectedIndex];
            this.currentChunk = 1;
            await this.Refresh();
        }
    }
}