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
	public partial class LevelEditorPage : ContentPage
	{
		public LevelEditorPage ()
		{
			this.InitializeComponent ();
        }

        private void ButtonEdit_Clicked(object sender, EventArgs e)
        {
            this.Navigation.PushAsync(new EditLevelPage());
        }

        private void ButtonCreate_Clicked(object sender, EventArgs e)
        {
            this.Navigation.PushAsync(new CreateNewLevelPage());
        }
    }
}