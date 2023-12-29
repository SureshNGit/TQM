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
        private Guid selectedCategoryID = Guid.Empty;
        private string selectedCategory = null;
        private Guid selectedMachineID = Guid.Empty;
        private string selectedMachineName = null;
        public Report()
        {
            InitializeComponent();
            getUserfieldConfig();
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {

                conn.CreateTable<CategoryModel>();

                List<CategoryModel> cm = conn.Table<CategoryModel>().ToList();
                picker_machinecategory.ItemsSource = cm;
            }
        }

        private void getUserfieldConfig()
        {
            ConfigModel ycConfig_uf = null;
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                ycConfig_uf = conn.Table<ConfigModel>().
                            Where(ConfigModel => (ConfigModel.uf_name_1 != null ||
                            ConfigModel.uf_name_1 != "")).FirstOrDefault();
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
                //picker_reportName.SelectedIndex = 0;
                selectedCategory = null;
                selectedCategoryID = Guid.Empty;
                selectedMachineID = Guid.Empty;
                selectedMachineName = null;
                date_fromdate.Date = DateTime.Now;
                date_enddate.Date = DateTime.Now;
                picker_machinecategory.SelectedIndex = -1;
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
                if (picker_reportType.SelectedIndex < 0)
                {
                    DisplayAlert("Attention", "Please select report type to proceed!!!", "OK");
                    return;
                }
                string drumNumber = "";
                string standardStrength = "";
                string testID = "";
                string reportType = null;
                string shift = "";
                string UFVAL1 = null;
                string UFVAL2 = null;
                string UFVAL3 = null;
                string UFVAL4 = null;
                if (entry_testID.Text.Trim() != "")
                {
                    if(picker_reportType.SelectedItem.ToString()!= "Detailed")
                    {
                        DisplayAlert("Attention", "Only Detailed Report is allowed with Test ID", "OK");
                        return;
                    }

                    if (entry_testID.Text.Contains("."))
                    {
                        DisplayAlert("Attention", "Test ID should not be decimal", "OK");
                        return;
                    }
                    else
                    {
                        testID = entry_testID.Text.Trim();
                    }
                    if (entry_testID.Text.Contains("."))
                    {
                        long startTestID = long.Parse(testID.Split('.')[0]);
                        long endTestID = long.Parse(testID.Split('.')[1]);
                        if (startTestID > endTestID)
                        {
                            DisplayAlert("Notice", "Invalid. Start Test ID should be less than End Test ID!!!", "OK");
                            return;
                        }
                    }
                }
                else
                {
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
                    if (picker_machinecategory.SelectedIndex < 0)
                    {
                        DisplayAlert("Attention", "Please select machine category to proceed!!!", "OK");
                        return;
                    }
                    
                    if (picker_drumNumber.SelectedItem != null)
                    {
                        if (picker_drumNumber.SelectedItem.ToString().Trim() != "")
                        {
                            if (picker_drumNumber.SelectedItem.ToString().Contains("."))
                            {
                                DisplayAlert("Attention", "Drum number should not be decimal", "OK");
                                return;
                            }
                            else
                            {
                                drumNumber = picker_drumNumber.SelectedItem.ToString().Trim();
                            }
                        }
                    }

                    

                    if (entry_stdStrength.Text.Trim() != "")
                    {
                        if (entry_stdStrength.Text.Trim().Contains("-"))
                        {
                            DisplayAlert("Attention", "Standard Strength should not be a negative value!!!", "Ok");
                            return;
                        }
                        if (entry_stdStrength.Text.Trim() == "" || decimal.Parse(entry_stdStrength.Text.Trim()) == 0)
                        {
                            DisplayAlert("Attention", "Standard Strength should not be blank or zero!!!", "Ok");
                            return;
                        }
                        standardStrength = entry_stdStrength.Text.Trim();
                    }

                    if (selectedCategory == null || selectedCategory == "" || selectedCategoryID==Guid.Empty)
                    {
                        DisplayAlert("Attention", "Please select machine category for consolidated report", "OK");
                        return;
                    }

                    if (picker_machinename.SelectedIndex < 0)
                    {
                        DisplayAlert("Attention", "Please select machine name to proceed!!!", "OK");
                        return;
                    }

                    
                    if (picker_shift.SelectedItem != null)
                    {
                        shift = picker_shift.SelectedItem.ToString();
                    }
                    
                    if (picker_reportType.SelectedItem != null)
                    {
                        reportType = picker_reportType.SelectedItem.ToString();
                    }
                    
                    if (lbl_userfield1.IsVisible)
                    {
                        if (entry_userfield1.Text.Trim() != "")
                        {
                            UFVAL1 = entry_userfield1.Text.Trim();
                        }
                    }
                    if (lbl_userfield2.IsVisible)
                    {
                        if (entry_userfield2.Text.Trim() != "")
                        {
                            UFVAL2 = entry_userfield2.Text.Trim();
                        }
                    }
                    if (lbl_userfield3.IsVisible)
                    {
                        if (entry_userfield3.Text.Trim() != "")
                        {
                            UFVAL3 = entry_userfield3.Text.Trim();
                        }
                    }
                    if (lbl_userfield4.IsVisible)
                    {
                        if (entry_userfield4.Text.Trim() != "")
                        {
                            UFVAL4 = entry_userfield4.Text.Trim();
                        }
                    }
                }

                

                
                bool is_consolidated = false;
                if (reportType == "Consolidated") { is_consolidated = true; }
                bool drumView = false;
                if (reportType == "Drum View") { drumView = true; }
                bool drumDetails = false;
                if (reportType == "Drum Details") { drumDetails = true; }
                bool is_maintenance = false;
                if(reportType == "Maintenance") { is_maintenance = true; }
                bool is_indBreifView = false;
                if(reportType == "Individual Test - Brief View") { is_indBreifView = true; }
                

                Navigation.PushAsync(new YCReport
                    (date_fromdate.Date, date_enddate.Date, selectedCategoryID, selectedCategory, selectedMachineID, shift, testID, drumNumber, standardStrength , false, is_indBreifView, is_consolidated, drumView, drumDetails, is_maintenance, UFVAL1, UFVAL2, UFVAL3, UFVAL4));
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
                picker_drumNumber.SelectedIndex = -1;
                picker_drumNumber.Items.Clear();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<ConfigModel>();
                    ConfigModel yarncountconfigmodel = conn.Table<ConfigModel>().Where(ConfigModel =>
                                                                (ConfigModel.categoryID == selectedCategoryID &&
                                                                ConfigModel.machineID == selectedMachineID &&
                                                                ConfigModel.machineName == selectedMachineName)).FirstOrDefault();
                    if (yarncountconfigmodel != null)
                    {
                        List<string> drums = new List<string>();
                        int totalDrums = yarncountconfigmodel.totalDrumCount;
                        for (int d = 0; d < totalDrums; d++)
                        {
                            picker_drumNumber.Items.Add((d + 1).ToString());
                        }

                    }
                }
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
                picker_drumNumber.SelectedIndex = -1;
                picker_drumNumber.Items.Clear();
                if (picker_machinecategory.SelectedItem == null) { return; }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {

                    List<CategoryModel> source = (List<CategoryModel>)picker_machinecategory.ItemsSource;
                    if (picker_machinecategory.SelectedIndex < 0)
                    {
                        return;
                    }
                    selectedCategoryID = (Guid)source[picker_machinecategory.SelectedIndex].ID;
                    CategoryModel selectedMachine = (CategoryModel)picker_machinecategory.SelectedItem;
                    selectedCategory = selectedMachine.category;
                    conn.CreateTable<MachineModel>();
                    List<MachineModel> machines = conn.Table<MachineModel>().Where(
                        MachineModel => MachineModel.categoryID == selectedCategoryID).ToList();
                    picker_machinename.ItemsSource = machines;
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
                bool is_consolidated = false;
                bool drumDetails = false;
                bool is_maintenance = false;
                bool drumView = false;
                bool is_indBreifView = false;

                if (picker_reportType.SelectedIndex < 0)
                {
                    DisplayAlert("Attention", "Please select report type to proceed!!!", "OK");
                    return;
                }
                if (picker_reportType.SelectedItem.ToString() != "Detailed")
                {
                    DisplayAlert("Attention", "Please select report type as 'Detailed Report' for record deletion", "OK");
                    return;
                }
                string drumNumber = "";
                string standardStrength = "";
                string testID = "";
                string reportType = null;
                string shift = "";
                string UFVAL1 = null;
                string UFVAL2 = null;
                string UFVAL3 = null;
                string UFVAL4 = null;
                if (entry_testID.Text.Trim() != "")
                {
                    if (picker_reportType.SelectedItem.ToString() != "Detailed")
                    {
                        DisplayAlert("Attention", "Only Detailed Report is allowed with Test ID", "OK");
                        return;
                    }

                    if (entry_testID.Text.Contains("."))
                    {
                        DisplayAlert("Attention", "Test ID should not be decimal", "OK");
                        return;
                    }
                    else
                    {
                        testID = entry_testID.Text.Trim();
                    }
                    if (entry_testID.Text.Contains("."))
                    {
                        long startTestID = long.Parse(testID.Split('.')[0]);
                        long endTestID = long.Parse(testID.Split('.')[1]);
                        if (startTestID > endTestID)
                        {
                            DisplayAlert("Notice", "Invalid. Start Test ID should be less than End Test ID!!!", "OK");
                            return;
                        }
                    }
                }
                else
                {
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
                    if (picker_machinecategory.SelectedIndex < 0)
                    {
                        DisplayAlert("Attention", "Please select machine category to proceed!!!", "OK");
                        return;
                    }

                    if (picker_drumNumber.SelectedItem != null)
                    {
                        if (picker_drumNumber.SelectedItem.ToString().Trim() != "")
                        {
                            if (picker_drumNumber.SelectedItem.ToString().Contains("."))
                            {
                                DisplayAlert("Attention", "Drum number should not be decimal", "OK");
                                return;
                            }
                            else
                            {
                                drumNumber = picker_drumNumber.SelectedItem.ToString().Trim();
                            }
                        }
                    }



                    if (entry_stdStrength.Text.Trim() != "")
                    {
                        if (entry_stdStrength.Text.Trim().Contains("-"))
                        {
                            DisplayAlert("Attention", "Standard Strength should not be a negative value!!!", "Ok");
                            return;
                        }
                        if (entry_stdStrength.Text.Trim() == "" || decimal.Parse(entry_stdStrength.Text.Trim()) == 0)
                        {
                            DisplayAlert("Attention", "Standard Strength should not be blank or zero!!!", "Ok");
                            return;
                        }
                        standardStrength = entry_stdStrength.Text.Trim();
                    }

                    if (selectedCategory == null || selectedCategory == "" || selectedCategoryID==Guid.Empty)
                    {
                        DisplayAlert("Attention", "Please select machine category for consolidated report", "OK");
                        return;
                    }

                    if (picker_machinename.SelectedIndex < 0)
                    {
                        DisplayAlert("Attention", "Please select machine name to proceed!!!", "OK");
                        return;
                    }


                    if (picker_shift.SelectedItem != null)
                    {
                        shift = picker_shift.SelectedItem.ToString();
                    }

                    if (picker_reportType.SelectedItem != null)
                    {
                        reportType = picker_reportType.SelectedItem.ToString();
                    }

                    if (lbl_userfield1.IsVisible)
                    {
                        if (entry_userfield1.Text.Trim() != "")
                        {
                            UFVAL1 = entry_userfield1.Text.Trim();
                        }
                    }
                    if (lbl_userfield2.IsVisible)
                    {
                        if (entry_userfield2.Text.Trim() != "")
                        {
                            UFVAL2 = entry_userfield2.Text.Trim();
                        }
                    }
                    if (lbl_userfield3.IsVisible)
                    {
                        if (entry_userfield3.Text.Trim() != "")
                        {
                            UFVAL3 = entry_userfield3.Text.Trim();
                        }
                    }
                    if (lbl_userfield4.IsVisible)
                    {
                        if (entry_userfield4.Text.Trim() != "")
                        {
                            UFVAL4 = entry_userfield4.Text.Trim();
                        }
                    }
                }

               Navigation.PushAsync(new YCReport
                    (date_fromdate.Date, date_enddate.Date,selectedCategoryID, selectedCategory, selectedMachineID, shift, testID, drumNumber, standardStrength, true, is_indBreifView, is_consolidated, drumView, drumDetails, is_maintenance, UFVAL1, UFVAL2, UFVAL3, UFVAL4));
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
                if (selectedReportType != "Detailed")
                {
                    lbl_testID.IsVisible = false;
                    entry_testID.IsVisible = false;
                    btn_deleteRecords.IsVisible = false;
                }
                else
                {
                    lbl_testID.IsVisible = true;
                    entry_testID.IsVisible = true;
                    btn_deleteRecords.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-ReportType", ex.Message.ToString(), "Ok");
            }

        }
    }
}