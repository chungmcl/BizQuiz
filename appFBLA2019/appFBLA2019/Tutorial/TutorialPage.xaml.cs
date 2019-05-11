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
        /// <summary>
        /// Current tutorial page loaded
        /// </summary>
        int tutorialIndex = 0;

        /// <summary>
        /// standard X coordinate for translating the tutorial cards
        /// </summary>
        double StandardX { get { return this.StackTutorials.Children[this.tutorialIndex].X; } }

        /// <summary>
        /// The page that called this
        /// </summary>
        Page ParentPage;

        /// <summary>
        /// The tutorial/help page that explains to the user how to use bizquiz.
        /// </summary>
        /// <param name="isStartup"></param>
        /// <param name="ParentPage"></param>
        public TutorialPage(bool isStartup, Page ParentPage)
        {
            InitializeComponent();
            this.ParentPage = ParentPage;

            if (isStartup)
            {
                this.ButtonDone.Text = "Create Account";
                this.ButtonSkip.Text = "Skip Tutorial";
                this.ButtonSkip.Clicked += this.Done_Clicked;
                this.ButtonSkip.Clicked += this.Done_Clicked;
                this.ButtonDone.Clicked += this.Done_Clicked;
                Xamarin.Forms.Application.Current.Properties["Tutorial"] = "Started";
            }
            else
            {
                this.ButtonDone.Text = "Exit Tutorial";
                this.ButtonSkip.Text = "Exit Tutorial";
                this.ButtonDone.Clicked += this.Finish_Clicked;
                this.ButtonSkip.Clicked += this.Finish_Clicked;
                
            }
        }

        /// <summary>
        /// Changes the tutorial card to the next in the stack
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Goes back one tutorial page in the stack
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Exist the tutorial page, used for starting up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Done_Clicked(object sender, EventArgs e)
        {
            if (Xamarin.Forms.Application.Current.Properties.ContainsKey("Tutorial"))
            {
                Xamarin.Forms.Application.Current.Properties["Tutorial"] = "Done";
            }
            
            (this.ParentPage as TabbedPage).CurrentPage = (this.ParentPage as TabbedPage).Children[2];

            await this.Navigation.PopToRootAsync();
        }

        /// <summary>
        /// Exist the tutorial, used for coming from the overflow menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Finish_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PopModalAsync();
        }
    }
}