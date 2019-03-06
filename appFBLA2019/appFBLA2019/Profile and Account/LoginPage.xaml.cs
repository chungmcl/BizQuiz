//BizQuiz App 2019

using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static appFBLA2019.CreateAccountPage;

namespace appFBLA2019
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentView
    {
        /// <summary>
        /// Handles when the user logs in
        /// </summary>
        /// <param name="source"></param>
        /// <param name="eventArgs"></param>
        public delegate void LoggedinEventHandler(object source, EventArgs eventArgs);

        /// <summary>
        /// Handles when the user logs in
        /// </summary>
        public event LoggedinEventHandler LoggedIn;

        /// <summary>
        /// shortest possible username/password length
        /// </summary>
        private const int minLength = 5;
        /// <summary>
        /// longest possible username/password length
        /// </summary>
        private const int maxLength = 32;

        /// <summary>
        /// default constructor
        /// </summary>
        public LoginPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// when the login button is clicked, attempt to login
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonLogin_Clicked(object sender, EventArgs e)
        {
            this.LabelMessage.Text = "";
            this.ButtonLogin.IsEnabled = false;
            this.ButtonToCreateAccountPage.IsEnabled = false;

            string username = this.EntryUsername.Text.Trim();
            string password = this.EntryPassword.Text;
            this.ActivityIndicator.IsVisible = true;
            this.ActivityIndicator.IsRunning = true;
            OperationReturnMessage response = await Task.Run(() => ServerOperations.LoginAccount(username, password));
            if (response != OperationReturnMessage.FalseFailedConnection)
            {
                if (response == OperationReturnMessage.True)
                {
                    await CredentialManager.SaveCredentialAsync(username, password, true);
                    this.OnLoggedIn();
                }
                else if (response == OperationReturnMessage.TrueConfirmEmail)
                {
                    var confirmationPage = new EmailConfirmationPage(username, password);
                    confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                    await this.Navigation.PushModalAsync(confirmationPage);
                    await CredentialManager.SaveCredentialAsync(username, password, false);
                    this.OnLoggedIn();
                }
                else if (response == OperationReturnMessage.FalseInvalidCredentials)
                {
                    this.LabelMessage.Text = "Login Failed - Username and/or Password are Incorrect.";
                }
                else
                {
                    this.LabelMessage.Text = "Login Failed.";
                }
            }
            else
            {
                this.LabelMessage.Text = "Connection failed: Please try again.";
            }

            this.ButtonToCreateAccountPage.IsEnabled = true;
            this.ActivityIndicator.IsRunning = false;
            this.ActivityIndicator.IsVisible = false;
            this.EntryPassword.Text = "";
            this.ButtonLogin.IsEnabled = true;
        }

        /// <summary>
        /// When the create account button is clicked, open the create account page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonToCreateAccountPage_Clicked(object sender, EventArgs e)
        {
            var createAccountPage = new CreateAccountPage();
            createAccountPage.AccountCreated += this.OnAccountCreated;
            await this.Navigation.PushAsync(createAccountPage);
        }

        /// <summary>
        /// when the email is confirmed, login is successful
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        public void OnEmailConfirmed(object source, EventArgs args)
        {
            this.LabelMessage.Text = "Login Successful!";
        }

        /// <summary>
        /// when the user is logged in, trigger the necessary setups
        /// </summary>
        protected virtual void OnLoggedIn()
        {
            this.LoggedIn?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// When the account is created, automatically log them in
        /// </summary>
        /// <param name="source"></param>
        /// <param name="accountArgs"></param>
        private void OnAccountCreated(object source, AccountCreatedEventArgs accountArgs)
        {
            this.EntryUsername.Text = accountArgs.Username;

            this.OnLoggedIn();
        }

        /// <summary>
        /// When signed out, the info message has nothing to say
        /// </summary>
        /// <param name="source"></param>
        /// <param name="eventArgs"></param>
        public void OnSignout(object source, EventArgs eventArgs)
        {
            this.LabelMessage.Text = "";
        }

        /// <summary>
        /// Opens the forgot password page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ForgotPassword_Tapped(object sender, EventArgs e)
        {
            this.Navigation.PushModalAsync(new ForgotPasswordPage());
        }

        /// <summary>
        /// animations for the forgot password button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ForgotPassword_Released(object sender, EventArgs e)
        {
            this.forgotPassword.TextColor = Color.DodgerBlue;
        }

        /// <summary>
        /// animations for the forgot password button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ForgotPassword_Pressed(object sender, EventArgs e)
        {
            this.forgotPassword.TextColor = Color.Blue;
        }
        
        /// <summary>
        /// if the username is long enough and does not contain forbidden characters
        /// </summary>
        private bool usernameLongEnough;
        /// <summary>
        /// if the password is long enough and does not contain forbidden characters
        /// </summary>
        private bool passwordLongEnough;

        /// <summary>
        /// Updates the login button if the username and password are valid entries
        /// </summary>
        private void CheckLength()
        {
            this.ButtonLogin.IsEnabled = this.usernameLongEnough && this.passwordLongEnough;
        }

        /// <summary>
        /// when the username text changes, check if it is valid; if so, enable the login button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EntryUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.usernameLongEnough = this.EntryUsername.Text.Length > minLength && this.EntryUsername.Text.Length <= maxLength;
            this.CheckLength();
        }

        /// <summary>
        /// when the password text changes, check if it is valid; if so, enable the login button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EntryPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.passwordLongEnough = this.EntryPassword.Text.Length > minLength && this.EntryPassword.Text.Length <= maxLength;
            this.CheckLength();
        }
    }
}