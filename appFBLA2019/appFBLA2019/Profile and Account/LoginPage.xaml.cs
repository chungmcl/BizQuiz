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
    }
}