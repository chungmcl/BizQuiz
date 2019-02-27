//BizQuiz App 2019

using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Text.RegularExpressions;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateAccountPage : ContentPage
    {
        public delegate void AccountCreatedEventHandler(object source, AccountCreatedEventArgs eventArgs);

        public event AccountCreatedEventHandler AccountCreated;

        private string username;
        private string password;

        private const int minLength = 5;
        private const int maxLength = 32;

        public CreateAccountPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Create a new account.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonCreateAccount_Clicked(object sender, EventArgs e)
        {
            this.ButtonCreateAccount.IsEnabled = false;
            this.EntryEmail.IsEnabled = false;
            this.EntryPassword.IsEnabled = false;
            this.EntryUsername.IsEnabled = false;
            this.ActivityIndicator.IsVisible = true;
            this.ActivityIndicator.IsRunning = true;

            string username = this.EntryUsername.Text.Trim();
            string password = this.EntryPassword.Text.Trim();
            string email = this.EntryEmail.Text.Trim();

            // Connect to the server in a background task and await a response.
            OperationReturnMessage response = await Task.Run(() => ServerOperations.RegisterAccount(username, password, email));

            if (response != OperationReturnMessage.FalseFailedConnection)
            {
                if (response == OperationReturnMessage.TrueConfirmEmail)
                {
                    this.LabelMessage.Text = "Account successfully created.";
                    this.username = username;
                    this.password = password;

                    var confirmationPage = new EmailConfirmationPage(username, password);
                    confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                    confirmationPage.ConfirmLaterSelected += this.OnConfirmLaterSelected;
                    await this.Navigation.PushModalAsync(confirmationPage);
                }
                else if (response == OperationReturnMessage.FalseUsernameAlreadyExists)
                {
                    this.LabelMessage.Text = $"Account could not be created - Username already exists.";
                }
                else if (response == OperationReturnMessage.FalseInvalidEmail)
                {
                    this.LabelMessage.Text = $"Account could not be created - Invalid Email.";
                }
                else
                {
                    this.LabelMessage.Text = $"Account could not be created.";
                }
            }
            else
            {
                this.LabelMessage.Text = "Connection failed: Please try again.";
            }

            this.ButtonCreateAccount.IsEnabled = true;
            this.EntryEmail.IsEnabled = true;
            this.EntryPassword.IsEnabled = true;
            this.EntryUsername.IsEnabled = true;
            this.ActivityIndicator.IsVisible = false;
            this.ActivityIndicator.IsRunning = false;
        }

        /// <summary>
        /// Handle when the user successfully confirms email.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="eventArgs"></param>
        private async void OnEmailConfirmed(object source, EventArgs eventArgs)
        {
            this.OnAccountCreated(true);
            await this.Navigation.PopAsync();
        }

        /// <summary>
        /// Handle when user chooses to confirm email later.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="eventArgs"></param>
        private async void OnConfirmLaterSelected(object source, EventArgs eventArgs)
        {
            this.OnAccountCreated(false);
            await this.Navigation.PopAsync();
        }

        /// <summary>
        /// Handle when the account is successfully created.
        /// </summary>
        /// <param name="emailConfirmed"></param>
        protected void OnAccountCreated(bool emailConfirmed)
        {
            CredentialManager.SaveCredential(this.username, this.password, emailConfirmed);
            this.AccountCreated?.Invoke(this,
                new AccountCreatedEventArgs
                {
                    EmailConfirmed = emailConfirmed,
                    Username = this.EntryUsername.Text.Trim()
                });
        }

        public class AccountCreatedEventArgs : EventArgs
        {
            public bool EmailConfirmed { get; set; }
            public string Username { get; set; }
        }

        /// <summary>
        /// If the username length fits the requirements.
        /// </summary>
        private bool usernameLength;
        /// <summary>
        /// If the password length fits the requirements.
        /// </summary>
        private bool passwordLength;
        /// <summary>
        /// If the email is a valid address.
        /// </summary>
        private bool emailCorrect;

        /// <summary>
        /// Show a "check" or "X" in UI to specify whether if the field is valid.
        /// </summary>
        /// <param name="newIcon">Name of the resource to change icon to.</param>
        /// <param name="oldIcon">Name of the resource the icon was.</param>
        /// <param name="image">Reference to the icon object.</param>
        /// <returns></returns>
        private async Task CheckIconAsync(string newIcon, string oldIcon, Image image)
        {
            if (image.Source.ToString().Split(':')[1].Trim() == oldIcon)
            {
                await image.FadeTo(0, 250, Easing.CubicInOut);
                image.Source = newIcon;
                await image.FadeTo(1, 250, Easing.CubicInOut);
            }
            else if (image.Opacity == 0)
                await image.FadeTo(1, 250, Easing.CubicInOut);
        }

        /// <summary>
        /// Check if entries are valid and enable or disable create account button accordingly.
        /// </summary>
        private void CheckEntries()
        {
            this.ButtonCreateAccount.IsEnabled = this.usernameLength && this.passwordLength && this.emailCorrect;
        }

        /// <summary>
        /// Handle when text in username entry is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EntryUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EntryUsername.Text.Length > minLength && EntryUsername.Text.Length <= maxLength)
            {
                this.usernameLength = true;
                await this.CheckIconAsync("ic_check_green_48dp.png", "ic_bad_red_48dp.png", this.checkUsername);
            }
            else
            {
                this.usernameLength = false;
                await this.CheckIconAsync("ic_bad_red_48dp.png", "ic_check_green_48dp.png", this.checkUsername);
            }
            this.CheckEntries();
        }

        /// <summary>
        /// Handle when text in password entry is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EntryPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EntryPassword.Text.Length > minLength && EntryPassword.Text.Length <= maxLength)
            {
                this.passwordLength = true;
                await this.CheckIconAsync("ic_check_green_48dp.png", "ic_bad_red_48dp.png", this.checkPassword);
            }
            else
            {
                this.passwordLength = false;
                await this.CheckIconAsync("ic_bad_red_48dp.png", "ic_check_green_48dp.png", this.checkPassword);
            }
            this.CheckEntries();
        }

        /// <summary>
        /// Regex string to check if email is in valid format.
        /// </summary>
        private string ComplexEmailPattern = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" // local-part
            + "@"
            + @"((([\w]+([-\w]*[\w]+)*\.)+[a-zA-Z]+)|" // domain
            + @"((([01]?[0-9]{1,2}|2[0-4][0-9]|25[0-5]).){3}[01]?[0-9]{1,2}|2[0-4][0-9]|25[0-5]))\z"; // or IP Address
        
        /// <summary>
        /// Handle when text in email entry is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EntryEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(Regex.IsMatch(EntryEmail.Text, ComplexEmailPattern))
            {
                this.emailCorrect = true;
                await this.CheckIconAsync("ic_check_green_48dp.png", "ic_bad_red_48dp.png", this.checkEmail);
            }
            else
            {
                this.emailCorrect = false;
                await this.CheckIconAsync("ic_bad_red_48dp.png", "ic_check_green_48dp.png", this.checkEmail);
            }
            this.CheckEntries();
        }
    }
}