//BizQuiz App 2019

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{


    /// Displays the photo a user adds to a question, they can either delete or change the image from here.
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PhotoPage : ContentPage
    {
        /// <summary>
        /// button with the image
        /// </summary>
        private ImageButton imageButton;
        /// <summary>
        /// different actions to take on a photo
        /// </summary>
        private enum PhotoAction { back, change, delete }
        /// <summary>
        /// what we're doing right now
        /// </summary>
        private PhotoAction currentAction;

        /// <summary>
        /// creates a photopage to edit an image
        /// </summary>
        /// <param name="image"></param>
        public PhotoPage(ImageButton image)
        {
            InitializeComponent();
            this.imageButton = image;
            this.pictureToEdit.Source = image.Source;
            this.pictureToEdit.Aspect = Aspect.AspectFit;
            this.pictureToEdit.HorizontalOptions = LayoutOptions.FillAndExpand;
            this.pictureToEdit.VerticalOptions = LayoutOptions.FillAndExpand;
        }

        /// <summary>
        /// When the page disappears, set the styleID to convey what the action was
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (this.currentAction == PhotoAction.back)
            {
                this.imageButton.StyleId = "back";
            }
            else if (this.currentAction == PhotoAction.change)
            {
                this.imageButton.StyleId = "change";
            }
            else if (this.currentAction == PhotoAction.delete)
            {
                this.imageButton.StyleId = "delete";
            }
        }   

        /// <summary>
        /// Event handler for when the user wants to change the image from the qustion
        /// </summary>
        private void ChangePhoto_Activated(object sender, EventArgs e)
        {
            this.currentAction = PhotoAction.change;
            this.Navigation.PopAsync();
        }

        /// <summary>
        /// Event handler for when the user wants to delete the image from the qustion
        /// </summary>
        private void Remove_Activated(object sender, EventArgs e)
        {
            this.currentAction = PhotoAction.delete;
            this.Navigation.PopAsync();
        }
    }
}