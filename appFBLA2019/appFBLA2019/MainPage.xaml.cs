using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace appFBLA2019
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : Xamarin.Forms.TabbedPage
	{
		public MainPage ()
		{
			InitializeComponent ();
#if __ANDROID__
            On<Xamarin.Forms.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
#endif
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new LoginPage(""));
        }
    }
}