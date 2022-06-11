using SQLite;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
        private string selectedCompanyName = null;
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


                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<CompanyModel>();
                    List<CompanyModel> companieslist = conn.Table<CompanyModel>().ToList();
                    selectedCompanyName = companieslist[0].Name;
                };


                PdfPage pdfPage = pdfDocument.Pages.Add();
                PdfGrid pdfGrid = null;
                PdfGridLayoutFormat layoutFormat = new PdfGridLayoutFormat();
                layoutFormat.Layout = PdfLayoutType.Paginate;
                List<OverallReportModelView> overallReportList = (List<OverallReportModelView>)listview_tcreport.ItemsSource;
                PdfLayoutResult result = null;
                PdfLayoutResult resultInfo = null;
                float overallHeight = 0;
                int tableNo = 1;
                foreach (OverallReportModelView orl in overallReportList)
                {

                    PdfGrid pdfGridInfo = new PdfGrid();
                    pdfGridInfo.Columns.Add(4);
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();

                    if (tableNo == 1)
                    {
                        pdfGridInfo.Rows[0].Cells[0].Value = selectedCompanyName;
                        pdfGridInfo.Rows[0].Cells[0].ColumnSpan = 4;
                        pdfGridInfo.Rows[0].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGridInfo.Rows[0].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGridInfo.Rows[0].Cells[0].Style.BackgroundBrush = PdfBrushes.Blue;
                        pdfGridInfo.Rows[0].Cells[0].Style.TextPen = PdfPens.White;
                        pdfGridInfo.Rows[0].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 16);
                    }
                    pdfGridInfo.Rows[1].Cells[0].Value = "Test ID: " + orl.testID;
                    pdfGridInfo.Rows[1].Cells[1].Value = "Tester: " + orl.userName;
                    pdfGridInfo.Rows[1].Cells[2].Value = "Machine Category: " + orl.machineCategory;
                    pdfGridInfo.Rows[1].Cells[3].Value = "Machine Name: " + orl.machineName;
                    pdfGridInfo.Rows[2].Cells[0].Value = "Count System: " + orl.countsysname;
                    pdfGridInfo.Rows[2].Cells[1].Value = "Length Unit: " + orl.yarnlenunit;
                    pdfGridInfo.Rows[2].Cells[2].Value = "Length: " + orl.yarnlength;
                    pdfGridInfo.Rows[2].Cells[3].Value = "Total Test: " + orl.totaltestcount;
                    pdfGridInfo.Rows[3].Cells[0].Value = "Average: " + orl.testaverage;
                    pdfGridInfo.Rows[3].Cells[1].Value = "SD: " + orl.testsd;
                    pdfGridInfo.Rows[3].Cells[2].Value = "CV: " + orl.testcv;
                    pdfGridInfo.Rows[3].Cells[3].Value = "A%: " + orl.apercent;
                    pdfGridInfo.Rows[4].Cells[0].Value = "Date: " + orl.createdate;

                    pdfGridInfo.Rows[0].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[0].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[0].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[0].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[3].Style.Borders.All = PdfPens.Transparent;

                    resultInfo = pdfGridInfo.Draw(pdfPage, new PointF(10, 10), layoutFormat);

                    pdfGrid = new PdfGrid();
                    //pdfGrid.ApplyBuiltinStyle(PdfGridBuiltinStyle.GridTable1LightAccent1);

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

                    if (result == null)
                    {
                        result = pdfGrid.Draw(pdfPage, new PointF(10, resultInfo.Bounds.Height + 10), layoutFormat);
                        overallHeight = result.Bounds.Height + 30;
                    }
                    else
                    {
                        if (overallHeight == 0)
                        {
                            result = pdfGrid.Draw(pdfPage, new PointF(10, resultInfo.Bounds.Height + 10), layoutFormat);
                            overallHeight = result.Bounds.Height + 30;
                        }
                        else
                        {
                            int prevPageCount = result.Page.Section.Pages.Count;
                            result = pdfGrid.Draw(result.Page, new PointF(10, (overallHeight)));
                            if (prevPageCount < result.Page.Section.Pages.Count)
                            {
                                overallHeight = 0;
                                pdfPage = result.Page;
                            }

                        }
                        overallHeight = overallHeight + result.Bounds.Height + 30;
                    }

                    Debug.WriteLine("Page Count ===>" + pdfPage.Section.Pages.Count);
                    Debug.WriteLine("Table NO==>" + tableNo + " ,tableHeigth ===>" + overallHeight);
                    tableNo++;
                };



                MemoryStream stream = new MemoryStream();
                pdfDocument.Save(stream);
                pdfDocument.Close(true);
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream);
                DisplayAlert("Notice", "PDF saved at [" + pdfPath + "]", "OK");
                //Process.Start(pdfPath);
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error occurred!!! Error: " + ex.Message.ToString(), "OK");
            }
        }


    }
}