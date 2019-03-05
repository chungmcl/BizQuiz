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
    /// <summary>
    /// A page so users can search for quizzes on the server and subscribe/download it for themselves.
    /// </summary>
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SearchPage : ContentPage
	{
        /// <summary>
        /// current search chunk
        /// </summary>
		private int chunkNum;
        /// <summary>
        /// if the server is out of levels to send
        /// </summary>
		private bool end;
        /// <summary>
        /// if the page is setting up
        /// </summary>
		private bool isLoading;
        /// <summary>
        /// the current category
        /// </summary>
        private string category;
        /// <summary>
        /// If the page is setting up for the first time
        /// </summary>
        private bool isStartup;
        /// <summary>
        /// the quizzes being displayed
        /// </summary>
		private List<SearchInfo> quizzesSearched;

        /// <summary>
        /// Types of search
        /// </summary>
        private enum SearchType { Title = 1, Author};

        /// <summary>
        /// the current type of search
        /// </summary>
        private SearchType searchType;

        /// <summary>
        /// loads a search page
        /// </summary>
		public SearchPage()
		{
            this.isStartup = true;
            this.quizzesSearched = new List<SearchInfo>();
			this.InitializeComponent();
            this.searchType = SearchType.Title;
            this.category = "All";
		}

        /// <summary>
        /// when the page is created, focus on the search bar
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
            if (this.isStartup)
            {
                this.SearchBar.Focus();
                this.searchIndicator.LayoutTo(new Rectangle(this.buttonTitle.X, this.searchIndicator.Y, this.buttonTitle.Width, 3));
            }

            this.isStartup = false;
		}

        

        /// <summary>
        /// Adds a quiz to the search stack given a QuizInfo
        /// </summary>
        /// <param name="quizzes"></param>
        private void AddQuizzes(List<SearchInfo> quizzes)
		{
            List<QuizInfo> currentlySubscribed = QuizRosterDatabase.GetRoster();
			foreach(SearchInfo quiz in quizzes)
            {// Only add quiz if the category is what user picked (we are asking the server for more then we need so this could be changed)
                if (this.category == "All" || quiz.Category == this.category) 
                {

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

                    Label quizName = new Label // 0
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
                    this.quizzesSearched.Add(quiz);
                    Device.BeginInvokeOnMainThread(() =>
                    this. SearchedStack.Children.Add(quizFrame));
                }
			}		   
		}

        /// <summary>
        /// When a user wants to unsubscribe from a quiz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImageButtonUnsubscribe_Clicked(object sender, EventArgs e)
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
                    (button.Parent as StackLayout).Children[(int)SubscribeUtils.SubscribeType.Subscribe].IsVisible = true; // add in subscribe button
                    QuizRosterDatabase.DeleteQuizInfo(dbId);
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

        /// <summary>
        /// When a user wants to subscribe to a quiz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImageButtonSubscribe_Clicked(object sender, EventArgs e)
        {
            if (CredentialManager.IsLoggedIn)
            {
                ImageButton button = (sender as ImageButton);
                string dbId = button.StyleId;

                ActivityIndicator indicatorSyncing = (button.Parent as StackLayout).Children[(int)SubscribeUtils.SubscribeType.Syncing] as ActivityIndicator;
                button.IsVisible = false;
                indicatorSyncing.IsVisible = true;
                indicatorSyncing.IsRunning = true;

                OperationReturnMessage returnMessage = await SubscribeUtils.SubscribeToQuizAsync(dbId, this.quizzesSearched);
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
                indicatorSyncing.IsVisible = false;
                indicatorSyncing.IsRunning = false;
            }
            else
            {
                await this.DisplayAlert("Hold on!", "Before you can subscribe to any quizzes, you have to login.", "Ok");
            }
        }
        private int currentChunk;

		/// <summary>
		/// Called when the user presses search
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void SearchBar_SearchButtonPressed(object sender, EventArgs e)
		{
			// Delete what was in there previously
			this.end = false;
            this.currentChunk = 1;
			Device.BeginInvokeOnMainThread(() => {
			    this.SearchedStack.Children.Clear();
			    this.ActivityIndicator.IsEnabled = true;
			    this.ActivityIndicator.IsRunning = true;
			});
			this.isLoading = true;
			try
            {
                await Task.Run(() => this.SearchAsync());
            }
            catch (Exception ex)
			{
				BugReportHandler.SaveReport(ex);
				await this.DisplayAlert("Search Failed", "Try again later", "Ok");
			}
			Device.BeginInvokeOnMainThread(() =>
			{
			    this.ActivityIndicator.IsEnabled = false;
			    this.ActivityIndicator.IsRunning = false;
			    this.isLoading = false;
			});
		}

        /// <summary>
        /// Conducts a search of the online database
        /// </summary>
        /// <returns></returns>
        private async Task SearchAsync()
        {
            List<SearchInfo> chunk = new List<SearchInfo>();
            if (this.searchType == SearchType.Title)
                chunk = SearchUtils.GetQuizzesByQuizNameChunked(this.SearchBar.Text, this.currentChunk);
            else
                chunk = SearchUtils.GetQuizzesByAuthorChunked(this.SearchBar.Text, this.currentChunk);
            if (this.currentChunk == 1 && chunk.Count == 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.SearchedStack.Children.Add(new Label()
                    {
                        Text = "Sorry, we couldn't find any quizzes matching what you searched", 
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                    });
                }
                );
            }
            if (chunk.Count < 20)
            await Task.Run(() => this.AddQuizzes(chunk));
        }

        /// <summary>
        /// gets the quiz to display depending on the tab the user is on
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        private List<string[]> GetQuizzes(int chunk)
		{
			if (this.searchType == SearchType.Title)
				return ServerOperations.GetQuizzesByQuizName(this.SearchBar.Text, chunk);
			else
				return ServerOperations.GetQuizzesByAuthorName(this.SearchBar.Text, chunk);
		}
		
        /// <summary>
        /// Clears search when the search bar changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.SearchedStack.Children.Clear();
            
            //this searches everytime we change text, cool effect but performance issues

			//if (!string.IsNullOrWhiteSpace(this.SearchBar.Text) && !this.isLoading)
			//{
			//    this.Search(1);
			//    await Task.Run(() => this.Search(1));
			//}    
		}

        /// <summary>
        /// clears the page when it disappears
        /// </summary>
		protected override void OnDisappearing()
		{
			this.SearchedStack.Children.Clear();
		}
		
        /// <summary>
        /// does a search when user scrolls to the end.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private async void ScrollSearch_Scrolled(object sender, ScrolledEventArgs e)
		{
			ScrollView scrollView = sender as ScrollView;
			double scrollingSpace = scrollView.ContentSize.Height - scrollView.Height;

			if (scrollingSpace <= e.ScrollY && !this.end && !this.isLoading)
			{
				try
				{
                    this.chunkNum++;
					await Task.Run(() => this.SearchAsync());
				}
				catch
				{
					await this.DisplayAlert("Search Failed", "Try again later", "Ok");
				}
			}
		}

        /// <summary>
        /// Runs when the user switches to search by title tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void ButtonTitle_Clicked(object sender, EventArgs e)
		{
			this.searchIndicator.LayoutTo(new Rectangle(this.buttonTitle.X, this.searchIndicator.Y, this.buttonTitle.Width, 3), 250, Easing.CubicInOut); 
			this.searchType = SearchType.Title;
		   
		}

        /// <summary>
        /// Runs when user switches to search by author tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void ButtonAuthor_Clicked(object sender, EventArgs e)
		{        
			this.searchIndicator.LayoutTo(new Rectangle(this.buttonAuthor.X, this.searchIndicator.Y, this.buttonAuthor.Width, 3), 250, Easing.CubicInOut);
			this.searchType = SearchType.Author;
		}

        private async void PickerCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.category = this.PickerCategory.Items[this.PickerCategory.SelectedIndex];
            this.currentChunk = 1;
            try
            {
                await Task.Run(() => this.SearchAsync());
            }
            catch
            {
                await this.DisplayAlert("Search Failed", "Try again later", "Ok");
            }
        }

    }
}