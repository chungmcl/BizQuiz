﻿//BizQuiz App 2019

using Plugin.Connectivity;
using Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        /// <summary>
        /// If the profilepage is currently loading
        /// </summary>
        public bool IsLoading { get; private set; }
        /// <summary>
        /// If the page is currently showing the login page
        /// </summary>
        public bool IsOnLoginPage { get; private set; }
        /// <summary>
        /// the page that contains the account settings
        /// </summary>
        private AccountSettingsPage accountSettingsPage;
        /// <summary>
        /// used for downloading levels from the server in chunks
        /// </summary>
        private int currentChunk = 1;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProfilePage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// When the page appears, animate the username
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await UpdateProfilePageAsync();
        }

        /// <summary>
        /// When the page disappears, animate the username
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.LabelUsername.FadeTo(0, 500, Easing.CubicInOut);
        }

        /// <summary>
        /// When the user wants to create a brand new quiz
        /// </summary>
        /// <param name="sender">  </param>
        /// <param name="e">       </param>
        private async void New_Activated(object sender, EventArgs e)
        {
            if (CredentialManager.IsLoggedIn)
            {
                CreateNewQuizPage quiz = new CreateNewQuizPage();
                await this.Navigation.PushAsync(quiz);
            }
            else
            {
                if (await this.DisplayAlert("Hold on!", "Before you can create your own custom quizzes, you have to create your own account.", "Login/Create Account", "Go Back"))
                {
                    ProfilePage profilePage = new ProfilePage();
                    if (!profilePage.IsLoading && !profilePage.IsOnLoginPage)
                    {
                        await profilePage.UpdateProfilePageAsync();
                    }
                    profilePage.SetTemporary();
                    await this.Navigation.PushAsync(profilePage);
                }
            }
        }

        /// <summary>
        /// Sets the loginpage to temporary so that when the user is logged in, it doesnt go to the profilepage
        /// </summary>
        public void SetTemporary()
        {
            this.LocalLoginPage.LoggedIn -= this.OnLoggedIn;
            this.LocalLoginPage.LoggedIn += (object sender, EventArgs e) =>
            {
                this.Navigation.PopAsync();
            };
        }

        /// <summary>
        /// Update the Profile Tab Page to either show profile info or the login page if not logged in.
        /// </summary>
        /// <returns>  </returns>
        public async Task UpdateProfilePageAsync()
        {
            this.StackLayoutProfilePageContent.IsVisible = CredentialManager.IsLoggedIn;
            this.LocalLoginPage.IsVisible = !(CredentialManager.IsLoggedIn);
            this.IsLoading = true;
            this.StackLayoutQuizStack.Children.Clear();
            this.LabelUsername.Text = this.GetHello() + CredentialManager.Username + "!";
            await this.LabelUsername.FadeTo(1, 500, Easing.CubicInOut);
            if (this.StackLayoutProfilePageContent.IsVisible)
            {
                await this.UpdateProfileContentAsync();
            }
            else
            {
                if (this.ToolbarItems.Count > 0)
                {
                    this.ToolbarItems.Clear();
                }

                this.IsOnLoginPage = true;
                this.LocalLoginPage.LoggedIn += this.OnLoggedIn;
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Loads the profile content if the user is logged in.
        /// </summary>
        /// <returns> </returns>
        private async Task UpdateProfileContentAsync()
        {
            this.StackLayoutQuizStack.IsVisible = false;
            this.ActivityIndicator.IsVisible = true;
            this.ActivityIndicator.IsRunning = true;

            await Task.Run(() =>
            {
                this.totalCount = 0;
                this.SetupLocalQuizzes();
                this.SetupNetworkQuizzes();
                int createdCountFromServer = ServerOperations.GetNumberOfQuizzesByAuthorName(CredentialManager.Username);
                if (createdCountFromServer >= 0)
                {
                    this.totalCount += createdCountFromServer;
                    if (this.totalCount == 0 && this.StackLayoutQuizStack.Children.Count < 1)
                    {
                        Frame frame = new Frame()
                        {
                            CornerRadius = 10,
                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                            Content = new Label
                            {
                                Text = "You haven't made any quizzes yet!",
                                HorizontalTextAlignment = TextAlignment.Center,
                                FontSize = 38
                            }
                        };
                        Device.BeginInvokeOnMainThread(() =>
                            this.StackLayoutQuizStack.Children.Add(frame));
                    }
                    Device.BeginInvokeOnMainThread(() =>
                        this.QuizNumber.Text = "You have created a total of " + this.totalCount + " quizzes!");
                }
            });

            if (this.ToolbarItems.Count <= 0)
            {
                ToolbarItem accountSettingsButton = new ToolbarItem();
                accountSettingsButton.Clicked += this.ToolbarItemAccountSettings_Clicked;
                accountSettingsButton.Icon = ImageSource.FromFile("ic_settings_white_48dp.png") as FileImageSource;
                this.ToolbarItems.Add(accountSettingsButton);
            }

            this.IsOnLoginPage = false;
            this.totalCount = 0;
            this.ActivityIndicator.IsRunning = false;
            this.ActivityIndicator.IsVisible = false;
            this.StackLayoutQuizStack.IsVisible = true;
        }

        /// <summary>
        /// Triggered when the scrollsearch is moved, checks if more levels need to be loaded as the user scrolls down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ScrollSearch_Scrolled(object sender, ScrolledEventArgs e)
        {
            ScrollView scrollView = sender as ScrollView;
            double scrollingSpace = scrollView.ContentSize.Height - scrollView.Height;

            if (scrollingSpace <= e.ScrollY)
            {
                try
                {
                    this.currentChunk++;
                    await Task.Run(() => this.SetupNetworkQuizzes());
                }
                catch (Exception ex)
                {
                    BugReportHandler.SaveReport(ex);
                    await this.DisplayAlert("Oops, network failure", "Try again later", "Ok");
                }
            }
        }

        /// <summary>
        /// Sets up the local quizzes owned by this user as cards
        /// </summary>
        private void SetupLocalQuizzes()
        {
            List<QuizInfo> quizzes = QuizRosterDatabase.GetRoster();
            List<QuizInfo> userQuizzes = new List<QuizInfo>();
            foreach (QuizInfo quiz in quizzes)
            {
                if (quiz.AuthorName == CredentialManager.Username)
                {
                    userQuizzes.Add(new QuizInfo()
                    {
                        DBId = quiz.DBId,
                        AuthorName = quiz.AuthorName,
                        QuizName = quiz.QuizName,
                        Category = quiz.Category
                    });
                }
            }
            this.AddQuizzes(userQuizzes, false);
            this.totalCount += userQuizzes.Count;
        }

        /// <summary>
        /// total levels owned by the user
        /// </summary>
        private int totalCount = 0;

        /// <summary>
        /// Sets up the network quizzes owned by this user as cards
        /// </summary>
        private void SetupNetworkQuizzes()
        {
            List<QuizInfo> chunk = SearchUtils.GetQuizzesByAuthorChunked(CredentialManager.Username, this.currentChunk);
            if (chunk.Count == 0)
            {
                return;
            }
            this.AddQuizzes(chunk, true);
        }

        /// <summary>
        /// Adds user's quizzes to the login page
        /// </summary>
        /// <param name="quizzes">       a list of quizzes to dislay </param>
        /// <param name="isNetworkQuiz"> if the incoming quiz is a networkQuiz </param>
        private void AddQuizzes(List<QuizInfo> quizzes, bool isNetworkQuiz)
        {
            foreach (QuizInfo quiz in quizzes)
            {
                //if its not already stored locally (only applies to network quizzess)
                if (isNetworkQuiz && QuizRosterDatabase.GetQuizInfo(quiz.DBId) != null)
                {
                    this.totalCount--;
                }
                else
                {
                    Frame frame = new Frame()
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
                        Orientation = StackOrientation.Horizontal,
                        Padding = 0
                    };

                    Label quizName = new Label
                    {
                        Text = quiz.QuizName,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                        HorizontalOptions = LayoutOptions.StartAndExpand
                    };
                    topStack.Children.Add(quizName);

                    ImageButton buttonDelete = new ImageButton();
                    {
                        buttonDelete.HorizontalOptions = LayoutOptions.End;
                        buttonDelete.Source = "ic_delete_red_48dp.png";
                        buttonDelete.StyleId = "/" + quiz.Category + "/" + quiz.QuizName + "`" + quiz.AuthorName;
                        buttonDelete.Clicked += this.ButtonDelete_Clicked;
                        buttonDelete.BackgroundColor = Color.White;
                        buttonDelete.HeightRequest = 35;
                    }
                    topStack.Children.Add(buttonDelete);

                    frameStack.Children.Add(topStack);

                    Label Category = new Label
                    {
                        Text = "Category: " + quiz.Category
                    };
                    frameStack.Children.Add(Category);

                    Label Subscribers = new Label
                    {
                        Text = "Subscribers: " + quiz.SubscriberCount
                    };
                    frameStack.Children.Add(Subscribers);

                    frame.Content = frameStack;

                    Device.BeginInvokeOnMainThread(() =>
                    this.StackLayoutQuizStack.Children.Add(frame));
                }
            }
        }

        /// <summary>
        /// Deletes the quiz from the roster 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonDelete_Clicked(object sender, EventArgs e)
        {
            bool answer = await this.DisplayAlert("Are you sure you want to delete this quiz?", "This will delete the copy on your device and in the cloud. This is not reversable.", "Yes", "No");
            if (answer)
            {
                string path = App.UserPath + ((ImageButton)sender).StyleId;
                
                // StyleId = "/" + quiz.Category + "/" + quiz.QuizName + "`" + quiz.Author;

                // Acquire QuizInfo from roster
                QuizInfo rosterInfo = QuizRosterDatabase.GetQuizInfo(
                    ((ImageButton)sender).StyleId.Split('/').Last().Split('`').First(), // Quiz Name
                    ((ImageButton)sender).StyleId.Split('/').Last().Split('`').Last()); // Author

                if (rosterInfo != null)
                {
                    string dbId = rosterInfo.DBId;

                    // tell the roster that the quiz is deleted
                    QuizInfo rosterInfoUpdated = new QuizInfo(rosterInfo)
                    {
                        IsDeletedLocally = true,
                        LastModifiedDate = DateTime.Now.ToString()
                    };
                    QuizRosterDatabase.EditQuizInfo(rosterInfoUpdated);

                    // If connected, tell server to delete this quiz If not, it will tell server to delete next time it is connected in QuizRosterDatabase.UpdateLocalDatabase()
                    OperationReturnMessage returnMessage = await ServerOperations.DeleteQuiz(dbId);
                    if (returnMessage == OperationReturnMessage.True)
                        QuizRosterDatabase.DeleteQuizInfo(dbId);

                    if (System.IO.Directory.Exists(path))
                        Directory.Delete(path, true);
                }
                else
                {
                    await DisplayAlert("Could not delete quiz", "This quiz could not be deleted at this time. Please try again later", "OK");
                }

                // Setup Page again after deletion
                await this.UpdateProfilePageAsync();
            }
        }

        /// <summary>
        /// Opens the account settings page when the user clicks the account settings toolbar icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ToolbarItemAccountSettings_Clicked(object sender, EventArgs e)
        {
            if (this.accountSettingsPage == null)
            {
                this.accountSettingsPage = new AccountSettingsPage();
                this.accountSettingsPage.SignedOut += this.OnSignedOut;
                this.accountSettingsPage.SignedOut += this.LocalLoginPage.OnSignout;
            }
            await this.Navigation.PushAsync(this.accountSettingsPage);
        }

        /// <summary>
        /// when the user is logged in, set up events for potential logout
        /// </summary>
        /// <param name="source"></param>
        /// <param name="eventArgs"></param>
        public async void OnLoggedIn(object source, EventArgs eventArgs)
        {
            this.accountSettingsPage = new AccountSettingsPage();
            this.accountSettingsPage.SignedOut += this.OnSignedOut;
            this.accountSettingsPage.SignedOut += this.LocalLoginPage.OnSignout;
            await this.UpdateProfilePageAsync();
        }

        /// <summary>
        /// Gets "Hello" randomly from 8 languages
        /// </summary>
        /// <returns>a translation of "hello"</returns>
        private string GetHello()
        {
            List<string> hello = new List<string> { "Hello, ", "Hola, ", "你好, ", "Aloha, ", "こんにちは, ", "Guten Tag, ", "Ciao, ", "Bonjour, " };
            return hello[App.random.Next(8)];
        }

        /// <summary>
        /// When the user is logged out, unload the account settings page and refresh the profile tab
        /// </summary>
        /// <param name="source"></param>
        /// <param name="eventArgs"></param>
        public async void OnSignedOut(object source, EventArgs eventArgs)
        {
            this.accountSettingsPage = null;
            await this.UpdateProfilePageAsync();
        }
    }
}