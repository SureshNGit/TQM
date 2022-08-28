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

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                reset();
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-ReportSearch", ex.Message.ToString(), "Ok");
            }
        }

        private void reset()
        {
            try
            {
                picker_reportName.SelectedIndex = 0;
                selectedMachineID = Guid.Empty;
                selectedMachineName = null;
                date_fromdate.Date = DateTime.Now;
                date_enddate.Date = DateTime.Now;
                picker_machinecategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-ReportSearch", ex.Message.ToString(), "Ok");
            }
        }

        private void btn_getreport_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (picker_reportName.SelectedIndex <= 0)
                {
                    DisplayAlert("Attention", "Please select report name to proceed!!!", "OK");
                    return;
                }
                if (date_enddate.Date < date_fromdate.Date)
                {
                    DisplayAlert("Attention", "Report End Date cannot be less than Report Start Date", "OK");
                    return;
                }
                if (date_fromdate.Date > date_fromdate.Date)
                {
                    DisplayAlert("Attention", "Report Start Date cannot be greater than Report End Date", "OK");
                    return;
                }
                string selectedCategory = null;
                if (picker_machinecategory.SelectedItem != null) { selectedCategory = picker_machinecategory.SelectedItem.ToString(); };
                if (picker_reportName.SelectedItem.ToString() == "Yarn Count")
                {
                    Navigation.PushAsync(new YCReport(date_fromdate.Date, date_enddate.Date, selectedCategory, selectedMachineID));
                }
                else if (picker_reportName.SelectedItem.ToString() == "A%")
                {
                    Navigation.PushAsync(new YCApercentReport(date_fromdate.Date, date_enddate.Date));
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-ReportSearch", ex.Message.ToString(), "Ok");
            }
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
                DisplayAlert("Notice-ReportSearch", ex.Message.ToString(), "Ok");
            }
        }

        private void picker_machinecategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    string selectedCategory = picker_machinecategory.SelectedItem.ToString();
                    conn.CreateTable<MachineModel>();
                    List<MachineModel> machines = conn.Table<MachineModel>().Where(
                        MachineModel => MachineModel.machineCategory == selectedCategory).ToList();
                    picker_machinename.ItemsSource = machines;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-ReportSearch", ex.Message.ToString(), "Ok");
            }
        }
    }
}