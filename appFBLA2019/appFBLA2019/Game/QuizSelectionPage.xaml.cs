//BizQuiz App 2019

using Plugin.Connectivity;
using Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuizSelectionPage : ContentPage
    {
        private TapGestureRecognizer recognizer = new TapGestureRecognizer();
        public bool IsLoading { get; set; }
        public bool isSetup;
        public bool serverConnected;

        public QuizSelectionPage(string category)
        {
            this.InitializeComponent();
            this.category = category;
            Directory.CreateDirectory(App.UserPath + $"{category}/");
            this.IsLoading = false;
            this.isSetup = false;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            this.CheckSetup();
            this.isSetup = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.CheckSetup();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.isSetup = false;
        }

        private void CheckSetup()
        {
            if (Application.Current.MainPage.Width >= 0 && !this.isSetup && !this.serverConnected)
            {
                this.Setup();                
            }
        }

        private readonly string category;

        // TO DO: Display author name of quiz
        internal void Setup()
        {
            if (!this.IsLoading)
            {
                this.IsLoading = true;
                this.ButtonStack.Children.Clear();

                List<QuizInfo> quizzes = QuizRosterDatabase.GetRoster(this.category);
                if (quizzes.Count == 0)
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
                            (stack.Children[2] as Button).Clicked += (object sender, EventArgs e) => this.Navigation.PushAsync(new StorePage());
                        }
                        Frame frame = new Frame()
                        {
                            CornerRadius = 10,
                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                            Content = stack
                        };
                        this.ButtonStack.Children.Add(frame);
                    });
                foreach (QuizInfo quiz in quizzes)
                {
                    Frame frame = new Frame()
                    {
                        HeightRequest = 100,
                        VerticalOptions = LayoutOptions.Start,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        CornerRadius = 10
                    };

                    RelativeLayout frameLayout = new RelativeLayout // Child of frame
                    {
                        HeightRequest = 100
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
                        Text = quiz.QuizName,
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        HeightRequest = 45
                    };
                    topStack.Children.Add(title);

                    ImageButton Sync = new ImageButton
                    {
                        HeightRequest = 25,
                        WidthRequest = 25,
                        BackgroundColor = Color.White,
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        HorizontalOptions = LayoutOptions.End,
                        StyleId = "/" + this.category + "/" + quiz.QuizName + "`" + quiz.AuthorName,
                        ClassId = quiz.DBId + "/" + quiz.AuthorName + "/" + quiz.QuizName + "/" + quiz.Category
                    };
                    
                    topStack.Children.Add(Sync);

                    ImageButton imageButtonMenu = new ImageButton
                    {
                        Source = "ic_more_vert_black_48dp.png",
                        HeightRequest = 35,
                        WidthRequest = 35,
                        BackgroundColor = Color.White,
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        HorizontalOptions = LayoutOptions.End
                    };

                    imageButtonMenu.Clicked += this.ImageButtonMenu_Clicked;

                    topStack.Children.Add(imageButtonMenu);

                    Frame frameMenu = new Frame // Child of frameLayout
                    {
                        Padding = 0,
                        IsVisible = false,
                    };
                    StackLayout menuStack = new StackLayout
                    {
                        FlowDirection = FlowDirection.LeftToRight,
                        Orientation = StackOrientation.Vertical,
                        Padding = 0,
                        Spacing = 0
                    };

                    Button ButtonEdit = new Button(); // 3
                    {
                        ButtonEdit.Clicked += this.ButtonEdit_Clicked;
                        ButtonEdit.Text = "Edit";
                        ButtonEdit.HeightRequest = 45;
                        ButtonEdit.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                        ButtonEdit.HorizontalOptions = LayoutOptions.CenterAndExpand;
                        ButtonEdit.VerticalOptions = LayoutOptions.StartAndExpand;
                        ButtonEdit.BackgroundColor = Color.White;
                        ButtonEdit.CornerRadius = 0;
                    }
                    menuStack.Children.Add(ButtonEdit);

                    Button ButtonDelete = new Button();
                    {
                        ButtonDelete.Clicked += this.ButtonDelete_Clicked;
                        ButtonDelete.HeightRequest = 35;
                        ButtonDelete.TextColor = Color.Red;
                        ButtonDelete.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                        ButtonDelete.HorizontalOptions = LayoutOptions.CenterAndExpand;
                        ButtonDelete.VerticalOptions = LayoutOptions.StartAndExpand;
                        ButtonDelete.BackgroundColor = Color.White;
                        ButtonDelete.CornerRadius = 0;
                        ButtonDelete.StyleId = "/" + this.category + "/" + quiz.QuizName + "`" + quiz.AuthorName;
                    }
                    menuStack.Children.Add(ButtonDelete);

                    if (CredentialManager.Username == quiz.AuthorName)
                    {
                        ButtonDelete.Text = "Delete";
                    }
                    else
                    {
                        ButtonDelete.Text = "Unsubscribe";
                    }

                    frameMenu.Content = menuStack;

                    frameLayout.Children.Add(frameMenu, Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Width - 100;
                    }), Constraint.RelativeToParent((parent) =>
                    {
                        return parent.Y;
                    }), Constraint.Constant(95), Constraint.Constant(90));

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
                        Text = "Created by: " + quiz.AuthorName,
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                        HeightRequest = 60,
                        VerticalOptions = LayoutOptions.End,
                        HorizontalOptions = LayoutOptions.StartAndExpand
                    };
                    frameStack.Children.Add(Author);

                    if (quiz.SyncStatus == 3)
                    {
                        Sync.Source = "ic_cloud_off_black_48dp.png";
                        Sync.Clicked += this.SyncOffline_Clicked;
                    }
                    else if (quiz.SyncStatus == 2)
                    {
                        Sync.Source = "ic_cloud_done_black_48dp.png";
                        Sync.Clicked += this.SyncNoChange_Clicked;
                    }
                    else if (quiz.SyncStatus == 1)
                    {
                        Sync.Source = "ic_cloud_upload_black_48dp.png";
                        Sync.Clicked += this.SyncUpload_Clicked;
                    }
                    else if (quiz.SyncStatus == 0 || quiz.SyncStatus == 4)
                    {
                        Sync.Source = "ic_cloud_download_black_48dp.png";
                        Sync.Clicked += this.SyncDownload_Clicked;
                        if (quiz.SyncStatus == 4)
                        {
                            frame.StyleId = "notLocal";
                            ButtonEdit.StyleId = "notLocal";
                        }
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
                            Sync.BackgroundColor = Color.LightGray;
                            Quiz newQuiz = new Quiz(this.category, quiz.QuizName, quiz.AuthorName);
                            newQuiz.LoadQuestions();
                            await this.RemoveMenu(frameMenu);
                            await this.Navigation.PushAsync(new Game(newQuiz));
                            frame.BackgroundColor = Color.Default;
                            Seperator.Color = Color.LightGray;
                            imageButtonMenu.BackgroundColor = Color.White;
                            Sync.BackgroundColor = Color.White;
                            frame.GestureRecognizers.Add(recognizer);
                        }
                        else
                        {
                            await this.DisplayAlert("Hold on!", "In order to study with this quiz, you must download it first", "OK");
                        }
                    };

                    frame.GestureRecognizers.Add(recognizer);

                    frameLayout.Children.Add(frameStack, Constraint.RelativeToParent((parent) =>
                    {
                        return 0;
                    }));


                    frame.Content = frameLayout;
                    this.ButtonStack.Children.Add(frame);
                }
                this.IsLoading = false;
            }
        }

        private async void SyncUpload_Clicked(object sender, EventArgs e)
        {
            this.serverConnected = true;
            ImageButton button = (sender as ImageButton);
            string quizPath = button.StyleId;
            button.IsEnabled = false;
            await button.FadeTo(0, 150, Easing.CubicInOut);
            button.Source = "ic_autorenew_black_48dp.png";
            await button.FadeTo(1, 150, Easing.CubicInOut);
            button.HeightRequest = 25;

            if (await Task.Run(async() => await ServerOperations.SendQuiz(quizPath)))
            {
                await button.FadeTo(0, 150, Easing.CubicInOut);
                button.Source = "ic_cloud_done_black_48dp.png";
                await button.FadeTo(1, 150, Easing.CubicInOut);
                button.IsEnabled = true;
                button.Clicked += this.SyncNoChange_Clicked;
                
            }
            else
            {
                await button.FadeTo(0, 150, Easing.CubicInOut);
                button.Source = "ic_cloud_upload_black_48dp.png";
                await button.FadeTo(1, 150, Easing.CubicInOut);
                button.IsEnabled = true;
                await this.DisplayAlert("Quiz Upload Failed.", 
                    "This quiz could not be uploaded to the server. Please try again.", 
                    "OK");
            }
            this.serverConnected = false;
        }

        private async void SyncDownload_Clicked(object sender, EventArgs e)
        {
            this.serverConnected = true;
            ImageButton button = sender as ImageButton;
            string dbId = button.ClassId.Split('/')[0];
            string authorName = button.ClassId.Split('/')[1];
            string quizName = button.ClassId.Split('/')[2];
            string category = button.ClassId.Split('/')[3];
            button.IsEnabled = false;
            await button.FadeTo(0, 150, Easing.CubicInOut);
            button.Source = "ic_autorenew_black_48dp.png";
            await button.FadeTo(1, 150, Easing.CubicInOut);
            button.HeightRequest = 25;

            if (await Task.Run(() => ServerOperations.GetQuiz(dbId, quizName, authorName, category)))
            {
                button.Source = "ic_cloud_done_black_48dp.png";
                button.IsEnabled = true;
                button.Clicked += this.SyncNoChange_Clicked;
                
                ((((button.Parent as StackLayout).Parent as StackLayout).Parent as RelativeLayout).Parent as Frame).StyleId = "Local";
            }
            else
            {
                button.Source = "ic_cloud_download_black_48dp.png";
                button.IsEnabled = true;
                await this.DisplayAlert("Quiz Upload Failed.",
                    "This quiz could not be downloaded from the server. Please try again.",
                    "OK");
            }
            this.serverConnected = false;
        }

        private void SyncNoChange_Clicked(object sender, EventArgs e)
        {
            this.DisplayAlert("Already Synchronized", "This quiz is already up to date with the server version!", "OK");
        }

        private void SyncOffline_Clicked(object sender, EventArgs e)
        {
            this.DisplayAlert("Offline", "This quiz cannot be synced because you are offline.", "OK");
        }

        private async void ButtonDelete_Clicked(object sender, EventArgs e)
        {
            bool unsubscribe = ((Button)sender).Text == "Unsubscribe";
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
                string path = App.UserPath + ((Button)sender).StyleId;

                if (System.IO.Directory.Exists(path))
                {
                    // Acquire DBId from the quiz's realm file
                    string realmFilePath = Directory.GetFiles(path, "*.realm").First();
                    Realm realm = Realm.GetInstance(new RealmConfiguration(realmFilePath));
                    QuizInfo info = realm.All<QuizInfo>().First();
                    string dbId = info.DBId;

                    // Acquire QuizInfo from roster
                    QuizInfo rosterInfo = QuizRosterDatabase.GetQuizInfo(dbId);
                    QuizInfo rosterInfoUpdated = new QuizInfo(rosterInfo)
                    {
                        IsDeletedLocally = true,
                        LastModifiedDate = DateTime.Now.ToString()
                    };
                    QuizRosterDatabase.EditQuizInfo(rosterInfo);
                    if (!unsubscribe) // If delete (user owns this quiz)
                    {
                        // If connected, tell server to delete this quiz If not, it will tell server to delete next time it is connected in QuizRosterDatabase.UpdateLocalDatabase()
                        if (CrossConnectivity.Current.IsConnected)
                        {
                            OperationReturnMessage returnMessage = await ServerOperations.DeleteQuiz(dbId);
                            if (returnMessage == OperationReturnMessage.True)
                            {
                                realm.Remove(realm.All<QuizInfo>().Where(quizInfo => quizInfo.DBId == rosterInfo.DBId).First());
                            }
                        }
                        Directory.Delete(path, true);
                    }
                    else // If unsubscribe
                    {
                        // If connected, tell server to delete this quiz If not, it will tell server to delete next time it is connected in QuizRosterDatabase.UpdateLocalDatabase()
                        if (CrossConnectivity.Current.IsConnected)
                        {
                            OperationReturnMessage returnMessage = await ServerOperations.UnsubscribeToQuiz(dbId);
                            if (returnMessage == OperationReturnMessage.True)
                            {
                                realm.Remove(realm.All<QuizInfo>().Where(quizInfo => quizInfo.DBId == rosterInfo.DBId).First());
                            }
                        }
                        Directory.Delete(path, true);
                    }
                    this.Setup();
                }
                else
                {
                    await this.DisplayAlert("Quiz not Found", "This quiz is not downloaded. Press download to download the quiz.", "OK");
                }
            }
            else
            {
                await this.RemoveMenu(((Frame)((StackLayout)((Button)sender).Parent).Parent));
            }
        }

        private async void ImageButtonMenu_Clicked(object sender, EventArgs e)
        {
            Frame menu = ((Frame)((RelativeLayout)((StackLayout)((StackLayout)((ImageButton)sender).Parent).Parent).Parent).Children[0]);
            Frame frame = ((Frame)((RelativeLayout)menu.Parent).Parent);

            frame.GestureRecognizers.Remove(this.recognizer);
            menu.Opacity = 0;
            menu.IsVisible = true;
            await menu.FadeTo(1, 200, Easing.CubicInOut);

            TapGestureRecognizer globalRecognizer = new TapGestureRecognizer();
            globalRecognizer.Tapped += async (s, a) =>
            {
                await this.RemoveMenu(menu);
                this.ButtonStack.GestureRecognizers.Remove(globalRecognizer);
                frame.GestureRecognizers.Add(this.recognizer);
            };
            this.ButtonStack.GestureRecognizers.Add(globalRecognizer);
        }

        private async Task RemoveMenu(Frame frame)
        {
            await frame.FadeTo(0, 200, Easing.CubicInOut);
            frame.IsVisible = false;
        }

        private async void ButtonEdit_Clicked(object sender, EventArgs e)
        {
            Frame frame = ((Frame)((StackLayout)((Button)sender).Parent).Parent);
            await this.RemoveMenu(frame);
            if (!CredentialManager.IsLoggedIn)
            {
                await this.DisplayAlert("Hold on!", "Before you can edit any quizzes, you have to login.", "Ok");
            }
            else if (((Button)sender).StyleId == "notLocal")
            {
                await this.DisplayAlert("Hold on!", "This quiz isn't on your device, download it before you try to edit it", "Ok");
            }
            else
            {
                string quizTitle = ((Label)((StackLayout)((StackLayout)((RelativeLayout)(frame).Parent).Children[1]).Children[0]).Children[0]).Text;
                string quizAuthor = ((Label)((StackLayout)((RelativeLayout)(frame).Parent).Children[1]).Children[2]).Text.Split(':')[1].Trim();
                DBHandler.SelectDatabase(this.category, quizTitle, quizAuthor);
                CreateNewQuizPage quizPage = new CreateNewQuizPage(this.category, quizTitle, quizAuthor); //Create the quizPage

                quizPage.SetQuizName(quizTitle);
                foreach (Question question in DBHandler.Database.GetQuestions())
                {
                    quizPage.AddNewQuestion(question);
                }
                await this.Navigation.PushAsync(quizPage);
            }
        }
    }
}