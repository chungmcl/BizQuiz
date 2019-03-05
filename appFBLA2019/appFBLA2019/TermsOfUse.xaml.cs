//BizQuiz App 2019

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TermsOfUse : ContentPage
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public TermsOfUse()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Opens the email app to email Micheal about the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Opens our facebook page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FacebookButton_Clicked(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("https://www.facebook.com/appdev.bhs.1"));
        }
    }
}