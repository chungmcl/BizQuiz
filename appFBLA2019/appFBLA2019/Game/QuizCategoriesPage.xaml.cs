//BizQuiz App 2019

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using System.Threading.Tasks;

namespace appFBLA2019
{
    /// <summary>
    /// A simple tabbed page that contains all of the quizselection pages
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuizCategoriesPage : Xamarin.Forms.TabbedPage
    {
        public bool IsLoading { get; set; }
        /// <summary>
        /// Initializes and sets colors
        /// </summary>
        public QuizCategoriesPage()
        {
            this.InitializeComponent();
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetBarSelectedItemColor(Color.White);
            this.On<Xamarin.Forms.PlatformConfiguration.Android>().SetBarItemColor(Color.DarkBlue);
        }

        /// <summary>
        /// Triggered when MainPage changes tab, refreshes all the pages within
        /// </summary>
        public async Task RefreshChildrenAsync()
        {
            this.IsLoading = true;
            foreach (Page page in this.Children)
            {
                await (page as QuizSelectionPage).RefreshPageAsync();
            }
            this.IsLoading = false;
        }
    }
}