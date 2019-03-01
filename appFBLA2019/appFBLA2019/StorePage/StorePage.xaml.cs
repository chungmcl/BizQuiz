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
	public partial class StorePage : ContentPage
	{
		private int chunkNum;
		private bool end;
		private bool isLoading;
        private string category;
        private bool isStartup;
		private List<SearchInfo> quizzesSearched;
        private enum SubscribeType { Subscribe = 1, Unsubscribe, Syncing };


        // either "Title" or "Author"
        private string searchType;

		public StorePage()
		{
            this.isStartup = true;
            this.quizzesSearched = new List<SearchInfo>();
			InitializeComponent();
			this.searchType = "Title";
            this.category = "All";
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
			base.OnAppearing();
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
            bool answer = await DisplayAlert("Are you sure you want to unsubscribe?", "You will no longer get updates of this quiz", "Yes", "No");
            if (answer)
            {
                ActivityIndicator buttonSyncing = (button.Parent as StackLayout).Children[(int)SubscribeType.Syncing] as ActivityIndicator;
                button.IsVisible = false;
                buttonSyncing.IsVisible = true;

                // get rosterInfo
                QuizInfo rosterInfo = QuizRosterDatabase.GetQuizInfo(dbId);
                // tell the roster that the level is deleted
                QuizInfo rosterInfoUpdated = new QuizInfo(rosterInfo)
                {
                    IsDeletedLocally = true,
                    LastModifiedDate = DateTime.Now.ToString()
                };
                QuizRosterDatabase.EditQuizInfo(rosterInfoUpdated);

                OperationReturnMessage returnMessage = await SubscribeUtils.UnsubscribeToLevel(dbId);

                if (returnMessage == OperationReturnMessage.True)
                {
                    buttonSyncing.IsVisible = false;
                    (button.Parent as StackLayout).Children[(int)SubscribeType.Subscribe].IsVisible = true; // add in subscribe button
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
            }
        }


        /// <summary>
        /// When a user wants to subscribe to a quiz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImageButtonSubscribe_Clicked(object sender, EventArgs e)
        {
            ImageButton button = (sender as ImageButton);
            string dbId = button.StyleId;

            ActivityIndicator buttonSyncing = (button.Parent as StackLayout).Children[(int)SubscribeType.Syncing] as ActivityIndicator;
            button.IsVisible = false;
            buttonSyncing.IsVisible = true;

            OperationReturnMessage returnMessage = await SubscribeUtils.SubscribeToLevel(dbId, this.quizzesSearched);
            if (returnMessage == OperationReturnMessage.True)
            {
                buttonSyncing.IsVisible = false; // remove subscribe button
                (button.Parent as StackLayout).Children[2].IsVisible = true; // add in unsubscribe button
            }
            else if (returnMessage == OperationReturnMessage.FalseInvalidCredentials)
            {
                button.IsVisible = true;
                await DisplayAlert("Invalid Credentials", "Your current login credentials are invalid. Please try logging in again.", "OK");
            }
            else
            {
                button.IsVisible = true;
                await DisplayAlert("Subscribe Failed", "The subscription request could not be completed. Please try again.", "OK");
            }
            
        }

        private bool quizzesRemaining;
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
            this.quizzesRemaining = true;
            this.currentChunk = 1;
			Device.BeginInvokeOnMainThread(() => {
			    this.SearchedStack.Children.Clear();
			    this.ActivityIndicator.IsEnabled = true;
			    this.ActivityIndicator.IsRunning = true;
			});
			this.isLoading = true;
			try
            {
                await Task.Run(() => this.Search());
            }
            catch (Exception ex)
			{
				BugReportHandler.SaveReport(ex, "StorePage_SearchBar");
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
        private async Task Search()
        {
            List<SearchInfo> chunk = new List<SearchInfo>();
            if (this.searchType == "Title")
                chunk = SearchUtils.GetQuizzesByQuizNameChunked(SearchBar.Text, this.currentChunk);
            else
                chunk = SearchUtils.GetQuizzesByAuthorChunked(SearchBar.Text, this.currentChunk);
            if (this.currentChunk == 1 && chunk.Count == 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SearchedStack.Children.Add(new Label()
                    {
                        Text = "Sorry, we couldn't find any quizzes matching what you searched", 
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                    });
                }
                );
            }
            if (chunk.Count < 20)
                this.quizzesRemaining = false;
            await Task.Run(() => this.AddQuizzes(chunk));
        }

        /// <summary>
        /// gets the quiz to display depending on the tab the user is on
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        private List<string[]> GetQuizs(int chunk)
		{
			if (this.searchType == "Title")
				return ServerOperations.GetQuizzesByQuizName(this.SearchBar.Text, chunk);
			else
				return ServerOperations.GetQuizzesByAuthorName(this.SearchBar.Text, chunk);
		}
		
        // Impliment this if we want to conduct a search each time we press a key down.
		private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.SearchedStack.Children.Clear();
			//if (!string.IsNullOrWhiteSpace(this.SearchBar.Text) && !this.isLoading)
			//{
			//    this.Search(1);
			//    await Task.Run(() => this.Search(1));
			//}    
		}

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
					await Task.Run(() => this.Search());
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
			this.searchType = "Title";
		   
		}

        /// <summary>
        /// Runs when user switches to search by author tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void ButtonAuthor_Clicked(object sender, EventArgs e)
		{        
			this.searchIndicator.LayoutTo(new Rectangle(this.buttonAuthor.X, this.searchIndicator.Y, this.buttonAuthor.Width, 3), 250, Easing.CubicInOut);
			this.searchType = "Author";
		}

        private async void PickerCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.category = this.PickerCategory.Items[this.PickerCategory.SelectedIndex];
            this.currentChunk = 1;
            try
            {
                await Task.Run(() => this.Search());
            }
            catch
            {
                await this.DisplayAlert("Search Failed", "Try again later", "Ok");
            }
        }

    }
}