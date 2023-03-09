using RestSharp;
using SQLite;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TQM.Model;
using TQM.ModelView;
using TQM.SfPdfViewer;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Color = Xamarin.Forms.Color;
using Exception = Java.Lang.Exception;
using String = System.String;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class YCReportWithCSP : ContentPage
    {

        private List<OverallCountStrengthReportModelView> _listOfReports;
        public List<OverallCountStrengthReportModelView> ListOfReport { get { return _listOfReports; } set { _listOfReports = value; base.OnPropertyChanged(); } }
        private string selectedCompanyName = null;
        private const string BLUE = "#0e0273";
        private RunConfiguration runConfiguration = new RunConfiguration();
        private List<YCStrengthTestSummaryModel> deleteList = null;
        private List<YCStrengthTestModel> partialDeleteList = null;
        private bool deleteAll = false;
        private int TOT_TEST = 0;
        private decimal CON_CSP = 0.0000m;
        private decimal CON_STD_DEV = 0.0000m;
        private decimal CON_CV = 0.0000m;
        private bool consolidatedReport = false;
        private bool hasPartialTest = false;
        private DateTime reportStartDate;
        private DateTime reportEndDate;

        public YCReportWithCSP()
        {
            InitializeComponent();
        }

        public YCReportWithCSP(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string process, string testID, bool deleteRequest, bool isConsolidated)
        {
            InitializeComponent();
            consolidatedReport = isConsolidated;
            if (consolidatedReport)
            {
                lbl_reportHeader.Text = "Consolidated CSP Report";
            }
            else
            {
                lbl_reportHeader.Text = "CSP Report";
            }
            if (deleteRequest)
            {
                btn_saveToPDF.Text = "Send & Delete Records";
                btn_saveToPDF.BackgroundColor = Color.Red;
                btn_saveToPDF.TextColor = Color.White;
            }
            reportStartDate = startDate;
            reportEndDate = endDate;
            getReport(startDate, endDate, categoryName, machineID, shift, process, testID, deleteRequest);
        }

        private void getReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string process, string testID, bool deleteRequest)
        {
            try
            {
                decimal stdHank = 0.000m;
                List<OverallCountStrengthReportModelView> OVS = new List<OverallCountStrengthReportModelView>();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<YCStrengthTestModel>();
                    conn.CreateTable<YCStrengthTestSummaryModel>();

                    List<YCStrengthTestSummaryModel> summaryModels = null;
                    if (categoryName == null || categoryName == "")
                    {
                        endDate = endDate.AddDays(1);

                        if (shift != "" && process != null)
                        {
                            summaryModels = conn.Table<YCStrengthTestSummaryModel>().Where(YCStrengthTestSummaryModel =>
                                                (YCStrengthTestSummaryModel.createdate >= startDate
                                                && YCStrengthTestSummaryModel.createdate < endDate
                                                && YCStrengthTestSummaryModel.shift == shift
                                                && YCStrengthTestSummaryModel.process.ToLower() == process.ToLower())).ToList();
                        }
                        else if (shift == "" && process != null)
                        {
                            summaryModels = conn.Table<YCStrengthTestSummaryModel>().Where(YCStrengthTestSummaryModel =>
                                                (YCStrengthTestSummaryModel.createdate >= startDate
                                                && YCStrengthTestSummaryModel.createdate < endDate
                                                && YCStrengthTestSummaryModel.process.ToLower() == process.ToLower())).ToList();
                        }
                        else if (shift != "" && process == null)
                        {
                            summaryModels = conn.Table<YCStrengthTestSummaryModel>().Where(YCStrengthTestSummaryModel =>
                                                (YCStrengthTestSummaryModel.createdate >= startDate
                                                && YCStrengthTestSummaryModel.createdate < endDate
                                                && YCStrengthTestSummaryModel.shift == shift)).ToList();
                        }
                        else if (shift == "" && process == null)
                        {
                            summaryModels = conn.Table<YCStrengthTestSummaryModel>().Where(YCStrengthTestSummaryModel =>
                                                (YCStrengthTestSummaryModel.createdate >= startDate
                                                && YCStrengthTestSummaryModel.createdate < endDate)).ToList();
                        }

                    }
                    else if (categoryName != null && machineID == Guid.Empty)
                    {
                        endDate = endDate.AddDays(1);

                        if (shift != "" && process != null)
                        {
                            summaryModels = conn.Table<YCStrengthTestSummaryModel>().Where(YCStrengthTestSummaryModel =>
                                                 (YCStrengthTestSummaryModel.createdate >= startDate
                                                 && YCStrengthTestSummaryModel.createdate < endDate
                                                 && YCStrengthTestSummaryModel.machineCategory == categoryName
                                                 && YCStrengthTestSummaryModel.shift == shift
                                                 && YCStrengthTestSummaryModel.process.ToLower() == process.ToLower())).ToList();
                        }
                        else if (shift == "" && process != null)
                        {
                            summaryModels = conn.Table<YCStrengthTestSummaryModel>().Where(YCStrengthTestSummaryModel =>
                                                 (YCStrengthTestSummaryModel.createdate >= startDate
                                                 && YCStrengthTestSummaryModel.createdate < endDate
                                                 && YCStrengthTestSummaryModel.machineCategory == categoryName
                                                 && YCStrengthTestSummaryModel.process.ToLower() == process.ToLower())).ToList();
                        }
                        else if (shift != "" && process == null)
                        {
                            summaryModels = conn.Table<YCStrengthTestSummaryModel>().Where(YCStrengthTestSummaryModel =>
                                                 (YCStrengthTestSummaryModel.createdate >= startDate
                                                 && YCStrengthTestSummaryModel.createdate < endDate
                                                 && YCStrengthTestSummaryModel.machineCategory == categoryName
                                                 && YCStrengthTestSummaryModel.shift == shift)).ToList();
                        }
                        else if (shift == "" && process == null)
                        {
                            summaryModels = conn.Table<YCStrengthTestSummaryModel>().Where(YCStrengthTestSummaryModel =>
                                                (YCStrengthTestSummaryModel.createdate >= startDate
                                                && YCStrengthTestSummaryModel.createdate < endDate
                                                && YCStrengthTestSummaryModel.machineCategory == categoryName)).ToList();
                        }


                    }
                    else if (categoryName != null && machineID != Guid.Empty)
                    {
                        endDate = endDate.AddDays(1);

                        if (shift != "" && process != null)
                        {
                            summaryModels = conn.Table<YCStrengthTestSummaryModel>().Where(YCStrengthTestSummaryModel =>
                                                    (YCStrengthTestSummaryModel.createdate >= startDate
                                                    && YCStrengthTestSummaryModel.createdate < endDate
                                                    && YCStrengthTestSummaryModel.machineCategory == categoryName)
                                                    && YCStrengthTestSummaryModel.machineID == machineID
                                                    && YCStrengthTestSummaryModel.shift == shift
                                                    && YCStrengthTestSummaryModel.process.ToLower() == process.ToLower()).ToList();
                        }
                        else if (shift == "" && process != null)
                        {
                            summaryModels = conn.Table<YCStrengthTestSummaryModel>().Where(YCStrengthTestSummaryModel =>
                                                    (YCStrengthTestSummaryModel.createdate >= startDate
                                                    && YCStrengthTestSummaryModel.createdate < endDate
                                                    && YCStrengthTestSummaryModel.machineCategory == categoryName)
                                                    && YCStrengthTestSummaryModel.machineID == machineID
                                                    && YCStrengthTestSummaryModel.process.ToLower() == process.ToLower()).ToList();
                        }
                        else if (shift != "" && process == null)
                        {
                            summaryModels = conn.Table<YCStrengthTestSummaryModel>().Where(YCStrengthTestSummaryModel =>
                                                    (YCStrengthTestSummaryModel.createdate >= startDate
                                                    && YCStrengthTestSummaryModel.createdate < endDate
                                                    && YCStrengthTestSummaryModel.machineCategory == categoryName)
                                                    && YCStrengthTestSummaryModel.machineID == machineID
                                                    && YCStrengthTestSummaryModel.shift == shift).ToList();
                        }
                        else if (shift == "" && process == null)
                        {
                            summaryModels = conn.Table<YCStrengthTestSummaryModel>().Where(YCStrengthTestSummaryModel =>
                                                    (YCStrengthTestSummaryModel.createdate >= startDate
                                                    && YCStrengthTestSummaryModel.createdate < endDate
                                                    && YCStrengthTestSummaryModel.machineCategory == categoryName)
                                                    && YCStrengthTestSummaryModel.machineID == machineID).ToList();
                        }

                    }
                    List<YCStrengthTestModel> partialTest = null;
                    hasPartialTest = false;
                    if (summaryModels.Count == 0)
                    {
                        if (testID != "" && summaryModels.Count == 0)
                        {
                            long idForDelete = long.Parse(testID);
                            partialTest = conn.Table<YCStrengthTestModel>().
                                                            Where(YCStrengthTestModel => YCStrengthTestModel.testID == idForDelete).ToList();
                        }
                        if (partialTest.Count > 0 && (partialTest.Count != partialTest[0].totaltestcount))
                        {
                            hasPartialTest = true;
                        }
                        else
                        {
                            DisplayAlert("Notice", "No records to display!!!", "OK");
                            return;
                        }
                    }
                    else
                    {
                        if (testID != "")
                        {
                            summaryModels = summaryModels.Where(t => t.testID == long.Parse(testID)).ToList();

                            long idForDelete = long.Parse(testID);
                            partialTest = conn.Table<YCStrengthTestModel>().
                                          Where(YCStrengthTestModel => YCStrengthTestModel.testID == idForDelete).ToList();

                            if (partialTest.Count > 0 && (partialTest.Count != partialTest[0].totaltestcount))
                            {
                                hasPartialTest = true;
                            }

                            else if (summaryModels.Count == 0 && partialTest.Count > 0 && (partialTest.Count == partialTest[0].totaltestcount))
                            {
                                hasPartialTest = true;
                            }
                            else
                            {
                                if (summaryModels.Count == 0 && partialTest.Count == 0)
                                {
                                    DisplayAlert("Notice", "No records to display!!!", "OK");
                                    return;
                                }
                            }
                        }
                        if (deleteRequest && hasPartialTest != true)
                        {
                            deleteAll = true;
                            deleteList = summaryModels;
                        }
                        else if (deleteRequest && hasPartialTest)
                        {
                            deleteAll = true;
                            partialDeleteList = partialTest;
                        }
                    }

                    CON_CSP = 0.0000m;
                    CON_STD_DEV = 0.0000m;
                    CON_CV = 0.0000m;

                    if (!hasPartialTest)
                    {

                        TOT_TEST = summaryModels.Count;

                        foreach (YCStrengthTestSummaryModel testsummary in summaryModels)
                        {
                            OverallCountStrengthReportModelView report = new OverallCountStrengthReportModelView();
                            List<YCStrengthTestModel> yctestlist = conn.Table<YCStrengthTestModel>().Where(YCStrengthTestModel => YCStrengthTestModel.testID == testsummary.testID).ToList();
                            if (yctestlist != null)
                            {
                                if (consolidatedReport)
                                {
                                    CON_CSP = CON_CSP + formatDecimal(testsummary.avgCSP);
                                    CON_STD_DEV = CON_STD_DEV + formatDecimal(testsummary.sdCSP);
                                    CON_CV = CON_CV + formatDecimal(testsummary.cvCSP);
                                }

                                YCStrengthTestReportModelView reportView = new YCStrengthTestReportModelView()
                                {
                                    testDescription = "",
                                    count = yctestlist[0].countsysname,
                                    strength = yctestlist[0].yarnstrengthunit,
                                    CSP = ""
                                };

                                report.Add(reportView);

                                foreach (YCStrengthTestModel test in yctestlist)
                                {
                                    reportView = new YCStrengthTestReportModelView()
                                    {
                                        testDescription = test.testcount.ToString(),
                                        count = formatDecimal(test.yccalcval).ToString(),
                                        strength = formatDecimal(test.yarnstrength).ToString(),
                                        CSP = test.CSP.ToString()
                                    };
                                    report.Add(reportView);
                                }

                                reportView = new YCStrengthTestReportModelView()
                                {
                                    testDescription = "AVG",
                                    count = formatDecimal(testsummary.testaverage).ToString(),
                                    strength = formatDecimal(testsummary.avgStrength).ToString(),
                                    CSP = Convert.ToInt32(testsummary.avgCSP).ToString()
                                };
                                report.Add(reportView);

                                reportView = new YCStrengthTestReportModelView()
                                {
                                    testDescription = "SD",
                                    count = formatDecimal(testsummary.testsd).ToString(),
                                    strength = formatDecimal(testsummary.sdStrength).ToString(),
                                    CSP = Math.Round(testsummary.sdCSP, 1).ToString()
                                };
                                report.Add(reportView);

                                reportView = new YCStrengthTestReportModelView()
                                {
                                    testDescription = "CV",
                                    count = formatDecimal(testsummary.testcv).ToString(),
                                    strength = formatDecimal(testsummary.cvStrength).ToString(),
                                    CSP = Math.Round(testsummary.cvCSP, 1).ToString()
                                };
                                report.Add(reportView);

                                reportView = new YCStrengthTestReportModelView()
                                {
                                    testDescription = "MIN",
                                    count = formatDecimal(testsummary.testMin).ToString(),
                                    strength = formatDecimal(testsummary.StrengthMin).ToString(),
                                    CSP = Convert.ToInt32(testsummary.CSPMin).ToString()
                                };
                                report.Add(reportView);

                                reportView = new YCStrengthTestReportModelView()
                                {
                                    testDescription = "MAX",
                                    count = formatDecimal(testsummary.testMax).ToString(),
                                    strength = formatDecimal(testsummary.StrengthMax).ToString(),
                                    CSP = Convert.ToInt32(testsummary.CSPMax).ToString()
                                };
                                report.Add(reportView);

                                reportView = new YCStrengthTestReportModelView()
                                {
                                    testDescription = "RANGE",
                                    count = formatDecimal(testsummary.testRange).ToString(),
                                    strength = formatDecimal(testsummary.StrengthRange).ToString(),
                                    CSP = Convert.ToInt32(testsummary.CSPRange).ToString()
                                };
                                report.Add(reportView);

                                report.testID = testsummary.testID;
                                report.userName = testsummary.userName;
                                report.machineCategory = testsummary.machineCategory;
                                report.machineName = testsummary.machineName;
                                report.shift = testsummary.shift;
                                report.process = testsummary.process;
                                report.countsysname = testsummary.countsysname;
                                report.yarnlenunit = testsummary.yarnlenunit;
                                report.yarnstrengthunit = testsummary.yarnstrengthunit;
                                report.yarnlength = testsummary.yarnlength;
                                report.totaltestcount = testsummary.totaltestcount;
                                report.createdate = testsummary.createdate;
                                report.testRemark = testsummary.testRemark;
                                report.testaverage = formatDecimal(testsummary.avgCSP);
                                report.testsd = formatDecimal(testsummary.sdCSP);
                                report.testcv = formatDecimal(testsummary.cvCSP);
                                report.standardCSP = testsummary.standardCSP;
                            }
                            OVS.Add(report);
                        }
                        CON_CSP = formatDecimal(CON_CSP / TOT_TEST);
                        CON_STD_DEV = formatDecimal(CON_STD_DEV / TOT_TEST);
                        CON_CV = formatDecimal(CON_CV / TOT_TEST);

                    }
                    else
                    {
                        OverallCountStrengthReportModelView report = new OverallCountStrengthReportModelView();

                        YCStrengthTestReportModelView reportView = new YCStrengthTestReportModelView()
                        {
                            testDescription = "",
                            count = "grams",
                            strength = partialTest[0].yarnstrengthunit,
                            CSP = ""
                        };

                        report.Add(reportView);

                        foreach (YCStrengthTestModel test in partialTest)
                        {
                            reportView = new YCStrengthTestReportModelView()
                            {
                                testDescription = test.testcount.ToString(),
                                count = formatDecimal(test.yccalcval).ToString(),
                                strength = formatDecimal(test.yarnstrength).ToString(),
                                CSP = test.CSP.ToString()
                            };
                            report.Add(reportView);

                        }
                        report.testID = partialTest[0].testID;
                        report.userName = partialTest[0].userName;
                        report.machineCategory = partialTest[0].machineCategory;
                        report.machineName = partialTest[0].machineName;
                        report.shift = partialTest[0].shift;
                        report.process = partialTest[0].process;
                        report.countsysname = partialTest[0].countsysname;
                        report.yarnlenunit = partialTest[0].yarnlenunit;
                        report.yarnstrengthunit = partialTest[0].yarnstrengthunit;
                        report.yarnlength = partialTest[0].yarnlength;
                        report.totaltestcount = partialTest[0].totaltestcount;
                        report.createdate = partialTest[0].createdate;
                        report.testRemark = "";
                        report.testaverage = 0.000m;
                        report.testsd = 0.000m;
                        report.testcv = 0.000m;
                        report.standardCSP = partialTest[0].standardCSP;

                        OVS.Add(report);

                    }
                    ListOfReport = OVS;
                    if (hasPartialTest && deleteRequest)
                    {
                        btn_saveToPDF.Text = "Delete Improper Test";
                    }
                    else if (hasPartialTest)
                    {
                        btn_saveToPDF.IsVisible = false;
                    }
                }
                listview_tcreport.ItemsSource = null;
                listview_tcreport.ItemsSource = ListOfReport;
                if (consolidatedReport)
                {
                    lbl_totalTest.Text = TOT_TEST.ToString();
                    lbl_AvgHank.Text = CON_CSP.ToString();
                    lbl_AvgSD.Text = CON_STD_DEV.ToString();
                    lbl_AvgCV.Text = CON_CV.ToString();
                    grid_consolidated.IsVisible = true;
                }

            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!! Error:" + ex.Message.ToString(), "OK");
            }
        }

        private void deleteRecords(List<YCStrengthTestSummaryModel> lstOfRecs)
        {
            if (lstOfRecs.Count == 0)
            {
                return;
            }
            foreach (YCStrengthTestSummaryModel rec in lstOfRecs)
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.Table<YCStrengthTestSummaryModel>().
                                           Where(YCStrengthTestSummaryModel =>
                                           YCStrengthTestSummaryModel.testID == rec.testID).Delete();
                    conn.Table<YCStrengthTestModel>().
                                        Where(YCStrengthTestModel =>
                                        YCStrengthTestModel.testID == rec.testID).Delete();
                }
            }
        }

        private void partialDeleteRecords(List<YCStrengthTestModel> lstOfRecs)
        {
            if (lstOfRecs.Count == 0)
            {
                return;
            }
            foreach (YCStrengthTestModel rec in lstOfRecs)
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.Table<YCStrengthTestModel>().
                                        Where(YCStrengthTestModel =>
                                        YCStrengthTestModel.testID == rec.testID).Delete();
                }
            }
        }

        private async Task resetBtn()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                listview_tcreport.ItemsSource = null;
                grid_consolidated.IsVisible = false;
                deleteAll = false;
                img_notification.IsVisible = false;
                btn_saveToPDF.Text = "Send Report";
                btn_saveToPDF.IsEnabled = true;
                btn_saveToPDF.BackgroundColor = Color.FromHex(BLUE);
                btn_backToReport.IsEnabled = true;
                btn_backToReport.BackgroundColor = Color.Green;
            });
        }

        [Obsolete]
        private async void btn_saveToPDF_Clicked(object sender, EventArgs e)
        {
            if (listview_tcreport.ItemsSource == null)
            {
                await DisplayAlert("Notice", "No records to generate PDF!!!", "OK");
                return;
            }
            if (deleteAll)
            {
                bool answer = await DisplayAlert("Attention!!!", "Would you like to delete selected records?", "Yes", "No");
                if (answer == false) { return; }
            }
            btn_backToReport.IsEnabled = false;
            btn_backToReport.BackgroundColor = Color.Gray;
            btn_saveToPDF.IsEnabled = false;
            btn_saveToPDF.BackgroundColor = Color.Gray;
            img_notification.IsVisible = true;
            CancellationTokenSource src = new CancellationTokenSource();
            CancellationToken ct = src.Token;
            ct.Register(() => Debug.WriteLine("Generate and Upload PDF Report"));
            await Task.Run(async () => await UploadReport(), ct);
            src.Cancel();
        }

        private bool generatePDFreport()
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
                List<OverallCountStrengthReportModelView> overallReportList = (List<OverallCountStrengthReportModelView>)listview_tcreport.ItemsSource;
                PdfLayoutResult result = null;
                //PdfLayoutResult resultInfo = null;
                float overallHeight = 0;
                int tableNo = 1;
                bool newPageAdded_Header = false;
                bool newPageAdded_Body = false;
                foreach (OverallCountStrengthReportModelView orl in overallReportList)
                {

                    List<YCStrengthTestReportModelView> testList = orl.ycStrengthTestlist;

                    //if (tableNo == int.Parse(entry_reportNo.Text.Trim())) break;
                    PdfGrid pdfGridInfo = new PdfGrid();
                    pdfGridInfo.RepeatHeader = true;
                    pdfGridInfo.Columns.Add(4);
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();

                    if (consolidatedReport && tableNo == 1)
                    {
                        //pdfGridInfo.Rows[0].Cells[0].Value = selectedCompanyName;
                        //pdfGridInfo.Rows[0].Cells[0].ColumnSpan = 4;
                        //pdfGridInfo.Rows[0].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                        //pdfGridInfo.Rows[0].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        //pdfGridInfo.Rows[0].Cells[0].Style.BackgroundBrush = PdfBrushes.Blue;
                        //pdfGridInfo.Rows[0].Cells[0].Style.TextPen = PdfPens.White;
                        //pdfGridInfo.Rows[0].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 18);
                        pdfGridInfo.Rows[0].Cells[0].Value = "Total Test: " + TOT_TEST;
                        pdfGridInfo.Rows[0].Cells[1].Value = "Con. Avg. CSP: " + CON_CSP;
                        pdfGridInfo.Rows[0].Cells[2].Value = "Con. SD: " + CON_STD_DEV;
                        pdfGridInfo.Rows[0].Cells[3].Value = "Con. CV: " + CON_CV;

                        PdfBrush brush_bg_con = new PdfSolidBrush(Syncfusion.Drawing.Color.LightSteelBlue);
                        pdfGridInfo.Rows[0].Cells[0].Style.BackgroundBrush = brush_bg_con;
                        pdfGridInfo.Rows[0].Cells[1].Style.BackgroundBrush = brush_bg_con;
                        pdfGridInfo.Rows[0].Cells[2].Style.BackgroundBrush = brush_bg_con;
                        pdfGridInfo.Rows[0].Cells[3].Style.BackgroundBrush = brush_bg_con;
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.Red);
                        pdfGridInfo.Rows[0].Cells[0].Style.TextBrush = brush_con;
                        pdfGridInfo.Rows[0].Cells[1].Style.TextBrush = brush_con;
                        pdfGridInfo.Rows[0].Cells[2].Style.TextBrush = brush_con;
                        pdfGridInfo.Rows[0].Cells[3].Style.TextBrush = brush_con;
                        pdfGridInfo.Rows[0].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Regular);
                        pdfGridInfo.Rows[0].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Regular);
                        pdfGridInfo.Rows[0].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Regular);
                        pdfGridInfo.Rows[0].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Regular);
                        PdfStringFormat format = new PdfStringFormat();
                        format.Alignment = PdfTextAlignment.Left;
                        format.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGridInfo.Rows[0].Cells[0].Style.StringFormat = format;
                        pdfGridInfo.Rows[0].Cells[1].Style.StringFormat = format;
                        pdfGridInfo.Rows[0].Cells[2].Style.StringFormat = format;
                        pdfGridInfo.Rows[0].Cells[3].Style.StringFormat = format;
                    }
                    pdfGridInfo.Rows[1].Cells[0].Value = "Test ID: " + orl.testID;
                    pdfGridInfo.Rows[1].Cells[0].ColumnSpan = 2;
                    pdfGridInfo.Rows[1].Cells[2].Value = "Tester: " + orl.userName;
                    pdfGridInfo.Rows[1].Cells[2].ColumnSpan = 2;
                    pdfGridInfo.Rows[2].Cells[0].Value = "Machine Category: " + orl.machineCategory;
                    pdfGridInfo.Rows[2].Cells[0].ColumnSpan = 2;
                    pdfGridInfo.Rows[2].Cells[2].Value = "Machine Name: " + orl.machineName;
                    pdfGridInfo.Rows[2].Cells[2].ColumnSpan = 2;
                    pdfGridInfo.Rows[3].Cells[0].Value = "Test System: " + orl.countsysname;
                    //pdfGridInfo.Rows[3].Cells[1].Value = "Length Unit: " + orl.yarnlenunit;
                    pdfGridInfo.Rows[3].Cells[2].Value = "Measuring Unit: " + orl.yarnlength + " " + orl.yarnlenunit;
                    pdfGridInfo.Rows[3].Cells[3].Value = "Strength Unit: " + orl.yarnstrengthunit;

                    //pdfGridInfo.Rows[4].Cells[0].Value = "Avg. CSP: " + orl.testaverage + " [Std CSP: " + orl.standardCSP + "]";
                    //pdfGridInfo.Rows[4].Cells[0].ColumnSpan = 2;
                    ////pdfGridInfo.Rows[4].Cells[0].Style.TextPen = PdfPens.Red;
                    //pdfGridInfo.Rows[4].Cells[2].Value = "SD: " + orl.testsd;
                    ////pdfGridInfo.Rows[4].Cells[1].Style.TextPen = PdfPens.Red;
                    //pdfGridInfo.Rows[4].Cells[3].Value = "CV: " + orl.testcv;

                    //pdfGridInfo.Rows[4].Cells[2].Style.TextPen = PdfPens.Red;
                    //pdfGridInfo.Rows[4].Cells[3].Value = "A%: " + orl.apercent;
                    pdfGridInfo.Rows[4].Cells[0].Value = "Date: " + orl.createdate;
                    pdfGridInfo.Rows[4].Cells[2].Value = "Shift: " + orl.shift;
                    pdfGridInfo.Rows[4].Cells[3].Value = "Total Test: " + orl.totaltestcount;

                    pdfGridInfo.Rows[5].Cells[0].Value = "Process: " + orl.process;
                    pdfGridInfo.Rows[5].Cells[0].ColumnSpan = 4;

                    pdfGridInfo.Rows[6].Cells[0].Value = "Remark: " + orl.testRemark;
                    pdfGridInfo.Rows[6].Cells[0].ColumnSpan = 4;


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
                    pdfGridInfo.Rows[5].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[5].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[5].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[5].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[6].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[6].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[6].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[6].Cells[3].Style.Borders.All = PdfPens.Transparent;

                    int totalRow_header = 7;
                    int totalRow_header_height = totalRow_header * 18;

                    if (overallHeight == 0)
                    {
                        result = pdfGridInfo.Draw(pdfPage, new PointF(10, 30), layoutFormat);
                        overallHeight = result.Bounds.Height + 35;
                    }
                    else
                    {
                        int prevPageCount = result.Page.Section.Pages.Count;
                        if (newPageAdded_Body)
                        {
                            newPageAdded_Body = false;
                            result = pdfGridInfo.Draw(pdfPage, new PointF(10, overallHeight), layoutFormat);
                        }
                        else
                        {
                            if ((overallHeight + totalRow_header_height + (testList.Count * 18)) > 730)
                            {
                                pdfPage = pdfDocument.Pages.Add();
                                result = pdfGridInfo.Draw(pdfPage, new PointF(10, 30), layoutFormat);
                                overallHeight = 0;
                                newPageAdded_Header = true;
                                pdfPage = result.Page;
                                overallHeight = result.Bounds.Height + 5;
                            }
                            else
                            {
                                result = pdfGridInfo.Draw(result.Page, new PointF(10, (overallHeight)));
                            }
                        }

                        if (prevPageCount < result.Page.Section.Pages.Count)
                        {
                            overallHeight = 0;
                            newPageAdded_Header = true;
                            pdfPage = result.Page;
                            overallHeight = result.Bounds.Height + 5;
                        }
                        else
                        {
                            overallHeight = overallHeight + result.Bounds.Height + 5;
                        }
                    }

                    pdfGrid = new PdfGrid();

                    pdfGrid.Columns.Add(4);
                    PdfGridRow row = new PdfGridRow(pdfGrid);
                    pdfGrid.Rows.Add(row);

                    pdfGrid.Rows[0].Cells[0].Value = "Sample No";
                    pdfGrid.Rows[0].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[0].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[0].Style.TextPen = PdfPens.Black;
                    pdfGrid.Rows[0].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    //pdfGrid.Rows[0].Cells[1].Value = "Sample Weight";
                    //pdfGrid.Rows[0].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[0].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    //pdfGrid.Rows[0].Cells[1].Style.BackgroundBrush = PdfBrushes.LightGray;
                    ////pdfGrid.Rows[0].Cells[1].Style.TextPen = PdfPens.Black;
                    //pdfGrid.Rows[0].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    pdfGrid.Rows[0].Cells[1].Value = "Count";
                    pdfGrid.Rows[0].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[1].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[2].Style.TextPen = PdfPens.Black;
                    pdfGrid.Rows[0].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    //pdfGrid.Rows[0].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    //pdfGrid.Rows[0].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    //pdfGrid.Rows[0].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGrid.Rows[0].Cells[2].Value = "Strength";
                    pdfGrid.Rows[0].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[2].Style.BackgroundBrush = PdfBrushes.LightGray;
                    pdfGrid.Rows[0].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    pdfGrid.Rows[0].Cells[3].Value = "CSP";
                    pdfGrid.Rows[0].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[3].Style.BackgroundBrush = PdfBrushes.LightGray;
                    pdfGrid.Rows[0].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);



                    int rowCount = 1;
                    foreach (YCStrengthTestReportModelView test in testList)
                    {
                        row = new PdfGridRow(pdfGrid);
                        pdfGrid.Rows.Add(row);
                        pdfGrid.Rows[rowCount].Cells[0].Value = test.testDescription.ToString();
                        //pdfGrid.Rows[rowCount].Cells[1].Value = formatDecimal(test.yarnweight).ToString();

                        decimal number_count;
                        if (Decimal.TryParse(test.count, out number_count))
                        {
                            pdfGrid.Rows[rowCount].Cells[1].Value = formatDecimal(number_count).ToString();
                        }
                        else
                        {
                            pdfGrid.Rows[rowCount].Cells[1].Value = test.count.ToString();
                        }

                        //pdfGrid.Rows[rowCount].Cells[2].Value = formatDecimal(test.yccalcval).ToString();
                        decimal number_strength;
                        if (Decimal.TryParse(test.strength, out number_strength))
                        {
                            pdfGrid.Rows[rowCount].Cells[2].Value = formatDecimal(number_strength).ToString();
                        }
                        else
                        {
                            pdfGrid.Rows[rowCount].Cells[2].Value = test.strength.ToString();
                        }

                        //pdfGrid.Rows[rowCount].Cells[3].Value = formatDecimal(test.yarnstrength).ToString();
                        decimal number_csp;
                        if (Decimal.TryParse(test.CSP, out number_csp))
                        {
                            if (test.testDescription.ToString() == "AVG")
                            {
                                pdfGrid.Rows[rowCount].Cells[3].Value = Convert.ToInt32(number_csp).ToString();
                            }
                            else
                            {
                                pdfGrid.Rows[rowCount].Cells[3].Value = number_csp.ToString();
                            }

                        }
                        else
                        {
                            pdfGrid.Rows[rowCount].Cells[3].Value = test.CSP.ToString();
                        }


                        //pdfGrid.Rows[rowCount].Cells[4].Value = formatDecimal(test.CSP).ToString();
                        pdfGrid.Rows[rowCount].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[rowCount].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[rowCount].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[rowCount].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        //pdfGrid.Rows[rowCount].Cells[4].StringFormat.Alignment = PdfTextAlignment.Center;
                        //pdfGrid.Rows[rowCount].Cells[4].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        rowCount++;
                    }

                    int totalRow_body_height = rowCount * 18;

                    if (result == null && overallHeight == 0)
                    {
                        result = pdfGrid.Draw(pdfPage, new PointF(10, result.Bounds.Height + 10), layoutFormat);
                        overallHeight = result.Bounds.Height + 30;
                    }
                    else if (result == null && overallHeight > 0)
                    {
                        result = pdfGrid.Draw(pdfPage, new PointF(10, overallHeight + 10), layoutFormat);
                        overallHeight = overallHeight + result.Bounds.Height + 40;//changed from 30 to 40
                    }
                    else
                    {
                        if (overallHeight == 0)
                        {
                            result = pdfGrid.Draw(pdfPage, new PointF(10, overallHeight + 10), layoutFormat);
                        }
                        else
                        {
                            int prevPageCount = result.Page.Section.Pages.Count;
                            if (newPageAdded_Header)
                            {
                                newPageAdded_Header = false;
                                result = pdfGrid.Draw(pdfPage, new PointF(10, overallHeight + 25), layoutFormat);
                                //changed from 10 to 25
                            }
                            else
                            {
                                if ((overallHeight + totalRow_body_height) > 730)
                                {
                                    pdfPage = pdfDocument.Pages.Add();
                                    result = pdfGrid.Draw(pdfPage, new PointF(10, 30), layoutFormat);
                                }
                                else
                                {
                                    result = pdfGrid.Draw(result.Page, new PointF(10, (overallHeight + 25)));
                                    //changed from 10 to 25
                                }
                            }



                            if (prevPageCount < result.Page.Section.Pages.Count)
                            {
                                if (result.Bounds.Height > 0)
                                {
                                    overallHeight = result.Bounds.Height + 30;
                                }
                                else //do not know when this condition will occur :( Need to analyze!!!
                                {
                                    overallHeight = overallHeight + result.Bounds.Height + 30;
                                }
                                newPageAdded_Body = true;
                                pdfPage = result.Page;
                            }
                            else
                            {
                                overallHeight = overallHeight + result.Bounds.Height + 30;
                            }

                        }

                    }

                    Debug.WriteLine("Page Count ===>" + pdfPage.Section.Pages.Count);
                    Debug.WriteLine("Table NO==>" + tableNo + " ,tableHeigth ===>" + overallHeight);
                    tableNo++;
                };


                addPageHeaderAndFooter(pdfDocument);
                MemoryStream stream = new MemoryStream();
                pdfDocument.Save(stream);
                pdfDocument.Close(true);
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream, "CSP_Report.pdf");
                //DisplayAlert("Notice", "PDF saved at [" + pdfPath + "]", "OK");
                //Process.Start(pdfPath);
                return true;
            }
            catch (Exception ex)
            {
                showAlert("Error occurred!!! Error: " + ex.Message.ToString(), "Error");
                return false;
            }
        }

        private void addPageHeaderAndFooter(PdfDocument pdfDocument)
        {
            String companyName = null;
            try
            {
                SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
                conn.CreateTable<CompanyModel>();
                var company = conn.Table<CompanyModel>().FirstOrDefault();
                if (company != null)
                {
                    companyName = company.Name;
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                showAlert("Error occurred!!! Error: " + ex.Message.ToString(), "Error");
            }

            for (int i = 0; i < pdfDocument.PageCount; i++)
            {
                RectangleF bounds = new RectangleF(0, 0, pdfDocument.Pages[i].GetClientSize().Width, 50);
                PdfPageTemplateElement header = new PdfPageTemplateElement(bounds);
                //Stream imageStream = App.Current.GetType().Assembly.GetManifestResourceStream("TQM.Assets.SasthaLogo.jpg");
                //PdfImage image = new PdfBitmap(imageStream);
                //header.Graphics.DrawImage(image, new PointF(0, 0), new SizeF(100, 50));
                PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                PdfBrush brush = new PdfSolidBrush(Syncfusion.Drawing.Color.Blue);
                header.Alignment = PdfAlignmentStyle.TopCenter;
                header.Graphics.DrawString(companyName, font, brush, new PointF(10, 0));
                //Title Starts
                PdfFont font_rn = new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Regular);
                PdfBrush brush_rn = new PdfSolidBrush(Syncfusion.Drawing.Color.Blue);
                //if (consolidatedReport)
                //{
                //    header.Graphics.DrawString("Consolidated YC+CSP Report - " + DateTime.Now.ToString(), font_rn, brush_rn, new PointF(135, 16));
                //}
                //else
                //{
                //    header.Graphics.DrawString("YC+CSP Report - " + DateTime.Now.ToString(), font_rn, brush_rn, new PointF(165, 16));
                //}
                if (consolidatedReport)
                {
                    header.Graphics.DrawString("Consolidated CSP Report (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(135, 16));
                }
                else
                {
                    header.Graphics.DrawString("CSP Report (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(165, 16));
                }
                //Title Ends
                pdfDocument.Template.Top = header;
                PdfPageTemplateElement footer = new PdfPageTemplateElement(bounds);
                PdfFont font_footer = new PdfStandardFont(PdfFontFamily.Helvetica, 7);
                PdfBrush brush_footer = new PdfSolidBrush(Syncfusion.Drawing.Color.Black);
                PdfPageNumberField pageNumber = new PdfPageNumberField(font_footer, brush_footer);
                PdfPageCountField count = new PdfPageCountField(font_footer, brush_footer);
                PdfCompositeField compositeField = new PdfCompositeField(font_footer, brush_footer, "Page {0} of {1}", pageNumber, count);
                compositeField.Bounds = footer.Bounds;
                compositeField.Draw(footer.Graphics, new PointF(470, 40));
                pdfDocument.Template.Bottom = footer;
            }

        }

        [Obsolete]
        public async Task UploadReport()
        {
            try
            {
                if (deleteAll && hasPartialTest)
                {
                    partialDeleteRecords(partialDeleteList);
                    showAlert("Improper test has been deleted sucessfully!!!");
                    await resetBtn();
                    return;
                }
                if (!generatePDFreport()) { showAlert("Error occurred in PDF report generation, hence upload is unsucessful!!!"); await resetBtn(); return; }
                else
                {
                    String companyName = null;
                    try
                    {
                        SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
                        conn.CreateTable<CompanyModel>();
                        var company = conn.Table<CompanyModel>().FirstOrDefault();
                        if (company != null)
                        {
                            companyName = company.Name;
                        }
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        showAlert("Error occurred!!! Error: " + ex.Message.ToString(), "Error");
                    }
                    string fileName = "CSP_Report.pdf";
                    string root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                    Java.IO.File myDir = new Java.IO.File(root + "/CSPDownloads");
                    Java.IO.File file = new Java.IO.File(myDir, fileName);
                    string filePath = file.Path;
                    var client = new RestClient("https://myconsoleerp.herokuapp.com/tqmreport/upload");
                    var request = new RestRequest();
                    request.Method = Method.Post;
                    //request.Timeout = Timeout.Infinite;
                    request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                    request.AddParameter("uploadedby", companyName);
                    request.AddParameter("title", "CSP_Report-" + DateTime.Now.ToString());
                    request.AddFile("reportpath", filePath);
                    RestResponse response = client.Execute(request);
                    if (response.IsSuccessful)
                    {
                        if (deleteAll)
                        {
                            deleteRecords(deleteList);
                            showAlert("Report uploaded and deleted sucessfully!!!");
                        }
                        else
                        {
                            showAlert("Report upload is sucessful!!!");
                        }
                    }
                    else
                    {
                        showAlert("Upload Failed. Please try again!!!", "Error");
                    }
                    await resetBtn();
                }
            }
            catch (Exception ex)
            {
                showAlert("Error occurred!!! Error: " + ex.Message.ToString(), "Error");
                await resetBtn();
            }
        }

        private async void showAlert(string msg, string title = "Notice")
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("Notice", msg, "Ok");
            });
        }

        private decimal formatDecimal(decimal inputVal)
        {
            inputVal = Math.Round(inputVal, 4);
            string inputString = inputVal.ToString();
            string[] ipStringArray = inputString.Split('.');
            if (ipStringArray.Length > 1)
            {
                string beforeDecimal = ipStringArray[0];
                string afterDecimal = ipStringArray[1];
                for (int i = ipStringArray[1].Length; i < 4; i++)
                {
                    afterDecimal = afterDecimal + "0";
                }
                return decimal.Parse(beforeDecimal + "." + afterDecimal);
            }
            else
            {
                return decimal.Parse(inputString + ".0000");
            }
        }

        private void btn_backToReport_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Report());
        }

    }
}