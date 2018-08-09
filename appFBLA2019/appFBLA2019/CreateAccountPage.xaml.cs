using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateAccountPage : ContentPage
    {
        public CreateAccountPage()
        {
            InitializeComponent();
        }

        private async void ButtonCreateAccount_Clicked(object sender, EventArgs e)
        {
            try
            {
                string username = this.EntryUsername.Text;
                await ServerConnector.QueryDB($"createAccount/{username}/{this.EntryPassword.Text}" +
                    $"/{this.EntryEmail.Text}/-");
                string databaseReturnInfo = await ServerConnector.ReceiveFromDB();

                if (databaseReturnInfo == "true/-")
                {
                    this.LabelMessage.Text = "Account successfully created.";

                    EmailConfirmationPage confirmationPage = new EmailConfirmationPage(username);
                    confirmationPage.EmailConfirmed += this.OnEmailConfirmed;

                    await this.Navigation.PushModalAsync(confirmationPage);
                }
                else
                {
                    string errorMessage = (databaseReturnInfo.Split('/'))[1];
                    this.LabelMessage.Text = $"Account could not be created: {errorMessage}";
                }
            }
            catch (Exception ex)
            {
                this.LabelMessage.Text = "Connection Error Occured: Please Try Again." + ex.Message;
            }
        }

        public void OnEmailConfirmed(object source, EventArgs args)
        {
            this.LabelMessage.Text = "Email Confirmed!";
        }
    }
}