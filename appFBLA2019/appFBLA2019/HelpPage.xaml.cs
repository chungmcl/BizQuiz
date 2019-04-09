//BizQuiz App 2019


using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HelpPage : CarouselPage
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public HelpPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set up sizes
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            this.MCQuestionImage.WidthRequest = this.TextQuestionImage.WidthRequest = this.Width * 2 / 5 ;
        }

        private async void ButtonEmail_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                Device.OpenUri(new Uri("mailto:chungmcl@hotmail.com?subject=BizQuiz&body=I really enjoyed your BizQuiz app!"));
            }
            catch
            {
                await this.DisplayAlert("Couldn't email", "You don't have an email app installed!", "OK");
            }
        }

        private async void ButtonToBugReporter_Clicked(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new BugReportPage());
        }
    }
}