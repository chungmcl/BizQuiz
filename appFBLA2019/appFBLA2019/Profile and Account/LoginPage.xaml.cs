using System;
using System.Threading.Tasks;
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

        private void ButtonLogin_Clicked(object sender, EventArgs e)
        {
            Task login = Task.Run(() => Login(this.EntryUsername.Text,
                this.EntryPassword.Text));

            this.ActivityIndicator.IsRunning = true;
        }

        private async void Login(string username, string password)
        {
            Task<bool> completedRequest = ServerConnector.QueryDB($"loginAccount/{username}/{password}/-");

            string message = "";
            if (await completedRequest)
            {
                string response = await ServerConnector.ReceiveFromDB();

                if (response == "true/-")
                {
                    message = "Login Successful!";
                }
                else if (response == "true/confirmEmail/-")
                {
                    Device.BeginInvokeOnMainThread(async() =>
                    {
                        var confirmationPage = new EmailConfirmationPage(username);
                        confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                        await this.Navigation.PushModalAsync(confirmationPage);
                    });
                }
                else
                {
                    message = "Login Failed: " + response.Split('/')[1];
                }
            }
            else
            {
                message = "Connection failed: Please try again.";
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                this.ActivityIndicator.IsRunning = false;
                this.LabelMessage.Text = message;
            });
        }

        private async void ButtonToCreateAccountPage_Clicked(object sender, EventArgs e)
        {
            this.LabelMessage.Text = "";

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