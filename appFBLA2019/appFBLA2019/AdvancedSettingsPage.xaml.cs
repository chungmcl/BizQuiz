using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AdvancedSettingsPage : ContentPage
	{
		public AdvancedSettingsPage ()
		{
			InitializeComponent ();
		}

        private async void ButtonConfirm_ClickedAsync(object sender, EventArgs e)
        {
            string newIP = this.EntryIP.Text;
            if (!string.IsNullOrWhiteSpace(newIP))
            {
                bool answer = await this.DisplayAlert("WARNING", "THIS ACTION WILL OVERRIDE THE CONNECTION TO THE SERVER.ONLY CONTINUE IF YOU KNOW WHAT YOU ARE DOING",  "Continue", "Cancel");
                if (answer)
                {
                    // change IP
                }
            }
        }

        private void EntryIP_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ButtonConfirm.IsEnabled = Regex.IsMatch(this.EntryIP.Text, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");
        }
    }
}