using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019.LevelEditor
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PhotoPage : ContentPage
	{
        ImageButton imageButton;
        int currentAction = 0;
		public PhotoPage (ImageButton image)
		{
			InitializeComponent ();
            this.imageButton = image;
            this.pictureToEdit.Source = image.Source;
            this.pictureToEdit.Aspect = Aspect.AspectFit;
            this.pictureToEdit.HorizontalOptions = LayoutOptions.FillAndExpand;
            this.pictureToEdit.VerticalOptions = LayoutOptions.FillAndExpand;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (currentAction == 0)
            {
                imageButton.StyleId = "back";
            }
            else if (currentAction == 1)
            {
                imageButton.StyleId = "change";
            }
            else if (currentAction == 2)
            {
                imageButton.StyleId = "delete";
            }
        }

        private void ChangePhoto_Activated(object sender, EventArgs e)
        {
            currentAction = 1;
            Navigation.PopAsync();
        }

        private void Remove_Activated(object sender, EventArgs e)
        {
            currentAction = 2;
            Navigation.PopAsync();
        }
    }
}