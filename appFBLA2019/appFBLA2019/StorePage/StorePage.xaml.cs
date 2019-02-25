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
		private List<SearchInfo> levelsSearched;


        // either "Title" or "Author"
        private string searchType;

		public StorePage()
		{
            this.isStartup = true;
            this.levelsSearched = new List<SearchInfo>();
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
        /// Adds a level to the search stack given a LevelInfo
        /// </summary>
        /// <param name="level"></param>
        private void AddLevels(List<SearchInfo> levels)
		{
            List<LevelInfo> currentlySubscribed = LevelRosterDatabase.GetRoster();
			foreach(SearchInfo level in levels)
            {// Only add level if the category is what user picked (we are asking the server for more then we need so this could be changed)
                if (this.category == "All" || level.Category == this.category) 
                {

                    Frame levelFrame = new Frame
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

                    Label levelName = new Label
                    {
                        Text = level.LevelName,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                        HorizontalOptions = LayoutOptions.StartAndExpand
                    };
                    topStack.Children.Add(levelName);

                    ImageButton ImageButtonSubscribe = new ImageButton
                    {
                        StyleId = level.DBId,
                        HeightRequest = 30,
                        BackgroundColor = Color.White,
                        HorizontalOptions = LayoutOptions.End
                    };

                    // If already subscribed
                    if (!(currentlySubscribed.Where(levelInfo => levelInfo.DBId == level.DBId).Count() > 0))
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

                    Label levelAuthor = new Label
                    {
                        Text = "Created by: " + level.Author,
                    };
                    frameStack.Children.Add(levelAuthor);

                    Label levelCategory = new Label
                    {
                        Text = "Category: " + level.Category,
                    };
                    frameStack.Children.Add(levelCategory);

                    levelFrame.Content = frameStack;
                    this.levelsSearched.Add(level);
                    Device.BeginInvokeOnMainThread(() =>
                    this.SearchedStack.Children.Add(levelFrame));
                }
			}		   
		}

        /// <summary>
        /// When a user wants to subscribe to a level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImageButtonSubscribe_Clicked(object sender, EventArgs e)
        {
            ImageButton button = (sender as ImageButton);
            string dbId = button.StyleId;
            if (button.Source.ToString() == "File: ic_playlist_add_check_black_48dp.png") // unsubscribe
            {
                bool answer = await DisplayAlert("Are you sure you want to unsubscribe?", "You will no longer get updates of this quiz", "Yes", "No");
                if (answer)
                {
                    LevelInfo info = LevelRosterDatabase.GetLevelInfo(dbId);
                    string location = App.UserPath + "/" + info.Category + "/" + info.LevelName + "`" + info.AuthorName;
                    if (Directory.Exists(location))
                        Directory.Delete(location, true);

                    LevelRosterDatabase.DeleteLevelInfo(dbId);
                    OperationReturnMessage returnMessage = await Task.Run(() => ServerOperations.UnsubscribeToLevel(dbId));
                    if (returnMessage == OperationReturnMessage.True)
                    {
                        await button.FadeTo(0, 150, Easing.CubicInOut);
                        button.Source = "ic_playlist_add_black_48dp.png";
                        button.HeightRequest = 30;
                        await button.FadeTo(1, 150, Easing.CubicInOut);
                    }
                    else if (returnMessage == OperationReturnMessage.FalseInvalidCredentials)
                    {
                        await DisplayAlert("Invalid Credentials", "Your current login credentials are invalid. Please try logging in again.", "OK");
                        CredentialManager.IsLoggedIn = false;
                    }
                    else
                    {
                        await DisplayAlert("Subscribe Failed", "The subscription request could not be completed. Please try again.", "OK");
                    }
                }
            }
            else // subscribe
            {

                OperationReturnMessage returnMessage = await Task.Run(() => ServerOperations.SubscribeToLevel(dbId));
                if (returnMessage == OperationReturnMessage.True)
                {
                    SearchInfo level = this.levelsSearched.Where(searchInfo => searchInfo.DBId == dbId).First();
                    LevelInfo newInfo = new LevelInfo
                    {
                        AuthorName = level.Author,
                        LevelName = level.LevelName,
                        Category = level.Category,
                        SyncStatus = 4 // 4 to represent not present in local directory and need download
                    };
                    LevelRosterDatabase.NewLevelInfo(newInfo);

                    await button.FadeTo(0, 150, Easing.CubicInOut);
                    button.Source = "ic_playlist_add_check_black_48dp.png";
                    button.HeightRequest = 30;
                    await button.FadeTo(1, 150, Easing.CubicInOut);
                }
                else if (returnMessage == OperationReturnMessage.FalseInvalidCredentials)
                {
                    await DisplayAlert("Invalid Credentials", "Your current login credentials are invalid. Please try logging in again.", "OK");
                    CredentialManager.IsLoggedIn = false;
                }
                else
                {
                    await DisplayAlert("Subscribe Failed", "The unsubscription request could not be completed. Please try again.", "OK");
                }
            }

        }

        private bool levelsRemaining;
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
            this.levelsRemaining = true;
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
				BugReportHandler.SubmitReport(ex, "StorePage_SearchBar");
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
                chunk = SearchUtils.GetLevelsByLevelNameChunked(SearchBar.Text, this.currentChunk);
            else
                chunk = SearchUtils.GetLevelsByAuthorChunked(SearchBar.Text, this.currentChunk);
            if (this.currentChunk == 1 && chunk.Count == 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SearchedStack.Children.Add(new Label()
                    {
                        Text = "Sorry, we couldn't find any levels matching what you searched", 
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                    });
                }
                );
            }
            if (chunk.Count < 20)
                this.levelsRemaining = false;
            await Task.Run(() => this.AddLevels(chunk));
        }

        /// <summary>
        /// gets the levels to display depending on the tab the user is on
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        private List<string[]> GetLevels(int chunk)
		{
			if (this.searchType == "Title")
				return ServerOperations.GetLevelsByLevelName(this.SearchBar.Text, chunk);
			else
				return ServerOperations.GetLevelsByAuthorName(this.SearchBar.Text, chunk);
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

        private void PickerCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.category = this.PickerCategory.Items[this.PickerCategory.SelectedIndex];
            this.currentChunk = 1;
        }
    }
}