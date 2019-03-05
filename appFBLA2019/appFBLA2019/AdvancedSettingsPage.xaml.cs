using System;
using System.Text.RegularExpressions;
using Xamarin.Essentials;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AdvancedSettingsPage : ContentPage
	{
        /// <summary>
        /// Default constructor
        /// </summary>
		public AdvancedSettingsPage ()
		{
			InitializeComponent ();
		}

        /// <summary>
        /// Triggered when the button to change the IP is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonConfirm_ClickedAsync(object sender, EventArgs e)
        {
            string newIP = this.EntryIP.Text.Trim();
            if (!string.IsNullOrWhiteSpace(newIP))
            {
                bool answer = await this.DisplayAlert("WARNING", "THIS ACTION WILL OVERRIDE THE DEFAULT IP ADDRESS TO THE SERVER. ONLY CONTINUE IF YOU KNOW WHAT YOU ARE DOING",  "Continue", "Cancel");
                if (answer)
                {
                    await SecureStorage.SetAsync("serverIP", newIP);
                    ServerConnector.Server = newIP;
                }
            }
        }

        /// <summary>
        /// Checks if the IP in the entry is well-formed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EntryIP_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ButtonConfirm.IsEnabled = Regex.IsMatch(this.EntryIP.Text, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");
        }
    }
}