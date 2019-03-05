using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ForgotPasswordPage : ContentPage
	{
        /// <summary>
        /// minimum lenght for a new password
        /// </summary>
        private const int minLength = 5;
        /// <summary>
        /// the username of current user
        /// </summary>
        private string username;

        /// <summary>
        /// default constructor
        /// </summary>
        public ForgotPasswordPage ()
		{
			this.InitializeComponent ();
            this.username = "";

        }

        /// <summary>
        /// Triggered when continue is clicked; checks if the user has an account to recover password for
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonContinue_Clicked(object sender, EventArgs e)
        {
            this.ActivityIndicator.IsVisible = true;
            this.ActivityIndicator.IsRunning = true;
            this.LabelMessage.Text = "";
            if (this.EntryUsername.Text != null)
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
            else
            {
                await this.DisplayAlert("Empty Field", "Username cannot be empty.", "Ok");
            }
            this.ActivityIndicator.IsVisible = false;
            this.ActivityIndicator.IsRunning = false;
        }

        /// <summary>
        /// Triggered when change password is clicked; asks the server to change the password
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonChangePassword_Clicked(object sender, EventArgs e)
        {
            this.ActivityIndicator.IsVisible = true;
            this.ActivityIndicator.IsRunning = true;
            if (this.EntryPassword.Text == this.EntryReenterPassword.Text && this.EntryPassword.Text.Length > minLength)
            {
                OperationReturnMessage message = 
                    ServerOperations.ForgotPasswordChangePassword(this.username, this.EntryCode.Text.Trim(), this.EntryPassword.Text.Trim());
                if (message == OperationReturnMessage.True)
                {
                    await this.DisplayAlert("Password Changed", "Your password was sucessfully changed.", "Ok");
                    await this.Navigation.PopModalAsync();
                }
                else if (message == OperationReturnMessage.FalseInvalidCredentials)
                {
                    await this.DisplayAlert("Incorrect Code", "Your code was incorrect. Please try again.", "Ok");
                }
                else
                {
                    await this.DisplayAlert("Failed", "Your password change was failed. Please try again.", "Ok");
                }
            }
            else
            {
                await this.DisplayAlert("Invalid fields", "Passwords do not match or are not greater than five characters or less than 32 characters", "Ok");
            }
            this.ActivityIndicator.IsVisible = false;
            this.ActivityIndicator.IsRunning = false;
        }
    }
}