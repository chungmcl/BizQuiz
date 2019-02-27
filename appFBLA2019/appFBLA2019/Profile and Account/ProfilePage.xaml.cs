//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        public bool IsLoading { get; private set; }
        public bool IsOnLoginPage { get; private set; }
        private AccountSettingsPage accountSettingsPage;
        private int currentChunk = 1;

        public ProfilePage()
        {
            this.InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await this.LabelUsername.FadeTo(1, 500, Easing.CubicInOut);

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.LabelUsername.FadeTo(0, 500, Easing.CubicInOut);
        }

        /// <summary>
        /// When the user wants to create a brand new quiz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        await profilePage.UpdateProfilePage();
                    }
                    profilePage.SetTemporary();
                    await this.Navigation.PushAsync(profilePage);
                }
            }
        }

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
        /// <returns></returns>
        public async Task UpdateProfilePage()
        {
            this.StackLayoutProfilePageContent.IsVisible = CredentialManager.IsLoggedIn;
            this.LocalLoginPage.IsVisible = !(CredentialManager.IsLoggedIn);
            this.IsLoading = true;
            this.QuizStack.Children.Clear();
            this.LabelUsername.Text = this.GetHello() + CredentialManager.Username + "!";
            await this.LabelUsername.FadeTo(1, 500, Easing.CubicInOut);
            if (this.StackLayoutProfilePageContent.IsVisible)
                await UpdateProfileContent();
            else
            {
                if (this.ToolbarItems.Count > 0)
                    this.ToolbarItems.Clear();

                this.IsOnLoginPage = true;
                this.LocalLoginPage.LoggedIn += OnLoggedIn;
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// Loads the profile content if the user is logged in.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateProfileContent()
        {
            this.QuizStack.IsVisible = false;
            this.ActivityIndicator.IsVisible = true;
            this.ActivityIndicator.IsRunning = true;

            await Task.Run(() =>
            {
                this.totalCount = 0;
                this.SetupLocalQuizzes();
                //this.SetupNetworkQuizzes();
                this.totalCount += ServerOperations.GetNumberOfQuizzesByAuthorName(CredentialManager.Username);
                if (totalCount == 0)
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
                        this.QuizStack.Children.Add(frame));
                }
                Device.BeginInvokeOnMainThread(() =>
                    this.QuizNumber.Text = "You have created a total of " + totalCount + " quizzes!");
            });

            if (this.ToolbarItems.Count <= 0)
            {
                ToolbarItem accountSettingsButton = new ToolbarItem();
                accountSettingsButton.Clicked += ToolbarItemAccountSettings_Clicked;
                accountSettingsButton.Icon = ImageSource.FromFile("ic_settings_white_48dp.png") as FileImageSource;
                this.ToolbarItems.Add(accountSettingsButton);
            }
            
            this.IsOnLoginPage = false;

            this.ActivityIndicator.IsRunning = false;
            this.ActivityIndicator.IsVisible = false;
            this.QuizStack.IsVisible = true;
        }

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
                catch
                {
                    await this.DisplayAlert("Search Failed", "Try again later", "Ok");
                }
            }
        }

        private void SetupLocalQuizzes()
        {
            List<QuizInfo> quizzes = QuizRosterDatabase.GetRoster();
            List<SearchInfo> userQuizzes = new List<SearchInfo>();
            foreach (QuizInfo quiz in quizzes)
            {
                if (quiz.AuthorName == CredentialManager.Username)
                {
                    userQuizzes.Add(new SearchInfo()
                    {
                        DBId = quiz.DBId,
                        Author = quiz.AuthorName,
                        QuizName = quiz.QuizName,
                        Category = quiz.Category
                    });
                }
            }
            this.AddQuizzes(userQuizzes, false);
            this.totalCount += userQuizzes.Count;
        }

        private int totalCount = 0;
        private void SetupNetworkQuizzes()
        {
            List<SearchInfo> chunk = SearchUtils.GetQuizzesByAuthorChunked(CredentialManager.Username, this.currentChunk);
            if (chunk.Count == 0)
            {
                return;
            }
            AddQuizzes(chunk, true);
        }

        private void AddQuizzes(List<SearchInfo> quizzes, bool isNetworkQuiz)
        {
            
            foreach (SearchInfo quiz in quizzes)
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

                    Label quizName = new Label
                    {
                        Text = quiz.QuizName,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                        HorizontalOptions = LayoutOptions.StartAndExpand
                    };
                    frameStack.Children.Add(quizName);

                    Label Category = new Label
                    {
                        Text = "Category: " + quiz.Category
                    };
                    frameStack.Children.Add(Category);


                    Label Subscribers = new Label
                    {
                        Text = "Subscribers: " + quiz.SubCount
                    };
                    frameStack.Children.Add(Subscribers);

                    frame.Content = frameStack;

                    Device.BeginInvokeOnMainThread(() =>
                    QuizStack.Children.Add(frame));
                }
            }
        }

        private async void ToolbarItemAccountSettings_Clicked(object sender, EventArgs e)
        {
            if (this.accountSettingsPage == null)
            {
                this.accountSettingsPage = new AccountSettingsPage();
                this.accountSettingsPage.SignedOut += OnSignedOut;
                this.accountSettingsPage.SignedOut += this.LocalLoginPage.OnSignout;
            }
            await this.Navigation.PushAsync(this.accountSettingsPage);
        }

        public async void OnLoggedIn(object source, EventArgs eventArgs)
        {
            this.accountSettingsPage = new AccountSettingsPage();
            this.accountSettingsPage.SignedOut += OnSignedOut;
            this.accountSettingsPage.SignedOut += this.LocalLoginPage.OnSignout;
            await UpdateProfilePage();
        }

        private string GetHello()
        {
            List<string> hello = new List<string> { "Hello, ", "Hola, ", "你好, ", "Aloha, ", "こんにちは, ", "Guten Tag, ", "Ciao, ", "Bonjour, "};
            Random rand = new Random();
            return hello[rand.Next(8)];
        }

        public async void OnSignedOut(object source, EventArgs eventArgs)
        {
            this.accountSettingsPage = null;
            await UpdateProfilePage();
        }
    }
}