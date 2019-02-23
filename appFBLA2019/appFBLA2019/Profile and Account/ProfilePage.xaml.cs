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
        private int currentChunk;

        public ProfilePage()
        {
            this.InitializeComponent();
        }

        public void SetTemporary()
        {
            this.LocalLoginPage.LoggedIn -= this.OnLoggedIn;
            this.LocalLoginPage.LoggedIn += (object sender, EventArgs e) =>
            {
                this.Navigation.PopAsync(); 
            };
        }

        public async Task UpdateProfilePage(bool updateLoginStatus)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // Initialize profile content to be hidden to avoid wonky UI behavior during async loading
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
                        this.SetupLocalQuizzes();
                        this.SetupNetworkQuizzes();
                        this.isSetup = true; ;
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
                this.SetupNetworkQuizzes();
            }
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
            List<LevelInfo> levels = LevelRosterDatabase.GetRoster();
            List<SearchInfo> userLevels = new List<SearchInfo>();
            foreach (LevelInfo level in levels)
            {
                if (level.AuthorName == CredentialManager.Username)
                {
                    userLevels.Add(new SearchInfo()
                    {
                        DBId = level.DBId,
                        Author = level.AuthorName,
                        LevelName = level.LevelName,
                        Category = level.Category
                    });
                }
            }
            this.AddLevels(userLevels, false);
            this.totalCount += userLevels.Count;
        }

        private int totalCount = 0;
        private void SetupNetworkQuizzes()
        {
            //this will take a while it would be good to make it async
            
            this.LabelUsername.FadeTo(1, 500, Easing.CubicInOut);
            List<SearchInfo> temp = SearchUtils.GetLevelsByAuthorChunked(CredentialManager.Username, this.currentChunk);
            totalCount += temp.Count;
            if (temp.Count == 0)
            {
                if (totalCount == 0)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        StackLayout stack = new StackLayout();
                        stack.Children.Add(new Label
                        {
                            Text = "You haven't made any levels yet!",
                            HorizontalTextAlignment = TextAlignment.Center,
                            FontSize = 38
                        });
                        stack.Children.Add(new Button
                        {
                            Text = "Make a level now",
                            CornerRadius = 25,
                            BackgroundColor = Color.Accent,
                            TextColor = Color.White,
                            FontSize = 26
                        });
                        (stack.Children[1] as Button).Clicked += (object sender, EventArgs e) => this.Navigation.PushModalAsync(new CreateNewLevelPage());
                        Frame frame = new Frame()
                        {
                            CornerRadius = 10,
                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                            Content = stack
                        };
                        this.LevelStack.Children.Add(frame);
                    }
                );
                }
                return;
            }
            this.QuizNumber.Text = "You have created a total of " + totalCount + " quizzes!";
            AddLevels(temp, true);
        }

        private void AddLevels(List<SearchInfo> levels, bool isNetworkLevels)
        {
            
                foreach (SearchInfo level in levels)
            {
                //if its not already stored locally (only apllies to networklevels)
                if (isNetworkLevels && LevelRosterDatabase.GetLevelInfo(level.DBId) != null)
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
                        Text = "Subscribers: " + level.SubCount
                    };
                    frameStack.Children.Add(Subscribers);

                    frame.Content = frameStack;
                    LevelStack.Children.Add(frame);
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
            await this.LabelUsername.FadeTo(1, 500, Easing.CubicInOut);
            this.SetupNetworkQuizzes();
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