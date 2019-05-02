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
    public partial class TutorialPage : ContentPage
    {
        int tutorialIndex = 0;

        double StandardX { get { return this.StackTutorials.Children[this.tutorialIndex].X; } }

        public TutorialPage()
        {
            InitializeComponent();
        }

        private async void Next_Clicked(object sender, EventArgs e)
        {
            this.ButtonNext.IsEnabled = false;
            
            this.StackTutorials.Children[this.tutorialIndex + 1].TranslationX = this.Width;

            await this.StackTutorials.Children[this.tutorialIndex].TranslateTo(-this.Width, this.Y, easing: Easing.CubicInOut);
            this.StackTutorials.Children[this.tutorialIndex].IsVisible = false;
            this.StackTutorials.Children[this.tutorialIndex + 1].IsVisible = true;
            await this.StackTutorials.Children[this.tutorialIndex + 1].TranslateTo(StandardX, this.Y, easing: Easing.CubicInOut);

            this.tutorialIndex++;

            this.ButtonNext.IsEnabled = true;
            if (this.tutorialIndex == this.StackTutorials.Children.Count - 1)
            {
                this.ButtonDone.IsVisible = true;
                this.ButtonNext.IsVisible = false;
            }
            else
            {
                this.ButtonPrevious.IsVisible = true;
            }
        }

        private async void Previous_Clicked(object sender, EventArgs e)
        {
            this.ButtonPrevious.IsEnabled = false;
            this.StackTutorials.Children[this.tutorialIndex - 1].TranslationX = -this.Width;

            await this.StackTutorials.Children[this.tutorialIndex].TranslateTo(this.Width, this.Y, easing: Easing.CubicInOut);
            this.StackTutorials.Children[this.tutorialIndex].IsVisible = false;
            this.StackTutorials.Children[this.tutorialIndex - 1].IsVisible = true;
            await this.StackTutorials.Children[this.tutorialIndex - 1].TranslateTo(StandardX, this.Y, easing: Easing.CubicInOut);

            this.tutorialIndex--;

            this.ButtonPrevious.IsEnabled = true;
            if (this.tutorialIndex == 0)
                this.ButtonPrevious.IsVisible = false;
            else
            {
                this.ButtonNext.IsVisible = true;
                this.ButtonDone.IsVisible = false;
            }
        }

        private void Skip_Clicked(object sender, EventArgs e)
        {
            this.Navigation.PopToRootAsync();
        }

        private void Done_Clicked(object sender, EventArgs e)
        {

        }
    }
}