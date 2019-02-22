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
        private bool emailConfirmed;
        public EmailConfirmationPage(string username, string password)
        {
            this.InitializeComponent();
            this.username = username;
            this.password = password;
            this.LabelTitle.Text = "Loading...";
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await Task.Run(() => this.GetEmail());
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (!this.emailConfirmed)
                OnConfirmLaterSelected();
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
            this.emailConfirmed = true;
            this.EmailConfirmed?.Invoke(this, EventArgs.Empty);
        }

        private const int maxEmailLengthSize = 640;

        private string email;
        private string username;
        private string password;

        private async void ButtonClose_Clicked(object sender, EventArgs e)
        {
            this.OnConfirmLaterSelected();
            await this.Navigation.PopModalAsync(true);
        }

        private async void ButtonConfirmEmail_Clicked(object sender, EventArgs e)
        {
            OperationReturnMessage result = await Task.Run(() => ServerOperations.ConfirmEmail(this.username, this.EntryConfirmationCode.Text.Trim()));

            if (result == OperationReturnMessage.True)
            {
                this.OnEmailConfirmed();
                await this.Navigation.PopModalAsync(true);
            }
            else
            {
                this.LabelMessage.Text = "Email could not be confirmed. Please try your code again.";
            }
        }

        private async void ButtonFixEmail_Clicked(object sender, EventArgs e)
        {
            string newEmail = this.EntryChangeEmail.Text.Trim();
            OperationReturnMessage result = await Task.Run(() => ServerOperations.ChangeEmail(this.username, 
                this.password, newEmail));

            if (result == OperationReturnMessage.True)
            {
                this.LabelMessage.Text = $"Enter the confirmation code sent to {newEmail}";
            }
            else
            {
                this.LabelMessage.Text = $"Email could not be changed.";
            }
        }

        private async Task GetEmail()
        {
            try
            {
                this.email = await Task.Run(() => ServerOperations.GetEmail(this.username, this.password));

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