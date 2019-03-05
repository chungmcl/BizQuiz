//BizQuiz App 2019


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
    }
}