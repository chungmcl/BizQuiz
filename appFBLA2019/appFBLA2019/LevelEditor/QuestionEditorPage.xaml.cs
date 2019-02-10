//BizQuiz App 2019

using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuestionEditorPage : ContentPage
    {
        public QuestionEditorPage()
        {
            this.InitializeComponent();
        }

        public delegate void AddPageEventHandler(object source, EventArgs eventArgs);

        public event AddPageEventHandler PageAdded;

        protected virtual void OnPageAdded()
        {
            this.PageAdded?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonAddQuestion_Clicked(object sender, EventArgs e)
        {
            this.OnPageAdded();
        }
    }
}