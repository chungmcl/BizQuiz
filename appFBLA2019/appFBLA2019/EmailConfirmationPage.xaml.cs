using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EmailConfirmationPage : ContentPage
    {
        private string username;
        public delegate void EmailConfirmedEventHandler(object source, EventArgs args);
        public event EmailConfirmedEventHandler EmailConfirmed;
        public EmailConfirmationPage(string username)
        {
            InitializeComponent();
            this.username = username;
            this.LabelTitle.Text = "Loading...";
            GetEmail();
        }

        private async void GetEmail()
        {
            try
            {
                await ServerConnector.QueryDB($"getEmail/{this.username}/-");
                string email = await ServerConnector.ReceiveFromDB();
                this.LabelTitle.Text = $"Enter the confirmation code sent to {email.Split('/')[1]}";
            }
            catch
            {
                ReturnToLoginPageWithError();
            }
        }

        private async void ReturnToLoginPageWithError()
        {
            await this.Navigation.PushAsync(new LoginPage("Could not connect to server. Please try again."));
        }

        private async void ButtonConfirmEmail_Clicked(object sender, EventArgs e)
        {
            try
            {
                string userInputToken = this.EntryConfirmationCode.Text.Trim();
                await ServerConnector.QueryDB($"confirmEmail/{this.username}/{userInputToken}/-");
                string returnData = await ServerConnector.ReceiveFromDB();

                if (returnData == "true/-")
                {
                    OnEmailConfirmed();
                    await this.Navigation.PopModalAsync();
                }
                else
                {
                    this.LabelMessage.Text = "Email could not be confirmed. Please try your code again.";
                }
            }
            catch
            {
                this.LabelMessage.Text = "Connection Error: Please try again.";
            }
        }

        private async void ButtonFixEmail_Clicked(object sender, EventArgs e)
        {
            string newEmail = this.EntryChangeEmail.Text;
            await ServerConnector.QueryDB($"changeEmail/{this.username}/{newEmail}/-");
            string result = await ServerConnector.ReceiveFromDB();

            if (result == "true/-")
            {
                this.LabelMessage.Text = $"Enter the confirmation code sent to {newEmail}";
            }
            else
            {
                this.LabelMessage.Text = $"Email could not be changed: {result.Split('/')[1]}";
            }
        }

        protected virtual void OnEmailConfirmed()
        {
            if (EmailConfirmed != null)
            {
                EmailConfirmed(this, EventArgs.Empty);
            }
        }
    }
}