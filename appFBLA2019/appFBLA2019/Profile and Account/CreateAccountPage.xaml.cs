using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Threading.Tasks;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateAccountPage : ContentPage
    {
        public delegate void AccountCreatedEventHandler(object source, AccountCreatedEventArgs eventArgs);
        public event AccountCreatedEventHandler AccountCreated;
        public CreateAccountPage()
        {
            InitializeComponent();
        }

        private void ButtonCreateAccount_Clicked(object sender, EventArgs e)
        {
            Task createAccount = Task.Run(() => CreateAccount(
                this.EntryUsername.Text.Trim(),
                this.EntryPassword.Text.Trim(),
                this.EntryEmail.Text.Trim()));
        }

        private async Task CreateAccount(string username, string password, string email)
        {
            Task<bool> completedRequest = ServerConnector.SendData(ServerRequestTypes.RegisterAccount, 
                $"createAccount/{username}/{password}" +
                $"/{email}/-");

            if (await completedRequest)
            {
                OperationReturnMessage databaseReturnInfo = await ServerConnector.ReceiveFromServerORM();

                if (databaseReturnInfo == OperationReturnMessage.True)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        this.LabelMessage.Text = "Account successfully created.";
                        var confirmationPage = new EmailConfirmationPage(username);
                        confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                        confirmationPage.ConfirmLaterSelected += this.OnConfirmLaterSelected;
                        await this.Navigation.PushModalAsync(confirmationPage);
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = $"Account could not be created.");
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = "Connection failed: Please try again.");
            }
        }

        private async void OnEmailConfirmed(object source, EventArgs eventArgs)
        {
            OnAccountCreated(true);
            await this.Navigation.PopAsync();
        }

        private async void OnConfirmLaterSelected(object source, EventArgs eventArgs)
        {
            OnAccountCreated(false);
            await this.Navigation.PopAsync();
        }

        protected void OnAccountCreated(bool emailConfirmed)
        {
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