﻿using System;
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
	public partial class FeaturedPage : ContentPage
	{
        private string category = "All";

        public FeaturedPage()
        {
            InitializeComponent();
            this.currentChunk = 1;
        }

        protected async override void OnAppearing()
            {
            this.levelsRemaining = true;
            await this.Refresh();
        }

        private bool levelsRemaining;
        private int currentChunk;

        private async Task Refresh()
        {
            SearchedStack.Children.Clear();
            try
            {
                Device.BeginInvokeOnMainThread(() => {
                    this.SearchedStack.Children.Clear();
                    this.ActivityIndicator.IsVisible = true;
                    this.ActivityIndicator.IsRunning = true;
                });
                await Task.Run(() => this.AddLevels(SearchUtils.GetLevelsByAuthorChunked("BizQuiz", 1)));
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.ActivityIndicator.IsVisible = false;
                    this.ActivityIndicator.IsRunning = false;
                });
            }
            catch (Exception ex)
            {
                BugReportHandler.SubmitReport(ex, nameof(FeaturedPage));
                await this.DisplayAlert("Error", "Couldn't get levels", "Ok");
            }
        }

        private async Task Search()
        {
            List<SearchInfo> chunk = new List<SearchInfo>();
            chunk = SearchUtils.GetLevelsByAuthorChunked("BizQuiz", this.currentChunk);
            if (chunk.Count < 20)
                this.levelsRemaining = false;
            await Task.Run(() => this.AddLevels(chunk));
        }

        private async void ScrollSearch_Scrolled(object sender, ScrolledEventArgs e)
        {
            ScrollView scrollView = sender as ScrollView;
            double scrollingSpace = scrollView.ContentSize.Height - scrollView.Height;

            if (scrollingSpace <= e.ScrollY && this.levelsRemaining)
            {
                try
                {
                    this.currentChunk++;
                    await Task.Run(() => this.Search());
                }
                catch
                {
                    await this.DisplayAlert("Error", "Couldn't get levels", "Ok");
                }
            }
        }

        /// <summary>
        /// Adds a level to the search stack given a LevelInfo
        /// </summary>
        /// <param name="level"></param>
        private void AddLevels(List<SearchInfo> levels)
        {

            foreach (SearchInfo level in levels)
            {
                if (this.category == "All" || level.Category == this.category) // Only add level if the category is what user picked
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
                    this.SearchedStack.Children.Add(levelFrame));
                }
            }
            if (this.SearchedStack.Children.Count() == 0)   
            {
                Label FeaturedMessage = new Label
                {
                    Text = "No featured quizzes here yet. Check back later!",
                    FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                };
                Device.BeginInvokeOnMainThread(() =>
                    this.SearchedStack.Children.Add(FeaturedMessage));
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
                bool answer = await this.DisplayAlert("Are you sure you want to unsubscribe?", "You will no longer get updates of this quiz", "Yes", "No");
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