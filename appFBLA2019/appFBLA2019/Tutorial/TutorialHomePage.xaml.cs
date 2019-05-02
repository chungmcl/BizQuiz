using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TutorialHomePage : ContentPage
    {
        public TutorialHomePage()
        {
            InitializeComponent();
        }

        private async void Next_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new TutorialGame());
        }
    }
}