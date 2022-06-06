using SQLite;
using System;
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
            InitializeComponent();
        }

        private void btn_save_Clicked(object sender, EventArgs e)
        {
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
                row = conn.Insert(yarnCountConfigModel);
                if (row > 0)
                {
                    DisplayAlert("Success", "Machine " + msg + " successfully!!!", "OK");
                }
                else
                {
                    DisplayAlert("Failure", "Machine failed to be " + msg + "!!!", "OK");
                }

            }
        }
    }
}