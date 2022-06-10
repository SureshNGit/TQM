using SQLite;
using System;
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
        }

        public YCReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID)
        {
            InitializeComponent();
            getReport(startDate, endDate, categoryName, machineID);
        }

        private void getReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID)
        {
            List<OverallReportModelView> OVS = new List<OverallReportModelView>();
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                //List<YCTestSummaryModel> ycTestSummaryModels = conn.Table<YCTestSummaryModel>().ToList();

                List<YCTestSummaryModel> ycTestSummaryModels = null;
                if (categoryName == null)
                {
                    if (startDate == endDate)
                    {
                        ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                       YCTestSummaryModel.createdate == startDate).ToList();
                    }
                    else
                    {
                        ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                       (YCTestSummaryModel.createdate >= startDate && YCTestSummaryModel.createdate <= endDate)).ToList();
                    }

                }
                else if (categoryName != null && machineID == Guid.Empty)
                {
                    ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                       (YCTestSummaryModel.createdate >= startDate && YCTestSummaryModel.createdate <= endDate
                       && YCTestSummaryModel.machineCategory == categoryName)).ToList();
                }
                else if (categoryName != null && machineID != Guid.Empty)
                {
                    ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                       (YCTestSummaryModel.createdate >= startDate && YCTestSummaryModel.createdate <= endDate
                       && YCTestSummaryModel.machineCategory == categoryName)
                       && YCTestSummaryModel.machineID == machineID).ToList();
                }

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