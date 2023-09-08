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
        private int currentShift = 0;
        private TimeSpan currentShift1 = TimeSpan.Zero;
        private TimeSpan currentShift2 = TimeSpan.Zero;
        private TimeSpan currentShift3 = TimeSpan.Zero;

        public YCSettings()
        {
            try
            {
                //using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                //{
                //    conn.DropTable<YarnCountConfigModel>();
                //}
                InitializeComponent();
                fetchConfig();
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred: " + ex.Message.ToString(), "OK");
            }
        }

        private void toggleUserField()
        {
            YarnCountConfigModel ycConfig_uf = null;
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<YarnCountConfigModel>();
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

        private void populateSettingsField(YarnCountConfigModel ycConfig, bool shiftAlone = false, bool isMacDiff = false)
        {
            toggleUserField();
            if (ycConfig == null)
            {
                btn_save.Text = "Save";
                btn_save.BackgroundColor = Color.Red;
                btn_save.TextColor = Color.White;
                currentID = Guid.Empty;
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
            }
            else
            {
                btn_save.Text = "Save";
                btn_save.BackgroundColor = Color.Red;
                btn_save.TextColor = Color.White;
                currentID = Guid.Empty;
            }

            if (isMacDiff == false)
            {
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

                YarnCountConfigModel yarnCountConfigModel = new YarnCountConfigModel()
                {
                    ID = guid,
                    machineCategory = selectedMachineCategory,
                    machineID = selectedMachineID,
                    machineName = selectedMachineName,
                    shiftCount = int.Parse(picker_shiftCount.SelectedItem.ToString()),
                    shift1time = shift1.Hours.ToString() + ":" + shift1.Minutes.ToString(),
                    shift2time = shift2.Hours.ToString() + ":" + shift2.Minutes.ToString(),
                    shift3time = shift3.Hours.ToString() + ":" + shift3.Minutes.ToString(),
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
                        int updatedRecCount = 0;
                        bool isShiftChanged = false;
                        List<YarnCountConfigModel> settingsList = conn.Table<YarnCountConfigModel>().ToList();
                        YarnCountConfigModel dbSettings = conn.Table<YarnCountConfigModel>().Where(YarnCountConfigModel =>
                                                            (YarnCountConfigModel.machineCategory == selectedMachineCategory &&
                                                            YarnCountConfigModel.machineName == selectedMachineName &&
                                                            YarnCountConfigModel.machineID == selectedMachineID)).FirstOrDefault();
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
                                    foreach (YarnCountConfigModel setting in settingsList)
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
                            machineSetting = ycConfigList.Where(YarnCountConfigModel =>
                                                    (YarnCountConfigModel.machineCategory == selectedMachineCategory))
                                                     .OrderByDescending(YarnCountConfigModel =>
                                                    (YarnCountConfigModel.createdate)).FirstOrDefault();
                            if (machineSetting != null)
                            {
                                populateSettingsField(machineSetting, false, true);
                            }
                            else
                            {
                                machineSetting = ycConfigList.Where(YarnCountConfigModel =>
                                                    (YarnCountConfigModel.machineCategory != "" &&
                                                    YarnCountConfigModel.machineCategory != null)).FirstOrDefault();
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
        private void btn_section1_Clicked(object sender, EventArgs e)
        {
            if (!frame_sec1.IsVisible)
            {
                frame_sec1.IsVisible = true;
                btn_section1.BackgroundColor = Color.Red;
            }
            else
            {
                frame_sec1.IsVisible = false;
                btn_section1.BackgroundColor = Color.FromHex("#0e0273");
            }
        }

        private void btn_section2_Clicked(object sender, EventArgs e)
        {
            if (!frame_sec2.IsVisible)
            {
                frame_sec2.IsVisible = true;
                btn_section2.BackgroundColor = Color.Red;
            }
            else
            {
                frame_sec2.IsVisible = false;
                btn_section2.BackgroundColor = Color.FromHex("#0e0273");
            }
        }
        private void btn_section3_Clicked(object sender, EventArgs e)
        {
            if (!frame_sec3.IsVisible)
            {
                frame_sec3.IsVisible = true;
                btn_section3.BackgroundColor = Color.Red;
            }
            else
            {
                frame_sec3.IsVisible = false;
                btn_section3.BackgroundColor = Color.FromHex("#0e0273");
            }
        }

        private void picker_sectionCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (picker_sectionCount.SelectedIndex > 0)
            {
                int totalDrumCount;
                int.TryParse(entry_drumCount.Text,out totalDrumCount);
                if (totalDrumCount == 0)
                {
                    DisplayAlert("Attention", "Total drum count should not be Zero!!!", "OK");
                    picker_sectionCount.SelectedIndex = 0;
                    return;
                }
                if (picker_sectionCount.SelectedItem.ToString() == "1")
                {
                    btn_section1.IsVisible = true;
                    btn_section2.IsVisible = false;
                    btn_section3.IsVisible = false;
                    btn_section2.BackgroundColor = Color.FromHex("#0e0273");
                    btn_section3.BackgroundColor = Color.FromHex("#0e0273");
                    frame_sec2.IsVisible = false;
                    frame_sec3.IsVisible = false;
                    entry_section1Drums_Sec1.Text = "1."+totalDrumCount.ToString();
                    entry_section1Drums_Sec1.IsEnabled = false;
                }
                if (picker_sectionCount.SelectedItem.ToString() == "2")
                {
                    btn_section1.IsVisible = true;
                    btn_section2.IsVisible = true;
                    btn_section3.IsVisible = false;
                    btn_section3.BackgroundColor = Color.FromHex("#0e0273");
                    frame_sec3.IsVisible = false;

                    int reminder = totalDrumCount % 2;
                    int equalPortion = (totalDrumCount - reminder) / 2;

                    entry_section1Drums_Sec1.Text = "1."+equalPortion.ToString();
                    entry_section1Drums_Sec1.IsEnabled = true;
                    entry_section1Drums_Sec2.Text = (equalPortion + 1).ToString() + "." + totalDrumCount;

                }
                if (picker_sectionCount.SelectedItem.ToString() == "3")
                {
                    btn_section1.IsVisible = true;
                    btn_section2.IsVisible = true;
                    btn_section3.IsVisible = true;

                    int reminder = totalDrumCount % 3;
                    int equalPortion = (totalDrumCount - reminder) / 3;

                    entry_section1Drums_Sec1.Text = "1." + equalPortion.ToString();
                    entry_section1Drums_Sec1.IsEnabled = true;
                    entry_section1Drums_Sec2.Text = (equalPortion + 1).ToString() + "." + (equalPortion*2);
                    entry_section1Drums_Sec3.Text = ((equalPortion*2) + 1).ToString() + "." + totalDrumCount;
                }
            }
            else
            {
                btn_section1.IsVisible = false;
                btn_section2.IsVisible = false;
                btn_section3.IsVisible = false;
                btn_section1.BackgroundColor = Color.FromHex("#0e0273");
                btn_section2.BackgroundColor = Color.FromHex("#0e0273");
                btn_section3.BackgroundColor = Color.FromHex("#0e0273");
                frame_sec1.IsVisible = false;
                frame_sec2.IsVisible = false;
                frame_sec3.IsVisible = false;

                entry_section1Drums_Sec1.Text = "0";
                entry_section1Drums_Sec1.IsEnabled = true;
                entry_section1Drums_Sec2.Text = "0";
                entry_section1Drums_Sec3.Text = "0";
            }
        }

        private void date_scheduledDayLimitDate_Sec3_DateSelected(object sender, DateChangedEventArgs e)
        {
            DateTime today = DateTime.Today;
            DateTime selectedDate = date_scheduledDayLimitDate_Sec3.Date;
            int dayDiff = DateTime.Compare(today, selectedDate);
            if (dayDiff == 0)
            {
                entry_scheduledDayLimit_Sec3.Text = "1";
            }
        }
    }
}