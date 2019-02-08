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
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LevelEndPage : ContentPage
    {
        public LevelEndPage(ScoreRecord score, int totalQuestions)
        {
            this.InitializeComponent();
            DBHandler.Database.AddScore(score);
            this.LabelScore.Text = $"{score.Score}/{totalQuestions * 2}";
        }

        public delegate void FinishedEventHandler(object source, EventArgs eventArgs);

        public event FinishedEventHandler Finished;

        protected virtual void OnFinished()
        {
            this.Finished?.Invoke(this, EventArgs.Empty);
        }

        private ScoreRecord score;

        private async void ButtonDone_Clicked(object sender, EventArgs e)
        {
            this.OnFinished();
            await this.Navigation.PopModalAsync(true);
        }

        private async void ButtonShareToFacebook_Clicked(object sender, EventArgs e)
        {
            FacebookShareLinkContent linkContent = new FacebookShareLinkContent("Check out my score!", new Uri("https://github.com/chungmcl"));
            var ret = await CrossFacebookClient.Current.ShareAsync(linkContent);
        }

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