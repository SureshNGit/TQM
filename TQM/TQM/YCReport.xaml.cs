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
    public partial class YCReport : ContentPage
    {

        private List<OverallReportModelView> _listOfReports;
        public List<OverallReportModelView> ListOfReport { get { return _listOfReports; } set { _listOfReports = value; base.OnPropertyChanged(); } }

        private List<YCTestConsolidatedReportMV> _listOfConsolidatedReports;
        public List<YCTestConsolidatedReportMV> ListOfConsolidatedReports { get { return _listOfConsolidatedReports; } set { _listOfConsolidatedReports = value; base.OnPropertyChanged(); } }

        private string selectedCompanyName = null;
        private string selectedMachineCategory = null;
        private const string BLUE = "#0e0273";
        private RunConfiguration runConfiguration = new RunConfiguration();
        private List<YCTestSummaryModel> deleteList = null;
        private bool deleteAll = false;
        private int TOT_TEST = 0;
        private decimal CON_HANK = 0.0000m;
        private decimal CON_STD_DEV = 0.0000m;
        private decimal CON_CV = 0.0000m;
        private bool consolidatedReport = false;
        private DateTime reportStartDate;
        private DateTime reportEndDate;

        public YCReport()
        {
            InitializeComponent();
        }

        public YCReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string process, string testID, string standHank, bool deleteRequest, bool isConsolidated)
        {
            InitializeComponent();
            consolidatedReport = isConsolidated;
            if (consolidatedReport)
            {
                if (categoryName != null && categoryName != "")
                {
                    lbl_reportHeader.Text = "Con. Wrapping Report - " + categoryName;
                }
                else
                {
                    lbl_reportHeader.Text = "Con. Wrapping Report - All";
                }
            }
            else
            {
                if (categoryName != null && categoryName != "")
                {
                    lbl_reportHeader.Text = "Wrapping Report - " + categoryName;
                }
                else
                {
                    lbl_reportHeader.Text = "Wrapping Report - All";
                }
            }
            if (deleteRequest)
            {
                btn_saveToPDF.Text = "Send & Delete Records";
                btn_saveToPDF.BackgroundColor = Color.Red;
                btn_saveToPDF.TextColor = Color.White;
            }
            if (categoryName != null && categoryName != "")
            {
                selectedMachineCategory = categoryName;
            }
            reportStartDate = startDate;
            reportEndDate = endDate;
            getReport(startDate, endDate, categoryName, machineID, shift, process, testID, standHank, deleteRequest);
        }

        private void getReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string process, string testID, string standHank, bool deleteRequest)
        {
            try
            {
                decimal stdHank = 0.000m;
                List<OverallReportModelView> OVS = new List<OverallReportModelView>();
                List<YCTestConsolidatedReportMV> OverallConsolidatedReports = new List<YCTestConsolidatedReportMV>();
                //List<YCTestSummaryModel> ycTestSummaryModels = null;
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    //List<YCTestSummaryModel> ycTestSummaryModels = conn.Table<YCTestSummaryModel>().ToList();

                    //conn.CreateTable<YarnCountConfigModel>();
                    //YarnCountConfigModel yarncountconfigmodel = conn.Table<YarnCountConfigModel>().FirstOrDefault();
                    //if (yarncountconfigmodel != null)
                    //{
                    //    stdHank = yarncountconfigmodel.standardHank;

                    //}

                    conn.CreateTable<YCTestModel>();
                    conn.CreateTable<YCTestSummaryModel>();

                    List<YCTestSummaryModel> ycTestSummaryModels = null;
                    if (testID != "")
                    {
                        long givenTestId = long.Parse(testID);
                        ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                              YCTestSummaryModel.testID == givenTestId).ToList();
                    }
                    else
                    {
                        if (categoryName == null || categoryName == "")
                        {
                            endDate = endDate.AddDays(1);

                            if (shift != "" && process != null)
                            {
                                ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                    (YCTestSummaryModel.createdate >= startDate
                                                    && YCTestSummaryModel.createdate < endDate
                                                    && YCTestSummaryModel.shift == shift
                                                    && YCTestSummaryModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift == "" && process != null)
                            {
                                ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                    (YCTestSummaryModel.createdate >= startDate
                                                    && YCTestSummaryModel.createdate < endDate
                                                    && YCTestSummaryModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift != "" && process == null)
                            {
                                ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                    (YCTestSummaryModel.createdate >= startDate
                                                    && YCTestSummaryModel.createdate < endDate
                                                    && YCTestSummaryModel.shift == shift)).ToList();
                            }
                            else if (shift == "" && process == null)
                            {
                                ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                    (YCTestSummaryModel.createdate >= startDate
                                                    && YCTestSummaryModel.createdate < endDate)).ToList();
                            }

                        }
                        else if (categoryName != null && machineID == Guid.Empty)
                        {
                            endDate = endDate.AddDays(1);

                            if (shift != "" && process != null)
                            {
                                ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                     (YCTestSummaryModel.createdate >= startDate
                                                     && YCTestSummaryModel.createdate < endDate
                                                     && YCTestSummaryModel.machineCategory == categoryName
                                                     && YCTestSummaryModel.shift == shift
                                                     && YCTestSummaryModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift == "" && process != null)
                            {
                                ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                     (YCTestSummaryModel.createdate >= startDate
                                                     && YCTestSummaryModel.createdate < endDate
                                                     && YCTestSummaryModel.machineCategory == categoryName
                                                     && YCTestSummaryModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift != "" && process == null)
                            {
                                ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                     (YCTestSummaryModel.createdate >= startDate
                                                     && YCTestSummaryModel.createdate < endDate
                                                     && YCTestSummaryModel.machineCategory == categoryName
                                                     && YCTestSummaryModel.shift == shift)).ToList();
                            }
                            else if (shift == "" && process == null)
                            {
                                ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                    (YCTestSummaryModel.createdate >= startDate
                                                    && YCTestSummaryModel.createdate < endDate
                                                    && YCTestSummaryModel.machineCategory == categoryName)).ToList();
                            }


                        }
                        else if (categoryName != null && machineID != Guid.Empty)
                        {
                            endDate = endDate.AddDays(1);

                            if (shift != "" && process != null)
                            {
                                ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                        (YCTestSummaryModel.createdate >= startDate
                                                        && YCTestSummaryModel.createdate < endDate
                                                        && YCTestSummaryModel.machineCategory == categoryName)
                                                        && YCTestSummaryModel.machineID == machineID
                                                        && YCTestSummaryModel.shift == shift
                                                        && YCTestSummaryModel.process.ToLower() == process.ToLower()).ToList();
                            }
                            else if (shift == "" && process != null)
                            {
                                ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                        (YCTestSummaryModel.createdate >= startDate
                                                        && YCTestSummaryModel.createdate < endDate
                                                        && YCTestSummaryModel.machineCategory == categoryName)
                                                        && YCTestSummaryModel.machineID == machineID
                                                        && YCTestSummaryModel.process.ToLower() == process.ToLower()).ToList();
                            }
                            else if (shift != "" && process == null)
                            {
                                ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                        (YCTestSummaryModel.createdate >= startDate
                                                        && YCTestSummaryModel.createdate < endDate
                                                        && YCTestSummaryModel.machineCategory == categoryName)
                                                        && YCTestSummaryModel.machineID == machineID
                                                        && YCTestSummaryModel.shift == shift).ToList();
                            }
                            else if (shift == "" && process == null)
                            {
                                ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                        (YCTestSummaryModel.createdate >= startDate
                                                        && YCTestSummaryModel.createdate < endDate
                                                        && YCTestSummaryModel.machineCategory == categoryName)
                                                        && YCTestSummaryModel.machineID == machineID).ToList();
                            }

                        }
                    }

                    if (ycTestSummaryModels.Count == 0)
                    {
                        DisplayAlert("Notice", "No records to display!!!", "OK");
                        return;
                    }
                    else
                    {
                        //if (testID != "")
                        //{
                        //    ycTestSummaryModels = ycTestSummaryModels.Where(t => t.testID == long.Parse(testID)).ToList();
                        //}
                        if (standHank != "")
                        {
                            ycTestSummaryModels = ycTestSummaryModels.Where(t => t.standardHank == Decimal.Parse(standHank)).ToList();
                        }
                        if (ycTestSummaryModels.Count == 0)
                        {
                            DisplayAlert("Notice", "No records to display!!!", "OK");
                            return;
                        }
                        if (deleteRequest)
                        {
                            deleteAll = true;
                            deleteList = ycTestSummaryModels;
                        }
                    }

                    CON_HANK = 0.0000m;
                    CON_STD_DEV = 0.0000m;
                    CON_CV = 0.0000m;

                    TOT_TEST = ycTestSummaryModels.Count;
                    int counter = 0;
                    foreach (YCTestSummaryModel testsummary in ycTestSummaryModels)
                    {
                        OverallReportModelView report = new OverallReportModelView();
                        YCTestConsolidatedReportMV consolItems = new YCTestConsolidatedReportMV();
                        List<YCTestModel> yctestlist = conn.Table<YCTestModel>().Where(YCTestModel => YCTestModel.testID == testsummary.testID).ToList();
                        if (yctestlist != null)
                        {
                            if (consolidatedReport)
                            {
                                CON_HANK = CON_HANK + formatDecimal(testsummary.testaverage);
                                CON_STD_DEV = CON_STD_DEV + formatDecimal(testsummary.testsd);
                                CON_CV = CON_CV + formatDecimal(testsummary.testcv);
                                if (counter == 0)
                                {
                                    if (testsummary.machineCategory == "Spinning" || testsummary.machineCategory == "Winding")
                                    {
                                        lbl_con_Hank.Text = "Avg. COUNT : ";
                                        lbl_conStdHank.Text = "Std. Count";
                                        lbl_conAvgHank.Text = "Avg. Count";
                                    }
                                    else
                                    {
                                        lbl_con_Hank.Text = "Avg. HANK : ";
                                        lbl_conStdHank.Text = "Std. Hank";
                                        lbl_conAvgHank.Text = "Avg. Hank";
                                    }
                                }


                                //decimal maxRangeVal = testsummary.standardHank + (testsummary.standardHank * (Convert.ToDecimal(testsummary.deviationPercent) / 100));
                                //decimal minRangeVal = testsummary.standardHank - (testsummary.standardHank * (Convert.ToDecimal(testsummary.deviationPercent) / 100));

                                decimal maxRangeVal = testsummary.standardHank + testsummary.deviationPercent;
                                decimal minRangeVal = testsummary.standardHank - testsummary.deviationPercent;


                                if (testsummary.testaverage < minRangeVal || testsummary.testaverage > maxRangeVal)
                                {
                                    consolItems.isRed = true;
                                    consolItems.isWhite = false;
                                }
                                else
                                {
                                    consolItems.isRed = false;
                                    consolItems.isWhite = true;
                                }

                                consolItems.serialNo = (counter + 1).ToString();
                                consolItems.testID = testsummary.testID.ToString();
                                consolItems.machineName = testsummary.machineName;
                                consolItems.testDate = testsummary.createdate.Day.ToString() + "-" + testsummary.createdate.Month.ToString() + "-" + testsummary.createdate.Year.ToString();
                                consolItems.shift = testsummary.shift;
                                if (testsummary.machineCategory == "Spinning" || testsummary.machineCategory == "Winding")
                                {
                                    consolItems.standardValue = formatDecimal(testsummary.standardHank, 2).ToString() + " " + "\u00B1" + formatDecimal(testsummary.deviationPercent, 2).ToString();

                                }
                                else
                                {
                                    consolItems.standardValue = formatDecimal(testsummary.standardHank, 4).ToString() + " " + "\u00B1" + formatDecimal(testsummary.deviationPercent, 4).ToString();
                                }
                                consolItems.testAverage = formatDecimal(testsummary.testaverage).ToString();
                                consolItems.standardDeviation = formatDecimal(testsummary.testsd).ToString();
                                consolItems.CoEfficientOfVariation = formatDecimal(testsummary.testcv).ToString();
                                consolItems.testDuration = testsummary.testDuration;
                                consolItems.remarks = testsummary.testRemark;

                                if (testsummary.machineCategory == "Spinning" || testsummary.machineCategory == "Winding")
                                {
                                    consolItems.isSpinning = true;
                                    consolItems.otherThanSpinning = false;
                                }
                                else
                                {
                                    consolItems.isSpinning = false;
                                    consolItems.otherThanSpinning = true;
                                }

                                //YCTestModel firstTest = yctestlist.Where(YCTestModel => YCTestModel.testcount == 1).FirstOrDefault();
                                //TimeSpan duration = (firstTest.createdate - testsummary.createdate).Duration();
                                //consolItems.testDuration = duration.Hours.ToString() + ":" + duration.Minutes.ToString() + ":" + duration.Seconds.ToString();


                                counter++;


                            }
                            else
                            {

                                if (testsummary.machineCategory == "Spinning" || testsummary.machineCategory == "Winding")
                                {
                                    report.isSpinning = true;
                                    report.otherThanSpinning = false;
                                }
                                else
                                {
                                    report.isSpinning = false;
                                    report.otherThanSpinning = true;
                                }


                                foreach (YCTestModel test in yctestlist)
                                {
                                    report.Add(test);
                                }
                                report.testID = testsummary.testID;
                                report.userName = testsummary.userName;
                                report.machineCategory = testsummary.machineCategory;
                                report.machineName = testsummary.machineName;
                                report.shift = testsummary.shift;
                                report.process = testsummary.process;
                                //report.apercent = testsummary.apercent;
                                report.countsysname = testsummary.countsysname;
                                report.yarnlenunit = testsummary.yarnlenunit;
                                report.yarnlength = testsummary.yarnlength;
                                report.totaltestcount = testsummary.totaltestcount;
                                report.createdate = testsummary.createdate;
                                report.testRemark = testsummary.testRemark;
                                report.testaverage = formatDecimal(testsummary.testaverage);
                                report.testsd = formatDecimal(testsummary.testsd);
                                report.testcv = formatDecimal(testsummary.testcv);
                                //report.standardHank = formatDecimal(stdHank);
                                report.standardHank = formatDecimal(testsummary.standardHank);
                                report.testDuration = testsummary.testDuration;
                                report.deviationPercent = "\u00B1" + testsummary.deviationPercent;

                                decimal maxRangeVal = testsummary.standardHank + testsummary.deviationPercent;
                                decimal minRangeVal = testsummary.standardHank - testsummary.deviationPercent;


                                if (testsummary.testaverage < minRangeVal || testsummary.testaverage > maxRangeVal)
                                {
                                    report.hankColor = "Red";
                                    report.hankColorGg = "Yellow";
                                }
                                else
                                {
                                    report.hankColor = "Green";
                                    report.hankColorGg = "None";
                                }


                            }
                        }
                        if (consolidatedReport)
                        {
                            OverallConsolidatedReports.Add(consolItems);
                        }
                        else
                        {
                            OVS.Add(report);
                        }
                    }


                    if (consolidatedReport)
                    {
                        CON_HANK = formatDecimal(CON_HANK / TOT_TEST);
                        CON_STD_DEV = formatDecimal(CON_STD_DEV / TOT_TEST);
                        CON_CV = formatDecimal(CON_CV / TOT_TEST);



                        YCTestConsolidatedReportMV consolItems = new YCTestConsolidatedReportMV()
                        {
                            serialNo = "Average",
                            testID = "",
                            machineName = "",
                            testDate = "",
                            shift = "",
                            standardValue = "",
                            testAverage = CON_HANK.ToString(),
                            standardDeviation = CON_STD_DEV.ToString(),
                            CoEfficientOfVariation = CON_CV.ToString(),
                            testDuration = "",
                            remarks = "",
                            isWhite = true,
                            isRed = false


                        };

                        if (selectedMachineCategory == "Spinning" || selectedMachineCategory == "Winding")
                        {
                            consolItems.isSpinning = true;
                            consolItems.otherThanSpinning = false;
                        }
                        else
                        {
                            consolItems.isSpinning = false;
                            consolItems.otherThanSpinning = true;
                        }

                        OverallConsolidatedReports.Add(consolItems);

                        ListOfConsolidatedReports = OverallConsolidatedReports;
                    }
                    else
                    {
                        //CON_HANK = formatDecimal(CON_HANK / TOT_TEST);
                        //CON_STD_DEV = formatDecimal(CON_STD_DEV / TOT_TEST);
                        //CON_CV = formatDecimal(CON_CV / TOT_TEST);
                        ListOfReport = OVS;
                    }

                }
                listview_tcreport.ItemsSource = null;
                listview_tcConsolidatedReport.ItemsSource = null;
                if (consolidatedReport)
                {
                    lbl_totalTest.Text = TOT_TEST.ToString();
                    lbl_AvgHank.Text = CON_HANK.ToString();
                    lbl_AvgSD.Text = CON_STD_DEV.ToString();
                    lbl_AvgCV.Text = CON_CV.ToString();
                    //grid_consolidated.IsVisible = true;

                    listview_tcreport.IsVisible = false;
                    listview_tcConsolidatedReport.IsVisible = true;
                    listview_tcConsolidatedReport.ItemsSource = ListOfConsolidatedReports;
                }
                else
                {
                    listview_tcConsolidatedReport.IsVisible = false;
                    listview_tcreport.IsVisible = true;
                    listview_tcreport.ItemsSource = ListOfReport;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!! Error:" + ex.Message.ToString(), "OK");
            }
        }

        private void deleteRecords(List<YCTestSummaryModel> lstOfRecs)
        {
            if (lstOfRecs.Count == 0)
            {
                return;
            }
            foreach (YCTestSummaryModel rec in lstOfRecs)
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.Table<YCTestSummaryModel>().
                                           Where(YCTestSummaryModel =>
                                           YCTestSummaryModel.testID == rec.testID).Delete();
                    conn.Table<YCTestModel>().
                                        Where(YCTestModel =>
                                        YCTestModel.testID == rec.testID).Delete();
                }
            }
        }

        private async Task resetBtn()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
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
            if (listview_tcreport.ItemsSource == null && listview_tcConsolidatedReport.ItemsSource == null)
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
                List<OverallReportModelView> overallReportList = (List<OverallReportModelView>)listview_tcreport.ItemsSource;
                PdfLayoutResult result = null;
                //PdfLayoutResult resultInfo = null;
                float overallHeight = 0;
                int tableNo = 1;
                bool newPageAdded_Header = false;
                bool newPageAdded_Body = false;
                foreach (OverallReportModelView orl in overallReportList)
                {

                    List<YCTestModel> testList = orl.yctestlist;

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
                        pdfGridInfo.Rows[0].Cells[1].Value = "Con. HANK: " + CON_HANK;
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
                    pdfGridInfo.Rows[3].Cells[1].Value = "Length Unit: " + orl.yarnlenunit;
                    pdfGridInfo.Rows[3].Cells[2].Value = "Length: " + orl.yarnlength;
                    pdfGridInfo.Rows[3].Cells[3].Value = "Total Test: " + orl.totaltestcount;

                    if (orl.machineCategory == "Spinning" || orl.machineCategory == "Winding")
                    {
                        pdfGridInfo.Rows[4].Cells[0].Value = "Count: " + formatDecimal(orl.testaverage, 2).ToString() +
                            " [Std Count: " + formatDecimal(orl.standardHank, 2) + " " + orl.deviationPercent + "]";
                    }
                    else
                    {
                        pdfGridInfo.Rows[4].Cells[0].Value = "Hank: " + formatDecimal(orl.testaverage, 2).ToString() +
                            " [Std Hank: " + formatDecimal(orl.standardHank, 2) + " " + orl.deviationPercent + "]";
                    }
                    pdfGridInfo.Rows[4].Cells[0].ColumnSpan = 2;
                    if (orl.hankColor == "Red")
                    {
                        pdfGridInfo.Rows[4].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGridInfo.Rows[4].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGridInfo.Rows[4].Cells[0].Style.BackgroundBrush = PdfBrushes.Red;
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                        pdfGridInfo.Rows[4].Cells[0].Style.TextBrush = brush_con;
                    }


                    //pdfGridInfo.Rows[4].Cells[0].Style.TextPen = PdfPens.Red;
                    pdfGridInfo.Rows[4].Cells[2].Value = "SD: " + orl.testsd;
                    //pdfGridInfo.Rows[4].Cells[1].Style.TextPen = PdfPens.Red;
                    pdfGridInfo.Rows[4].Cells[3].Value = "CV: " + orl.testcv;
                    //pdfGridInfo.Rows[4].Cells[2].Style.TextPen = PdfPens.Red;
                    //pdfGridInfo.Rows[4].Cells[3].Value = "A%: " + orl.apercent;
                    pdfGridInfo.Rows[5].Cells[0].Value = "Date: " + orl.createdate;
                    pdfGridInfo.Rows[5].Cells[0].ColumnSpan = 1;
                    pdfGridInfo.Rows[5].Cells[1].Value = "Duration: " + orl.testDuration;
                    pdfGridInfo.Rows[5].Cells[2].Value = "Shift: " + orl.shift;
                    pdfGridInfo.Rows[5].Cells[3].Value = "Process: " + orl.process;

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

                    pdfGrid.Columns.Add(3);
                    PdfGridRow row = new PdfGridRow(pdfGrid);
                    pdfGrid.Rows.Add(row);

                    pdfGrid.Rows[0].Cells[0].Value = "Sample No";
                    pdfGrid.Rows[0].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[0].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[0].Style.TextPen = PdfPens.Black;
                    pdfGrid.Rows[0].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    pdfGrid.Rows[0].Cells[1].Value = "Sample Weight";
                    pdfGrid.Rows[0].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[1].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[1].Style.TextPen = PdfPens.Black;
                    pdfGrid.Rows[0].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    if (orl.machineCategory == "Spinning" || orl.machineCategory == "Winding")
                    {
                        pdfGrid.Rows[0].Cells[2].Value = "Count";
                    }
                    else
                    {
                        pdfGrid.Rows[0].Cells[2].Value = "Hank";
                    }
                    pdfGrid.Rows[0].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[2].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[2].Style.TextPen = PdfPens.Black;
                    pdfGrid.Rows[0].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    //pdfGrid.Rows[0].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    //pdfGrid.Rows[0].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    //pdfGrid.Rows[0].Cells[2].Style.Borders.All = PdfPens.Transparent;



                    int rowCount = 1;
                    foreach (YCTestModel test in testList)
                    {
                        row = new PdfGridRow(pdfGrid);
                        pdfGrid.Rows.Add(row);
                        pdfGrid.Rows[rowCount].Cells[0].Value = test.testcount.ToString();
                        if (orl.machineCategory == "Spinning" || orl.machineCategory == "Winding")
                        {
                            pdfGrid.Rows[rowCount].Cells[1].Value = formatDecimal(test.yarnweight, 2).ToString();
                            pdfGrid.Rows[rowCount].Cells[2].Value = formatDecimal(test.yccalcval, 2).ToString();
                        }
                        else
                        {
                            pdfGrid.Rows[rowCount].Cells[1].Value = formatDecimal(test.yarnweight, 4).ToString();
                            pdfGrid.Rows[rowCount].Cells[2].Value = formatDecimal(test.yccalcval, 4).ToString();
                        }
                        pdfGrid.Rows[rowCount].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[rowCount].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[rowCount].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
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
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream, "TQM_Report(Wrapping).pdf");
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


        private bool generatePDFConsolidatedReport()
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
                List<YCTestConsolidatedReportMV> overallReportList = (List<YCTestConsolidatedReportMV>)listview_tcConsolidatedReport.ItemsSource;
                PdfLayoutResult result = null;
                float overallHeight = 0;
                int tableNo = 1;
                //bool newPageAdded_Header = false;
                //bool newPageAdded_Body = false;

                PdfGrid pdfGridInfo = new PdfGrid();
                pdfGridInfo.RepeatHeader = true;


                int totalRow_header = 1;
                int totalRow_header_height = totalRow_header * 18;

                int rowCount = 1;
                int pageRecordCount = 1;
                float rowHeights = 0;
                bool includeHeader = true;
                //PdfGrid pdfGridBody = null;
                PdfGridRow row = null;
                foreach (YCTestConsolidatedReportMV orl in overallReportList)
                {
                    if (includeHeader)
                    {
                        includeHeader = false;
                        pdfGrid = new PdfGrid();

                        pdfGrid.Columns.Add(11);
                        row = new PdfGridRow(pdfGrid);
                        pdfGrid.Rows.Add(row);

                        pdfGrid.Rows[0].Cells[0].Value = "S.No";
                        pdfGrid.Rows[0].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[0].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[1].Value = "Date";
                        pdfGrid.Rows[0].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[1].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[2].Value = "ID";
                        pdfGrid.Rows[0].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[2].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[3].Value = "Mac Name";
                        pdfGrid.Rows[0].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[3].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[4].Value = "Shift";
                        pdfGrid.Rows[0].Cells[4].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[4].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[4].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);

                        if (selectedMachineCategory == "Spinning" || selectedMachineCategory == "Winding")
                        {
                            pdfGrid.Rows[0].Cells[5].Value = "Std. Count";
                            pdfGrid.Rows[0].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                            pdfGrid.Rows[0].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                            pdfGrid.Rows[0].Cells[5].Style.BackgroundBrush = PdfBrushes.LightGray;
                            pdfGrid.Rows[0].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                            pdfGrid.Rows[0].Cells[6].Value = "Avg. Count";
                            pdfGrid.Rows[0].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                            pdfGrid.Rows[0].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                            pdfGrid.Rows[0].Cells[6].Style.BackgroundBrush = PdfBrushes.LightGray;
                            pdfGrid.Rows[0].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        }
                        else
                        {
                            pdfGrid.Rows[0].Cells[5].Value = "Std. Hank";
                            pdfGrid.Rows[0].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                            pdfGrid.Rows[0].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                            pdfGrid.Rows[0].Cells[5].Style.BackgroundBrush = PdfBrushes.LightGray;
                            pdfGrid.Rows[0].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                            pdfGrid.Rows[0].Cells[6].Value = "Avg. Hank";
                            pdfGrid.Rows[0].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                            pdfGrid.Rows[0].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                            pdfGrid.Rows[0].Cells[6].Style.BackgroundBrush = PdfBrushes.LightGray;
                            pdfGrid.Rows[0].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        }

                        pdfGrid.Rows[0].Cells[7].Value = "SD";
                        pdfGrid.Rows[0].Cells[7].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[7].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[7].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[7].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[8].Value = "CV";
                        pdfGrid.Rows[0].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[8].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[9].Value = "Duration";
                        pdfGrid.Rows[0].Cells[9].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[9].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[9].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[9].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[10].Value = "Remark";
                        pdfGrid.Rows[0].Cells[10].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[10].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[10].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[10].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        row = new PdfGridRow(pdfGrid);
                        pdfGrid.Rows.Add(row);
                        rowHeights = rowHeights + pdfGrid.Rows[0].Height;
                    }


                    pdfGrid.Rows.Add();

                    pdfGrid.Rows[pageRecordCount].Cells[0].Value = orl.serialNo;
                    pdfGrid.Rows[pageRecordCount].Cells[1].Value = orl.testDate;
                    pdfGrid.Rows[pageRecordCount].Cells[2].Value = orl.testID;
                    pdfGrid.Rows[pageRecordCount].Cells[3].Value = orl.machineName;
                    pdfGrid.Rows[pageRecordCount].Cells[4].Value = orl.shift;
                    pdfGrid.Rows[pageRecordCount].Cells[5].Value = orl.standardValue;



                    if (orl.testAverage != null && orl.testAverage != "")
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[6].Value = formatDecimal(Decimal.Parse(orl.testAverage)).ToString();
                    }
                    else
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[6].Value = orl.testAverage;
                    }
                    if (orl.standardDeviation != null && orl.standardDeviation != "")
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[7].Value = formatDecimal(Decimal.Parse(orl.standardDeviation)).ToString();
                    }
                    else
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[7].Value = orl.standardDeviation;
                    }
                    if (orl.CoEfficientOfVariation != null && orl.CoEfficientOfVariation != "")
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[8].Value = formatDecimal(Decimal.Parse(orl.CoEfficientOfVariation)).ToString();
                    }
                    else
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[8].Value = orl.CoEfficientOfVariation;
                    }
                    pdfGrid.Rows[pageRecordCount].Cells[9].Value = orl.testDuration;
                    pdfGrid.Rows[pageRecordCount].Cells[10].Value = orl.remarks;

                    if (orl.remarks != null || orl.standardValue != null)
                    {
                        //if (orl.testID == "82")
                        //{
                        //    decimal a = 1 / 9;
                        //}
                        PdfStringFormat format = new PdfStringFormat();
                        format.WordWrap = PdfWordWrapType.Word;
                        pdfGrid.Rows[pageRecordCount].Cells[10].Style.StringFormat = format;
                        float currentRowHeight = pdfGrid.Rows[pageRecordCount].Height;
                        int contentLength = 0;
                        if (orl.remarks != null)
                        {
                            contentLength = orl.remarks.Length;
                        }
                        if (orl.standardValue != null && orl.remarks != null)
                        {
                            if (orl.standardValue.Length > orl.remarks.Length)
                            {
                                contentLength = orl.standardValue.Length;
                            }
                        }
                        else if (orl.standardValue != null && orl.remarks == null)
                        {
                            contentLength = orl.standardValue.Length;
                        }
                        if (contentLength >= 9)
                        {
                            pdfGrid.Rows[pageRecordCount].Height = currentRowHeight * ((contentLength / 9) + 1);
                        }
                    }

                    pdfGrid.Rows[pageRecordCount].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

                    if (orl.isWhite)
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[6].Style.BackgroundBrush = PdfBrushes.White;
                        //pdfGrid.Rows[pageRecordCount].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.Black);
                        pdfGrid.Rows[pageRecordCount].Cells[6].Style.TextBrush = brush_con;
                    }
                    else
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[6].Style.BackgroundBrush = PdfBrushes.Red;
                        //pdfGrid.Rows[pageRecordCount].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                        pdfGrid.Rows[pageRecordCount].Cells[6].Style.TextBrush = brush_con;
                    }

                    pdfGrid.Rows[pageRecordCount].Cells[7].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[7].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[9].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[9].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[10].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[10].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

                    if (rowCount == overallReportList.Count)
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[0].Style.Borders.All = PdfPens.Transparent;
                        pdfGrid.Rows[pageRecordCount].Cells[1].Style.Borders.All = PdfPens.Transparent;
                        pdfGrid.Rows[pageRecordCount].Cells[2].Style.Borders.All = PdfPens.Transparent;
                        pdfGrid.Rows[pageRecordCount].Cells[3].Style.Borders.All = PdfPens.Transparent;
                        pdfGrid.Rows[pageRecordCount].Cells[4].Style.Borders.All = PdfPens.Transparent;
                        pdfGrid.Rows[pageRecordCount].Cells[5].Style.Borders.All = PdfPens.Transparent;
                        pdfGrid.Rows[pageRecordCount].Cells[6].Style.Borders.All = PdfPens.Transparent;
                        pdfGrid.Rows[pageRecordCount].Cells[7].Style.Borders.All = PdfPens.Transparent;
                        pdfGrid.Rows[pageRecordCount].Cells[8].Style.Borders.All = PdfPens.Transparent;
                        pdfGrid.Rows[pageRecordCount].Cells[9].Style.Borders.All = PdfPens.Transparent;
                        pdfGrid.Rows[pageRecordCount].Cells[10].Style.Borders.All = PdfPens.Transparent;

                        pdfGrid.Rows[pageRecordCount].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[7].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[9].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[10].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                    }
                    rowHeights = rowHeights + pdfGrid.Rows[pageRecordCount].Height;
                    if (rowHeights <= 700 && rowCount == overallReportList.Count)
                    {
                        result = pdfGrid.Draw(pdfPage, new PointF(10, 55), layoutFormat);
                    }
                    else if ((rowHeights >= 670 && rowHeights <= 700) && pageRecordCount != overallReportList.Count)
                    {
                        result = pdfGrid.Draw(pdfPage, new PointF(10, 55), layoutFormat);
                        pdfPage = pdfDocument.Pages.Add();
                        pageRecordCount = 0;
                        rowHeights = 0;
                        includeHeader = true;
                    }
                    Debug.WriteLine("Page Count ===>" + pdfPage.Section.Pages.Count);
                    pageRecordCount++;
                    rowCount++;
                }




                //Debug.WriteLine("Page Count ===>" + pdfPage.Section.Pages.Count);
                Debug.WriteLine("Table NO==>" + tableNo + " ,tableHeigth ===>" + overallHeight);
                tableNo++;
                //};


                addPageHeaderAndFooter(pdfDocument);
                MemoryStream stream = new MemoryStream();
                pdfDocument.Save(stream);
                pdfDocument.Close(true);
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream, "TQM_Report_Consolidated(Wrapping).pdf");
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
                if (consolidatedReport)
                {
                    header.Graphics.DrawString("Con. Wrappping Report - " + selectedMachineCategory + " (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(135, 20));
                    header.Graphics.DrawString("Date: " + DateTime.Now.ToString(), font_rn, brush_rn, new PointF(200, 36));
                }
                else
                {
                    if (selectedMachineCategory != null)
                    {
                        header.Graphics.DrawString("Wrapping Report - " + selectedMachineCategory + " (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(165, 16));
                    }
                    else
                    {
                        header.Graphics.DrawString("Wrapping Report - (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(165, 16));
                    }
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
                if (consolidatedReport)
                {

                    if (!generatePDFConsolidatedReport()) { showAlert("Error occurred in PDF report generation, hence upload is unsucessful!!!"); await resetBtn(); return; }
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
                        string fileName = "TQM_Report_Consolidated(Wrapping).pdf";
                        string root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                        Java.IO.File myDir = new Java.IO.File(root + "/TQMDownloads");
                        Java.IO.File file = new Java.IO.File(myDir, fileName);
                        string filePath = file.Path;
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqmreport/upload");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        //request.Timeout = Timeout.Infinite;
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("uploadedby", companyName);
                        request.AddParameter("title", "TQMReportsConsolidated(Wrapping)-" + DateTime.Now.ToString());
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
                else
                {
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
                        string fileName = "TQM_Report(Wrapping).pdf";
                        string root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                        Java.IO.File myDir = new Java.IO.File(root + "/TQMDownloads");
                        Java.IO.File file = new Java.IO.File(myDir, fileName);
                        string filePath = file.Path;
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqmreport/upload");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        //request.Timeout = Timeout.Infinite;
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("uploadedby", companyName);
                        request.AddParameter("title", "TQMReports(Wrapping)-" + DateTime.Now.ToString());
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

        private decimal formatDecimal(decimal inputVal, int afterDecimalCount = 4)
        {
            if (selectedMachineCategory == "Spinning" || selectedMachineCategory == "Winding")
            {
                afterDecimalCount = 2;
            }
            inputVal = Math.Round(inputVal, afterDecimalCount);
            string inputString = inputVal.ToString();
            string[] ipStringArray = inputString.Split('.');
            if (ipStringArray.Length > 1)
            {
                string beforeDecimal = ipStringArray[0];
                string afterDecimal = ipStringArray[1];
                for (int i = ipStringArray[1].Length; i < afterDecimalCount; i++)
                {
                    afterDecimal = afterDecimal + "0";
                }
                return decimal.Parse(beforeDecimal + "." + afterDecimal);
            }
            else
            {
                inputString = inputString + ".";
                for (int i = 0; i < afterDecimalCount; i++)
                {
                    inputString = inputString + "0";
                }
                return decimal.Parse(inputString);
            }
        }

        private void btn_backToReport_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Report());
        }

    }


}