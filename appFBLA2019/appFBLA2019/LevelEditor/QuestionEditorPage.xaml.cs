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
	public partial class QuestionEditorPage : ContentPage
	{
        public delegate void AddPageEventHandler(object source, EventArgs eventArgs);
        public event AddPageEventHandler PageAdded;
        public QuestionEditorPage ()
		{
			InitializeComponent ();
		}

        protected virtual void OnPageAdded()
        {
            this.PageAdded?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonAddQuestion_Clicked(object sender, EventArgs e)
        {
            OnPageAdded();
        }
    }
}