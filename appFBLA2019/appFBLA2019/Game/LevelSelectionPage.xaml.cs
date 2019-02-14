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
    /// <summary>
    /// A page that shows a selection of levels from the specified category
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LevelSelectionPage : ContentPage
    {
        /// <summary>
        /// Constructor that creates a levelselectionpage, connects to the database for the category, and sets up the cards
        /// </summary>
        /// <param name="category">
        /// </param>
        public LevelSelectionPage(string category)
        {
            this.InitializeComponent();
            this.category = category;
            Directory.CreateDirectory(App.Path + $"/{category}");
            // TO DO: Replace "DependencyService... .GetStorage()" with the location where the databases are being stored WHEN the app is is RELEASED (See DBHandler)
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await Task.Run(() => this.Setup());
        }

        /// <summary>
        /// Default constructor that specifies "other" as the category
        /// </summary>
        public LevelSelectionPage() : this("Other") { }

        /// <summary>
        /// Loads all questions from the current category and displays them as cards with some options
        /// </summary>
        /// <returns>
        /// an awaitable task to setup the page
        /// </returns>
        internal void Setup()
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

                frameStack.Children.Add(new Label
                {
                    HorizontalOptions = LayoutOptions.End
                });

                TapGestureRecognizer recognizer = new TapGestureRecognizer();
                recognizer.Tapped += async (object sender, EventArgs e) =>
                {
                    Level newLevel = new Level(this.category, level.First(), level.Last());
                    newLevel.LoadQuestions();
                    await this.Navigation.PushAsync(new Game(newLevel));
                };

                frame.GestureRecognizers.Add(recognizer);
                frame.Content = frameStack;
                this.ButtonStack.Children.Add(frame);
            }
        }

        /// <summary>
        /// The category of the page
        /// </summary>
        private readonly string category;
    }
}