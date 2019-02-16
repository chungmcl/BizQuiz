using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateAccountPage : ContentPage
    {
        public delegate void AccountCreatedEventHandler(object source, AccountCreatedEventArgs eventArgs);
        public event AccountCreatedEventHandler AccountCreated;

        private string username;
        private string password;

        private const int minLength = 5;
        private const int maxLength = 32;

        public CreateAccountPage()
        {
            InitializeComponent();
        }

        private void ButtonCreateAccount_Clicked(object sender, EventArgs e)
        {
            this.ButtonCreateAccount.IsEnabled = false;
            this.EntryEmail.IsEnabled = false;
            this.EntryPassword.IsEnabled = false;
            this.EntryUsername.IsEnabled = false;
            this.ActivityIndicator.IsVisible = true;
            this.ActivityIndicator.IsRunning = true;

            Task createAccount = Task.Run(() => CreateAccount(
                this.EntryUsername.Text.Trim(),
                this.EntryPassword.Text.Trim(),
                this.EntryEmail.Text.Trim()));
        }

        private async Task CreateAccount(string username, string password, string email)
        {
            bool completedRequest = await Task.Run(() => ServerConnector.SendData(ServerRequestTypes.RegisterAccount, 
                $"{username}/{password}" +
                $"/{email}/-"));

            if (completedRequest)
            {
                OperationReturnMessage databaseReturnInfo = await Task.Run(() => ServerConnector.ReceiveFromServerORM());

                if (databaseReturnInfo == OperationReturnMessage.TrueConfirmEmail)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        this.LabelMessage.Text = "Account successfully created.";
                        this.username = username;
                        this.password = password;

                        var confirmationPage = new EmailConfirmationPage(username, password);
                        confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                        confirmationPage.ConfirmLaterSelected += this.OnConfirmLaterSelected;
                        await this.Navigation.PushModalAsync(confirmationPage);
                    });
                }
                else if (databaseReturnInfo == OperationReturnMessage.FalseUsernameAlreadyExists)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = $"Account could not be created - Username already exists.");
                }
                else if (databaseReturnInfo == OperationReturnMessage.FalseInvalidEmail)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = $"Account could not be created - Invalid Email.");
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = "Connection failed: Please try again.");
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                this.ButtonCreateAccount.IsEnabled = true;
                this.EntryEmail.IsEnabled = true;
                this.EntryPassword.IsEnabled = true;
                this.EntryUsername.IsEnabled = true;
                this.ActivityIndicator.IsVisible = false;
                this.ActivityIndicator.IsRunning = false;   
            });
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
            CredentialManager.SaveCredential(this.username, this.password, emailConfirmed);
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

        private bool usernameLength;
        private bool passwordLength;
        private bool emailCorrect;

        private async Task CheckIconAsync(string newIcon, string oldIcon, Image image)
        {
            if (image.Source.ToString().Split(':')[1].Trim() == oldIcon)
            {
                await image.FadeTo(0, 250, Easing.CubicInOut);
                image.Source = newIcon;
                await image.FadeTo(1, 250, Easing.CubicInOut);
            }
            else if (image.Opacity == 0)
                await image.FadeTo(1, 250, Easing.CubicInOut);
        }

        private void CheckEntries()
        {
            this.ButtonCreateAccount.IsEnabled = this.usernameLength && this.passwordLength && this.emailCorrect;
        }

        private async void EntryUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EntryUsername.Text.Length > minLength && EntryUsername.Text.Length <= maxLength)
            {
                this.usernameLength = true;
                await this.CheckIconAsync("ic_check_green_48dp.png", "ic_bad_red_48dp.png", this.checkUsername);
            }
            else
            {
                this.usernameLength = false;
                await this.CheckIconAsync("ic_bad_red_48dp.png", "ic_check_green_48dp.png", this.checkUsername);
            }
            this.CheckEntries();
        }

        private async void EntryPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EntryPassword.Text.Length > minLength && EntryPassword.Text.Length <= maxLength)
            {
                this.passwordLength = true;
                await this.CheckIconAsync("ic_check_green_48dp.png", "ic_bad_red_48dp.png", this.checkPassword);
            }
            else
            {
                this.passwordLength = false;
                await this.CheckIconAsync("ic_bad_red_48dp.png", "ic_check_green_48dp.png", this.checkPassword);
            }
            this.CheckEntries();
        }

        private string ComplexEmailPattern = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" // local-part
            + "@"
            + @"((([\w]+([-\w]*[\w]+)*\.)+[a-zA-Z]+)|" // domain
            + @"((([01]?[0-9]{1,2}|2[0-4][0-9]|25[0-5]).){3}[01]?[0-9]{1,2}|2[0-4][0-9]|25[0-5]))\z"; // or IP Address

        private async void EntryEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(Regex.IsMatch(EntryEmail.Text, ComplexEmailPattern))
            {
                this.emailCorrect = true;
                await this.CheckIconAsync("ic_check_green_48dp.png", "ic_bad_red_48dp.png", this.checkEmail);
            }
            else
            {
                this.emailCorrect = false;
                await this.CheckIconAsync("ic_bad_red_48dp.png", "ic_check_green_48dp.png", this.checkEmail);
            }
            this.CheckEntries();
        }
    }
}