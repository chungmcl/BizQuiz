//BizQuiz App 2019
using Plugin.Connectivity;
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
        /// Gets a value indicating whether this <see cref="T:appFBLA2019.ProfilePage"/> is loading.
        /// </summary>
        /// <value><c>true</c> if is loading; otherwise, <c>false</c>.</value>
        public bool IsLoading { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:appFBLA2019.ProfilePage"/> is loading QuizStack.
        /// </summary>
        /// <value><c>true</c> if is content loading; otherwise, <c>false</c>.</value>
        private bool IsContentLoading { get; set; }

        /// <summary>
        /// If the page is currently showing the login page
        /// </summary> 
        public bool IsOnLoginPage { get; private set; }

        private string SelectedCategory
        {
            get
            {
                int selectedIndex = this.PickerCategory.SelectedIndex;
                if (selectedIndex < 0)
                    return "All";

                return this.PickerCategory.Items[selectedIndex];
            }
        }

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
            this.PickerCategory.IsVisible = false;
        }

        /// <summary>
        /// When the user wants to create a brand new quiz
        /// </summary>
        /// <param name="sender">  </param>
        /// <param name="e">       </param>
        private async void ButtonCreateQuiz_Activated(object sender, EventArgs e)
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
                if (this.PickerCategory.SelectedIndex < 0)
                    await this.UpdateProfileContentAsync("All");
                else
                    await this.UpdateProfileContentAsync(this.SelectedCategory);
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
        private async Task UpdateProfileContentAsync(string category)
        {
            if (!this.IsContentLoading)
            {
                this.IsContentLoading = true;
                this.QuizNumber.IsVisible = true;
                this.PickerCategory.IsVisible = false;
                this.PickerCategory.IsEnabled = false;
                this.StackLayoutQuizStack.Children.Clear();
                this.StackLayoutQuizStack.IsEnabled = false;
                this.ActivityIndicator.IsVisible = true;
                this.ActivityIndicator.IsRunning = true;

                await Task.Run(async() =>
                {
                    await this.SetupQuizzes(category);
                    int createdCountFromServer = ServerOperations.GetNumberOfQuizzesByAuthorName(CredentialManager.Username);
                    string frameMessage = "";
                    if (createdCountFromServer >= 0) // Returns -1 if can't connect to server
                    {
                        if (createdCountFromServer == 0 &&
                                !QuizRosterDatabase.GetRoster()
                                    .Any(quizInfo => quizInfo.AuthorName == CredentialManager.Username))
                        {
                            frameMessage = "You haven't made any quizzes yet!";
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                this.PickerCategory.IsVisible = false;
                                this.QuizNumber.IsVisible = false;
                            });
                        }
                        else
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                this.QuizNumber.Text = "You have published a total of " + createdCountFromServer + " quizzes!";
                                this.PickerCategory.IsVisible = true;
                                this.PickerCategory.IsEnabled = true;
                            });
                        }
                    }
                    else
                    {
                        frameMessage = "Could not connect to BizQuiz servers. Please try again later.";
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            this.PickerCategory.IsVisible = true;
                            this.PickerCategory.IsEnabled = true;
                        });
                    }

                    if (frameMessage != "")
                    {
                        Frame frame = new Frame()
                        {
                            CornerRadius = 10,
                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                            Content = new Label
                            {
                                Text = frameMessage,
                                HorizontalTextAlignment = TextAlignment.Center,
                                FontSize = 38
                            }
                        };

                        Device.BeginInvokeOnMainThread(() =>
                            this.StackLayoutQuizStack.Children.Add(frame));
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
                this.ActivityIndicator.IsRunning = false;
                this.ActivityIndicator.IsVisible = false;
                this.StackLayoutQuizStack.IsEnabled = true;
                this.IsContentLoading = false;
            }
        }

        /// <summary>
        /// Generate a frame (card-like button) for the UI based on a QuizInfo
        /// </summary>
        /// <param name="quizInfo">QuizInfo of the quiz to generate a frame for</param>
        /// <returns></returns>
        private Frame GenerateFrame(QuizInfo quizInfo)
        {
            Frame frame = new Frame()
            {
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 10
            };

            frame.ClassId = quizInfo.DBId;

            StackLayout frameStack = new StackLayout // 1st Child of frameLayout
            {
                FlowDirection = FlowDirection.LeftToRight,
                Orientation = StackOrientation.Vertical,
                Padding = 10
            };

            StackLayout topStack = new StackLayout
            {
                FlowDirection = FlowDirection.LeftToRight,
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            Label title = new Label // 0
            {
                Text = quizInfo.QuizName,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            topStack.Children.Add(title);

            // Add the sync buttons, We create one for each sync action to keep correct formatting and fix a sizing bug.
            ImageButton SyncOffline = new ImageButton // 1
            {
                IsVisible = false,
                Source = "ic_cloud_off_black_48dp.png",
                HeightRequest = 25,
                WidthRequest = 25,
                BackgroundColor = Color.White,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.End,
                ClassId = quizInfo.DBId,
                StyleId = quizInfo.Category
            };
            SyncOffline.Clicked += this.SyncOffline_Clicked;
            topStack.Children.Add(SyncOffline);

            ImageButton SyncUpload = new ImageButton // 2
            {
                IsVisible = false,
                Source = "ic_cloud_upload_black_48dp.png",
                HeightRequest = 25,
                WidthRequest = 25,
                BackgroundColor = Color.White,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.End,
                ClassId = quizInfo.DBId,
                StyleId = quizInfo.Category
            };
            SyncUpload.Clicked += this.SyncUpload_Clicked;
            topStack.Children.Add(SyncUpload);

            ImageButton SyncDownload = new ImageButton // 3
            {
                IsVisible = false,
                Source = "ic_cloud_download_black_48dp.png",
                HeightRequest = 25,
                WidthRequest = 25,
                BackgroundColor = Color.White,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.End,
                ClassId = quizInfo.DBId,
                StyleId = quizInfo.Category
            };
            SyncDownload.Clicked += this.SyncDownload_Clicked;
            topStack.Children.Add(SyncDownload);

            ImageButton SyncNoChange = new ImageButton // 4
            {
                IsVisible = false,
                Source = "ic_cloud_done_black_48dp.png",
                HeightRequest = 25,
                WidthRequest = 25,
                BackgroundColor = Color.White,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.End,
                ClassId = quizInfo.DBId,
                StyleId = quizInfo.Category
            };
            SyncNoChange.Clicked += this.SyncNoChange_Clicked;
            topStack.Children.Add(SyncNoChange);

            ActivityIndicator Syncing = new ActivityIndicator // 5
            {
                IsVisible = false,
                Color = Color.Accent,
                HeightRequest = 25,
                WidthRequest = 25,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.End,
            };
            topStack.Children.Add(Syncing);

            ImageButton imageButtonMenu = new ImageButton // 6
            {
                Source = "ic_more_vert_black_48dp.png",
                HeightRequest = 35,
                WidthRequest = 35,
                BackgroundColor = Color.White,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.End,
                ClassId = quizInfo.DBId
            };
            imageButtonMenu.Clicked += this.ImageButtonMenu_Clicked;
            topStack.Children.Add(imageButtonMenu);

            if (CredentialManager.Username == quizInfo.AuthorName)
            {
                imageButtonMenu.StyleId = "Delete";
            }
            else
            {
                imageButtonMenu.StyleId = "Unsubscribe";
            }

            frameStack.Children.Add(topStack);

            BoxView Seperator = new BoxView // 1
            {
                Color = Color.LightGray,
                CornerRadius = 1,
                HeightRequest = 2,
                WidthRequest = Application.Current.MainPage.Width - 75,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
            frameStack.Children.Add(Seperator);

            Label SubscriberCount = new Label // 2
            {
                Text = "Subscriber Count: " + quizInfo.SubscriberCount,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.StartAndExpand
            };
            frameStack.Children.Add(SubscriberCount);

            // The sync button thats active in the current frame
            ImageButton activeSync;

            if (quizInfo.SyncStatus == (int)SyncStatusEnum.Offline) // SyncOffline
            {
                SyncOffline.IsVisible = true;
                activeSync = SyncOffline;
            }
            else if (quizInfo.SyncStatus == (int)SyncStatusEnum.Synced) // SyncNoChange
            {
                SyncNoChange.IsVisible = true;
                activeSync = SyncNoChange;
            }
            else if (quizInfo.SyncStatus == (int)SyncStatusEnum.NeedUpload && quizInfo.AuthorName == CredentialManager.Username) // SyncUpload
            {
                SyncUpload.IsVisible = true;
                activeSync = SyncUpload;
            }
            else if (quizInfo.SyncStatus == (int)SyncStatusEnum.NeedDownload || 
                quizInfo.SyncStatus == (int)SyncStatusEnum.NotDownloadedAndNeedDownload) // SyncDownload
            {
                SyncDownload.IsVisible = true;
                activeSync = SyncDownload;
                if (quizInfo.SyncStatus == (int)SyncStatusEnum.NotDownloadedAndNeedDownload) // Sync Download & notLocal yet
                {
                    frame.StyleId = "notLocal";
                }
            }
            else
            {
                SyncOffline.IsVisible = true;
                activeSync = SyncOffline;
            }

            TapGestureRecognizer recognizer = new TapGestureRecognizer();
            recognizer.Tapped += async (object sender, EventArgs e) =>
            {
                if (frame.StyleId != "notLocal")
                {
                    frame.GestureRecognizers.Remove(recognizer);
                    frame.BackgroundColor = Color.LightGray;
                    Seperator.Color = Color.Gray;
                    imageButtonMenu.BackgroundColor = Color.LightGray;
                    activeSync.BackgroundColor = Color.LightGray;

                    // Load the quiz associated with this DBId
                    Quiz newQuiz = new Quiz(frame.ClassId);
                    await this.Navigation.PushAsync(new Game(newQuiz));
                    frame.BackgroundColor = Color.Default;
                    Seperator.Color = Color.LightGray;
                    imageButtonMenu.BackgroundColor = Color.White;
                    activeSync.BackgroundColor = Color.White;
                    frame.GestureRecognizers.Add(recognizer);
                }
                else
                {
                    await this.DisplayAlert("Hold on!", "In order to study with this quiz, you must download it first", "OK");
                }
            };

            frame.GestureRecognizers.Add(recognizer);
            frame.Content = frameStack;

            return frame;
        }

        /// <summary>
        /// Called when the user tries to upload a quiz
        /// </summary>
        /// <param name="sender">  </param>
        /// <param name="e">       </param>
        private async void SyncUpload_Clicked(object sender, EventArgs e)
        {
            ImageButton button = (sender as ImageButton);
            ActivityIndicator indicatorSyncing = (button.Parent as StackLayout).Children[(int)UISyncStatus.Syncing] as ActivityIndicator;
            string quizPath = "/" + button.StyleId + "/" + button.ClassId;
            button.IsVisible = false;
            indicatorSyncing.IsVisible = true;
            indicatorSyncing.IsRunning = true;
            if (await Task.Run(async () => await ServerOperations.SendQuiz(quizPath)))
            {
                ImageButton buttonSyncNoChange = (button.Parent as StackLayout).Children[(int)UISyncStatus.NoChange] as ImageButton;
                indicatorSyncing.IsVisible = false;
                buttonSyncNoChange.IsVisible = true;
            }
            else // if it failed to upload
            {
                indicatorSyncing.IsVisible = false;
                button.IsVisible = true;
                await this.DisplayAlert("Quiz Upload Failed",
                    "This quiz could not be uploaded to the server. Please try again.",
                    "OK");
            }
            indicatorSyncing.IsRunning = false;
            this.QuizNumber.Text = 
                "You have published a total of " + 
                await Task.Run(() => ServerOperations.GetNumberOfQuizzesByAuthorName(CredentialManager.Username)) + 
                " quizzes!";
        }

        /// <summary>
        /// Called when the user tries to download a quiz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SyncDownload_Clicked(object sender, EventArgs e)
        {
            // this.serverConnected = true;
            ImageButton button = sender as ImageButton;
            string dbId = button.ClassId;
            QuizInfo quizInfo = QuizRosterDatabase.GetQuizInfo(dbId);

            // DOES THIS WORK?
            while (quizInfo == null && Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
            {
                await QuizRosterDatabase.UpdateLocalDatabaseAsync();
                quizInfo = QuizRosterDatabase.GetQuizInfo(dbId);
            }
            
            string authorName = quizInfo.AuthorName;
            string quizName = quizInfo.QuizName;
            string category = quizInfo.Category;

            ActivityIndicator indicatorSyncing = (button.Parent as StackLayout).Children[(int)UISyncStatus.Syncing] as ActivityIndicator;
            string quizPath = button.ClassId;
            button.IsVisible = false;
            indicatorSyncing.IsVisible = true;
            indicatorSyncing.IsRunning = true;
            if (await Task.Run(() => ServerOperations.GetQuiz(dbId, category)))
            {
                ImageButton buttonSyncNoChange = (button.Parent as StackLayout).Children[(int)UISyncStatus.NoChange] as ImageButton;
                indicatorSyncing.IsVisible = false;
                buttonSyncNoChange.IsVisible = true;

                (((button.Parent as StackLayout).Parent as StackLayout).Parent as Frame).StyleId = "Local";
            }
            else // If it failed to download
            {
                indicatorSyncing.IsVisible = false;
                button.IsVisible = true;
                await this.DisplayAlert("Quiz Download Failed",
                    "This quiz could not be downloaded from the server. Please try again.",
                    "OK");
            }
            indicatorSyncing.IsRunning = false;
            await this.UpdateProfileContentAsync(
                this.SelectedCategory);
        }

        /// <summary>
        /// Called when a user clicks on the "already synced" icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncNoChange_Clicked(object sender, EventArgs e)
        {
            this.DisplayAlert("Already Synchronized", "This quiz is already up to date with the server version!", "OK");
        }

        /// <summary>
        /// Called when the user clicks on the "offline" icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncOffline_Clicked(object sender, EventArgs e)
        {
            this.DisplayAlert("Offline", "This quiz cannot be synced because you are offline.", "OK");
        }

        /// <summary>
        /// Handle when user presses "three dot" icon on the quiz tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImageButtonMenu_Clicked(object sender, EventArgs e)
        {
            string DBId = ((ImageButton)sender).ClassId;
            QuizInfo quizInfo = QuizRosterDatabase.GetQuizInfo(DBId);
            string quizName = quizInfo.QuizName;
            string author = quizInfo.AuthorName;
            string deleteText = "Delete";
            if (author != CredentialManager.Username)
            {
                deleteText = "Unsubscribe";
            }

            string action = await this.DisplayActionSheet(quizName, "Back", null, deleteText, "Edit");

            if (action == deleteText)
            {
                await this.ButtonDelete_Clicked(DBId);
            }
            else if (action == "Edit")
            {
                await this.ButtonEdit_Clicked(DBId);
            }
        }

        /// <summary>
        /// Handle when user clicks edit button
        /// </summary>
        private async Task ButtonEdit_Clicked(string DBId)
        {
            QuizInfo info = QuizRosterDatabase.GetQuizInfo(DBId);
            if (!CredentialManager.IsLoggedIn)
            {
                await this.DisplayAlert("Hold on!", "Before you can edit any quizzes, you have to login.", "Ok");
            }
            else if (info.SyncStatus == (int)SyncStatusEnum.NotDownloadedAndNeedDownload)
            {
                await this.DisplayAlert("Hold on!", "This quiz isn't on your device, download it before you try to edit it", "Ok");
            }
            else
            {
                CreateNewQuizPage quizPage = new CreateNewQuizPage(info); //Create the quizPage

                quizPage.SetQuizName(info.QuizName);
                Quiz quizDB = new Quiz(info.DBId);
                foreach (Question question in quizDB.GetQuestions())
                {
                    quizPage.AddNewQuestion(question);
                }
                await this.Navigation.PushAsync(quizPage);
            }
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
                    if (this.PickerCategory.SelectedIndex >= 0)
                    {
                        this.currentChunk++;

                        string category = this.PickerCategory.Items[this.PickerCategory.SelectedIndex];

                        await Task.Run(() =>
                            this.SetupNetworkQuizzes(category));
                    }
                }
                catch (Exception ex)
                {
                    BugReportHandler.SaveReport(ex);
                    await this.DisplayAlert("Oops, network failure", "Try again later", "Ok");
                }
            }
        }

        /// <summary>
        /// Sets up the quizzes owned by this user as cards
        /// </summary>
        private async Task SetupQuizzes(string category)
        {
            await QuizRosterDatabase.UpdateLocalDatabaseAsync();

            // A single LINQ statement to get all quizzes by the current
            // user that is logged in, and of the specified category,
            // taking the top 20 of the current chunk ordered by
            // the subscriber count of the quizzes
            List<QuizInfo> rosterCopy = QuizRosterDatabase.GetRoster();
            List<QuizInfo> quizzes =
                rosterCopy.
                Where(
                    quizInfo =>
                        (quizInfo.AuthorName == CredentialManager.Username 
                        &&
                        (quizInfo.Category == category || category == "All"))).

                OrderBy(quizInfo => quizInfo.SubscriberCount).

                Take(20 * this.currentChunk).ToList();

            this.AddQuizzes(quizzes, false);
        }

        /// <summary>
        /// Sets up the network quizzes owned by this user as cards
        /// </summary>
        private void SetupNetworkQuizzes(string category)
        {
            List<QuizInfo> chunk = SearchUtils.GetQuizzesByAuthorChunked(CredentialManager.Username, this.currentChunk);
            if (chunk.Count == 0)
            {
                return;
            }
            this.AddQuizzes(
                chunk.Where(quizInfo => 
                    (quizInfo.Category == category) || (category == "All")).ToList(), true);
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
                Frame frame = GenerateFrame(quiz);

                Device.BeginInvokeOnMainThread(() =>
                this.StackLayoutQuizStack.Children.Add(frame));
            }
        }

        /// <summary>
        /// Deletes the quiz from the roster 
        /// </summary>
        private async Task ButtonDelete_Clicked(string DBId)
        {
            bool answer = await this.DisplayAlert("Are you sure you want to delete this quiz?", "This will delete the copy on your device and in the cloud. This is not reversable.", "Yes", "No");
            if (answer)
            {
                // Acquire QuizInfo from roster
                QuizInfo rosterInfo = QuizRosterDatabase.GetQuizInfo(DBId);

                string path = rosterInfo.RelativePath;

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

                    this.QuizNumber.Text =
                    "You have published a total of " +
                    await Task.Run(() => ServerOperations.GetNumberOfQuizzesByAuthorName(CredentialManager.Username)) +
                    " quizzes!";
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
        /// Update the page to show only quizzes of a particular category when the user changes the category from the picker.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private async void PickerCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = (sender as Picker).SelectedIndex;
            if (selectedIndex >= 0)
            {
                string category = this.PickerCategory.Items[selectedIndex];
                await UpdateProfileContentAsync(category);
            }
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
            string[] hello = new string[] { "Hello, ", "Hola, ", "Ni Hao, ", "Aloha, ", "Konnichiwa, ", "Guten Tag, ", "Ciao, ", "Bonjour, ", "Yo, ", "Howdy, " };
            return hello[App.random.Next(hello.Length)];
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