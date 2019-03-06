//BizQuiz App 2019

using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : Xamarin.Forms.TabbedPage
    {
        /// <summary>
        /// ID for the tabs
        /// </summary>
        public enum TabID : int { quizCategoriesPage, quizStorePage, profilePage}

        /// <summary>
        /// loads the main page and sets up the toolbars
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            // Default tabs on Android are on top - Set to bottom on Android (to serve as Navigation)
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetBarSelectedItemColor(Color.Accent);
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetBarItemColor(Color.Gray); //Color.FromHex("#003463")
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetIsSwipePagingEnabled(false);
            this.BarTextColor = Color.Gray;
        }

        /// <summary>
        /// when pages are opened, update them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TabbedPage_CurrentPageChanged(object sender, EventArgs e)
        {
            var index = (TabID)this.Children.IndexOf(this.CurrentPage);

            // If your called method requires async code in initialization, define an "IsLoading" property in your page and change and check that accordingly
            switch (index)
            {
                case TabID.profilePage:
                    {
                        ProfilePage profilePage = (ProfilePage)this.TabbedPagePage.Children[(int)TabID.profilePage];
                        if (!profilePage.IsLoading && !profilePage.IsOnLoginPage)
                        {
                            await profilePage.UpdateProfilePageAsync();
                        }
                    }
                break;
            }
        }

        /// <summary>
        /// Open the terms of use
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TermsOfUse_Activated(object sender, EventArgs e)
        {
            await this.Navigation.PushModalAsync(new TermsOfUse());
        }

        /// <summary>
        /// Open the bug report screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BugReportToolbarItem_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new BugReportPage());
        }

        /// <summary>
        /// Open the about page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AboutPageToolbarItem_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushModalAsync(new AboutUsPage());
        }

        /// <summary>
        /// Open the help page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushModalAsync(new HelpPage());
        }

        /// <summary>
        /// Open the advanced settings page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AdvancedSettings_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushModalAsync(new AdvancedSettingsPage());
        }
    }
}