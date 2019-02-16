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
        private bool isSetup;

        public ProfilePage()
        {
            this.InitializeComponent();
        }

        public async Task UpdateProfilePage(bool updateLoginStatus)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // Initialize profile content to avoid wonky UI behavior during async loading
                this.StackLayoutProfilePageContent.IsVisible = false;
                this.ActivityIndicator.IsVisible = true;
                this.ActivityIndicator.IsRunning = true;
                this.IsLoading = true;
            });

            if (updateLoginStatus)
                await CredentialManager.CheckLoginStatus();

            Device.BeginInvokeOnMainThread(() =>
            {
                this.StackLayoutProfilePageContent.IsVisible = CredentialManager.IsLoggedIn;
                this.LocalLoginPage.IsVisible = !(CredentialManager.IsLoggedIn);
                if (this.StackLayoutProfilePageContent.IsVisible)
                {
                    if (this.ToolbarItems.Count <= 0)
                    {
                        ToolbarItem accountSettingsButton = new ToolbarItem();
                        accountSettingsButton.Clicked += ToolbarItemAccountSettings_Clicked;
                        accountSettingsButton.Icon = ImageSource.FromFile("ic_settings_white_48dp.png") as FileImageSource;
                        this.ToolbarItems.Add(accountSettingsButton);
                    }

                    // Get number of quizes from server.
                    this.IsOnLoginPage = false;
                                      
                    this.LabelUsername.Text = this.GetHello() + CredentialManager.Username + "!";
                    if (!isSetup)
                    {
                        this.SetupUserQuizes();
                    }
                }
                else
                {
                    if (this.ToolbarItems.Count > 0)
                        this.ToolbarItems.Clear();

                    this.IsOnLoginPage = true;
                    this.LocalLoginPage.LoggedIn += OnLoggedIn;
                }

                this.ActivityIndicator.IsRunning = false;
                this.ActivityIndicator.IsVisible = false;
                this.IsLoading = false;                    
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (this.StackLayoutProfilePageContent.IsVisible)
            {
                this.LabelUsername.Opacity = 0;
                this.LevelStack.Children.Clear();
                this.isSetup = false;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (this.StackLayoutProfilePageContent.IsVisible)
            {
                this.SetupUserQuizes();
            }
        }

        private List<LevelInfo> SearchByUser(string username)
        {
            List<LevelInfo> testInfo = new List<LevelInfo>();
            testInfo.Add(new LevelInfo { DBId = "TestDBID", AuthorName = "TestAuthor", LevelName = "TestLevel", Category = "FBLA General", Subscribers = 12 });
            testInfo.Add(new LevelInfo { DBId = "TestDBID2", AuthorName = "TestAuthor2", LevelName = "TestLevel2", Category = "FBLA General", Subscribers = 3 });
            return testInfo;
        }

        private void SetupUserQuizes()
        {
            this.QuizNumber.Text = "You have created a total of " + SearchByUser(CredentialManager.Username).Count + " quizes!";
            this.LabelUsername.FadeTo(1, 500, Easing.CubicInOut);
            foreach (LevelInfo level in SearchByUser(CredentialManager.Username))
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

                Label levelName = new Label
                {
                    Text = level.LevelName,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                    HorizontalOptions = LayoutOptions.StartAndExpand
                };
                frameStack.Children.Add(levelName);

                Label Category = new Label
                {
                    Text = "Category: " + level.Category
                };
                frameStack.Children.Add(Category);


                Label Subscribers = new Label
                {
                    Text = "Subscribers: " + level.Subscribers
                };
                frameStack.Children.Add(Subscribers);

                frame.Content = frameStack;
                LevelStack.Children.Add(frame);
            }
            this.isSetup = true;
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
            await this.LabelUsername.FadeTo(1, 500, Easing.CubicInOut);
            this.SetupUserQuizes();
            await Task.Run(() => UpdateProfilePage(false));
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
            await Task.Run(() => UpdateProfilePage(false));
        }
    }
}