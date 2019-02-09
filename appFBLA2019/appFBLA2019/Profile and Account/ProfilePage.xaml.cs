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
        public ProfilePage()
        {
            this.InitializeComponent();
        }

        public bool IsLoading { get; set; }

        public async Task UpdateProfilePage(bool updateLoginStatus)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                this.ActivityIndicator.IsVisible = true;
                this.ActivityIndicator.IsRunning = true;
                this.IsLoading = true;
            });

            if (updateLoginStatus)
            {
                await Task.Run(() => CredentialManager.CheckLoginStatus());
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                this.StackLayoutProfilePageContent.IsVisible = CredentialManager.IsLoggedIn;
                this.LocalLoginPage.IsVisible = !(CredentialManager.IsLoggedIn);
                if (this.StackLayoutProfilePageContent.IsVisible)
                {
                    ToolbarItem accountSettingsButton = new ToolbarItem();
                    accountSettingsButton.Clicked += ToolbarItemAccountSettings_Clicked;
                    accountSettingsButton.Icon = FileImageSource.FromResource("ic_settings_black_48dp.png") as FileImageSource;
                }
                else
                {
                    if (this.ToolbarItems.Count > 0)
                    {
                        this.ToolbarItems.RemoveAt(0);
                    }
                }

                this.ActivityIndicator.IsRunning = false;
                this.ActivityIndicator.IsVisible = false;
                this.IsLoading = false;
            });
        }

        private async void ToolbarItemAccountSettings_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new AccountSettingsPage());
        }
    }
}