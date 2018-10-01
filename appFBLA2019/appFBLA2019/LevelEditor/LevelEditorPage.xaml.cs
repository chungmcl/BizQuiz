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
			InitializeComponent ();
		}

        private void ButtonNewTextGameEditor_Clicked(object sender, EventArgs e)
        {
            this.Navigation.PushAsync(new TextGameEditor());
        }
    }
}