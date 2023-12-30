using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using TQM.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class YCSettings : ContentPage
    {
        private static readonly DateTime DEFAULTDATE = new DateTime(2000, 01, 01);
        private Guid currentID = Guid.Empty;
        private Guid selectedCategoryID = Guid.Empty;
        private string selectedMachineCategory = null;
        private Guid selectedMachineID = Guid.Empty;
        private string selectedMachineName = null;
        private DateTime selectedScheduledStartDate = DEFAULTDATE;
        private DateTime selectedScheduledEndDate = DEFAULTDATE;
        private bool is_ScheduledDateUpdateInTestRequired = false;
        private int currentShift = 0;
        private TimeSpan currentShift1 = TimeSpan.Zero;
        private TimeSpan currentShift2 = TimeSpan.Zero;
        private TimeSpan currentShift3 = TimeSpan.Zero;

        private DateTime currentCreatedDate = DEFAULTDATE;

        public YCSettings()
        {
            try
            {
                InitializeComponent();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    //conn.DropTable<ConfigModel>();
                    //conn.DropTable<TestConfigModel>();

                    conn.CreateTable<ConfigModel>();
                    conn.CreateTable<TestConfigModel>();

                    conn.CreateTable<CategoryModel>();
                    conn.CreateTable<MachineModel>();

                    List<CategoryModel> cm = conn.Table<CategoryModel>().ToList();
                    picker_machinecategory.ItemsSource = cm;
                }
               
                //picker_machinecategory.SelectedItem = "OE Auto Coner";

                fetchConfig();
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred: " + ex.Message.ToString(), "OK");
            }
        }

        private void toggleUserField()
        {
            ConfigModel ycConfig_uf = null;
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<ConfigModel>();
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
                    entry_userfield1.Text = "";
                }

                if (ycConfig_uf.uf_name_2 != null)
                {
                    lbl_userfield2.IsVisible = true;
                    lbl_userfield2.Text = ycConfig_uf.uf_name_2;
                    entry_userfield2.IsVisible = true;
                    entry_userfield2.Text = "";
                }

                if (ycConfig_uf.uf_name_3 != null)
                {
                    lbl_userfield3.IsVisible = true;
                    lbl_userfield3.Text = ycConfig_uf.uf_name_3;
                    entry_userfield3.IsVisible = true;
                    entry_userfield3.Text = "";
                }

                if (ycConfig_uf.uf_name_4 != null)
                {
                    lbl_userfield4.IsVisible = true;
                    lbl_userfield4.Text = ycConfig_uf.uf_name_4;
                    entry_userfield4.IsVisible = true;
                    entry_userfield4.Text = "";
                }
            }
        }

        private void fetchConfig()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<ConfigModel>();
                    List<ConfigModel> ycConfigList = conn.Table<ConfigModel>().ToList();
                    if (ycConfigList.Count > 0)
                    {

                        ConfigModel SettingWithMachine = ycConfigList.Where(ConfigModel =>
                                                (ConfigModel.machineCategory != null || ConfigModel.machineCategory != "")).FirstOrDefault();
                        if (SettingWithMachine == null)
                        {
                            populateSettingsField(ycConfigList[0]);
                        }
                        else
                        {
                            if (selectedMachineCategory != null && selectedMachineCategory != "" && selectedMachineName != null
                                && selectedMachineName != "" && selectedMachineID != Guid.Empty)
                            {
                                ConfigModel machineSetting = ycConfigList.Where(ConfigModel =>
                                                (ConfigModel.machineCategory == selectedMachineCategory &&
                                                ConfigModel.machineID == selectedMachineID &&
                                                ConfigModel.machineName == selectedMachineName)).FirstOrDefault();
                                if (machineSetting != null)
                                {
                                    populateSettingsField(machineSetting);
                                }
                            }
                        }
                    }
                    else
                    {
                        btn_save.Text = "Save";
                        btn_save.BackgroundColor = Color.Red;
                        btn_save.TextColor = Color.White;
                    }
                }
                toggleUserField();
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred: " + ex.Message.ToString(), "OK");
            }
        }

        private void populateSettingsField(ConfigModel ycConfig, bool shiftAlone = false, bool isMacDiff = false)
        {
            is_ScheduledDateUpdateInTestRequired = false;
            toggleUserField();
            if (ycConfig == null)
            {
                btn_save.Text = "Save";
                btn_save.BackgroundColor = Color.Red;
                btn_save.TextColor = Color.White;
                currentID = Guid.Empty;
                currentCreatedDate = DEFAULTDATE;
                reset();
                picker_shiftCount.SelectedIndex = 0;
                currentShift = 0;
                currentShift1 = TimeSpan.Zero;
                currentShift2 = TimeSpan.Zero;
                currentShift3 = TimeSpan.Zero;
                toggleShift();
                return;
            }

            if (ycConfig != null && shiftAlone == true)
            {
                btn_save.Text = "Save";
                btn_save.BackgroundColor = Color.Red;
                btn_save.TextColor = Color.White;
                currentID = Guid.Empty;
                currentCreatedDate = DEFAULTDATE;
                if (ycConfig.shiftCount > 0)
                {

                    IList<string> shiftCountList = picker_shiftCount.Items;
                    int shiftCountIndex = 0;
                    foreach (string count in shiftCountList)
                    {
                        if (count != ycConfig.shiftCount.ToString())
                        {
                            shiftCountIndex++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    picker_shiftCount.SelectedIndex = shiftCountIndex;
                    currentShift = ycConfig.shiftCount;
                    currentShift1 = TimeSpan.Zero; currentShift2 = TimeSpan.Zero; currentShift3 = TimeSpan.Zero;
                    toggleShift();
                }

                if (ycConfig.shift1time != null && ycConfig.shift1time != "")
                {
                    Shift1_timePicker.Time = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift1time).TotalHours);
                    currentShift1 = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift1time).TotalHours);
                }

                if (ycConfig.shift2time != null && ycConfig.shift2time != "")
                {
                    Shift2_timePicker.Time = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift2time).TotalHours);
                    currentShift2 = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift2time).TotalHours);
                }
                if (ycConfig.shift3time != null && ycConfig.shift3time != "")
                {
                    Shift3_timePicker.Time = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift3time).TotalHours);
                    currentShift3 = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift3time).TotalHours);
                }

                return;
            }

            if (isMacDiff == false)
            {
                btn_save.Text = "Update";
                btn_save.BackgroundColor = Color.FromHex("#0e0273");
                btn_save.TextColor = Color.White;
                currentID = ycConfig.ID;
                currentCreatedDate = ycConfig.createdate;
                //currentUpdatedDate = ycConfig.updateddate;
            }
            else
            {
                btn_save.Text = "Save";
                btn_save.BackgroundColor = Color.Red;
                btn_save.TextColor = Color.White;
                currentID = Guid.Empty;
                currentCreatedDate = DEFAULTDATE;
            }

            if (isMacDiff == false)
            {
                //picker_drumSection_Sec1.SelectedItem = "Scheduled";
                //picker_drumSection_Sec2.SelectedItem = "Scheduled";
                //picker_drumSection_Sec3.SelectedItem = "Scheduled";
                //picker_drumSection_Sec4.SelectedItem = "Scheduled";

                IList<string> machineCategorylist = picker_machinecategory.Items;
                int machineCatindex = 0;
                foreach (string mCat in machineCategorylist)
                {
                    if (mCat != ycConfig.machineCategory.ToString())
                    {
                        machineCatindex++;
                    }
                    else
                    {
                        selectedMachineCategory = ycConfig.machineCategory.ToString();
                        break;
                    }
                }
                picker_machinecategory.SelectedIndex = machineCatindex;
                if (machineCatindex != 0) { selectedMachineCategory = machineCategorylist[machineCatindex]; }

                IList<string> machinelist = picker_machinename.Items;
                int machineindex = 0;
                foreach (string m in machinelist)
                {
                    if (m != ycConfig.machineName.ToString())
                    {
                        machineindex++;
                    }
                    else
                    {
                        selectedMachineID = ycConfig.machineID;
                        selectedMachineName = ycConfig.machineName.ToString();
                        break;
                    }
                }
                picker_machinename.SelectedIndex = machineindex;
            }
            //General Data
            entry_drumCount.Text = ycConfig.totalDrumCount.ToString();
            IList<string> sectionlist = picker_sectionCount.Items;
            int sectionindex = 0;
            foreach (string s in sectionlist)
            {
                if (s != ycConfig.totalSections.ToString())
                {
                    sectionindex++;
                }
                else
                {
                    break;
                }
            }
            picker_sectionCount.SelectedIndex = sectionindex;
            entry_macSpeed.Text = ycConfig.speed.ToString();
            entry_p1.Text = ycConfig.p1.ToString();
            entry_p1Deviation.Text = ycConfig.p1Deviation.ToString();
            entry_p2.Text = ycConfig.p2.ToString();
            entry_p2Deviation.Text = ycConfig.p2Deviation.ToString();
            entry_n1.Text = ycConfig.n1.ToString();
            entry_n1Deviation.Text = ycConfig.n1Deviation.ToString();
            entry_stdRollingStrength.Text = ycConfig.stdRollingStrength.ToString();
            entry_strengthDeviation.Text = ycConfig.strengthDeviation.ToString();
            entry_MinLimit.Text = ycConfig.belowLimit.ToString();
            entry_MaxLimit.Text = ycConfig.maxLimit.ToString();
            entry_totalTestCount.Text = ycConfig.totalSamples.ToString();
            entry_matCount.Text = ycConfig.materialCount;
            date_scheduledStartDate.Date = ycConfig.scheduledStartDate;
            selectedScheduledStartDate = ycConfig.scheduledStartDate;
            date_scheduledEndDate.Date = ycConfig.scheduledEndDate;
            selectedScheduledEndDate = ycConfig.scheduledEndDate;



            //Section-1

            if (ycConfig.drumNumbers_s1 != null)
            {
                entry_Drums_from_s1.Text = ycConfig.drumNumbers_s1.ToString().Split('.')[0];
                entry_Drums_to_s1.Text = ycConfig.drumNumbers_s1.ToString().Split('.')[1];
            }

            //Section-2
            if (ycConfig.drumNumbers_s2 != null)
            {
                entry_Drums_from_s2.Text = ycConfig.drumNumbers_s2.ToString().Split('.')[0];
                entry_Drums_to_s2.Text = ycConfig.drumNumbers_s2.ToString().Split('.')[1];
            }

            //Section-3
            if (ycConfig.drumNumbers_s3 != null)
            {
                entry_Drums_from_s3.Text = ycConfig.drumNumbers_s3.ToString().Split('.')[0];
                entry_Drums_to_s3.Text = ycConfig.drumNumbers_s3.ToString().Split('.')[1];
            }

            //Section-4
            if (ycConfig.drumNumbers_s4 != null)
            {
                entry_Drums_from_s4.Text = ycConfig.drumNumbers_s4.ToString().Split('.')[0];
                entry_Drums_to_s4.Text = ycConfig.drumNumbers_s4.ToString().Split('.')[1];
            }




            //Toggle Frames
            if (btn_section1.IsVisible)
            {
                frame_sec1.IsVisible = false;
                toggleSectionFrames(frame_sec1, btn_section1);
                //toggleSectionFrames(frame_sec1, btn_section1, date_scheduledStartDate_Sec1,date_scheduledEndDate_Sec1, entry_scheduledDayLimit_Sec1, picker_drumSection_Sec1);
                if (btn_section2.IsVisible)
                {
                    frame_sec2.IsVisible = false;
                    toggleSectionFrames(frame_sec2, btn_section2);
                    //toggleSectionFrames(frame_sec2, btn_section2, date_scheduledStartDate_Sec2, date_scheduledEndDate_Sec2, entry_scheduledDayLimit_Sec2, picker_drumSection_Sec2);
                    if (btn_section3.IsVisible)
                    {
                        frame_sec3.IsVisible = false;
                        toggleSectionFrames(frame_sec3, btn_section3);
                        //toggleSectionFrames(frame_sec3, btn_section3, date_scheduledStartDate_Sec3, date_scheduledEndDate_Sec3, entry_scheduledDayLimit_Sec3, picker_drumSection_Sec3);
                    }
                    if (btn_section4.IsVisible)
                    {
                        frame_sec4.IsVisible = false;
                        toggleSectionFrames(frame_sec4, btn_section4);
                        //toggleSectionFrames(frame_sec4, btn_section4, date_scheduledStartDate_Sec4, date_scheduledEndDate_Sec4, entry_scheduledDayLimit_Sec4, picker_drumSection_Sec4);
                    }
                }
            }


            //Shift Data
            if (ycConfig.shiftCount > 0)
            {

                IList<string> shiftCountList = picker_shiftCount.Items;
                int shiftCountIndex = 0;
                foreach (string count in shiftCountList)
                {
                    if (count != ycConfig.shiftCount.ToString())
                    {
                        shiftCountIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
                picker_shiftCount.SelectedIndex = shiftCountIndex;
                currentShift = ycConfig.shiftCount;
                currentShift1 = TimeSpan.Zero; currentShift2 = TimeSpan.Zero; currentShift3 = TimeSpan.Zero;
                toggleShift();
            }



            if (ycConfig.shift1time != null && ycConfig.shift1time != "")
            {
                Shift1_timePicker.Time = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift1time).TotalHours);
                currentShift1 = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift1time).TotalHours);
            }

            if (ycConfig.shift2time != null && ycConfig.shift2time != "")
            {
                Shift2_timePicker.Time = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift2time).TotalHours);
                currentShift2 = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift2time).TotalHours);
            }
            if (ycConfig.shift3time != null && ycConfig.shift3time != "")
            {
                Shift3_timePicker.Time = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift3time).TotalHours);
                currentShift3 = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift3time).TotalHours);
            }


            if (ycConfig.uf_name_1 != null)
            {
                lbl_userfield1.IsVisible = true;
                lbl_userfield1.Text = ycConfig.uf_name_1;
                entry_userfield1.IsVisible = true;
                entry_userfield1.Text = ycConfig.uf_value_1;
            }

            if (ycConfig.uf_name_2 != null)
            {
                lbl_userfield2.IsVisible = true;
                lbl_userfield2.Text = ycConfig.uf_name_2;
                entry_userfield2.IsVisible = true;
                entry_userfield2.Text = ycConfig.uf_value_2;
            }

            if (ycConfig.uf_name_3 != null)
            {
                lbl_userfield3.IsVisible = true;
                lbl_userfield3.Text = ycConfig.uf_name_3;
                entry_userfield3.IsVisible = true;
                entry_userfield3.Text = ycConfig.uf_value_3;
            }

            if (ycConfig.uf_name_4 != null)
            {
                lbl_userfield4.IsVisible = true;
                lbl_userfield4.Text = ycConfig.uf_name_4;
                entry_userfield4.IsVisible = true;
                entry_userfield4.Text = ycConfig.uf_value_4;
            }

        }

        private bool checkScheduleDateChange()
        {
            bool ret = true;
            if (date_scheduledStartDate.Date > date_scheduledEndDate.Date)
            {
                DisplayAlert("Attention", "Scheduled Start date should be less than end date!!!", "Ok");
                return false;
            }
            
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                
                //Check if there is any entry for the given scheduled start and end date
                DateTime sch_startDate = Convert.ToDateTime(selectedScheduledStartDate.Date);
                DateTime sch_endDate = Convert.ToDateTime(selectedScheduledEndDate.Date);
                List<StrengthTestSummaryModel> sts = conn.Table<StrengthTestSummaryModel>().Where(StrengthTestSummaryModel =>
                                                        (StrengthTestSummaryModel.machineCategory==selectedMachineCategory
                                                        && StrengthTestSummaryModel.machineID==selectedMachineID
                                                        && StrengthTestSummaryModel.scheduledStartDate== sch_startDate
                                                        && StrengthTestSummaryModel.scheduledEndDate==sch_endDate)).ToList();

                if (sts.Count > 0)
                {
                    int overallDrumCount = 0;
                    int.TryParse(entry_drumCount.Text, out overallDrumCount);

                    if (overallDrumCount != sts[0].overallDrumCount)
                    {
                        //Check if the previous schedule date is expired
                        if ((selectedScheduledEndDate.Date < DateTime.Today.Date) == false)
                        {
                            DisplayAlert("Attention", "Test already started for the scheduled period ("
                                                + selectedScheduledStartDate.Date.ToShortDateString()
                                                + " - "
                                                + selectedScheduledEndDate.Date.ToShortDateString()
                                                + "), hence total drum count cannot be changed until scheduled date is expired", "OK");
                            ret = false;
                        }
                    }

                    int noOfSections = 0;
                    int.TryParse(picker_sectionCount.SelectedItem.ToString(), out noOfSections);

                    if (noOfSections == 0)
                    {
                        DisplayAlert("Attention", "Invalid number of sections selected!!!", "Ok");
                        return false;
                    }
                    else
                    {
                        if (noOfSections != sts[0].overallSections)
                        {
                            //Check if the previous schedule date is expired
                            if ((selectedScheduledEndDate.Date < DateTime.Today.Date)==false)
                            {
                                DisplayAlert("Attention", "Test already started for the scheduled period ("
                                                    + selectedScheduledStartDate.Date.ToShortDateString()
                                                    + " - "
                                                    + selectedScheduledEndDate.Date.ToShortDateString()
                                                    + "), hence number of sections cannot be changed until scheduled date is expired", "OK");
                                ret = false;
                            }
                        }
                    }

                    if(selectedScheduledStartDate.Date != date_scheduledStartDate.Date)
                    {
                        //Check if the previous schedule date is expired
                        if (selectedScheduledEndDate.Date < DateTime.Today.Date)
                        {
                            if (date_scheduledStartDate.Date <= selectedScheduledEndDate.Date)
                            {
                                DisplayAlert("Attention", "The new scheduled date should not be within the than previous scheduled period ("
                                                    + selectedScheduledStartDate.Date.ToShortDateString()
                                                    + " - "
                                                    + selectedScheduledEndDate.Date.ToShortDateString()
                                                    + "), hence start date cannot be changed", "OK");
                                ret = false;
                            }
                        }

                        //DisplayAlert("Attention", "Test started for the scheduled date ("
                        //                            + selectedScheduledStartDate.Date.ToShortDateString()
                        //                            + " - "
                        //                            + selectedScheduledEndDate.Date.ToShortDateString()
                        //                            +"), hence start date cannot be changed", "OK");
                        //ret= false;
                    }
                    
                    if (selectedScheduledEndDate.Date != date_scheduledEndDate.Date)
                    {

                        //check if the new scheduled end date is less than the test taken date
                        List<StrengthTestSummaryModel> sts_1 = conn.Table<StrengthTestSummaryModel>()
                                                                .Where(StrengthTestSummaryModel =>
                                                                (StrengthTestSummaryModel.machineCategory == selectedMachineCategory
                                                                && StrengthTestSummaryModel.machineID == selectedMachineID
                                                                && StrengthTestSummaryModel.scheduledStartDate == sch_startDate))
                                                                .OrderByDescending(
                                                                StrengthTestSummaryModel=>StrengthTestSummaryModel.createdate).ToList();
                        if (sts_1.Count > 0)
                        {
                            if (sts_1[0].createdate.Date > date_scheduledEndDate.Date)
                            {
                                DisplayAlert("Attention", "Test started for the scheduled date ("
                                                + selectedScheduledStartDate.Date.ToShortDateString()
                                                + " - "
                                                + selectedScheduledEndDate.Date.ToShortDateString()
                                                + "), hence end date cannot be changed to less than last test taken date", "OK");
                                ret = false;
                            }
                            else
                            {
                                is_ScheduledDateUpdateInTestRequired = true;
                            }
                        }
                    }
                }
            }
            return ret;
        }

        private void btn_save_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (entry_drumCount.Text.Trim().Contains(".") || entry_drumCount.Text.Trim().Contains("-"))
                {
                    DisplayAlert("Attention", "Total Drum Count should not be a decimal or negative value!!!", "Ok");
                    return;
                }
                if (entry_drumCount.Text.Trim() == "" || int.Parse(entry_drumCount.Text.Trim()) == 0)
                {
                    DisplayAlert("Attention", "Total Drum Count should not be blank or zero!!!", "Ok");
                    return;
                }
                if (picker_sectionCount.SelectedIndex == -1 || picker_sectionCount.SelectedItem.ToString() == "")
                {
                    DisplayAlert("Attention", "Please select valid No.Of Sections!!!", "OK");
                    return;
                }
                if (!checkScheduleDateChange()) { return; }
                if (selectedMachineCategory == null || selectedMachineCategory == "")
                {
                    DisplayAlert("Attention", "Please select machine category to proceed!!!", "OK");
                    return;
                }

                if (selectedMachineName == null || selectedMachineName == "" || selectedMachineID == Guid.Empty)
                {
                    DisplayAlert("Attention", "Please select machine name to proceed!!!", "OK");
                    return;
                }
                if (entry_macSpeed.Text.Trim().Contains(".") || entry_macSpeed.Text.Trim().Contains("-"))
                {
                    DisplayAlert("Attention", "Machine speed should not be a decimal or negative value!!!", "Ok");
                    return;
                }
                if (entry_macSpeed.Text.Trim() == "" || int.Parse(entry_macSpeed.Text.Trim()) == 0)
                {
                    DisplayAlert("Attention", "Machine speed should not be blank or zero!!!", "Ok");
                    return;
                }
                if (entry_p1.Text.Trim() == "-")
                {
                    DisplayAlert("Attention", "P1 is invalid. Please check!!!", "Ok");
                    return;
                }
                if (entry_p1.Text.Trim() == "" || decimal.Parse(entry_p1.Text.Trim()) <= 0m)
                {
                    DisplayAlert("Attention", "P1 should not be blank or zero or negative!!!", "Ok");
                    return;
                }
                if (entry_p1Deviation.Text.Trim() == "-")
                {
                    DisplayAlert("Attention", "P1 Deviation is invalid. Please check!!!", "Ok");
                    return;
                }
                if (entry_p1Deviation.Text.Trim() == "" || decimal.Parse(entry_p1Deviation.Text.Trim()) < 0m)
                {
                    DisplayAlert("Attention", "P1 Deviation should not be blank or negative!!!", "Ok");
                    return;
                }
                if (entry_p2.Text.Trim() == "-")
                {
                    DisplayAlert("Attention", "P2 is invalid. Please check!!!", "Ok");
                    return;
                }
                if (entry_p2.Text.Trim() == "" || decimal.Parse(entry_p2.Text.Trim()) <= 0m)
                {
                    DisplayAlert("Attention", "P2 should not be blank or zero or negative!!!", "Ok");
                    return;
                }
                if (entry_p2Deviation.Text.Trim() == "-")
                {
                    DisplayAlert("Attention", "P2 Deviation is invalid. Please check!!!", "Ok");
                    return;
                }
                if (entry_p2Deviation.Text.Trim() == "" || decimal.Parse(entry_p2Deviation.Text.Trim()) < 0m)
                {
                    DisplayAlert("Attention", "P2 Deviation should not be blank or negative!!!", "Ok");
                    return;
                }
                if (entry_n1.Text.Trim() == "-")
                {
                    DisplayAlert("Attention", "N1 is invalid. Please check!!!", "Ok");
                    return;
                }
                if (entry_n1.Text.Trim() == "" || decimal.Parse(entry_n1.Text.Trim()) <= 0m)
                {
                    DisplayAlert("Attention", "N1 should not be blank or zero or negative!!!", "Ok");
                    return;
                }
                if (entry_n1Deviation.Text.Trim() == "-")
                {
                    DisplayAlert("Attention", "N1 Deviation is invalid. Please check!!!", "Ok");
                    return;
                }
                if (entry_n1Deviation.Text.Trim() == "" || decimal.Parse(entry_n1Deviation.Text.Trim()) < 0m)
                {
                    DisplayAlert("Attention", "N1 Deviation should not be blank or negative!!!", "Ok");
                    return;
                }
                if (entry_stdRollingStrength.Text.Trim() == "-")
                {
                    DisplayAlert("Attention", "Standard Rolling Strength is invalid. Please check!!!", "Ok");
                    return;
                }
                if (entry_stdRollingStrength.Text.Trim() == "" || decimal.Parse(entry_stdRollingStrength.Text.Trim()) <= 0m)
                {
                    DisplayAlert("Attention", "Standard Rolling Strength should not be blank or zero or negative!!!", "Ok");
                    return;
                }
                if (entry_strengthDeviation.Text.Trim() == "-")
                {
                    DisplayAlert("Attention", "Strength Deviation is invalid. Please check!!!", "Ok");
                    return;
                }
                if (entry_strengthDeviation.Text.Trim() == "" || decimal.Parse(entry_strengthDeviation.Text.Trim()) < 0m)
                {
                    DisplayAlert("Attention", "Strength Deviation should not be blank or negative!!!", "Ok");
                    return;
                }
                if (entry_MinLimit.Text.Trim().Contains(".") || entry_MinLimit.Text.Trim().Contains("-"))
                {
                    DisplayAlert("Attention", "Minimum Limit should not be a decimal or negative value!!!", "Ok");
                    return;
                }
                if (entry_MinLimit.Text.Trim() == "" || int.Parse(entry_MinLimit.Text.Trim()) == 0)
                {
                    DisplayAlert("Attention", "Minimum Limit should not be blank or zero!!!", "Ok");
                    return;
                }
                if (entry_MaxLimit.Text.Trim().Contains(".") || entry_MaxLimit.Text.Trim().Contains("-"))
                {
                    DisplayAlert("Attention", "Maximum Limit should not be a decimal or negative value!!!", "Ok");
                    return;
                }
                if (entry_MaxLimit.Text.Trim() == "" || int.Parse(entry_MaxLimit.Text.Trim()) == 0)
                {
                    DisplayAlert("Attention", "Maximum Limit should not be blank or zero!!!", "Ok");
                    return;
                }
                if (entry_totalTestCount.Text.Trim().Contains(".") || entry_totalTestCount.Text.Trim().Contains("-"))
                {
                    DisplayAlert("Attention", "Total test count should not be a decimal or negative value!!!", "Ok");
                    return;
                }
                if (entry_totalTestCount.Text.Trim() == "" || int.Parse(entry_totalTestCount.Text.Trim()) == 0)
                {
                    DisplayAlert("Attention", "Total test count should not be blank or zero!!!", "Ok");
                    return;
                }
               
                if (entry_matCount.Text.Trim() == "")
                {
                    DisplayAlert("Attention", "Material count is invalid. Please check!!!", "Ok");
                    return;
                }

                

                string drumNumbers_S1 = "0.0";
                if (btn_section1.IsVisible)
                {
                    if (entry_Drums_from_s1.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Drum Number (From) is invalid in section-1. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_Drums_from_s1.Text.Trim() == "" || decimal.Parse(entry_Drums_from_s1.Text.Trim()) < 0m)
                    {
                        DisplayAlert("Attention", "Drum Number (From) should not be blank or negative in section-1!!!", "Ok");
                        return;
                    }
                    if (entry_Drums_to_s1.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Drum Number (To) is invalid in section-1. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_Drums_to_s1.Text.Trim() == "" || decimal.Parse(entry_Drums_to_s1.Text.Trim()) < 0m)
                    {
                        DisplayAlert("Attention", "Drum Number (To) should not be blank or negative in section-1!!!", "Ok");
                        return;
                    }
                    drumNumbers_S1 = decimal.Parse(entry_Drums_from_s1.Text + "." + entry_Drums_to_s1.Text).ToString();
                }

                string drumNumbers_S2 = "0.0";
                if (btn_section2.IsVisible)
                {
                    if (entry_Drums_from_s2.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Drum Number (From) is invalid in section-2. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_Drums_from_s2.Text.Trim() == "" || decimal.Parse(entry_Drums_from_s2.Text.Trim()) < 0m)
                    {
                        DisplayAlert("Attention", "Drum Number (From) should not be blank or negative in section-2!!!", "Ok");
                        return;
                    }
                    if (entry_Drums_to_s2.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Drum Number (To) is invalid in section-2. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_Drums_to_s2.Text.Trim() == "" || decimal.Parse(entry_Drums_to_s2.Text.Trim()) < 0m)
                    {
                        DisplayAlert("Attention", "Drum Number (To) should not be blank or negative in section-2!!!", "Ok");
                        return;
                    }
                    drumNumbers_S2 = decimal.Parse(entry_Drums_from_s2.Text + "." + entry_Drums_to_s2.Text).ToString();
                }

                string drumNumbers_S3 = "0.0";
                if (btn_section3.IsVisible)
                {
                    if (entry_Drums_from_s3.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Drum Number (From) is invalid in section-3. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_Drums_from_s3.Text.Trim() == "" || decimal.Parse(entry_Drums_from_s3.Text.Trim()) < 0m)
                    {
                        DisplayAlert("Attention", "Drum Number (From) should not be blank or negative in section-3!!!", "Ok");
                        return;
                    }
                    if (entry_Drums_to_s3.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Drum Number (To) is invalid in section-3. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_Drums_to_s3.Text.Trim() == "" || decimal.Parse(entry_Drums_to_s3.Text.Trim()) < 0m)
                    {
                        DisplayAlert("Attention", "Drum Number (To) should not be blank or negative in section-3!!!", "Ok");
                        return;
                    }
                    drumNumbers_S3 = decimal.Parse(entry_Drums_from_s3.Text + "." + entry_Drums_to_s3.Text).ToString();
                }

                string drumNumbers_S4 = "0.0";
                if (btn_section4.IsVisible)
                {
                    if (entry_Drums_from_s4.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Drum Number (From) is invalid in section-4. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_Drums_from_s4.Text.Trim() == "" || decimal.Parse(entry_Drums_from_s4.Text.Trim()) < 0m)
                    {
                        DisplayAlert("Attention", "Drum Number (From) should not be blank or negative in section-4!!!", "Ok");
                        return;
                    }
                    if (entry_Drums_to_s4.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Drum Number (To) is invalid in section-4. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_Drums_to_s4.Text.Trim() == "" || decimal.Parse(entry_Drums_to_s4.Text.Trim()) < 0m)
                    {
                        DisplayAlert("Attention", "Drum Number (To) should not be blank or negative in section-4!!!", "Ok");
                        return;
                    }
                    drumNumbers_S4 = decimal.Parse(entry_Drums_from_s4.Text + "." + entry_Drums_to_s4.Text).ToString();
                }

                int totalSections = int.Parse(picker_sectionCount.SelectedItem.ToString());


                if (totalSections == 4)
                {
                    int min_s1 = int.Parse(entry_Drums_from_s1.Text);
                    int max_s1 = int.Parse(entry_Drums_to_s1.Text);

                    List<int> list_s1 = new List<int>();
                    for (int i = min_s1; i <= max_s1; i++)
                    {
                        list_s1.Add(i);
                    }

                    int min_s2 = int.Parse(entry_Drums_from_s2.Text);
                    int max_s2 = int.Parse(entry_Drums_to_s2.Text);

                    List<int> list_s2 = new List<int>();
                    for (int i = min_s2; i <= max_s2; i++)
                    {
                        list_s2.Add(i);
                    }

                    int min_s3 = int.Parse(entry_Drums_from_s3.Text);
                    int max_s3 = int.Parse(entry_Drums_to_s3.Text);

                    List<int> list_s3 = new List<int>();
                    for (int i = min_s3; i <= max_s3; i++)
                    {
                        list_s3.Add(i);
                    }

                    int min_s4 = int.Parse(entry_Drums_from_s4.Text);
                    int max_s4 = int.Parse(entry_Drums_to_s4.Text);

                    List<int> list_s4 = new List<int>();
                    for (int i = min_s4; i <= max_s4; i++)
                    {
                        list_s4.Add(i);
                    }

                    //check S1 drums in S2
                    if (checkDrumsInRange(min_s2, max_s2, list_s1))
                    {
                        DisplayAlert("Attention", "Section-1 drums are overlapping with Section-2 drums. Please check!!!", "Ok");
                        return;
                    }

                    //check S2 drums in S3
                    if (checkDrumsInRange(min_s3, max_s3, list_s2))
                    {
                        DisplayAlert("Attention", "Section-2 drums are overlapping with Section-3 drums. Please check!!!", "Ok");
                        return;
                    }

                    //check S3 drums in S1
                    if (checkDrumsInRange(min_s1, max_s1, list_s3))
                    {
                        DisplayAlert("Attention", "Section-3 drums are overlapping with Section-1 drums. Please check!!!", "Ok");
                        return;
                    }

                    //check S3 drums in S4
                    if (checkDrumsInRange(min_s4, max_s4, list_s3))
                    {
                        DisplayAlert("Attention", "Section-3 drums are overlapping with Section-4 drums. Please check!!!", "Ok");
                        return;
                    }

                    //check S4 drums in S1
                    if (checkDrumsInRange(min_s1, max_s1, list_s4))
                    {
                        DisplayAlert("Attention", "Section-4 drums are overlapping with Section-1 drums. Please check!!!", "Ok");
                        return;
                    }
                }
                else if (totalSections == 3)
                {
                    int min_s1 = int.Parse(entry_Drums_from_s1.Text);
                    int max_s1 = int.Parse(entry_Drums_to_s1.Text);

                    List<int> list_s1 = new List<int>();
                    for (int i = min_s1; i <= max_s1; i++)
                    {
                        list_s1.Add(i);
                    }

                    int min_s2 = int.Parse(entry_Drums_from_s2.Text);
                    int max_s2 = int.Parse(entry_Drums_to_s2.Text);

                    List<int> list_s2 = new List<int>();
                    for (int i = min_s2; i <= max_s2; i++)
                    {
                        list_s2.Add(i);
                    }

                    int min_s3 = int.Parse(entry_Drums_from_s3.Text);
                    int max_s3 = int.Parse(entry_Drums_to_s3.Text);

                    List<int> list_s3 = new List<int>();
                    for (int i = min_s3; i <= max_s3; i++)
                    {
                        list_s3.Add(i);
                    }

                    //check S1 drums in S2
                    if (checkDrumsInRange(min_s2, max_s2, list_s1))
                    {
                        DisplayAlert("Attention", "Section-1 drums are overlapping with Section-2 drums. Please check!!!", "Ok");
                        return;
                    }

                    //check S2 drums in S3
                    if (checkDrumsInRange(min_s3, max_s3, list_s2))
                    {
                        DisplayAlert("Attention", "Section-2 drums are overlapping with Section-3 drums. Please check!!!", "Ok");
                        return;
                    }

                    //check S3 drums in S1
                    if (checkDrumsInRange(min_s1, max_s1, list_s3))
                    {
                        DisplayAlert("Attention", "Section-3 drums are overlapping with Section-1 drums. Please check!!!", "Ok");
                        return;
                    }
                }
                else if (totalSections == 2)
                {
                    int min_s1 = int.Parse(entry_Drums_from_s1.Text);
                    int max_s1 = int.Parse(entry_Drums_to_s1.Text);

                    List<int> list_s1 = new List<int>();
                    for (int i = min_s1; i <= max_s1; i++)
                    {
                        list_s1.Add(i);
                    }

                    int min_s2 = int.Parse(entry_Drums_from_s2.Text);
                    int max_s2 = int.Parse(entry_Drums_to_s2.Text);

                    List<int> list_s2 = new List<int>();
                    for (int i = min_s2; i <= max_s2; i++)
                    {
                        list_s2.Add(i);
                    }

                    //check S1 drums in S2
                    if (checkDrumsInRange(min_s2, max_s2, list_s1))
                    {
                        DisplayAlert("Attention", "Section-1 drums are overlapping with Section-2 drums. Please check!!!", "Ok");
                        return;
                    }
                }
                else if (totalSections == 1)
                {
                    int min_s1 = int.Parse(entry_Drums_from_s1.Text);
                    int max_s1 = int.Parse(entry_Drums_to_s1.Text);

                    //check S1 drums aligned with total drums
                    if (min_s1 != 1 || max_s1 != int.Parse(entry_drumCount.Text))
                    {
                        DisplayAlert("Attention", "Section-1 drums are not aligned with total drums. Please check!!!", "Ok");
                        return;
                    }
                }



                if (picker_shiftCount.SelectedIndex == -1 || picker_shiftCount.SelectedItem.ToString() == "")
                {
                    DisplayAlert("Attention", "Please select valid shift count to proceed!!!", "OK");
                    return;
                }

                if ((Shift1_timePicker.Time == Shift2_timePicker.Time) && (Shift1_timePicker.Time == Shift3_timePicker.Time))
                {
                    DisplayAlert("Attention", "Please set valid shift timings to proceed!!!", "OK");
                    return;
                }

                int shiftHrs = 24 / int.Parse(picker_shiftCount.SelectedItem.ToString());

                if (picker_shiftCount.SelectedItem.ToString() == "2")
                {
                    TimeSpan shiftTime;
                    if (Shift1_timePicker.Time > Shift2_timePicker.Time)
                    {
                        shiftTime = Shift1_timePicker.Time - Shift2_timePicker.Time;
                    }
                    else
                    {
                        shiftTime = Shift2_timePicker.Time - Shift1_timePicker.Time;
                    }

                    int enteredShiftHrs = shiftTime.Hours;
                    if ((enteredShiftHrs != shiftHrs) ||
                        (Shift1_timePicker.Time.Minutes != Shift2_timePicker.Time.Minutes) ||
                        (Shift1_timePicker.Time.Seconds != Shift2_timePicker.Time.Seconds))
                    {
                        DisplayAlert("Attention", "The duration between shift is not " + shiftHrs.ToString() + " hrs !!!", "OK");
                        return;
                    }

                }
                else if (picker_shiftCount.SelectedItem.ToString() == "3")
                {
                    TimeSpan shiftTime;
                    if (Shift1_timePicker.Time > Shift2_timePicker.Time)
                    {
                        shiftTime = Shift1_timePicker.Time - Shift2_timePicker.Time;
                    }
                    else
                    {
                        shiftTime = Shift2_timePicker.Time - Shift1_timePicker.Time;
                    }

                    int enteredShiftHrs = shiftTime.Hours;

                    if ((enteredShiftHrs != shiftHrs) ||
                        (Shift1_timePicker.Time.Minutes != Shift2_timePicker.Time.Minutes) ||
                        (Shift1_timePicker.Time.Seconds != Shift2_timePicker.Time.Seconds))
                    {
                        DisplayAlert("Attention", "The duration between shift 1 and 2 is not " + shiftHrs.ToString() + " hrs !!!", "OK");
                        return;
                    }

                    if (Shift2_timePicker.Time > Shift3_timePicker.Time)
                    {
                        shiftTime = Shift2_timePicker.Time - Shift3_timePicker.Time;
                    }
                    else
                    {
                        shiftTime = Shift3_timePicker.Time - Shift2_timePicker.Time;
                    }

                    enteredShiftHrs = shiftTime.Hours;

                    if ((enteredShiftHrs != shiftHrs) ||
                        (Shift2_timePicker.Time.Minutes != Shift3_timePicker.Time.Minutes) ||
                        (Shift2_timePicker.Time.Seconds != Shift3_timePicker.Time.Seconds))
                    {
                        DisplayAlert("Attention", "The duration between shift 2 and 3 is not " + shiftHrs.ToString() + " hrs !!!", "OK");
                        return;
                    }

                }

                Guid guid = Guid.NewGuid();
                if (currentID != Guid.Empty) { guid = currentID; }
                int row = 0;
                string msg = "saved";
                

                TimeSpan shift1 = TimeSpan.Zero;
                TimeSpan shift2 = TimeSpan.Zero;
                TimeSpan shift3 = TimeSpan.Zero;
                int shift = int.Parse(picker_shiftCount.SelectedItem.ToString());
                if (shift == 1)
                {
                    shift1 = TimeSpan.Parse(Shift1_timePicker.Time.Hours.ToString() + ":" + Shift1_timePicker.Time.Minutes.ToString());
                }
                else if (shift == 2)
                {
                    shift1 = TimeSpan.Parse(Shift1_timePicker.Time.Hours.ToString() + ":" + Shift1_timePicker.Time.Minutes.ToString());
                    shift2 = TimeSpan.Parse(Shift2_timePicker.Time.Hours.ToString() + ":" + Shift2_timePicker.Time.Minutes.ToString());
                }
                else if (shift == 3)
                {
                    shift1 = TimeSpan.Parse(Shift1_timePicker.Time.Hours.ToString() + ":" + Shift1_timePicker.Time.Minutes.ToString());
                    shift2 = TimeSpan.Parse(Shift2_timePicker.Time.Hours.ToString() + ":" + Shift2_timePicker.Time.Minutes.ToString());
                    shift3 = TimeSpan.Parse(Shift3_timePicker.Time.Hours.ToString() + ":" + Shift3_timePicker.Time.Minutes.ToString());
                }

                string uf_name_1 = null;
                string uf_value_1 = null;
                string uf_name_2 = null;
                string uf_value_2 = null;
                string uf_name_3 = null;
                string uf_value_3 = null;
                string uf_name_4 = null;
                string uf_value_4 = null;

                if (lbl_userfield1.IsVisible)
                {
                    uf_name_1 = lbl_userfield1.Text.Trim();
                    uf_value_1 = entry_userfield1.Text.Trim();
                }

                if (lbl_userfield2.IsVisible)
                {
                    uf_name_2 = lbl_userfield2.Text.Trim();
                    uf_value_2 = entry_userfield2.Text.Trim();
                }

                if (lbl_userfield3.IsVisible)
                {
                    uf_name_3 = lbl_userfield3.Text.Trim();
                    uf_value_3 = entry_userfield3.Text.Trim();
                }

                if (lbl_userfield4.IsVisible)
                {
                    uf_name_4 = lbl_userfield4.Text.Trim();
                    uf_value_4 = entry_userfield4.Text.Trim();
                }

                ConfigModel configModel = new ConfigModel()
                {
                    ID = guid,
                    categoryID = selectedCategoryID,
                    machineCategory = selectedMachineCategory,
                    machineID = selectedMachineID,
                    machineName = selectedMachineName,
                    //General Data
                    speed = int.Parse(entry_macSpeed.Text),
                    p1 = decimal.Parse(entry_p1.Text.ToString()),
                    p1Deviation = decimal.Parse(entry_p1Deviation.Text.ToString()),
                    p2 = decimal.Parse(entry_p2.Text.ToString()),
                    p2Deviation = decimal.Parse(entry_p2Deviation.Text.ToString()),
                    n1 = decimal.Parse(entry_n1.Text.ToString()),
                    n1Deviation = decimal.Parse(entry_n1Deviation.Text.ToString()),
                    totalDrumCount = int.Parse(entry_drumCount.Text),
                    totalSections = int.Parse(picker_sectionCount.SelectedItem.ToString()),
                    stdRollingStrength = decimal.Parse(entry_stdRollingStrength.Text),
                    strengthDeviation = decimal.Parse(entry_strengthDeviation.Text),
                    belowLimit = int.Parse(entry_MinLimit.Text),
                    maxLimit = int.Parse(entry_MaxLimit.Text),
                    totalSamples = int.Parse(entry_totalTestCount.Text),
                    materialCount = entry_matCount.Text,
                    scheduledStartDate = date_scheduledStartDate.Date,
                    scheduledEndDate = date_scheduledEndDate.Date,
                    //Section-1

                    drumNumbers_s1 = drumNumbers_S1,
                    
                    
                    //Section-2
                    
                    drumNumbers_s2 = drumNumbers_S2,
                   
                    //Section-3
                    
                    drumNumbers_s3 = drumNumbers_S3,
                    
                    //Section-4
                   
                    drumNumbers_s4 = drumNumbers_S4,
                    
                    //Shift Details
                    shiftCount = int.Parse(picker_shiftCount.SelectedItem.ToString()),
                    shift1time = shift1.Hours.ToString() + ":" + shift1.Minutes.ToString(),
                    shift2time = shift2.Hours.ToString() + ":" + shift2.Minutes.ToString(),
                    shift3time = shift3.Hours.ToString() + ":" + shift3.Minutes.ToString(),
                    //User fields
                    uf_name_1 = uf_name_1,
                    uf_value_1 = uf_value_1,
                    uf_name_2 = uf_name_2,
                    uf_value_2 = uf_value_2,
                    uf_name_3 = uf_name_3,
                    uf_value_3 = uf_value_3,
                    uf_name_4 = uf_name_4,
                    uf_value_4 = uf_value_4
                };

                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<ConfigModel>();
                    if (btn_save.Text == "Save")
                    {
                        configModel.createdate = DateTime.Now;
                        configModel.updateddate = DateTime.Now;
                        row = conn.Insert(configModel);
                    }
                    else
                    {
                        msg = "updated";
                        configModel.createdate = currentCreatedDate;
                        configModel.updateddate = DateTime.Now;
                        row = conn.Update(configModel);
                    }
                    if (row > 0)
                    {
                        bool modifyScheduleDateStatus = true;
                        if (is_ScheduledDateUpdateInTestRequired)
                        {
                            modifyScheduleDateStatus = modifyScheduleDatesInTest();
                            if (modifyScheduleDateStatus)
                            {
                                selectedScheduledEndDate = date_scheduledEndDate.Date;
                            }
                        }
                        bool historyStatus = addHistory(selectedMachineID);
                        if (!historyStatus || !modifyScheduleDateStatus)
                        {
                            DisplayAlert("Failure", "Settings failed to be " + msg + ". Try again!!!", "OK");
                        }
                        int updatedRecCount = 0;
                        bool isShiftChanged = false;
                        List<ConfigModel> settingsList = conn.Table<ConfigModel>().ToList();
                        ConfigModel dbSettings = conn.Table<ConfigModel>().Where(ConfigModel =>
                                                            (ConfigModel.machineCategory == selectedMachineCategory &&
                                                            ConfigModel.machineName == selectedMachineName &&
                                                            ConfigModel.machineID == selectedMachineID)).FirstOrDefault();
                        if (dbSettings != null)
                        {
                            if (dbSettings.shiftCount != currentShift) { isShiftChanged = true; }
                            if (TimeSpan.FromHours(TimeSpan.Parse(dbSettings.shift1time).TotalHours) != currentShift1) { isShiftChanged = true; }
                            if (TimeSpan.FromHours(TimeSpan.Parse(dbSettings.shift2time).TotalHours) != currentShift2) { isShiftChanged = true; }
                            if (TimeSpan.FromHours(TimeSpan.Parse(dbSettings.shift3time).TotalHours) != currentShift3) { isShiftChanged = true; }

                            if (isShiftChanged)
                            {

                                if (settingsList.Count > 0)
                                {
                                    foreach (ConfigModel setting in settingsList)
                                    {
                                        setting.shiftCount = int.Parse(picker_shiftCount.SelectedItem.ToString());
                                        setting.shift1time = shift1.Hours.ToString() + ":" + shift1.Minutes.ToString();
                                        setting.shift2time = shift2.Hours.ToString() + ":" + shift2.Minutes.ToString();
                                        setting.shift3time = shift3.Hours.ToString() + ":" + shift3.Minutes.ToString();
                                        int rowImp = conn.Update(setting);
                                        if (rowImp > 0) { updatedRecCount++; }
                                    }
                                }
                            }
                        }

                        if (isShiftChanged)
                        {
                            if (settingsList.Count == updatedRecCount)
                            {
                                btn_save.Text = "Update";
                                btn_save.BackgroundColor = Color.FromHex("#0e0273");
                                btn_save.TextColor = Color.White;
                                DisplayAlert("Success", "Settings " + msg + " successfully!!!", "OK");
                            }
                            else
                            {
                                btn_save.Text = "Update";
                                btn_save.BackgroundColor = Color.FromHex("#0e0273");
                                btn_save.TextColor = Color.White;
                                DisplayAlert("Warning", "Settings " + msg + " successfully but failed to update shift details for all machines!!!", "OK");
                            }
                        }
                        else
                        {
                            btn_save.Text = "Update";
                            btn_save.BackgroundColor = Color.FromHex("#0e0273");
                            btn_save.TextColor = Color.White;
                            DisplayAlert("Success", "Settings " + msg + " successfully!!!", "OK");
                        }
                    }
                    else
                    {
                        DisplayAlert("Failure", "Settings failed to be " + msg + "!!!", "OK");
                    }

                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred: " + ex.Message.ToString(), "OK");
            }
        }

        private bool modifyScheduleDatesInTest()
        {
            try
            {
                DateTime sch_startDate = Convert.ToDateTime(selectedScheduledStartDate.Date);
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    List<StrengthTestSummaryModel> sts_list = conn.Table<StrengthTestSummaryModel>().Where(StrengthTestSummaryModel =>
                                                              (StrengthTestSummaryModel.categoryID == selectedCategoryID
                                                              && StrengthTestSummaryModel.machineID == selectedMachineID
                                                              && StrengthTestSummaryModel.scheduledStartDate == sch_startDate)).ToList();
                    if (sts_list.Count > 0)
                    {
                        int failCounter = 0;
                        foreach(StrengthTestSummaryModel sts in sts_list)
                        {
                            sts.scheduledEndDate = date_scheduledEndDate.Date;
                            int row = conn.Update(sts);
                            if (row < 1) { failCounter += 1; }
                        }
                        if (failCounter > 0) { return false; }
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool addHistory(Guid selectedMachineID)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    ConfigModel cm = conn.Table<ConfigModel>().Where(ConfigModel =>
                                    (ConfigModel.machineID == selectedMachineID)).FirstOrDefault();

                    if (cm != null)
                    {
                        TestConfigModel tcm = new TestConfigModel()
                        {
                            ID = Guid.NewGuid(),
                            testID = 0,
                            categoryID = cm.categoryID,
                            machineID = cm.machineID,
                            machineCategory = cm.machineCategory,
                            machineName = cm.machineName,
                            speed = cm.speed,
                            p1 = cm.p1,
                            p1Deviation = cm.p1Deviation,
                            p2 = cm.p2,
                            p2Deviation = cm.p2Deviation,
                            n1 = cm.n1,
                            n1Deviation = cm.n1Deviation,
                            totalDrumCount = cm.totalDrumCount,
                            totalSections = cm.totalSections,
                            stdRollingStrength = cm.stdRollingStrength,
                            strengthDeviation = cm.strengthDeviation,
                            belowLimit = cm.belowLimit,
                            maxLimit = cm.maxLimit,
                            totalSamples = cm.totalSamples,
                            materialCount = cm.materialCount,
                            scheduledStartDate = cm.scheduledStartDate,
                            scheduledEndDate = cm.scheduledEndDate,
                            drumNumbers_s1 = cm.drumNumbers_s1,
                            drumNumbers_s2 = cm.drumNumbers_s2,
                            drumNumbers_s3 = cm.drumNumbers_s3,
                            drumNumbers_s4 = cm.drumNumbers_s4,
                            shiftCount = cm.shiftCount,
                            shift1time = cm.shift1time,
                            shift2time = cm.shift2time,
                            shift3time = cm.shift3time,
                            uf_name_1 = cm.uf_name_1,
                            uf_value_1 = cm.uf_value_1,
                            uf_name_2 = cm.uf_name_2,
                            uf_value_2 = cm.uf_value_2,
                            uf_name_3 = cm.uf_name_3,
                            uf_value_3 = cm.uf_value_3,
                            uf_name_4 = cm.uf_name_4,
                            uf_value_4 = cm.uf_value_4,
                            updateddate = cm.updateddate,
                            createdate = cm.createdate,
                        };
                        int row_tcm = conn.Insert(tcm);
                        if (row_tcm < 1)
                        {
                            return false;
                        }
                    }
                    else { return false; }
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        } 

        private bool checkDrumsInRange(int min, int max, List<int> src)
        {
            try
            {
                for (int i = min; i <= max; i++)
                {
                    if (src.Contains(i)) { return true; }
                }
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private void picker_shiftCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            toggleShift();
        }

        private void toggleShift()
        {
            if (picker_shiftCount.SelectedIndex != -1 && picker_shiftCount.SelectedIndex != 0)
            {
                if (picker_shiftCount.SelectedItem.ToString() == "1")
                {
                    shift1label.IsVisible = true;
                    Shift1_timePicker.IsVisible = true;
                    shift2label.IsVisible = false;
                    Shift2_timePicker.IsVisible = false;
                    shift3label.IsVisible = false;
                    Shift3_timePicker.IsVisible = false;
                }
                else if (picker_shiftCount.SelectedItem.ToString() == "2")
                {
                    shift1label.IsVisible = true;
                    Shift1_timePicker.IsVisible = true;
                    shift2label.IsVisible = true;
                    Shift2_timePicker.IsVisible = true;
                    shift3label.IsVisible = false;
                    Shift3_timePicker.IsVisible = false;
                }
                else if (picker_shiftCount.SelectedItem.ToString() == "3")
                {
                    shift1label.IsVisible = true;
                    Shift1_timePicker.IsVisible = true;
                    shift2label.IsVisible = true;
                    Shift2_timePicker.IsVisible = true;
                    shift3label.IsVisible = true;
                    Shift3_timePicker.IsVisible = true;
                }
            }
            else
            {
                shift1label.IsVisible = false;
                Shift1_timePicker.IsVisible = false;
                shift2label.IsVisible = false;
                Shift2_timePicker.IsVisible = false;
                shift3label.IsVisible = false;
                Shift3_timePicker.IsVisible = false;
            }

        }

        private void reset()
        {
            entry_macSpeed.Text = "";
            entry_p1.Text = "";
            entry_p1Deviation.Text = "";
            entry_p2.Text = "";
            entry_p2Deviation.Text = "";
            entry_n1.Text = "";
            entry_n1Deviation.Text = "";
            entry_drumCount.Text = "";
            picker_sectionCount.SelectedItem = -1;
            entry_stdRollingStrength.Text = "";
            entry_strengthDeviation.Text = "";
            entry_MinLimit.Text = "";
            entry_MaxLimit.Text = "";
            entry_totalTestCount.Text = "";
            entry_matCount.Text = "";
            date_scheduledStartDate.Date = DateTime.Today.Date;
            date_scheduledEndDate.Date = DateTime.Today.Date;
            frame_sec1.IsVisible = false;
            btn_section1.BackgroundColor =  Color.FromHex("#0e0273");
            entry_Drums_from_s1.Text = "";
            entry_Drums_to_s1.Text = "";
            frame_sec2.IsVisible = false;
            btn_section2.BackgroundColor = Color.FromHex("#0e0273");
            entry_Drums_from_s2.Text = "";
            entry_Drums_to_s2.Text = "";
            frame_sec3.IsVisible = false;
            btn_section3.BackgroundColor = Color.FromHex("#0e0273");
            entry_Drums_from_s3.Text = "";
            entry_Drums_to_s3.Text = "";
            frame_sec4.IsVisible = false;
            btn_section4.BackgroundColor = Color.FromHex("#0e0273");
            entry_Drums_from_s4.Text = "";
            entry_Drums_to_s4.Text = "";
        }
       
        private void picker_machinecategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                List<CategoryModel> source = (List<CategoryModel>)picker_machinecategory.ItemsSource;
                if (picker_machinecategory.SelectedIndex < 0)
                {
                    selectedCategoryID = Guid.Empty;
                    selectedMachineCategory = null;
                    selectedMachineID = Guid.Empty;
                    selectedMachineName = null;
                    return;
                }
                selectedCategoryID = (Guid)source[picker_machinecategory.SelectedIndex].ID;
                CategoryModel selectedMachine = (CategoryModel)picker_machinecategory.SelectedItem;
                selectedMachineCategory = selectedMachine.category;

                if (selectedMachineCategory == "" || selectedMachineCategory == null)
                {
                    picker_machinename.ItemsSource = null;
                }
                else
                {
                    using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                    {
                        conn.CreateTable<MachineModel>();
                        List<MachineModel> machineModelList = conn.Table<MachineModel>().Where(MachineModel => MachineModel.categoryID == selectedCategoryID).ToList();
                        picker_machinename.ItemsSource = machineModelList;
                    }
                }
                populateSettingsField(null);
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }

        private void picker_machinename_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                List<MachineModel> source = (List<MachineModel>)picker_machinename.ItemsSource;
                if (picker_machinename.SelectedIndex < 0)
                {
                    selectedMachineID = Guid.Empty;
                    selectedMachineName = null;
                    return;
                }
                selectedMachineID = (Guid)source[picker_machinename.SelectedIndex].ID;
                MachineModel selectedMachine = (MachineModel)picker_machinename.SelectedItem;
                selectedMachineName = selectedMachine.machineName;
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<ConfigModel>();
                    List<ConfigModel> ycConfigList = conn.Table<ConfigModel>().ToList();
                    if (ycConfigList.Count > 0)
                    {
                        ConfigModel machineSetting = ycConfigList.Where(ConfigModel =>
                                                    (ConfigModel.categoryID == selectedCategoryID &&
                                                    ConfigModel.machineID == selectedMachineID &&
                                                    ConfigModel.machineName == selectedMachineName)).FirstOrDefault();
                        if (machineSetting != null)
                        {
                            populateSettingsField(machineSetting);
                        }
                        else
                        {
                            machineSetting = ycConfigList.Where(ConfigModel =>
                                                    (ConfigModel.categoryID == selectedCategoryID))
                                                     .OrderByDescending(ConfigModel =>
                                                    (ConfigModel.createdate)).FirstOrDefault();
                            if (machineSetting != null)
                            {
                                populateSettingsField(machineSetting, false, true);
                            }
                            else
                            {
                                machineSetting = ycConfigList.Where(ConfigModel =>
                                                    (ConfigModel.machineCategory != "" &&
                                                    ConfigModel.machineCategory != null)).FirstOrDefault();
                                if (machineSetting != null)
                                {
                                    populateSettingsField(machineSetting, true, true);
                                }
                                else
                                {
                                    populateSettingsField(null);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }

        private async void btn_addUserField_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (lbl_userfield1.IsVisible == true &&
                    lbl_userfield2.IsVisible == true &&
                    lbl_userfield3.IsVisible == true &&
                    lbl_userfield4.IsVisible == true)
                {
                    await DisplayAlert("Attention", "Only 4 user fields can be added!!!", "OK");
                    return;
                }
                var userconfirmation = await DisplayAlert("New Field", "Do you want to add new field ?", "Yes", "No");
                if (userconfirmation)
                {
                    string fieldName = await DisplayPromptAsync("Field Name", "Enter the field name to proceed");
                    if (fieldName == null) { return; }
                    fieldName = fieldName.Trim();
                    if (fieldName != null && fieldName != "")
                    {
                        if (fieldName.Length > 10)
                        {
                            await DisplayAlert("Attention", "Field name can be of 10 characters maximum", "OK");
                            return;
                        }
                        if (hasSpecialChar(fieldName))
                        {
                            await DisplayAlert("Attention", "Field Name should be in combinations of letters, spaces and numbers!!!", "OK");
                            return;
                        }
                        fieldName = firstCharToUpper(fieldName);
                        if (lbl_userfield1.IsVisible == false)
                        {
                            lbl_userfield1.Text = fieldName;
                            lbl_userfield1.IsVisible = true;
                            entry_userfield1.IsVisible = true;
                        }
                        else if (lbl_userfield2.IsVisible == false)
                        {
                            lbl_userfield2.Text = fieldName;
                            lbl_userfield2.IsVisible = true;
                            entry_userfield2.IsVisible = true;
                        }
                        else if (lbl_userfield3.IsVisible == false)
                        {
                            lbl_userfield3.Text = fieldName;
                            lbl_userfield3.IsVisible = true;
                            entry_userfield3.IsVisible = true;
                        }
                        else if (lbl_userfield4.IsVisible == false)
                        {
                            lbl_userfield4.Text = fieldName;
                            lbl_userfield4.IsVisible = true;
                            entry_userfield4.IsVisible = true;
                        }
                        else
                        {
                            await DisplayAlert("Attention", "Only 4 user fields can be added!!!", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }

        }

        public static bool hasSpecialChar(string input)
        {
            string specialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,";
            foreach (var item in specialChar)
            {
                if (input.Contains(item)) return true;
            }
            return false;
        }

        public static string firstCharToUpper(string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default:
                    string[] inputSplit = input.Split(' ');
                    string finalOut = null;
                    int counter = 1;
                    foreach (var split in inputSplit)
                    {
                        if (counter == 1)
                        {
                            finalOut = split[0].ToString().ToUpper() + split.Substring(1);
                        }
                        else
                        {
                            finalOut = finalOut + " " + split[0].ToString().ToUpper() + split.Substring(1);
                        }
                        counter++;
                    }
                    return finalOut;
            }
        }

        //private void toggleSectionFrames(Frame frame,Button sectionButton, DatePicker scheduledStartDate , DatePicker scheduledEndDate,Entry scheduledDayLimit,Picker drumSelectionMethod)
        //{
        //    try
        //    {
        //        if (!frame.IsVisible)
        //        {
        //            frame.IsVisible = true;
        //            sectionButton.BackgroundColor = Color.Red;
        //        }
        //        else
        //        {
        //            frame.IsVisible = false;
        //            sectionButton.BackgroundColor = Color.FromHex("#0e0273");
        //        }
        //        //calculateDay(drumSelectionMethod,scheduledDayLimitDate, scheduledDayLimit);
        //        calculateDay(scheduledStartDate, scheduledEndDate, scheduledDayLimit);
        //    }
        //    catch (Exception)
        //    {
        //        //ignore
        //    }
        //}

        private void toggleSectionFrames(Frame frame, Button sectionButton)
        {
            try
            {
                if (!frame.IsVisible)
                {
                    frame.IsVisible = true;
                    sectionButton.BackgroundColor = Color.Red;
                }
                else
                {
                    frame.IsVisible = false;
                    sectionButton.BackgroundColor = Color.FromHex("#0e0273");
                }
                //calculateDay(drumSelectionMethod,scheduledDayLimitDate, scheduledDayLimit);
                //calculateDay(scheduledStartDate, scheduledEndDate, scheduledDayLimit);
            }
            catch (Exception)
            {
                //ignore
            }
        }

        private void btn_section1_Clicked(object sender, EventArgs e)
        {
            //toggleSectionFrames(frame_sec1, btn_section1, date_scheduledDayLimitDate_Sec1, entry_scheduledDayLimit_Sec1, picker_drumSection_Sec1);
            //toggleSectionFrames(frame_sec1, btn_section1, date_scheduledStartDate_Sec1, date_scheduledEndDate_Sec1, entry_scheduledDayLimit_Sec1, picker_drumSection_Sec1);
            //picker_drumSection_Sec1.SelectedItem = "Scheduled";
            toggleSectionFrames(frame_sec1, btn_section1);
        }

        private void btn_section2_Clicked(object sender, EventArgs e)
        {
            //toggleSectionFrames(frame_sec2, btn_section2, date_scheduledDayLimitDate_Sec2, entry_scheduledDayLimit_Sec2,picker_drumSection_Sec2);
            //toggleSectionFrames(frame_sec2, btn_section2, date_scheduledStartDate_Sec2, date_scheduledEndDate_Sec2, entry_scheduledDayLimit_Sec2, picker_drumSection_Sec2);
            //picker_drumSection_Sec2.SelectedItem = "Scheduled";
            toggleSectionFrames(frame_sec2, btn_section2);
        }

        private void btn_section3_Clicked(object sender, EventArgs e)
        {
            //toggleSectionFrames(frame_sec3, btn_section3, date_scheduledDayLimitDate_Sec3, entry_scheduledDayLimit_Sec3, picker_drumSection_Sec3);
            //toggleSectionFrames(frame_sec3, btn_section3, date_scheduledStartDate_Sec3, date_scheduledEndDate_Sec3, entry_scheduledDayLimit_Sec3, picker_drumSection_Sec3);
            //picker_drumSection_Sec3.SelectedItem = "Scheduled";
            toggleSectionFrames(frame_sec3, btn_section3);
        }

        private void btn_section4_Clicked(object sender, EventArgs e)
        {
            //toggleSectionFrames(frame_sec4, btn_section4, date_scheduledDayLimitDate_Sec4, entry_scheduledDayLimit_Sec4, picker_drumSection_Sec4);
            //toggleSectionFrames(frame_sec4, btn_section4, date_scheduledStartDate_Sec4, date_scheduledEndDate_Sec4, entry_scheduledDayLimit_Sec4, picker_drumSection_Sec4);
            //picker_drumSection_Sec4.SelectedItem = "Scheduled";
            toggleSectionFrames(frame_sec4, btn_section4);
        }

        private void picker_sectionCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            calculateDrumNumbers();
        }

        private void calculateDrumNumbers()
        {
            try
            {
                if (picker_sectionCount.SelectedIndex > 0)
                {
                    int totalDrumCount;
                    int.TryParse(entry_drumCount.Text, out totalDrumCount);
                    //if (totalDrumCount == 0)
                    //{
                    //    DisplayAlert("Attention", "Total drum count should not be Zero!!!", "OK");
                    //    picker_sectionCount.SelectedIndex = 0;
                    //    return;
                    //}
                    if (picker_sectionCount.SelectedItem.ToString() == "1")
                    {
                        btn_section1.IsVisible = true;
                        btn_section2.IsVisible = false;
                        btn_section3.IsVisible = false;
                        btn_section4.IsVisible = false;
                        btn_section2.BackgroundColor = Color.FromHex("#0e0273");
                        btn_section3.BackgroundColor = Color.FromHex("#0e0273");
                        btn_section4.BackgroundColor = Color.FromHex("#0e0273");
                        frame_sec1.IsVisible = true;
                        frame_sec2.IsVisible = false;
                        frame_sec3.IsVisible = false;
                        frame_sec4.IsVisible = false;

                        entry_Drums_from_s1.Text = "1";
                        entry_Drums_to_s1.Text = totalDrumCount.ToString();

                        
                        btn_section1.BackgroundColor = Color.Red;
                        

                    }
                    if (picker_sectionCount.SelectedItem.ToString() == "2")
                    {
                        btn_section1.IsVisible = true;
                        btn_section2.IsVisible = true;
                        btn_section3.IsVisible = false;
                        btn_section4.IsVisible = false;
                        
                        frame_sec1.IsVisible = true;
                        frame_sec2.IsVisible = true;
                        frame_sec3.IsVisible = false;
                        frame_sec4.IsVisible = false;

                        btn_section1.BackgroundColor = Color.Red;
                        btn_section2.BackgroundColor = Color.Red;
                        btn_section3.BackgroundColor = Color.FromHex("#0e0273");
                        btn_section4.BackgroundColor = Color.FromHex("#0e0273");

                        int reminder = totalDrumCount % 2;
                        int equalPortion = (totalDrumCount - reminder) / 2;

                        entry_Drums_from_s1.Text = "1";
                        entry_Drums_to_s1.Text = equalPortion.ToString();
      
                        entry_Drums_from_s2.Text = (equalPortion + 1).ToString();
                        entry_Drums_to_s2.Text =  totalDrumCount.ToString();

                    }
                    if (picker_sectionCount.SelectedItem.ToString() == "3")
                    {
                        btn_section1.IsVisible = true;
                        btn_section2.IsVisible = true;
                        btn_section3.IsVisible = true;
                        btn_section4.IsVisible = false;

                        frame_sec1.IsVisible = true;
                        frame_sec2.IsVisible = true;
                        frame_sec3.IsVisible = true;
                        frame_sec4.IsVisible = false;

                        btn_section1.BackgroundColor = Color.Red;
                        btn_section2.BackgroundColor = Color.Red;
                        btn_section3.BackgroundColor = Color.Red;
                        btn_section4.BackgroundColor = Color.FromHex("#0e0273");

                        int reminder = totalDrumCount % 3;
                        int equalPortion = (totalDrumCount - reminder) / 3;

                        entry_Drums_from_s1.Text = "1";
                        entry_Drums_to_s1.Text = equalPortion.ToString();

                        entry_Drums_from_s2.Text = (equalPortion + 1).ToString();
                        entry_Drums_to_s2.Text = (equalPortion * 2).ToString();

                        entry_Drums_from_s3.Text = ((equalPortion * 2) + 1).ToString();
                        entry_Drums_to_s3.Text = totalDrumCount.ToString();

                        //entry_Drums_Sec1.Text = "1." + equalPortion.ToString();
                        //entry_Drums_Sec1.IsEnabled = true;
                        //entry_Drums_Sec2.Text = (equalPortion + 1).ToString() + "." + (equalPortion * 2);
                        //entry_Drums_Sec3.Text = ((equalPortion * 2) + 1).ToString() + "." + totalDrumCount;
                    }
                    if (picker_sectionCount.SelectedItem.ToString() == "4")
                    {
                        btn_section1.IsVisible = true;
                        btn_section2.IsVisible = true;
                        btn_section3.IsVisible = true;
                        btn_section4.IsVisible = true;

                        frame_sec1.IsVisible = true;
                        frame_sec2.IsVisible = true;
                        frame_sec3.IsVisible = true;
                        frame_sec4.IsVisible = true;

                        btn_section1.BackgroundColor = Color.Red;
                        btn_section2.BackgroundColor = Color.Red;
                        btn_section3.BackgroundColor = Color.Red;
                        btn_section4.BackgroundColor = Color.Red;

                        int reminder = totalDrumCount % 4;
                        int equalPortion = (totalDrumCount - reminder) / 4;

                        entry_Drums_from_s1.Text = "1";
                        entry_Drums_to_s1.Text = equalPortion.ToString();

                        entry_Drums_from_s2.Text = (equalPortion + 1).ToString();
                        entry_Drums_to_s2.Text = (equalPortion * 2).ToString();

                        entry_Drums_from_s3.Text = ((equalPortion * 2) + 1).ToString();
                        entry_Drums_to_s3.Text = (equalPortion * 3).ToString();

                        entry_Drums_from_s4.Text = ((equalPortion * 3) + 1).ToString();
                        entry_Drums_to_s4.Text = totalDrumCount.ToString();

                        //entry_Drums_Sec1.Text = "1." + equalPortion.ToString();//1.4
                        //entry_Drums_Sec1.IsEnabled = true;
                        //entry_Drums_Sec2.Text = (equalPortion + 1).ToString() + "." + (equalPortion * 2);//5.8
                        //entry_Drums_Sec3.Text = ((equalPortion * 2) + 1).ToString() + "." + (equalPortion * 3);//9.12
                        //entry_Drums_Sec4.Text = ((equalPortion * 3) + 1).ToString() + "." + totalDrumCount;//13.16
                    }
                }
                //else
                //{
                //    btn_section1.IsVisible = false;
                //    btn_section2.IsVisible = false;
                //    btn_section3.IsVisible = false;
                //    btn_section1.BackgroundColor = Color.FromHex("#0e0273");
                //    btn_section2.BackgroundColor = Color.FromHex("#0e0273");
                //    btn_section3.BackgroundColor = Color.FromHex("#0e0273");
                //    frame_sec1.IsVisible = false;
                //    frame_sec2.IsVisible = false;
                //    frame_sec3.IsVisible = false;

                //    entry_Drums_Sec1.Text = "0";
                //    entry_Drums_Sec1.IsEnabled = true;
                //    entry_Drums_Sec2.Text = "0";
                //    entry_Drums_Sec3.Text = "0";
                //}
            }
            catch(Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }

        //private void date_scheduledDayLimitDate_Sec1_DateSelected(object sender, DateChangedEventArgs e)
        //{
        //    calculateDay(picker_drumSection_Sec1,date_scheduledDayLimitDate_Sec1, entry_scheduledDayLimit_Sec1);
        //}

        //private void date_scheduledDayLimitDate_Sec2_DateSelected(object sender, DateChangedEventArgs e)
        //{
        //    calculateDay(picker_drumSection_Sec2,date_scheduledDayLimitDate_Sec2, entry_scheduledDayLimit_Sec2);
        //}

        //private void date_scheduledDayLimitDate_Sec3_DateSelected(object sender, DateChangedEventArgs e)
        //{
        //    calculateDay(picker_drumSection_Sec3,date_scheduledDayLimitDate_Sec3, entry_scheduledDayLimit_Sec3);
        //}

        //private void calculateDay(Picker picker, DatePicker datePicker, Entry entry)
        //{
        //    try
        //    {
        //        DateTime today = DateTime.Today;
        //        if (currentUpdatedDate != DEFAULTDATE) { today = currentUpdatedDate; }
        //        DateTime selectedDate = datePicker.Date;
        //        int dayDiff = (int)(selectedDate - today).TotalDays;
        //        if (dayDiff < 0 && picker.SelectedItem.ToString() == "Scheduled" && currentUpdatedDate!=DEFAULTDATE)
        //        {
        //            entry.Text = "0";
        //            DisplayAlert("Attention", "Schedule day limit date should be greater than or equal to today's date!!!", "OK");
        //            return;
        //        }
        //        if (dayDiff >= 0)
        //        {
        //            entry.Text = (dayDiff + 1).ToString();
        //        }
        //    }
        //    catch(Exception)
        //    {
        //        entry.Text = "0";
        //        DisplayAlert("Attention", "Error occurred in Schedule day limit date selection!!!", "OK");
        //    }
        //}

        private void calculateDay(DatePicker startDatePicker, DatePicker endDatePicker, Entry scheduledDayLimit)
        {
            try
            {
                DateTime startDate = startDatePicker.Date;
                DateTime endDate = endDatePicker.Date;
                int dayDiff = (int)(endDate - startDate).TotalDays;
                if (dayDiff < 0)
                {
                    scheduledDayLimit.Text = (dayDiff + (-1)).ToString();
                    //scheduledDayLimit.Text = "0";
                    //DisplayAlert("Attention", "Schedule end date should be greater than or equal to start date!!!", "OK");
                    //return;
                }
                if (dayDiff >= 0)
                {
                    scheduledDayLimit.Text = (dayDiff + 1).ToString();
                }
            }
            catch (Exception)
            {
                scheduledDayLimit.Text = "0";
                DisplayAlert("Attention", "Error occurred in Schedule date selection!!!", "OK");
            }
        }

        //private void picker_drumSection_Sec1_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (picker_drumSection_Sec1.SelectedItem.ToString() == "Scheduled")
        //    {
        //        lbl_scheduledDayLimit_Sec1.IsVisible = true;
        //        entry_scheduledDayLimit_Sec1.IsVisible = true;
        //        date_scheduledDayLimitDate_Sec1.IsVisible = true;
        //        if(currentUpdatedDate == DEFAULTDATE) { date_scheduledDayLimitDate_Sec1.Date = DateTime.Now; }
        //    }
        //    else
        //    {
        //        lbl_scheduledDayLimit_Sec1.IsVisible = false;
        //        entry_scheduledDayLimit_Sec1.IsVisible = false;
        //        date_scheduledDayLimitDate_Sec1.IsVisible = false;
        //        entry_scheduledDayLimit_Sec1.Text = "0";
        //        if (currentUpdatedDate == DEFAULTDATE)
        //        {
        //            date_scheduledDayLimitDate_Sec1.Date = DEFAULTDATE;
        //        }
        //        else
        //        {
        //            date_scheduledDayLimitDate_Sec1.Date = currentUpdatedDate;
        //        };
        //    }
        //}

        //private void picker_drumSection_Sec2_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (picker_drumSection_Sec2.SelectedItem.ToString() == "Scheduled")
        //    {
        //        lbl_scheduledDayLimit_Sec2.IsVisible = true;
        //        entry_scheduledDayLimit_Sec2.IsVisible = true;
        //        date_scheduledDayLimitDate_Sec2.IsVisible = true;
        //        if (currentUpdatedDate == DEFAULTDATE) { date_scheduledDayLimitDate_Sec2.Date = DateTime.Now; }
        //    }
        //    else
        //    {
        //        lbl_scheduledDayLimit_Sec2.IsVisible = false;
        //        entry_scheduledDayLimit_Sec2.IsVisible = false;
        //        date_scheduledDayLimitDate_Sec2.IsVisible = false;
        //        entry_scheduledDayLimit_Sec2.Text = "0";
        //        if (currentUpdatedDate == DEFAULTDATE)
        //        {
        //            date_scheduledDayLimitDate_Sec2.Date = DEFAULTDATE;
        //        }
        //        else
        //        {
        //            date_scheduledDayLimitDate_Sec2.Date = currentUpdatedDate;
        //        };
        //    }
        //}

        //private void picker_drumSection_Sec3_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (picker_drumSection_Sec3.SelectedItem.ToString() == "Scheduled")
        //    {
        //        lbl_scheduledDayLimit_Sec3.IsVisible = true;
        //        entry_scheduledDayLimit_Sec3.IsVisible = true;
        //        date_scheduledDayLimitDate_Sec3.IsVisible = true;
        //        if (currentUpdatedDate == DEFAULTDATE) { date_scheduledDayLimitDate_Sec3.Date = DateTime.Now; }
        //    }
        //    else
        //    {
        //        lbl_scheduledDayLimit_Sec3.IsVisible = false;
        //        entry_scheduledDayLimit_Sec3.IsVisible = false;
        //        date_scheduledDayLimitDate_Sec3.IsVisible = false;
        //        entry_scheduledDayLimit_Sec3.Text = "0";
        //        if (currentUpdatedDate == DEFAULTDATE)
        //        {
        //            date_scheduledDayLimitDate_Sec3.Date = DEFAULTDATE;
        //        }
        //        else
        //        {
        //            date_scheduledDayLimitDate_Sec3.Date = currentUpdatedDate;
        //        };
        //    }
        //}

        private void entry_drumCount_TextChanged(System.Object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            calculateDrumNumbers();
        }

        //void date_scheduledStartDate_Sec1_DateSelected(System.Object sender, Xamarin.Forms.DateChangedEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec1, date_scheduledEndDate_Sec1, entry_scheduledDayLimit_Sec1);
        //}

        //void date_scheduledStartDate_Sec1_Unfocused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec1, date_scheduledEndDate_Sec1, entry_scheduledDayLimit_Sec1);
        //}

        //void date_scheduledEndDate_Sec1_DateSelected(System.Object sender, Xamarin.Forms.DateChangedEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec1, date_scheduledEndDate_Sec1, entry_scheduledDayLimit_Sec1);
        //}

        //void date_scheduledEndDate_Sec1_Unfocused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec1, date_scheduledEndDate_Sec1, entry_scheduledDayLimit_Sec1);
        //}

        //void date_scheduledStartDate_Sec2_DateSelected(System.Object sender, Xamarin.Forms.DateChangedEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec2, date_scheduledEndDate_Sec2, entry_scheduledDayLimit_Sec2);
        //}

        //void date_scheduledStartDate_Sec2_Unfocused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec2, date_scheduledEndDate_Sec2, entry_scheduledDayLimit_Sec2);
        //}

        //void date_scheduledEndDate_Sec2_DateSelected(System.Object sender, Xamarin.Forms.DateChangedEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec2, date_scheduledEndDate_Sec2, entry_scheduledDayLimit_Sec2);
        //}

        //void date_scheduledEndDate_Sec2_Unfocused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec2, date_scheduledEndDate_Sec2, entry_scheduledDayLimit_Sec2);
        //}

        //void date_scheduledStartDate_Sec3_DateSelected(System.Object sender, Xamarin.Forms.DateChangedEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec3, date_scheduledEndDate_Sec3, entry_scheduledDayLimit_Sec3);
        //}

        //void date_scheduledStartDate_Sec3_Unfocused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec3, date_scheduledEndDate_Sec3, entry_scheduledDayLimit_Sec3);
        //}

        //void date_scheduledEndDate_Sec3_DateSelected(System.Object sender, Xamarin.Forms.DateChangedEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec3, date_scheduledEndDate_Sec3, entry_scheduledDayLimit_Sec3);
        //}

        //void date_scheduledEndDate_Sec3_Unfocused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec3, date_scheduledEndDate_Sec3, entry_scheduledDayLimit_Sec3);
        //}

        //void date_scheduledStartDate_Sec4_DateSelected(System.Object sender, Xamarin.Forms.DateChangedEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec4, date_scheduledEndDate_Sec4, entry_scheduledDayLimit_Sec4);
        //}

        //void date_scheduledStartDate_Sec4_Unfocused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec4, date_scheduledEndDate_Sec4, entry_scheduledDayLimit_Sec4);
        //}

        //void date_scheduledEndDate_Sec4_DateSelected(System.Object sender, Xamarin.Forms.DateChangedEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec4, date_scheduledEndDate_Sec4, entry_scheduledDayLimit_Sec4);
        //}

        //void date_scheduledEndDate_Sec4_Unfocused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        //{
        //    calculateDay(date_scheduledStartDate_Sec4, date_scheduledEndDate_Sec4, entry_scheduledDayLimit_Sec4);
        //}
    }
}