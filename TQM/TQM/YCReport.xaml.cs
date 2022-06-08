using SQLite;
using System.Collections.Generic;
using TQM.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class YCReport : ContentPage
    {
        public YCReport()
        {
            InitializeComponent();
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<YCTestModel>();
                List<YCTestModel> allYCTest = conn.Table<YCTestModel>().ToList();
                listview_tcreport.ItemsSource = allYCTest;
            }
        }
    }
}