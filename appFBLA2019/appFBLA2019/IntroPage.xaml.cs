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
	public partial class IntroPage : CarouselPage
	{
		public IntroPage ()
		{
			InitializeComponent ();
		}

        private async void Button_ClickedAsync(object sender, EventArgs e)
        {
            if (this.Navigation.NavigationStack.Count > 0)
            {
                await this.Navigation.PopAsync();
            }
        }
        
    }
}