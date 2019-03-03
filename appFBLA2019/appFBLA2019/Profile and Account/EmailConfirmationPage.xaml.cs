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
        /// <summary>
        /// If the email is confirmed.
        /// </summary>
        private bool emailConfirmed;

        /// <summary>
        /// Construct the confirmation page based on the username and password.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public EmailConfirmationPage(string username, string password)
        {
            this.InitializeComponent();
            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// Setup page elements when the page appears.
        /// </summary>
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            this.LabelTitle.Text = "Loading...";

            // Load the user's email into top text
            await Task.Run(() => this.GetEmail());
        }

        /// <summary>
        /// Handle when the page dissappears
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (!this.emailConfirmed)
                OnConfirmLaterSelected();
            else
                OnEmailConfirmed();
        }

        public delegate void ConfirmLaterSelectedEventHandler(object source, EventArgs eventArgs);

        public delegate void EmailConfirmedEventHandler(object source, EventArgs eventArgs);

        public event ConfirmLaterSelectedEventHandler ConfirmLaterSelected;

        public event EmailConfirmedEventHandler EmailConfirmed;

        /// <summary>
        /// Handle when user chooses to confirm email later.
        /// </summary>
        protected virtual void OnConfirmLaterSelected()
        {
            this.ConfirmLaterSelected?.Invoke(this, EventArgs.Empty);
            this.emailConfirmed = false;
        }

        /// <summary>
        /// Handle when the email is successfully confirmed.
        /// </summary>
        protected virtual void OnEmailConfirmed()
        {
            this.emailConfirmed = true;
            this.EmailConfirmed?.Invoke(this, EventArgs.Empty);
        }

        private const int maxEmailLengthSize = 640;
        
        private string username;
        private string password;

        /// <summary>
        /// Handle when the close button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonClose_Clicked(object sender, EventArgs e)
        {
            this.emailConfirmed = false;
            await this.Navigation.PopModalAsync(true);
        }

        private async void ButtonConfirmEmail_Clicked(object sender, EventArgs e)
        {
            OperationReturnMessage result = await Task.Run(() => ServerOperations.ConfirmEmail(this.username, this.EntryConfirmationCode.Text.Trim()));

            if (result == OperationReturnMessage.True)
            {
                this.emailConfirmed = true;
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

            if (result == OperationReturnMessage.TrueConfirmEmail)
            {
                this.LabelTitle.Text = $"Enter the confirmation code sent to {newEmail}";
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
                string email = await Task.Run(() => ServerOperations.GetEmail(this.username, this.password));

                Device.BeginInvokeOnMainThread(() =>
                this.LabelTitle.Text =
                $"Enter the confirmation code sent to {email}");
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() =>
                this.LabelMessage.Text = "Connection Error: Please Try Again.");
            }
        }
    }
}