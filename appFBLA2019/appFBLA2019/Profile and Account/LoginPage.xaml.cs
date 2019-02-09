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
            Task login = Task.Run(() => this.Login(this.EntryUsername.Text,
                this.EntryPassword.Text));
        }

        private async void Login(string username, string password)
        {
            Device.BeginInvokeOnMainThread(() => this.LabelMessage.Text = "Waiting...");
            bool completedRequest = await Task.Run(() => ServerConnector.SendData(ServerRequestTypes.LoginAccount, $"{username}/{password}/-"));
            
            if (completedRequest)
            {
                OperationReturnMessage response = await Task.Run(() => ServerConnector.ReceiveFromServerORM());

                if (response == OperationReturnMessage.True)
                {
                    Device.BeginInvokeOnMainThread(() => this.LabelMessage.Text = "Login Successful!");
                    CredentialManager.SaveCredential(username, password);
                    OnLoggedIn();
                }
                else if (response == OperationReturnMessage.TrueConfirmEmail)
                {
                    Device.BeginInvokeOnMainThread(async() =>
                    {
                        this.LabelMessage.Text = "Please confirm your email.";
                        var confirmationPage = new EmailConfirmationPage(username);
                        confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                        await this.Navigation.PushModalAsync(confirmationPage);
                    });
                    CredentialManager.SaveCredential(username, password);
                    OnLoggedIn();
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
            if (accountArgs.EmailConfirmed)
            {
                this.LabelMessage.Text = "Account created successfully!";
            }
            else
            {
                this.LabelMessage.Text = "Account created successfully! " +
                    "Please confirm your email as soon as possible.";
            }
        }
    }
}