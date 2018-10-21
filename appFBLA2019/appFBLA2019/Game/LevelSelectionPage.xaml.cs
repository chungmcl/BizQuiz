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
            
            for (int i = 0; i < levels.Count(); i++)
            {
                Button button = new Button
                {
                    Text = levels[i],
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                };
                //adds an event handler to the button to deal with clicks
                button.Clicked += async (object sender, EventArgs e) =>
                    {
                        Level level = new Level((sender as Button).Text);
                        await level.LoadQuestionsAsync();
                        await this.Navigation.PushAsync(new TextGame(level));
                    };
                
                this.ButtonStack.Children.Add(button);
            }
        }

        public LevelSelectionPage()
        { }
    }
}