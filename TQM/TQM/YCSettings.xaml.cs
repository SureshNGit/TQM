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
                        entry_yarnlength.Text = ycConfigList[0].yarnlength.ToString();
                        entry_testcount.Text = ycConfigList[0].testcount.ToString();
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

        private void btn_save_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (picker_countsysname.SelectedItem.ToString() == "" ||
                    picker_yarnlengthunit.SelectedItem.ToString() == "" ||
                    entry_yarnlength.Text.Trim().ToString() == "" ||
                    entry_testcount.Text.Trim().ToString() == "")
                {
                    DisplayAlert("Attention", "Please fill all fields with valid data to proceed!!!", "OK");
                    return;
                }
                Guid guid = Guid.NewGuid();
                if (currentID != Guid.Empty) { guid = currentID; }
                int row = 0;
                string msg = "saved";
                YarnCountConfigModel yarnCountConfigModel = new YarnCountConfigModel()
                {
                    ID = guid,
                    countsysname = picker_countsysname.SelectedItem.ToString(),
                    yarnlenunit = picker_yarnlengthunit.SelectedItem.ToString(),
                    yarnlength = int.Parse(entry_yarnlength.Text.ToString()),
                    testcount = int.Parse(entry_testcount.Text.ToString())
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
    }
}