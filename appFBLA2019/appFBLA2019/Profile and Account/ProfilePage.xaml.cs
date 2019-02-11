//BizQuiz App 2019

using Plugin.FacebookClient;
using Plugin.FacebookClient.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        public bool IsLoading { get; private set; }
        public bool IsOnLoginPage { get; private set; }
        private AccountSettingsPage accountSettingsPage;
        public ProfilePage()
        {
            this.InitializeComponent();
        }

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
                await CredentialManager.CheckLoginStatus();

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
                        accountSettingsButton.Icon = ImageSource.FromFile("ic_settings_black_48dp.png") as FileImageSource;

                        this.ToolbarItems.Add(accountSettingsButton);
                    }

                    this.IsOnLoginPage = false;
                    this.LabelUsername.Text = CredentialManager.Username;
                }
                else
                {
                    if (this.ToolbarItems.Count > 0)
                        this.ToolbarItems.Clear();

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
            if (this.accountSettingsPage == null)
            {
                this.accountSettingsPage = new AccountSettingsPage();
                this.accountSettingsPage.SignedOut += OnSignedOut;
            }
            await this.Navigation.PushAsync(this.accountSettingsPage);
        }

        public async void OnLoggedIn(object source, EventArgs eventArgs)
        {
            this.accountSettingsPage = new AccountSettingsPage();
            this.accountSettingsPage.SignedOut += OnSignedOut;
            await Task.Run(() => UpdateProfilePage(false));
        }

        public async void OnSignedOut(object source, EventArgs eventArgs)
        {
            this.accountSettingsPage = null;
            await Task.Run(() => UpdateProfilePage(false));
        }
    }
}