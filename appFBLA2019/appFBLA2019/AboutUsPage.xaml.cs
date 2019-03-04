//BizQuiz App 2019

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutUsPage : ContentPage
    {
        public AboutUsPage()
        {
            this.InitializeComponent();
        }

        private void GithubButton_Clicked(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://github.com/chungmcl"));
        }

        private async void EmailButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                Device.OpenUri(new Uri("mailto:chungmcl@hotmail.com?subject=BizQuiz&body=I really enjoyed your Bizquiz app!"));
            }
            catch
            {
                await this.DisplayAlert("Couldn't email", "You don't have an email app installed!", "OK");
            }
        }

        private void FacebookButton_Clicked(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://www.facebook.com/appdev.bhs.1"));
        }

        private void BugReportButton_Clicked(object sender, EventArgs e)
        {
            this.Navigation.PushAsync(new BugReportPage());
        }
    }
}