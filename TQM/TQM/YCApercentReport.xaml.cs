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
    public partial class YCApercentReport : ContentPage
    {

        private List<OverallApercentReportModelView> _listOfReports;
        public List<OverallApercentReportModelView> ListOfReport { get { return _listOfReports; } set { _listOfReports = value; base.OnPropertyChanged(); } }
        private string selectedCompanyName = null;
        private const string BLUE = "#0e0273";
        public YCApercentReport()
        {
            InitializeComponent();
        }

        public YCApercentReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string process)
        {
            InitializeComponent();
            getReport(startDate, endDate, categoryName, machineID, shift, process);
        }

        private void getReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string process)
        {
            try
            {
                List<OverallApercentReportModelView> OVS = new List<OverallApercentReportModelView>();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {

                    conn.CreateTable<YCTestApercentModel>();
                    conn.CreateTable<YCTestApercentCalculatedModel>();



                    List<YCTestApercentCalculatedModel> apercentCalcList = null;
                    if (categoryName == null || categoryName == "")
                    {
                        endDate = endDate.AddDays(1);

                        if (shift != "" && process != null)
                        {
                            apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                 (YCTestApercentCalculatedModel.createdate >= startDate
                                                 && YCTestApercentCalculatedModel.createdate < endDate
                                                 && YCTestApercentCalculatedModel.status == true
                                                 && YCTestApercentCalculatedModel.shift == shift
                                                 && YCTestApercentCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                        }
                        else if (shift == "" && process != null)
                        {
                            apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                 (YCTestApercentCalculatedModel.createdate >= startDate
                                                 && YCTestApercentCalculatedModel.createdate < endDate
                                                 && YCTestApercentCalculatedModel.status == true
                                                 && YCTestApercentCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                        }
                        else if (shift != "" && process == null)
                        {
                            apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                (YCTestApercentCalculatedModel.createdate >= startDate
                                                && YCTestApercentCalculatedModel.createdate < endDate
                                                && YCTestApercentCalculatedModel.status == true
                                                && YCTestApercentCalculatedModel.shift == shift)).ToList();
                        }
                        else if (shift == "" && process == null)
                        {
                            apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                (YCTestApercentCalculatedModel.createdate >= startDate
                                                && YCTestApercentCalculatedModel.createdate < endDate
                                                && YCTestApercentCalculatedModel.status == true)).ToList();
                        }



                    }
                    else if (categoryName != null && machineID == Guid.Empty)
                    {
                        endDate = endDate.AddDays(1);

                        if (shift != "" && process != null)
                        {
                            apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                (YCTestApercentCalculatedModel.createdate >= startDate
                                                && YCTestApercentCalculatedModel.createdate < endDate
                                                && YCTestApercentCalculatedModel.machineCategory == categoryName
                                                && YCTestApercentCalculatedModel.status == true
                                                && YCTestApercentCalculatedModel.shift == shift
                                                && YCTestApercentCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                        }
                        else if (shift == "" && process != null)
                        {
                            apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                (YCTestApercentCalculatedModel.createdate >= startDate
                                                && YCTestApercentCalculatedModel.createdate < endDate
                                                && YCTestApercentCalculatedModel.machineCategory == categoryName
                                                && YCTestApercentCalculatedModel.status == true
                                                && YCTestApercentCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                        }
                        else if (shift != "" && process == null)
                        {
                            apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                (YCTestApercentCalculatedModel.createdate >= startDate
                                                && YCTestApercentCalculatedModel.createdate < endDate
                                                && YCTestApercentCalculatedModel.machineCategory == categoryName
                                                && YCTestApercentCalculatedModel.status == true
                                                && YCTestApercentCalculatedModel.shift == shift)).ToList();
                        }
                        else if (shift == "" && process == null)
                        {
                            apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                (YCTestApercentCalculatedModel.createdate >= startDate
                                                && YCTestApercentCalculatedModel.createdate < endDate
                                                && YCTestApercentCalculatedModel.machineCategory == categoryName
                                                && YCTestApercentCalculatedModel.status == true)).ToList();
                        }


                    }
                    else if (categoryName != null && machineID != Guid.Empty)
                    {
                        endDate = endDate.AddDays(1);

                        if (shift != "" && process != null)
                        {
                            apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                 (YCTestApercentCalculatedModel.createdate >= startDate
                                                 && YCTestApercentCalculatedModel.createdate < endDate
                                                 && YCTestApercentCalculatedModel.machineCategory == categoryName
                                                 && YCTestApercentCalculatedModel.machineID == machineID
                                                 && YCTestApercentCalculatedModel.status == true
                                                 && YCTestApercentCalculatedModel.shift == shift
                                                 && YCTestApercentCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                        }
                        else if (shift == "" && process != null)
                        {
                            apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                (YCTestApercentCalculatedModel.createdate >= startDate
                                                && YCTestApercentCalculatedModel.createdate < endDate
                                                && YCTestApercentCalculatedModel.machineCategory == categoryName
                                                && YCTestApercentCalculatedModel.machineID == machineID
                                                && YCTestApercentCalculatedModel.status == true
                                                && YCTestApercentCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                        }
                        else if (shift != "" && process == null)
                        {
                            apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                (YCTestApercentCalculatedModel.createdate >= startDate
                                                && YCTestApercentCalculatedModel.createdate < endDate
                                                && YCTestApercentCalculatedModel.machineCategory == categoryName
                                                && YCTestApercentCalculatedModel.machineID == machineID
                                                && YCTestApercentCalculatedModel.status == true
                                                && YCTestApercentCalculatedModel.shift == shift)).ToList();
                        }
                        else if (shift == "" && process == null)
                        {
                            apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                (YCTestApercentCalculatedModel.createdate >= startDate
                                                && YCTestApercentCalculatedModel.createdate < endDate
                                                && YCTestApercentCalculatedModel.machineCategory == categoryName
                                                && YCTestApercentCalculatedModel.machineID == machineID
                                                && YCTestApercentCalculatedModel.status == true)).ToList();
                        }


                    }

                    //List<YCTestApercentCalculatedModel> apercentCalcList = null;

                    //apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(
                    //      YCTestApercentCalculatedModel =>
                    //      (YCTestApercentCalculatedModel.status == true)).ToList();





                    if (apercentCalcList.Count == 0)
                    {
                        DisplayAlert("Notice", "No records to display!!!", "OK");
                        return;
                    }

                    foreach (YCTestApercentCalculatedModel apercentCalc in apercentCalcList)
                    {
                        OverallApercentReportModelView report = new OverallApercentReportModelView();
                        List<YCTestApercentModel> yctestApercentlist_nMinus1 = conn.Table<YCTestApercentModel>().Where(
                            YCTestApercentModel =>
                            (YCTestApercentModel.testType == "nMinus1"
                            && YCTestApercentModel.status == true
                            && YCTestApercentModel.testID == apercentCalc.testID)).ToList();
                        List<YCTestApercentModel> yctestApercentlist_N = conn.Table<YCTestApercentModel>().Where(
                            YCTestApercentModel =>
                            (YCTestApercentModel.testType == "N"
                            && YCTestApercentModel.status == true
                            && YCTestApercentModel.testID == apercentCalc.testID)).ToList();
                        List<YCTestApercentModel> yctestApercentlist_nPlus1 = conn.Table<YCTestApercentModel>().Where(
                            YCTestApercentModel =>
                            (YCTestApercentModel.testType == "nPlus1"
                            && YCTestApercentModel.status == true
                            && YCTestApercentModel.testID == apercentCalc.testID)).ToList();
                        if (yctestApercentlist_nMinus1 != null && yctestApercentlist_N != null && yctestApercentlist_nPlus1 != null)
                        {

                            int loopCount = 0;
                            foreach (YCTestApercentModel test in yctestApercentlist_nMinus1)
                            {
                                ApercentReportModelView apercentReportMV = new ApercentReportModelView()
                                {
                                    testID = test.testID,
                                    description = test.testcount.ToString(),
                                    nMinus1 = formatDecimal(test.yarnweight),
                                    N = formatDecimal(yctestApercentlist_N[loopCount].yarnweight),
                                    nPlus1 = formatDecimal(yctestApercentlist_nPlus1[loopCount].yarnweight),
                                };
                                report.Add(apercentReportMV);
                                loopCount += 1;
                            }

                            ApercentReportModelView apercentReportModelView = new ApercentReportModelView()
                            {
                                testID = apercentCalc.testID,
                                description = "Average Weight",
                                nMinus1 = formatDecimal(apercentCalc.avg_weight_nMinus1),
                                N = formatDecimal(apercentCalc.avg_weight_N),
                                nPlus1 = formatDecimal(apercentCalc.avg_weight_nPlus1),
                            };
                            report.Add(apercentReportModelView);

                            apercentReportModelView = new ApercentReportModelView()
                            {
                                testID = apercentCalc.testID,
                                description = "Weight (Max)",
                                nMinus1 = formatDecimal(apercentCalc.max_nMinus1),
                                N = formatDecimal(apercentCalc.max_N),
                                nPlus1 = formatDecimal(apercentCalc.max_nPlus1),
                            };
                            report.Add(apercentReportModelView);

                            apercentReportModelView = new ApercentReportModelView()
                            {
                                testID = apercentCalc.testID,
                                description = "Weight (Min)",
                                nMinus1 = formatDecimal(apercentCalc.min_nMinus1),
                                N = formatDecimal(apercentCalc.min_N),
                                nPlus1 = formatDecimal(apercentCalc.min_nPlus1),
                            };
                            report.Add(apercentReportModelView);

                            apercentReportModelView = new ApercentReportModelView()
                            {
                                testID = apercentCalc.testID,
                                description = "Range",
                                nMinus1 = formatDecimal(apercentCalc.range_nMinus1),
                                N = formatDecimal(apercentCalc.range_N),
                                nPlus1 = formatDecimal(apercentCalc.range_nPlus1),
                            };
                            report.Add(apercentReportModelView);

                            apercentReportModelView = new ApercentReportModelView()
                            {
                                testID = apercentCalc.testID,
                                description = "HANK",
                                nMinus1 = formatDecimal(apercentCalc.testaverage_nMinus1),
                                N = formatDecimal(apercentCalc.testaverage_N),
                                nPlus1 = formatDecimal(apercentCalc.testaverage_nPlus1),
                            };
                            report.Add(apercentReportModelView);

                            apercentReportModelView = new ApercentReportModelView()
                            {
                                testID = apercentCalc.testID,
                                description = "SD",
                                nMinus1 = formatDecimal(apercentCalc.testsd_nMinus1),
                                N = formatDecimal(apercentCalc.testsd_N),
                                nPlus1 = formatDecimal(apercentCalc.testsd_nPlus1),
                            };
                            report.Add(apercentReportModelView);

                            apercentReportModelView = new ApercentReportModelView()
                            {
                                testID = apercentCalc.testID,
                                description = "CV",
                                nMinus1 = formatDecimal(apercentCalc.testcv_nMinus1),
                                N = formatDecimal(apercentCalc.testcv_N),
                                nPlus1 = formatDecimal(apercentCalc.testcv_nPlus1),
                            };
                            report.Add(apercentReportModelView);


                            report.testID = apercentCalc.testID;
                            report.userName = apercentCalc.userName;
                            report.machineCategory = apercentCalc.machineCategory;
                            report.machineName = apercentCalc.machineName;
                            report.shift = apercentCalc.shift;
                            report.process = apercentCalc.process;
                            report.countsysname = apercentCalc.countsysname;
                            report.yarnlenunit = apercentCalc.yarnlenunit;
                            report.yarnlength = apercentCalc.yarnlength;
                            report.totaltestcount = apercentCalc.totaltestcount;
                            report.testaverage_nMinus1 = formatDecimal(apercentCalc.testaverage_nMinus1);
                            report.testsd_nMinus1 = formatDecimal(apercentCalc.testsd_nMinus1);
                            report.testcv_nMinus1 = formatDecimal(apercentCalc.testcv_nMinus1);
                            //report.max_nMinus1 = apercentCalc.max_nMinus1;
                            //report.min_nMinus1 = apercentCalc.min_nMinus1;
                            //report.range_nMinus1 = apercentCalc.range_nMinus1;
                            report.apercent_nMinus1 = formatDecimal(apercentCalc.apercent_nMinus1);
                            report.testaverage_N = formatDecimal(apercentCalc.testaverage_N);
                            report.testsd_N = formatDecimal(apercentCalc.testsd_N);
                            report.testcv_N = formatDecimal(apercentCalc.testcv_N);
                            //report.max_N = apercentCalc.max_N;
                            //report.min_N = apercentCalc.min_N;
                            //report.range_N = apercentCalc.range_N;
                            report.testaverage_nPlus1 = formatDecimal(apercentCalc.testaverage_nPlus1);
                            report.testsd_nPlus1 = formatDecimal(apercentCalc.testsd_nPlus1);
                            report.testcv_nPlus1 = formatDecimal(apercentCalc.testcv_nPlus1);
                            //report.max_nPlus1 = apercentCalc.max_nPlus1;
                            //report.min_nPlus1 = apercentCalc.min_nPlus1;
                            //report.range_nPlus1 = apercentCalc.range_nPlus1;
                            report.apercent_nPlus1 = formatDecimal(apercentCalc.apercent_nPlus1);
                            report.createdate = apercentCalc.createdate;
                        }
                        OVS.Add(report);
                    }
                    ListOfReport = OVS;
                }
                listview_tcreport.ItemsSource = null;
                listview_tcreport.ItemsSource = ListOfReport;
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!! Error:" + ex.Message.ToString(), "OK");
            }
        }

        private async Task resetBtn()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                img_notification.IsVisible = false;
                btn_saveToPDF.IsEnabled = true;
                btn_saveToPDF.BackgroundColor = Color.FromHex(BLUE);
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
                List<OverallApercentReportModelView> overallReportList = (List<OverallApercentReportModelView>)listview_tcreport.ItemsSource;
                PdfLayoutResult result = null;
                //PdfLayoutResult resultInfo = null;
                float overallHeight = 0;
                int tableNo = 1;
                bool newPageAdded_Header = false;
                bool newPageAdded_Body = false;
                foreach (OverallApercentReportModelView orl in overallReportList)
                {

                    List<ApercentReportModelView> testList = orl.apercentReportMV;

                    //if (tableNo == int.Parse(entry_reportNo.Text.Trim())) break;
                    PdfGrid pdfGridInfo = new PdfGrid();
                    pdfGridInfo.RepeatHeader = true;
                    pdfGridInfo.Columns.Add(5);
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    //pdfGridInfo.Rows.Add();

                    if (tableNo == 1)
                    {
                        //pdfGridInfo.Rows[0].Cells[0].Value = selectedCompanyName;
                        //pdfGridInfo.Rows[0].Cells[0].ColumnSpan = 4;
                        //pdfGridInfo.Rows[0].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                        //pdfGridInfo.Rows[0].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        //pdfGridInfo.Rows[0].Cells[0].Style.BackgroundBrush = PdfBrushes.Blue;
                        //pdfGridInfo.Rows[0].Cells[0].Style.TextPen = PdfPens.White;
                        //pdfGridInfo.Rows[0].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 18);
                    }
                    pdfGridInfo.Rows[1].Cells[0].Value = "Test ID: " + orl.testID;
                    pdfGridInfo.Rows[1].Cells[0].ColumnSpan = 1;
                    pdfGridInfo.Rows[1].Cells[1].Value = "Machine: " + orl.machineName + "[" + orl.machineCategory + "]";
                    pdfGridInfo.Rows[1].Cells[1].ColumnSpan = 3;
                    pdfGridInfo.Rows[2].Cells[0].Value = "Count System: " + orl.countsysname;
                    pdfGridInfo.Rows[2].Cells[1].Value = "Length Unit: " + orl.yarnlenunit;
                    pdfGridInfo.Rows[2].Cells[2].Value = "Length: " + orl.yarnlength;
                    pdfGridInfo.Rows[2].Cells[3].Value = "Total Test: " + orl.totaltestcount;
                    pdfGridInfo.Rows[3].Cells[0].Value = "A% (N-1): " + formatDecimal(orl.apercent_nMinus1).ToString();
                    //pdfGridInfo.Rows[4].Cells[0].Style.TextPen = PdfPens.Red;
                    pdfGridInfo.Rows[3].Cells[1].Value = "A% (N+1): " + formatDecimal(orl.apercent_nPlus1).ToString();
                    //pdfGridInfo.Rows[4].Cells[1].Style.TextPen = PdfPens.Red;
                    pdfGridInfo.Rows[4].Cells[0].Value = "Date: " + orl.createdate;
                    pdfGridInfo.Rows[4].Cells[1].Value = "Tester: " + orl.userName;
                    pdfGridInfo.Rows[4].Cells[1].ColumnSpan = 2;
                    pdfGridInfo.Rows[4].Cells[3].Value = "Shift: " + orl.shift;
                    pdfGridInfo.Rows[4].Cells[4].Value = "Process: " + orl.process;

                    pdfGridInfo.Rows[0].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[0].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[0].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[0].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[0].Cells[4].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[4].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[4].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[4].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[4].Style.Borders.All = PdfPens.Transparent;
                    //pdfGridInfo.Rows[5].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    //pdfGridInfo.Rows[5].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    //pdfGridInfo.Rows[5].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    //pdfGridInfo.Rows[5].Cells[3].Style.Borders.All = PdfPens.Transparent;

                    int totalRow_header = 6;
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
                    pdfGrid.Rows[0].Cells[1].Value = "N-1";
                    pdfGrid.Rows[0].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[1].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[1].Style.TextPen = PdfPens.Black;
                    pdfGrid.Rows[0].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    pdfGrid.Rows[0].Cells[2].Value = "N";
                    pdfGrid.Rows[0].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[2].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[2].Style.TextPen = PdfPens.Black;
                    pdfGrid.Rows[0].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    //pdfGrid.Rows[0].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    //pdfGrid.Rows[0].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    //pdfGrid.Rows[0].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGrid.Rows[0].Cells[3].Value = "N+1";
                    pdfGrid.Rows[0].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[3].Style.BackgroundBrush = PdfBrushes.LightGray;
                    pdfGrid.Rows[0].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);



                    int rowCount = 1;
                    foreach (ApercentReportModelView test in testList)
                    {
                        row = new PdfGridRow(pdfGrid);
                        pdfGrid.Rows.Add(row);
                        pdfGrid.Rows[rowCount].Cells[0].Value = test.description.ToString();
                        pdfGrid.Rows[rowCount].Cells[1].Value = formatDecimal(test.nMinus1).ToString();
                        pdfGrid.Rows[rowCount].Cells[2].Value = formatDecimal(test.N).ToString();
                        pdfGrid.Rows[rowCount].Cells[3].Value = formatDecimal(test.nPlus1).ToString();
                        pdfGrid.Rows[rowCount].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[rowCount].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[rowCount].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[rowCount].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
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
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream, "TQM_Report(A Percent).pdf");
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
                PdfFont font_rn = new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Underline);
                PdfBrush brush_rn = new PdfSolidBrush(Syncfusion.Drawing.Color.Blue);
                header.Graphics.DrawString("A% Report - " + DateTime.Now.ToString(), font_rn, brush_rn, new PointF(165, 16));
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
                    string fileName = "TQM_Report(A Percent).pdf";
                    string root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                    Java.IO.File myDir = new Java.IO.File(root + "/TQMDownloads");
                    Java.IO.File file = new Java.IO.File(myDir, fileName);
                    string filePath = file.Path;
                    var client = new RestClient("https://myconsoleerp.herokuapp.com/tqmreport/upload");
                    var request = new RestRequest();
                    request.Method = Method.Post;
                    //request.Timeout = Timeout.Infinite;
                    request.AddParameter("userName", "tqmuser");
                    request.AddParameter("uploadedby", companyName);
                    request.AddParameter("title", "TQMReports(A Percent)-" + DateTime.Now.ToString());
                    request.AddFile("reportpath", filePath);
                    RestResponse response = client.Execute(request);
                    if (response.IsSuccessful)
                    {
                        showAlert("Report upload is sucessful!!!");
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
    }
}