//BizQuiz App 2019

using System.Collections.Generic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using static Xamarin.Forms.Application;
using System.Threading.Tasks;
using Plugin.Connectivity;

namespace appFBLA2019
{
    /// <summary>
    /// A simple tabbed page that contains all of the levelselection pages
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LevelCategoriesPage : Xamarin.Forms.TabbedPage
    {
        public bool IsLoading { get; set; }
        /// <summary>
        /// Initializes and sets colors
        /// </summary>
        public LevelCategoriesPage()
        {
            this.InitializeComponent();
#if __ANDROID__
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetBarSelectedItemColor(Color.White);
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetBarItemColor(Color.DarkBlue);
#endif
        }

        /// <summary>
        /// Triggered when MainPage changes tab, refreshes all the pages within
        /// </summary>
        public void RefreshChildren()
        {
            this.IsLoading = true;
            foreach (Page page in this.Children)
            {
                (page as LevelSelectionPage).Setup();
            }
            this.IsLoading = false;
        }

        /// <summary>
        /// When the user wants to create a brand new level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void New_Activated(object sender, EventArgs e)
        {
            if (CredentialManager.IsLoggedIn)
            {
                CreateNewLevelPage level = new CreateNewLevelPage();
                this.Navigation.PushAsync(level);
            }
            else
            {
                if (await this.DisplayAlert("Hold on!", "Before you can create your own custom levels, you have to create your own account.", "Login/Create Account","Go Back" ))
                {
                    ProfilePage profilePage = new ProfilePage();
                    if (!profilePage.IsLoading && !profilePage.IsOnLoginPage)
                    {
                        await profilePage.UpdateProfilePage();
                    }
                    profilePage.SetTemporary();
                    await this.Navigation.PushAsync(profilePage);
                }
            }
        }
    }
}