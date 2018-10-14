﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LevelSelectionPage : ContentPage
	{
		public LevelSelectionPage ()
		{
            this.InitializeComponent ();
		}

        private void History_Clicked(object sender, EventArgs e)
        {
            Level level = new Level("test");
            this.Navigation.PushAsync(new TextGame(level));
        }
    }
}