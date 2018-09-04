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
	public partial class TopicPage : ContentPage
	{
		public TopicPage ()
		{
			InitializeComponent ();
		}

        private void History_Clicked(object sender, EventArgs e)
        {
            this.Navigation.PushAsync(new TextGame(new Topic("Topics\\TestTopic.txt")));
        }
    }
}