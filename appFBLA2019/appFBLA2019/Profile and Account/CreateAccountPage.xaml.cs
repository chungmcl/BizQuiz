//BizQuiz App 2019

using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateAccountPage : ContentPage
    {
        public delegate void AccountCreatedEventHandler(object source, AccountCreatedEventArgs eventArgs);

        public event AccountCreatedEventHandler AccountCreated;

        private string username;
        private string password;

        public CreateAccountPage()
        {
            this.InitializeComponent();
        }

        private void ButtonCreateAccount_Clicked(object sender, EventArgs e)
        {
            this.ButtonCreateAccount.IsEnabled = false;
            this.EntryEmail.IsEnabled = false;
            this.EntryPassword.IsEnabled = false;
            this.EntryUsername.IsEnabled = false;
            this.ActivityIndicator.IsVisible = true;
            this.ActivityIndicator.IsRunning = true;

            Task createAccount = Task.Run(() => this.CreateAccount(
                this.EntryUsername.Text.Trim(),
                this.EntryPassword.Text.Trim(),
                this.EntryEmail.Text.Trim()));
        }

        private async Task CreateAccount(string username, string password, string email)
        {
            bool completedRequest = await Task.Run(() => ServerConnector.SendData(ServerRequestTypes.RegisterAccount,
                $"{username}/{password}" +
                $"/{email}/-"));

            if (completedRequest)
            {
                OperationReturnMessage databaseReturnInfo = await Task.Run(() => ServerConnector.ReceiveFromServerORM());

                if (databaseReturnInfo == OperationReturnMessage.TrueConfirmEmail)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        this.LabelMessage.Text = "Account successfully created.";
                        this.username = username;
                        this.password = password;

                        var confirmationPage = new EmailConfirmationPage(username, password);
                        confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                        confirmationPage.ConfirmLaterSelected += this.OnConfirmLaterSelected;
                        await this.Navigation.PushModalAsync(confirmationPage);
                    });
                }
                else if (databaseReturnInfo == OperationReturnMessage.FalseUsernameAlreadyExists)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = $"Account could not be created - Username already exists.");
                }
                else if (databaseReturnInfo == OperationReturnMessage.FalseInvalidEmail)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = $"Account could not be created - Invalid Email.");
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = "Connection failed: Please try again.");
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                this.ActivityIndicator.IsRunning = false;
            });
        }

        private async void OnEmailConfirmed(object source, EventArgs eventArgs)
        {
            this.OnAccountCreated(true);
            await this.Navigation.PopAsync();
        }

        private async void OnConfirmLaterSelected(object source, EventArgs eventArgs)
        {
            this.OnAccountCreated(false);
            await this.Navigation.PopAsync();
        }

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
    }
}