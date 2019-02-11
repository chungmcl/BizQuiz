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
	public partial class AccountSettingsPage : ContentPage
	{
        public delegate void SignOutEventHandler(object source, EventArgs eventArgs);
        public event SignOutEventHandler SignedOut;
        public AccountSettingsPage ()
		{
			InitializeComponent ();
		}

        private void ButtonLogout_Clicked(object sender, EventArgs e)
        {
            this.ButtonLogout.IsEnabled = false;
            CredentialManager.Logout();
            OnSignedOut();
            this.Navigation.PopAsync();
        }

        protected virtual void OnSignedOut()
        {
            this.SignedOut?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonChangeEmail_Clicked(object sender, EventArgs e)
        {

        }

        private void ButtonChangePassword_Clicked(object sender, EventArgs e)
        {

        }

        private void ButtonConfirmEmail_Clicked(object sender, EventArgs e)
        {

        }

        private void ButtonDeleteAccount_Clicked(object sender, EventArgs e)
        {

        }

        #region Image Button Event Handlers
        private void ImageButtonCloseChangeEmail_Clicked(object sender, EventArgs e)
        {
            this.ImageButtonCloseChangeEmail.RelRotateTo(180);
            this.StackLayoutChangeEmailContent.IsVisible = !this.StackLayoutChangeEmailContent.IsVisible;
        }

        private void ImageButtonCloseChangePassword_Clicked(object sender, EventArgs e)
        {
            this.ImageButtonCloseChangePassword.RelRotateTo(180);
            this.StackLayoutChangePasswordContent.IsVisible = !this.StackLayoutChangePasswordContent.IsVisible;
        }

        private void ImageButtonCloseConfirmEmail_Clicked(object sender, EventArgs e)
        {
            this.ImageButtonCloseConfirmEmail.RelRotateTo(180);
            this.StackLayoutConfirmEmailContent.IsVisible = !this.StackLayoutConfirmEmailContent.IsVisible;
        }

        private void ImageButtonDeleteAccount_Clicked(object sender, EventArgs e)
        {
            this.ImageButtonDeleteAccount.RelRotateTo(180);
            this.StackLayoutDeleteAccountContent.IsVisible = !this.StackLayoutDeleteAccountContent.IsVisible;
        }
        #endregion

        // To do:
        // Change email
        // Change password
        // Confirm Email (IsVisible only if user needs to confirm email)
        // Delete account permanently
    }
}