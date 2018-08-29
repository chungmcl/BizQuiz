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
            Task<bool> completedRequest = ServerConnector.QueryDB(
                $"createAccount/{username}/{password}" +
                $"/{email}/-");

            string message = "";
            if (await completedRequest)
            {
                string databaseReturnInfo = await ServerConnector.ReceiveFromDB();

                if (databaseReturnInfo == "true/-")
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var confirmationPage = new EmailConfirmationPage(username);
                        confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                        confirmationPage.ConfirmLaterSelected += this.OnConfirmLaterSelected;
                        await this.Navigation.PushModalAsync(confirmationPage);
                    });
                }
                else
                {
                    string errorMessage = (databaseReturnInfo.Split('/'))[1];
                    message = $"Account could not be created: {errorMessage}";
                }
            }
            else
            {
                message = "Connection failed: Please try again.";
            }

            Device.BeginInvokeOnMainThread(() => this.LabelMessage.Text = message);
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