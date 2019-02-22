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
        private List<SearchInfo> levelsSearched;

        // either "Title" or "Author"
        private string searchType;

        public StorePage()
        {
            InitializeComponent();
            this.searchType = "Title";
           
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            base.OnAppearing();
            this.SearchBar.Focus();
            this.searchIndicator.LayoutTo(new Rectangle(this.buttonTitle.X, this.searchIndicator.Y, this.buttonTitle.Width, 2));
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
                SearchedStack.Children.Add(levelFrame);
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


        /// <summary>
        /// Called when the user presses search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            // Delete what was in there previously
            this.SearchedStack.Children.Clear();
            this.end = false;
            //try
            {
                await Task.Run(() => this.Search(1));
            }
            //catch
            {
                //await this.DisplayAlert("Search Failed", "Try again later", "Ok");
            }
            //this.Search(1);
        }

        private List<string[]> GetLevels(int chunk)
        {
            if (this.searchType == "Title")
                return ServerOperations.GetLevelsByLevelName(this.SearchBar.Text, chunk);
            else
                return ServerOperations.GetLevelsByAuthorName(this.SearchBar.Text, chunk);
        }


        /// <summary>
        /// Searches the server for levels by levelName
        /// </summary>
        /// <param name="chunkNum">the chunk to get by 1, 2, 3...</param>
        private void Search(int chunkNum)
        {
            //Device.BeginInvokeOnMainThread(() =>
            //{
                this.SearchedStack.Children.Clear();
                this.ActivityIndicator.IsVisible = true;
                this.ActivityIndicator.IsRunning = true;
                this.isLoading = true;
            //});

            int i = 0;

            List<string[]> levels = this.GetLevels(chunkNum);
            if (levels != null)
            {
                //Device.BeginInvokeOnMainThread(() =>
                //{
                    foreach (string[] level in levels)
                    {
                        this.levelsSearched.Add(new SearchInfo
                        {
                            DBId = level[0],
                            Author = level[1],
                            LevelName = level[2],
                            Category = level[3],
                            SubCount = int.Parse(level[4])
                        });
                        i++;
                    }

                    this.AddLevels(this.levelsSearched);
                //});
            }
            else
            {
                this.message.Text = "No levels found matching search";
            }

            if (i < 20)
                this.end = true;

            //Device.BeginInvokeOnMainThread(() =>
            //{
                this.ActivityIndicator.IsVisible = false;
                this.ActivityIndicator.IsRunning = false;
                this.isLoading = false;
            //});

        }
        

        private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
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
                    await Task.Run(() => this.Search(this.chunkNum++));
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