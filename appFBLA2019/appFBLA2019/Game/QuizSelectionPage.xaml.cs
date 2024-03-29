﻿//BizQuiz App 2019

using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuizSelectionPage : ContentPage
    {
        /// <summary>
        /// To detect taps on quiz cards
        /// </summary>
        private TapGestureRecognizer recognizer = new TapGestureRecognizer();
        
        /// <summary>
        /// Whether the selection page is loading or not
        /// </summary>
        public bool IsLoading { get; set; }

        /// <summary>
        /// Whether the selection page is completely setup or not
        /// </summary>
        public bool isSetup;

        /// <summary>
        /// Whether something on the page is making use of the server
        /// </summary>
        public bool serverConnected;

        /// <summary>
        /// The category of the selectionpage
        /// </summary>
        private readonly string category;

        /// <summary>
        /// Creates a quizselectionpage for the given category
        /// </summary>
        /// <param name="category">The category of the page</param>
        public QuizSelectionPage(string category)
        {
            this.InitializeComponent();
            this.category = category;
            this.IsLoading = false;
            this.isSetup = false;

            this.FrameCreateNew.IsVisible = CredentialManager.IsLoggedIn;
        }

        /// <summary>
        /// Called when the page receives a size, sets up the cards if possible
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            this.CheckSetup();
            this.isSetup = true;
        }

        /// <summary>
        /// Called when the page is displayed to the user, sets up the cards if necessary 
        /// (this allows refreshing the page when the user navigates away and returns)
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.FrameCreateNew.IsVisible = CredentialManager.IsLoggedIn;
            this.CheckSetup();
        }

        /// <summary>
        /// Flags the page as not set up in order to trigger a setup when the user sees the page again
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.isSetup = false;
        }

        /// <summary>
        /// Check if setup can be performed at the time the method is called. If true,
        /// setup the page.
        /// </summary>
        private void CheckSetup()
        {
            if (Application.Current.MainPage.Width >= 0 && 
                !this.isSetup && 
                !this.serverConnected && 
                !this.IsLoading && 
                App.Path != null)
            {
                Directory.CreateDirectory(App.UserPath + $"{this.category}/");
                this.Setup();
            }
        }

        /// <summary>
        /// Sets up the page with quizzes the user has subscribed to from the category of the page
        /// </summary>
        public void Setup()
        {
            if (!this.IsLoading)
            {
                this.IsLoading = true;
                this.StackLayoutButtonStack.Children.Clear();

                List<QuizInfo> quizzes = QuizRosterDatabase.GetRoster(this.category);
                if (quizzes.Count == 0)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        StackLayout stack = new StackLayout();
                        stack.Children.Add(new Label
                        {
                            Text = "You don't have any quizzes in this category yet!",
                            HorizontalTextAlignment = TextAlignment.Center,
                            FontSize = 38
                        });
                        if (CredentialManager.IsLoggedIn)
                        {
                            stack.Children.Add(new Button
                            {
                                Text = "Make a quiz now",
                                CornerRadius = 25,
                                BackgroundColor = Color.Accent,
                                TextColor = Color.White,
                                FontSize = 26
                            });
                            (stack.Children[1] as Button).Clicked += (object sender, EventArgs e) => this.Navigation.PushAsync(new CreateNewQuizPage());
                            stack.Children.Add(new Button
                            {
                                Text = "Search for quizzes",
                                CornerRadius = 25,
                                BackgroundColor = Color.Accent,
                                TextColor = Color.White,
                                FontSize = 26
                            });
                            (stack.Children[2] as Button).Clicked += (object sender, EventArgs e) => this.Navigation.PushAsync(new SearchPage());
                        }
                        Frame frame = new Frame()
                        {
                            CornerRadius = 10,
                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                            Content = stack
                        };
                        this.StackLayoutButtonStack.Children.Add(frame);
                    });
                }

                foreach (QuizInfo quiz in quizzes)
                {
                    Frame frame = GenerateFrame(quiz);
                    this.StackLayoutButtonStack.Children.Add(frame);
                }
                this.IsLoading = false;
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
                HorizontalOptions = LayoutOptions.FillAndExpand//,
                                                               //HeightRequest = 45
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

            Label Author = new Label // 2
            {
                Text = "Created by: " + quizInfo.AuthorName,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.StartAndExpand
            };
            frameStack.Children.Add(Author);

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
                    Quiz newQuiz = new Quiz(quizInfo.DBId);
                    //await this.RemoveMenu(frameMenu);
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
            this.serverConnected = true;
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
            this.serverConnected = false;
        }

        /// <summary>
        /// Called when the user tries to download a quiz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SyncDownload_Clicked(object sender, EventArgs e)
        {
            this.serverConnected = true;
            ImageButton button = sender as ImageButton;

            string dbId = button.ClassId;
            QuizInfo quizInfo = QuizRosterDatabase.GetQuizInfo(dbId);
            string quizName = quizInfo.AuthorName;
            string authorName = quizInfo.AuthorName;
            string quizCategory = quizInfo.Category;

            ActivityIndicator indicatorSyncing = (button.Parent as StackLayout).Children[(int)UISyncStatus.Syncing] as ActivityIndicator;
            string quizPath = button.ClassId;
            button.IsVisible = false;
            indicatorSyncing.IsVisible = true;
            indicatorSyncing.IsRunning = true;
            if (await Task.Run(() => ServerOperations.GetQuiz(dbId, quizCategory)))
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
            this.serverConnected = false;
            this.CheckSetup();
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
        /// Handle event when user clicks delete quiz button.
        /// </summary>
        /// <param name="e"></param>
        private async void ButtonDelete_Clicked(string deleteType, string DBId)
        {
            if (CredentialManager.IsLoggedIn)
            {
                bool unsubscribe = deleteType == "Unsubscribe";
                string question;
                string message;
                if (unsubscribe)
                {
                    question = "Are you sure you want to unsubscribe?";
                    message = "This will remove the copy from your device";
                }
                else
                {
                    question = "Are you sure you want to delete this quiz?";
                    message = "This will delete the copy on your device and in the cloud. This is not reversable.";
                }

                bool answer = await this.DisplayAlert(question, message, "Yes", "No");
                if (answer)
                {
                    // Acquire QuizInfo from roster
                    QuizInfo rosterInfo = QuizRosterDatabase.GetQuizInfo(DBId); // Author
                    string path = rosterInfo.RelativePath;

                    // tell the roster that the quiz is deleted
                    QuizInfo rosterInfoUpdated = new QuizInfo(rosterInfo)
                    {
                        IsDeletedLocally = true,
                        LastModifiedDate = DateTime.Now.ToString()
                    };
                    QuizRosterDatabase.EditQuizInfo(rosterInfoUpdated);

                    // If connected, tell server to delete this quiz If not, it will tell server to delete next time it is connected in QuizRosterDatabase.UpdateLocalDatabase()
                    if (CrossConnectivity.Current.IsConnected)
                    {
                        OperationReturnMessage returnMessage;
                        if (unsubscribe)
                        {
                            returnMessage = await SubscribeUtils.UnsubscribeFromQuizAsync(DBId);
                        }
                        else
                        {
                            returnMessage = await ServerOperations.DeleteQuiz(DBId);
                        }

                        if (System.IO.Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        this.Setup();
                    }
                }
            }
            else
            {
                await this.DisplayAlert("Hold on!", "You must be create an account or log in before you can unsubscribe", "OK");
            }
            
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
          
            if(action == deleteText)
            {
                this.ButtonDelete_Clicked(((ImageButton)sender).StyleId, ((ImageButton)sender).ClassId);
            }
            else if(action == "Edit")
            {
                this.ButtonEdit_Clicked(DBId);
            }
        }

        /// <summary>
        /// Remove the drop down menu of the "three dot" button given the containing parent Frame
        /// </summary>
        /// <param name="frame">The containing parent Frame</param>
        /// <returns></returns>
        private async Task RemoveMenu(Frame frame)
        {
            await frame.FadeTo(0, 200, Easing.CubicInOut);
            frame.IsVisible = false;
        }

        /// <summary>
        /// Handle when user clicks edit button
        /// </summary>
        private async void ButtonEdit_Clicked(string DBId)
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
        /// Handle when user presses refresh button at top.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ToolbarItemRefresh_Activated(object sender, EventArgs e)
        {
            await this.RefreshPageAsync();
        }

        /// <summary>
        /// Show an activity indicator and refresh the page in the background.
        /// </summary>
        /// <returns></returns>
        public async Task RefreshPageAsync()
        {
            await this.StackLayoutButtonStack.FadeTo(0, 1);
            this.ActivityIndicator.IsVisible = true;
            this.ActivityIndicator.IsRunning = true;
            this.isSetup = false;

            await Task.Run(() => QuizRosterDatabase.UpdateLocalDatabaseAsync());
            this.CheckSetup();

            this.ActivityIndicator.IsVisible = false;
            this.ActivityIndicator.IsRunning = false;
            await this.StackLayoutButtonStack.FadeTo(1, 1);
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
    }
}