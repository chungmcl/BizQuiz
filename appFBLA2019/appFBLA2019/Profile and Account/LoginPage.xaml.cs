using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
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

        private void ButtonLogin_Clicked(object sender, EventArgs e)
        {
            this.LabelMessage.Text = "";
            this.ButtonLogin.IsEnabled = false;
            this.ButtonToCreateAccountPage.IsEnabled = false;
            Task login = Task.Run(() => this.Login(this.EntryUsername.Text,
                this.EntryPassword.Text));
        }

        private async void Login(string username, string password)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                this.ActivityIndicator.IsVisible = true;
                this.ActivityIndicator.IsRunning = true;
            });
            bool completedRequest = await Task.Run(() => ServerConnector.SendData(ServerRequestTypes.LoginAccount, $"{username}/{password}/-"));
            
            if (completedRequest)
            {
                OperationReturnMessage response = await Task.Run(() => ServerConnector.ReceiveFromServerORM());

                if (response == OperationReturnMessage.True)
                {
                    CredentialManager.SaveCredential(username, password, true);
                    OnLoggedIn();
                }
                else if (response == OperationReturnMessage.TrueConfirmEmail)
                {
                    Device.BeginInvokeOnMainThread(async() =>
                    {
                        var confirmationPage = new EmailConfirmationPage(username, password);
                        confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                        await this.Navigation.PushModalAsync(confirmationPage);
                    });
                    CredentialManager.SaveCredential(username, password, false);
                    OnLoggedIn();
                }
                else if (response == OperationReturnMessage.FalseInvalidCredentials)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = "Login Failed - Username and/or Password are Incorrect.");
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = "Login Failed.");
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => this.LabelMessage.Text = "Connection failed: Please try again.");
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                this.ButtonLogin.IsEnabled = true;
                this.ButtonToCreateAccountPage.IsEnabled = true;
                this.ActivityIndicator.IsRunning = false;
                this.ActivityIndicator.IsVisible = false;
            });
            
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

            OnLoggedIn();
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
    }
}