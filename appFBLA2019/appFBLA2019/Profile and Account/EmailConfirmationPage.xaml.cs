﻿using System;
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
            this.InitializeComponent();
            this.username = username;
            this.LabelTitle.Text = "Loading...";
            Task getEmail = Task.Run(() => this.GetEmail());
        }

        private async Task GetEmail()
        {
            try
            {
                await ServerConnector.SendData(ServerRequestTypes.GetEmail, $"getEmail/{this.username}/-");
                this.email = await ServerConnector.ReceiveFromServerStringData();

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
            Task confirmEmail = Task.Run(() => this.ConfirmEmail(
                this.EntryConfirmationCode.Text,
                this.username));
        }

        private async Task ConfirmEmail(string confirmationCode, string username)
        {
            Task<bool> completedRequest = ServerConnector.SendData(ServerRequestTypes.ConfirmEmail, 
                    $"confirmEmail/{username}/{confirmationCode}/-");

            if (await completedRequest)
            {
                OperationReturnMessage returnData = await ServerConnector.ReceiveFromServerORM();

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

        private void ButtonFixEmail_Clicked(object sender, EventArgs e)
        {
            Task changeEmail = Task.Run(() => this.ChangeEmail(
                this.EntryChangeEmail.Text,
                this.username));
        }

        private async Task ChangeEmail(string newEmail, string username)
        {
            Task<bool> completedRequest = ServerConnector.SendData(ServerRequestTypes.ChangeEmail, $"changeEmail/{username}/{newEmail}/-");

            if (await completedRequest)
            {
                OperationReturnMessage result = await ServerConnector.ReceiveFromServerORM();
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

        private async void ButtonClose_Clicked(object sender, EventArgs e)
        {
            this.OnConfirmLaterSelected();
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