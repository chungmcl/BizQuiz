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
            InitializeComponent();
        }

        private void BugBodyEntry_Focused(object sender, FocusEventArgs e)
        {

        }

        private void BugBodyEntry_Unfocused(object sender, FocusEventArgs e)
        {

        }
    }
}