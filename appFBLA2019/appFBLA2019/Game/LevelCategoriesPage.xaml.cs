//BizQuiz App 2019

using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LevelCategoriesPage : Xamarin.Forms.TabbedPage
    {
        public LevelCategoriesPage()
        {
            this.InitializeComponent();
#if __ANDROID__
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetBarSelectedItemColor(Color.White);
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetBarItemColor(Color.DarkBlue);
#endif
        }

        public void RefreshChildren()
        {
            foreach (Page page in this.Children)
            {
                (page as LevelSelectionPage).Setup();
            }
        }
    }
}