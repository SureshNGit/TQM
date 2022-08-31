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

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NoilsReport : ContentPage
    {

        private List<OverallNoilsReportModelView> _listOfReports;
        public List<OverallNoilsReportModelView> ListOfReport { get { return _listOfReports; } set { _listOfReports = value; base.OnPropertyChanged(); } }
        private string selectedCompanyName = null;
        private const string BLUE = "#0e0273";
        public NoilsReport()
        {
            InitializeComponent();
        }

        public NoilsReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID)
        {
            InitializeComponent();
            getReport(startDate, endDate, categoryName, machineID);
        }

        private void getReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID)
        {
            try
            {
                List<OverallNoilsReportModelView> OVS = new List<OverallNoilsReportModelView>();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {

                    conn.CreateTable<NoilsTestModel>();
                    conn.CreateTable<NoilsTestCalculatedModel>();

                    List<NoilsTestCalculatedModel> noilsCalcList = null;
                    if (categoryName == null || categoryName == "")
                    {
                        endDate = endDate.AddDays(1);
                        noilsCalcList = conn.Table<NoilsTestCalculatedModel>().Where(NoilsTestCalculatedModel =>
                        NoilsTestCalculatedModel.createdate >= startDate &&
                        NoilsTestCalculatedModel.createdate < endDate &&
                        NoilsTestCalculatedModel.status == true).ToList();
                    }
                    else if (categoryName != null && machineID == Guid.Empty)
                    {
                        endDate = endDate.AddDays(1);
                        noilsCalcList = conn.Table<NoilsTestCalculatedModel>().Where(NoilsTestCalculatedModel =>
                           (NoilsTestCalculatedModel.createdate >= startDate && NoilsTestCalculatedModel.createdate < endDate
                           && NoilsTestCalculatedModel.machineCategory == categoryName) && NoilsTestCalculatedModel.status == true).ToList();
                    }
                    else if (categoryName != null && machineID != Guid.Empty)
                    {
                        endDate = endDate.AddDays(1);
                        noilsCalcList = conn.Table<NoilsTestCalculatedModel>().Where(NoilsTestCalculatedModel =>
                           (NoilsTestCalculatedModel.createdate >= startDate && NoilsTestCalculatedModel.createdate < endDate
                           && NoilsTestCalculatedModel.machineCategory == categoryName)
                           && NoilsTestCalculatedModel.machineID == machineID && NoilsTestCalculatedModel.status == true).ToList();
                    }

                    //List<StretchTestCalculatedModel> stretchCalcList = null;

                    //stretchCalcList = conn.Table<StretchTestCalculatedModel>().Where(
                    //      StretchTestCalculatedModel =>
                    //      (StretchTestCalculatedModel.status == true)).ToList();

                    if (noilsCalcList.Count == 0)
                    {
                        DisplayAlert("Notice", "No records to display!!!", "OK");
                        return;
                    }

                    foreach (NoilsTestCalculatedModel noilsCalc in noilsCalcList)
                    {
                        OverallNoilsReportModelView report = new OverallNoilsReportModelView();
                        List<NoilsTestFinalModel> list_finalNoils = conn.Table<NoilsTestFinalModel>().Where(
                            NoilsTestFinalModel =>
                            (NoilsTestFinalModel.testID == noilsCalc.testID && NoilsTestFinalModel.status == true)).ToList();


                        if (list_finalNoils != null)
                        {

                            int loopCount = 0;
                            foreach (NoilsTestFinalModel test in list_finalNoils)
                            {
                                NoilsReportModelView noilsReportMV = new NoilsReportModelView()
                                {
                                    testID = test.testID,
                                    description = test.testcount.ToString(),
                                    silver_wt = test.weigth_sliver,
                                    noils_wt = test.weigth_noils,
                                    noils = test.noils
                                };
                                report.Add(noilsReportMV);
                                loopCount += 1;
                            }

                            NoilsReportModelView noilsReportModelView = new NoilsReportModelView()
                            {
                                testID = noilsCalc.testID,
                                description = "Average Weight",
                                silver_wt = noilsCalc.average_wt_sliverwt,
                                noils_wt = noilsCalc.average_wt_noilswt,
                                noils = noilsCalc.average_wt_noils
                            };
                            report.Add(noilsReportModelView);

                            noilsReportModelView = new NoilsReportModelView()
                            {
                                testID = noilsCalc.testID,
                                description = "Weight (Max)",
                                silver_wt = noilsCalc.max_sliverwt,
                                noils_wt = noilsCalc.max_noilswt,
                                noils = noilsCalc.max_noils
                            };
                            report.Add(noilsReportModelView);

                            noilsReportModelView = new NoilsReportModelView()
                            {
                                testID = noilsCalc.testID,
                                description = "Weight (Min)",
                                silver_wt = noilsCalc.min_sliverwt,
                                noils_wt = noilsCalc.min_noilswt,
                                noils = noilsCalc.min_noils
                            };
                            report.Add(noilsReportModelView);

                            noilsReportModelView = new NoilsReportModelView()
                            {
                                testID = noilsCalc.testID,
                                description = "Range",
                                silver_wt = noilsCalc.range_sliverwt,
                                noils_wt = noilsCalc.range_noilswt,
                                noils = noilsCalc.range_noils
                            };
                            report.Add(noilsReportModelView);

                            //noilsReportModelView = new NoilsReportModelView()
                            //{
                            //    testID = noilsCalc.testID,
                            //    description = "HANK",
                            //    silver_wt = noilsCalc.testaverage_sliverwt,
                            //    noils_wt = noilsCalc.testaverage_noilswt,
                            //    noils = 0.00m
                            //};
                            //report.Add(noilsReportModelView);

                            noilsReportModelView = new NoilsReportModelView()
                            {
                                testID = noilsCalc.testID,
                                description = "SD",
                                silver_wt = noilsCalc.testsd_sliverwt,
                                noils_wt = noilsCalc.testsd_noilswt,
                                noils = noilsCalc.testsd_noils
                            };
                            report.Add(noilsReportModelView);

                            noilsReportModelView = new NoilsReportModelView()
                            {
                                testID = noilsCalc.testID,
                                description = "CV",
                                silver_wt = noilsCalc.testcv_sliverwt,
                                noils_wt = noilsCalc.testcv_noilswt,
                                noils = noilsCalc.testcv_noils
                            };
                            report.Add(noilsReportModelView);


                            report.testID = noilsCalc.testID;
                            report.userName = noilsCalc.userName;
                            report.machineCategory = noilsCalc.machineCategory;
                            report.machineName = noilsCalc.machineName;
                            report.countsysname = noilsCalc.countsysname;
                            report.yarnlenunit = noilsCalc.yarnlenunit;
                            report.yarnlength = noilsCalc.yarnlength;
                            report.totaltestcount = noilsCalc.totaltestcount;
                            report.createdate = noilsCalc.createdate;
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
                List<OverallNoilsReportModelView> overallReportList = (List<OverallNoilsReportModelView>)listview_tcreport.ItemsSource;
                PdfLayoutResult result = null;
                //PdfLayoutResult resultInfo = null;
                float overallHeight = 0;
                int tableNo = 1;
                bool newPageAdded_Header = false;
                bool newPageAdded_Body = false;
                foreach (OverallNoilsReportModelView orl in overallReportList)
                {

                    List<NoilsReportModelView> testList = orl.noilsTestList;

                    //if (tableNo == int.Parse(entry_reportNo.Text.Trim())) break;
                    PdfGrid pdfGridInfo = new PdfGrid();
                    pdfGridInfo.RepeatHeader = true;
                    pdfGridInfo.Columns.Add(4);
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    pdfGridInfo.Rows.Add();
                    //pdfGridInfo.Rows.Add();
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
                    //pdfGridInfo.Rows[3].Cells[0].Value = "Stretch %: " + orl.stretch;
                    //pdfGridInfo.Rows[4].Cells[0].Style.TextPen = PdfPens.Red;
                    //pdfGridInfo.Rows[3].Cells[1].Value = "A% (N+1): " + orl.apercent_nPlus1;
                    //pdfGridInfo.Rows[4].Cells[1].Style.TextPen = PdfPens.Red;
                    pdfGridInfo.Rows[3].Cells[0].Value = "Date: " + orl.createdate;
                    pdfGridInfo.Rows[3].Cells[1].Value = "Tester: " + orl.userName;
                    pdfGridInfo.Rows[3].Cells[1].ColumnSpan = 2;


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
                    //pdfGridInfo.Rows[4].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    //pdfGridInfo.Rows[4].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    //pdfGridInfo.Rows[4].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    //pdfGridInfo.Rows[4].Cells[3].Style.Borders.All = PdfPens.Transparent;
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

                    pdfGrid.Rows[0].Cells[1].Value = "Sliver Wt";
                    pdfGrid.Rows[0].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[1].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[1].Style.TextPen = PdfPens.Black;
                    pdfGrid.Rows[0].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);

                    pdfGrid.Rows[0].Cells[2].Value = "Noils Wt";
                    pdfGrid.Rows[0].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[2].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[2].Style.TextPen = PdfPens.Black;
                    pdfGrid.Rows[0].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);

                    //pdfGrid.Rows[0].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    //pdfGrid.Rows[0].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    //pdfGrid.Rows[0].Cells[2].Style.Borders.All = PdfPens.Transparent;

                    pdfGrid.Rows[0].Cells[3].Value = "Noils %";
                    pdfGrid.Rows[0].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[3].Style.BackgroundBrush = PdfBrushes.LightGray;
                    pdfGrid.Rows[0].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);



                    int rowCount = 1;
                    foreach (NoilsReportModelView test in testList)
                    {
                        row = new PdfGridRow(pdfGrid);
                        pdfGrid.Rows.Add(row);
                        pdfGrid.Rows[rowCount].Cells[0].Value = test.description.ToString();
                        pdfGrid.Rows[rowCount].Cells[1].Value = test.silver_wt.ToString();
                        pdfGrid.Rows[rowCount].Cells[2].Value = test.noils_wt.ToString();
                        pdfGrid.Rows[rowCount].Cells[3].Value = test.noils.ToString();
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
                        overallHeight = overallHeight + result.Bounds.Height + 30;
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
                                result = pdfGrid.Draw(pdfPage, new PointF(10, overallHeight + 10), layoutFormat);
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
                                    result = pdfGrid.Draw(result.Page, new PointF(10, (overallHeight + 10)));
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
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream, "TQM_Report(NOILS).pdf");
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
                PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20);
                PdfBrush brush = new PdfSolidBrush(Syncfusion.Drawing.Color.Blue);
                header.Alignment = PdfAlignmentStyle.TopCenter;
                header.Graphics.DrawString(companyName, font, brush, new PointF(10, 0));
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
                    string fileName = "TQM_Report(NOILS).pdf";
                    string root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                    Java.IO.File myDir = new Java.IO.File(root + "/TQMDownloads");
                    Java.IO.File file = new Java.IO.File(myDir, fileName);
                    string filePath = file.Path;
                    var client = new RestClient("https://myconsoleerp.herokuapp.com/tqmreport/upload");
                    var request = new RestRequest();
                    request.Method = Method.Post;
                    //request.Timeout = Timeout.Infinite;
                    request.AddParameter("uploadedby", companyName);
                    request.AddParameter("title", "TQMReports(NOILS)-" + DateTime.Now.ToString());
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
    }
}