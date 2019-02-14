//BizQuiz App 2019

using System.Collections.Generic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

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
    }
}