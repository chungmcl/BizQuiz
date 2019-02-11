using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using System.IO;

namespace appFBLA2019
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : Xamarin.Forms.TabbedPage
	{
        private const int levelCatagoriesPage = 0;
        private const int levelEditorPage = 1;
        //private const int levelStorePage = 2;
        private const int profilePageIndex = 3;

		public MainPage ()
		{
			this.InitializeComponent ();
            // Default tabs on Android are on top - Set to bottom on Android (to serve as Navigation)
#if __ANDROID__
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetBarSelectedItemColor(Color.White);
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetBarItemColor(Color.FromHex("#003463"));
#endif
            this.BarTextColor = Color.White;            
        }

        private void TabbedPage_CurrentPageChanged(object sender, EventArgs e)
        {
            var index = this.Children.IndexOf(this.CurrentPage);

            // If your called method requires async code in initialization, define an "IsLoading" property in your page and change and check that accordingly
            switch (index)
            {
                case profilePageIndex:
                    {
                        // Reference to current instantiation of ProfilePage
                        ProfilePage profilePage = (ProfilePage)this.TabbedPagePage.Children[profilePageIndex];
                        if (!profilePage.IsLoading && !profilePage.IsOnLoginPage)
                            Task.Run(() => profilePage.UpdateProfilePage(true));
                    }
                    break;
            }
        }
    }
}