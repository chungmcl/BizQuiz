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

        /// <summary>
        /// Adds a level to the search stack given a LevelInfo
        /// </summary>
        /// <param name="level"></param>
        private void AddLevel(LevelInfo level)
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
                Text = "Created by: " + level.AuthorName,
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
        private List<LevelInfo> Search(string text)
        {
            List <LevelInfo> testInfo = new List<LevelInfo>();
            testInfo.Add(new LevelInfo { DBId = "TestDBID", AuthorName = "TestAuthor", LevelName = "TestLevel", Category = "FBLA General" });
            testInfo.Add(new LevelInfo { DBId = "TestDBID2", AuthorName = "TestAuthor2", LevelName = "TestLevel2", Category = "FBLA General" });
            return testInfo;
        }

        public StorePage ()
		{
			InitializeComponent ();
		}

        /// <summary>
        /// Called when the user presses search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            // Delete what was in there previously
            this.SearchedStack.Children.Clear();
            foreach (LevelInfo level in Search(this.SearchBar.Text))
            {
                this.AddLevel(level);
            }
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.SearchedStack.Children.Clear();
        }

        protected override void OnDisappearing()
        {
            this.SearchedStack.Children.Clear();
        }
    }


    /// <summary>
    /// Temporary
    /// </summary>
    internal class LevelInfo
    {
        public string DBId { get; set; }
        public string AuthorName { get; set; }
        public string LevelName { get; set; }
        public string Category { get; set; }
        public string LastModifiedDate { get; set; }
        public bool NeedUploadSync { get; set; }
        public bool NeedDownloadSync { get; set; }
        public bool IsDeleted { get; set; }
    }
}