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
	public partial class ForgotPasswordPage : ContentPage
	{
        private const int minLength = 5;
        private string username;

        public ForgotPasswordPage ()
		{
			InitializeComponent ();
            this.username = "";

        }

        private async void ButtonContinue_Clicked(object sender, EventArgs e)
        {
            this.username = this.EntryUsername.Text.Trim();
            OperationReturnMessage message = await Task.Run(() => ServerOperations.ForgotPassword(this.username));
            if (message == OperationReturnMessage.True)
            {
                this.StackLayoutResetPassword.IsVisible = true;
            }
            else if (message == OperationReturnMessage.FalseInvalidCredentials)
            {
                this.LabelMessage.Text = "Username was not found.";
            }
            else
            {
                this.LabelMessage.Text = "Could not connect to server. Please try again.";
            }

        }

        private void ButtonChangePassword_Clicked(object sender, EventArgs e)
        {
            if (this.EntryPassword.Text == this.EntryReenterPassword.Text && this.EntryPassword.Text.Length > minLength)
            {
                OperationReturnMessage message = 
                    ServerOperations.ForgotPasswordChangePassword(this.username, this.EntryCode.Text.Trim(), this.EntryPassword.Text.Trim());
            }
            else
            {
                DisplayAlert("Invalid fields", "Passwords do not match or are not greater than five characters or less than 32 characters", "Ok");
            }
        }
    }
}