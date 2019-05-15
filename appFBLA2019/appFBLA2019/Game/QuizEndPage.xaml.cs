//BizQuiz App 2019

using Plugin.FacebookClient;
using Plugin.FacebookClient.Abstractions;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

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

        private string ShareText
        {
            get
            {
                string text = $"I got {this.percentScore.ToString("0.##")}% correct studying {this.quizName} on BizQuiz! " +
                    $"Study with me and let's achieve great things in FBLA together! ";

                if (CredentialManager.IsLoggedIn)
                    text = text + $"Find me on BizQuiz @{CredentialManager.Username}";

                return text;
            }
        }

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

                case double x when (x >= 100):
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

            await this.LabelFeedback.ScaleTo(1, 250, Easing.BounceIn);
            await this.LabelScore.ScaleTo(1, 1000, Easing.BounceIn);
        }

        protected override bool OnBackButtonPressed()
        {
            OnFinished();
            return base.OnBackButtonPressed();
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
                new FacebookShareLinkContent(this.ShareText, 
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
            request.Text = this.ShareText;
            request.Uri = "https://www.bizquiz.app/";
            await Share.RequestAsync(request);

            this.ButtonShareToOtherMedia.IsEnabled = true;
        }

        private async void ButtonShareToTwitter_Clicked(object sender, EventArgs e)
        {
            this.ButtonShareToTwitter.IsEnabled = false;
            bool twitterInstalled = DependencyService.Get<IGetStorage>().IsPackageInstalled(twitterPackageName);
            string shareURI = $"https://twitter.com/share?text={Uri.EscapeUriString(this.ShareText)}&url=http://www.bizquiz.app&hashtags=BizQuiz";
            if (!twitterInstalled)
            {
                bool result = 
                    await DisplayAlert("Twitter not installed", "Twitter is not installed on this device. Open in browser?", "OK", "Cancel");

                if (result)
                    Device.OpenUri(new Uri(shareURI));
            }
            else
            {
                Device.OpenUri(new Uri(shareURI));
            }
            this.ButtonShareToTwitter.IsEnabled = true;
        }
    }
}