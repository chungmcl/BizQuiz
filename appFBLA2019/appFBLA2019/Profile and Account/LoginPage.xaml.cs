using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static appFBLA2019.CreateAccountPage;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void ButtonLogin_Clicked(object sender, EventArgs e)
        {
            string username = this.EntryUsername.Text;
            bool completedRequest = await ServerConnector.QueryDB($"loginAccount/{username}/{this.EntryPassword.Text}/-");
            if (completedRequest)
            {

                string response = await ServerConnector.ReceiveFromDB();

                if (response == "true/-")
                {
                    this.LabelMessage.Text = "Login Successful!";
                }
                else if (response == "true/confirmEmail/-")
                {
                    this.LabelMessage.Text = "Please confirm your email.";

                    var confirmationPage = new EmailConfirmationPage(username);
                    confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                    await this.Navigation.PushModalAsync(confirmationPage);
                }
                else
                {
                    this.LabelMessage.Text = "Login Failed: " + response.Split('/')[1];
                }
            }
            else
            {
                this.LabelMessage.Text = "Connection failed: Please try again.";
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