using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AccountSettingsPage : ContentPage
	{
        private Frame currentlyOpenFrame;
        public delegate void SignOutEventHandler(object source, EventArgs eventArgs);
        public event SignOutEventHandler SignedOut;
        public AccountSettingsPage ()
		{
			InitializeComponent ();
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.FrameConfirmEmail.IsVisible = !CredentialManager.EmailConfirmed;
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

        private async void ButtonChangeEmail_Clicked(object sender, EventArgs e)
        {
            this.ButtonChangeEmail.IsEnabled = false;
            this.EntryEnterPasswordChangeEmail.IsEnabled = false;
            this.EntryEnterNewEmailChangeEmail.IsEnabled = false;

            if ((this.EntryEnterPasswordChangeEmail.Text == null || this.EntryEnterNewEmailChangeEmail.Text == null) || 
                (this.EntryEnterPasswordChangeEmail.Text == "" || this.EntryEnterNewEmailChangeEmail.Text == ""))
            {
                this.LabelChangeEmailMessage.Text = "Fields cannot be empty.";
            }
            else
            {
                OperationReturnMessage message = await Task.Run(() => ChangeEmail(
                    this.EntryEnterPasswordChangeEmail.Text,
                    this.EntryEnterNewEmailChangeEmail.Text));

                if (message == OperationReturnMessage.TrueConfirmEmail)
                {
                    this.LabelChangeEmailMessage.Text = "Email Changed. Please Confirm Email.";
                    EmailConfirmationPage emailConfirmationPage = new EmailConfirmationPage(
                        CredentialManager.Username, this.EntryEnterPasswordChangeEmail.Text);
                    emailConfirmationPage.EmailConfirmed += OnEmailConfirmed;
                    emailConfirmationPage.ConfirmLaterSelected += OnConfirmLaterSelected;
                    await this.Navigation.PushModalAsync(emailConfirmationPage, true);
                }
                else if (message == OperationReturnMessage.FalseInvalidCredentials)
                {
                    this.LabelChangeEmailMessage.Text = "Incorrect password.";
                }
                else if (message == OperationReturnMessage.FalseInvalidEmail)
                {
                    this.LabelChangeEmailMessage.Text = "Invalid email. Please check the email and try again.";
                }
            }
            
            this.ButtonChangeEmail.IsEnabled = true;
            this.EntryEnterPasswordChangeEmail.IsEnabled = true;
            this.EntryEnterNewEmailChangeEmail.IsEnabled = true;
        }
        
        private OperationReturnMessage ChangeEmail(string password, string newEmail)
        {
            Device.BeginInvokeOnMainThread(() => this.LabelChangeEmailMessage.Text = "Waiting...");
            Thread.Sleep(5000);
            ServerConnector.SendData(ServerRequestTypes.ChangeEmail,
                    $"{CredentialManager.Username}/{password.Trim()}/{newEmail.Trim()}/-");
            return ServerConnector.ReceiveFromServerORM();
        }


        private void OnEmailConfirmed(object sender, EventArgs eventArgs)
        {
            this.FrameConfirmEmail.IsVisible = false;
            this.LabelChangeEmailMessage.Text = "";
            this.StackLayoutChangeEmailContent.IsVisible = false;
        }

        private void OnConfirmLaterSelected(object sender, EventArgs eventArgs)
        {
            this.FrameConfirmEmail.IsVisible = true;
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

        private async Task CloseCurrentlyOpenFrame()
        {
            if (this.currentlyOpenFrame != null)
            {
                ImageButton imageButton = ((this.currentlyOpenFrame.Content as StackLayout).Children[0] as StackLayout).Children[1] as ImageButton;
                StackLayout contentStackLayout = (this.currentlyOpenFrame.Content as StackLayout).Children[1] as StackLayout;

                await CloseFrame(((this.currentlyOpenFrame.Content as StackLayout).Children[0] as StackLayout).Children[1] as ImageButton,
                    contentStackLayout,
                    this.currentlyOpenFrame);
            }
        }

        #region Image Button Event Handlers
        private async void ImageButtonCloseChangeEmail_Clicked(object sender, EventArgs e)
        {
            await AnimateFrame(this.ImageButtonCloseChangeEmail, this.StackLayoutChangeEmailContent, this.FrameChangeEmail);
        }

        private async void ImageButtonCloseChangePassword_Clicked(object sender, EventArgs e)
        {
            await AnimateFrame(this.ImageButtonCloseChangePassword, this.StackLayoutChangePasswordContent, this.FrameChangePassword);
        }

        private async void ImageButtonCloseConfirmEmail_Clicked(object sender, EventArgs e)
        {
            await AnimateFrame(this.ImageButtonCloseConfirmEmail, this.StackLayoutConfirmEmailContent, this.FrameConfirmEmail);
        }

        private async void ImageButtonDeleteAccount_Clicked(object sender, EventArgs e)
        {
            await AnimateFrame(this.ImageButtonDeleteAccount, this.StackLayoutDeleteAccountContent, this.FrameDeleteAccount);
        }

        private async Task AnimateFrame(ImageButton imageButton, StackLayout contentStack, Frame frame)
        {
            imageButton.IsEnabled = false;
            
            if (contentStack.IsVisible) // close
            {
                this.currentlyOpenFrame = null;
                await CloseFrame(imageButton, contentStack, frame);
            }
            else // open
            {
                await CloseCurrentlyOpenFrame();
                this.currentlyOpenFrame = frame;
                await OpenFrame(imageButton, contentStack, frame);
            }

            imageButton.IsEnabled = true;
        }

        private async Task OpenFrame(ImageButton imageButton, StackLayout contentStack, Frame frame)
        {
            contentStack.FadeTo(1, 250, Easing.CubicInOut);

            contentStack.IsVisible = true;

            await imageButton.RelRotateTo(180);
            await frame.LayoutTo(new Rectangle(frame.X,
                frame.Y,
                frame.Width,
                frame.Height + contentStack.HeightRequest), 100, Easing.CubicInOut);
        }

        private async Task CloseFrame(ImageButton imageButton, StackLayout contentStack, Frame frame)
        {
            contentStack.FadeTo(0, 175, Easing.CubicInOut);
            imageButton.RelRotateTo(180);
            await frame.LayoutTo(new Rectangle(frame.X,
                frame.Y,
                frame.Width,
                frame.Height - contentStack.Height), 200, Easing.CubicInOut);

            contentStack.IsVisible = false;
        }
        #endregion

        // To do:
        // Change email
        // Change password
        // Confirm Email (IsVisible only if user needs to confirm email)
        // Delete account permanently
    }
}