using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Internals;

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
            this.BarTextColor = Color.White;
        }
	}
}