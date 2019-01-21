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
	public partial class TextGameEditor : CarouselPage
	{
		public TextGameEditor ()
		{
            OnPageAdded(this, EventArgs.Empty);
        }

        private void OnPageAdded(object source, EventArgs args)
        {
            QuestionEditorPage questionEditorPage = new QuestionEditorPage();
            this.Children.Add(questionEditorPage);
            questionEditorPage.PageAdded += this.OnPageAdded;
        }

	}
}