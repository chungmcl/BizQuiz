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
    public partial class EditLevelPage : ContentPage
    {
        public EditLevelPage()
        {
            InitializeComponent();
            FindDatabase();
            
        }

        // Now the user can edit the questions, essentially the same as create new level but with everything filled out already.
        // This fires evertime the User selects an item in the selector.
        // Also this isn't actually being called for some reason.
        //void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        //{
        //    var picker = (Picker)sender;
        //    int selectedIndex = picker.SelectedIndex;

        //    if (selectedIndex != -1)
        //    {
        //        DatbaseNameLabel.Text = (string)picker.ItemsSource[selectedIndex];
        //    }
        //}

            // Get the list of Quizes the user can edit
        private void FindDatabase()
        {
            List<string> databaseList = new List<string>();
            // get a list of all databases the user has.
            // Either thats on the server or on the device.
            // Im not sure yet.
            databaseList.Add("levelTest");
            databaseList.Add("testdatabaseHEY");
            databaseList.Add("okay");
            PickerLevelSelect.ItemsSource = databaseList;
            DBHandler.SelectDatabase((string)PickerLevelSelect.SelectedItem, "testAuthor");
        }

        // Called when the user presses the button to edit a quiz
        private void ButtonEditLevel_Clicked(object sender, EventArgs e)
        {
            if(PickerLevelSelect.SelectedIndex != -1) // If the user has selected something then open the page
            {
                this.Navigation.PushAsync(new CreateNewLevelPage()); // Right now it just opens a new level, should open selected level
            }
            
        }
    }
}