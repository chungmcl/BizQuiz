﻿using System;
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
        private bool currentlyOpenFrameIsRunning;

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
            CredentialManager.Logout(false);
            OnSignedOut();
            this.Navigation.PopAsync();
        }

        protected virtual void OnSignedOut()
        {
            this.SignedOut?.Invoke(this, EventArgs.Empty);
        }

        private async void ButtonChangePassword_Clicked(object sender, EventArgs e)
        {
            if (SetupFrameBegin(this.FrameChangePassword, this.StackLayoutChangePasswordContent))
            {
                string oldPassword = this.EntryCurrentPasswordChangePassword.Text.Trim();
                string newPassword = this.EntryNewPasswordChangePassword.Text.Trim();
                string newPasswordReenter = this.EntryReenterNewPasswordChangePassword.Text.Trim();
                if (newPassword == newPasswordReenter)
                {
                    if (!(newPassword.Length < 8 || newPassword.Length > 16))
                    {
                        if (!(newPassword.Contains(" ") || newPassword.Contains(".") || newPassword.Contains("/") || newPassword.Contains("`")))
                        {
                            OperationReturnMessage message = await Task.Run(() => ChangePassword(oldPassword, newPassword));
                            if (message == OperationReturnMessage.True)
                            {
                                this.LabelChangePasswordMessage.Text = "Password changed successfully.";
                                CredentialManager.SaveCredential(CredentialManager.Username, newPassword, CredentialManager.EmailConfirmed);
                            }
                            else if (message == OperationReturnMessage.FalseInvalidCredentials)
                            {
                                this.LabelChangePasswordMessage.Text = "Incorrect current password.";
                            }
                            else
                            {
                                this.LabelChangePasswordMessage.Text = "Password change failed - Please try again.";
                            }
                        }
                        else
                        {
                            this.LabelChangePasswordMessage.Text =
                            "New password must not contain empty space, \'.\', \'/\', or \'`\'";
                        }
                    }
                    else
                    {
                        this.LabelChangePasswordMessage.Text = 
                            "New password must be greater than 8 characters and less than 16 characters long.";
                    }
                }
                else
                {
                    this.LabelChangePasswordMessage.Text = "New passwords do not match.";
                }
            }
            SetupFrameEnd(this.StackLayoutChangePasswordContent);
        }

        private OperationReturnMessage ChangePassword(string password, string newPassword)
        {
            Device.BeginInvokeOnMainThread(() => this.LabelChangePasswordMessage.Text = "Waiting...");
            ServerConnector.SendData(ServerRequestTypes.ChangePassword,
                    $"{CredentialManager.Username}/{password.Trim()}/{newPassword.Trim()}/-");
            return ServerConnector.ReceiveFromServerORM();
        }
        
        private async void ButtonChangeEmail_Clicked(object sender, EventArgs e)
        {
            if (SetupFrameBegin(this.FrameChangeEmail, this.StackLayoutChangeEmailContent))
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
                    this.LabelChangeEmailMessage.Text = "Incorrect password.";
                else if (message == OperationReturnMessage.FalseInvalidEmail)
                    this.LabelChangeEmailMessage.Text = "Invalid email. Please check the email and try again.";
            }
            SetupFrameEnd(this.StackLayoutChangeEmailContent);
        }

        private OperationReturnMessage ChangeEmail(string password, string newEmail)
        {
            Device.BeginInvokeOnMainThread(() => this.LabelChangeEmailMessage.Text = "Waiting...");
            ServerConnector.SendData(ServerRequestTypes.ChangeEmail,
                    $"{CredentialManager.Username}/{password.Trim()}/{newEmail.Trim()}/-");
            return ServerConnector.ReceiveFromServerORM();
        }

        private async void ButtonConfirmEmail_Clicked(object sender, EventArgs e)
        {
            if (SetupFrameBegin(this.FrameConfirmEmail, this.StackLayoutConfirmEmailContent))
            {
                string token = this.EntryEnterConfirmationCodeConfirmEmail.Text.Trim();
                OperationReturnMessage message = await Task.Run(() => ConfirmEmail(token));
                if (message == OperationReturnMessage.True)
                {
                    this.LabelChangeEmailMessage.Text = "Email was confirmed.";
                    CredentialManager.EmailConfirmed = true;
                    this.FrameConfirmEmail.IsVisible = false;
                    await DisplayAlert("Email Confirmation", "Email was confirmed", "OK");
                }
                else
                {
                    this.LabelChangeEmailMessage.Text = "Email confirmation code was incorrect.";
                }
            }
            SetupFrameEnd(this.StackLayoutConfirmEmailContent);
        }

        private OperationReturnMessage ConfirmEmail(string token)
        {
            Device.BeginInvokeOnMainThread(() => this.LabelConfirmEmailMessage.Text = "Waiting...");
            ServerConnector.SendData(ServerRequestTypes.ConfirmEmail,
                    $"{CredentialManager.Username}/{token.Trim()}/-");
            return ServerConnector.ReceiveFromServerORM();
        }

        private async void ButtonDeleteAccount_Clicked(object sender, EventArgs e)
        {
            if (SetupFrameBegin(this.FrameDeleteAccount, this.StackLayoutDeleteAccountContent))
            {
                string password = this.EntryEnterPasswordDeleteAccount.Text.Trim();
                OperationReturnMessage message = await Task.Run(() => DeleteAccount(password));
                if (message == OperationReturnMessage.True)
                {
                    CredentialManager.Logout(true);
                    await DisplayAlert("Account Deletion", "Account successfully deleted", "OK");
                    OnSignedOut();
                    await this.Navigation.PopAsync();
                }
                else
                {
                    this.LabelDeleteAccountMessage.Text = "Incorrect password. Please try again.";
                }
            }
            SetupFrameEnd(this.StackLayoutDeleteAccountContent);
        }

        private OperationReturnMessage DeleteAccount(string password)
        {
            Device.BeginInvokeOnMainThread(() => this.LabelDeleteAccountMessage.Text = "Waiting...");
            ServerConnector.SendData(ServerRequestTypes.DeleteAccount,
                    $"{CredentialManager.Username}/{password.Trim()}/-");
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

        #region UI Behavior Methods
        /// <summary>
        /// Setup the frame elements when button is pressed.
        /// </summary>
        /// <param name="frame">The frame</param>
        /// <param name="contentStackLayout">The content StackLayout of the frame</param>
        /// <returns>If the content contained data</returns>
        private bool SetupFrameBegin(Frame frame, StackLayout contentStackLayout)
        {
            contentStackLayout.IsEnabled = false;
            for (int i = 0; i < contentStackLayout.Children.Count - 2; i++)
            {
                string entryText = (contentStackLayout.Children[i] as Entry).Text;
                if (entryText == null || entryText == "")
                {
                    Label labelMessage = contentStackLayout.Children[contentStackLayout.Children.Count - 2] as Label;
                    labelMessage.Text = "Fields cannot be empty";
                    this.currentlyOpenFrameIsRunning = false;
                    SetupFrameEnd(contentStackLayout);
                    return false;
                }
            }
            this.currentlyOpenFrameIsRunning = true;
            DisableOtherTabs(frame);
            return true;
        }

        private void SetupFrameEnd(StackLayout contentStackLayout)
        {
            if (this.currentlyOpenFrameIsRunning)
            {
                EnableAllTabs();
                this.currentlyOpenFrameIsRunning = false;
            }

            contentStackLayout.IsEnabled = true;
        }

        private void DisableOtherTabs(View keepMeEnabled)
        {
            for (int i = 0; i < this.StackLayoutMain.Children.Count; i++)
            {
                if (this.StackLayoutMain.Children[i] != keepMeEnabled)
                {
                    Frame tryFrame = this.StackLayoutMain.Children[i] as Frame;
                    if (tryFrame != null)
                    {
                        ((tryFrame.Content
                            as StackLayout).Children[0]
                            as StackLayout).IsEnabled = false;
                        tryFrame.BackgroundColor = Color.Gray;
                    }
                }
            }
            this.ButtonLogout.BackgroundColor = Color.Gray;
            this.ButtonLogout.IsEnabled = false;
        }

        private void EnableAllTabs()
        {
            for (int i = 0; i < this.StackLayoutMain.Children.Count; i++)
            {
                Frame tryFrame = this.StackLayoutMain.Children[i] as Frame;
                if (tryFrame != null)
                {
                    ((tryFrame.Content
                             as StackLayout).Children[0]
                             as StackLayout).IsEnabled = true;
                    tryFrame.BackgroundColor = Color.Default;
                }
            }
            this.ButtonLogout.IsEnabled = true;
            this.ButtonLogout.BackgroundColor = Color.Accent;
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

                // Clear fields if the open frame is not performing a task
                if (!this.currentlyOpenFrameIsRunning)
                    ClearContentStack(contentStackLayout);
            }
        }

        private void ClearContentStack(StackLayout contentStackLayout)
        {
            // For each entry in the Frame (contentStack - button - label = entries)
            for (int i = 0; i < contentStackLayout.Children.Count - 2; i++)
            {
                Entry entry = contentStackLayout.Children[i] as Entry;
                entry.Text = "";
            }

            // Clear the label
            Label label = contentStackLayout.Children[contentStackLayout.Children.Count - 2] as Label;
            label.Text = "";
        }

        private async Task AnimateFrame(ImageButton imageButton, StackLayout contentStack, Frame frame)
        {
            imageButton.IsEnabled = false;

            if (contentStack.IsVisible) // close
            {
                if (this.currentlyOpenFrame == frame)
                    if (!this.currentlyOpenFrameIsRunning)
                        ClearContentStack((frame.Content as StackLayout).Children[1] as StackLayout);

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
            ((frame.Content as StackLayout).Children[0] as StackLayout).IsEnabled = false;
            contentStack.FadeTo(1, 250, Easing.CubicInOut);

            contentStack.IsVisible = true;

            await imageButton.RelRotateTo(180);
            await frame.LayoutTo(new Rectangle(frame.X,
                frame.Y,
                frame.Width,
                frame.Height + contentStack.HeightRequest), 100, Easing.CubicInOut);
            ((frame.Content as StackLayout).Children[0] as StackLayout).IsEnabled = true;
        }

        private async Task CloseFrame(ImageButton imageButton, StackLayout contentStack, Frame frame)
        {
            ((frame.Content as StackLayout).Children[0] as StackLayout).IsEnabled = false;
            contentStack.FadeTo(0, 175, Easing.CubicInOut);
            await imageButton.RelRotateTo(180);
            await frame.LayoutTo(new Rectangle(frame.X,
                frame.Y,
                frame.Width,
                frame.Height - contentStack.Height), 200, Easing.CubicInOut);

            contentStack.IsVisible = false;
            ((frame.Content as StackLayout).Children[0] as StackLayout).IsEnabled = true;
        }
        #endregion

        #region Image Button Event Handlers
        private async void ChangeEmailTab_Clicked(object sender, EventArgs e)
        {
            await AnimateFrame(this.ImageButtonCloseChangeEmail, this.StackLayoutChangeEmailContent, this.FrameChangeEmail);
        }

        private async void ChangePasswordTab_Clicked(object sender, EventArgs e)
        {
            await AnimateFrame(this.ImageButtonCloseChangePassword, this.StackLayoutChangePasswordContent, this.FrameChangePassword);
        }

        private async void ConfirmEmailTab_Clicked(object sender, EventArgs e)
        {
            await AnimateFrame(this.ImageButtonCloseConfirmEmail, this.StackLayoutConfirmEmailContent, this.FrameConfirmEmail);
        }

        private async void DeleteAccountTab_Clicked(object sender, EventArgs e)
        {
            await AnimateFrame(this.ImageButtonDeleteAccount, this.StackLayoutDeleteAccountContent, this.FrameDeleteAccount);
        }
        
        #endregion

        // To do:
        // Change email
        // Change password
        // Confirm Email (IsVisible only if user needs to confirm email)
        // Delete account permanently
    }
}