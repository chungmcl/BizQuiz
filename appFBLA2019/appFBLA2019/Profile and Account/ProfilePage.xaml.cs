//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            this.InitializeComponent();
        }

        public bool IsLoading { get; private set; }
        public bool IsOnLoginPage { get; private set; }

        public async Task UpdateProfilePage(bool updateLoginStatus)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // Initialize profile content to avoid wonky UI behavior during async loading
                this.StackLayoutProfilePageContent.IsVisible = false;
                this.ActivityIndicator.IsVisible = true;
                this.ActivityIndicator.IsRunning = true;
                this.IsLoading = true;
            });

            if (updateLoginStatus)
            {
                await CredentialManager.CheckLoginStatus();
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                this.StackLayoutProfilePageContent.IsVisible = CredentialManager.IsLoggedIn;
                this.LocalLoginPage.IsVisible = !(CredentialManager.IsLoggedIn);
                if (this.StackLayoutProfilePageContent.IsVisible)
                {
                    if (this.ToolbarItems.Count <= 0)
                    {
                        ToolbarItem accountSettingsButton = new ToolbarItem();
                        accountSettingsButton.Clicked += ToolbarItemAccountSettings_Clicked;
                        accountSettingsButton.Icon = FileImageSource.FromFile("ic_settings_black_48dp.png") as FileImageSource;

                        this.ToolbarItems.Add(accountSettingsButton);
                    }

                    this.IsOnLoginPage = false;
                    this.LabelUsername.Text = CredentialManager.Username;
                }
                else
                {
                    if (this.ToolbarItems.Count > 0)
                    {
                        this.ToolbarItems.Clear();
                    }

                    this.IsOnLoginPage = true;
                    this.LocalLoginPage.LoggedIn += OnLoggedIn;
                }

                this.ActivityIndicator.IsRunning = false;
                this.ActivityIndicator.IsVisible = false;
                this.IsLoading = false;
            });
        }

        private async void ToolbarItemAccountSettings_Clicked(object sender, EventArgs e)
        {
            AccountSettingsPage accountSettingsPage = new AccountSettingsPage();
            accountSettingsPage.SignedOut += OnSignedOut;
            await this.Navigation.PushAsync(accountSettingsPage);
        }

        public async void OnLoggedIn(object source, EventArgs eventArgs)
        {
            await Task.Run(() => UpdateProfilePage(false));
        }

        public async void OnSignedOut(object source, EventArgs eventArgs)
        {
            await Task.Run(() => UpdateProfilePage(false));
        }
    }
}