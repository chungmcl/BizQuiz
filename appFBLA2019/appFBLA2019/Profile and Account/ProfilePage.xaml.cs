using Plugin.FacebookClient;
using Plugin.FacebookClient.Abstractions;
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
	public partial class ProfilePage : ContentPage
	{
		public ProfilePage ()
		{
			InitializeComponent ();
		}

        private async void ButtonToLoginPage_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new LoginPage());
        }

        private async void ButtonShare_Clicked(object sender, EventArgs e)
        {
            FacebookShareLinkContent linkContent = new FacebookShareLinkContent("Check out my github", new Uri("https://github.com/chungmcl"));
            var ret = await CrossFacebookClient.Current.ShareAsync(linkContent);
        }
    }
}