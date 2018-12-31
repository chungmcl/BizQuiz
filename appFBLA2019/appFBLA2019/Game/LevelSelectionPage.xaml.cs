﻿using System;
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
            string[] subFolderNames = Directory.GetDirectories(DependencyService.Get<IGetStorage>().GetStorage());
            List<string[]> levels = new List<string[]>();
            foreach (string levelName in subFolderNames)
            {
                levels.Add( new string[] { (levelName.Split('`'))[0], (levelName.Split('`'))[1] });
            }
            this.Setup(levels);
        }

        // TO DO: Display author name of level
        private void Setup(List<string[]> levels)
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
                    Text = levels[i][0],
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
                
                double avgScore = Level.GetLevelAvgScore(levels[i][0], levels[i][1]);

                (frameStack.Children[1] as Label).Text = avgScore.ToString("00.0") ?? "0%";

                TapGestureRecognizer recognizer = new TapGestureRecognizer();
                recognizer.Tapped += async (object sender, EventArgs e) =>
                {
                    //messy but the best i have
                    Level level = new Level
                    ((((sender as Frame).Content as StackLayout).Children[0] as Label).Text, levels[i][1]);
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