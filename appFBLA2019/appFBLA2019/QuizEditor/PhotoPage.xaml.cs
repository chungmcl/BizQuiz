//BizQuiz App 2019

using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{


    /// Displays the photo a user adds to a question, they can either delete or change the image from here.
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PhotoPage : ContentPage
    {
        private ImageButton imageButton;
        private enum PhotoAction { back, change, delete }
        private PhotoAction currentAction;

        public PhotoPage(ImageButton image)
        {
            InitializeComponent();
            this.imageButton = image;
            this.pictureToEdit.Source = image.Source;
            this.pictureToEdit.Aspect = Aspect.AspectFit;
            this.pictureToEdit.HorizontalOptions = LayoutOptions.FillAndExpand;
            this.pictureToEdit.VerticalOptions = LayoutOptions.FillAndExpand;
        }

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