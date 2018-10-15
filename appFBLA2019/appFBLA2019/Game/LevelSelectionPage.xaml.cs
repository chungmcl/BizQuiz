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
		public LevelSelectionPage ()
		{
            this.InitializeComponent ();
		}

        private async void History_Clicked(object sender, EventArgs e)
        {
            Level level = new Level("test");
            await level.LoadQuestionsAsync();
            await this.Navigation.PushAsync(new TextGame(level));
        }
    }
}