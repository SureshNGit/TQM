using CsvHelper;
using CsvHelper.Configuration;
using Java.Util;
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
using static Android.Resource;
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
        private string CON_UF_NAME_1 = null;
        private string CON_UF_NAME_2 = null;
        private string CON_UF_NAME_3 = null;
        private string CON_UF_NAME_4 = null;
        private string CON_UF_VAL_1 = null;
        private string CON_UF_VAL_2 = null;
        private string CON_UF_VAL_3 = null;
        private string CON_UF_VAL_4 = null;
        private bool isFinalAvgRowPresent = false;

        public YCReport()
        {
            InitializeComponent();
        }

        public YCReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string process, string testID, string matType,string materialLength, string standHank, bool deleteRequest, bool isConsolidated, string UFVAL1, string UFVAL2, string UFVAL3, string UFVAL4)
        {
            InitializeComponent();
            isFinalAvgRowPresent = false;
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
            getReport(startDate, endDate, categoryName, machineID, shift, process, testID, matType, materialLength, standHank, deleteRequest, UFVAL1, UFVAL2, UFVAL3, UFVAL4);
        }

        private void getReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string process, string testID, string matType, string materialLength, string standHank, bool deleteRequest, string UFVAL1, string UFVAL2, string UFVAL3, string UFVAL4)
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
                        if (!testID.Contains("."))
                        {
                            long givenTestId = long.Parse(testID);
                            ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                  YCTestSummaryModel.testID == givenTestId).ToList();
                        }
                        else
                        {
                            long startTestID = long.Parse(testID.Split('.')[0]);
                            long endTestID = long.Parse(testID.Split('.')[1]);
                            
                            if (startTestID == endTestID)
                            {
                                ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                 YCTestSummaryModel.testID == startTestID).ToList();
                            }
                            else
                            {
                                for(long i = startTestID; i <= endTestID; i++)
                                {
                                    if (ycTestSummaryModels == null)
                                    {
                                        ycTestSummaryModels = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                     YCTestSummaryModel.testID == i).ToList();
                                    }
                                    else
                                    {
                                        ycTestSummaryModels.AddRange(conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                        YCTestSummaryModel.testID == i).ToList());
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
                        List<YCTestSummaryModel> parentList = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                (YCTestSummaryModel.createdate >= startDate
                                                && YCTestSummaryModel.createdate < endDate))
                                                .OrderBy(YCTestSummaryModel => YCTestSummaryModel.createdate).ToList();
                        if (parentList.Count == 0)
                        {
                            DisplayAlert("Notice", "No records to display!!!", "OK");
                            return;
                        }

                        //Start of Logic to check last shift for the given end date is logged in end date + 1 day date
                        //Get first record of actual end date + 1 day
                        List<YCTestSummaryModel> recs_actualEndDatePlusOne = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                                        (YCTestSummaryModel.createdate >= endDate
                                                                        && YCTestSummaryModel.createdate < endDatePlusOne))
                                                                        .OrderBy(YCTestSummaryModel => YCTestSummaryModel.createdate).ToList();
                        if (recs_actualEndDatePlusOne.Count > 0) { 
                            //Check if the 1st record of actual end date + 1 day is not Shift-1
                            if (recs_actualEndDatePlusOne[0].shift != "Shift-1")
                            {
                                YCTestSummaryModel actualEndDatePlusOne_Shift1_Recs = recs_actualEndDatePlusOne.Where(YCTestSummaryModel =>
                                                                                             (YCTestSummaryModel.shift != recs_actualEndDatePlusOne[0].shift))
                                                                                            .OrderBy(YCTestSummaryModel => YCTestSummaryModel.createdate)
                                                                                            .FirstOrDefault();

                                //Merge last shift record of actual end date from (actual end date + 1day) with parent list
                                if (actualEndDatePlusOne_Shift1_Recs != null)
                                {
                                    List<YCTestSummaryModel> tempSummaryList = recs_actualEndDatePlusOne.Where(YCTestSummaryModel =>
                                                                                (YCTestSummaryModel.createdate >= endDate
                                                                                && YCTestSummaryModel.createdate < actualEndDatePlusOne_Shift1_Recs.createdate
                                                                                && YCTestSummaryModel.shift == recs_actualEndDatePlusOne[0].shift))
                                                                                .OrderBy(YCTestSummaryModel => YCTestSummaryModel.createdate).ToList();
                                    parentList.Concat(tempSummaryList);
                                }
                            }
                        }
                        //End of Logic to check last shift for the given end date is logged in end date + 1 day date

                        //Start of logic to ignore the previous date last shift record from the given actual start date
                        if (parentList[0].shift != "Shift-1")
                        {
                            YCTestSummaryModel actualStartDate_Shift1_Recs = parentList.Where(YCTestSummaryModel =>
                                                                                         (YCTestSummaryModel.shift == "Shift-1"))
                                                                                        .OrderBy(YCTestSummaryModel => YCTestSummaryModel.createdate)
                                                                                        .FirstOrDefault();
                            if (actualStartDate_Shift1_Recs != null)
                            {
                                List<YCTestSummaryModel> lastShiftOfPreviousDay_in_ActualStartDateRecs =
                                                                        parentList.Where(YCTestSummaryModel =>
                                                                        (YCTestSummaryModel.shift == parentList[0].shift
                                                                        && YCTestSummaryModel.createdate < actualStartDate_Shift1_Recs.createdate))
                                                                        .OrderBy(YCTestSummaryModel => YCTestSummaryModel.createdate)
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
                                ycTestSummaryModels = parentList.Where(YCTestSummaryModel =>
                                                    (YCTestSummaryModel.shift == shift
                                                    && YCTestSummaryModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift == "" && process != null)
                            {
                                ycTestSummaryModels = parentList.Where(YCTestSummaryModel =>
                                                    (YCTestSummaryModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift != "" && process == null)
                            {
                                ycTestSummaryModels = parentList.Where(YCTestSummaryModel =>
                                                    (YCTestSummaryModel.shift == shift)).ToList();
                            }
                            else if (shift == "" && process == null)
                            {
                                ycTestSummaryModels = parentList;
                            }

                        }
                        else if (categoryName != null && machineID == Guid.Empty)
                        {
                            //endDate = endDate.AddDays(1);

                            if (shift != "" && process != null)
                            {
                                ycTestSummaryModels = parentList.Where(YCTestSummaryModel =>
                                                     (YCTestSummaryModel.machineCategory == categoryName
                                                     && YCTestSummaryModel.shift == shift
                                                     && YCTestSummaryModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift == "" && process != null)
                            {
                                ycTestSummaryModels = parentList.Where(YCTestSummaryModel =>
                                                     (YCTestSummaryModel.machineCategory == categoryName
                                                     && YCTestSummaryModel.process.ToLower() == process.ToLower())).ToList();
                            }
                            else if (shift != "" && process == null)
                            {
                                ycTestSummaryModels = parentList.Where(YCTestSummaryModel =>
                                                     (YCTestSummaryModel.machineCategory == categoryName
                                                     && YCTestSummaryModel.shift == shift)).ToList();
                            }
                            else if (shift == "" && process == null)
                            {
                                ycTestSummaryModels = parentList.Where(YCTestSummaryModel =>
                                                    (YCTestSummaryModel.machineCategory == categoryName)).ToList();
                            }


                        }
                        else if (categoryName != null && machineID != Guid.Empty)
                        {
                            //endDate = endDate.AddDays(1);

                            if (shift != "" && process != null)
                            {
                                ycTestSummaryModels = parentList.Where(YCTestSummaryModel =>
                                                        (YCTestSummaryModel.machineCategory == categoryName)
                                                        && YCTestSummaryModel.machineID == machineID
                                                        && YCTestSummaryModel.shift == shift
                                                        && YCTestSummaryModel.process.ToLower() == process.ToLower()).ToList();
                            }
                            else if (shift == "" && process != null)
                            {
                                ycTestSummaryModels = parentList.Where(YCTestSummaryModel =>
                                                        (YCTestSummaryModel.machineCategory == categoryName)
                                                        && YCTestSummaryModel.machineID == machineID
                                                        && YCTestSummaryModel.process.ToLower() == process.ToLower()).ToList();
                            }
                            else if (shift != "" && process == null)
                            {
                                ycTestSummaryModels = parentList.Where(YCTestSummaryModel =>
                                                        (YCTestSummaryModel.machineCategory == categoryName)
                                                        && YCTestSummaryModel.machineID == machineID
                                                        && YCTestSummaryModel.shift == shift).ToList();
                            }
                            else if (shift == "" && process == null)
                            {
                                ycTestSummaryModels = parentList.Where(YCTestSummaryModel =>
                                                        (YCTestSummaryModel.machineCategory == categoryName)
                                                        && YCTestSummaryModel.machineID == machineID).ToList();
                            }

                        }
                    }



                    if (matType!="" && materialLength != "")
                    {
                        decimal yarnLength = 0.00m;
                        try
                        {
                            yarnLength = decimal.Parse(materialLength);
                        }catch(Exception){
                            DisplayAlert("Attention", "Invalid unit length!!!", "OK");
                            return;
                        }
                        ycTestSummaryModels = ycTestSummaryModels.Where(YCTestSummaryModel => (YCTestSummaryModel.yarnlength == yarnLength
                                                &&YCTestSummaryModel.yarnlenunit==matType)).ToList();
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
                        if (UFVAL1 != "" && UFVAL1 != null)
                        {
                            ycTestSummaryModels = ycTestSummaryModels.Where(t => t.uf_value_1 != null).ToList();
                            ycTestSummaryModels = ycTestSummaryModels.Where(t => t.uf_value_1.ToLower() == UFVAL1.ToLower()).ToList();
                        }
                        if (ycTestSummaryModels.Count == 0)
                        {
                            DisplayAlert("Notice", "No records to display!!!", "OK");
                            return;
                        }
                        if (UFVAL2 != "" && UFVAL2 != null)
                        {
                            ycTestSummaryModels = ycTestSummaryModels.Where(t => t.uf_value_2 != null).ToList();
                            ycTestSummaryModels = ycTestSummaryModels.Where(t => t.uf_value_2.ToLower() == UFVAL2.ToLower()).ToList();
                        }
                        if (ycTestSummaryModels.Count == 0)
                        {
                            DisplayAlert("Notice", "No records to display!!!", "OK");
                            return;
                        }
                        if (UFVAL3 != "" && UFVAL3 != null)
                        {
                            ycTestSummaryModels = ycTestSummaryModels.Where(t => t.uf_value_3 != null).ToList();
                            ycTestSummaryModels = ycTestSummaryModels.Where(t => t.uf_value_3.ToLower() == UFVAL3.ToLower()).ToList();
                        }
                        if (ycTestSummaryModels.Count == 0)
                        {
                            DisplayAlert("Notice", "No records to display!!!", "OK");
                            return;
                        }
                        if (UFVAL4 != "" && UFVAL4 != null)
                        {
                            ycTestSummaryModels = ycTestSummaryModels.Where(t => t.uf_value_4 != null).ToList();
                            ycTestSummaryModels = ycTestSummaryModels.Where(t => t.uf_value_4.ToLower() == UFVAL4.ToLower()).ToList();
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

                    if (consolidatedReport)
                    {
                        ycTestSummaryModels = ycTestSummaryModels.OrderBy(YCTestSummaryModel => YCTestSummaryModel.machineName).ToList();
                    }

                    CON_HANK = 0.0000m;
                    CON_STD_DEV = 0.0000m;
                    CON_CV = 0.0000m;

                    TOT_TEST = ycTestSummaryModels.Count;
                    int counter = 0;
                    string prev_macID = null;
                    int macTestNo = 0;
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
                                        //lbl_con_Hank.Text = "Avg. COUNT : ";
                                        lbl_conStdHank.Text = "Std. Count";
                                        lbl_conAvgHank.Text = "Avg. Count";
                                    }
                                    else
                                    {
                                        //lbl_con_Hank.Text = "Avg. HANK : ";
                                        lbl_conStdHank.Text = "Std. Hank";
                                        lbl_conAvgHank.Text = "Avg. Hank";
                                    }

                                    if (UFVAL1 != "" && UFVAL1 != null)
                                    {
                                        YarnCountConfigModel ycConfig_uf = conn.Table<YarnCountConfigModel>().
                                                                                        Where(YarnCountConfigModel => (YarnCountConfigModel.uf_name_1 != null ||
                                                                                        YarnCountConfigModel.uf_name_1 != "")
                                                                                        && YarnCountConfigModel.machineCategory == testsummary.machineCategory
                                                                                        && YarnCountConfigModel.machineID == testsummary.machineID
                                                                                        && YarnCountConfigModel.machineName == testsummary.machineName).FirstOrDefault();
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
                                                                                        && YarnCountConfigModel.machineCategory == testsummary.machineCategory
                                                                                        && YarnCountConfigModel.machineID == testsummary.machineID
                                                                                        && YarnCountConfigModel.machineName == testsummary.machineName).FirstOrDefault();
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
                                                                                        && YarnCountConfigModel.machineCategory == testsummary.machineCategory
                                                                                        && YarnCountConfigModel.machineID == testsummary.machineID
                                                                                        && YarnCountConfigModel.machineName == testsummary.machineName).FirstOrDefault();
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
                                                                                        && YarnCountConfigModel.machineCategory == testsummary.machineCategory
                                                                                        && YarnCountConfigModel.machineID == testsummary.machineID
                                                                                        && YarnCountConfigModel.machineName == testsummary.machineName).FirstOrDefault();
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
                                                                                        YarnCountConfigModel.machineCategory == testsummary.machineCategory
                                                                                        && YarnCountConfigModel.machineID == testsummary.machineID
                                                                                        && YarnCountConfigModel.machineName == testsummary.machineName)).FirstOrDefault();
                                if (ycConfig_uf_1 == null)
                                {
                                    DisplayAlert("Notice", "Unable to reterive user fields from settings!!!", "OK");
                                    return;
                                }

                                consolItems.uf_name_1 = ycConfig_uf_1.uf_name_1;
                                consolItems.uf_name_2 = ycConfig_uf_1.uf_name_2;
                                consolItems.uf_name_3 = ycConfig_uf_1.uf_name_3;
                                consolItems.uf_name_4 = ycConfig_uf_1.uf_name_4;

                                consolItems.uf_value_1 = testsummary.uf_value_1;
                                consolItems.uf_value_2 = testsummary.uf_value_2;
                                consolItems.uf_value_3 = testsummary.uf_value_3;
                                consolItems.uf_value_4 = testsummary.uf_value_4;

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

                                if (prev_macID == null)
                                {
                                    prev_macID = testsummary.machineID.ToString();
                                    macTestNo = 1;
                                }
                                else
                                {
                                    if (prev_macID != testsummary.machineID.ToString())
                                    {
                                        prev_macID = testsummary.machineID.ToString();
                                        macTestNo = 1;
                                    }
                                    else
                                    {
                                        macTestNo = macTestNo + 1;
                                    }
                                }

                                consolItems.serialNo = (counter + 1).ToString();
                                consolItems.testNo = macTestNo.ToString();
                                consolItems.testID = testsummary.testID.ToString();
                                consolItems.machineName = testsummary.machineName;
                                consolItems.testDate = testsummary.createdate.Day.ToString() + "-" +
                                                        testsummary.createdate.Month.ToString() + "-" +
                                                        testsummary.createdate.Year.ToString() + "\n" +
                                                        formatTime(testsummary.createdate);
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
                                //consolItems.standardDeviation = formatDecimal(testsummary.testsd).ToString();
                                consolItems.standardDeviation = formatDecimal(testsummary.rhcorrectedhank,4).ToString() + "\n" + "[RH%: " + testsummary.rhcorrection.ToString() +"]" ;
                                if (testsummary.standardCV == 0.0m)
                                {
                                    consolItems.CoEfficientOfVariation = formatDecimal(testsummary.testcv).ToString();
                                }
                                else
                                {
                                    consolItems.CoEfficientOfVariation = formatDecimal(testsummary.testcv).ToString() +
                                                                        " \n" +
                                                                        "(" +
                                                                        formatDecimal(testsummary.standardCV, 4).ToString() +
                                                                        "\n" +
                                                                        "\u00B1" +
                                                                        formatDecimal(testsummary.CVDeviationPercent, 4).ToString() +
                                                                        ")";
                                }
                                //consolItems.CoEfficientOfVariation = formatDecimal(testsummary.testcv).ToString();
                                //consolItems.testDuration = testsummary.testDuration;
                                //consolItems.testDuration = formatTime(testsummary.createdate);

                                decimal maxRangeVal_CV = testsummary.standardCV + testsummary.CVDeviationPercent;
                                decimal minRangeVal_CV = testsummary.standardCV - testsummary.CVDeviationPercent;


                                if (testsummary.yarnWeightCV < minRangeVal_CV || testsummary.yarnWeightCV > maxRangeVal_CV)
                                {
                                    if (testsummary.standardCV > 0.0m)
                                    {
                                        consolItems.isRed_CV = true;
                                        consolItems.isWhite_CV = false;
                                    }
                                    else
                                    {
                                        consolItems.isRed_CV = false;
                                        consolItems.isWhite_CV = true;
                                    }
                                }
                                else
                                {
                                    consolItems.isRed_CV = false;
                                    consolItems.isWhite_CV = true;
                                }


                                string userParams = "";

                                if(testsummary.uf_value_1!=null && testsummary.uf_value_1 != "")
                                {
                                    userParams = testsummary.uf_value_1;
                                }

                                if (testsummary.uf_value_2 != null && testsummary.uf_value_2 != "")
                                {
                                    if (userParams == "")
                                    { 
                                        userParams = testsummary.uf_value_2;
                                    }
                                    else
                                    {
                                        userParams = userParams + "\n" + testsummary.uf_value_2;
                                    }
                                }

                                if (testsummary.uf_value_3 != null && testsummary.uf_value_3 != "")
                                {
                                    if (userParams == "")
                                    {
                                        userParams = testsummary.uf_value_3;
                                    }
                                    else
                                    {
                                        userParams = userParams + "\n" + testsummary.uf_value_3;
                                    }
                                }

                                if (testsummary.uf_value_4 != null && testsummary.uf_value_4 != "")
                                {
                                    if (userParams == "")
                                    {
                                        userParams = testsummary.uf_value_4;
                                    }
                                    else
                                    {
                                        userParams = userParams + "\n" + testsummary.uf_value_4;
                                    }
                                }

                                consolItems.testDuration = userParams;

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

                                //List<YCTestReportModelView> finalView = new List<YCTestReportModelView>();
                                int loopCount = 0;
                                decimal totalWeight = 0.0000m;
                                decimal totalCalcCountVal = 0.0000m;

                                

                                foreach (YCTestModel test in yctestlist)
                                {
                                    
                                    YCTestReportModelView ycTestReportMV = new YCTestReportModelView()
                                    {
                                        testID = test.testID,
                                        description = test.testcount.ToString(),
                                        weight = formatDecimal(test.yarnweight).ToString(),
                                        hank = formatDecimal(test.yccalcval).ToString(),
                                    };
                                    report.Add(ycTestReportMV);
                                    loopCount += 1;
                                    totalCalcCountVal = totalCalcCountVal + test.yccalcval;
                                    totalCalcCountVal = formatDecimal(totalCalcCountVal);
                                    totalWeight = totalWeight + test.yarnweight;
                                    totalWeight = formatDecimal(totalWeight);
                                }


                                decimal mean = 0.0000m;
                                decimal min = 0.0000m;
                                decimal max = 0.0000m;
                                decimal range = 0.0000m;
                                decimal sd = 0.0000m;
                                decimal cv = 0.0000m;

                                decimal mean_weight = 0.0000m;
                                decimal min_weight = 0.0000m;
                                decimal max_weight = 0.0000m;
                                decimal range_weight = 0.0000m;
                                decimal sd_weight = 0.0000m;
                                decimal cv_weight = 0.0000m;

                                if (testsummary.yarnWeightAvg == 0.0m)
                                {
                                    mean = totalCalcCountVal / yctestlist[0].totaltestcount;
                                    decimal IndividualCalValminusMean = 0m;
                                    foreach (YCTestModel test in yctestlist)
                                    {
                                        IndividualCalValminusMean = IndividualCalValminusMean + ((test.yccalcval - mean) * (test.yccalcval - mean));
                                    }
                                    sd = (decimal)Math.Sqrt((double)IndividualCalValminusMean / (double)(yctestlist[0].totaltestcount - 1));//Standard Deviation
                                    sd = formatDecimal(sd);
                                    mean = formatDecimal(mean);
                                    cv = (sd / mean) * 100.0000m; //Coefficient of Variation
                                    cv = formatDecimal(cv);

                                    min = yctestlist.Min(YCTestModel => YCTestModel.yccalcval);
                                    max = yctestlist.Max(YCTestModel => YCTestModel.yccalcval);
                                    range = max - min;


                                    mean_weight = totalWeight / yctestlist[0].totaltestcount;
                                    decimal IndividualWeightminusMean = 0m;
                                    foreach (YCTestModel test in yctestlist)
                                    {
                                        IndividualWeightminusMean = IndividualWeightminusMean + ((test.yarnweight - mean) * (test.yarnweight - mean));
                                    }
                                    sd_weight = (decimal)Math.Sqrt((double)IndividualWeightminusMean / (double)(yctestlist[0].totaltestcount - 1));//Standard Deviation
                                    sd_weight = formatDecimal(sd_weight);
                                    mean_weight = formatDecimal(mean_weight);
                                    cv_weight = (sd_weight / mean_weight) * 100.0000m; //Coefficient of Variation
                                    cv_weight = formatDecimal(cv_weight);

                                    min_weight = yctestlist.Min(YCTestModel => YCTestModel.yarnweight);
                                    max_weight = yctestlist.Max(YCTestModel => YCTestModel.yarnweight);
                                    range_weight = max_weight - min_weight;
                                }
                                else
                                {
                                    mean_weight = testsummary.yarnWeightAvg;
                                    mean = testsummary.testaverage;

                                    max_weight = testsummary.yarnWeightMax;
                                    max = testsummary.testMax;

                                    min_weight = testsummary.yarnWeightMin;
                                    min = testsummary.testMin;

                                    range_weight = testsummary.yarnWeightRange;
                                    range = testsummary.testRange;

                                    sd_weight = testsummary.yarnWeightSD;
                                    sd = testsummary.testsd;

                                    cv_weight = testsummary.yarnWeightCV;
                                    cv = testsummary.testcv;
                                }

                                YCTestReportModelView testMV = new YCTestReportModelView()
                                {
                                    testID = testsummary.testID,
                                    description = "Average",
                                    weight = formatDecimal(mean_weight).ToString(),
                                    hank = formatDecimal(mean).ToString(),
                                };
                                report.Add(testMV);

                                testMV = new YCTestReportModelView()
                                {
                                    testID = testsummary.testID,
                                    description = "Max",
                                    weight = formatDecimal(max_weight).ToString(),
                                    hank = formatDecimal(max).ToString(),
                                };
                                report.Add(testMV);

                                testMV = new YCTestReportModelView()
                                {
                                    testID = testsummary.testID,
                                    description = "Min",
                                    weight = formatDecimal(min_weight).ToString(),
                                    hank = formatDecimal(min).ToString(),
                                };
                                report.Add(testMV);

                                testMV = new YCTestReportModelView()
                                {
                                    testID = testsummary.testID,
                                    description = "Range",
                                    weight = formatDecimal(range_weight).ToString(),
                                    hank = formatDecimal(range).ToString(),
                                };
                                report.Add(testMV);


                                testMV = new YCTestReportModelView()
                                {
                                    testID = testsummary.testID,
                                    description = "SD",
                                    weight = formatDecimal(sd_weight).ToString(),
                                    hank = formatDecimal(sd).ToString(),
                                };
                                report.Add(testMV);

                                testMV = new YCTestReportModelView()
                                {
                                    testID = testsummary.testID,
                                    description = "CV",
                                    weight = formatDecimal(cv_weight).ToString(),
                                    hank = formatDecimal(cv).ToString(),
                                };
                                report.Add(testMV);




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
                                report.standardCV = testsummary.standardCV;
                                report.CVDeviationPercent= "\u00B1" + testsummary.CVDeviationPercent;
                                report.rhcorrectedhank = testsummary.rhcorrectedhank;
                                report.rhcorrectionpercentage = testsummary.rhcorrection;

                                if (testsummary.uf_value_1 != null && testsummary.uf_value_1 != "")
                                {
                                    YarnCountConfigModel ycConfig_uf = conn.Table<YarnCountConfigModel>().
                                                                                        Where(YarnCountConfigModel => (YarnCountConfigModel.uf_name_1 != null ||
                                                                                        YarnCountConfigModel.uf_name_1 != "")
                                                                                        && YarnCountConfigModel.machineCategory == testsummary.machineCategory
                                                                                        && YarnCountConfigModel.machineID == testsummary.machineID
                                                                                        && YarnCountConfigModel.machineName == testsummary.machineName).FirstOrDefault();
                                    if (ycConfig_uf != null)
                                    {
                                        report.uf_name_1 = ycConfig_uf.uf_name_1;
                                        report.DispUF_1 = true;
                                        report.uf_value_1 = testsummary.uf_value_1;
                                        report.remark_1 = false;
                                        report.DispUF_2_Col1 = false;
                                    }
                                    else
                                    {
                                        report.DispUF_1 = false;
                                        report.uf_value_1 = null;
                                        report.uf_name_1 = null;
                                        report.remark_1 = true;
                                    }
                                }
                                else
                                {
                                    report.DispUF_1 = false;
                                    report.uf_value_1 = null;
                                    report.uf_name_1 = null;
                                    report.remark_1 = true;
                                }
                                if (testsummary.uf_value_2 != null && testsummary.uf_value_2 != "")
                                {
                                    YarnCountConfigModel ycConfig_uf = conn.Table<YarnCountConfigModel>().
                                                                                        Where(YarnCountConfigModel =>
                                                                                        (YarnCountConfigModel.uf_name_2 != null ||
                                                                                        YarnCountConfigModel.uf_name_2 != "")
                                                                                        && YarnCountConfigModel.machineCategory == testsummary.machineCategory
                                                                                        && YarnCountConfigModel.machineID == testsummary.machineID
                                                                                        && YarnCountConfigModel.machineName == testsummary.machineName).FirstOrDefault();
                                    if (ycConfig_uf != null)
                                    {
                                        if (!report.DispUF_1)
                                        {
                                            report.remark_1 = false;
                                            report.uf_name_2 = ycConfig_uf.uf_name_2;
                                            report.DispUF_2_Col1 = true;
                                            report.DispUF_2 = false;
                                            report.uf_value_2 = testsummary.uf_value_2;
                                        }
                                        else
                                        {
                                            report.remark_1 = false;
                                            report.uf_name_2 = ycConfig_uf.uf_name_2;
                                            report.DispUF_2 = true;
                                            report.DispUF_2_Col1 = false;
                                            report.uf_value_2 = testsummary.uf_value_2;
                                        }
                                    }
                                    else
                                    {
                                        report.DispUF_2 = false;
                                        report.uf_value_2 = null;
                                        report.uf_name_2 = null;
                                    }
                                }
                                else
                                {
                                    report.DispUF_2 = false;
                                    report.uf_value_2 = null;
                                    report.uf_name_2 = null;
                                }
                                if (testsummary.uf_value_3 != null && testsummary.uf_value_3 != "")
                                {
                                    YarnCountConfigModel ycConfig_uf = conn.Table<YarnCountConfigModel>().
                                                                                       Where(YarnCountConfigModel =>
                                                                                       (YarnCountConfigModel.uf_name_3 != null ||
                                                                                       YarnCountConfigModel.uf_name_3 != "")
                                                                                       && YarnCountConfigModel.machineCategory == testsummary.machineCategory
                                                                                       && YarnCountConfigModel.machineID == testsummary.machineID
                                                                                       && YarnCountConfigModel.machineName == testsummary.machineName).FirstOrDefault();
                                    if (ycConfig_uf != null)
                                    {
                                        report.uf_name_3 = ycConfig_uf.uf_name_3;
                                        report.DispUF_3 = true;
                                        report.uf_value_3 = testsummary.uf_value_3;
                                        report.remark_2 = false;
                                        report.DispUF_4_Col1 = false;
                                    }
                                    else
                                    {
                                        report.DispUF_3 = false;
                                        report.uf_value_3 = null;
                                        report.uf_name_3 = null;
                                        report.remark_2 = true;
                                    }
                                }
                                else
                                {
                                    report.DispUF_3 = false;
                                    report.uf_value_3 = null;
                                    report.uf_name_3 = null;
                                    if (report.remark_1 == false)
                                    {
                                        report.remark_2 = true;
                                    }
                                }
                                if (testsummary.uf_value_4 != null && testsummary.uf_value_4 != "")
                                {
                                    YarnCountConfigModel ycConfig_uf = conn.Table<YarnCountConfigModel>().
                                                                                       Where(YarnCountConfigModel =>
                                                                                       (YarnCountConfigModel.uf_name_4 != null ||
                                                                                       YarnCountConfigModel.uf_name_4 != "")
                                                                                       && YarnCountConfigModel.machineCategory == testsummary.machineCategory
                                                                                       && YarnCountConfigModel.machineID == testsummary.machineID
                                                                                       && YarnCountConfigModel.machineName == testsummary.machineName).FirstOrDefault();
                                    if (ycConfig_uf != null)
                                    {
                                        if (!report.DispUF_3)
                                        {
                                            report.remark_2 = false;
                                            report.uf_name_4 = ycConfig_uf.uf_name_4;
                                            report.DispUF_4_Col1 = true;
                                            report.DispUF_4 = false;
                                            report.uf_value_4 = testsummary.uf_value_4;
                                        }
                                        else
                                        {
                                            report.remark_2 = false;
                                            report.uf_name_4 = ycConfig_uf.uf_name_4;
                                            report.DispUF_4 = true;
                                            report.DispUF_4_Col1 = false;
                                            report.uf_value_4 = testsummary.uf_value_4;
                                        }
                                    }
                                    else
                                    {
                                        report.DispUF_4 = false;
                                        report.uf_value_4 = null;
                                        report.uf_name_4 = null;
                                    }
                                }
                                else
                                {
                                    report.DispUF_4 = false;
                                    report.uf_value_4 = null;
                                    report.uf_name_4 = null;
                                }

                                if (report.remark_1 == false && report.remark_2 == false) { report.remark_3 = true; }

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

                                decimal maxRangeVal_CV = testsummary.standardCV + testsummary.CVDeviationPercent;
                                decimal minRangeVal_CV = testsummary.standardCV - testsummary.CVDeviationPercent;


                                if (testsummary.yarnWeightCV < minRangeVal_CV || testsummary.yarnWeightCV > maxRangeVal_CV)
                                {
                                    report.CVColor = "Red";
                                    report.CVColorGg = "Yellow";
                                }
                                else
                                {
                                    report.CVColor = "Green";
                                    report.CVColorGg = "None";
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
                isFinalAvgRowPresent = false;
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

                    List<YCTestReportModelView> testList = orl.yctestlist;

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
                        pdfGridInfo.Rows[4].Cells[0].Value = "Hank: " + formatDecimal(orl.testaverage, 4).ToString() +
                            " [Std Hank: " + formatDecimal(orl.standardHank, 4) + " " + orl.deviationPercent + "]";
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


                    pdfGridInfo.Rows[4].Cells[2].Value = "RH Corrected Hank: " + formatDecimal(orl.rhcorrectedhank, 4).ToString() + " [RH% : " + orl.rhcorrectionpercentage.ToString() + "]";
                    pdfGridInfo.Rows[4].Cells[2].ColumnSpan = 2;
                    //pdfGridInfo.Rows[4].Cells[0].Style.TextPen = PdfPens.Red;
                    //pdfGridInfo.Rows[4].Cells[2].Value = "SD: " + orl.testsd;
                    //pdfGridInfo.Rows[4].Cells[1].Style.TextPen = PdfPens.Red;
                    //pdfGridInfo.Rows[4].Cells[3].Value = "CV: " + orl.testcv;
                    //pdfGridInfo.Rows[4].Cells[2].Style.TextPen = PdfPens.Red;
                    //pdfGridInfo.Rows[4].Cells[3].Value = "A%: " + orl.apercent;

                    //To bold HANK and CV Values
                    pdfGridInfo.Rows[4].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                    pdfGridInfo.Rows[4].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                    pdfGridInfo.Rows[4].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                    pdfGridInfo.Rows[4].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                    //pdfGridInfo.Rows[4].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);

                    pdfGridInfo.Rows[5].Cells[0].Value = "Date: " + orl.createdate;
                    pdfGridInfo.Rows[5].Cells[0].ColumnSpan = 1;
                    pdfGridInfo.Rows[5].Cells[1].Value = "Duration: " + orl.testDuration;
                    pdfGridInfo.Rows[5].Cells[2].Value = "Shift: " + orl.shift;
                    pdfGridInfo.Rows[5].Cells[3].Value = "Process: " + orl.process;

                    if (orl.remark_1)
                    {
                        pdfGridInfo.Rows[6].Cells[0].Value = "Remark: " + orl.testRemark;
                        pdfGridInfo.Rows[6].Cells[0].ColumnSpan = 4;
                    }
                    else if (orl.remark_1 == false && orl.DispUF_1)
                    {
                        pdfGridInfo.Rows[6].Cells[0].Value = orl.uf_name_1 + ": " + orl.uf_value_1;
                        pdfGridInfo.Rows[6].Cells[0].ColumnSpan = 2;

                        if (orl.DispUF_2)
                        {
                            pdfGridInfo.Rows[6].Cells[2].Value = orl.uf_name_2 + ": " + orl.uf_value_2;
                            pdfGridInfo.Rows[6].Cells[2].ColumnSpan = 2;
                        }

                    }
                    else if (orl.remark_1 == false && orl.DispUF_1 == false && orl.DispUF_2_Col1 == true)
                    {
                        pdfGridInfo.Rows[6].Cells[0].Value = orl.uf_name_2 + ": " + orl.uf_value_2;
                        pdfGridInfo.Rows[6].Cells[0].ColumnSpan = 2;
                    }

                    if (orl.remark_2)
                    {
                        pdfGridInfo.Rows.Add();
                        pdfGridInfo.Rows[7].Cells[0].Value = "Remark: " + orl.testRemark;
                        pdfGridInfo.Rows[7].Cells[0].ColumnSpan = 4;
                    }
                    else if (orl.remark_2 == false && orl.DispUF_3)
                    {
                        pdfGridInfo.Rows.Add();
                        pdfGridInfo.Rows[7].Cells[0].Value = orl.uf_name_3 + ": " + orl.uf_value_3;
                        pdfGridInfo.Rows[7].Cells[0].ColumnSpan = 2;

                        if (orl.DispUF_4)
                        {
                            pdfGridInfo.Rows[7].Cells[2].Value = orl.uf_name_4 + ": " + orl.uf_value_4;
                            pdfGridInfo.Rows[7].Cells[2].ColumnSpan = 2;
                        }

                    }
                    else if (orl.remark_2 == false && orl.DispUF_3 == false && orl.DispUF_4_Col1 == true)
                    {
                        pdfGridInfo.Rows.Add();
                        pdfGridInfo.Rows[7].Cells[0].Value = orl.uf_name_4 + ": " + orl.uf_value_4;
                        pdfGridInfo.Rows[7].Cells[0].ColumnSpan = 2;
                    }

                    if (orl.remark_1 == false && orl.remark_2 == false)
                    {
                        pdfGridInfo.Rows.Add();
                        pdfGridInfo.Rows[8].Cells[0].Value = "Remark: " + orl.testRemark;
                        pdfGridInfo.Rows[8].Cells[0].ColumnSpan = 4;
                    }


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

                    if ((orl.remark_1 == false && orl.DispUF_1) || (orl.remark_1 == false && orl.DispUF_1 == false && orl.DispUF_2_Col1))
                    {
                        pdfGridInfo.Rows[7].Cells[0].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[7].Cells[1].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[7].Cells[2].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[7].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    }
                    if ((orl.remark_2 == false && orl.DispUF_3) || (orl.remark_2 == false && orl.DispUF_3 == false && orl.DispUF_4_Col1))
                    {
                        pdfGridInfo.Rows[8].Cells[0].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[8].Cells[1].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[8].Cells[2].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[8].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    }

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
                    foreach (YCTestReportModelView test in testList)
                    {
                        row = new PdfGridRow(pdfGrid);
                        pdfGrid.Rows.Add(row);
                        pdfGrid.Rows[rowCount].Cells[0].Value = test.description.ToString();
                        if (orl.machineCategory == "Spinning" || orl.machineCategory == "Winding")
                        {
                            pdfGrid.Rows[rowCount].Cells[1].Value = formatDecimal(Decimal.Parse(test.weight), 2).ToString();
                            pdfGrid.Rows[rowCount].Cells[2].Value = formatDecimal(Decimal.Parse(test.hank), 2).ToString();
                        }
                        else
                        {
                            if (test.description.ToString().Equals("CV"))
                            {
                                if (orl.standardCV == 0.0m)
                                {
                                    pdfGrid.Rows[rowCount].Cells[1].Value = formatDecimal(Decimal.Parse(test.weight), 4).ToString();
                                    pdfGrid.Rows[rowCount].Cells[2].Value = formatDecimal(Decimal.Parse(test.hank), 4).ToString();
                                }
                                else
                                {
                                    pdfGrid.Rows[rowCount].Cells[1].Value = formatDecimal(Decimal.Parse(test.weight), 4).ToString() +
                                                                            "(" +
                                                                            formatDecimal(orl.standardCV, 4).ToString() +
                                                                            orl.CVDeviationPercent + ")";
                                    pdfGrid.Rows[rowCount].Cells[2].Value = formatDecimal(Decimal.Parse(test.hank), 4).ToString();
                                    if (orl.CVColor == "Red")
                                    {
                                        pdfGrid.Rows[rowCount].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                                        pdfGrid.Rows[rowCount].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                                        pdfGrid.Rows[rowCount].Cells[1].Style.BackgroundBrush = PdfBrushes.Red;
                                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                                        pdfGrid.Rows[rowCount].Cells[1].Style.TextBrush = brush_con;
                                    }
                                }
                            }
                            else
                            {
                                pdfGrid.Rows[rowCount].Cells[1].Value = formatDecimal(Decimal.Parse(test.weight), 4).ToString();
                                pdfGrid.Rows[rowCount].Cells[2].Value = formatDecimal(Decimal.Parse(test.hank), 4).ToString();
                            }
                        }

                        

                        if (test.description.ToString().Equals("Average") ||
                            test.description.ToString().Equals("SD") ||
                            test.description.ToString().Equals("CV"))
                        {
                            pdfGrid.Rows[rowCount].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                            pdfGrid.Rows[rowCount].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                            pdfGrid.Rows[rowCount].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
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

        [Obsolete]
        private bool generateCSVConsolidatedReport()
        {
            try
            {
                string downloadsFolder = Path.Combine(Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads),"TQMDownloads");
                using (var textWriter = new StreamWriter(Path.Combine(downloadsFolder, "TQM_Report_Consolidated(Wrapping).csv")))
                {
                   
                    var writer = new CsvWriter(textWriter, CultureInfo.InvariantCulture);
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter=",",
                        HasHeaderRecord=false
                    };
                    //Header
                    writer.WriteField("S.No");
                    writer.WriteField("Date");
                    writer.WriteField("Test No");
                    writer.WriteField("Mac Name");
                    writer.WriteField("Mix ID");
                    writer.WriteField("Lot No");
                    writer.WriteField("P Type");
                    writer.WriteField("Shift");
                    if (selectedMachineCategory == "Spinning" || selectedMachineCategory == "Winding")
                    {
                        //writer.WriteField("Std. Count");
                        writer.WriteField("Avg. Count");
                    }
                    else
                    {
                        //writer.WriteField("Std. Hank");
                        writer.WriteField("Avg. Hank");
                    }
                    writer.WriteField("RH Corrected Hank");
                    writer.WriteField("CV");
                    //writer.WriteField("Test Time");
                    writer.WriteField("Remark");
                    //Actual Data
                    writer.NextRecord();
                    List<YCTestConsolidatedReportMV> overallReportList = (List<YCTestConsolidatedReportMV>)listview_tcConsolidatedReport.ItemsSource;
                    foreach (YCTestConsolidatedReportMV orl in overallReportList)
                    {
                        writer.WriteField(orl.serialNo);
                        writer.WriteField(orl.testDate.Replace("\n"," "));
                        writer.WriteField(orl.testNo);
                        writer.WriteField(orl.machineName);
                        if (orl.uf_value_2 != null)
                        {
                            writer.WriteField(orl.uf_value_2);
                        }
                        else
                        {
                            writer.WriteField("");
                        }
                        if (orl.uf_value_1 != null)
                        {
                            writer.WriteField(orl.uf_value_1);
                        }
                        else
                        {
                            writer.WriteField("");
                        }
                        if (orl.uf_value_4 != null)
                        {
                            writer.WriteField(orl.uf_value_4);
                        }
                        else
                        {
                            writer.WriteField("");
                        }
                        writer.WriteField(orl.shift);
                        //writer.WriteField(orl.standardValue);


                        if (orl.testAverage != null && orl.testAverage != "")
                        {
                            writer.WriteField(formatDecimal(Decimal.Parse(orl.testAverage)).ToString());
                        }
                        else
                        {
                            writer.WriteField(orl.testAverage);
                        }

                        //if (orl.standardDeviation != null && orl.standardDeviation != "")
                        //{
                        //    writer.WriteField(formatDecimal(Decimal.Parse(orl.standardDeviation)).ToString());
                        //}
                        //else
                        //{
                        //    writer.WriteField(orl.standardDeviation);
                        //}

                        
                        writer.WriteField(orl.rhcorrectedhank.ToString() + "\n [" + orl.rhcorrectionpercentage + "]");
                      

                        if (orl.CoEfficientOfVariation != null && orl.CoEfficientOfVariation != "")
                        {
                            writer.WriteField(orl.CoEfficientOfVariation);
                        }
                        else
                        {
                            writer.WriteField(orl.CoEfficientOfVariation);
                        }

                        //writer.WriteField(orl.testDuration);
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
                        pdfGrid.Rows[0].Cells[3].Value = "M.Name";
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
                            pdfGrid.Rows[0].Cells[5].Value = "Std.Count";
                            pdfGrid.Rows[0].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                            pdfGrid.Rows[0].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                            pdfGrid.Rows[0].Cells[5].Style.BackgroundBrush = PdfBrushes.LightGray;
                            pdfGrid.Rows[0].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                            pdfGrid.Rows[0].Cells[6].Value = "Avg.Count";
                            pdfGrid.Rows[0].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                            pdfGrid.Rows[0].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                            pdfGrid.Rows[0].Cells[6].Style.BackgroundBrush = PdfBrushes.LightGray;
                            pdfGrid.Rows[0].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        }
                        else
                        {
                            pdfGrid.Rows[0].Cells[5].Value = "Std.Hank";
                            pdfGrid.Rows[0].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                            pdfGrid.Rows[0].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                            pdfGrid.Rows[0].Cells[5].Style.BackgroundBrush = PdfBrushes.LightGray;
                            pdfGrid.Rows[0].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                            pdfGrid.Rows[0].Cells[6].Value = "Avg.Hank";
                            pdfGrid.Rows[0].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                            pdfGrid.Rows[0].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                            pdfGrid.Rows[0].Cells[6].Style.BackgroundBrush = PdfBrushes.LightGray;
                            pdfGrid.Rows[0].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        }

                        //pdfGrid.Rows[0].Cells[7].Value = "SD";
                        pdfGrid.Rows[0].Cells[7].Value = "RHC.Hank";
                        pdfGrid.Rows[0].Cells[7].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[7].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[7].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[7].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[8].Value = "CV";
                        pdfGrid.Rows[0].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[8].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[9].Value = "U.Params";
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
                    //if (orl.standardDeviation != null && orl.standardDeviation != "")
                    //{
                    //    pdfGrid.Rows[pageRecordCount].Cells[7].Value = formatDecimal(Decimal.Parse(orl.standardDeviation)).ToString();
                    //}
                    //else
                    //{
                    //    pdfGrid.Rows[pageRecordCount].Cells[7].Value = orl.standardDeviation;
                    //}

                    pdfGrid.Rows[pageRecordCount].Cells[7].Value = orl.standardDeviation;

                    if (orl.CoEfficientOfVariation != null && orl.CoEfficientOfVariation != "")
                    {
                        if (orl.isRed_CV)
                        {
                            pdfGrid.Rows[pageRecordCount].Cells[8].Value = orl.CoEfficientOfVariation;
                        }
                        else
                        {
                            pdfGrid.Rows[pageRecordCount].Cells[8].Value = orl.CoEfficientOfVariation;
                        }
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

                        if (orl.testDuration != null)
                        {
                            if (orl.testDuration.Length > contentLength)
                            {
                                contentLength = orl.testDuration.Length;
                            }
                        }

                        if (contentLength >= 9 && contentLength >= currentRowHeight)
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
                    //pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

                    if (orl.isWhite_CV)
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[8].Style.BackgroundBrush = PdfBrushes.White;
                        //pdfGrid.Rows[pageRecordCount].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.Black);
                        pdfGrid.Rows[pageRecordCount].Cells[8].Style.TextBrush = brush_con;
                    }
                    else
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[8].Style.BackgroundBrush = PdfBrushes.Red;
                        //pdfGrid.Rows[pageRecordCount].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                        pdfGrid.Rows[pageRecordCount].Cells[8].Style.TextBrush = brush_con;
                    }

                    pdfGrid.Rows[pageRecordCount].Cells[9].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[9].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[10].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[10].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

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
                    
                    //**************************** End of Overall total *****************************

                    rowHeights = rowHeights + pdfGrid.Rows[pageRecordCount].Height;
                    if (rowHeights <= 700 && rowCount == overallReportList.Count)
                    {
                        result = pdfGrid.Draw(pdfPage, new PointF(10, 75), layoutFormat);
                    }
                    else if ((rowHeights >= 600 && rowHeights <= 700) && pageRecordCount != overallReportList.Count)
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
                RectangleF bounds = new RectangleF(0, 0, pdfDocument.Pages[i].GetClientSize().Width, 70);
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
                PdfFont font_rn_uf = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Regular);
                PdfBrush brush_rn = new PdfSolidBrush(Syncfusion.Drawing.Color.Blue);
                if (consolidatedReport)
                {
                    header.Graphics.DrawString("Con. Wrappping Report - " + selectedMachineCategory + " (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(135, 20));
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
                    if (selectedMachineCategory != null)
                    {
                        header.Graphics.DrawString("Wrapping Report - " + selectedMachineCategory + " (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(165, 16));
                    }
                    else
                    {
                        header.Graphics.DrawString("Wrapping Report - All (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(165, 16));
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
                        if (selectedMachineCategory != null)
                        {
                            request.AddParameter("title", "TQMReportsConsolidated(Wrapping-" + selectedMachineCategory + ")-" + DateTime.Now.ToString());
                        }
                        else
                        {
                            request.AddParameter("title", "TQMReportsConsolidated(Wrapping-All)-" + DateTime.Now.ToString());
                        }
                        request.AddFile("reportpath", filePath);
                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (runConfiguration.getCSVReportStatus())
                            {
                                if (!generateCSVConsolidatedReport()) { showAlert("Error occurred in CSV report generation, hence upload is unsucessful!!!"); await resetBtn(); return; }
                                fileName = "TQM_Report_Consolidated(Wrapping).csv";
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
                                    request.AddParameter("title", "TQMReportsConsolidated-CSV-(Wrapping-" + selectedMachineCategory + ")-" + DateTime.Now.ToString());
                                }
                                else
                                {
                                    request.AddParameter("title", "TQMReportsConsolidated-CSV-(Wrapping-All)-" + DateTime.Now.ToString());
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
                        }else
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
                        if (selectedMachineCategory != null)
                        {
                            request.AddParameter("title", "TQMReports(Wrapping-" + selectedMachineCategory + ")-" + DateTime.Now.ToString());
                        }
                        else
                        {
                            request.AddParameter("title", "TQMReports(Wrapping-All" + DateTime.Now.ToString());
                        }
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

    }

}