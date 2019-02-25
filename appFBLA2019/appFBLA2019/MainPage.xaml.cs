//BizQuiz App 2019

using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : Xamarin.Forms.TabbedPage
    {
        public enum TabID : int { quizCategoriesPage, quizStorePage, profilePage}

        public MainPage()
        {
            this.InitializeComponent();
            // Default tabs on Android are on top - Set to bottom on Android (to serve as Navigation)
#if __ANDROID__
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetBarSelectedItemColor(Color.Accent);
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetBarItemColor(Color.Gray); //Color.FromHex("#003463")
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetIsSwipePagingEnabled(false);
#endif
            this.BarTextColor = Color.Gray;
        }

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
                        await profilePage.UpdateProfilePage();
                    }
                }
                break;

                case TabID.quizCategoriesPage:
                {
                    //this.quizsPage.RefreshChildren();
                }
                break;
            }
        }

        private void TermsOfUse_Activated(object sender, EventArgs e)
        {
            this.Navigation.PushModalAsync(new TermsOfUse());
        }

        private void BugReportToolbarItem_Clicked(object sender, EventArgs e)
        {
            this.Navigation.PushAsync(new BugReportPage());
        }

        private void AboutPageToolbarItem_Clicked(object sender, EventArgs e)
        {
            this.Navigation.PushModalAsync(new AboutUsPage());
        }

        private async void TutorialButton_Clicked(object sender, EventArgs e)
        {
            this.Navigation.PushAsync(new HelpPage());
        }

        private void HelpButton_Clicked(object sender, EventArgs e)
        {
            this.Navigation.PushModalAsync(new HelpPage());
        }
    }
}