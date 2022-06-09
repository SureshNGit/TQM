using SQLite;
using System.Collections.Generic;
using TQM.Model;
using TQM.ModelView;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class YCReport : ContentPage
    {

        private List<OverallReportModelView> _listOfReports;
        public List<OverallReportModelView> ListOfReport { get { return _listOfReports; } set { _listOfReports = value; base.OnPropertyChanged(); } }

        public YCReport()
        {
            InitializeComponent();
            List<OverallReportModelView> OVS = new List<OverallReportModelView>();
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<YCTestModel>();
                List<YCTestModel> allYCTest = conn.Table<YCTestModel>().ToList();
                List<YCTestSummaryModel> ycTestSummaryModels = conn.Table<YCTestSummaryModel>().ToList();

                foreach (YCTestSummaryModel testsummary in ycTestSummaryModels)
                {
                    OverallReportModelView report = new OverallReportModelView();
                    List<YCTestModel> yctestlist = conn.Table<YCTestModel>().Where(YCTestModel => YCTestModel.testID == testsummary.testID).ToList();
                    if (yctestlist != null)
                    {

                        foreach (YCTestModel test in yctestlist)
                        {
                            report.Add(test);
                        }
                        report.testID = testsummary.testID;
                        report.userName = testsummary.userName;
                        report.machineCategory = testsummary.machineCategory;
                        report.machineName = testsummary.machineName;
                        report.countsysname = testsummary.countsysname;
                        report.yarnlenunit = testsummary.yarnlenunit;
                        report.yarnlength = testsummary.yarnlength;
                        report.totaltestcount = testsummary.totaltestcount;
                        report.createdate = testsummary.createdate;
                        report.testaverage = testsummary.testaverage;
                        report.testsd = testsummary.testsd;
                        report.testcv = testsummary.testcv;
                    }
                    OVS.Add(report);
                }
                ListOfReport = OVS;
            }
            listview_tcreport.ItemsSource = null;
            listview_tcreport.ItemsSource = ListOfReport;
        }
    }
}