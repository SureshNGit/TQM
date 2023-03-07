using SQLite;
using System;
using System.Collections.Generic;
using TQM.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class YCSettings : ContentPage
    {
        private Guid currentID = Guid.Empty;
        public YCSettings()
        {
            try
            {
                InitializeComponent();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<YarnCountConfigModel>();
                    List<YarnCountConfigModel> ycConfigList = conn.Table<YarnCountConfigModel>().ToList();
                    if (ycConfigList.Count > 0)
                    {
                        btn_save.Text = "Update";
                        currentID = ycConfigList[0].ID;
                        IList<string> countsystemlist = picker_countsysname.Items;
                        int countsysindex = 0;
                        foreach (string countsystem in countsystemlist)
                        {
                            if (countsystem != ycConfigList[0].countsysname.ToString())
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
                            if (yclenunit != ycConfigList[0].yarnlenunit.ToString())
                            {
                                yarncountlenindex++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        picker_yarnlengthunit.SelectedIndex = yarncountlenindex;
                        if (ycConfigList[0].yarnStrengthUnit != null)
                        {
                            IList<string> yarnstrengthlenunitlist = picker_yarnStrengthUnit.Items;
                            int yarnstrengthlenindex = 0;
                            foreach (string ycstrengthunit in yarnstrengthlenunitlist)
                            {
                                if (ycstrengthunit != ycConfigList[0].yarnStrengthUnit.ToString())
                                {
                                    yarnstrengthlenindex++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            picker_yarnStrengthUnit.SelectedIndex = yarnstrengthlenindex;
                        }
                        entry_yarnlength.Text = ycConfigList[0].yarnLength.ToString();
                        entry_testcount.Text = ycConfigList[0].testcount.ToString();
                        entry_standardHank.Text = ycConfigList[0].standardHank.ToString();
                        entry_standardCSP.Text = ycConfigList[0].standardCSP.ToString();
                        if (ycConfigList[0].shiftCount > 0)
                        {

                            IList<string> shiftCountList = picker_shiftCount.Items;
                            int shiftCountIndex = 0;
                            foreach (string count in shiftCountList)
                            {
                                if (count != ycConfigList[0].shiftCount.ToString())
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



                        if (ycConfigList[0].shift1time != null && ycConfigList[0].shift1time != "")
                        {
                            Shift1_timePicker.Time = TimeSpan.FromHours(TimeSpan.Parse(ycConfigList[0].shift1time).TotalHours);
                        }

                        if (ycConfigList[0].shift2time != null && ycConfigList[0].shift2time != "")
                        {
                            Shift2_timePicker.Time = TimeSpan.FromHours(TimeSpan.Parse(ycConfigList[0].shift2time).TotalHours);
                        }
                        if (ycConfigList[0].shift3time != null && ycConfigList[0].shift3time != "")
                        {
                            Shift3_timePicker.Time = TimeSpan.FromHours(TimeSpan.Parse(ycConfigList[0].shift3time).TotalHours);
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

        private void toggleShift()
        {
            if (picker_shiftCount.SelectedIndex != -1)
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

        private void btn_save_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (picker_countsysname.SelectedItem.ToString() == "" ||
                    picker_yarnlengthunit.SelectedItem.ToString() == "" ||
                    picker_yarnStrengthUnit.SelectedItem.ToString() == "" ||
                    entry_yarnlength.Text.Trim().ToString() == "" ||
                    entry_testcount.Text.Trim().ToString() == "" ||
                    entry_standardHank.Text.Trim().ToString() == "" ||
                    entry_standardCSP.Text.Trim().ToString() == "")

                {
                    DisplayAlert("Attention", "Please fill all fields with valid data to proceed!!!", "OK");
                    return;
                }

                if (picker_shiftCount.SelectedIndex == -1)
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
                int stdCSP = 0;
                if (entry_standardCSP.Text.Trim().ToString() != "")
                {
                    stdCSP = Convert.ToInt32(entry_standardCSP.Text.ToString());
                }
                YarnCountConfigModel yarnCountConfigModel = new YarnCountConfigModel()
                {
                    ID = guid,
                    countsysname = picker_countsysname.SelectedItem.ToString(),
                    yarnlenunit = picker_yarnlengthunit.SelectedItem.ToString(),
                    yarnStrengthUnit = picker_yarnStrengthUnit.SelectedItem.ToString(),
                    yarnLength = int.Parse(entry_yarnlength.Text.ToString()),
                    testcount = int.Parse(entry_testcount.Text.ToString()),
                    shiftCount = int.Parse(picker_shiftCount.SelectedItem.ToString()),
                    standardHank = stdHank,
                    standardCSP = stdCSP,
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
    }
}