using System;
using System.Collections.Generic;
using SQLite;
using TQM.Model;
using Xamarin.Forms;

namespace TQM
{	
	public partial class TestHome : ContentPage
	{
        private string selectedCategory = null;
        private Guid selectedMachineID = Guid.Empty;
        private string selectedMachineName = null;
        public TestHome ()
		{
			InitializeComponent ();
		}

        private void picker_machinename_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                List<MachineModel> source = (List<MachineModel>)picker_machinename.ItemsSource;
                if (picker_machinename.SelectedIndex < 0) { return; }
                selectedMachineID = (Guid)source[picker_machinename.SelectedIndex].ID;
                MachineModel selectedMachine = (MachineModel)picker_machinename.SelectedItem;
                selectedMachineName = selectedMachine.machineName;
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-Home", ex.Message.ToString(), "Ok");
            }
        }

        private void picker_machinecategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (picker_machinecategory.SelectedItem == null) { return; }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    selectedCategory = picker_machinecategory.SelectedItem.ToString();
                    conn.CreateTable<MachineModel>();
                    List<MachineModel> machines = conn.Table<MachineModel>().Where(
                        MachineModel => MachineModel.machineCategory == selectedCategory).ToList();
                    picker_machinename.ItemsSource = machines;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-Home", ex.Message.ToString(), "Ok");
            }
        }
    }
}

