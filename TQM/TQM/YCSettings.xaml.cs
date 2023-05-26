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
                picker_countsysname.SelectedIndex = 0;
                picker_yarnlengthunit.SelectedIndex = 0;
                //picker_leaLength.SelectedIndex = 0;
                entry_leaLength.Text = "";
                entry_sliverlength.Text = "";
                entry_rovinglength.Text = "";
                entry_testcount.Text = "";
                entry_standardHank.Text = "";
                entry_hankDeviationPercent.Text = "";
                entry_testcountApercent.Text = "";
                entry_standardApercent.Text = "";
                entry_testcountStretch.Text = "";
                entry_standardStretch.Text = "";
                entry_testcountNoils.Text = "";
                entry_standardNoils.Text = "";
                entry_noilsRange.Text = "";
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
                picker_countsysname.SelectedIndex = 0;
                picker_yarnlengthunit.SelectedIndex = 0;
                //picker_leaLength.SelectedIndex = 0;
                entry_leaLength.Text = "";
                entry_sliverlength.Text = "";
                entry_rovinglength.Text = "";
                entry_testcount.Text = "";
                entry_standardHank.Text = "";
                entry_hankDeviationPercent.Text = "";
                entry_testcountApercent.Text = "";
                entry_standardApercent.Text = "";
                entry_testcountStretch.Text = "";
                entry_standardStretch.Text = "";
                entry_testcountNoils.Text = "";
                entry_standardNoils.Text = "";
                entry_noilsRange.Text = "";
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

                //lbl_userfield1.IsVisible = false;
                //lbl_userfield1.Text = "";
                //entry_userfield1.IsVisible = false;
                //entry_userfield1.Text = "";
                //lbl_userfield2.IsVisible = false;
                //lbl_userfield2.Text = "";
                //entry_userfield2.IsVisible = false;
                //entry_userfield2.Text = "";
                //lbl_userfield3.IsVisible = false;
                //lbl_userfield3.Text = "";
                //entry_userfield3.IsVisible = false;
                //entry_userfield3.Text = "";
                //lbl_userfield4.IsVisible = false;
                //lbl_userfield4.Text = "";
                //entry_userfield4.IsVisible = false;
                //entry_userfield4.Text = "";
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
            //if (ycConfig.lealength == 60)
            //{
            //    picker_leaLength.SelectedIndex = 1;
            //}
            //else
            //{
            //    picker_leaLength.SelectedIndex = 2;
            //}
            entry_leaLength.Text = ycConfig.lealength.ToString();
            entry_sliverlength.Text = ycConfig.sliverlength.ToString();
            entry_rovinglength.Text = ycConfig.rovinglength.ToString();
            entry_testcount.Text = ycConfig.testcount.ToString();
            entry_standardHank.Text = ycConfig.standardHank.ToString();
            entry_hankDeviationPercent.Text = ycConfig.deviationPercent.ToString();
            entry_testcountApercent.Text = ycConfig.testcountApercent.ToString();
            entry_standardApercent.Text = ycConfig.standardApercent.ToString();
            entry_testcountStretch.Text = ycConfig.testcountStretch.ToString();
            entry_standardStretch.Text = ycConfig.standardStretch.ToString();
            entry_testcountNoils.Text = ycConfig.testcountNoils.ToString();
            entry_standardNoils.Text = ycConfig.standardNoils.ToString();
            entry_noilsRange.Text = ycConfig.noilsRange.ToString();

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

                if (picker_countsysname.SelectedItem.ToString() == "" ||
                    picker_yarnlengthunit.SelectedItem.ToString() == "" ||
                    entry_testcount.Text.Trim().ToString() == "")
                {
                    DisplayAlert("Attention", "Please fill all fields with valid data to proceed!!!", "OK");
                    return;
                }

                if (selectedMachineCategory == "Spinning" || selectedMachineCategory == "Winding")
                {
                    entry_sliverlength.Text = "0";
                    entry_rovinglength.Text = "0";
                    entry_testcountApercent.Text = "0";
                    entry_standardApercent.Text = "0";
                    entry_testcountStretch.Text = "0";
                    entry_standardStretch.Text = "0";
                    entry_testcountNoils.Text = "0";
                    entry_standardNoils.Text = "0";
                    entry_noilsRange.Text = "0";
                    entry_standardApercent.Text = "0";


                    if (entry_leaLength.Text.Trim().Contains(".") || entry_leaLength.Text.Trim().Contains("-"))
                    {
                        DisplayAlert("Attention", "Lea length should not be a decimal or negative value!!!", "Ok");
                        return;
                    }

                    if (entry_leaLength.Text.Trim() == "" || int.Parse(entry_leaLength.Text.Trim()) == 0)
                    {
                        DisplayAlert("Attention", "Lea length should not be blank or zero!!!", "Ok");
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
                    if (entry_standardHank.Text.Trim() == "." || entry_standardHank.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Standard Count is invalid. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_standardHank.Text.Trim() == "" || decimal.Parse(entry_standardHank.Text.Trim()) <= 0m)
                    {
                        DisplayAlert("Attention", "Standard Count should not be blank or zero or negative!!!", "Ok");
                        return;
                    }
                    if (entry_hankDeviationPercent.Text.Trim() == "." || entry_hankDeviationPercent.Text.Trim().Contains("-"))
                    {
                        DisplayAlert("Attention", "Deviation percent should not be a decimal or negative value!!!", "Ok");
                        return;
                    }
                    if (entry_hankDeviationPercent.Text.Trim() == "" || decimal.Parse(entry_hankDeviationPercent.Text.Trim()) == 0m)
                    {
                        DisplayAlert("Attention", "Deviation percent should not be blank or zero!!!", "Ok");
                        return;
                    }


                }
                else if (selectedMachineCategory == "Carding" || selectedMachineCategory == "Breaker Drawing")
                {
                    entry_rovinglength.Text = "0";
                    entry_leaLength.Text = "0";
                    entry_testcountApercent.Text = "0";
                    entry_standardApercent.Text = "0";
                    entry_testcountStretch.Text = "0";
                    entry_standardStretch.Text = "0";
                    entry_testcountNoils.Text = "0";
                    entry_standardNoils.Text = "0";
                    entry_noilsRange.Text = "0";
                    entry_standardApercent.Text = "0";


                    if (entry_sliverlength.Text.Trim().ToString() == "")
                    {
                        DisplayAlert("Attention", "Sliver length should not be blank!!!", "Ok");
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
                    if (entry_standardHank.Text.Trim() == "." || entry_standardHank.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Standard Hank is invalid. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_standardHank.Text.Trim() == "" || decimal.Parse(entry_standardHank.Text.Trim()) <= 0m)
                    {
                        DisplayAlert("Attention", "Standard Hank should not be blank or zero or negative!!!", "Ok");
                        return;
                    }
                    if (entry_hankDeviationPercent.Text.Trim() == "." || entry_hankDeviationPercent.Text.Trim().Contains("-"))
                    {
                        DisplayAlert("Attention", "Deviation percent should not be a decimal or negative value!!!", "Ok");
                        return;
                    }
                    if (entry_hankDeviationPercent.Text.Trim() == "" || decimal.Parse(entry_hankDeviationPercent.Text.Trim()) == 0m)
                    {
                        DisplayAlert("Attention", "Deviation percent should not be blank or zero!!!", "Ok");
                        return;
                    }

                }
                else if (selectedMachineCategory == "Comber")
                {
                    entry_rovinglength.Text = "0";
                    entry_leaLength.Text = "0";
                    entry_testcountApercent.Text = "0";
                    entry_standardApercent.Text = "0";
                    entry_testcountStretch.Text = "0";
                    entry_standardStretch.Text = "0";

                    if (entry_sliverlength.Text.Trim().ToString() == "")
                    {
                        DisplayAlert("Attention", "Sliver length should not be blank!!!", "Ok");
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
                    if (entry_standardHank.Text.Trim() == "." || entry_standardHank.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Standard Hank is invalid. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_standardHank.Text.Trim() == "" || decimal.Parse(entry_standardHank.Text.Trim()) <= 0m)
                    {
                        DisplayAlert("Attention", "Standard Hank should not be blank or zero or negative!!!", "Ok");
                        return;
                    }
                    if (entry_hankDeviationPercent.Text.Trim() == "." || entry_hankDeviationPercent.Text.Trim().Contains("-"))
                    {
                        DisplayAlert("Attention", "Deviation percent should not be a decimal or negative value!!!", "Ok");
                        return;
                    }
                    if (entry_hankDeviationPercent.Text.Trim() == "" || decimal.Parse(entry_hankDeviationPercent.Text.Trim()) == 0m)
                    {
                        DisplayAlert("Attention", "Deviation percent should not be blank or zero!!!", "Ok");
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
                    if (entry_standardNoils.Text.Trim() == "." || entry_standardNoils.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Standard Noils % is invalid. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_standardNoils.Text.Trim() == "" || decimal.Parse(entry_standardNoils.Text.Trim()) <= 0m)
                    {
                        DisplayAlert("Attention", "Standard Noils % should not be blank or zero or negative!!!", "Ok");
                        return;
                    }
                    if (entry_noilsRange.Text.Trim() == "." || entry_noilsRange.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Noils Range is invalid. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_noilsRange.Text.Trim() == "" || decimal.Parse(entry_noilsRange.Text.Trim()) <= 0m)
                    {
                        DisplayAlert("Attention", "Noils Range should not be blank or zero or negative!!!", "Ok");
                        return;
                    }
                }
                else if (selectedMachineCategory == "Drawing")
                {
                    entry_rovinglength.Text = "0";
                    entry_leaLength.Text = "0";
                    entry_testcountStretch.Text = "0";
                    entry_standardStretch.Text = "0";
                    entry_testcountNoils.Text = "0";
                    entry_standardNoils.Text = "0";
                    entry_noilsRange.Text = "0";

                    if (entry_sliverlength.Text.Trim().ToString() == "")
                    {
                        DisplayAlert("Attention", "Sliver length should not be blank!!!", "Ok");
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
                    if (entry_standardHank.Text.Trim() == "." || entry_standardHank.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Standard Hank is invalid. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_standardHank.Text.Trim() == "" || decimal.Parse(entry_standardHank.Text.Trim()) <= 0m)
                    {
                        DisplayAlert("Attention", "Standard Hank should not be blank or zero or negative!!!", "Ok");
                        return;
                    }
                    if (entry_hankDeviationPercent.Text.Trim() == "." || entry_hankDeviationPercent.Text.Trim().Contains("-"))
                    {
                        DisplayAlert("Attention", "Deviation percent should not be a decimal or negative value!!!", "Ok");
                        return;
                    }
                    if (entry_hankDeviationPercent.Text.Trim() == "" || decimal.Parse(entry_hankDeviationPercent.Text.Trim()) == 0m)
                    {
                        DisplayAlert("Attention", "Deviation percent should not be blank or zero!!!", "Ok");
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
                    if (entry_standardApercent.Text.Trim() == "." || entry_standardApercent.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Standard A % is invalid. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_standardApercent.Text.Trim() == "" || decimal.Parse(entry_standardApercent.Text.Trim()) <= 0m)
                    {
                        DisplayAlert("Attention", "Standard A % should not be blank or zero or negative!!!", "Ok");
                        return;
                    }
                }
                else if (selectedMachineCategory == "Simplex/SpeedFrame")
                {
                    entry_sliverlength.Text = "0";
                    entry_leaLength.Text = "0";
                    entry_testcountApercent.Text = "0";
                    entry_testcountNoils.Text = "0";
                    entry_standardNoils.Text = "0";
                    entry_noilsRange.Text = "0";
                    entry_standardApercent.Text = "0";

                    if (entry_rovinglength.Text.Trim().ToString() == "")
                    {
                        DisplayAlert("Attention", "Roving length should not be blank!!!", "Ok");
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
                    if (entry_standardHank.Text.Trim() == "." || entry_standardHank.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Standard Hank is invalid. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_standardHank.Text.Trim() == "" || decimal.Parse(entry_standardHank.Text.Trim()) <= 0m)
                    {
                        DisplayAlert("Attention", "Standard Hank should not be blank or zero or negative!!!", "Ok");
                        return;
                    }
                    if (entry_hankDeviationPercent.Text.Trim() == "." || entry_hankDeviationPercent.Text.Trim().Contains("-"))
                    {
                        DisplayAlert("Attention", "Deviation percent should not be a decimal or negative value!!!", "Ok");
                        return;
                    }
                    if (entry_hankDeviationPercent.Text.Trim() == "" || decimal.Parse(entry_hankDeviationPercent.Text.Trim()) == 0m)
                    {
                        DisplayAlert("Attention", "Deviation percent should not be blank or zero!!!", "Ok");
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
                    if (entry_standardStretch.Text.Trim() == "." || entry_standardStretch.Text.Trim() == "-")
                    {
                        DisplayAlert("Attention", "Standard Stretch is invalid. Please check!!!", "Ok");
                        return;
                    }
                    if (entry_standardStretch.Text.Trim() == "" || decimal.Parse(entry_standardStretch.Text.Trim()) <= 0m)
                    {
                        DisplayAlert("Attention", "Standard Stretch should not be blank or zero or negative!!!", "Ok");
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
                decimal stdHank = 0.0000m;
                if (entry_standardHank.Text.Trim().ToString() != "")
                {
                    stdHank = decimal.Parse(entry_standardHank.Text.ToString());
                }
                decimal stdNoils = 0.0000m;
                if (entry_standardNoils.Text.Trim().ToString() != "")
                {
                    stdNoils = decimal.Parse(entry_standardNoils.Text.ToString());
                }
                decimal noilsRange = 0.0000m;
                if (entry_noilsRange.Text.Trim().ToString() != "")
                {
                    noilsRange = decimal.Parse(entry_noilsRange.Text.ToString());
                }
                decimal stdApercent = 0.0000m;
                if (entry_standardApercent.Text.Trim().ToString() != "")
                {
                    stdApercent = decimal.Parse(entry_standardApercent.Text.ToString());
                }
                decimal stdStretch = 0.0000m;
                if (entry_standardStretch.Text.Trim().ToString() != "")
                {
                    stdStretch = decimal.Parse(entry_standardStretch.Text.ToString());
                }
                int enteredLeaLength = 0;
                int enteredSliverLength = 0;
                int enteredRovingLength = 0;
                if (selectedMachineCategory == "Spinning" || selectedMachineCategory == "Winding")
                {
                    //enteredLeaLength = 120;
                    //if (picker_leaLength.SelectedItem == "Half Lea")
                    //{
                    //    enteredLeaLength = 60;
                    //}
                    enteredLeaLength = int.Parse(entry_leaLength.Text.ToString());
                }
                else
                {
                    enteredSliverLength = int.Parse(entry_sliverlength.Text.ToString());
                    enteredRovingLength = int.Parse(entry_rovinglength.Text.ToString());
                }

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
                    countsysname = picker_countsysname.SelectedItem.ToString(),
                    yarnlenunit = picker_yarnlengthunit.SelectedItem.ToString(),
                    lealength = enteredLeaLength,
                    sliverlength = enteredSliverLength,
                    rovinglength = enteredRovingLength,
                    testcount = int.Parse(entry_testcount.Text.ToString()),
                    standardHank = stdHank,
                    deviationPercent = decimal.Parse(entry_hankDeviationPercent.Text.ToString()),
                    testcountApercent = int.Parse(entry_testcountApercent.Text.ToString()),
                    standardApercent = stdApercent,
                    testcountStretch = int.Parse(entry_testcountStretch.Text.ToString()),
                    standardStretch = stdStretch,
                    testcountNoils = int.Parse(entry_testcountNoils.Text.ToString()),
                    standardNoils = stdNoils,
                    noilsRange = noilsRange,
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
                    if (selectedMachineCategory == "Spinning" || selectedMachineCategory == "Winding")
                    {
                        lbl_testCountApercent.IsVisible = false;
                        entry_testcountApercent.IsVisible = false;
                        lbl_testCountStretch.IsVisible = false;
                        entry_testcountStretch.IsVisible = false;
                        lbl_testCountNoils.IsVisible = false;
                        entry_testcountNoils.IsVisible = false;
                        lbl_sliverlength.IsVisible = false;
                        entry_sliverlength.IsVisible = false;
                        lbl_rovinglength.IsVisible = false;
                        entry_rovinglength.IsVisible = false;
                        lbl_standardNoils.IsVisible = false;
                        entry_standardNoils.IsVisible = false;
                        lbl_noilsRange.IsVisible = false;
                        entry_noilsRange.IsVisible = false;
                        lbl_standardApercent.IsVisible = false;
                        entry_standardApercent.IsVisible = false;
                        lbl_standardStretch.IsVisible = false;
                        entry_standardStretch.IsVisible = false;

                        lbl_lealength.IsVisible = true;
                        //picker_leaLength.IsVisible = true;
                        entry_leaLength.IsVisible = true;
                        lbl_standardHank.Text = "Standard Count (Wrapping)";
                        lbl_hankDeviation.Text = "Count Deviation ±";
                    }
                    else if (selectedMachineCategory == "Carding" || selectedMachineCategory == "Breaker Drawing")
                    {
                        lbl_rovinglength.IsVisible = false;
                        entry_rovinglength.IsVisible = false;
                        lbl_testCountApercent.IsVisible = false;
                        entry_testcountApercent.IsVisible = false;
                        lbl_testCountStretch.IsVisible = false;
                        entry_testcountStretch.IsVisible = false;
                        lbl_testCountNoils.IsVisible = false;
                        entry_testcountNoils.IsVisible = false;
                        lbl_lealength.IsVisible = false;
                        //picker_leaLength.IsVisible = false;
                        entry_leaLength.IsVisible = false;
                        lbl_standardNoils.IsVisible = false;
                        entry_standardNoils.IsVisible = false;
                        lbl_noilsRange.IsVisible = false;
                        entry_noilsRange.IsVisible = false;
                        lbl_standardApercent.IsVisible = false;
                        entry_standardApercent.IsVisible = false;
                        lbl_standardStretch.IsVisible = false;
                        entry_standardStretch.IsVisible = false;


                        lbl_sliverlength.IsVisible = true;
                        entry_sliverlength.IsVisible = true;
                        lbl_standardHank.Text = "Standard Hank (Wrapping)";
                        lbl_hankDeviation.Text = "Hank Deviation ±";
                    }
                    else if (selectedMachineCategory == "Comber")
                    {
                        lbl_rovinglength.IsVisible = false;
                        entry_rovinglength.IsVisible = false;
                        lbl_testCountApercent.IsVisible = false;
                        entry_testcountApercent.IsVisible = false;
                        lbl_testCountStretch.IsVisible = false;
                        entry_testcountStretch.IsVisible = false;
                        lbl_lealength.IsVisible = false;
                        //picker_leaLength.IsVisible = false;
                        entry_leaLength.IsVisible = false;
                        lbl_standardApercent.IsVisible = false;
                        entry_standardApercent.IsVisible = false;
                        lbl_standardStretch.IsVisible = false;
                        entry_standardStretch.IsVisible = false;

                        lbl_testCountNoils.IsVisible = true;
                        entry_testcountNoils.IsVisible = true;
                        lbl_sliverlength.IsVisible = true;
                        entry_sliverlength.IsVisible = true;
                        lbl_standardNoils.IsVisible = true;
                        entry_standardNoils.IsVisible = true;
                        lbl_noilsRange.IsVisible = true;
                        entry_noilsRange.IsVisible = true;


                        lbl_standardHank.Text = "Standard Hank (Wrapping)";
                        lbl_hankDeviation.Text = "Hank Deviation ±";
                    }
                    else if (selectedMachineCategory == "Drawing")
                    {
                        lbl_rovinglength.IsVisible = false;
                        entry_rovinglength.IsVisible = false;
                        lbl_testCountStretch.IsVisible = false;
                        entry_testcountStretch.IsVisible = false;
                        lbl_testCountNoils.IsVisible = false;
                        entry_testcountNoils.IsVisible = false;
                        lbl_lealength.IsVisible = false;
                        entry_leaLength.IsVisible = false;
                        lbl_standardNoils.IsVisible = false;
                        entry_standardNoils.IsVisible = false;
                        lbl_noilsRange.IsVisible = false;
                        entry_noilsRange.IsVisible = false;
                        lbl_standardStretch.IsVisible = false;
                        entry_standardStretch.IsVisible = false;

                        lbl_testCountApercent.IsVisible = true;
                        entry_testcountApercent.IsVisible = true;
                        lbl_sliverlength.IsVisible = true;
                        entry_sliverlength.IsVisible = true;
                        lbl_standardApercent.IsVisible = true;
                        entry_standardApercent.IsVisible = true;

                        lbl_standardHank.Text = "Standard Hank (Wrapping)";
                        lbl_hankDeviation.Text = "Hank Deviation ±";
                    }
                    else if (selectedMachineCategory == "Simplex/SpeedFrame")
                    {
                        lbl_sliverlength.IsVisible = false;
                        entry_sliverlength.IsVisible = false;
                        lbl_testCountApercent.IsVisible = false;
                        entry_testcountApercent.IsVisible = false;
                        lbl_testCountNoils.IsVisible = false;
                        entry_testcountNoils.IsVisible = false;
                        lbl_lealength.IsVisible = false;
                        entry_leaLength.IsVisible = false;
                        lbl_standardNoils.IsVisible = false;
                        entry_standardNoils.IsVisible = false;
                        lbl_noilsRange.IsVisible = false;
                        entry_noilsRange.IsVisible = false;
                        lbl_standardApercent.IsVisible = false;
                        entry_standardApercent.IsVisible = false;


                        lbl_testCountStretch.IsVisible = true;
                        entry_testcountStretch.IsVisible = true;
                        lbl_rovinglength.IsVisible = true;
                        entry_rovinglength.IsVisible = true;
                        lbl_standardStretch.IsVisible = true;
                        entry_standardStretch.IsVisible = true;
                        lbl_standardHank.Text = "Standard Hank (Wrapping)";
                        lbl_hankDeviation.Text = "Hank Deviation ±";
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
    }
}