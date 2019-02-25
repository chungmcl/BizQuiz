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
	public partial class ForgotPasswordPage : ContentPage
	{
        private const int minLength = 5;

        public ForgotPasswordPage ()
		{
			InitializeComponent ();
            
		}

        private void ButtonContinue_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ButtonChangePassword_Clicked(object sender, EventArgs e)
        {
            if (this.EntryPassword.Text == this.EntryReenterPassword.Text && this.EntryPassword.Text.Length > minLength)
            {
                // change password
            }
            else
            {
                // tell user, passwords aren't equal or have to be longer than 5 characters.
            }
        }
    }
}