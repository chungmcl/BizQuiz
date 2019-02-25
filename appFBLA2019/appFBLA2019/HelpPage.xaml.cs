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
    public partial class HelpPage : CarouselPage
    {
        public HelpPage()
        {
            InitializeComponent();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            this.MCQuestionImage.WidthRequest = this.TextQuestionImage.WidthRequest = this.Width * 2 / 5 ;
        }
    }
}