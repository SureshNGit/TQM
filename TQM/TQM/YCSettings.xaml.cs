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
        private Guid currentID = Guid.Empty;
        private string selectedMachineCategory = null;
        private Guid selectedMachineID = Guid.Empty;
        private string selectedMachineName = null;
        public YCSettings()
        {
            try
            {
                InitializeComponent();
                fetchConfig();
                //using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                //{
                //    conn.CreateTable<YarnCountConfigModel>();
                //    List<YarnCountConfigModel> ycConfigList = conn.Table<YarnCountConfigModel>().ToList();
                //    if (ycConfigList.Count > 0)
                //    {
                //        btn_save.Text = "Update";
                //        currentID = ycConfigList[0].ID;
                //        IList<string> countsystemlist = picker_countsysname.Items;
                //        int countsysindex = 0;
                //        foreach (string countsystem in countsystemlist)
                //        {
                //            if (countsystem != ycConfigList[0].countsysname.ToString())
                //            {
                //                countsysindex++;
                //            }
                //            else
                //            {
                //                break;
                //            }
                //        }
                //        picker_countsysname.SelectedIndex = countsysindex;
                //        IList<string> yarncountlenunitlist = picker_yarnlengthunit.Items;
                //        int yarncountlenindex = 0;
                //        foreach (string yclenunit in yarncountlenunitlist)
                //        {
                //            if (yclenunit != ycConfigList[0].yarnlenunit.ToString())
                //            {
                //                yarncountlenindex++;
                //            }
                //            else
                //            {
                //                break;
                //            }
                //        }
                //        picker_yarnlengthunit.SelectedIndex = yarncountlenindex;
                //        entry_sliverlength.Text = ycConfigList[0].sliverlength.ToString();
                //        entry_rovinglength.Text = ycConfigList[0].rovinglength.ToString();
                //        entry_testcount.Text = ycConfigList[0].testcount.ToString();
                //        entry_standardHank.Text = ycConfigList[0].standardHank.ToString();
                //        entry_testcountApercent.Text = ycConfigList[0].testcountApercent.ToString();
                //        entry_testcountStretch.Text = ycConfigList[0].testcountStretch.ToString();
                //        entry_testcountNoils.Text = ycConfigList[0].testcountNoils.ToString();
                //    }
                //    else
                //    {
                //        btn_save.Text = "Save";
                //    }
                //}
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred: " + ex.Message.ToString(), "OK");
            }
        }

        private void fetchConfig()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<YarnCountConfigModel>();
                    List<YarnCountConfigModel> ycConfigList = conn.Table<YarnCountConfigModel>().ToList();
                    if (ycConfigList.Count > 0)
                    {

                        YarnCountConfigModel SettingWithMachine = ycConfigList.Where(YarnCountConfigModel =>
                                                (YarnCountConfigModel.machineCategory != null || YarnCountConfigModel.machineCategory != "")).FirstOrDefault();
                        if (SettingWithMachine == null)
                        {
                            populateSettingsField(ycConfigList[0]);
                        }
                        else
                        {
                            if (selectedMachineCategory != null && selectedMachineCategory != "" && selectedMachineName != null
                                && selectedMachineName != "" && selectedMachineID != Guid.Empty)
                            {
                                YarnCountConfigModel machineSetting = ycConfigList.Where(YarnCountConfigModel =>
                                                (YarnCountConfigModel.machineCategory == selectedMachineCategory &&
                                                YarnCountConfigModel.machineID == selectedMachineID &&
                                                YarnCountConfigModel.machineName == selectedMachineName)).FirstOrDefault();
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
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred: " + ex.Message.ToString(), "OK");
            }
        }

        private void populateSettingsField(YarnCountConfigModel ycConfig)
        {
            if (ycConfig == null)
            {
                btn_save.Text = "Save";
                currentID = Guid.Empty;
                picker_countsysname.SelectedIndex = 0;
                picker_yarnlengthunit.SelectedIndex = 0;
                picker_leaLength.SelectedIndex = 0;
                entry_sliverlength.Text = "";
                entry_rovinglength.Text = "";
                entry_testcount.Text = "";
                entry_standardHank.Text = "";
                entry_hankDeviationPercent.Text = "";
                entry_testcountApercent.Text = "";
                entry_testcountStretch.Text = "";
                entry_testcountNoils.Text = "";
                picker_shiftCount.SelectedIndex = 0;
                toggleShift();
                return;
            }
            btn_save.Text = "Update";
            currentID = ycConfig.ID;

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


            IList<string> countsystemlist = picker_countsysname.Items;
            int countsysindex = 0;
            foreach (string countsystem in countsystemlist)
            {
                if (countsystem != ycConfig.countsysname.ToString())
                {
                    countsysindex++;
                }
                else
                {
                    break;
                }
            }
            picker_countsysname.SelectedIndex = countsysindex;
            IList<string> yarncountlenunitlist = picker_yarnlengthunit.Items;
            int yarncountlenindex = 0;
            foreach (string yclenunit in yarncountlenunitlist)
            {
                if (yclenunit != ycConfig.yarnlenunit.ToString())
                {
                    yarncountlenindex++;
                }
                else
                {
                    break;
                }
            }
            picker_yarnlengthunit.SelectedIndex = yarncountlenindex;
            if (ycConfig.lealength == 60)
            {
                picker_leaLength.SelectedIndex = 1;
            }
            else
            {
                picker_leaLength.SelectedIndex = 2;
            }
            entry_sliverlength.Text = ycConfig.sliverlength.ToString();
            entry_rovinglength.Text = ycConfig.rovinglength.ToString();
            entry_testcount.Text = ycConfig.testcount.ToString();
            entry_standardHank.Text = ycConfig.standardHank.ToString();
            entry_hankDeviationPercent.Text = ycConfig.deviationPercent.ToString();
            entry_testcountApercent.Text = ycConfig.testcountApercent.ToString();
            entry_testcountStretch.Text = ycConfig.testcountStretch.ToString();
            entry_testcountNoils.Text = ycConfig.testcountNoils.ToString();

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
                toggleShift();
            }



            if (ycConfig.shift1time != null && ycConfig.shift1time != "")
            {
                Shift1_timePicker.Time = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift1time).TotalHours);
            }

            if (ycConfig.shift2time != null && ycConfig.shift2time != "")
            {
                Shift2_timePicker.Time = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift2time).TotalHours);
            }
            if (ycConfig.shift3time != null && ycConfig.shift3time != "")
            {
                Shift3_timePicker.Time = TimeSpan.FromHours(TimeSpan.Parse(ycConfig.shift3time).TotalHours);
            }

        }

        private void btn_save_Clicked(object sender, EventArgs e)
        {
            try
            {
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

                if (picker_countsysname.SelectedItem.ToString() == "" ||
                    picker_yarnlengthunit.SelectedItem.ToString() == "" ||
                    entry_testcount.Text.Trim().ToString() == "" ||
                    entry_testcountApercent.Text.Trim().ToString() == "" ||
                    entry_testcountStretch.Text.Trim().ToString() == "" ||
                    entry_testcountNoils.Text.Trim().ToString() == "")
                {
                    DisplayAlert("Attention", "Please fill all fields with valid data to proceed!!!", "OK");
                    return;
                }

                if (selectedMachineCategory == "Spinning")
                {
                    if (picker_leaLength.SelectedIndex == -1 || picker_leaLength.SelectedIndex == 0)
                    {
                        DisplayAlert("Attention", "Lea length should not be blank!!!", "Ok");
                        return;
                    }
                }
                else
                {
                    if (entry_sliverlength.Text.Trim().ToString() == "" || entry_rovinglength.Text.Trim().ToString() == "")
                    {
                        DisplayAlert("Attention", "Sliver and Roving length should not be blank!!!", "Ok");
                        return;
                    }
                }

                if (entry_hankDeviationPercent.Text.Trim().Contains(".") || entry_hankDeviationPercent.Text.Trim().Contains("-"))
                {
                    DisplayAlert("Attention", "Deviation percent should not be a decimal or negative value!!!", "Ok");
                    return;
                }
                if (entry_hankDeviationPercent.Text.Trim() == "" || int.Parse(entry_hankDeviationPercent.Text.Trim()) == 0)
                {
                    DisplayAlert("Attention", "Deviation percent should not be blank or zero!!!", "Ok");
                    return;
                }

                if (int.Parse(entry_hankDeviationPercent.Text.ToString()) > 100)
                {
                    DisplayAlert("Attention", "Deviation percent should not greater than 100!!!", "Ok");
                    return;
                }

                if (entry_testcount.Text.Trim().Contains(".") || entry_testcount.Text.Trim().Contains("-"))
                {
                    DisplayAlert("Attention", "Test sample (Wrapping) should not be a decimal or negative value!!!", "Ok");
                    return;
                }
                if (entry_testcount.Text.Trim() == "" || int.Parse(entry_testcount.Text.Trim()) == 0)
                {
                    DisplayAlert("Attention", "Test sample (Wrapping) should not be blank or zero!!!", "Ok");
                    return;
                }

                if (entry_testcountApercent.Text.Trim().Contains(".") || entry_testcountApercent.Text.Trim().Contains("-"))
                {
                    DisplayAlert("Attention", "Test sample (A%) should not be a decimal or negative value!!!", "Ok");
                    return;
                }
                if (entry_testcountApercent.Text.Trim() == "" || int.Parse(entry_testcountApercent.Text.Trim()) == 0)
                {
                    DisplayAlert("Attention", "Test sample (A%) should not be blank or zero!!!", "Ok");
                    return;
                }

                if (entry_testcountStretch.Text.Trim().Contains(".") || entry_testcountStretch.Text.Trim().Contains("-"))
                {
                    DisplayAlert("Attention", "Test sample (Stretch) should not be a decimal or negative value!!!", "Ok");
                    return;
                }
                if (entry_testcountStretch.Text.Trim() == "" || int.Parse(entry_testcountStretch.Text.Trim()) == 0)
                {
                    DisplayAlert("Attention", "Test sample (Stretch) should not be blank or zero!!!", "Ok");
                    return;
                }

                if (entry_testcountNoils.Text.Trim().Contains(".") || entry_testcountNoils.Text.Trim().Contains("-"))
                {
                    DisplayAlert("Attention", "Test sample (Noils) should not be a decimal or negative value!!!", "Ok");
                    return;
                }
                if (entry_testcountNoils.Text.Trim() == "" || int.Parse(entry_testcountNoils.Text.Trim()) == 0)
                {
                    DisplayAlert("Attention", "Test sample (Noils) should not be blank or zero!!!", "Ok");
                    return;
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
                decimal stdHank = 0.0000m;
                if (entry_standardHank.Text.Trim().ToString() != "")
                {
                    stdHank = decimal.Parse(entry_standardHank.Text.ToString());
                }
                int enteredLeaLength = 0;
                int enteredSliverLength = 0;
                int enteredRovingLength = 0;
                if (selectedMachineCategory == "Spinning")
                {
                    enteredLeaLength = 120;
                    if (picker_leaLength.SelectedItem == "Half Lea")
                    {
                        enteredLeaLength = 60;
                    }
                }
                else
                {
                    enteredSliverLength = int.Parse(entry_sliverlength.Text.ToString());
                    enteredRovingLength = int.Parse(entry_rovinglength.Text.ToString());
                }
                YarnCountConfigModel yarnCountConfigModel = new YarnCountConfigModel()
                {
                    ID = guid,
                    machineCategory = selectedMachineCategory,
                    machineID = selectedMachineID,
                    machineName = selectedMachineName,
                    countsysname = picker_countsysname.SelectedItem.ToString(),
                    yarnlenunit = picker_yarnlengthunit.SelectedItem.ToString(),
                    lealength = enteredLeaLength,
                    sliverlength = enteredSliverLength,
                    rovinglength = enteredRovingLength,
                    testcount = int.Parse(entry_testcount.Text.ToString()),
                    standardHank = stdHank,
                    deviationPercent = int.Parse(entry_hankDeviationPercent.Text.ToString()),
                    testcountApercent = int.Parse(entry_testcountApercent.Text.ToString()),
                    testcountStretch = int.Parse(entry_testcountStretch.Text.ToString()),
                    testcountNoils = int.Parse(entry_testcountNoils.Text.ToString()),
                    shiftCount = int.Parse(picker_shiftCount.SelectedItem.ToString()),
                    shift1time = Shift1_timePicker.Time.Hours.ToString() + ":" + Shift1_timePicker.Time.Minutes.ToString(),
                    shift2time = Shift2_timePicker.Time.Hours.ToString() + ":" + Shift2_timePicker.Time.Minutes.ToString(),
                    shift3time = Shift3_timePicker.Time.Hours.ToString() + ":" + Shift3_timePicker.Time.Minutes.ToString()
                };

                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<YarnCountConfigModel>();
                    if (btn_save.Text == "Save")
                    {
                        row = conn.Insert(yarnCountConfigModel);
                    }
                    else
                    {
                        msg = "updated";
                        row = conn.Update(yarnCountConfigModel);
                    }
                    if (row > 0)
                    {
                        btn_save.Text = "Update";
                        DisplayAlert("Success", "Yarn count settings " + msg + " successfully!!!", "OK");
                    }
                    else
                    {
                        DisplayAlert("Failure", "Yarn count settings failed to be " + msg + "!!!", "OK");
                    }

                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred: " + ex.Message.ToString(), "OK");
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

        private void picker_machinecategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedMachineCategory = picker_machinecategory.SelectedItem.ToString();
                if (selectedMachineCategory == "" || selectedMachineCategory == null)
                {
                    picker_machinename.ItemsSource = null;
                }
                else
                {
                    using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                    {
                        conn.CreateTable<MachineModel>();
                        List<MachineModel> machineModelList = conn.Table<MachineModel>().Where(MachineModel => MachineModel.machineCategory == selectedMachineCategory).ToList();
                        picker_machinename.ItemsSource = machineModelList;
                    }
                    if (selectedMachineCategory == "Spinning")
                    {
                        lbl_lealength.IsVisible = true;
                        picker_leaLength.IsVisible = true;
                        lbl_sliverlength.IsVisible = false;
                        entry_sliverlength.IsVisible = false;
                        lbl_rovinglength.IsVisible = false;
                        entry_rovinglength.IsVisible = false;
                        lbl_standardHank.Text = "Standard Count (Wrapping)";
                        lbl_hankDeviation.Text = "Count Deviation %";
                    }
                    else
                    {
                        lbl_lealength.IsVisible = false;
                        picker_leaLength.IsVisible = false;
                        lbl_sliverlength.IsVisible = true;
                        entry_sliverlength.IsVisible = true;
                        lbl_rovinglength.IsVisible = true;
                        entry_rovinglength.IsVisible = true;
                        lbl_standardHank.Text = "Standard Hank (Wrapping)";
                        lbl_hankDeviation.Text = "Hank Deviation %";
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
                    conn.CreateTable<YarnCountConfigModel>();
                    List<YarnCountConfigModel> ycConfigList = conn.Table<YarnCountConfigModel>().ToList();
                    if (ycConfigList.Count > 0)
                    {
                        YarnCountConfigModel machineSetting = ycConfigList.Where(YarnCountConfigModel =>
                                                    (YarnCountConfigModel.machineCategory == selectedMachineCategory &&
                                                    YarnCountConfigModel.machineID == selectedMachineID &&
                                                    YarnCountConfigModel.machineName == selectedMachineName)).FirstOrDefault();
                        if (machineSetting != null)
                        {
                            populateSettingsField(machineSetting);
                        }
                        else
                        {
                            populateSettingsField(null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }

    }
}