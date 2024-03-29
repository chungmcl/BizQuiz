﻿//BizQuiz App 2019

using Plugin.Media;
using System;
using System.IO;
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
                BugReport report = new BugReport(this.BugTitleEntry.Text, this.CategoryPicker.SelectedItem as string, this.BugBodyEntry.Text, this.ImagePath);
                BugReportHandler.SaveReport(report);
                Task task = Task.Run(() => BugReportHandler.SubmitSavedReports());
                await this.DisplayAlert("Report Saved", "We'll send your report as soon as we can, and our team will take a look at your issue. Thanks for letting us know!", "OK");
                await task;
                await this.Navigation.PopAsync();
            }
        }

        private string ImagePath;
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

        private async void AddImage_Clicked(object sender, EventArgs e)
        {
            if (this.BugImage.IsEnabled)
            {
                switch (await this.DisplayActionSheet("Image Options", "Cancel", "Remove", "Change"))
                {
                    case "Cancel":
                        return;

                    case "Remove":
                        this.BugImage.IsEnabled = false;
                        this.BugImageFrame.IsEnabled = false;
                        this.BugImageFrame.IsVisible = false;
                        this.ImagePath = null;
                        return;

                    case "Change":
                    default:
                        break;
                }
            }

            await CrossMedia.Current.Initialize();
            Plugin.Media.Abstractions.MediaFile file = await CrossMedia.Current.PickPhotoAsync();

            if (file != null) // if the user actually picked an image
            {
                MemoryStream memoryStream = new MemoryStream();
                file.GetStream().CopyTo(memoryStream);

                if (memoryStream.Length < 3000000)
                {
                    this.BugImage.IsEnabled = true;
                    this.BugImageFrame.IsEnabled = true;
                    this.BugImageFrame.IsVisible = true;
                    this.BugImage.Source = FileImageSource.FromFile(file.Path);
                    this.ImagePath = file.Path;
                }
                else
                {
                    await this.DisplayAlert("Couldn't use Picture", "Pictures must be under 3 MB", "Back");
                }
                file.Dispose();
            }
        }
    }
}