using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountSettingsPage : ContentPage
    {
        /// <summary>
        /// The currently open frame
        /// </summary>
        private Frame currentlyOpenFrame;

        /// <summary>
        /// If the task/operation in the current frame is running.
        /// </summary>
        private bool currentlyOpenFrameIsRunning;

        /// <summary>
        /// handles user signout
        /// </summary>
        /// <param name="source"></param>
        /// <param name="eventArgs"></param>
        public delegate void SignOutEventHandler(object source, EventArgs eventArgs);

        /// <summary>
        /// handles user signout
        /// </summary>
        public event SignOutEventHandler SignedOut;

        /// <summary>
        /// default constructor
        /// </summary>
        public AccountSettingsPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// sets the confirm email to visible or not based on need for it
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.FrameConfirmEmail.IsVisible = !CredentialManager.EmailConfirmed;
        }

        /// <summary>
        /// Log the user out of the account.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLogout_Clicked(object sender, EventArgs e)
        {
            this.ButtonLogout.IsEnabled = false;
            CredentialManager.Logout(true);
            this.OnSignedOut();
            this.Navigation.PopAsync();
        }

        /// <summary>
        /// When signed out, triggers the signout handling
        /// </summary>
        protected virtual void OnSignedOut()
        {
            this.SignedOut?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Change the password of the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonChangePassword_Clicked(object sender, EventArgs e)
        {
            if (this.SetupFrameBegin(this.FrameChangePassword, this.StackLayoutChangePasswordContent))
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
                            OperationReturnMessage message = await Task.Run(() => this.ChangePassword(oldPassword, newPassword));
                            if (message == OperationReturnMessage.True)
                            {
                                this.LabelChangePasswordMessage.Text = "Password changed successfully.";
                                CredentialManager.SaveCredentialAsync(CredentialManager.Username, newPassword, CredentialManager.EmailConfirmed);
                            }
                            else if (message == OperationReturnMessage.FalseInvalidCredentials)
                            {
                                this.LabelChangePasswordMessage.Text = "Incorrect current password.";
                            }
                            else if (message == OperationReturnMessage.FalseNoConnection)
                            {
                                this.LabelDeleteAccountMessage.Text = "Failed to connect to server. Please try again.";
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
            this.SetupFrameEnd(this.StackLayoutChangePasswordContent);
        }

        /// <summary>
        /// Run the Change Password operation in a background task.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        private OperationReturnMessage ChangePassword(string password, string newPassword)
        {
            Device.BeginInvokeOnMainThread(() => this.LabelChangePasswordMessage.Text = "Waiting...");
            return ServerOperations.ChangePassword(CredentialManager.Username, password.Trim(), newPassword.Trim());
        }

        /// <summary>
        /// Change the email of the account.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonChangeEmail_Clicked(object sender, EventArgs e)
        {
            if (this.SetupFrameBegin(this.FrameChangeEmail, this.StackLayoutChangeEmailContent))
            {
                OperationReturnMessage message = await Task.Run(() => this.ChangeEmail(
                    this.EntryEnterPasswordChangeEmail.Text,
                    this.EntryEnterNewEmailChangeEmail.Text));

                if (message == OperationReturnMessage.TrueConfirmEmail)
                {
                    this.LabelChangeEmailMessage.Text = "Email Changed. Please Confirm Email.";
                    EmailConfirmationPage emailConfirmationPage = new EmailConfirmationPage(
                        CredentialManager.Username, this.EntryEnterPasswordChangeEmail.Text);
                    emailConfirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                    emailConfirmationPage.ConfirmLaterSelected += this.OnConfirmLaterSelected;
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
                else if (message == OperationReturnMessage.FalseNoConnection)
                {
                    this.LabelDeleteAccountMessage.Text = "Failed to connect to server. Please try again.";
                }
            }
            this.SetupFrameEnd(this.StackLayoutChangeEmailContent);
        }

        /// <summary>
        /// Change the user's email in a background task.
        /// </summary>
        /// <param name="password">The current password of the account.</param>
        /// <param name="newEmail">The new email.</param>
        /// <returns></returns>
        private OperationReturnMessage ChangeEmail(string password, string newEmail)
        {
            Device.BeginInvokeOnMainThread(() => this.LabelChangeEmailMessage.Text = "Waiting...");
            return ServerOperations.ChangeEmail(CredentialManager.Username, password.Trim(), newEmail.Trim());
        }

        /// <summary>
        /// Confirm the email associated with the account.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonConfirmEmail_Clicked(object sender, EventArgs e)
        {
            if (this.SetupFrameBegin(this.FrameConfirmEmail, this.StackLayoutConfirmEmailContent))
            {
                string token = this.EntryEnterConfirmationCodeConfirmEmail.Text.Trim();
                OperationReturnMessage message = await Task.Run(() => this.ConfirmEmail(token));
                if (message == OperationReturnMessage.True)
                {
                    this.LabelConfirmEmailMessage.Text = "Email was confirmed.";
                    CredentialManager.EmailConfirmed = true;
                    this.FrameConfirmEmail.IsVisible = false;
                    await this.DisplayAlert("Email Confirmation", "Email was confirmed", "OK");
                }
                else if (message == OperationReturnMessage.FalseNoConnection)
                {
                    this.LabelDeleteAccountMessage.Text = "Failed to connect to server. Please try again.";
                }
                else
                {
                    this.LabelConfirmEmailMessage.Text = "Email confirmation code was incorrect.";
                }
            }
            this.SetupFrameEnd(this.StackLayoutConfirmEmailContent);
        }

        /// <summary>
        /// Confirm the email associated with the account in a background task.
        /// </summary>
        /// <param name="token">The confirmation token sent to the user's email.</param>
        /// <returns></returns>
        private OperationReturnMessage ConfirmEmail(string token)
        {
            Device.BeginInvokeOnMainThread(() => this.LabelConfirmEmailMessage.Text = "Waiting...");
            return ServerOperations.ConfirmEmail(CredentialManager.Username, token.Trim());
        }

        /// <summary>
        /// Delete the user's acccount.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonDeleteAccount_Clicked(object sender, EventArgs e)
        {
            if (this.SetupFrameBegin(this.FrameDeleteAccount, this.StackLayoutDeleteAccountContent))
            {
                bool confirmed = await this.DisplayAlert("Confirm Delete", "" +
                        "Are you sure you want to delete your account? Your account cannot be recovered, and all created quizzes will be deleted.",
                        "Yes", "No");
                if (confirmed)
                {
                    string password = this.EntryEnterPasswordDeleteAccount.Text.Trim();
                    OperationReturnMessage message = await Task.Run(() => this.DeleteAccount(password));
                    if (message == OperationReturnMessage.True)
                    {
                        CredentialManager.Logout(true);
                        await this.DisplayAlert("Account Deletion", "Account successfully deleted", "OK");
                        this.OnSignedOut();
                        await this.Navigation.PopAsync();
                    }
                    else if (message == OperationReturnMessage.FalseNoConnection)
                    {
                        this.LabelDeleteAccountMessage.Text = "Failed to connect to server. Please try again.";
                    }
                    else
                    {
                        this.LabelDeleteAccountMessage.Text = "Incorrect password. Please try again.";
                    }
                }
            }
            this.SetupFrameEnd(this.StackLayoutDeleteAccountContent);
        }

        /// <summary>
        /// Delete the user's account in a background task.
        /// </summary>
        /// <param name="password">the current password of the account.</param>
        /// <returns></returns>
        private OperationReturnMessage DeleteAccount(string password)
        {
            Device.BeginInvokeOnMainThread(() => this.LabelDeleteAccountMessage.Text = "Waiting...");
            return ServerOperations.DeleteAccount(CredentialManager.Username, password.Trim());
        }

        /// <summary>
        /// Handle when the email is confirmed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnEmailConfirmed(object sender, EventArgs eventArgs)
        {
            this.FrameConfirmEmail.IsVisible = false;
            this.LabelChangeEmailMessage.Text = "";
            this.StackLayoutChangeEmailContent.IsVisible = false;
        }

        /// <summary>
        /// Handle when the user chooses to confirm email later.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
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
                    this.SetupFrameEnd(contentStackLayout);
                    return false;
                }
            }
            this.currentlyOpenFrameIsRunning = true;
            this.DisableOtherFrames(frame);
            return true;
        }

        /// <summary>
        /// sets up the frames
        /// </summary>
        /// <param name="contentStackLayout"></param>
        private void SetupFrameEnd(StackLayout contentStackLayout)
        {
            if (this.currentlyOpenFrameIsRunning)
            {
                this.EnableAllFrames();
                this.currentlyOpenFrameIsRunning = false;
            }

            contentStackLayout.IsEnabled = true;
        }

        /// <summary>
        /// when one frame is open, the rest are locked closed
        /// </summary>
        /// <param name="keepMeEnabled"></param>
        private void DisableOtherFrames(View keepMeEnabled)
        {
            for (int i = 0; i < this.StackLayoutMain.Children.Count; i++)
            {
                if (this.StackLayoutMain.Children[i] != keepMeEnabled)
                {
                    if (this.StackLayoutMain.Children[i] is Frame tryFrame)
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

        /// <summary>
        /// When all frames are closed, they are all available to be opened
        /// </summary>
        private void EnableAllFrames()
        {
            for (int i = 0; i < this.StackLayoutMain.Children.Count; i++)
            {
                if (this.StackLayoutMain.Children[i] is Frame tryFrame)
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

        /// <summary>
        /// Closes the currently open frame
        /// </summary>
        /// <returns></returns>
        private async Task CloseCurrentlyOpenFrameAsync()
        {
            if (this.currentlyOpenFrame != null)
            {
                ImageButton imageButton = ((this.currentlyOpenFrame.Content as StackLayout).Children[0] as StackLayout).Children[1] as ImageButton;
                StackLayout contentStackLayout = (this.currentlyOpenFrame.Content as StackLayout).Children[1] as StackLayout;

                await this.CloseFrame(((this.currentlyOpenFrame.Content as StackLayout).Children[0] as StackLayout).Children[1] as ImageButton,
                    contentStackLayout,
                    this.currentlyOpenFrame);

                // Clear fields if the open frame is not performing a task
                if (!this.currentlyOpenFrameIsRunning)
                {
                    this.ClearContentStack(contentStackLayout);
                }
            }
        }
        
        /// <summary>
        /// gets rid of the elements in the stack
        /// </summary>
        /// <param name="contentStackLayout"></param>
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

        /// <summary>
        /// manages frame animations
        /// </summary>
        /// <param name="imageButton">The button that triggered the animation</param>
        /// <param name="contentStack">The stack inside the frame</param>
        /// <param name="frame">The frame itself</param>
        /// <returns></returns>
        private async Task AnimateFrameAsync(ImageButton imageButton, StackLayout contentStack, Frame frame)
        {
            imageButton.IsEnabled = false;

            if (contentStack.IsVisible) // close
            {
                if (this.currentlyOpenFrame == frame)
                {
                    if (!this.currentlyOpenFrameIsRunning)
                    {
                        this.ClearContentStack((frame.Content as StackLayout).Children[1] as StackLayout);
                    }
                }

                this.currentlyOpenFrame = null;

                await this.CloseFrame(imageButton, contentStack, frame);
            }
            else // open
            {
                await this.CloseCurrentlyOpenFrameAsync();
                this.currentlyOpenFrame = frame;
                await this.OpenFrameAsync(imageButton, contentStack, frame);
            }

            imageButton.IsEnabled = true;
        }

        /// <summary>
        /// Animates the opening of the frame
        /// </summary>
        /// <param name="imageButton">The button that triggered the opening</param>
        /// <param name="contentStack">The stack inside the frame</param>
        /// <param name="frame">The frame itself</param>
        /// <returns></returns>
        private async Task OpenFrameAsync(ImageButton imageButton, StackLayout contentStack, Frame frame)
        {
            ((frame.Content as StackLayout).Children[0] as StackLayout).IsEnabled = false;
            _ = contentStack.FadeTo(1, 250, Easing.CubicInOut);

            contentStack.IsVisible = true;

            await imageButton.RelRotateTo(180);
            await frame.LayoutTo(new Rectangle(frame.X,
                frame.Y,
                frame.Width,
                frame.Height + contentStack.HeightRequest), 100, Easing.CubicInOut);
            ((frame.Content as StackLayout).Children[0] as StackLayout).IsEnabled = true;
        }

        /// <summary>
        /// Animates the closing of the frame
        /// </summary>
        /// <param name="imageButton">The button that triggered the opening</param>
        /// <param name="contentStack">The stack inside the frame</param>
        /// <param name="frame">The frame itself</param>
        /// <returns></returns>
        private async Task CloseFrame(ImageButton imageButton, StackLayout contentStack, Frame frame)
        {
            ((frame.Content as StackLayout).Children[0] as StackLayout).IsEnabled = false;
            _ = contentStack.FadeTo(0, 175, Easing.CubicInOut);
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
        /// <summary>
        /// opens the change email frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeEmailFrame_Clicked(object sender, EventArgs e)
        {
            await this.AnimateFrameAsync(this.ImageButtonCloseChangeEmail, this.StackLayoutChangeEmailContent, this.FrameChangeEmail);
        }

        /// <summary>
        /// opens the change password frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangePasswordFrame_Clicked(object sender, EventArgs e)
        {
            await this.AnimateFrameAsync(this.ImageButtonCloseChangePassword, this.StackLayoutChangePasswordContent, this.FrameChangePassword);
        }

        /// <summary>
        /// opens the confirm email frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConfirmEmailFrame_Clicked(object sender, EventArgs e)
        {
            await this.AnimateFrameAsync(this.ImageButtonCloseConfirmEmail, this.StackLayoutConfirmEmailContent, this.FrameConfirmEmail);
        }

        /// <summary>
        /// opens the delete account frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteAccountFrame_Clicked(object sender, EventArgs e)
        {
            await this.AnimateFrameAsync(this.ImageButtonDeleteAccount, this.StackLayoutDeleteAccountContent, this.FrameDeleteAccount);
        }

        #endregion
    }
}