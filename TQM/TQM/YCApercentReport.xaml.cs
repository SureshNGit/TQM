using CsvHelper;
using CsvHelper.Configuration;
using RestSharp;
using SQLite;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
    public partial class YCApercentReport : ContentPage
    {

        private List<OverallApercentReportModelView> _listOfReports;
        public List<OverallApercentReportModelView> ListOfReport { get { return _listOfReports; } set { _listOfReports = value; base.OnPropertyChanged(); } }

        private List<YCTestConsolidatedApercentReportMV> _listOfConsolidatedReports;
        public List<YCTestConsolidatedApercentReportMV> ListOfConsolidatedReports { get { return _listOfConsolidatedReports; } set { _listOfConsolidatedReports = value; base.OnPropertyChanged(); } }

        private string selectedCompanyName = null;
        private const string BLUE = "#0e0273";
        private RunConfiguration runConfiguration = new RunConfiguration();
        private List<YCTestApercentCalculatedModel> deleteList = null;
        private bool deleteAll = false;
        private DateTime reportStartDate;
        private DateTime reportEndDate;
        private bool consolidatedReport = false;
        private string CON_UF_NAME_1 = null;
        private string CON_UF_NAME_2 = null;
        private string CON_UF_NAME_3 = null;
        private string CON_UF_NAME_4 = null;
        private string CON_UF_VAL_1 = null;
        private string CON_UF_VAL_2 = null;
        private string CON_UF_VAL_3 = null;
        private string CON_UF_VAL_4 = null;
        private string selectedMachineCategory = null;
        private bool isFinalAvgRowPresent = false;

        public YCApercentReport()
        {
            InitializeComponent();
        }

        public YCApercentReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string process, string testID, string matType, string materialLength, bool deleteRequest, bool isConsolidated, string UFVAL1, string UFVAL2, string UFVAL3, string UFVAL4)
        {
            InitializeComponent();
            isFinalAvgRowPresent = false;
            consolidatedReport = isConsolidated;
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
            getReport(startDate, endDate, categoryName, machineID, shift, process, testID, matType, materialLength, deleteRequest, UFVAL1, UFVAL2, UFVAL3, UFVAL4);
        }

        private void getReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string process, string testID, string matType, string materialLength, bool deleteRequest,string UFVAL1, string UFVAL2, string UFVAL3, string UFVAL4)
        {
            try
            {
                List<OverallApercentReportModelView> OVS = new List<OverallApercentReportModelView>();
                List<YCTestConsolidatedApercentReportMV> OverallConsolidatedReports = new List<YCTestConsolidatedApercentReportMV>();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {

                    conn.CreateTable<YCTestApercentModel>();
                    conn.CreateTable<YCTestApercentCalculatedModel>();



                    List<YCTestApercentCalculatedModel> apercentCalcList = null;
                    if (testID != "")
                    {
                        if (!testID.Contains("."))
                        {
                            long givenTestId = long.Parse(testID);
                            apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(t => t.testID == givenTestId).ToList();
                        }
                        else
                        {
                            long startTestID = long.Parse(testID.Split('.')[0]);
                            long endTestID = long.Parse(testID.Split('.')[1]);

                            if (startTestID == endTestID)
                            {
                                apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                 YCTestApercentCalculatedModel.testID == startTestID).ToList();
                            }
                            else
                            {
                                for (long i = startTestID; i <= endTestID; i++)
                                {
                                    if (apercentCalcList == null)
                                    {
                                        apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                 YCTestApercentCalculatedModel.testID == i).ToList();
                                    }
                                    else
                                    {
                                        List<YCTestApercentCalculatedModel> tempList = conn.Table<YCTestApercentCalculatedModel>()
                                                                    .Where(YCTestApercentCalculatedModel =>
                                                                            YCTestApercentCalculatedModel.testID == i).ToList();
                                        //stretchCalcList.Concat(tempList).ToList();
                                        apercentCalcList.AddRange(tempList);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        DateTime actualEndDate = endDate;

                        endDate = actualEndDate.AddDays(1);
                        DateTime endDatePlusOne = endDate.AddDays(1);
                        //Parent List
                        List<YCTestApercentCalculatedModel> parentList = conn.Table<YCTestApercentCalculatedModel>().Where(YCTestApercentCalculatedModel =>
                                                (YCTestApercentCalculatedModel.createdate >= startDate
                                                && YCTestApercentCalculatedModel.createdate < endDate))
                                                .OrderBy(YCTestApercentCalculatedModel => YCTestApercentCalculatedModel.createdate).ToList();
                        if (parentList.Count == 0)
                        {
                            DisplayAlert("Notice", "No records to display!!!", "OK");
                            return;
                        }

                        //Start of Logic to check last shift for the given end date is logged in end date + 1 day date
                        //Get first record of actual end date + 1 day
                        List<YCTestApercentCalculatedModel> recs_actualEndDatePlusOne = conn.Table<YCTestApercentCalculatedModel>()
                                                                        .Where(YCTestApercentCalculatedModel =>
                                                                        (YCTestApercentCalculatedModel.createdate >= endDate
                                                                        && YCTestApercentCalculatedModel.createdate < endDatePlusOne))
                                                                        .OrderBy(YCTestApercentCalculatedModel => YCTestApercentCalculatedModel.createdate).ToList();
                        if (recs_actualEndDatePlusOne.Count > 0)
                        {
                            //Check if the 1st record of actual end date + 1 day is not Shift-1
                            if (recs_actualEndDatePlusOne[0].shift != "Shift-1")
                            {
                                YCTestApercentCalculatedModel actualEndDatePlusOne_Shift1_Recs = recs_actualEndDatePlusOne.Where(YCTestApercentCalculatedModel =>
                                                                                             (YCTestApercentCalculatedModel.shift == "Shift-1"))
                                                                                            .OrderBy(YCTestApercentCalculatedModel => YCTestApercentCalculatedModel.createdate)
                                                                                            .FirstOrDefault();
                                //Merge last shift record of actual end date from (actual end date + 1day) with parent list
                                parentList.Concat(recs_actualEndDatePlusOne.Where(YCTestApercentCalculatedModel =>
                                                                            (YCTestApercentCalculatedModel.createdate >= endDate
                                                                            && YCTestApercentCalculatedModel.createdate < actualEndDatePlusOne_Shift1_Recs.createdate
                                                                            && YCTestApercentCalculatedModel.shift == recs_actualEndDatePlusOne[0].shift))
                                                                            .OrderBy(YCTestApercentCalculatedModel => YCTestApercentCalculatedModel.createdate).ToList());
                            }
                        }
                        //End of Logic to check last shift for the given end date is logged in end date + 1 day date

                        //Start of logic to ignore the previous date last shift record from the given actual start date
                        if (parentList[0].shift != "Shift-1")
                        {
                            YCTestApercentCalculatedModel actualStartDate_Shift1_Recs = parentList.Where(YCTestApercentCalculatedModel =>
                                                                                         (YCTestApercentCalculatedModel.shift == "Shift-1"))
                                                                                        .OrderBy(YCTestApercentCalculatedModel => YCTestApercentCalculatedModel.createdate)
                                                                                        .FirstOrDefault();
                            if (actualStartDate_Shift1_Recs != null)
                            {
                                List<YCTestApercentCalculatedModel> lastShiftOfPreviousDay_in_ActualStartDateRecs =
                                                                        parentList.Where(YCTestApercentCalculatedModel =>
                                                                        (YCTestApercentCalculatedModel.shift == parentList[0].shift
                                                                        && YCTestApercentCalculatedModel.createdate < actualStartDate_Shift1_Recs.createdate))
                                                                        .OrderBy(YCTestApercentCalculatedModel => YCTestApercentCalculatedModel.createdate)
                                                                        .ToList();
                                parentList.RemoveAll(i => lastShiftOfPreviousDay_in_ActualStartDateRecs.Contains(i));
                            }
                        }
                        //End of logic to ignore the previous date last shift record from the given actual start date

                        if (categoryName == null || categoryName == "")
                        {
                            //endDate = endDate.AddDays(1);

                            if (shift != "" && process != null)
                            {
                                apercentCalcList = parentList.Where(YCTestApercentCalculatedModel =>
                                                     (YCTestApercentCalculatedModel.shift == shift
                                                     && YCTestApercentCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift == "" && process != null)
                            {
                                apercentCalcList = parentList.Where(YCTestApercentCalculatedModel =>
                                                     (YCTestApercentCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift != "" && process == null)
                            {
                                apercentCalcList = parentList.Where(YCTestApercentCalculatedModel =>
                                                    (YCTestApercentCalculatedModel.shift == shift)).ToList();
                            }
                            else if (shift == "" && process == null)
                            {
                                apercentCalcList = parentList;
                            }



                        }
                        else if (categoryName != null && machineID == Guid.Empty)
                        {
                            //endDate = endDate.AddDays(1);

                            if (shift != "" && process != null)
                            {
                                apercentCalcList = parentList.Where(YCTestApercentCalculatedModel =>
                                                    (YCTestApercentCalculatedModel.machineCategory == categoryName
                                                    && YCTestApercentCalculatedModel.shift == shift
                                                    && YCTestApercentCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift == "" && process != null)
                            {
                                apercentCalcList = parentList.Where(YCTestApercentCalculatedModel =>
                                                    (YCTestApercentCalculatedModel.machineCategory == categoryName
                                                    && YCTestApercentCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift != "" && process == null)
                            {
                                apercentCalcList = parentList.Where(YCTestApercentCalculatedModel =>
                                                    (YCTestApercentCalculatedModel.machineCategory == categoryName
                                                    && YCTestApercentCalculatedModel.shift == shift)).ToList();
                            }
                            else if (shift == "" && process == null)
                            {
                                apercentCalcList = parentList.Where(YCTestApercentCalculatedModel =>
                                                    (YCTestApercentCalculatedModel.machineCategory == categoryName)).ToList();
                            }


                        }
                        else if (categoryName != null && machineID != Guid.Empty)
                        {
                            //endDate = endDate.AddDays(1);

                            if (shift != "" && process != null)
                            {
                                apercentCalcList = parentList.Where(YCTestApercentCalculatedModel =>
                                                     (YCTestApercentCalculatedModel.machineCategory == categoryName
                                                     && YCTestApercentCalculatedModel.machineID == machineID
                                                     && YCTestApercentCalculatedModel.shift == shift
                                                     && YCTestApercentCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift == "" && process != null)
                            {
                                apercentCalcList = parentList.Where(YCTestApercentCalculatedModel =>
                                                    (YCTestApercentCalculatedModel.machineCategory == categoryName
                                                    && YCTestApercentCalculatedModel.machineID == machineID
                                                    && YCTestApercentCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift != "" && process == null)
                            {
                                apercentCalcList = parentList.Where(YCTestApercentCalculatedModel =>
                                                    (YCTestApercentCalculatedModel.machineCategory == categoryName
                                                    && YCTestApercentCalculatedModel.machineID == machineID
                                                    && YCTestApercentCalculatedModel.shift == shift)).ToList();
                            }
                            else if (shift == "" && process == null)
                            {
                                apercentCalcList = parentList.Where(YCTestApercentCalculatedModel =>
                                                    (YCTestApercentCalculatedModel.machineCategory == categoryName
                                                    && YCTestApercentCalculatedModel.machineID == machineID)).ToList();
                            }


                        }
                    }

                    //List<YCTestApercentCalculatedModel> apercentCalcList = null;

                    //apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(
                    //      YCTestApercentCalculatedModel =>
                    //      (YCTestApercentCalculatedModel.status == true)).ToList();


                    if (matType != "" && materialLength != "")
                    {
                        decimal yarnLength = 0.00m;
                        try
                        {
                            yarnLength = decimal.Parse(materialLength);
                        }
                        catch (Exception)
                        {
                            DisplayAlert("Attention", "Invalid unit length!!!", "OK");
                            return;
                        }
                        apercentCalcList = apercentCalcList.Where(YCTestApercentCalculatedModel => (YCTestApercentCalculatedModel.yarnlength == yarnLength
                                                && YCTestApercentCalculatedModel.yarnlenunit == matType)).ToList();
                    }


                    if (apercentCalcList.Count == 0)
                    {
                        DisplayAlert("Notice", "No records to display!!!", "OK");
                        return;
                    }
                    else
                    {
                        if (deleteRequest)
                        {
                            deleteAll = true;
                            deleteList = apercentCalcList;
                        }
                    }


                    if (consolidatedReport)
                    {
                        apercentCalcList = apercentCalcList.OrderBy(YCTestApercentCalculatedModel => YCTestApercentCalculatedModel.machineName).ToList();
                    }

                    int counter = 0;
                    string prev_macID = null;
                    int macTestNo = 0;

                    foreach (YCTestApercentCalculatedModel apercentCalc in apercentCalcList)
                    {
                        OverallApercentReportModelView report = new OverallApercentReportModelView();
                        YCTestConsolidatedApercentReportMV consolItems = new YCTestConsolidatedApercentReportMV();

                        if (consolidatedReport)
                        {
                            if (counter == 0)
                            {
                                //if (apercentCalc.machineCategory == "Spinning" || apercentCalc.machineCategory == "Winding")
                                //{
                                //    //lbl_con_Hank.Text = "Avg. COUNT : ";
                                //    lbl_conStdHank.Text = "Std. Count";
                                //    lbl_conAvgHank.Text = "Avg. Count";
                                //}
                                //else
                                //{
                                //    //lbl_con_Hank.Text = "Avg. HANK : ";
                                //    lbl_conStdHank.Text = "Std. Hank";
                                //    lbl_conAvgHank.Text = "Avg. Hank";
                                //}

                                if (UFVAL1 != "" && UFVAL1 != null)
                                {
                                    YarnCountConfigModel ycConfig_uf = conn.Table<YarnCountConfigModel>().
                                                                                    Where(YarnCountConfigModel => (YarnCountConfigModel.uf_name_1 != null ||
                                                                                    YarnCountConfigModel.uf_name_1 != "")
                                                                                    && YarnCountConfigModel.machineCategory == apercentCalc.machineCategory
                                                                                    && YarnCountConfigModel.machineID == apercentCalc.machineID
                                                                                    && YarnCountConfigModel.machineName == apercentCalc.machineName).FirstOrDefault();
                                    if (ycConfig_uf != null)
                                    {
                                        CON_UF_NAME_1 = ycConfig_uf.uf_name_1;
                                        CON_UF_VAL_1 = UFVAL1;
                                    }
                                    else
                                    {
                                        CON_UF_NAME_1 = null;
                                        CON_UF_VAL_1 = null;
                                    }
                                }
                                if (UFVAL2 != "" && UFVAL2 != null)
                                {
                                    YarnCountConfigModel ycConfig_uf = conn.Table<YarnCountConfigModel>().
                                                                                    Where(YarnCountConfigModel => (YarnCountConfigModel.uf_name_2 != null ||
                                                                                    YarnCountConfigModel.uf_name_2 != "")
                                                                                    && YarnCountConfigModel.machineCategory == apercentCalc.machineCategory
                                                                                    && YarnCountConfigModel.machineID == apercentCalc.machineID
                                                                                    && YarnCountConfigModel.machineName == apercentCalc.machineName).FirstOrDefault();
                                    if (ycConfig_uf != null)
                                    {
                                        CON_UF_NAME_2 = ycConfig_uf.uf_name_2;
                                        CON_UF_VAL_2 = UFVAL2;
                                    }
                                    else
                                    {
                                        CON_UF_NAME_2 = null;
                                        CON_UF_VAL_2 = null;
                                    }
                                }
                                if (UFVAL3 != "" && UFVAL3 != null)
                                {
                                    YarnCountConfigModel ycConfig_uf = conn.Table<YarnCountConfigModel>().
                                                                                    Where(YarnCountConfigModel => (YarnCountConfigModel.uf_name_3 != null ||
                                                                                    YarnCountConfigModel.uf_name_3 != "")
                                                                                    && YarnCountConfigModel.machineCategory == apercentCalc.machineCategory
                                                                                    && YarnCountConfigModel.machineID == apercentCalc.machineID
                                                                                    && YarnCountConfigModel.machineName == apercentCalc.machineName).FirstOrDefault();
                                    if (ycConfig_uf != null)
                                    {
                                        CON_UF_NAME_3 = ycConfig_uf.uf_name_3;
                                        CON_UF_VAL_3 = UFVAL3;
                                    }
                                    else
                                    {
                                        CON_UF_NAME_3 = null;
                                        CON_UF_VAL_3 = null;
                                    }
                                }
                                if (UFVAL4 != "" && UFVAL4 != null)
                                {
                                    YarnCountConfigModel ycConfig_uf = conn.Table<YarnCountConfigModel>().
                                                                                    Where(YarnCountConfigModel => (YarnCountConfigModel.uf_name_4 != null ||
                                                                                    YarnCountConfigModel.uf_name_4 != "")
                                                                                    && YarnCountConfigModel.machineCategory == apercentCalc.machineCategory
                                                                                    && YarnCountConfigModel.machineID == apercentCalc.machineID
                                                                                    && YarnCountConfigModel.machineName == apercentCalc.machineName).FirstOrDefault();
                                    if (ycConfig_uf != null)
                                    {
                                        CON_UF_NAME_4 = ycConfig_uf.uf_name_4;
                                        CON_UF_VAL_4 = UFVAL4;
                                    }
                                    else
                                    {
                                        CON_UF_NAME_4 = null;
                                        CON_UF_VAL_4 = null;
                                    }
                                }
                            }

                            YarnCountConfigModel ycConfig_uf_1 = conn.Table<YarnCountConfigModel>().
                                                                                    Where(YarnCountConfigModel => (
                                                                                    YarnCountConfigModel.machineCategory == apercentCalc.machineCategory
                                                                                    && YarnCountConfigModel.machineID == apercentCalc.machineID
                                                                                    && YarnCountConfigModel.machineName == apercentCalc.machineName)).FirstOrDefault();
                            if (ycConfig_uf_1 == null)
                            {
                                DisplayAlert("Notice", "Unable to reterive user fields from settings!!!", "OK");
                                return;
                            }

                            consolItems.uf_name_1 = ycConfig_uf_1.uf_name_1;
                            consolItems.uf_name_2 = ycConfig_uf_1.uf_name_2;
                            consolItems.uf_name_3 = ycConfig_uf_1.uf_name_3;
                            consolItems.uf_name_4 = ycConfig_uf_1.uf_name_4;

                            consolItems.uf_value_1 = apercentCalc.uf_value_1;
                            consolItems.uf_value_2 = apercentCalc.uf_value_2;
                            consolItems.uf_value_3 = apercentCalc.uf_value_3;
                            consolItems.uf_value_4 = apercentCalc.uf_value_4;

                            //decimal maxRangeVal = testsummary.standardHank + (testsummary.standardHank * (Convert.ToDecimal(testsummary.deviationPercent) / 100));
                            //decimal minRangeVal = testsummary.standardHank - (testsummary.standardHank * (Convert.ToDecimal(testsummary.deviationPercent) / 100));

                            decimal maxRangeVal = apercentCalc.standardApercent;
                            decimal minRangeVal = Decimal.Parse("-"+apercentCalc.standardApercent.ToString());


                            if (apercentCalc.apercent_nMinus1 < minRangeVal || apercentCalc.apercent_nMinus1 > maxRangeVal)
                            {
                                consolItems.isRed_Nminus1 = true;
                                consolItems.isWhite_Nminus1 = false;
                            }
                            else
                            {
                                consolItems.isRed_Nminus1 = false;
                                consolItems.isWhite_Nminus1 = true;
                            }

                            if (apercentCalc.apercent_nPlus1 < minRangeVal || apercentCalc.apercent_nPlus1 > maxRangeVal)
                            {
                                consolItems.isRed_Nplus1 = true;
                                consolItems.isWhite_Nplus1 = false;
                            }
                            else
                            {
                                consolItems.isRed_Nplus1 = false;
                                consolItems.isWhite_Nplus1 = true;
                            }

                            if (prev_macID == null)
                            {
                                prev_macID = apercentCalc.machineID.ToString();
                                macTestNo = 1;
                            }
                            else
                            {
                                if (prev_macID != apercentCalc.machineID.ToString())
                                {
                                    prev_macID = apercentCalc.machineID.ToString();
                                    macTestNo = 1;
                                }
                                else
                                {
                                    macTestNo = macTestNo + 1;
                                }
                            }

                            consolItems.serialNo = (counter + 1).ToString();
                            consolItems.testNo = macTestNo.ToString();
                            consolItems.testID = apercentCalc.testID.ToString();
                            consolItems.machineName = apercentCalc.machineName;
                            consolItems.testDate = apercentCalc.createdate.Day.ToString() + "-" +
                                                    apercentCalc.createdate.Month.ToString() + "-" +
                                                    apercentCalc.createdate.Year.ToString() + "\n" +
                                                    formatTime(apercentCalc.createdate);
                            consolItems.shift = apercentCalc.shift;
                            consolItems.standardValue = "\u00B1" + formatDecimal(apercentCalc.standardApercent, 2).ToString() ;

                            consolItems.Nminus1 = formatDecimal(apercentCalc.apercent_nMinus1).ToString();
                            consolItems.Nplus1 = formatDecimal(apercentCalc.apercent_nPlus1).ToString();

                            //consolItems.testDuration = testsummary.testDuration;
                            //consolItems.testDuration = formatTime(apercentCalc.createdate);

                            string userParams = "";

                            if (apercentCalc.uf_value_1 != null && apercentCalc.uf_value_1 != "")
                            {
                                userParams = apercentCalc.uf_value_1;
                            }

                            if (apercentCalc.uf_value_2 != null && apercentCalc.uf_value_2 != "")
                            {
                                if (userParams == "")
                                {
                                    userParams = apercentCalc.uf_value_2;
                                }
                                else
                                {
                                    userParams = userParams + "\n" + apercentCalc.uf_value_2;
                                }
                            }

                            if (apercentCalc.uf_value_3 != null && apercentCalc.uf_value_3 != "")
                            {
                                if (userParams == "")
                                {
                                    userParams = apercentCalc.uf_value_3;
                                }
                                else
                                {
                                    userParams = userParams + "\n" + apercentCalc.uf_value_3;
                                }
                            }

                            if (apercentCalc.uf_value_4 != null && apercentCalc.uf_value_4 != "")
                            {
                                if (userParams == "")
                                {
                                    userParams = apercentCalc.uf_value_4;
                                }
                                else
                                {
                                    userParams = userParams + "\n" + apercentCalc.uf_value_4;
                                }
                            }

                            consolItems.testDuration = userParams;

                            consolItems.remarks = apercentCalc.testRemark;

                            //if (apercentCalc.machineCategory == "Spinning" || apercentCalc.machineCategory == "Winding")
                            //{
                            //    consolItems.isSpinning = true;
                            //    consolItems.otherThanSpinning = false;
                            //}
                            //else
                            //{
                            //    consolItems.isSpinning = false;
                            //    consolItems.otherThanSpinning = true;
                            //}

                            //YCTestModel firstTest = yctestlist.Where(YCTestModel => YCTestModel.testcount == 1).FirstOrDefault();
                            //TimeSpan duration = (firstTest.createdate - testsummary.createdate).Duration();
                            //consolItems.testDuration = duration.Hours.ToString() + ":" + duration.Minutes.ToString() + ":" + duration.Seconds.ToString();


                            counter++;
                        }
                        else
                        {
                            List<YCTestApercentModel> yctestApercentlist_nMinus1 = conn.Table<YCTestApercentModel>().Where(
                            YCTestApercentModel =>
                            (YCTestApercentModel.testType == "nMinus1"
                            //&& YCTestApercentModel.status == true
                            && YCTestApercentModel.testID == apercentCalc.testID)).ToList();
                            List<YCTestApercentModel> yctestApercentlist_N = conn.Table<YCTestApercentModel>().Where(
                                YCTestApercentModel =>
                                (YCTestApercentModel.testType == "N"
                                //&& YCTestApercentModel.status == true
                                && YCTestApercentModel.testID == apercentCalc.testID)).ToList();
                            List<YCTestApercentModel> yctestApercentlist_nPlus1 = conn.Table<YCTestApercentModel>().Where(
                                YCTestApercentModel =>
                                (YCTestApercentModel.testType == "nPlus1"
                                //&& YCTestApercentModel.status == true
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

                                if (apercentCalc.machineCategory == "Spinning")
                                {
                                    apercentReportModelView = new ApercentReportModelView()
                                    {
                                        testID = apercentCalc.testID,
                                        description = "Count",
                                        nMinus1 = formatDecimal(apercentCalc.testaverage_nMinus1),
                                        N = formatDecimal(apercentCalc.testaverage_N),
                                        nPlus1 = formatDecimal(apercentCalc.testaverage_nPlus1),
                                    };
                                    report.Add(apercentReportModelView);
                                }
                                else
                                {
                                    apercentReportModelView = new ApercentReportModelView()
                                    {
                                        testID = apercentCalc.testID,
                                        description = "Hank",
                                        nMinus1 = formatDecimal(apercentCalc.testaverage_nMinus1),
                                        N = formatDecimal(apercentCalc.testaverage_N),
                                        nPlus1 = formatDecimal(apercentCalc.testaverage_nPlus1),
                                    };
                                    report.Add(apercentReportModelView);
                                }

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
                                report.standardApercent = apercentCalc.standardApercent;


                                decimal actual_Nminus1 = apercentCalc.apercent_nMinus1;
                                decimal actual_nPlus1 = apercentCalc.apercent_nPlus1;

                                decimal expMin = decimal.Parse("-" + apercentCalc.standardApercent.ToString());
                                decimal expMax = apercentCalc.standardApercent;

                                if (actual_Nminus1 < expMin || actual_Nminus1 > expMax)
                                {
                                    report.isGREEN_NM1 = false;
                                    report.isRED_NM1 = true;
                                    if (actual_Nminus1 < expMin)
                                    {
                                        report.correctionRemark_NM1 = "Under Correction";
                                    }
                                    else if (actual_Nminus1 > expMax)
                                    {
                                        report.correctionRemark_NM1 = "Over Correction";
                                    }
                                    else
                                    {
                                        report.correctionRemark_NM1 = "";
                                    }
                                }
                                else
                                {
                                    report.isGREEN_NM1 = true;
                                    report.isRED_NM1 = false;
                                }

                                if (actual_nPlus1 < expMin || actual_nPlus1 > expMax)
                                {
                                    report.isGREEN_NP1 = false;
                                    report.isRED_NP1 = true;

                                    if (actual_nPlus1 < expMin)
                                    {
                                        report.correctionRemark_NP1 = "Under Correction";
                                    }
                                    else if (actual_nPlus1 > expMax)
                                    {
                                        report.correctionRemark_NP1 = "Over Correction";
                                    }
                                    else
                                    {
                                        report.correctionRemark_NP1 = "";
                                    }
                                }
                                else
                                {
                                    report.isGREEN_NP1 = true;
                                    report.isRED_NP1 = false;
                                }

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
                                report.testRemark = apercentCalc.testRemark;
                                report.createdate = apercentCalc.createdate;
                            }
                            //OVS.Add(report);
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
                        YCTestConsolidatedApercentReportMV consolItems = new YCTestConsolidatedApercentReportMV()
                        {
                            serialNo = "Average",
                            testID = "",
                            machineName = "",
                            testDate = "",
                            shift = "",
                            standardValue = "",
                            Nminus1 = "",
                            Nplus1 = "",
                            testDuration = "",
                            remarks = "",
                            isWhite_Nminus1 = true,
                            isRed_Nminus1 = false,
                            isWhite_Nplus1 = true,
                            isRed_Nplus1 = false,
                        };

                        //if (selectedMachineCategory == "Spinning" || selectedMachineCategory == "Winding")
                        //{
                        //    consolItems.isSpinning = true;
                        //    consolItems.otherThanSpinning = false;
                        //}
                        //else
                        //{
                        //    consolItems.isSpinning = false;
                        //    consolItems.otherThanSpinning = true;
                        //}

                        if (machineID != Guid.Empty)
                        {
                            isFinalAvgRowPresent = true;
                            OverallConsolidatedReports.Add(consolItems);
                        }

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
                    //lbl_totalTest.Text = TOT_TEST.ToString();
                    //lbl_AvgHank.Text = CON_HANK.ToString();
                    //lbl_AvgSD.Text = CON_STD_DEV.ToString();
                    //lbl_AvgCV.Text = CON_CV.ToString();

                    if (((CON_UF_NAME_1 != null && CON_UF_NAME_1 != "") && (CON_UF_VAL_1 != null && CON_UF_VAL_1 != "")) ||
                        ((CON_UF_NAME_2 != null && CON_UF_NAME_2 != "") && (CON_UF_VAL_2 != null && CON_UF_VAL_2 != "")) ||
                        ((CON_UF_NAME_3 != null && CON_UF_NAME_3 != "") && (CON_UF_VAL_3 != null && CON_UF_VAL_3 != "")) ||
                        ((CON_UF_NAME_4 != null && CON_UF_NAME_4 != "") && (CON_UF_VAL_4 != null && CON_UF_VAL_4 != "")))
                    {
                        grid_consolidated.IsVisible = true;
                    }

                    if (CON_UF_VAL_1 != null)
                    {
                        lbl_uf_name_1.IsVisible = true;
                        lbl_uf_value_1.IsVisible = true;
                        lbl_uf_name_1.Text = CON_UF_NAME_1;
                        lbl_uf_value_1.Text = CON_UF_VAL_1;
                    }
                    if (CON_UF_VAL_1 == null)
                    {
                        lbl_uf_name_1.IsVisible = false;
                        lbl_uf_value_1.IsVisible = false;
                        if (CON_UF_VAL_2 != null)
                        {
                            lbl_uf_name_2_col1.IsVisible = true;
                            lbl_uf_value_2_col1.IsVisible = true;
                            lbl_uf_name_2_col1.Text = CON_UF_NAME_2;
                            lbl_uf_value_2_col1.Text = CON_UF_VAL_2;
                            lbl_uf_name_2_col2.IsVisible = false;
                            lbl_uf_value_2_col2.IsVisible = false;
                        }
                    }
                    else
                    {
                        if (CON_UF_VAL_2 != null)
                        {
                            lbl_uf_name_2_col2.IsVisible = true;
                            lbl_uf_value_2_col2.IsVisible = true;
                            lbl_uf_name_2_col2.Text = CON_UF_NAME_2;
                            lbl_uf_value_2_col2.Text = CON_UF_VAL_2;
                            lbl_uf_name_2_col1.IsVisible = false;
                            lbl_uf_value_2_col1.IsVisible = false;
                        }
                        else
                        {
                            lbl_uf_name_2_col2.IsVisible = false;
                            lbl_uf_value_2_col2.IsVisible = false;
                            lbl_uf_name_2_col1.IsVisible = false;
                            lbl_uf_value_2_col1.IsVisible = false;
                        }
                    }

                    if (CON_UF_VAL_1 == null && CON_UF_VAL_2 == null)
                    {
                        if (CON_UF_VAL_3 != null)
                        {
                            lbl_uf_name_3_row1.IsVisible = true;
                            lbl_uf_value_3_row1.IsVisible = true;
                            lbl_uf_name_3_row1.Text = CON_UF_NAME_3;
                            lbl_uf_value_3_row1.Text = CON_UF_VAL_3;
                            lbl_uf_name_3_row2.IsVisible = false;
                            lbl_uf_value_3_row2.IsVisible = false;
                            if (CON_UF_VAL_4 != null)
                            {
                                lbl_uf_name_4_row1_col2.IsVisible = true;
                                lbl_uf_value_4_row1_col2.IsVisible = true;
                                lbl_uf_name_4_row1_col2.Text = CON_UF_NAME_3;
                                lbl_uf_value_4_row1_col2.Text = CON_UF_VAL_3;
                                lbl_uf_name_4_row2_col2.IsVisible = false;
                                lbl_uf_value_4_row2_col2.IsVisible = false;
                            }
                            else
                            {
                                lbl_uf_name_4_row1_col2.IsVisible = false;
                                lbl_uf_value_4_row1_col2.IsVisible = false;
                                lbl_uf_name_4_row2_col2.IsVisible = false;
                                lbl_uf_value_4_row2_col2.IsVisible = false;
                            }
                        }
                    }
                    else
                    {
                        if (CON_UF_VAL_3 != null)
                        {
                            lbl_uf_name_3_row2.IsVisible = true;
                            lbl_uf_value_3_row2.IsVisible = true;
                            lbl_uf_name_3_row2.Text = CON_UF_NAME_3;
                            lbl_uf_value_3_row2.Text = CON_UF_VAL_3;
                            lbl_uf_name_3_row1.IsVisible = false;
                            lbl_uf_value_3_row1.IsVisible = false;
                            if (CON_UF_VAL_4 != null)
                            {
                                lbl_uf_name_4_row2_col2.IsVisible = true;
                                lbl_uf_value_4_row2_col2.IsVisible = true;
                                lbl_uf_name_4_row2_col2.Text = CON_UF_NAME_3;
                                lbl_uf_value_4_row2_col2.Text = CON_UF_VAL_3;
                                lbl_uf_name_4_row1_col2.IsVisible = false;
                                lbl_uf_value_4_row1_col2.IsVisible = false;
                            }
                            else
                            {
                                lbl_uf_name_4_row2_col2.IsVisible = false;
                                lbl_uf_value_4_row2_col2.IsVisible = false;
                                lbl_uf_name_4_row1_col2.IsVisible = false;
                                lbl_uf_value_4_row1_col2.IsVisible = false;
                            }
                        }
                        else
                        {
                            lbl_uf_name_3_row2.IsVisible = false;
                            lbl_uf_value_3_row2.IsVisible = false;
                        }
                    }

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

        private void deleteRecords(List<YCTestApercentCalculatedModel> lstOfRecs)
        {
            if (lstOfRecs.Count == 0)
            {
                return;
            }
            foreach (YCTestApercentCalculatedModel rec in lstOfRecs)
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.Table<YCTestApercentCalculatedModel>().
                                           Where(YCTestApercentCalculatedModel =>
                                           YCTestApercentCalculatedModel.testID == rec.testID).Delete();
                    conn.Table<YCTestApercentSummaryModel>().
                                           Where(YCTestApercentSummaryModel =>
                                           YCTestApercentSummaryModel.testID == rec.testID).Delete();
                    conn.Table<YCTestApercentModel>().
                                        Where(YCTestApercentModel =>
                                        YCTestApercentModel.testID == rec.testID).Delete();
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
            if (consolidatedReport)
            {
                if (listview_tcConsolidatedReport.ItemsSource == null)
                {
                    await DisplayAlert("Notice", "No records to generate PDF!!!", "OK");
                    return;
                }
            }
            else
            {
                if (listview_tcreport.ItemsSource == null)
                {
                    await DisplayAlert("Notice", "No records to generate PDF!!!", "OK");
                    return;
                }
            }
            if (deleteAll)
            {
                bool answer = await DisplayAlert("Attention!!!", "Would you like to delete selected records?", "Yes", "No");
                if (answer == false) { return; }
            }
            btn_saveToPDF.IsEnabled = false;
            btn_saveToPDF.BackgroundColor = Color.Gray;
            btn_backToReport.IsEnabled = false;
            btn_backToReport.BackgroundColor = Color.Gray;
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
                    pdfGridInfo.Rows.Add();

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

                    pdfGridInfo.Rows[3].Cells[0].Value = "Standard A%: " + formatDecimal(orl.standardApercent).ToString();

                    if (orl.isGREEN_NM1)
                    {
                        pdfGridInfo.Rows[3].Cells[1].Value = "A% (N-1): " + formatDecimal(orl.apercent_nMinus1).ToString();
                    }
                    else
                    {
                        pdfGridInfo.Rows[3].Cells[1].Value = "A% (N-1): " + formatDecimal(orl.apercent_nMinus1).ToString()
                                                                + " " + orl.correctionRemark_NM1;
                        pdfGridInfo.Rows[3].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGridInfo.Rows[3].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGridInfo.Rows[3].Cells[1].Style.BackgroundBrush = PdfBrushes.Red;
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                        pdfGridInfo.Rows[3].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
                        pdfGridInfo.Rows[3].Cells[1].Style.TextBrush = brush_con;
                        pdfGridInfo.Rows[3].Cells[1].ColumnSpan = 2;
                    }

                    if (orl.isGREEN_NP1)
                    {
                        pdfGridInfo.Rows[3].Cells[2].Value = "A% (N+1): " + formatDecimal(orl.apercent_nPlus1).ToString();
                    }
                    else
                    {
                        pdfGridInfo.Rows[3].Cells[3].Value = "A% (N+1): " + formatDecimal(orl.apercent_nPlus1).ToString()
                                                                + " " + orl.correctionRemark_NP1;
                        pdfGridInfo.Rows[3].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGridInfo.Rows[3].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGridInfo.Rows[3].Cells[3].Style.BackgroundBrush = PdfBrushes.Red;
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                        pdfGridInfo.Rows[3].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
                        pdfGridInfo.Rows[3].Cells[3].Style.TextBrush = brush_con;
                        pdfGridInfo.Rows[3].Cells[3].ColumnSpan = 2;
                    }

                    //To bold A% Values
                    pdfGridInfo.Rows[3].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                    pdfGridInfo.Rows[3].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                    pdfGridInfo.Rows[3].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                    pdfGridInfo.Rows[3].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                    pdfGridInfo.Rows[3].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);



                    pdfGridInfo.Rows[4].Cells[0].Value = "Date: " + orl.createdate;
                    pdfGridInfo.Rows[4].Cells[1].Value = "Tester: " + orl.userName;
                    pdfGridInfo.Rows[4].Cells[1].ColumnSpan = 2;
                    pdfGridInfo.Rows[4].Cells[3].Value = "Shift: " + orl.shift;
                    pdfGridInfo.Rows[4].Cells[4].Value = "Process: " + orl.process;

                    pdfGridInfo.Rows[5].Cells[0].Value = "Remark: " + orl.testRemark;
                    pdfGridInfo.Rows[5].Cells[0].ColumnSpan = 4;
                    PdfBrush brush_red = new PdfSolidBrush(Syncfusion.Drawing.Color.Red);
                    pdfGridInfo.Rows[5].Cells[0].Style.TextBrush = brush_red;
                    pdfGridInfo.Rows[5].Cells[1].Style.TextBrush = brush_red;
                    pdfGridInfo.Rows[5].Cells[2].Style.TextBrush = brush_red;
                    pdfGridInfo.Rows[5].Cells[3].Style.TextBrush = brush_red;
                    pdfGridInfo.Rows[5].Cells[4].Style.TextBrush = brush_red;
                    pdfGridInfo.Rows[5].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Regular);
                    pdfGridInfo.Rows[5].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Regular);
                    pdfGridInfo.Rows[5].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Regular);
                    pdfGridInfo.Rows[5].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Regular);
                    pdfGridInfo.Rows[5].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Regular);

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
                    pdfGridInfo.Rows[5].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[5].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[5].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[5].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[5].Cells[4].Style.Borders.All = PdfPens.Transparent;

                    int totalRow_header = 5;
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
                        

                        if (test.description.ToString().Equals("Average Weight") ||
                            test.description.ToString().Equals("Hank") ||
                            test.description.ToString().Equals("SD") ||
                            test.description.ToString().Equals("CV"))
                        {
                            pdfGrid.Rows[rowCount].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                            pdfGrid.Rows[rowCount].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                            pdfGrid.Rows[rowCount].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                            pdfGrid.Rows[rowCount].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        }

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
                List<YCTestConsolidatedApercentReportMV> overallReportList = (List<YCTestConsolidatedApercentReportMV>)listview_tcConsolidatedReport.ItemsSource;
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
                foreach (YCTestConsolidatedApercentReportMV orl in overallReportList)
                {
                    if (includeHeader)
                    {
                        includeHeader = false;
                        pdfGrid = new PdfGrid();

                        pdfGrid.Columns.Add(10);
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

                       
                        pdfGrid.Rows[0].Cells[5].Value = "Std. A%";
                        pdfGrid.Rows[0].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[5].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[6].Value = "N-1";
                        pdfGrid.Rows[0].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[6].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                       

                        pdfGrid.Rows[0].Cells[7].Value = "N+1";
                        pdfGrid.Rows[0].Cells[7].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[7].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[7].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[7].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        
                        pdfGrid.Rows[0].Cells[8].Value = "User Params";
                        pdfGrid.Rows[0].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[8].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[9].Value = "Remark";
                        pdfGrid.Rows[0].Cells[9].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[9].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[9].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[9].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
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



                    if (orl.Nminus1 != null && orl.Nminus1 != "")
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[6].Value = formatDecimal(Decimal.Parse(orl.Nminus1)).ToString();
                    }
                    else
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[6].Value = orl.Nminus1;
                    }
                    if (orl.Nplus1 != null && orl.Nplus1 != "")
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[7].Value = formatDecimal(Decimal.Parse(orl.Nplus1)).ToString();
                    }
                    else
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[7].Value = orl.Nplus1;
                    }
                   
                    pdfGrid.Rows[pageRecordCount].Cells[8].Value = orl.testDuration;
                    pdfGrid.Rows[pageRecordCount].Cells[9].Value = orl.remarks;

                    if (orl.remarks != null || orl.standardValue != null)
                    {
                        //if (orl.testID == "82")
                        //{
                        //    decimal a = 1 / 9;
                        //}
                        PdfStringFormat format = new PdfStringFormat();
                        format.WordWrap = PdfWordWrapType.Word;
                        pdfGrid.Rows[pageRecordCount].Cells[9].Style.StringFormat = format;
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

                        if (orl.testDuration != null)
                        {
                            if (orl.testDuration.Length > contentLength)
                            {
                                contentLength = orl.testDuration.Length;
                            }
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

                    if (orl.isWhite_Nminus1)
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

                    if (orl.isWhite_Nplus1)
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[7].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[7].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[7].Style.BackgroundBrush = PdfBrushes.White;
                        //pdfGrid.Rows[pageRecordCount].Cells[7].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.Black);
                        pdfGrid.Rows[pageRecordCount].Cells[7].Style.TextBrush = brush_con;
                    }
                    else
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[7].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[7].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[7].Style.BackgroundBrush = PdfBrushes.Red;
                        //pdfGrid.Rows[pageRecordCount].Cells[7].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                        pdfGrid.Rows[pageRecordCount].Cells[7].Style.TextBrush = brush_con;
                    }

                    pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[9].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[9].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    

                    //**************************** Overall total ****************************

                    if (rowCount == overallReportList.Count && isFinalAvgRowPresent)
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
                    }

                    //**************************** End of Overall total *****************************

                    rowHeights = rowHeights + pdfGrid.Rows[pageRecordCount].Height;
                    if (rowHeights <= 700 && rowCount == overallReportList.Count)
                    {
                        result = pdfGrid.Draw(pdfPage, new PointF(10, 75), layoutFormat);
                    }
                    else if ((rowHeights >= 670 && rowHeights <= 700) && pageRecordCount != overallReportList.Count)
                    {
                        result = pdfGrid.Draw(pdfPage, new PointF(10, 75), layoutFormat);
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
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream, "TQM_Report_Consolidated(Apercent).pdf");
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

        [Obsolete]
        private bool generateCSVConsolidatedReport()
        {
            try
            {
                string downloadsFolder = Path.Combine(Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads), "TQMDownloads");
                using (var textWriter = new StreamWriter(Path.Combine(downloadsFolder, "TQM_Report_Consolidated(Apercent).csv")))
                {

                    var writer = new CsvWriter(textWriter, CultureInfo.InvariantCulture);
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ",",
                        HasHeaderRecord = false
                    };
                    //Header
                    writer.WriteField("S.No");
                    writer.WriteField("Date");
                    writer.WriteField("Test No");
                    writer.WriteField("Mac Name");
                    writer.WriteField("Shift");
                    writer.WriteField("Std. A%");
                    writer.WriteField("N-1");
                  
                    writer.WriteField("N+1");
                    writer.WriteField("User Params");
                    writer.WriteField("Remark");
                    //Actual Data
                    writer.NextRecord();
                    List<YCTestConsolidatedApercentReportMV> overallReportList = (List<YCTestConsolidatedApercentReportMV>)listview_tcConsolidatedReport.ItemsSource;
                    foreach (YCTestConsolidatedApercentReportMV orl in overallReportList)
                    {
                        writer.WriteField(orl.serialNo);
                        writer.WriteField(orl.testDate.Replace("\n"," "));
                        writer.WriteField(orl.testNo);
                        writer.WriteField(orl.machineName);
                       
                        writer.WriteField(orl.shift);
                        writer.WriteField(orl.standardValue);


                        if (orl.Nminus1 != null && orl.Nminus1 != "")
                        {
                            writer.WriteField(formatDecimal(Decimal.Parse(orl.Nminus1)).ToString());
                        }
                        else
                        {
                            writer.WriteField(orl.Nminus1);
                        }

                        if (orl.Nplus1 != null && orl.Nplus1 != "")
                        {
                            writer.WriteField(formatDecimal(Decimal.Parse(orl.Nplus1)).ToString());
                        }
                        else
                        {
                            writer.WriteField(orl.Nplus1);
                        }

                        

                        writer.WriteField(orl.testDuration);
                        writer.WriteField(orl.remarks);
                        writer.NextRecord();
                    }
                    writer.Flush();


                }
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
                    header.Graphics.DrawString("Con. A% Report - " + selectedMachineCategory + " (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(135, 20));
                    header.Graphics.DrawString("Date: " + DateTime.Now.ToString(), font_rn, brush_rn, new PointF(200, 36));
                    if (CON_UF_NAME_1 != null && CON_UF_NAME_1 != "")
                    {
                        //header.Graphics.DrawString("Lot Number" + ": " + "Ajksdjfk kdsfkjsdkfl ksjdfkjsdkf skfklsajkfj eeeeW", font_rn_uf, brush_rn, new PointF(10, 48));
                        //header.Graphics.DrawString("Material" + ": " + "Ajksdjfk kdsfkjsdkfl ksjdfkjsdkf skfklsajkfj eeeeW", font_rn_uf, brush_rn, new PointF(285, 48));
                        //header.Graphics.DrawString("Lot Number" + ": " + "Ajksdjfk kdsfkjsdkfl ksjdfkjsdkf skfklsajkfj eeeeW", font_rn_uf, brush_rn, new PointF(10, 58));
                        //header.Graphics.DrawString("Material" + ": " + "Ajksdjfk kdsfkjsdkfl ksjdfkjsdkf skfklsajkfj eeeeW", font_rn_uf, brush_rn, new PointF(285, 58));

                        header.Graphics.DrawString(CON_UF_NAME_1 + ": " + CON_UF_VAL_1, font_rn, brush_rn, new PointF(10, 48));
                        if (CON_UF_NAME_2 != null && CON_UF_NAME_2 != "")
                        {
                            header.Graphics.DrawString(CON_UF_NAME_2 + ": " + CON_UF_VAL_2, font_rn, brush_rn, new PointF(285, 48));
                        }
                    }
                    if ((CON_UF_NAME_1 == null || CON_UF_NAME_1 == "") && CON_UF_NAME_2 != null && CON_UF_NAME_2 != "")
                    {
                        header.Graphics.DrawString(CON_UF_NAME_2 + ": " + CON_UF_VAL_2, font_rn, brush_rn, new PointF(10, 48));
                    }
                    if ((CON_UF_NAME_1 != null && CON_UF_NAME_1 != "") || (CON_UF_NAME_2 != null && CON_UF_NAME_2 != ""))
                    {
                        if (CON_UF_NAME_3 != null && CON_UF_NAME_3 != "")
                        {
                            header.Graphics.DrawString(CON_UF_NAME_3 + ": " + CON_UF_VAL_3, font_rn, brush_rn, new PointF(10, 58));
                            if (CON_UF_NAME_4 != null && CON_UF_NAME_4 != "")
                            {
                                header.Graphics.DrawString(CON_UF_NAME_4 + ": " + CON_UF_VAL_4, font_rn, brush_rn, new PointF(285, 58));
                            }
                        }
                        if ((CON_UF_NAME_3 == null || CON_UF_NAME_3 == "") && CON_UF_NAME_4 != null && CON_UF_NAME_4 != "")
                        {
                            header.Graphics.DrawString(CON_UF_NAME_4 + ": " + CON_UF_VAL_4, font_rn, brush_rn, new PointF(10, 58));
                        }
                    }
                    else
                    {
                        if (CON_UF_NAME_3 != null && CON_UF_NAME_3 != "")
                        {
                            header.Graphics.DrawString(CON_UF_NAME_3 + ": " + CON_UF_VAL_3, font_rn, brush_rn, new PointF(10, 48));
                            if (CON_UF_NAME_4 != null && CON_UF_NAME_4 != "")
                            {
                                header.Graphics.DrawString(CON_UF_NAME_4 + ": " + CON_UF_VAL_4, font_rn, brush_rn, new PointF(285, 48));
                            }
                        }
                        if ((CON_UF_NAME_3 == null || CON_UF_NAME_3 == "") && CON_UF_NAME_4 != null && CON_UF_NAME_4 != "")
                        {
                            header.Graphics.DrawString(CON_UF_NAME_4 + ": " + CON_UF_VAL_4, font_rn, brush_rn, new PointF(10, 48));
                        }
                    }
                }
                else
                {

                    //header.Graphics.DrawString("A% Report - " + DateTime.Now.ToString(), font_rn, brush_rn, new PointF(165, 16));
                    header.Graphics.DrawString("A% Report - (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(165, 16));
                    
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
                        string fileName = "TQM_Report_Consolidated(Apercent).pdf";
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
                        if (selectedMachineCategory != null)
                        {
                            request.AddParameter("title", "TQMReportsConsolidated(A%-" + selectedMachineCategory + ")-" + DateTime.Now.ToString());
                        }
                        else
                        {
                            request.AddParameter("title", "TQMReportsConsolidated(A%-All)-" + DateTime.Now.ToString());
                        }
                        request.AddFile("reportpath", filePath);
                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (runConfiguration.getCSVReportStatus())
                            {
                                if (!generateCSVConsolidatedReport()) { showAlert("Error occurred in CSV report generation, hence upload is unsucessful!!!"); await resetBtn(); return; }
                                fileName = "TQM_Report_Consolidated(Apercent).csv";
                                root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                                myDir = new Java.IO.File(root + "/TQMDownloads");
                                file = new Java.IO.File(myDir, fileName);
                                filePath = file.Path;
                                client = new RestClient("https://myconsoleerp.herokuapp.com/tqmreport/upload");
                                request = new RestRequest();
                                request.Method = Method.Post;
                                //request.Timeout = Timeout.Infinite;
                                request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                                request.AddParameter("uploadedby", companyName);
                                if (selectedMachineCategory != null)
                                {
                                    request.AddParameter("title", "TQMReportsConsolidated-CSV-(A%-" + selectedMachineCategory + ")-" + DateTime.Now.ToString());
                                }
                                else
                                {
                                    request.AddParameter("title", "TQMReportsConsolidated-CSV-(A%-All)-" + DateTime.Now.ToString());
                                }
                                request.AddFile("reportpath", filePath);
                                response = client.Execute(request);
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
                            }
                            else
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
                        string fileName = "TQM_Report(A Percent).pdf";
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
                        request.AddParameter("title", "TQMReports(A Percent)-" + DateTime.Now.ToString());
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

        private string formatTime(DateTime startDateTime)
        {
            string hrs = startDateTime.Hour.ToString();
            if (hrs.Length < 2)
            {
                hrs = "0" + hrs;
            }
            string mins = startDateTime.Minute.ToString();
            if (mins.Length < 2)
            {
                mins = "0" + mins;
            }
            string sec = startDateTime.Second.ToString();
            if (sec.Length < 2)
            {
                sec = "0" + sec;
            }
            //return hrs + "h:" + mins + "m:" + sec + "s";
            return hrs + ":" + mins + ":" + sec;
        }

        private void btn_backToReport_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Report());
        }
    }
}