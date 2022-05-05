
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    using SQLite;
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
            machineModel machine = new machineModel()
            {
                machineName = machineNameEntry.Text,
            };

            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<machineModel>();
                int row = conn.Insert(machine);
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