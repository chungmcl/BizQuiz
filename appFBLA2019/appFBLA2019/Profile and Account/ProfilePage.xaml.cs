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
            this.StackLayoutQuizStack.Children.Clear();
            this.LabelUsername.Text = this.GetHello() + CredentialManager.Username + "!";
            await this.LabelUsername.FadeTo(1, 500, Easing.CubicInOut);
            if (this.StackLayoutProfilePageContent.IsVisible)
                await this.UpdateProfileContent();
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
                            this.StackLayoutQuizStack.Children.Add(frame));
                    }
                    Device.BeginInvokeOnMainThread(() =>
                        this.QuizNumber.Text = "You have created a total of " + this.totalCount + " quizzes!");
                }
            });

            if (this.ToolbarItems.Count <= 0)
            {
                ToolbarItem accountSettingsButton = new ToolbarItem();
                accountSettingsButton.Clicked += ToolbarItemAccountSettings_Clicked;
                accountSettingsButton.Icon = ImageSource.FromFile("ic_settings_white_48dp.png") as FileImageSource;
                this.ToolbarItems.Add(accountSettingsButton);
            }
            
            this.IsOnLoginPage = false;
            this.totalCount = 0;
            this.ActivityIndicator.IsRunning = false;
            this.ActivityIndicator.IsVisible = false;
            this.StackLayoutQuizStack.IsVisible = true;
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
                catch (Exception ex)
                {
                    BugReportHandler.SaveReport(ex);
                    await this.DisplayAlert("Oops, network failure", "Try again later", "Ok");
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

        /// <summary>
        /// Adds user's quizzes to the login page
        /// </summary>
        /// <param name="quizzes"> a list of quizzes to dislay</param>
        /// <param name="isNetworkQuiz">if the incoming quiz is a networkQuiz </param>
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
                        buttonDelete.StyleId = "/" + quiz.Category + "/" + quiz.QuizName + "`" + quiz.Author;
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
                        Text = "Subscribers: " + quiz.SubCount
                    };
                    frameStack.Children.Add(Subscribers);

                    frame.Content = frameStack;

                    Device.BeginInvokeOnMainThread(() =>
                    this.StackLayoutQuizStack.Children.Add(frame));
                }
            }
        }

        private async void ButtonDelete_Clicked(object sender, EventArgs e)
        {
            bool answer = await this.DisplayAlert("Are you sure you want to delete this quiz?", "This will delete the copy on your device and in the cloud.This is not reversable.", "Yes", "No");
            if (answer)
            {
                string path = App.UserPath + ((ImageButton)sender).StyleId;

                // StyleId = "/" + quiz.Category + "/" + quiz.QuizName + "`" + quiz.Author;

                // Acquire QuizInfo from roster
                QuizInfo rosterInfo = QuizRosterDatabase.GetQuizInfo(
                    ((ImageButton)sender).StyleId.Split('/').Last().Split('`').First(), // Quiz Name
                    ((ImageButton)sender).StyleId.Split('/').Last().Split('`').Last()); // Author
                string dbId = rosterInfo.DBId;

                // tell the roster that the level is deleted
                QuizInfo rosterInfoUpdated = new QuizInfo(rosterInfo)
                {
                    IsDeletedLocally = true,
                    LastModifiedDate = DateTime.Now.ToString()
                };
                QuizRosterDatabase.EditQuizInfo(rosterInfoUpdated);

                // If connected, tell server to delete this quiz If not, it will tell server to delete next time it is connected in QuizRosterDatabase.UpdateLocalDatabase()
                if (CrossConnectivity.Current.IsConnected)
                {
                    OperationReturnMessage returnMessage = await ServerOperations.DeleteQuiz(dbId);

                    if (System.IO.Directory.Exists(path))
                    {
                        // Get the Realm File   
                        string realmFilePath = Directory.GetFiles(path, "*.realm").First();
                        Realm realm = Realm.GetInstance(new RealmConfiguration(realmFilePath));
                        if (returnMessage == OperationReturnMessage.True)
                        {
                            QuizRosterDatabase.DeleteQuizInfo(dbId);
                            realm.Write(() =>
                            {
                                realm.Remove(realm.All<QuizInfo>().Where(quizInfo => quizInfo.DBId == dbId).First());
                            });
                        }
                        Directory.Delete(path, true);
                    }
                    else
                    {
                        await this.DisplayAlert("Quiz not Found", "Is this quiz already deleted?", "Back");
                    }
                }
                // Setup Page again after deletion
                this.StackLayoutProfilePageContent.Children.Clear();
                await this.UpdateProfileContent();
            }
        }



        private async void ButtonDelete_ClickedNope(object sender, EventArgs e)
        {

            bool answer = await this.DisplayAlert("Are you sure you want to delete this quiz?", "This will delete the copy on your device and in the cloud.This is not reversable.", "Yes", "No");
            if (answer)
            {
                string path = App.UserPath + ((ImageButton)sender).StyleId;

                if (System.IO.Directory.Exists(path))
                {
                    // Acquire DBId from the quiz's realm file
                    QuizInfo rosterInfo = QuizRosterDatabase.GetQuizInfo(
                       ((Button)sender).StyleId.Split('/').Last().Split('`').First(), // Quiz Name
                       ((Button)sender).StyleId.Split('/').Last().Split('`').Last()); // Author
                    string dbId = rosterInfo.DBId;

                    QuizInfo rosterInfoUpdated = new QuizInfo(rosterInfo)
                    {
                        IsDeletedLocally = true,
                        LastModifiedDate = DateTime.Now.ToString()
                    };
                    QuizRosterDatabase.EditQuizInfo(rosterInfoUpdated);
                    // If connected, tell server to delete this quiz If not, it will tell server to delete next time it is connected in QuizRosterDatabase.UpdateLocalDatabase()
                    if (CrossConnectivity.Current.IsConnected)
                    {
                        // Get the Realm File   
                        string realmFilePath = Directory.GetFiles(path, "*.realm").First();
                        Realm realm = Realm.GetInstance(new RealmConfiguration(realmFilePath));
                        OperationReturnMessage returnMessage = await ServerOperations.DeleteQuiz(dbId);
                        if (returnMessage == OperationReturnMessage.True)
                        {
                            realm.Write(() =>
                            {
                                realm.Remove(realm.All<QuizInfo>().Where(quizInfo => quizInfo.DBId == dbId).First());
                            });
                        }
                    }
                    Directory.Delete(path, true);
                   
                }
                else
                {
                    await this.DisplayAlert("Quiz not Found", "Is this quiz already deleted?", "Back");
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