using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace appFBLA2019
{
    public partial class MainPage : TabbedPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void ButtonToLoginPage_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new LoginPage(""));
        }
    }
}
