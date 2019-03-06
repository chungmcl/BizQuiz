using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    /// <summary>
    /// The page that displays featured quizzes, Created by the BizQuizTeam. To be displayed in the middle bottom tab
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FeaturedPage : ContentPage
    {
        /// <summary>
        /// the category filter
        /// </summary>
        private string category;

        /// <summary>
        /// the quizzes shown on the featured page
        /// </summary>
        private List<SearchInfo> quizzesFeatured;

        /// <summary>
        /// Sets up the featured page
        /// </summary>
        public FeaturedPage()
        {
            this.InitializeComponent();
            this.currentChunk = 1;
            this.quizzesFeatured = new List<SearchInfo>();
        }

        /// <summary>
        /// preloads the featured page
        /// </summary>
        protected async override void OnAppearing()
        {
            this.quizzesRemaining = true;
            this.category = "All";
            await this.Refresh();
        }

        /// <summary>
        /// if there are quizzes left to load
        /// </summary>
        private bool quizzesRemaining;

        /// <summary>
        /// the most recently loaded chunk of quizzes
        /// </summary>
        private int currentChunk;

        /// <summary>
        /// Refreshes the page's quiz content
        /// </summary>
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
                BugReportHandler.SaveReport(ex);
                await this.DisplayAlert("Error", "Couldn't get quizzes", "Ok");
            }
        }

        /// <summary>
        /// Searches the server for featured quizzes and sets the chunk equal to the servers response
        /// </summary>
        private async Task Search()
        {
            List<SearchInfo> chunk = new List<SearchInfo>();
            chunk = SearchUtils.GetQuizzesByAuthorChunked("BizQuiz", this.currentChunk);
            if (chunk.Count < 20)
                this.quizzesRemaining = false;
            await Task.Run(() => this.AddQuizzes(chunk));
        }

        /// <summary>
        /// fires when the page is scrolled. If scrolled all the way, it will search the server for the next chunk
        /// </summary>
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

                    ImageButton ImageButtonSubscribe = new ImageButton // 1
                    {
                        IsVisible = false,
                        Source = "ic_playlist_add_black_48dp.png",
                        StyleId = quiz.DBId,
                        HeightRequest = 30,
                        BackgroundColor = Color.White,
                        HorizontalOptions = LayoutOptions.End
                    };
                    ImageButtonSubscribe.Clicked += this.ImageButtonSubscribe_Clicked;
                    topStack.Children.Add(ImageButtonSubscribe);

                    ImageButton ImageButtonUnsubscribe = new ImageButton // 2
                    {
                        IsVisible = false,
                        Source = "ic_playlist_add_check_black_48dp.png",
                        StyleId = quiz.DBId,
                        HeightRequest = 30,
                        BackgroundColor = Color.White,
                        HorizontalOptions = LayoutOptions.End
                    };
                    ImageButtonUnsubscribe.Clicked += this.ImageButtonUnsubscribe_Clicked;
                    topStack.Children.Add(ImageButtonUnsubscribe);

                    ActivityIndicator Syncing = new ActivityIndicator // 3
                    {
                        IsVisible = false,
                        Color = Color.Accent,
                        HeightRequest = 25,
                        WidthRequest = 25,
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        HorizontalOptions = LayoutOptions.End,
                    };
                    topStack.Children.Add(Syncing);



                    if (quiz.Author != CredentialManager.Username)
                    {
                        // If already subscribed
                        if (!(currentlySubscribed.Where(quizInfo => quizInfo.DBId == quiz.DBId && !quizInfo.IsDeletedLocally).Count() > 0))
                        {
                            ImageButtonSubscribe.IsVisible = true;
                        }
                        else
                        {
                            ImageButtonUnsubscribe.IsVisible = true;
                        }
                    }

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
        /// Called when the user wants to unsubscribe from a quiz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImageButtonUnsubscribe_Clicked(object sender, EventArgs e)
        {
            if (CredentialManager.IsLoggedIn)
            {
                ImageButton button = (sender as ImageButton);
                string dbId = button.StyleId;
                bool answer = await this.DisplayAlert("Are you sure you want to unsubscribe?", "You will no longer get updates of this quiz", "Yes", "No");
                if (answer)
                {
                    ActivityIndicator indicatorSyncing = (button.Parent as StackLayout).Children[(int)SubscribeUtils.SubscribeType.Syncing] as ActivityIndicator;
                    button.IsVisible = false;
                    indicatorSyncing.IsVisible = true;
                    indicatorSyncing.IsRunning = true;
                    // get rosterInfo
                    QuizInfo rosterInfo = QuizRosterDatabase.GetQuizInfo(dbId);
                    // tell the roster that the quiz is deleted
                    QuizInfo rosterInfoUpdated = new QuizInfo(rosterInfo)
                    {
                        IsDeletedLocally = true,
                        LastModifiedDate = DateTime.Now.ToString()
                    };
                    QuizRosterDatabase.EditQuizInfo(rosterInfoUpdated);

                    OperationReturnMessage returnMessage = await SubscribeUtils.UnsubscribeFromQuizAsync(dbId);

                    if (returnMessage == OperationReturnMessage.True)
                    {
                        QuizRosterDatabase.DeleteQuizInfo(dbId);
                        (button.Parent as StackLayout).Children[(int)SubscribeUtils.SubscribeType.Subscribe].IsVisible = true; // add in subscribe button
                    }
                    else if (returnMessage == OperationReturnMessage.FalseInvalidCredentials)
                    {
                        button.IsVisible = true;
                        await this.DisplayAlert("Invalid Credentials", "Your current login credentials are invalid. Please log in and try again.", "OK");
                    }
                    else
                    {
                        button.IsVisible = true;
                        await this.DisplayAlert("Unsubscribe Failed", "The unsubscription request could not be completed. Please try again.", "OK");
                    }
                    indicatorSyncing.IsVisible = false;
                    indicatorSyncing.IsRunning = false;
                }
            }
            else
            {
                await this.DisplayAlert("Hold on!", "Before you can manage any quizzes, you have to login.", "Ok");
            }

            
        }

        /// <summary>
        /// When a user wants to subscribe to a quiz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void ImageButtonSubscribe_Clicked(object sender, EventArgs e)
        {
            if (CredentialManager.IsLoggedIn)
            {
                ImageButton button = (sender as ImageButton);
                string dbId = button.StyleId;

                ActivityIndicator indicatorSyncing = (button.Parent as StackLayout).Children[(int)SubscribeUtils.SubscribeType.Syncing] as ActivityIndicator;
                button.IsVisible = false;
                indicatorSyncing.IsVisible = true;
                indicatorSyncing.IsRunning = true;
                OperationReturnMessage returnMessage = await SubscribeUtils.SubscribeToQuizAsync(dbId, this.quizzesFeatured);
                if (returnMessage == OperationReturnMessage.True)
                {
                    (button.Parent as StackLayout).Children[2].IsVisible = true; // add in unsubscribe button
                }
                else if (returnMessage == OperationReturnMessage.FalseInvalidCredentials)
                {
                    button.IsVisible = true;
                    await this.DisplayAlert("Invalid Credentials", "Your current login credentials are invalid. Please try logging in again.", "OK");
                }
                else
                {
                    button.IsVisible = true;
                    await this.DisplayAlert("Subscribe Failed", "The subscription request could not be completed. Please try again.", "OK");
                }
                indicatorSyncing.IsVisible = false; // remove activity indicator
                indicatorSyncing.IsRunning = false;
            }
            else
            {
                await this.DisplayAlert("Hold on!", "Before you can subscribe to any quizzes, you have to login.", "Ok");
            }
            
        }

        /// <summary>
        /// when the search button is pressed, opens a search page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Activated(object sender, EventArgs e)
        {
            this.Navigation.PushModalAsync(new SearchPage());
        }

        /// <summary>
        /// when a category is selected, reset the page with the new category
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PickerCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.category = this.PickerCategory.Items[this.PickerCategory.SelectedIndex];
            this.currentChunk = 1;
            await this.Refresh();
        }
    }
}