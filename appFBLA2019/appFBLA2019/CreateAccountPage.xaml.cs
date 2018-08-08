using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appFBLA2019
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateAccountPage : ContentPage
    {
        public CreateAccountPage()
        {
            InitializeComponent();
        }

        private async void ButtonCreateAccount_Clicked(object sender, EventArgs e)
        {
            try
            {
                await ServerConnector.QueryDB($"createAccount/{this.EntryUsername.Text}/{this.EntryPassword.Text}" +
                    $"/{this.EntryEmail.Text}/-");
                string databaseReturnInfo = await ServerConnector.ReceiveFromDB();

                if (databaseReturnInfo == "true/-")
                {
                    this.LabelMessage.Text = "Account successfully created.";
                    await this.Navigation.PushAsync(new EmailConfirmationPage(this.EntryUsername.Text));
                }
                else
                {
                    string errorMessage = (databaseReturnInfo.Split('/'))[1];
                    this.LabelMessage.Text = $"Account could not be created: {errorMessage}";
                }
            }
            catch
            {
                this.LabelMessage.Text = "Connection Error Occured: Please Try Again.";
            }
        }
    }
}