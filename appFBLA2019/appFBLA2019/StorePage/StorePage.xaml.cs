using System;
using System.Collections.Generic;
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

        private bool isStartup;
		private List<SearchInfo> levelsSearched;

		// either "Title" or "Author"
		private string searchType;

		public StorePage()
		{
            this.isStartup = true;
			InitializeComponent();
			this.searchType = "Title";
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
			base.OnAppearing();
            if (this.isStartup)
            {
                this.SearchBar.Focus();
                this.searchIndicator.LayoutTo(new Rectangle(this.buttonTitle.X, this.searchIndicator.Y, this.buttonTitle.Width, 2));
            }

            this.isStartup = false;
		}

		/// <summary>
		/// Adds a level to the search stack given a LevelInfo
		/// </summary>
		/// <param name="level"></param>
		private void AddLevels(List<SearchInfo> levels)
		{
			foreach(SearchInfo level in levels)
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

				// source is add if not subscribed and if they are then source is check
				ImageButtonSubscribe.Source = "ic_playlist_add_black_48dp.png";

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
				Device.BeginInvokeOnMainThread(() =>
				SearchedStack.Children.Add(levelFrame));
			}
		   
		}

		/// <summary>
		/// When a user wants to subscribe to a level
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		async private void ImageButtonSubscribe_Clicked(object sender, EventArgs e)
		{
			ImageButton button = (sender as ImageButton);
			if (button.Source.ToString() == "File: ic_playlist_add_check_black_48dp.png") // unsubscribe
			{
				bool answer = await DisplayAlert("Are you sure you want to unsubscribe?", "You will no longer get updates of this quiz", "Yes", "No");
				if (answer)
				{
					await button.FadeTo(0, 150, Easing.CubicInOut);
					button.Source = "ic_playlist_add_black_48dp.png";
					button.HeightRequest = 30;
					await button.FadeTo(1, 150, Easing.CubicInOut);
					// remove from device
				}
			}
			else // subscribe
			{
				await button.FadeTo(0, 150, Easing.CubicInOut);
				button.Source = "ic_playlist_add_check_black_48dp.png";
				button.HeightRequest = 30;
				await button.FadeTo(1, 150, Easing.CubicInOut);
				// save to device
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
			//Device.BeginInvokeOnMainThread(() => {
			this.SearchedStack.Children.Clear();
			this.ActivityIndicator.IsEnabled = true;
			this.ActivityIndicator.IsRunning = true;
			//});
			this.isLoading = true;
			try
            {
                await this.Search();
            }
            catch (Exception ex)
			{
				BugReportHandler.SubmitReport(ex, "StorePage_SearchBar");
				await this.DisplayAlert("Search Failed", "Try again later", "Ok");
			}
			//Device.BeginInvokeOnMainThread(() =>
			//{
			this.ActivityIndicator.IsEnabled = false;
			this.ActivityIndicator.IsRunning = false;
			this.isLoading = false;
			//});
		}

        private async Task Search()
        {
            List<SearchInfo> chunk = new List<SearchInfo>();
            if (this.searchType == "Title")
                chunk = SearchUtils.GetLevelsByLevelNameChunked(SearchBar.Text, this.currentChunk);
            else
                chunk = SearchUtils.GetLevelsByAuthorChunked(SearchBar.Text, this.currentChunk);
            if (currentChunk == 1 && chunk.Count == 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SearchedStack.Children.Add(new Frame()
                    {
                        Content = new Label
                        {
                            Text = "Sorry, we couldn't find any levels matching what you searched",
                            HorizontalTextAlignment = TextAlignment.Center,
                            FontSize = 38
                        },
                        CornerRadius = 10,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                    });
                }
                );
            }
            if (chunk.Count < 20)
                levelsRemaining = false;
            await Task.Run(() => this.AddLevels(chunk));
        }

        private List<string[]> GetLevels(int chunk)
		{
			if (this.searchType == "Title")
				return ServerOperations.GetLevelsByLevelName(this.SearchBar.Text, chunk);
			else
				return ServerOperations.GetLevelsByAuthorName(this.SearchBar.Text, chunk);
		}
		

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

		private void ButtonTitle_Clicked(object sender, EventArgs e)
		{
			this.searchIndicator.LayoutTo(new Rectangle(this.buttonTitle.X, this.searchIndicator.Y, this.buttonTitle.Width, 2), 250, Easing.CubicInOut); 
			this.searchType = "Title";
		   
		}

		private void ButtonAuthor_Clicked(object sender, EventArgs e)
		{        
			//this.searchIndicator.TranslateTo(this.buttonAuthor.X, this.Y, 150, Easing.CubicInOut);
			this.searchIndicator.LayoutTo(new Rectangle(this.buttonAuthor.X, this.searchIndicator.Y, this.buttonAuthor.Width, 2), 250, Easing.CubicInOut);
			this.searchType = "Author";
		}
	}
}