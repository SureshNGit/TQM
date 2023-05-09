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
            getUserfieldConfig();
        }

        private void getUserfieldConfig()
        {
            YarnCountConfigModel ycConfig_uf = null;
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                ycConfig_uf = conn.Table<YarnCountConfigModel>().
                            Where(YarnCountConfigModel => (YarnCountConfigModel.uf_name_1 != null ||
                            YarnCountConfigModel.uf_name_1 != "")).FirstOrDefault();
            }
            if (ycConfig_uf != null)
            {
                if (ycConfig_uf.uf_name_1 != null)
                {
                    lbl_userfield1.IsVisible = true;
                    lbl_userfield1.Text = ycConfig_uf.uf_name_1;
                    entry_userfield1.IsVisible = true;
                }
                if (ycConfig_uf.uf_name_2 != null)
                {
                    lbl_userfield2.IsVisible = true;
                    lbl_userfield2.Text = ycConfig_uf.uf_name_2;
                    entry_userfield2.IsVisible = true;
                }
                if (ycConfig_uf.uf_name_3 != null)
                {
                    lbl_userfield3.IsVisible = true;
                    lbl_userfield3.Text = ycConfig_uf.uf_name_3;
                    entry_userfield3.IsVisible = true;
                }
                if (ycConfig_uf.uf_name_4 != null)
                {
                    lbl_userfield4.IsVisible = true;
                    lbl_userfield4.Text = ycConfig_uf.uf_name_4;
                    entry_userfield4.IsVisible = true;
                }
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                reset();
                bool isAdmin = false;
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<UserModel>();
                    UserModel userinfo = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                    if (userinfo != null)
                    {

                        if (userinfo.isAdmin) { isAdmin = true; }
                    }
                }
                if (isAdmin)
                {
                    btn_deleteRecords.IsVisible = true;
                }
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
                string testID = "";
                if (entry_testID.Text.Trim() != "")
                {
                    if (entry_testID.Text.Contains("."))
                    {
                        DisplayAlert("Attention", "Test ID should not be decimal", "OK");
                        return;
                    }
                    else
                    {
                        testID = entry_testID.Text.Trim();
                    }
                }
                string standHank = "";
                if (entry_standHank.Text.Trim() != "")
                {
                    if (entry_standHank.Text.Contains("-"))
                    {
                        DisplayAlert("Attention", "Standard Hank should not be decimal", "OK");
                        return;
                    }
                    else
                    {
                        standHank = entry_standHank.Text.Trim();
                    }
                }
                string selectedCategory = null;
                string shift = "";
                if (picker_shift.SelectedItem != null)
                {
                    shift = picker_shift.SelectedItem.ToString();
                }
                string process = null;
                if (picker_process.SelectedItem != null)
                {
                    process = picker_process.SelectedItem.ToString();
                }
                string reportType = null;
                if (picker_reportType.SelectedItem != null)
                {
                    reportType = picker_reportType.SelectedItem.ToString();
                }
                bool is_consolidated = false;
                if (reportType == "Consolidated") { is_consolidated = true; }
                if (picker_machinecategory.SelectedItem != null) { selectedCategory = picker_machinecategory.SelectedItem.ToString(); };
                if (picker_reportName.SelectedItem.ToString() == "Wrapping")
                {
                    if (is_consolidated)
                    {
                        //if ((selectedCategory == null || selectedMachineID == Guid.Empty) && standHank == "")
                        if (selectedCategory == null || selectedCategory == "")
                        {
                            DisplayAlert("Attention", "Please select machine category for consolidated report", "OK");
                            return;
                        }
                    }
                    Navigation.PushAsync(new YCReport
                        (date_fromdate.Date, date_enddate.Date, selectedCategory, selectedMachineID, shift, process, testID, standHank, false, is_consolidated));
                }
                else if (picker_reportName.SelectedItem.ToString() == "A%")
                {
                    Navigation.PushAsync(new YCApercentReport
                        (date_fromdate.Date, date_enddate.Date, selectedCategory, selectedMachineID, shift, process, testID, false));
                }
                else if (picker_reportName.SelectedItem.ToString() == "Stretch")
                {
                    Navigation.PushAsync(new StretchReport
                        (date_fromdate.Date, date_enddate.Date, selectedCategory, selectedMachineID, shift, process, testID, false));
                }
                else if (picker_reportName.SelectedItem.ToString() == "Noils")
                {
                    Navigation.PushAsync(new NoilsReport
                        (date_fromdate.Date, date_enddate.Date, selectedCategory, selectedMachineID, shift, process, testID, false));
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
                if (picker_machinecategory.SelectedItem == null) { return; }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    string selectedCategory = picker_machinecategory.SelectedItem.ToString();
                    conn.CreateTable<MachineModel>();
                    List<MachineModel> machines = conn.Table<MachineModel>().Where(
                        MachineModel => MachineModel.machineCategory == selectedCategory).ToList();
                    picker_machinename.ItemsSource = machines;
                    if (selectedCategory == "Spinning" || selectedCategory == "Winding")
                    {
                        lbl_stadHank.Text = "Std. Count:";
                    }
                    else
                    {
                        lbl_stadHank.Text = "Std. Hank:";
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-ReportSearch", ex.Message.ToString(), "Ok");
            }
        }

        private void btn_deleteRecords_Clicked(object sender, EventArgs e)
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
                string testID = "";
                if (entry_testID.Text.Trim() != "")
                {
                    if (entry_testID.Text.Contains("."))
                    {
                        DisplayAlert("Attention", "Test ID should not be decimal", "OK");
                        return;
                    }
                    else
                    {
                        testID = entry_testID.Text.Trim();
                    }
                }
                string standHank = "";
                if (entry_standHank.Text.Trim() != "")
                {
                    if (entry_standHank.Text.Contains("-"))
                    {
                        DisplayAlert("Attention", "Standard Hank should not be decimal", "OK");
                        return;
                    }
                    else
                    {
                        standHank = entry_standHank.Text.Trim();
                    }
                }
                string selectedCategory = null;
                string shift = "";
                if (picker_shift.SelectedItem != null)
                {
                    shift = picker_shift.SelectedItem.ToString();
                }
                string process = null;
                if (picker_process.SelectedItem != null)
                {
                    process = picker_process.SelectedItem.ToString();
                }
                string reportType = null;
                if (picker_reportType.SelectedItem != null)
                {
                    reportType = picker_reportType.SelectedItem.ToString();
                }
                bool is_consolidated = false;
                if (reportType == "Consolidated") { is_consolidated = true; }
                if (picker_machinecategory.SelectedItem != null) { selectedCategory = picker_machinecategory.SelectedItem.ToString(); };
                if (picker_reportName.SelectedItem.ToString() == "Wrapping")
                {
                    if (is_consolidated)
                    {
                        //if ((selectedCategory == null || selectedMachineID == Guid.Empty) && standHank == "")
                        if (selectedCategory == null || selectedCategory == "")
                        {
                            DisplayAlert("Attention", "Please select machine category for consolidated report", "OK");
                            return;
                        }
                    }
                    Navigation.PushAsync(new YCReport
                        (date_fromdate.Date, date_enddate.Date, selectedCategory, selectedMachineID, shift, process, testID, standHank, true, is_consolidated));
                }
                else if (picker_reportName.SelectedItem.ToString() == "A%")
                {
                    Navigation.PushAsync(new YCApercentReport
                        (date_fromdate.Date, date_enddate.Date, selectedCategory, selectedMachineID, shift, process, testID, true));
                }
                else if (picker_reportName.SelectedItem.ToString() == "Stretch")
                {
                    Navigation.PushAsync(new StretchReport
                        (date_fromdate.Date, date_enddate.Date, selectedCategory, selectedMachineID, shift, process, testID, true));
                }
                else if (picker_reportName.SelectedItem.ToString() == "Noils")
                {
                    Navigation.PushAsync(new NoilsReport
                        (date_fromdate.Date, date_enddate.Date, selectedCategory, selectedMachineID, shift, process, testID, true));
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-ReportSearch", ex.Message.ToString(), "Ok");
            }
        }

        private void picker_reportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedReportType = picker_reportType.SelectedItem.ToString();
                if (selectedReportType == "Consolidated")
                {
                    var lst_reportName = new List<string>();
                    lst_reportName.Add("");
                    lst_reportName.Add("Wrapping");
                    picker_reportName.ItemsSource = lst_reportName;
                    picker_reportName.SelectedItem = "Wrapping";
                    lbl_testID.IsVisible = false;
                    entry_testID.IsVisible = false;
                    btn_deleteRecords.IsVisible = false;
                    picker_reportName.IsEnabled = false;
                    //lbl_stadHank.IsVisible = true;
                    //entry_standHank.IsVisible = true;
                }
                else
                {
                    var lst_reportName = new List<string>();
                    lst_reportName.Add("");
                    lst_reportName.Add("Wrapping");
                    lst_reportName.Add("A%");
                    lst_reportName.Add("Stretch");
                    lst_reportName.Add("Noils");
                    picker_reportName.ItemsSource = lst_reportName;
                    lbl_testID.IsVisible = true;
                    entry_testID.IsVisible = true;
                    btn_deleteRecords.IsVisible = true;
                    picker_reportName.IsEnabled = true;
                    //lbl_stadHank.IsVisible = false;
                    //entry_standHank.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-ReportType", ex.Message.ToString(), "Ok");
            }

        }

        private void picker_reportName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (picker_reportName.SelectedItem == null) { return; }
                string selectedReportName = picker_reportName.SelectedItem.ToString();
                if (selectedReportName == "Wrapping")
                {
                    var lst_macCategory = new List<string>();
                    lst_macCategory.Add("");
                    lst_macCategory.Add("Carding");
                    lst_macCategory.Add("Breaker Drawing");
                    lst_macCategory.Add("Comber");
                    lst_macCategory.Add("Drawing");
                    lst_macCategory.Add("Simplex/SpeedFrame");
                    lst_macCategory.Add("Spinning");
                    lst_macCategory.Add("Winding");
                    picker_machinecategory.ItemsSource = lst_macCategory;
                }
                else if (selectedReportName == "A%")
                {
                    var lst_macCategory = new List<string>();
                    //lst_macCategory.Add("");
                    lst_macCategory.Add("Drawing");
                    picker_machinecategory.ItemsSource = lst_macCategory;
                    picker_machinecategory.SelectedIndex = 0;
                }
                else if (selectedReportName == "Stretch")
                {
                    var lst_macCategory = new List<string>();
                    //lst_macCategory.Add("");
                    lst_macCategory.Add("Simplex/SpeedFrame");
                    picker_machinecategory.ItemsSource = lst_macCategory;
                    picker_machinecategory.SelectedIndex = 0;
                }
                else if (selectedReportName == "Noils")
                {
                    var lst_macCategory = new List<string>();
                    //lst_macCategory.Add("");
                    lst_macCategory.Add("Comber");
                    picker_machinecategory.ItemsSource = lst_macCategory;
                    picker_machinecategory.SelectedIndex = 0;
                }
                else
                {
                    var lst_macCategory = new List<string>();
                    lst_macCategory.Add("");
                    picker_machinecategory.ItemsSource = lst_macCategory;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-ReportName", ex.Message.ToString(), "Ok");
            }
        }
    }
}