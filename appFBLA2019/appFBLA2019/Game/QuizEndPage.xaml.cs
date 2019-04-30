﻿//BizQuiz App 2019

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
        private double percentScore;
        private string quizName;
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
                new FacebookShareLinkContent($"I got {this.percentScore}% correct studying {this.quizName} on BizQuiz!", 
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
            //IShare shareinfo = CrossShare.Current;
            //await CrossShare.Current.Share(new ShareMessage
            //{
            //    Text = $"I got { this.percentScore }% correct studying { this.quizName } on BizQuiz!",
            //    Title = "Loving BizQuiz!",
            //    Url = "https://www.bizquiz.app/"
            //});
            await Share.RequestAsync($"I got { this.percentScore }% correct studying { this.quizName } on BizQuiz!", 
                "Loving BizQuiz!");
            this.ButtonShareToOtherMedia.IsEnabled = true;
        }

        private void ButtonShareToTwitter_Clicked(object sender, EventArgs e)
        {
            TwitterService twitter = new TwitterService()
            {
                ConsumerKey = "xxM17xeqLnMNXmWzbIbr9WSl4",
                ConsumerSecret = "hw1O2mgIbRjcQDDNWvKJgglIQ8WbYjY5lCDB8GCPp7IpdvenB0"
            };
            twitter.ShareItemAsync(new Item("test"), new Xamarin.Auth.Account("bhsappdev"));
        }

        private void ButtonShareToInstagram_Clicked(object sender, EventArgs e)
        {

        }
    }
}