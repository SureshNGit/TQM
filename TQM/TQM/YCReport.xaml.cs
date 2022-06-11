using SQLite;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using TQM.Model;
using TQM.ModelView;
using TQM.SfPdfViewer;
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
                        endDate = startDate.AddDays(1);
                        ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                       YCTestSummaryModel.createdate >= startDate && YCTestSummaryModel.createdate < endDate).ToList();
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
                        report.apercent = testsummary.apercent;
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


        private void btn_saveToPDF_Clicked(object sender, EventArgs e)
        {
            try
            {
                PdfDocument pdfDocument = new PdfDocument();






                PdfPage pdfPage = pdfDocument.Pages.Add();
                PdfGrid pdfGrid = null;

                List<OverallReportModelView> overallReportList = (List<OverallReportModelView>)listview_tcreport.ItemsSource;
                foreach (OverallReportModelView orl in overallReportList)
                {

                    pdfGrid = new PdfGrid();
                    pdfGrid.ApplyBuiltinStyle(PdfGridBuiltinStyle.GridTable1LightAccent1);
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("Test No");
                    dataTable.Columns.Add("Weight");
                    dataTable.Columns.Add("Hank");
                    List<YCTestModel> testList = orl.yctestlist;
                    foreach (YCTestModel test in testList)
                    {




                        dataTable.Rows.Add(new object[] { test.testcount, test.yarnweight, test.yccalcval });




                    }
                    pdfGrid.DataSource = dataTable;
                    PdfGridLayoutFormat layoutFormat = new PdfGridLayoutFormat();
                    layoutFormat.Layout = PdfLayoutType.Paginate;
                    PdfLayoutResult result = pdfGrid.Draw(pdfPage, new PointF(10, 10), layoutFormat);
                };



                MemoryStream stream = new MemoryStream();
                pdfDocument.Save(stream);
                pdfDocument.Close(true);
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream);
                DisplayAlert("Notice", "PDF saved at [" + pdfPath + "]", "OK");
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error occurred!!! Error: " + ex.Message.ToString(), "OK");
            }
        }


    }
}