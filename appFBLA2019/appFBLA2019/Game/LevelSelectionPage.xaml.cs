using System;
using System.Collections.Generic;
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
        public LevelSelectionPage(String[] levels)
		{
            this.InitializeComponent ();

            Setup(levels);
            //for (int i = 0; i < levels.Count(); i++)
            //{
            //    Button button = new Button
            //    {
            //        Text = levels[i],
            //        VerticalOptions = LayoutOptions.CenterAndExpand,
            //        HorizontalOptions = LayoutOptions.CenterAndExpand
            //    };
            //    //adds an event handler to the button to deal with clicks
            //    button.Clicked += async (object sender, EventArgs e) =>
            //        {
            //            Level level = new Level((sender as Button).Text);
            //            await level.LoadQuestionsAsync();
            //            await this.Navigation.PushAsync(new TextGame(level));
            //        };

                //    this.ButtonStack.Children.Add(button);
                //}
        }

        private async void Setup(string[] levels)
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

                //This needs to be fixed, idk how to use Async with the Database (or in general)
                //Task<double> avgScore = Task.Run(() => Level.GetLevelAvgScore(levels[i]));
                //Task.WaitAll(avgScore);
                //(frameStack.Children[1] as Label).Text = avgScore.Result.ToString("00.0") ?? "0%";

                TapGestureRecognizer recognizer = new TapGestureRecognizer();
                recognizer.Tapped += async (object sender, EventArgs e) =>
                {
                    //messy but the best i have
                    Level level = new Level
                    ((((sender as Frame).Content as StackLayout).Children[0] as Label).Text);
                    await level.LoadQuestionsAsync();
                    await this.Navigation.PushAsync(new TextGame(level));
                };

                frame.GestureRecognizers.Add(recognizer);
                frame.Content = frameStack;
                ButtonStack.Children.Add(frame);
            }
        }

        public LevelSelectionPage()
        { }
    }
}