//BizQuiz App 2019

using Plugin.FacebookClient;
using Plugin.FacebookClient.Abstractions;
using Plugin.Share;
using Plugin.Share.Abstractions;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Xamarin.Social;
using Xamarin.Social.Services;

namespace appFBLA2019
{
    /// <summary>
    /// Showed at the end of the quiz
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuizEndPage : ContentPage
    {
        private const string twitterPackageName = "com.twitter.android";
        private const string instagramPackageName = "com.instagram.android";

        private double percentScore;
        private string quizName;

        private string shareText { get { return 
                    $"I got {this.percentScore.ToString("#.##")}% correct studying {this.quizName} on BizQuiz!"; } }

        /// <summary>
        /// Constructs the page and sets the score
        /// </summary>
        /// <param name="score">           </param>
        /// <param name="totalQuestions">  </param>
        public QuizEndPage(double score, int totalQuestions, string quizName)
        {
            this.InitializeComponent();
            this.quizName = quizName;
            this.percentScore = score / (totalQuestions) * 100;
            switch (this.percentScore)
            {
                case double x when (x < 60):
                    this.LabelFeedback.Text = "Uh oh, looks like you need to practice some more...";
                    break;

                case double x when (x > 60 && x < 90):
                    this.LabelFeedback.Text = "Good job, but there's room for improvement!";
                    break;

                case double x when (x > 90 && x < 100):
                    this.LabelFeedback.Text = "Great job! Just a little more work and you're an expert!";
                    break;

                case double x when (x == 100):
                    this.LabelFeedback.Text = "Wow! You're a master!";
                    break;
            }
            this.LabelScore.Text = $"{score}/{totalQuestions}";
        }

        /// <summary>
        /// event handler used to close this and the textgame simultaneously
        /// </summary>
        /// <param name="source">     </param>
        /// <param name="eventArgs">  </param>
        public delegate void FinishedEventHandler(object source, EventArgs eventArgs);

        /// <summary>
        /// event handler used to close this and the textgame simultaneously
        /// </summary>
        /// <param name="source">     </param>
        /// <param name="eventArgs">  </param>
        public event FinishedEventHandler Finished;

        /// <summary>
        /// some nifty animations to make this page more interesting
        /// </summary>
        protected override async void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            this.LabelFeedback.Scale = 0.00001;
            this.LabelScore.Scale = 0.00001;
            this.ButtonShareToFacebook.Scale = 0.00001;
            this.ButtonShareToOtherMedia.Scale = 0.00001;
            this.ButtonDone.Scale = 0.00001;

            await this.LabelFeedback.ScaleTo(1, 250, Easing.BounceIn);
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

        /// <summary>
        /// closes this page and the page below it
        /// </summary>
        /// <param name="sender">  </param>
        /// <param name="e">       </param>
        private async void ButtonDone_Clicked(object sender, EventArgs e)
        {
            this.ButtonDone.IsEnabled = false;
            this.OnFinished();
            await this.Navigation.PopModalAsync(true);
            this.ButtonDone.IsEnabled = true;
        }

        /// <summary>
        /// brings up a dialog to share to facebook
        /// </summary>
        /// <param name="sender">  </param>
        /// <param name="e">       </param>
        private async void ButtonShareToFacebook_Clicked(object sender, EventArgs e)
        {
            this.ButtonShareToFacebook.IsEnabled = false;

            FacebookShareLinkContent linkContent = 
                new FacebookShareLinkContent(this.shareText, 
                new Uri("https://www.bizquiz.app/"));
            var ret = await CrossFacebookClient.Current.ShareAsync(linkContent);
            this.ButtonShareToFacebook.IsEnabled = true;
        }

        /// <summary>
        /// brings up the system sharing dialog
        /// </summary>
        /// <param name="sender">  </param>
        /// <param name="e">       </param>
        private async void ButtonShareToOtherMedia_Clicked(object sender, EventArgs e)
        {
            this.ButtonShareToOtherMedia.IsEnabled = false;

            ShareTextRequest request = new ShareTextRequest();
            request.Title = $"Share to other social media platforms";
            request.Text = this.shareText;
            request.Uri = "https://www.bizquiz.app/";
            await Share.RequestAsync(request);

            this.ButtonShareToOtherMedia.IsEnabled = true;
        }

        private void ButtonShareToTwitter_Clicked(object sender, EventArgs e)
        {
            this.ButtonShareToTwitter.IsEnabled = false;

            Device.OpenUri(new Uri("https://twitter.com/"));

            this.ButtonShareToTwitter.IsEnabled = true;
        }

        private void ButtonShareToInstagram_Clicked(object sender, EventArgs e)
        {
            this.ButtonShareToInstagram.IsEnabled = false;
            DependencyService.Get<ISocialMedia>().CreateInstagramIntent("/storage/emulated/0/DCIM/Camera/bothell boy og.png", "test");
            if (DependencyService.Get<ISocialMedia>().IsPackageInstalled(instagramPackageName))
            {
                DependencyService.Get<ISocialMedia>().CreateInstagramIntent("/storage/emulated/0/DCIM/Camera/bothell boy og.png", "test");
            }
            else
            {
                Device.OpenUri(new Uri("https://www.instagram.com/"));
            }
            this.ButtonShareToInstagram.IsEnabled = true;
        }
    }
}