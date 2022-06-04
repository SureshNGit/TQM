
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    using SQLite;
    using System;
    using TQM.Model;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class machinePage : ContentPage
    {
        public machinePage()
        {
            InitializeComponent();
        }

        private void addMachineButton_Clicked(object sender, System.EventArgs e)
        {
            MachineModel machinemodel = new MachineModel()
            {
                machineName = machineNameEntry.Text,
                createdate = DateTime.Now
            };

            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<MachineModel>();
                int row = conn.Insert(machinemodel);
                if (row > 0)
                {
                    DisplayAlert("Success", "Machine Added Successfully!!!", "OK");
                }
                else
                {
                    DisplayAlert("Failure", "Machine failed to be inserted!!!", "OK");
                }
            }
        }
    }
}