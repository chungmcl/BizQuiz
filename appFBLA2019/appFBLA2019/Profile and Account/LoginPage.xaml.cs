﻿using System;
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
            this.InitializeComponent();
        }

        private void ButtonLogin_Clicked(object sender, EventArgs e)
        {
            Task login = Task.Run(() => this.Login(this.EntryUsername.Text,
                this.EntryPassword.Text));
        }

        private async void Login(string username, string password)
        {
            Device.BeginInvokeOnMainThread(() => this.LabelMessage.Text = "Waiting...");
            Task<bool> completedRequest = ServerConnector.QueryDB($"loginAccount/{username}/{password}/-");
            
            if (await completedRequest)
            {
                string response = await ServerConnector.ReceiveFromDB();

                if (response == "true/-")
                {
                    Device.BeginInvokeOnMainThread(() => this.LabelMessage.Text = "Login Successful!");
                }
                else if (response == "true/confirmEmail/-")
                {
                    Device.BeginInvokeOnMainThread(async() =>
                    {
                        this.LabelMessage.Text = "Please confirm your email.";
                        var confirmationPage = new EmailConfirmationPage(username);
                        confirmationPage.EmailConfirmed += this.OnEmailConfirmed;
                        await this.Navigation.PushModalAsync(confirmationPage);
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    this.LabelMessage.Text = "Login Failed: " + response.Split('/')[1]);
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => this.LabelMessage.Text = "Connection failed: Please try again.");
            }
        }


        private async void ButtonToCreateAccountPage_Clicked(object sender, EventArgs e)
        {
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