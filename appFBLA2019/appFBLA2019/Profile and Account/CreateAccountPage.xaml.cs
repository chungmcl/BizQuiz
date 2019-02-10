﻿//BizQuiz App 2019

using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateAccountPage : ContentPage
    {
        public CreateAccountPage()
        {
            this.InitializeComponent();
        }

        public delegate void AccountCreatedEventHandler(object source, AccountCreatedEventArgs eventArgs);

        public event AccountCreatedEventHandler AccountCreated;

        public class AccountCreatedEventArgs : EventArgs
        {
            public bool EmailConfirmed { get; set; }
            public string Username { get; set; }
        }

        protected void OnAccountCreated(bool emailConfirmed)
        {
            this.AccountCreated?.Invoke(this,
                new AccountCreatedEventArgs
                {
                    EmailConfirmed = emailConfirmed,
                    Username = this.EntryUsername.Text.Trim()
                });
        }

        private void ButtonCreateAccount_Clicked(object sender, EventArgs e)
        {
            Task createAccount = Task.Run(() => this.CreateAccount(
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
                        var confirmationPage = new EmailConfirmationPage(username);
                        confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                        confirmationPage.ConfirmLaterSelected += this.OnConfirmLaterSelected;
                        await this.Navigation.PushModalAsync(confirmationPage);
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = $"Account could not be created.");
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = "Connection failed: Please try again.");
            }
        }

        private async void OnConfirmLaterSelected(object source, EventArgs eventArgs)
        {
            this.OnAccountCreated(false);
            await this.Navigation.PopAsync();
        }

        private async void OnEmailConfirmed(object source, EventArgs eventArgs)
        {
            this.OnAccountCreated(true);
            await this.Navigation.PopAsync();
        }
    }
}