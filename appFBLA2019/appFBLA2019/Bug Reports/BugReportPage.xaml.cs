//BizQuiz App 2019

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
    public partial class BugReportPage : ContentPage
    {
        public BugReportPage()
        {
            this.InitializeComponent();
        }

        private async void Submit_Clicked(object sender, EventArgs e)
        {
            if (this.BugBodyEntry.Text == "" || String.IsNullOrWhiteSpace((string)this.CategoryPicker.SelectedItem) || this.BugTitleEntry.Text == "")
            {
                await this.DisplayAlert("Cannot Submit", "Please fill all required fields before submitting a bug report.", "Keep editing");
            }
            else
            {
                if (!BugReportHandler.SubmitReport(new BugReport(this.BugTitleEntry.Text, this.CategoryPicker.SelectedItem as string, this.BugBodyEntry.Text)))
                {
                    await this.DisplayAlert("No Connection", "We couldn't connect to the server, so we'll send your report as soon as we can.", "OK");
                }
                await this.Navigation.PopAsync();
            }
        }

        private bool canClose = true;

        /// <summary>
        /// Overrides the backbutton to make sure the user really wants to leave
        /// </summary>
        /// <returns>  </returns>
        protected override bool OnBackButtonPressed()
        {
            if (this.canClose)
            {
                this.ShowExitDialogue();
            }

            return this.canClose;
        }

        /// <summary>
        /// Shows the exit dialogue to confirm if the user wants to leave without saving
        /// </summary>
        private async void ShowExitDialogue()
        {
            var answer = await this.DisplayAlert("Exit Creation", "Are you sure you want to leave? Your bug report will not be saved!", "Yes, leave", "No, keep my report");
            if (answer)
            {
                this.canClose = false;
                this.OnBackButtonPressed();
                await this.Navigation.PopAsync(true);
            }
        }

        private void InputSizeChanged(object sender, EventArgs e)
        {
            this.UpdateChildrenLayout();
        }
    }
}