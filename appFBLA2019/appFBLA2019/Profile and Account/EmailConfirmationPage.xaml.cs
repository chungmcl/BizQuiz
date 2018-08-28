using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EmailConfirmationPage : ContentPage
    {
        private string username;
        private string email;

        public delegate void EmailConfirmedEventHandler(object source, EventArgs eventArgs);
        public event EmailConfirmedEventHandler EmailConfirmed;

        public delegate void ConfirmLaterSelectedEventHandler(object source, EventArgs eventArgs);
        public event ConfirmLaterSelectedEventHandler ConfirmLaterSelected;
        public EmailConfirmationPage(string username)
        {
            InitializeComponent();
            this.username = username;
            this.LabelTitle.Text = "Loading...";
            Task getEmail = Task.Run(() => GetEmail());
        }

        private async Task GetEmail()
        {
            try
            {
                await ServerConnector.QueryDB($"getEmail/{this.username}/-");
                this.email = await ServerConnector.ReceiveFromDB();

                Device.BeginInvokeOnMainThread(() =>
                this.LabelTitle.Text =
                $"Enter the confirmation code sent to {this.email.Split('/')[1]}");
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() =>
                this.LabelMessage.Text = "Connection Error: Please Try Again.");
            }
        }

        private void ButtonConfirmEmail_Clicked(object sender, EventArgs e)
        {
            Task confirmEmail = Task.Run(() => ConfirmEmail(
                this.EntryConfirmationCode.Text,
                this.username));
        }

        private async Task ConfirmEmail(string confirmationCode, string username)
        {
            Task<bool> completedRequest = ServerConnector.QueryDB(
                    $"confirmEmail/{username}/{confirmationCode}/-");

            if (await completedRequest)
            {
                string returnData = await ServerConnector.ReceiveFromDB();

                if (returnData == "true/-")
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        OnEmailConfirmed();
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

        private void ButtonFixEmail_Clicked(object sender, EventArgs e)
        {
            Task changeEmail = Task.Run(() => ChangeEmail(
                this.EntryChangeEmail.Text,
                this.username));
        }

        private async Task ChangeEmail(string newEmail, string username)
        {
            Task<bool> completedRequest = ServerConnector.QueryDB($"changeEmail/{username}/{newEmail}/-");

            if (await completedRequest)
            {
                string result = await ServerConnector.ReceiveFromDB();
                if (result == "true/-")
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = $"Enter the confirmation code sent to {newEmail}");
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = $"Email could not be changed: {result.Split('/')[1]}");
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = $"Connection failed: Please try again.");
            }
        }

        private async void ButtonClose_Clicked(object sender, EventArgs e)
        {
            OnConfirmLaterSelected();
            await this.Navigation.PopModalAsync(true);
        }

        protected virtual void OnEmailConfirmed()
        {
            this.EmailConfirmed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnConfirmLaterSelected()
        {
            this.ConfirmLaterSelected?.Invoke(this, EventArgs.Empty);
        }
    }
}