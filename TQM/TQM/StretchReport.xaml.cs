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
using System.Net;
using System.Net.Sockets;
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
    public partial class StretchReport : ContentPage
    {

        private List<OverallStretchReportModelView> _listOfReports;
        public List<OverallStretchReportModelView> ListOfReport { get { return _listOfReports; } set { _listOfReports = value; base.OnPropertyChanged(); } }


        private List<YCTestConsolidatedStretchReportMV> _listOfConsolidatedReports;
        public List<YCTestConsolidatedStretchReportMV> ListOfConsolidatedReports { get { return _listOfConsolidatedReports; } set { _listOfConsolidatedReports = value; base.OnPropertyChanged(); } }


        private string selectedCompanyName = null;
        private const string BLUE = "#0e0273";
        private RunConfiguration runConfiguration = new RunConfiguration();
        private List<StretchTestCalculatedModel> deleteList = null;
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

        public StretchReport()
        {
            InitializeComponent();
        }

        public StretchReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string process, string testID, string matType, string materialLength, bool deleteRequest, bool isConsolidated, string UFVAL1, string UFVAL2, string UFVAL3, string UFVAL4)
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

        private void getReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string process, string testID, string matType, string materialLength, bool deleteRequest, string UFVAL1, string UFVAL2, string UFVAL3, string UFVAL4)
        {
            try
            {
                List<OverallStretchReportModelView> OVS = new List<OverallStretchReportModelView>();
                List<YCTestConsolidatedStretchReportMV> OverallConsolidatedReports = new List<YCTestConsolidatedStretchReportMV>();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {

                    conn.CreateTable<StretchTestModel>();
                    conn.CreateTable<StretchTestCalculatedModel>();

                    List<StretchTestCalculatedModel> stretchCalcList = null;

                    if (testID != "")
                    {
                        if (!testID.Contains("."))
                        {
                            long givenTestId = long.Parse(testID);
                            stretchCalcList = conn.Table<StretchTestCalculatedModel>().Where(StretchTestCalculatedModel =>
                                                 StretchTestCalculatedModel.testID == givenTestId).ToList();
                        }
                        else
                        {
                            long startTestID = long.Parse(testID.Split('.')[0]);
                            long endTestID = long.Parse(testID.Split('.')[1]);

                            if (startTestID == endTestID)
                            {
                                stretchCalcList = conn.Table<StretchTestCalculatedModel>().Where(StretchTestCalculatedModel =>
                                                 StretchTestCalculatedModel.testID == startTestID).ToList();
                            }
                            else
                            {
                                for (long i = startTestID; i <= endTestID; i++)
                                {
                                    if (stretchCalcList == null)
                                    {
                                        stretchCalcList = conn.Table<StretchTestCalculatedModel>().Where(StretchTestCalculatedModel =>
                                                 StretchTestCalculatedModel.testID == i).ToList();
                                    }
                                    else
                                    {
                                        List<StretchTestCalculatedModel> tempList =  conn.Table<StretchTestCalculatedModel>().Where(StretchTestCalculatedModel =>
                                                 StretchTestCalculatedModel.testID == i).ToList();
                                        //stretchCalcList.Concat(tempList).ToList();
                                        stretchCalcList.AddRange(tempList);
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
                        List<StretchTestCalculatedModel> parentList = conn.Table<StretchTestCalculatedModel>().Where(StretchTestCalculatedModel =>
                                                (StretchTestCalculatedModel.createdate >= startDate
                                                && StretchTestCalculatedModel.createdate < endDate))
                                                .OrderBy(StretchTestCalculatedModel => StretchTestCalculatedModel.createdate).ToList();
                        if (parentList.Count == 0)
                        {
                            DisplayAlert("Notice", "No records to display!!!", "OK");
                            return;
                        }

                        //Start of Logic to check last shift for the given end date is logged in end date + 1 day date
                        //Get first record of actual end date + 1 day
                        List<StretchTestCalculatedModel> recs_actualEndDatePlusOne = conn.Table<StretchTestCalculatedModel>()
                                                                        .Where(StretchTestCalculatedModel =>
                                                                        (StretchTestCalculatedModel.createdate >= endDate
                                                                        && StretchTestCalculatedModel.createdate < endDatePlusOne))
                                                                        .OrderBy(StretchTestCalculatedModel => StretchTestCalculatedModel.createdate).ToList();
                        if (recs_actualEndDatePlusOne.Count > 0)
                        {
                            //Check if the 1st record of actual end date + 1 day is not Shift-1
                            if (recs_actualEndDatePlusOne[0].shift != "Shift-1")
                            {
                                StretchTestCalculatedModel actualEndDatePlusOne_Shift1_Recs = recs_actualEndDatePlusOne.Where(StretchTestCalculatedModel =>
                                                                                             (StretchTestCalculatedModel.shift == "Shift-1"))
                                                                                            .OrderBy(StretchTestCalculatedModel => StretchTestCalculatedModel.createdate)
                                                                                            .FirstOrDefault();
                                //Merge last shift record of actual end date from (actual end date + 1day) with parent list
                                parentList.Concat(recs_actualEndDatePlusOne.Where(StretchTestCalculatedModel =>
                                                                            (StretchTestCalculatedModel.createdate >= endDate
                                                                            && StretchTestCalculatedModel.createdate < actualEndDatePlusOne_Shift1_Recs.createdate
                                                                            && StretchTestCalculatedModel.shift == recs_actualEndDatePlusOne[0].shift))
                                                                            .OrderBy(StretchTestCalculatedModel => StretchTestCalculatedModel.createdate).ToList());
                            }
                        }
                        //End of Logic to check last shift for the given end date is logged in end date + 1 day date

                        //Start of logic to ignore the previous date last shift record from the given actual start date
                        if (parentList[0].shift != "Shift-1")
                        {
                            StretchTestCalculatedModel actualStartDate_Shift1_Recs = parentList.Where(StretchTestCalculatedModel =>
                                                                                         (StretchTestCalculatedModel.shift == "Shift-1"))
                                                                                        .OrderBy(StretchTestCalculatedModel => StretchTestCalculatedModel.createdate)
                                                                                        .FirstOrDefault();
                            if (actualStartDate_Shift1_Recs != null)
                            {
                                List<StretchTestCalculatedModel> lastShiftOfPreviousDay_in_ActualStartDateRecs =
                                                                        parentList.Where(StretchTestCalculatedModel =>
                                                                        (StretchTestCalculatedModel.shift == parentList[0].shift
                                                                        && StretchTestCalculatedModel.createdate < actualStartDate_Shift1_Recs.createdate))
                                                                        .OrderBy(StretchTestCalculatedModel => StretchTestCalculatedModel.createdate)
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
                                stretchCalcList = parentList.Where(StretchTestCalculatedModel =>
                                                (StretchTestCalculatedModel.shift == shift
                                                && StretchTestCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift == "" && process != null)
                            {
                                stretchCalcList = parentList.Where(StretchTestCalculatedModel =>
                                                (StretchTestCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift != "" && process == null)
                            {
                                stretchCalcList = parentList.Where(StretchTestCalculatedModel =>
                                                (StretchTestCalculatedModel.shift == shift)).ToList();
                            }
                            else if (shift == "" && process == null)
                            {
                                stretchCalcList = parentList;
                            }


                        }
                        else if (categoryName != null && machineID == Guid.Empty)
                        {
                            //endDate = endDate.AddDays(1);

                            if (shift != "" && process != null)
                            {
                                stretchCalcList = parentList.Where(StretchTestCalculatedModel =>
                                              (StretchTestCalculatedModel.machineCategory == categoryName
                                              && StretchTestCalculatedModel.shift == shift
                                              && StretchTestCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift == "" && process != null)
                            {
                                stretchCalcList = parentList.Where(StretchTestCalculatedModel =>
                                              (StretchTestCalculatedModel.machineCategory == categoryName
                                              && StretchTestCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift != "" && process == null)
                            {
                                stretchCalcList = parentList.Where(StretchTestCalculatedModel =>
                                              (StretchTestCalculatedModel.machineCategory == categoryName
                                              && StretchTestCalculatedModel.shift == shift)).ToList();
                            }
                            else if (shift == "" && process == null)
                            {
                                stretchCalcList = parentList.Where(StretchTestCalculatedModel =>
                                              (StretchTestCalculatedModel.machineCategory == categoryName)).ToList();
                            }


                        }
                        else if (categoryName != null && machineID != Guid.Empty)
                        {
                            //endDate = endDate.AddDays(1);

                            if (shift != "" && process != null)
                            {
                                stretchCalcList = parentList.Where(StretchTestCalculatedModel =>
                                               (StretchTestCalculatedModel.machineCategory == categoryName
                                               && StretchTestCalculatedModel.machineID == machineID
                                               && StretchTestCalculatedModel.shift == shift
                                               && StretchTestCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift == "" && process != null)
                            {
                                stretchCalcList = parentList.Where(StretchTestCalculatedModel =>
                                               (StretchTestCalculatedModel.machineCategory == categoryName
                                               && StretchTestCalculatedModel.machineID == machineID
                                               && StretchTestCalculatedModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift != "" && process == null)
                            {
                                stretchCalcList = parentList.Where(StretchTestCalculatedModel =>
                                               (StretchTestCalculatedModel.machineCategory == categoryName
                                               && StretchTestCalculatedModel.machineID == machineID
                                               && StretchTestCalculatedModel.shift == shift)).ToList();
                            }
                            else if (shift == "" && process == null)
                            {
                                stretchCalcList = parentList.Where(StretchTestCalculatedModel =>
                                               (StretchTestCalculatedModel.machineCategory == categoryName
                                               && StretchTestCalculatedModel.machineID == machineID)).ToList();
                            }


                        }
                    }

                    //List<StretchTestCalculatedModel> stretchCalcList = null;

                    //stretchCalcList = conn.Table<StretchTestCalculatedModel>().Where(
                    //      StretchTestCalculatedModel =>
                    //      (StretchTestCalculatedModel.status == true)).ToList();


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
                        stretchCalcList = stretchCalcList.Where(StretchTestCalculatedModel => (StretchTestCalculatedModel.yarnlength == yarnLength
                                                && StretchTestCalculatedModel.yarnlenunit == matType)).ToList();
                    }

                    if (stretchCalcList.Count == 0)
                    {
                        DisplayAlert("Notice", "No records to display!!!", "OK");
                        return;
                    }
                    else
                    {
                       
                        if (deleteRequest)
                        {
                            deleteAll = true;
                            deleteList = stretchCalcList;
                        }
                    }

                    if (consolidatedReport)
                    {
                        stretchCalcList = stretchCalcList.OrderBy(StretchTestCalculatedModel => StretchTestCalculatedModel.machineName).ToList();
                    }

                    int counter = 0;
                    string prev_macID = null;
                    int macTestNo = 0;

                    foreach (StretchTestCalculatedModel stretchCalc in stretchCalcList)
                    {
                        OverallStretchReportModelView report = new OverallStretchReportModelView();
                        YCTestConsolidatedStretchReportMV consolItems = new YCTestConsolidatedStretchReportMV();


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
                                                                                    && YarnCountConfigModel.machineCategory == stretchCalc.machineCategory
                                                                                    && YarnCountConfigModel.machineID == stretchCalc.machineID
                                                                                    && YarnCountConfigModel.machineName == stretchCalc.machineName).FirstOrDefault();
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
                                                                                    && YarnCountConfigModel.machineCategory == stretchCalc.machineCategory
                                                                                    && YarnCountConfigModel.machineID == stretchCalc.machineID
                                                                                    && YarnCountConfigModel.machineName == stretchCalc.machineName).FirstOrDefault();
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
                                                                                    && YarnCountConfigModel.machineCategory == stretchCalc.machineCategory
                                                                                    && YarnCountConfigModel.machineID == stretchCalc.machineID
                                                                                    && YarnCountConfigModel.machineName == stretchCalc.machineName).FirstOrDefault();
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
                                                                                    && YarnCountConfigModel.machineCategory == stretchCalc.machineCategory
                                                                                    && YarnCountConfigModel.machineID == stretchCalc.machineID
                                                                                    && YarnCountConfigModel.machineName == stretchCalc.machineName).FirstOrDefault();
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
                                                                                    YarnCountConfigModel.machineCategory == stretchCalc.machineCategory
                                                                                    && YarnCountConfigModel.machineID == stretchCalc.machineID
                                                                                    && YarnCountConfigModel.machineName == stretchCalc.machineName)).FirstOrDefault();
                            if (ycConfig_uf_1 == null)
                            {
                                DisplayAlert("Notice", "Unable to reterive user fields from settings!!!", "OK");
                                return;
                            }

                            consolItems.uf_name_1 = ycConfig_uf_1.uf_name_1;
                            consolItems.uf_name_2 = ycConfig_uf_1.uf_name_2;
                            consolItems.uf_name_3 = ycConfig_uf_1.uf_name_3;
                            consolItems.uf_name_4 = ycConfig_uf_1.uf_name_4;

                            consolItems.uf_value_1 = stretchCalc.uf_value_1;
                            consolItems.uf_value_2 = stretchCalc.uf_value_2;
                            consolItems.uf_value_3 = stretchCalc.uf_value_3;
                            consolItems.uf_value_4 = stretchCalc.uf_value_4;

                            //decimal maxRangeVal = testsummary.standardHank + (testsummary.standardHank * (Convert.ToDecimal(testsummary.deviationPercent) / 100));
                            //decimal minRangeVal = testsummary.standardHank - (testsummary.standardHank * (Convert.ToDecimal(testsummary.deviationPercent) / 100));

                            
                            //decimal expMin = decimal.Parse("-" + (stretchCalc.standardStretch-stretchCalc.stretchDeviation).ToString());
                            decimal minRangeVal = decimal.Parse("-" + (stretchCalc.standardStretch + stretchCalc.stretchDeviation).ToString());
                            decimal maxRangeVal = stretchCalc.standardStretch + stretchCalc.stretchDeviation;

                            if (stretchCalc.stretch < minRangeVal || stretchCalc.stretch > maxRangeVal)
                            {
                                consolItems.isRed = true;
                                consolItems.isWhite = false;
                            }
                            else
                            {
                                consolItems.isRed = false;
                                consolItems.isWhite = true;
                            }

                            if (prev_macID == null)
                            {
                                prev_macID = stretchCalc.machineID.ToString();
                                macTestNo = 1;
                            }
                            else
                            {
                                if (prev_macID != stretchCalc.machineID.ToString())
                                {
                                    prev_macID = stretchCalc.machineID.ToString();
                                    macTestNo = 1;
                                }
                                else
                                {
                                    macTestNo = macTestNo + 1;
                                }
                            }

                            consolItems.serialNo = (counter + 1).ToString();
                            consolItems.testNo = macTestNo.ToString();
                            consolItems.testID = stretchCalc.testID.ToString();
                            consolItems.machineName = stretchCalc.machineName;
                            consolItems.testDate = stretchCalc.createdate.Day.ToString() + "-" +
                                                    stretchCalc.createdate.Month.ToString() + "-" +
                                                    stretchCalc.createdate.Year.ToString() + "\n" +
                                                    formatTime(stretchCalc.createdate);
                            consolItems.shift = stretchCalc.shift;
                            consolItems.standardValue = "\u00B1" + formatDecimal(stretchCalc.standardStretch, 2).ToString();

                            consolItems.stretch = formatDecimal(stretchCalc.stretch).ToString();
                            //consolItems.testDuration = stretchCalc.testDuration;
                            //consolItems.testDuration = formatTime(stretchCalc.createdate);

                            string userParams = "";

                            if (stretchCalc.uf_value_1 != null && stretchCalc.uf_value_1 != "")
                            {
                                userParams = stretchCalc.uf_value_1;
                            }

                            if (stretchCalc.uf_value_2 != null && stretchCalc.uf_value_2 != "")
                            {
                                if (userParams == "")
                                {
                                    userParams = stretchCalc.uf_value_2;
                                }
                                else
                                {
                                    userParams = userParams + "\n" + stretchCalc.uf_value_2;
                                }
                            }

                            if (stretchCalc.uf_value_3 != null && stretchCalc.uf_value_3 != "")
                            {
                                if (userParams == "")
                                {
                                    userParams = stretchCalc.uf_value_3;
                                }
                                else
                                {
                                    userParams = userParams + "\n" + stretchCalc.uf_value_3;
                                }
                            }

                            if (stretchCalc.uf_value_4 != null && stretchCalc.uf_value_4 != "")
                            {
                                if (userParams == "")
                                {
                                    userParams = stretchCalc.uf_value_4;
                                }
                                else
                                {
                                    userParams = userParams + "\n" + stretchCalc.uf_value_4;
                                }
                            }

                            consolItems.testDuration = userParams;

                            //consolItems.testDuration = stretchCalc.userName;


                            consolItems.remarks = stretchCalc.testRemark;

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

                            List<StretchTestModel> yctestStretchlist_IB = conn.Table<StretchTestModel>().Where(
                                StretchTestModel =>
                                (StretchTestModel.testType == "IB"
                                //&& StretchTestModel.status == true
                                && StretchTestModel.testID == stretchCalc.testID)).ToList();

                            List<StretchTestModel> yctestStretchlist_FB = conn.Table<StretchTestModel>().Where(
                                StretchTestModel =>
                                (StretchTestModel.testType == "FB"
                                //&& StretchTestModel.status == true
                                && StretchTestModel.testID == stretchCalc.testID)).ToList();
                            if (yctestStretchlist_IB != null && yctestStretchlist_FB != null)
                            {

                                int loopCount = 0;
                                foreach (StretchTestModel test in yctestStretchlist_IB)
                                {
                                    StretchReportModelView stretchReportMV = new StretchReportModelView()
                                    {
                                        testID = test.testID,
                                        description = test.testcount.ToString(),
                                        //IB = formatDecimal(yctestStretchlist_IB[loopCount].yarnweight),
                                        //FB = formatDecimal(yctestStretchlist_FB[loopCount].yarnweight),
                                        IB = formatDecimal(yctestStretchlist_IB[loopCount].yccalcval),
                                        FB = formatDecimal(yctestStretchlist_FB[loopCount].yccalcval),
                                    };
                                    report.Add(stretchReportMV);
                                    loopCount += 1;
                                }

                                //StretchReportModelView StretchReportModelView = new StretchReportModelView()
                                //{
                                //    testID = stretchCalc.testID,
                                //    description = "Average Weight",
                                //    IB = formatDecimal(stretchCalc.avg_weight_IB),
                                //    FB = formatDecimal(stretchCalc.avg_weight_FB),
                                //};
                                //report.Add(StretchReportModelView);

                                //StretchReportModelView = new StretchReportModelView()
                                //{
                                //    testID = stretchCalc.testID,
                                //    description = "Weight (Max)",
                                //    IB = formatDecimal(stretchCalc.max_IB),
                                //    FB = formatDecimal(stretchCalc.max_FB),
                                //};
                                //report.Add(StretchReportModelView);

                                //StretchReportModelView = new StretchReportModelView()
                                //{
                                //    testID = stretchCalc.testID,
                                //    description = "Weight (Min)",
                                //    IB = formatDecimal(stretchCalc.min_IB),
                                //    FB = formatDecimal(stretchCalc.min_FB),
                                //};
                                //report.Add(StretchReportModelView);

                                //StretchReportModelView = new StretchReportModelView()
                                //{
                                //    testID = stretchCalc.testID,
                                //    description = "Range",
                                //    IB = formatDecimal(stretchCalc.range_IB),
                                //    FB = formatDecimal(stretchCalc.range_FB),
                                //};
                                //report.Add(StretchReportModelView);

                                StretchReportModelView StretchReportModelView;

                                if (stretchCalc.machineCategory == "Spinning")
                                {
                                    StretchReportModelView = new StretchReportModelView()
                                    {
                                        testID = stretchCalc.testID,
                                        description = "Count",
                                        IB = formatDecimal(stretchCalc.testaverage_IB),
                                        FB = formatDecimal(stretchCalc.testaverage_FB),
                                    };
                                    report.Add(StretchReportModelView);
                                }
                                else
                                {
                                    StretchReportModelView = new StretchReportModelView()
                                    {
                                        testID = stretchCalc.testID,
                                        description = "Hank",
                                        IB = formatDecimal(stretchCalc.testaverage_IB),
                                        FB = formatDecimal(stretchCalc.testaverage_FB),
                                    };
                                    report.Add(StretchReportModelView);
                                }

                                StretchReportModelView = new StretchReportModelView()
                                {
                                    testID = stretchCalc.testID,
                                    description = "SD",
                                    IB = formatDecimal(stretchCalc.testsd_IB),
                                    FB = formatDecimal(stretchCalc.testsd_FB),
                                };
                                report.Add(StretchReportModelView);

                                StretchReportModelView = new StretchReportModelView()
                                {
                                    testID = stretchCalc.testID,
                                    description = "CV",
                                    IB = formatDecimal(stretchCalc.testcv_IB),
                                    FB = formatDecimal(stretchCalc.testcv_FB),
                                };
                                report.Add(StretchReportModelView);


                                report.testID = stretchCalc.testID;
                                report.userName = stretchCalc.userName;
                                report.machineCategory = stretchCalc.machineCategory;
                                report.machineName = stretchCalc.machineName;
                                report.shift = stretchCalc.shift;
                                report.process = stretchCalc.process;
                                report.countsysname = stretchCalc.countsysname;
                                report.yarnlenunit = stretchCalc.yarnlenunit;
                                report.yarnlength = stretchCalc.yarnlength;
                                report.totaltestcount = stretchCalc.totaltestcount;

                                report.standardStretch = formatDecimal(stretchCalc.standardStretch);
                                report.stretchDeviation = stretchCalc.stretchDeviation;

                                decimal actual = stretchCalc.stretch;
                                //decimal expMin = decimal.Parse("-" + (stretchCalc.standardStretch-stretchCalc.stretchDeviation).ToString());
                                decimal expMin = decimal.Parse("-" + (stretchCalc.standardStretch + stretchCalc.stretchDeviation).ToString());
                                decimal expMax = stretchCalc.standardStretch + stretchCalc.stretchDeviation;


                                if (actual < expMin || actual > expMax)
                                {
                                    report.isGREEN = false;
                                    report.isRED = true;
                                }
                                else
                                {
                                    report.isGREEN = true;
                                    report.isRED = false;
                                }
                                
                                report.standardCV = stretchCalc.standardCV;
                                report.CVDeviationPercent = stretchCalc.CVDeviationPercent;

                                decimal maxRangeVal_CV = stretchCalc.standardCV + stretchCalc.CVDeviationPercent;
                                decimal minRangeVal_CV = stretchCalc.standardCV - stretchCalc.CVDeviationPercent;


                                if (stretchCalc.testcv_IB < minRangeVal_CV || stretchCalc.testcv_IB > maxRangeVal_CV)
                                {
                                    if (stretchCalc.standardCV > 0.0m)
                                    {
                                        report.isRed_IB_CV = true;
                                        report.isWhite_IB_CV = false;
                                    }
                                    else
                                    {
                                        report.isRed_IB_CV = false;
                                        report.isWhite_IB_CV = true;
                                    }
                                }

                                if (stretchCalc.testcv_FB < minRangeVal_CV || stretchCalc.testcv_FB > maxRangeVal_CV)
                                {
                                    if (stretchCalc.standardCV > 0.0m)
                                    {
                                        report.isRed_FB_CV = true;
                                        report.isWhite_FB_CV = false;
                                    }
                                    else
                                    {
                                        report.isRed_FB_CV = false;
                                        report.isWhite_FB_CV = true;
                                    }
                                }

                                report.testaverage_IB = stretchCalc.testaverage_IB;
                                report.testsd_IB = stretchCalc.testsd_IB;
                                report.testcv_IB = stretchCalc.testcv_IB;
                                report.testaverage_FB = stretchCalc.testaverage_FB;
                                report.testsd_FB = stretchCalc.testsd_FB;
                                report.testcv_FB = stretchCalc.testcv_FB;
                                report.stretch = formatDecimal(stretchCalc.stretch);
                                report.testRemark = stretchCalc.testRemark;
                                report.createdate = stretchCalc.createdate;
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
                        YCTestConsolidatedStretchReportMV consolItems = new YCTestConsolidatedStretchReportMV()
                        {
                            serialNo = "Average",
                            testID = "",
                            machineName = "",
                            testDate = "",
                            shift = "",
                            standardValue = "",
                            stretch = "",
                            testDuration = "",
                            remarks = "",
                            isWhite = true,
                            isRed = false,
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

        private void deleteRecords(List<StretchTestCalculatedModel> lstOfRecs)
        {
            if (lstOfRecs.Count == 0)
            {
                return;
            }
            foreach (StretchTestCalculatedModel rec in lstOfRecs)
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.Table<StretchTestCalculatedModel>().
                                           Where(StretchTestCalculatedModel =>
                                           StretchTestCalculatedModel.testID == rec.testID).Delete();
                    conn.Table<StretchTestSummaryModel>().
                                           Where(StretchTestSummaryModel =>
                                           StretchTestSummaryModel.testID == rec.testID).Delete();
                    conn.Table<StretchTestModel>().
                                        Where(StretchTestModel =>
                                        StretchTestModel.testID == rec.testID).Delete();
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
                List<OverallStretchReportModelView> overallReportList = (List<OverallStretchReportModelView>)listview_tcreport.ItemsSource;
                PdfLayoutResult result = null;
                //PdfLayoutResult resultInfo = null;
                float overallHeight = 0;
                int tableNo = 1;
                bool newPageAdded_Header = false;
                bool newPageAdded_Body = false;
                foreach (OverallStretchReportModelView orl in overallReportList)
                {

                    List<StretchReportModelView> testList = orl.stretchReportMV;

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


                    pdfGridInfo.Rows[3].Cells[0].Value = "Std. Stretch %: " + formatDecimal(orl.standardStretch).ToString() + " " + "\u00B1"
                                                            +orl.stretchDeviation;
                    pdfGridInfo.Rows[3].Cells[0].ColumnSpan = 2;

                    pdfGridInfo.Rows[3].Cells[2].Value = "Stretch %: " + formatDecimal(orl.stretch).ToString();

                    if (orl.isRED)
                    {
                        pdfGridInfo.Rows[3].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGridInfo.Rows[3].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGridInfo.Rows[3].Cells[2].Style.BackgroundBrush = PdfBrushes.Red;
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                        pdfGridInfo.Rows[3].Cells[2].Style.TextBrush = brush_con;
                    }

                    pdfGridInfo.Rows[3].Cells[2].ColumnSpan = 2;

                    //To bold Stretch Values
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

                    pdfGrid.Columns.Add(3);
                    PdfGridRow row = new PdfGridRow(pdfGrid);
                    pdfGrid.Rows.Add(row);

                    pdfGrid.Rows[0].Cells[0].Value = "Sample No";
                    pdfGrid.Rows[0].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[0].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[0].Style.TextPen = PdfPens.Black;
                    pdfGrid.Rows[0].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);

                    pdfGrid.Rows[0].Cells[1].Value = "Initial Bobbin";
                    pdfGrid.Rows[0].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[1].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[1].Style.TextPen = PdfPens.Black;
                    pdfGrid.Rows[0].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);

                    //pdfGrid.Rows[0].Cells[2].Value = "N";
                    //pdfGrid.Rows[0].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[0].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    //pdfGrid.Rows[0].Cells[2].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[2].Style.TextPen = PdfPens.Black;
                    //pdfGrid.Rows[0].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);

                    //pdfGrid.Rows[0].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    //pdfGrid.Rows[0].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    //pdfGrid.Rows[0].Cells[2].Style.Borders.All = PdfPens.Transparent;

                    pdfGrid.Rows[0].Cells[2].Value = "Full Bobbin";
                    pdfGrid.Rows[0].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[2].Style.BackgroundBrush = PdfBrushes.LightGray;
                    pdfGrid.Rows[0].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);



                    int rowCount = 1;
                    foreach (StretchReportModelView test in testList)
                    {
                        row = new PdfGridRow(pdfGrid);
                        pdfGrid.Rows.Add(row);
                        pdfGrid.Rows[rowCount].Cells[0].Value = test.description.ToString();

                        if (test.description.ToString().Equals("CV"))
                        {
                            if (orl.standardCV == 0.0m)
                            {
                                pdfGrid.Rows[rowCount].Cells[1].Value = formatDecimal(test.IB).ToString();
                                pdfGrid.Rows[rowCount].Cells[2].Value = formatDecimal(test.FB).ToString();
                            }
                            else
                            {
                                pdfGrid.Rows[rowCount].Cells[1].Value = test.IB +
                                                                        "(" +
                                                                        formatDecimal(orl.standardCV, 4).ToString() +
                                                                        "\u00B1" +
                                                                        orl.CVDeviationPercent + ")";
                                pdfGrid.Rows[rowCount].Cells[2].Value = test.FB +
                                                                        "(" +
                                                                        formatDecimal(orl.standardCV, 4).ToString() +
                                                                        "\u00B1" +
                                                                        orl.CVDeviationPercent + ")";
                                
                                if (orl.isRed_IB_CV)
                                {
                                    pdfGrid.Rows[rowCount].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                                    pdfGrid.Rows[rowCount].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                                    pdfGrid.Rows[rowCount].Cells[1].Style.BackgroundBrush = PdfBrushes.Red;
                                    PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                                    pdfGrid.Rows[rowCount].Cells[1].Style.TextBrush = brush_con;
                                }
                                if (orl.isRed_FB_CV)
                                {
                                    pdfGrid.Rows[rowCount].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                                    pdfGrid.Rows[rowCount].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                                    pdfGrid.Rows[rowCount].Cells[2].Style.BackgroundBrush = PdfBrushes.Red;
                                    PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                                    pdfGrid.Rows[rowCount].Cells[2].Style.TextBrush = brush_con;
                                }
                            }
                        }
                        else
                        {
                            pdfGrid.Rows[rowCount].Cells[1].Value = formatDecimal(test.IB).ToString();
                            pdfGrid.Rows[rowCount].Cells[2].Value = formatDecimal(test.FB).ToString();
                        }

                        


                        pdfGrid.Rows[rowCount].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[rowCount].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[rowCount].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[rowCount].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        //pdfGrid.Rows[rowCount].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                        //pdfGrid.Rows[rowCount].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

                        if (test.description.ToString().Equals("Hank") ||
                            test.description.ToString().Equals("SD") ||
                            test.description.ToString().Equals("CV"))
                        {
                            pdfGrid.Rows[rowCount].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                            pdfGrid.Rows[rowCount].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                            pdfGrid.Rows[rowCount].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
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
                                result = pdfGrid.Draw(pdfPage, new PointF(10, overallHeight + 10), layoutFormat);
                                //changed from 10 to 25
                            }
                            else
                            {
                                //commented below code--->When test more than 30 samples, the overall table is moving to next page keeping 1st
                                //page as blank

                                //if ((overallHeight + totalRow_body_height) > 730)
                                //{
                                //    pdfPage = pdfDocument.Pages.Add();
                                //    result = pdfGrid.Draw(pdfPage, new PointF(10, 30), layoutFormat);
                                //}
                                //else
                                //{
                                    result = pdfGrid.Draw(result.Page, new PointF(10, (overallHeight + 10)));
                                    //changed from 10 to 25
                                //}
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
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream, "TQM_Report(Stretch).pdf");
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
                List<YCTestConsolidatedStretchReportMV> overallReportList = (List<YCTestConsolidatedStretchReportMV>)listview_tcConsolidatedReport.ItemsSource;
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
                foreach (YCTestConsolidatedStretchReportMV orl in overallReportList)
                {
                    if (includeHeader)
                    {
                        includeHeader = false;
                        pdfGrid = new PdfGrid();

                        pdfGrid.Columns.Add(9);
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


                        pdfGrid.Rows[0].Cells[5].Value = "Std. Stretch";
                        pdfGrid.Rows[0].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[5].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[6].Value = "Stretch";
                        pdfGrid.Rows[0].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[6].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);


                       

                        pdfGrid.Rows[0].Cells[7].Value = "User Params";
                        //pdfGrid.Rows[0].Cells[7].Value = "User Name";
                        pdfGrid.Rows[0].Cells[7].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[7].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[7].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[7].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[8].Value = "Remark";
                        pdfGrid.Rows[0].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[8].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
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



                    if (orl.stretch != null && orl.stretch != "")
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[6].Value = formatDecimal(Decimal.Parse(orl.stretch)).ToString();
                    }
                    else
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[6].Value = orl.stretch;
                    }
                   

                    pdfGrid.Rows[pageRecordCount].Cells[7].Value = orl.testDuration;
                    pdfGrid.Rows[pageRecordCount].Cells[8].Value = orl.remarks;

                    if (orl.remarks != null || orl.standardValue != null)
                    {
                        //if (orl.testID == "82")
                        //{
                        //    decimal a = 1 / 9;
                        //}
                        PdfStringFormat format = new PdfStringFormat();
                        format.WordWrap = PdfWordWrapType.Word;
                        pdfGrid.Rows[pageRecordCount].Cells[8].Style.StringFormat = format;
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
                        //pdfGrid.Rows[pageRecordCount].Cells[9].Style.Borders.All = PdfPens.Transparent;

                        pdfGrid.Rows[pageRecordCount].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[7].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        //pdfGrid.Rows[pageRecordCount].Cells[9].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
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
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream, "TQM_Report_Consolidated(Stretch).pdf");
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
                using (var textWriter = new StreamWriter(Path.Combine(downloadsFolder, "TQM_Report_Consolidated(Stretch).csv")))
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
                    writer.WriteField("Std. Stretch");
                    writer.WriteField("Stretch");

                    writer.WriteField("User Params");
                    writer.WriteField("Remark");
                    //Actual Data
                    writer.NextRecord();
                    List<YCTestConsolidatedStretchReportMV> overallReportList = (List<YCTestConsolidatedStretchReportMV>)listview_tcConsolidatedReport.ItemsSource;
                    foreach (YCTestConsolidatedStretchReportMV orl in overallReportList)
                    {
                        writer.WriteField(orl.serialNo);
                        writer.WriteField(orl.testDate.Replace("\n"," "));
                        writer.WriteField(orl.testNo);
                        writer.WriteField(orl.machineName);

                        writer.WriteField(orl.shift);
                        writer.WriteField(orl.standardValue);


                        if (orl.stretch != null && orl.stretch != "")
                        {
                            writer.WriteField(formatDecimal(Decimal.Parse(orl.stretch)).ToString());
                        }
                        else
                        {
                            writer.WriteField(orl.stretch);
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
                //header.Graphics.DrawString("Stretch Report - " + DateTime.Now.ToString(), font_rn, brush_rn, new PointF(165, 16));

                if (consolidatedReport)
                {
                    header.Graphics.DrawString("Con. Stretch Report - " + selectedMachineCategory + " (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(135, 20));
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

                    header.Graphics.DrawString("Stretch Report - (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(165, 16));

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


        private async Task<bool> UploadFileViaFTP(string sourceFilePath, string ftpHost, string ftpUser, string ftpPass, string fileName)
        {
            try
            {
                string ftpUri = $"ftp://{ftpHost}/{fileName}";

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUri);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpUser, ftpPass);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;

                byte[] fileContents;

                try
                {
                    // File read is separated to catch IO issues specifically
                    fileContents = File.ReadAllBytes(sourceFilePath);
                }
                catch (IOException ioEx)
                {
                    showAlert("File read failed: " + ioEx.Message, "Error");
                    return false;
                }

                request.ContentLength = fileContents.Length;

                try
                {
                    using (Stream requestStream = await request.GetRequestStreamAsync())
                    {
                        await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
                    }

                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        Console.WriteLine($"✅ Upload complete: {response.StatusDescription}");
                    }

                    return true;
                }
                catch (WebException webEx)
                {
                    string ftpError = "";

                    if (webEx.Response is FtpWebResponse ftpResponse)
                    {
                        ftpError = ftpResponse.StatusDescription;
                    }

                    showAlert("FTP upload failed: " + webEx.Message + "\n" + ftpError, "FTP Error");
                    Console.WriteLine($"❌ WebException: {webEx.Message}");
                    return false;
                }
                catch (SocketException sockEx)
                {
                    showAlert("Network connection error: " + sockEx.Message, "Connection Error");
                    Console.WriteLine($"❌ SocketException: {sockEx.Message}");
                    return false;
                }
                catch (IOException ioEx)
                {
                    showAlert("I/O error during transfer: " + ioEx.Message, "IO Error");
                    Console.WriteLine($"❌ IOException: {ioEx.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    showAlert("Unexpected error during FTP: " + ex.Message, "Error");
                    Console.WriteLine($"❌ General Exception: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Top-level fallback (should not usually be reached)
                showAlert("Unexpected failure: " + ex.Message, "Fatal Error");
                Console.WriteLine($"❌ Fatal Exception: {ex.Message}");
                return false;
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

                        bool action = runConfiguration.getMoveReportToCloud();

                        if (!action)
                        {
                            string FTPServerIP = "";
                            string username = "";
                            string password = "";

                            try
                            {
                                SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
                                conn.CreateTable<CompanyModel>();
                                var company = conn.Table<CompanyModel>().FirstOrDefault();
                                if (company != null)
                                {
                                    FTPServerIP = company.ftpIpAddress;
                                    username = company.username;
                                    password = company.password;
                                }
                                conn.Close();
                            }
                            catch (Exception ex)
                            {
                                showAlert("Error occurred!!! Error: " + ex.Message.ToString(), "Error");
                                await resetBtn();
                                return;
                            }

                            if (!string.IsNullOrWhiteSpace(FTPServerIP) && !string.IsNullOrWhiteSpace(username) && password != null)
                            {
                                string fileName_ftp = "TQM_Report_Consolidated(Stretch).pdf";
                                string root_ftp = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                                Java.IO.File myDir_ftp = new Java.IO.File(root_ftp + "/TQMDownloads");
                                Java.IO.File file_ftp = new Java.IO.File(myDir_ftp, fileName_ftp);
                                string filePath_ftp = file_ftp.Path;
                                bool moved = await UploadFileViaFTP(filePath_ftp, FTPServerIP, username, password, fileName_ftp);
                                if (moved)
                                {
                                    if (runConfiguration.getCSVReportStatus())
                                    {
                                        if (!generateCSVConsolidatedReport()) { showAlert("Error occurred in CSV report generation, hence upload is unsucessful!!!"); await resetBtn(); return; }
                                        string fileName_csv = "TQM_Report_Consolidated(Stretch).csv";
                                        string root_csv = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                                        Java.IO.File myDir_csv = new Java.IO.File(root_csv + "/TQMDownloads");
                                        Java.IO.File file_csv = new Java.IO.File(myDir_csv, fileName_csv);
                                        string filePath_csv = file_csv.Path;
                                        moved = await UploadFileViaFTP(filePath_csv, FTPServerIP, username, password, fileName_csv);
                                        if (moved)
                                        {
                                            
                                            if (deleteAll)
                                            {
                                                deleteRecords(deleteList);
                                                showAlert("Report moved to network/shared folder and deleted sucessfully!!!");
                                            }
                                            else
                                            {
                                                showAlert("Report moved to network/shared folder successfully!");
                                            }
                                        }
                                        else
                                        {
                                            showAlert("Failed to move report to network/shared folder. Please try again!!!", "Error");
                                        }
                                    }
                                }
                                else
                                {
                                    showAlert("Failed to move report to network/shared folder. Please try again!!!", "Error");
                                }
                            }

                            await resetBtn();
                            return;
                        }

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
                        string fileName = "TQM_Report_Consolidated(Stretch).pdf";
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
                            request.AddParameter("title", "TQMReportsConsolidated(Stretch-" + selectedMachineCategory + ")-" + DateTime.Now.ToString());
                        }
                        else
                        {
                            request.AddParameter("title", "TQMReportsConsolidated(Stretch-All)-" + DateTime.Now.ToString());
                        }
                        request.AddFile("reportpath", filePath);
                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (runConfiguration.getCSVReportStatus())
                            {
                                if (!generateCSVConsolidatedReport()) { showAlert("Error occurred in CSV report generation, hence upload is unsucessful!!!"); await resetBtn(); return; }
                                fileName = "TQM_Report_Consolidated(Stretch).csv";
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
                                    request.AddParameter("title", "TQMReportsConsolidated-CSV-(Stretch-" + selectedMachineCategory + ")-" + DateTime.Now.ToString());
                                }
                                else
                                {
                                    request.AddParameter("title", "TQMReportsConsolidated-CSV-(Stretch-All)-" + DateTime.Now.ToString());
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
                        bool action = runConfiguration.getMoveReportToCloud();

                        if (!action)
                        {
                            string FTPServerIP = "";
                            string username = "";
                            string password = "";

                            try
                            {
                                SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
                                conn.CreateTable<CompanyModel>();
                                var company = conn.Table<CompanyModel>().FirstOrDefault();
                                if (company != null)
                                {
                                    FTPServerIP = company.ftpIpAddress;
                                    username = company.username;
                                    password = company.password;
                                }
                                conn.Close();
                            }
                            catch (Exception ex)
                            {
                                showAlert("Error occurred!!! Error: " + ex.Message.ToString(), "Error");
                                await resetBtn();
                                return;
                            }

                            if (!string.IsNullOrWhiteSpace(FTPServerIP) && !string.IsNullOrWhiteSpace(username) && password != null)
                            {
                                string fileName_ftp = "TQM_Report(Stretch).pdf";
                                string root_ftp = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                                Java.IO.File myDir_ftp = new Java.IO.File(root_ftp + "/TQMDownloads");
                                Java.IO.File file_ftp = new Java.IO.File(myDir_ftp, fileName_ftp);
                                string filePath_ftp = file_ftp.Path;
                                bool moved = await UploadFileViaFTP(filePath_ftp, FTPServerIP, username, password, fileName_ftp);
                                if (moved)
                                {
                                    if (deleteAll)
                                    {
                                        deleteRecords(deleteList);
                                        showAlert("Report moved to network/shared folder and deleted sucessfully!!!");
                                    }
                                    else
                                    {
                                        showAlert("Report moved to network/shared folder successfully!");
                                    }
                                }
                                else
                                {
                                    showAlert("Failed to move report to network/shared folder. Please try again!!!", "Error");
                                }
                            }

                            await resetBtn();
                            return;
                        }

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
                        string fileName = "TQM_Report(Stretch).pdf";
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
                        request.AddParameter("title", "TQMReports(Stretch)-" + DateTime.Now.ToString());
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