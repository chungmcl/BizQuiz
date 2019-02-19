//BizQuiz App 2019

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
    public partial class LevelSelectionPage : ContentPage
    {
        public bool IsLoading { get; set; }
        public LevelSelectionPage(string category)
        {
            this.InitializeComponent();
            this.category = category;
            Directory.CreateDirectory(App.Path + $"/{category}");
            this.IsLoading = false;
            // TO DO: Replace "DependencyService... .GetStorage()" with the location where the databases are being stored WHEN the app is is RELEASED (See DBHandler)
            //this.Setup();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            this.Setup();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Application.Current.MainPage.Width <= 0)
                this.Setup();
        }

        private readonly string category;
        // TO DO: Display author name of level
        internal void Setup()
        {
            if (!this.IsLoading)
            {
                this.IsLoading = true;
                this.ButtonStack.Children.Clear();

                List<LevelInfo> levels = LevelRosterDatabase.GetRoster();

                foreach (LevelInfo level in levels)
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
                        Text = level.LevelName,
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        HeightRequest = 45
                    };
                    topStack.Children.Add(title);

                    // check status
                    ImageButton Sync = new ImageButton
                    {
                        HeightRequest = 25,
                        WidthRequest = 25,
                        BackgroundColor = Color.White,
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        HorizontalOptions = LayoutOptions.End,
                        StyleId = "/" + category + "/" + level.LevelName + "`" + level.AuthorName
                    };

                    if (level.SyncStatus == 3)
                    {
                        Sync.Source = "ic_cloud_off_black_48dp.png";
                        Sync.Clicked += SyncOffline_Clicked;
                    }
                    else if (level.SyncStatus == 2)
                    {
                        Sync.Source = "ic_cloud_done_black_48dp.png";
                        Sync.Clicked += SyncNoChange_Clicked;
                    }
                    else if (level.SyncStatus == 1)
                    {
                        Sync.Source = "ic_cloud_upload_black_48dp.png";
                        Sync.Clicked += SyncUpload_Clicked;
                    }
                    else if (level.SyncStatus == 0)
                    {
                        Sync.Source = "ic_cloud_download_black_48dp.png";
                        Sync.Clicked += SyncDownload_Clicked;
                    }

                    topStack.Children.Add(Sync);

                    ImageButton imageButtonMenu = new ImageButton
                    {
                        Source = "ic_more_vert_black_48dp.png",
                        HeightRequest = 25,
                        WidthRequest = 25,
                        BackgroundColor = Color.White,
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        HorizontalOptions = LayoutOptions.End,
                    };

                    imageButtonMenu.Clicked += this.ImageButtonMenu_Clicked;

                    topStack.Children.Add(imageButtonMenu);

                    Frame frameMenu = new Frame // Child of frameLayout
                    {
                        Padding = 0,
                        IsVisible = false
                    };
                    StackLayout menuStack = new StackLayout
                    {
                        FlowDirection = FlowDirection.LeftToRight,
                        Orientation = StackOrientation.Vertical,
                        Padding = 0
                    };

                    Button ButtonEdit = new Button(); // 3
                    {
                        ButtonEdit.Clicked += this.ButtonEdit_Clicked;
                        ButtonEdit.Text = "Edit";
                        ButtonEdit.HeightRequest = 35;
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
                        ButtonDelete.StyleId = "/" + category + "/" + level.LevelName + "`" + level.AuthorName;
                    }
                    menuStack.Children.Add(ButtonDelete);

                    if (CredentialManager.Username == level.AuthorName)
                    {
                        ButtonDelete.Text = "Delete";
                    }
                    else
                    {
                        ButtonDelete.Text = "Unsubscribe";
                    }

                    frameMenu.Content = menuStack;

                    frameLayout.Children.Add(frameMenu, Constraint.RelativeToParent((parent) => {
                        return parent.Width - 100;
                    }), Constraint.RelativeToParent((parent) => {
                        return parent.Y + 20;
                    }), Constraint.Constant(83), Constraint.Constant(76));

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
                        Text = "Created by: " + level.AuthorName,
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                        HeightRequest = 60,
                        VerticalOptions = LayoutOptions.End,
                        HorizontalOptions = LayoutOptions.StartAndExpand
                    };
                    frameStack.Children.Add(Author);



                    TapGestureRecognizer recognizer = new TapGestureRecognizer();
                    recognizer.Tapped += async (object sender, EventArgs e) =>
                    {
                        frame.GestureRecognizers.Remove(recognizer);
                        frame.BackgroundColor = Color.LightGray;
                        Seperator.Color = Color.Gray;
                        imageButtonMenu.BackgroundColor = Color.LightGray;
                        Sync.BackgroundColor = Color.LightGray;
                        Level newLevel = new Level(this.category, level.LevelName, level.AuthorName);
                        newLevel.LoadQuestions();
                        await this.RemoveMenu(frameMenu);
                        await this.Navigation.PushAsync(new Game(newLevel));
                        frame.BackgroundColor = Color.Default;
                        Seperator.Color = Color.LightGray;
                        imageButtonMenu.BackgroundColor = Color.White;
                        Sync.BackgroundColor = Color.White;
                        frame.GestureRecognizers.Add(recognizer);
                    };

                    frame.GestureRecognizers.Add(recognizer);


                    frameLayout.Children.Add(frameStack, Constraint.RelativeToParent((parent) => {
                        return 0;
                    }));

                    frame.Content = frameLayout;
                    this.ButtonStack.Children.Add(frame);
                }
                this.IsLoading = false;
            }
        }

        private void SyncUpload_Clicked(object sender, EventArgs e)
        {
            string levelPath = (sender as ImageButton).StyleId;
            ServerOperations.SendLevel(levelPath);
        }

        private void SyncDownload_Clicked(object sender, EventArgs e)
        {

        }

        private void SyncNoChange_Clicked(object sender, EventArgs e)
        {
            DisplayAlert("Already Synchronized", "This level is already up to date with the server version!", "OK");
        }

        private void SyncOffline_Clicked(object sender, EventArgs e)
        {
            DisplayAlert("Offline", "This level cannot be synced because you are offline.", "OK");
        }

        private async void ButtonDelete_Clicked(object sender, EventArgs e)
        {
            bool unsubscribe = ((Button)sender).Text == "Unsubscribe";
            string question = "Are you sure you want to delete this level?";
            string message = "This will delete the copy on your device and in the cloud. This is not reversable.";
            if (unsubscribe)
            {
                question = "Are you sure you want to unsubscribe?";
                message = "This will remove the copy from your device";
                // Decrement subscriber count on server
            }

            bool answer = await DisplayAlert(question, message, "Yes", "No");
            if (answer)
            {
                string path = App.Path + ((Button)sender).StyleId;

                if (System.IO.Directory.Exists(path))
                {
                    if (!unsubscribe) // If delete (user owns this level)
                    {
                        // Acquire DBId from the level's realm file
                        string realmFilePath = Directory.GetFiles(path, "*.realm").First();
                        Realm realm = Realm.GetInstance(new RealmConfiguration(realmFilePath));
                        LevelInfo info = realm.All<LevelInfo>().First();
                        string dbId = info.DBId;

                        // Acquire LevelInfo from roster
                        LevelInfo rosterInfo = LevelRosterDatabase.GetLevelInfo(dbId);
                        LevelInfo rosterInfoUpdated = new LevelInfo(rosterInfo);
                        rosterInfoUpdated.IsDeletedLocally = true;

                        rosterInfoUpdated.LastModifiedDate = DateTime.Now.ToString();
                        LevelRosterDatabase.EditLevelInfo(rosterInfoUpdated);

                        // If connected, tell server to delete this level
                        // If not, it will tell server to delete next time it is connected in LevelRosterDatabase.UpdateLocalDatabase()
                        if (CrossConnectivity.Current.IsConnected)
                        {
                            OperationReturnMessage returnMessage = ServerOperations.DeleteLevel(dbId);
                            if (returnMessage == OperationReturnMessage.True)
                                realm.Remove(rosterInfo);
                        }

                        // Clear out DBHandler.GameDatabase in case it references the level just deleted
                        DBHandler.DisposeDatabase();
                        Directory.Delete(path, true);
                    }
                    this.Setup();
                }
            }
            else
            {
                await this.RemoveMenu(((Frame)((StackLayout)((Button)sender).Parent).Parent));
            }
        }

        async private void ImageButtonMenu_Clicked(object sender, EventArgs e)
        {
            Frame frame = ((Frame)((RelativeLayout)((StackLayout)((StackLayout)((ImageButton)sender).Parent).Parent).Parent).Children[0]);
            frame.Opacity = 0;
            frame.IsVisible = true;
            await frame.FadeTo(1, 200, Easing.CubicInOut);

            TapGestureRecognizer globalRecognizer = new TapGestureRecognizer();
            globalRecognizer.Tapped += async (s, a) => {
                await this.RemoveMenu(frame);
                this.ButtonStack.GestureRecognizers.Remove(globalRecognizer);
            };
            this.ButtonStack.GestureRecognizers.Add(globalRecognizer);

        }

        async private Task RemoveMenu(Frame frame)
        {
            await frame.FadeTo(0, 200, Easing.CubicInOut);
            frame.IsVisible = false;          
        }

        async private void ButtonEdit_Clicked(object sender, EventArgs e)
        {
            Frame frame = ((Frame)((StackLayout)((Button)sender).Parent).Parent);
            await this.RemoveMenu(frame);
            if (CredentialManager.IsLoggedIn)
            {
                string levelTitle = ((Label)((StackLayout)((StackLayout)((RelativeLayout)(frame).Parent).Children[1]).Children[0]).Children[0]).Text;
                string levelAuthor = ((Label)((StackLayout)((RelativeLayout)(frame).Parent).Children[1]).Children[2]).Text.Split(':')[1].Trim();
                DBHandler.SelectDatabase(category, levelTitle, levelAuthor);
                CreateNewLevelPage levelPage = new CreateNewLevelPage(category, levelTitle, levelAuthor); //Create the levelPage

                levelPage.SetLevelName(levelTitle);
                foreach (Question question in DBHandler.Database.GetQuestions())
                {
                    levelPage.AddNewQuestion(question);
                }
                await this.Navigation.PushAsync(levelPage);
            }
            else
            {
                await this.DisplayAlert("Hold on!", "Before you can create your own custom levels, you have to create your own account.", "Ok");
            }
        }

        private void ButtonCreateLevel_Clicked(object sender, EventArgs e)
        {
            if (CredentialManager.IsLoggedIn)
            {
                CreateNewLevelPage level = new CreateNewLevelPage(category);
                this.Navigation.PushAsync(level);
            }
            else
            {
                this.DisplayAlert("Hold on!", "Before you can create your own custom levels, you have to create your own account.", "Ok");
            }
        }
    }
}