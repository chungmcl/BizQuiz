//BizQuiz App 2019

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
        public LevelSelectionPage(string category)
        {
            this.InitializeComponent();
            this.category = category;
            Directory.CreateDirectory(App.Path + $"/{category}");
            // TO DO: Replace "DependencyService... .GetStorage()" with the location where the databases are being stored WHEN the app is is RELEASED (See DBHandler)
            Task.Run(() => this.Setup());
        }

        private readonly string category;
        // TO DO: Display author name of level
        internal async Task Setup()
        {
            this.ButtonStack.Children.Clear();

            string[] levelPaths = Directory.GetDirectories(App.Path + $"/{this.category}");
            List<string[]> levels = new List<string[]>();
            foreach (string levelName in levelPaths)
            {
                if (levelName.Contains('`'))
                {
                    levels.Add(new string[] { levelName.Split('/').Last().Split('`').First(), levelName.Split('/').Last().Split('`').Last() });
                }
            }

            foreach (string[] level in levels)
            {
                Frame frame = new Frame()
                {
                    HeightRequest = 100,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    CornerRadius = 10
                };
                StackLayout frameStack = new StackLayout
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    Orientation = StackOrientation.Horizontal
                };

                Label title = new Label
                {
                    Text = level.First(),
                    FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    HorizontalOptions = LayoutOptions.StartAndExpand
                };
                frameStack.Children.Add(title);

                TapGestureRecognizer recognizer = new TapGestureRecognizer();
                recognizer.Tapped += async (object sender, EventArgs e) =>
                {
                    Level newLevel = new Level(this.category, level.First(), level.Last());
                    newLevel.LoadQuestions();
                    await this.Navigation.PushAsync(new TextGame(newLevel));
                };

                frame.GestureRecognizers.Add(recognizer);
                frame.Content = frameStack;
                this.ButtonStack.Children.Add(frame);
            }
        }
    }
}