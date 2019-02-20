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

        /// <summary>
        /// Adds a level to the search stack given a LevelInfo
        /// </summary>
        /// <param name="level"></param>
        private void AddLevel(SearchInfo level)
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

        /// <summary>
        /// When a user wants to subscribe to a level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void ImageButtonSubscribe_Clicked(object sender, EventArgs e)
        {
            
            if ((sender as ImageButton).Source.ToString() == "File: ic_playlist_add_check_black_48dp.png") // unsubscribe
            {
                bool answer = await DisplayAlert("Are you sure you want to unsubscribe?", "You will no longer get updates of this quiz", "Yes", "No");
                if (answer)
                {
                    (sender as ImageButton).Source = "ic_playlist_add_black_48dp.png";
                    (sender as ImageButton).HeightRequest = 30;
                    // remove from device
                }
            }
            else // subscribe
            {
                (sender as ImageButton).Source = "ic_playlist_add_check_black_48dp.png";
                (sender as ImageButton).HeightRequest = 30;
                // save to device
            }

        }

        // Temporary
        //private List<LevelInfo> Search(string text)
        //{
        //    List <LevelInfo> testInfo = new List<LevelInfo>();
        //    testInfo.Add(new LevelInfo { DBId = "TestDBID", AuthorName = "TestAuthor", LevelName = "TestLevel", Category = "FBLA General" });
        //    testInfo.Add(new LevelInfo { DBId = "TestDBID2", AuthorName = "TestAuthor2", LevelName = "TestLevel2", Category = "FBLA General" });
        //    return testInfo;
        //}

        public StorePage ()
		{
			InitializeComponent ();
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
            await Task.Run(() => this.Search(1));
            //this.Search(1);
        }


        /// <summary>
        /// Searches the server for levels by levelName
        /// </summary>
        /// <param name="chunkNum">the chunk to get by 1, 2, 3...</param>
        private void Search(int chunkNum)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                this.ActivityIndicator.IsVisible = true;
                this.ActivityIndicator.IsRunning = true;
                this.isLoading = true;
            });



            int i = 0;

            List<string[]> test = ServerOperations.GetLevelsByLevelName(this.SearchBar.Text, chunkNum);

            foreach (string[] level in ServerOperations.GetLevelsByLevelName(this.SearchBar.Text, chunkNum))
            {
                this.AddLevel(new SearchInfo
                {
                    DBId = level[0],
                    Author = level[1],
                    LevelName = level[2],
                    Category = level[3],
                    SubCount = int.Parse(level[4])
                });
                i++;
            }
            if (i < 20)
                this.end = true;

            Device.BeginInvokeOnMainThread(() =>
            {
                this.ActivityIndicator.IsVisible = false;
                this.ActivityIndicator.IsRunning = false;
                this.isLoading = false;
            });

        }
        

        private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.SearchedStack.Children.Clear();
            if (!string.IsNullOrWhiteSpace(this.SearchBar.Text) && !this.isLoading)
            {
                //this.Search(1);
                //await Task.Run(() => this.Search(1));
            }    
        }

        protected override void OnDisappearing()
        {
            this.SearchedStack.Children.Clear();
        }
        

        private class SearchInfo
        {
            public string DBId { get; set; }
            public string Author { get; set; }
            public string LevelName { get; set; }
            public string Category { get; set; }
            public int SubCount { get; set; }

        }

        private async void ScrollSearch_Scrolled(object sender, ScrolledEventArgs e)
        {
            ScrollView scrollView = sender as ScrollView;
            double scrollingSpace = scrollView.ContentSize.Height - scrollView.Height;

            if (scrollingSpace <= e.ScrollY && !this.end && !this.isLoading)
            {
                await Task.Run(() => this.Search(this.chunkNum++));
                //this.Search(this.chunkNum++);
            }
        }
    }
}