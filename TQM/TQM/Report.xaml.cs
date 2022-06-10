using SQLite;
using System;
using System.Collections.Generic;
using TQM.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Report : ContentPage
    {
        private Guid selectedMachineID = Guid.Empty;
        private string selectedMachineName = null;
        public Report()
        {
            InitializeComponent();
        }

        private void btn_getreport_Clicked(object sender, EventArgs e)
        {
            if (date_enddate.Date < date_fromdate.Date)
            {
                DisplayAlert("Attention", "Report End Date cannot be less than Report Start Date", "OK");
                return;
            }
            string selectedCategory = null;
            if (picker_machinecategory.SelectedItem != null) { selectedCategory = picker_machinecategory.SelectedItem.ToString(); };
            Navigation.PushAsync(new YCReport(date_fromdate.Date, date_enddate.Date, selectedCategory, selectedMachineID));
        }

        private void picker_machinename_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<MachineModel>();
                List<MachineModel> machineNames = conn.Table<MachineModel>().Where(MachineModel => MachineModel.machineCategory == picker_machinename.SelectedItem).ToList();
                picker_machinecategory.ItemsSource = machineNames;
            }
        }

        private void picker_machinecategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<MachineModel> source = (List<MachineModel>)picker_machinename.ItemsSource;
            selectedMachineID = (Guid)source[picker_machinename.SelectedIndex].ID;
            MachineModel selectedMachine = (MachineModel)picker_machinename.SelectedItem;
            selectedMachineName = selectedMachine.machineName;
        }
    }
}