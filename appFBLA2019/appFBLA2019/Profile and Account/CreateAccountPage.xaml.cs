using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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

        private async void ButtonCreateAccount_Clicked(object sender, EventArgs e)
        {
            try
            {
                string username = this.EntryUsername.Text.Trim();
                bool connectionSuccess = await ServerConnector.QueryDB(
                    $"createAccount/{username}/{this.EntryPassword.Text.Trim()}" +
                    $"/{this.EntryEmail.Text.Trim()}/-");

                if (connectionSuccess)
                {
                    string databaseReturnInfo = await ServerConnector.ReceiveFromDB();

                    if (databaseReturnInfo == "true/-")
                    {
                        this.LabelMessage.Text = "Account successfully created.";

                        var confirmationPage = new EmailConfirmationPage(username);
                        confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                        confirmationPage.ConfirmLaterSelected += this.OnConfirmLaterSelected;
                        await this.Navigation.PushModalAsync(confirmationPage);
                    }
                    else
                    {
                        string errorMessage = (databaseReturnInfo.Split('/'))[1];
                        this.LabelMessage.Text = $"Account could not be created: {errorMessage}";
                    }
                }
                else
                {
                    this.LabelMessage.Text = "Connection failed: Please try again.";
                }
            }
            catch (Exception ex)
            {
                this.LabelMessage.Text = "Connection Error Occured: Please Try Again." + ex.Message;
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