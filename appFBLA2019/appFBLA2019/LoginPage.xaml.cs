using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage(string message)
        {
            InitializeComponent();
            this.LabelMessage.Text = message;
        }

        private async void ButtonLogin_Clicked(object sender, EventArgs e)
        {
            string username = this.EntryUsername.Text;
            await ServerConnector.QueryDB($"loginAccount/{username}/{this.EntryPassword.Text}/-");
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

                // This throws a NullReferenceException, but if one uses
                // await this.Navigation.PushModalAsync(new EmailConfirmationPage(username));
                // instead, it works perfectly fine...
                await this.Navigation.PushModalAsync(confirmationPage);
            }
            else
            {
                this.LabelMessage.Text = "Login Failed: " + response.Split('/')[1];
            }
        }

        private async void ButtonToCreateAccountPage_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new CreateAccountPage());
        }

        public void OnEmailConfirmed(object source, EventArgs args)
        {
            this.LabelMessage.Text = "Login Successful!";
        }
    }
}