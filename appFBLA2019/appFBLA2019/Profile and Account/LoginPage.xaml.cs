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

        public delegate void LoggedinEventHandler(object source, EventArgs eventArgs);

        public event LoggedinEventHandler LoggedIn;

        private const int minLength = 5;
        private const int maxLength = 32;

        public LoginPage()
        {
            this.InitializeComponent();
            
        }

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
                    CredentialManager.SaveCredential(username, password, true);
                    this.OnLoggedIn();
                }
                else if (response == OperationReturnMessage.TrueConfirmEmail)
                {
                    var confirmationPage = new EmailConfirmationPage(username, password);
                    confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                    await this.Navigation.PushModalAsync(confirmationPage);
                    CredentialManager.SaveCredential(username, password, false);
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

            this.ButtonLogin.IsEnabled = true;
            this.ButtonToCreateAccountPage.IsEnabled = true;
            this.ActivityIndicator.IsRunning = false;
            this.ActivityIndicator.IsVisible = false;
            this.EntryPassword.Text = "";
        }

        private async void ButtonToCreateAccountPage_Clicked(object sender, EventArgs e)
        {
            var createAccountPage = new CreateAccountPage();
            createAccountPage.AccountCreated += this.OnAccountCreated;
            await this.Navigation.PushAsync(createAccountPage);
        }

        public void OnEmailConfirmed(object source, EventArgs args)
        {
            this.LabelMessage.Text = "Login Successful!";
        }

        protected virtual void OnLoggedIn()
        {
            this.LoggedIn?.Invoke(this, EventArgs.Empty);
        }

        private void OnAccountCreated(object source, AccountCreatedEventArgs accountArgs)
        {
            this.EntryUsername.Text = accountArgs.Username;

            this.OnLoggedIn();
        }

        public void OnSignout(object source, EventArgs eventArgs)
        {
            this.LabelMessage.Text = "";
        }

        private void ForgotPassword_Tapped(object sender, EventArgs e)
        {
            this.Navigation.PushModalAsync(new ForgotPasswordPage());
        }

        private void ForgotPassword_Released(object sender, EventArgs e)
        {
            this.forgotPassword.TextColor = Color.DodgerBlue;
        }

        private void ForgotPassword_Pressed(object sender, EventArgs e)
        {
            this.forgotPassword.TextColor = Color.Blue;
        }

        private bool usernameLength;
        private bool passwordLength;

        private void CheckLength()
        {
            this.ButtonLogin.IsEnabled = this.usernameLength && this.passwordLength;
        }

        private void EntryUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.usernameLength = this.EntryUsername.Text.Length > minLength && this.EntryUsername.Text.Length <= maxLength;
            this.CheckLength();
        }

        private void EntryPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.passwordLength = this.EntryPassword.Text.Length > minLength && this.EntryPassword.Text.Length <= maxLength;
            this.CheckLength();
        }
    }
}