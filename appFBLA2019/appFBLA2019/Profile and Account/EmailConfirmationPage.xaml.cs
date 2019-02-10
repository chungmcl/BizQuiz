//BizQuiz App 2019

using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EmailConfirmationPage : ContentPage
    {
        public EmailConfirmationPage(string username)
        {
            this.InitializeComponent();
            this.username = username;
            this.LabelTitle.Text = "Loading...";
            Task getEmail = Task.Run(() => this.GetEmail());
        }

        public delegate void ConfirmLaterSelectedEventHandler(object source, EventArgs eventArgs);

        public delegate void EmailConfirmedEventHandler(object source, EventArgs eventArgs);

        public event ConfirmLaterSelectedEventHandler ConfirmLaterSelected;

        public event EmailConfirmedEventHandler EmailConfirmed;

        protected virtual void OnConfirmLaterSelected()
        {
            this.ConfirmLaterSelected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnEmailConfirmed()
        {
            this.EmailConfirmed?.Invoke(this, EventArgs.Empty);
        }

        private const int maxEmailLengthSize = 640;

        private string email;
        private string username;

        private async void ButtonClose_Clicked(object sender, EventArgs e)
        {
            this.OnConfirmLaterSelected();
            await this.Navigation.PopModalAsync(true);
        }

        private void ButtonConfirmEmail_Clicked(object sender, EventArgs e)
        {
            Task confirmEmail = Task.Run(() => this.ConfirmEmail(
                this.EntryConfirmationCode.Text,
                this.username));
        }

        private void ButtonFixEmail_Clicked(object sender, EventArgs e)
        {
            Task changeEmail = Task.Run(() => this.ChangeEmail(
                this.EntryChangeEmail.Text,
                this.username));
        }

        private async Task ChangeEmail(string newEmail, string username)
        {
            bool completedRequest = await Task.Run(() => ServerConnector.SendData(ServerRequestTypes.ChangeEmail, $"{username}/{newEmail}/-"));

            if (completedRequest)
            {
                OperationReturnMessage result = await Task.Run(() => ServerConnector.ReceiveFromServerORM());
                if (result == OperationReturnMessage.True)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = $"Enter the confirmation code sent to {newEmail}");
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = $"Email could not be changed.");
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = $"Connection failed: Please try again.");
            }
        }

        private async Task ConfirmEmail(string confirmationCode, string username)
        {
            bool completedRequest = await Task.Run(() => ServerConnector.SendData(ServerRequestTypes.ConfirmEmail,
                    $"{username}/{confirmationCode}/-"));

            if (completedRequest)
            {
                OperationReturnMessage returnData = await Task.Run(() => ServerConnector.ReceiveFromServerORM());

                if (returnData == OperationReturnMessage.True)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        this.OnEmailConfirmed();
                        await this.Navigation.PopModalAsync(true);
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = "Email could not be confirmed. Please try your code again.");
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = "Connection failed: Please try again.");
            }
        }

        private async Task GetEmail()
        {
            try
            {
                await Task.Run(() => ServerConnector.SendData(ServerRequestTypes.GetEmail, $"{this.username}/-"));
                this.email = await Task.Run(() => ServerConnector.ReceiveFromServerStringData());

                Device.BeginInvokeOnMainThread(() =>
                this.LabelTitle.Text =
                $"Enter the confirmation code sent to {this.email}");
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() =>
                this.LabelMessage.Text = "Connection Error: Please Try Again.");
            }
        }
    }
}