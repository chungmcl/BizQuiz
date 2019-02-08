using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LevelSelectionPage : ContentPage
	{
        public LevelSelectionPage()
		{
            this.InitializeComponent ();
            // TO DO: Replace "DependencyService... .GetStorage()" with the location where the databases are being stored
            // WHEN the app is is RELEASED (See DBHandler)
            string[] subFolderNames = Directory.GetDirectories(App.Path);
            List<string[]> levels = new List<string[]>();
            foreach (string levelName in subFolderNames)
            {
                if (levelName.Contains('`'))
                    levels.Add(new string[] { (levelName.Remove(0, App.Path.Length + 1).Split('`'))[0], (levelName.Remove(0, App.Path.Length).Split('`'))[1] });
            }
            this.Setup(levels);
        }

        // TO DO: Display author name of level
        private void Setup(List<string[]> levels)
        {
            foreach (string[] level in levels)
            {
                Frame frame = new Frame()
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
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
                    Text = level[0],
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

                DBHandler.SelectDatabase(level[0], level[1]);

                TapGestureRecognizer recognizer = new TapGestureRecognizer();
                recognizer.Tapped += async (object sender, EventArgs e) =>
                {
                    Level newLevel = new Level(level[0], level[1]);
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