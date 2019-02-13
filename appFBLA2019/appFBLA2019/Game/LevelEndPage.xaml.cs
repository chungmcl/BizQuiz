//BizQuiz App 2019

using Plugin.FacebookClient;
using Plugin.FacebookClient.Abstractions;
using Plugin.Share;
using Plugin.Share.Abstractions;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    /// <summary>
    /// Showed at the end of the level
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LevelEndPage : ContentPage
    {
        /// <summary>
        /// Constructs the page and sets the score
        /// </summary>
        /// <param name="score">
        /// </param>
        /// <param name="totalQuestions">
        /// </param>
        public LevelEndPage(int score, int totalQuestions)
        {
            this.InitializeComponent();
            this.percentscore = score / (totalQuestions * 2.0);
            this.LabelScore.Text = $"{score}/{totalQuestions * 2}";
        }

        /// <summary>
        /// event handler used to close this and the textgame simultaneously
        /// </summary>
        /// <param name="source">
        /// </param>
        /// <param name="eventArgs">
        /// </param>
        public delegate void FinishedEventHandler(object source, EventArgs eventArgs);

        /// <summary>
        /// event handler used to close this and the textgame simultaneously
        /// </summary>
        /// <param name="source">
        /// </param>
        /// <param name="eventArgs">
        /// </param>
        public event FinishedEventHandler Finished;

        /// <summary>
        /// some nifty animations to make this page more interesting
        /// </summary>
        protected override async void OnAppearing()
        {
            this.LabelScore.Scale = 0.00001;
            this.ButtonShareToFacebook.Scale = 0.00001;
            this.ButtonShareToOtherMedia.Scale = 0.00001;
            this.ButtonDone.Scale = 0.00001;

            await this.LabelScore.ScaleTo(1, 1000, Easing.BounceIn);

            await this.ButtonShareToFacebook.ScaleTo(1, 500, Easing.SpringOut);
            await this.ButtonShareToOtherMedia.ScaleTo(1, 500, Easing.SpringOut);
            await this.ButtonDone.ScaleTo(1, 500, Easing.SpringOut);
        }

        /// <summary>
        /// closes the page underneath
        /// </summary>
        protected virtual void OnFinished()
        {
            this.Finished?.Invoke(this, EventArgs.Empty);
        }

        private double percentscore;

        /// <summary>
        /// closes this page and the page below it
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private async void ButtonDone_Clicked(object sender, EventArgs e)
        {
            this.OnFinished();
            await this.Navigation.PopModalAsync(true);
        }

        /// <summary>
        /// brings up a dialog to share to facebook
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private async void ButtonShareToFacebook_Clicked(object sender, EventArgs e)
        {
            FacebookShareLinkContent linkContent = new FacebookShareLinkContent("Check out my score!", new Uri("https://github.com/chungmcl"));
            var ret = await CrossFacebookClient.Current.ShareAsync(linkContent);
        }

        /// <summary>
        /// brings up the system sharing dialog
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private async void ButtonShareToOtherMedia_Clicked(object sender, EventArgs e)
        {
            IShare shareinfo = CrossShare.Current;
            await CrossShare.Current.Share(new ShareMessage
            {
                Text = "Check out my github",
                Title = "Title",
                Url = "https://github.com/chungmcl",
            });
        }
    }
}