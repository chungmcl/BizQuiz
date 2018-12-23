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
            // Replace "DependencyService... .GetStorage()" with the location where the databases are being stored
            // when the app is is released (See DBHandler)
            DirectoryInfo dInfo = new DirectoryInfo(DependencyService.Get<IGetStorage>().GetStorage());

            FileInfo[] files = dInfo.GetFiles("*.realm");
            List<string> levels = new List<string>();
            foreach (FileInfo file in files)
            {
                levels.Add((file.Name.Split('.'))[0]);
            }
            this.Setup(levels.ToArray());
            DBHandler.SelectDatabase("test");
        }

        private void Setup(string[] levels)
        {
            for (int i = 0; i < levels.Count(); i++)
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
                    Text = levels[i],
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
                
                double avgScore = Level.GetLevelAvgScore(levels[i]);

                (frameStack.Children[1] as Label).Text = avgScore.ToString("00.0") ?? "0%";

                TapGestureRecognizer recognizer = new TapGestureRecognizer();
                recognizer.Tapped += async (object sender, EventArgs e) =>
                {
                    //messy but the best i have
                    Level level = new Level
                    ((((sender as Frame).Content as StackLayout).Children[0] as Label).Text);
                    level.LoadQuestions();
                    await this.Navigation.PushAsync(new TextGame(level));
                };

                frame.GestureRecognizers.Add(recognizer);
                frame.Content = frameStack;
                this.ButtonStack.Children.Add(frame);
            }
        }

        //public LevelSelectionPage()
        //{ }
    }
}