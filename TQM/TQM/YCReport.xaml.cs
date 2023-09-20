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
using System.Security.Cryptography;
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
        private static readonly DateTime DEFAULTDATE = new DateTime(2000, 01, 01);
        private List<OverallReportModelView> _listOfReports;
        public List<OverallReportModelView> ListOfReport { get { return _listOfReports; } set { _listOfReports = value; base.OnPropertyChanged(); } }

        private List<StrengthTestConsolidatedReportMV> _listOfConsolidatedReports;
        public List<StrengthTestConsolidatedReportMV> ListOfConsolidatedReports { get { return _listOfConsolidatedReports; } set { _listOfConsolidatedReports = value; base.OnPropertyChanged(); } }

        private List<MissingDrumReportModelView> _listOfMissingDrumReports;
        public List<MissingDrumReportModelView> ListOfMissingDrumReports { get { return _listOfMissingDrumReports; } set { _listOfMissingDrumReports = value; base.OnPropertyChanged(); } }

        private List<MaintenanceReportMV> _listOfMaintenanceReports;
        public List<MaintenanceReportMV> ListOfMaintenanceReports { get { return _listOfMaintenanceReports; } set { _listOfMaintenanceReports = value; base.OnPropertyChanged(); } }

        private string selectedCompanyName = null;
        private string selectedMachineCategory = null;
        private const string BLUE = "#0e0273";
        private RunConfiguration runConfiguration = new RunConfiguration();
        private List<StrengthTestSummaryModel> deleteList = null;
        private bool deleteAll = false;
        private int TOT_TEST = 0;
        private decimal CON_HANK = 0.0000m;
        private decimal CON_STD_DEV = 0.0000m;
        private decimal CON_CV = 0.0000m;
        private bool consolidatedReport = false;
        private bool drumDetailsReport = false;
        private bool maintenanceReport = false;
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
        private TestedDrumsModelView tdmv = null;

        private List<MissingDrumReportModelView> odl = new List<MissingDrumReportModelView>();
        private List<MaintenanceReportMV> oml = new List<MaintenanceReportMV>();

        public YCReport()
        {
            InitializeComponent();
        }

        public YCReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string testID, string drumNumber,string standardStrength, bool deleteRequest, bool isConsolidated, bool drumDetails, bool is_maintenance, string UFVAL1, string UFVAL2, string UFVAL3, string UFVAL4)
        {
            InitializeComponent();
            isFinalAvgRowPresent = false;
            consolidatedReport = isConsolidated;
            drumDetailsReport = drumDetails;
            maintenanceReport = is_maintenance;
            if (consolidatedReport)
            {
                //if (categoryName != null && categoryName != "")
                //{
                //    lbl_reportHeader.Text = "Con. Wrapping Report - " + categoryName;
                //}
                //else
                //{
                //    lbl_reportHeader.Text = "Con. Wrapping Report - All";
                //}
                lbl_reportHeader.Text = "Consolidated Report";
            }
            else if (drumDetailsReport)
            {
                lbl_reportHeader.Text = "Drum Details Report";
            }
            else if (maintenanceReport)
            {
                lbl_reportHeader.Text = "Maintenance Report";
            }
            else
            {
                //if (categoryName != null && categoryName != "")
                //{
                //    lbl_reportHeader.Text = "Detailed Report - " + categoryName;
                //}
                //else
                //{
                //    lbl_reportHeader.Text = "Detailed Report - All";
                //}
                lbl_reportHeader.Text = "Detailed Report";
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
            getReport(startDate, endDate, categoryName, machineID, shift, testID, drumNumber, standardStrength, deleteRequest, UFVAL1, UFVAL2, UFVAL3, UFVAL4);
        }

        private void updateDrumReportModel(String machineCat,
                                            Guid macID,
                                            String macName,
                                            int totalDrums,
                                            int totalSecs,
                                            int secNo,
                                            string totDrumNos,
                                            DateTime SSD,
                                            DateTime SED,
                                            DateTime SUD,
                                            int minDrumNo,
                                            int maxDrumNo,
                                            List<int> dl,
                                            TestConfigModel tcm,
                                            bool isLast)
        {
            try
            {
                if (tdmv == null)
                {
                    tdmv = new TestedDrumsModelView()
                    {
                        machineID = macID,
                        totalDrumCount = totalDrums,
                        totalSections = totalSecs,
                        scheduledStartDate = SSD.ToShortDateString(),
                        scheduledEndDate = SED.ToShortDateString(),
                        tcm = tcm
                    };

                    if (isLast)
                    {
                        for (int i = 1; i <= tdmv.totalSections; i++)
                        {
                            if (i!= secNo)
                            {
                                MissingDrumReportModelView mdd_temp = new MissingDrumReportModelView();

                                mdd_temp.machineID = tdmv.machineID;
                                mdd_temp.machineCategory = tdmv.tcm.machineCategory;
                                mdd_temp.machineName = tdmv.tcm.machineName;
                                mdd_temp.totalDrumCount = tdmv.totalDrumCount;
                                mdd_temp.totalSections = tdmv.totalSections;
                                mdd_temp.sectionNumber = i;
                                if (i == 1)
                                {
                                    mdd_temp.totalDrumNumbers = tdmv.tcm.drumNumbers_s1;
                                    mdd_temp.scheduledStartDate = tdmv.tcm.scheduledStartDate_s1.ToShortDateString();
                                    mdd_temp.scheduledEndDate = tdmv.tcm.scheduledEndDate_s1.ToShortDateString();

                                }
                                else if (i == 2)
                                {
                                    mdd_temp.totalDrumNumbers = tdmv.tcm.drumNumbers_s2;
                                    mdd_temp.scheduledStartDate = tdmv.tcm.scheduledStartDate_s2.ToShortDateString();
                                    mdd_temp.scheduledEndDate = tdmv.tcm.scheduledEndDate_s2.ToShortDateString();

                                }
                                else
                                {
                                    mdd_temp.totalDrumNumbers = tdmv.tcm.drumNumbers_s3;
                                    mdd_temp.scheduledStartDate = tdmv.tcm.scheduledStartDate_s3.ToShortDateString();
                                    mdd_temp.scheduledEndDate = tdmv.tcm.scheduledEndDate_s3.ToShortDateString();
                                }

                                mdd_temp.settingsUpdatedDate = tdmv.tcm.updateddate.ToShortDateString();

                                int minDrumNo_temp = 0;
                                int maxDrumNo_temp = 0;
                                if (mdd_temp.totalDrumNumbers != null)
                                {
                                    minDrumNo_temp = int.Parse(mdd_temp.totalDrumNumbers.ToString().Split('.')[0]);
                                    maxDrumNo_temp = int.Parse(mdd_temp.totalDrumNumbers.ToString().Split('.')[1]);
                                }

                                string pendingTestDrums_temp = "";
                                for (int d = minDrumNo_temp; d <= maxDrumNo_temp; d++)
                                {
                                    if (pendingTestDrums_temp == "")
                                    {
                                        pendingTestDrums_temp = d.ToString();
                                    }
                                    else
                                    {
                                        pendingTestDrums_temp = pendingTestDrums_temp + " , " + d.ToString();
                                    }

                                }

                                mdd_temp.testCompletedDrums = "";
                                mdd_temp.pendingTestDrums = pendingTestDrums_temp;
                                if (pendingTestDrums_temp != "")
                                {
                                    mdd_temp.pendingTestDrumsColor = "red";
                                    mdd_temp.pendingTestDrumsTextColor = "white";
                                }
                                else
                                {
                                    mdd_temp.pendingTestDrumsColor = "green";
                                    mdd_temp.pendingTestDrumsTextColor = "black";
                                }
                                odl.Add(mdd_temp);
                            }
                        }
                    }
                }
                else
                {
                    TestedDrumsModelView tdmv_temp = new TestedDrumsModelView()
                    {
                        machineID = macID,
                        totalDrumCount = totalDrums,
                        totalSections = totalSecs,
                        scheduledStartDate = SSD.ToShortDateString(),
                        scheduledEndDate = SED.ToShortDateString(),
                        tcm = tcm
                    };
                    if (tdmv != tdmv_temp)
                    {
                        List<MissingDrumReportModelView> temp_final = odl;
                        List<MissingDrumReportModelView> testedDrumsList = temp_final.Where(MissingDrumReportModelView =>
                                                    (MissingDrumReportModelView.machineID == tdmv.machineID
                                                    && MissingDrumReportModelView.totalDrumCount == tdmv.totalDrumCount
                                                    && MissingDrumReportModelView.totalSections == tdmv.totalSections
                                                    && MissingDrumReportModelView.scheduledStartDate == tdmv.scheduledStartDate
                                                    && MissingDrumReportModelView.scheduledEndDate == tdmv.scheduledEndDate))
                                                    .ToList();
                        List<int> testedSections = new List<int>();
                        if (testedDrumsList.Count > 0)
                        {
                            foreach(MissingDrumReportModelView t in testedDrumsList)
                            {
                                testedSections.Add(t.sectionNumber);
                            }
                        }
                        if (testedSections.Count > 0 && testedSections.Count < tdmv.totalSections)
                        {
                            for(int i = 1; i <= tdmv.totalSections; i++)
                            {
                                if (!testedSections.Contains(i))
                                {
                                    MissingDrumReportModelView mdd_temp = new MissingDrumReportModelView();

                                    mdd_temp.machineID = tdmv.machineID;
                                    mdd_temp.machineCategory = tdmv.tcm.machineCategory;
                                    mdd_temp.machineName = tdmv.tcm.machineName;
                                    mdd_temp.totalDrumCount = tdmv.totalDrumCount;
                                    mdd_temp.totalSections = tdmv.totalSections;
                                    mdd_temp.sectionNumber = i;
                                    if (i == 1)
                                    {
                                        mdd_temp.totalDrumNumbers = tdmv.tcm.drumNumbers_s1;
                                        mdd_temp.scheduledStartDate = tdmv.tcm.scheduledStartDate_s1.ToShortDateString();
                                        mdd_temp.scheduledEndDate = tdmv.tcm.scheduledEndDate_s1.ToShortDateString();
                                        
                                    }
                                    else if (i == 2)
                                    {
                                        mdd_temp.totalDrumNumbers = tdmv.tcm.drumNumbers_s2;
                                        mdd_temp.scheduledStartDate = tdmv.tcm.scheduledStartDate_s2.ToShortDateString();
                                        mdd_temp.scheduledEndDate = tdmv.tcm.scheduledEndDate_s2.ToShortDateString();
                                        
                                    }
                                    else
                                    {
                                        mdd_temp.totalDrumNumbers = tdmv.tcm.drumNumbers_s3;
                                        mdd_temp.scheduledStartDate = tdmv.tcm.scheduledStartDate_s3.ToShortDateString();
                                        mdd_temp.scheduledEndDate = tdmv.tcm.scheduledEndDate_s3.ToShortDateString();
                                    }

                                    mdd_temp.settingsUpdatedDate = tdmv.tcm.updateddate.ToShortDateString();

                                    int minDrumNo_temp = 0;
                                    int maxDrumNo_temp = 0;
                                    if (mdd_temp.totalDrumNumbers != null)
                                    {
                                        minDrumNo_temp = int.Parse(mdd_temp.totalDrumNumbers.ToString().Split('.')[0]);
                                        maxDrumNo_temp = int.Parse(mdd_temp.totalDrumNumbers.ToString().Split('.')[1]);
                                    }

                                    string pendingTestDrums_temp = "";
                                    for (int d = minDrumNo_temp; d <= maxDrumNo_temp; d++)
                                    {
                                        if (pendingTestDrums_temp == "")
                                        {
                                            pendingTestDrums_temp = d.ToString();
                                        }
                                        else
                                        {
                                            pendingTestDrums_temp = pendingTestDrums_temp + " , " + d.ToString();
                                        }
                                           
                                    }

                                    mdd_temp.testCompletedDrums = "";
                                    mdd_temp.pendingTestDrums = pendingTestDrums_temp;
                                    if (pendingTestDrums_temp != "")
                                    {
                                        mdd_temp.pendingTestDrumsColor = "red";
                                        mdd_temp.pendingTestDrumsTextColor = "white";
                                    }
                                    else
                                    {
                                        mdd_temp.pendingTestDrumsColor = "green";
                                        mdd_temp.pendingTestDrumsTextColor = "black";
                                    }
                                    odl.Add(mdd_temp);
                                }
                            }
                        }
                    }
                    tdmv = tdmv_temp;
                }

                MissingDrumReportModelView mdd = new MissingDrumReportModelView();

                mdd.machineID = macID;
                mdd.machineCategory = machineCat;
                mdd.machineName = macName;
                mdd.totalDrumCount = totalDrums;
                mdd.totalSections = totalSecs;
                mdd.sectionNumber = secNo;
                mdd.totalDrumNumbers = totDrumNos;
                mdd.scheduledStartDate = SSD.ToShortDateString();
                mdd.scheduledEndDate = SED.ToShortDateString();
                mdd.settingsUpdatedDate = SUD.ToShortDateString();

                string testCompletedDrums = "";
                string pendingTestDrums = "";
                for (int i = minDrumNo; i <= maxDrumNo; i++)
                {
                    if (!dl.Contains(i))
                    {
                        if (testCompletedDrums == "") { testCompletedDrums = i.ToString(); }
                        else
                        {
                            testCompletedDrums = testCompletedDrums + " , " + i.ToString();
                        }
                    }
                    else
                    {
                        if (pendingTestDrums == "") { pendingTestDrums = i.ToString(); }
                        else
                        {
                            pendingTestDrums = pendingTestDrums + " , " + i.ToString();
                        }
                    }
                }

                mdd.testCompletedDrums = testCompletedDrums;
                mdd.pendingTestDrums = pendingTestDrums;
                if (pendingTestDrums != "")
                {
                    mdd.pendingTestDrumsColor = "red";
                    mdd.pendingTestDrumsTextColor = "white";
                }
                else
                {
                    mdd.pendingTestDrumsColor = "green";
                    mdd.pendingTestDrumsTextColor = "black";
                }
                odl.Add(mdd);

                

            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!! Error:" + ex.Message.ToString(), "OK");
            }
        }

        private void getDrumReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<StrengthTestModel>();
                    conn.CreateTable<StrengthTestSummaryModel>();
                    conn.CreateTable<TestConfigModel>();
                    conn.CreateTable<ConfigModel>();

                    List<StrengthTestSummaryModel> strengthTestSummaryList =
                        conn.Table<StrengthTestSummaryModel>().Where(StrengthTestSummaryModel =>
                         ((StrengthTestSummaryModel.scheduledStartDate <= startDate
                         || StrengthTestSummaryModel.scheduledEndDate >= endDate)
                         && StrengthTestSummaryModel.machineCategory == categoryName
                         && StrengthTestSummaryModel.machineID == machineID))
                        .OrderBy(StrengthTestSummaryModel => StrengthTestSummaryModel.machineID)
                        .ThenBy(StrengthTestSummaryModel => StrengthTestSummaryModel.sectionNumber)
                        .ThenBy(StrengthTestSummaryModel => StrengthTestSummaryModel.totalDrumNumbers)
                        .ThenBy(StrengthTestSummaryModel => StrengthTestSummaryModel.scheduledStartDate)
                        .ThenBy(StrengthTestSummaryModel => StrengthTestSummaryModel.scheduledEndDate)
                        .ToList();

                    if (strengthTestSummaryList.Count == 0)
                    {
                        DisplayAlert("Notice", "No records to display!!!", "OK");
                        return;
                    }
                    

                    Guid prev_machineID = Guid.Empty;
                    string prev_MachineCategory = null;
                    string prev_MachineName = null;
                    int prev_totalDrumCount = 0;
                    int prev_totalSections = 0;
                    int prev_SectionNumber = 0;
                    string prev_TotalDrumNumbers = null;
                    DateTime prev_SSD = DEFAULTDATE;
                    DateTime prev_SED = DEFAULTDATE;
                    DateTime prev_SUD = DEFAULTDATE;
                    int prev_MinDrumNo = 0;
                    int prev_MaxDrumNo = 0;
                    List<int> drumList = null;
                    TestConfigModel prev_tcm = null;

                    foreach (StrengthTestSummaryModel S_Test in strengthTestSummaryList)
                    {
                        prev_tcm = conn.Table<TestConfigModel>().Where(TestConfigModel =>
                                                (TestConfigModel.testID == S_Test.testID)).FirstOrDefault();

                        if (prev_tcm == null)
                        {
                            DisplayAlert("Attention", "Error Occurred-Unable get detailed drum report", "OK");
                            return;
                        }
                        else
                        {
                            prev_totalDrumCount = prev_tcm.totalDrumCount;
                            prev_totalSections = prev_tcm.totalSections;
                        }

                        int minDrumNo = 0;
                        int maxDrumNo = 0;
                        if (S_Test.totalDrumNumbers != null)
                        {
                            minDrumNo = int.Parse(S_Test.totalDrumNumbers.ToString().Split('.')[0]);
                            maxDrumNo = int.Parse(S_Test.totalDrumNumbers.ToString().Split('.')[1]);
                        }
                        if(prev_MachineName ==null && prev_SectionNumber==0 && prev_TotalDrumNumbers == null
                            && prev_SSD == DEFAULTDATE && prev_SED == DEFAULTDATE)
                        {
                            drumList = new List<int>();
                            prev_MachineCategory = S_Test.machineCategory;
                            prev_machineID = S_Test.machineID;
                            prev_MachineName = S_Test.machineName;
                            prev_SectionNumber = S_Test.sectionNumber;
                            prev_TotalDrumNumbers = S_Test.totalDrumNumbers;
                            prev_SSD = S_Test.scheduledStartDate;
                            prev_SED = S_Test.scheduledEndDate;
                            prev_SUD = S_Test.settingsUpdatedDate;
                            prev_MinDrumNo = minDrumNo;
                            prev_MaxDrumNo = maxDrumNo;


                            for (int i= minDrumNo; i <= maxDrumNo; i++)
                            {
                                drumList.Add(i);
                            }
                            drumList.Remove(S_Test.drumNumber);
                        }
                        else
                        {
                            if(prev_MachineName == S_Test.machineName
                                && prev_SectionNumber == S_Test.sectionNumber
                                && prev_TotalDrumNumbers == S_Test.totalDrumNumbers
                                && prev_SSD == S_Test.scheduledStartDate
                                && prev_SED == S_Test.scheduledEndDate)
                            {
                                drumList.Remove(S_Test.drumNumber);
                            }
                            else
                            {
                                updateDrumReportModel(prev_MachineCategory,
                                                        prev_machineID,
                                                        prev_MachineName,
                                                        prev_totalDrumCount,
                                                        prev_totalSections,
                                                        prev_SectionNumber,
                                                        prev_TotalDrumNumbers,
                                                        prev_SSD,
                                                        prev_SED,
                                                        prev_SUD,
                                                        prev_MinDrumNo,
                                                        prev_MaxDrumNo,
                                                        drumList,
                                                        prev_tcm,
                                                        false);
                                drumList = new List<int>();
                                prev_MachineCategory = S_Test.machineCategory;
                                prev_machineID = S_Test.machineID;
                                prev_MachineName = S_Test.machineName;
                                prev_SectionNumber = S_Test.sectionNumber;
                                prev_TotalDrumNumbers = S_Test.totalDrumNumbers;
                                prev_SSD = S_Test.scheduledStartDate;
                                prev_SED = S_Test.scheduledEndDate;
                                prev_SUD = S_Test.settingsUpdatedDate;
                                prev_MinDrumNo = minDrumNo;
                                prev_MaxDrumNo = maxDrumNo;
                                for (int i = minDrumNo; i <= maxDrumNo; i++)
                                {
                                    drumList.Add(i);
                                }
                                drumList.Remove(S_Test.drumNumber);
                            }
                        }
                    }

                    if (drumList != null)
                    {
                        updateDrumReportModel(prev_MachineCategory,
                                                            prev_machineID,
                                                            prev_MachineName,
                                                            prev_totalDrumCount,
                                                            prev_totalSections,
                                                            prev_SectionNumber,
                                                            prev_TotalDrumNumbers,
                                                            prev_SSD,
                                                            prev_SED,
                                                            prev_SUD,
                                                            prev_MinDrumNo,
                                                            prev_MaxDrumNo,
                                                            drumList,
                                                            prev_tcm,
                                                            true);
                    }
                    odl = odl.OrderBy(MissingDrumReportModelView => MissingDrumReportModelView.machineName)
                            .ThenBy(MissingDrumReportModelView => MissingDrumReportModelView.sectionNumber).ToList();
                    ListOfMissingDrumReports = odl;
                    listview_tcreport_missingDrum.IsVisible = true;
                    listview_tcreport_missingDrum.ItemsSource = null;
                    listview_tcreport_missingDrum.ItemsSource = ListOfMissingDrumReports;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!! Error:" + ex.Message.ToString(), "OK");
            }
        }


        private void getMaintenanceReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    List<StrengthTestSummaryModel> testSummary = conn.Table<StrengthTestSummaryModel>()
                                        .Where(StrengthTestSummaryModel =>
                                        ((StrengthTestSummaryModel.createdate >= startDate
                                        || StrengthTestSummaryModel.createdate <= endDate)
                                        && StrengthTestSummaryModel.machineCategory == categoryName
                                        && StrengthTestSummaryModel.machineID == machineID))
                                        .OrderBy(StrengthTestSummaryModel=>StrengthTestSummaryModel.machineID)
                                        .ThenBy(StrengthTestSummaryModel => StrengthTestSummaryModel.machineName)
                                        .ToList();
                    if (testSummary.Count==0)
                    {
                        DisplayAlert("Notice", "No records to display!!!", "OK");
                        return;
                    }
                    int sn = 1;

                    Guid prev_macID = Guid.Empty;
                    string prev_macName = "";
                    int prev_speed = 0;
                    decimal prev_p1 = 0.0m;
                    decimal prev_p2 = 0.0m;
                    decimal prev_n1 = 0.0m;

                    foreach( StrengthTestSummaryModel st in testSummary)
                    {
                        if(prev_macID==st.machineID
                            && prev_macName==st.machineName
                            && prev_speed==st.speed
                            && prev_p1==st.p1
                            && prev_p2==st.p2
                            && prev_n1 == st.n1)
                        {
                            continue;
                        }
                        else
                        {
                            prev_macID = st.machineID;
                            prev_macName = st.machineName;
                            prev_speed = st.speed;
                            prev_p1 = st.p1;
                            prev_p2 = st.p2;
                            prev_n1 = st.n1;
                        }

                        TestConfigModel st_config = conn.Table<TestConfigModel>().Where(TestConfigModel =>
                                                    (TestConfigModel.testID == st.testID)).FirstOrDefault();
                        if (st_config == null)
                        {
                            DisplayAlert("Notice", "Error Occurred while preparing report", "OK");
                            return;
                        }

                        string p1_bgColor = "green";
                        string p1_textColor = "black";

                        decimal p1_max = st_config.p1 + st_config.p1Deviation;
                        decimal p1_min = st_config.p1 - st_config.p1Deviation;

                        if (st.p1 < p1_min || st.p1 > p1_max)
                        {
                            p1_bgColor = "red";
                            p1_textColor = "white";
                        }

                        string p2_bgColor = "green";
                        string p2_textColor = "black";

                        decimal p2_max = st_config.p2 + st_config.p2Deviation;
                        decimal p2_min = st_config.p2 - st_config.p2Deviation;

                        if (st.p2 < p2_min || st.p2 > p2_max)
                        {
                            p2_bgColor = "red";
                            p2_textColor = "white";
                        }

                        string n1_bgColor = "green";
                        string n1_textColor = "black";

                        decimal n1_max = st_config.n1 + st_config.n1Deviation;
                        decimal n1_min = st_config.n1 - st_config.n1Deviation;

                        if (st.n1 < n1_min || st.n1 > n1_max)
                        {
                            n1_bgColor = "red";
                            n1_textColor = "white";
                        }

                        MaintenanceReportMV m = new MaintenanceReportMV()
                        {
                            serialNo = sn.ToString(),
                            date = st.createdate.ToString(),
                            machineName = st.machineName,
                            speed = st.speed.ToString(),
                            p1 = st.p1.ToString(),
                            p1_bgcolor = p1_bgColor,
                            p1_textcolor = p1_textColor,
                            p2 = st.p2.ToString(),
                            p2_bgcolor = p2_bgColor,
                            p2_textcolor = p2_textColor,
                            n1 = st.n1.ToString(),
                            n1_bgcolor = n1_bgColor,
                            n1_textcolor = n1_textColor,
                            remark = ""
                        };
                        oml.Add(m);
                        sn++;

                    }
                    ListOfMaintenanceReports = oml;
                    listview_tcreport_maintenance.IsVisible = true;
                    listview_tcreport_maintenance.ItemsSource = null;
                    listview_tcreport_maintenance.ItemsSource = ListOfMaintenanceReports;
                }
            }
            catch(Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!! Error:" + ex.Message.ToString(), "OK");
            }
        }


        private void getReport(DateTime startDate, DateTime endDate, string categoryName, Guid machineID, string shift, string testID, string drumNumber, string standardStrength, bool deleteRequest, string UFVAL1, string UFVAL2, string UFVAL3, string UFVAL4)
        {
            try
            {
               

                if (drumDetailsReport)
                {
                    getDrumReport( startDate,  endDate,  categoryName,  machineID);
                    return;
                }

                if (maintenanceReport)
                {
                    getMaintenanceReport(startDate, endDate, categoryName, machineID);
                    return;
                }

                //decimal stdHank = 0.000m;
                List<OverallReportModelView> OVS = new List<OverallReportModelView>();
                List<StrengthTestConsolidatedReportMV> OverallConsolidatedReports = new List<StrengthTestConsolidatedReportMV>();
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

                    conn.CreateTable<StrengthTestModel>();
                    conn.CreateTable<StrengthTestSummaryModel>();
                    conn.CreateTable<TestConfigModel>();
                    conn.CreateTable<ConfigModel>();

                    List<StrengthTestSummaryModel> ycTestSummaryModels = null;
                    if (testID != "")
                    {
                        if (!testID.Contains("."))
                        {
                            long givenTestId = long.Parse(testID);
                            ycTestSummaryModels = conn.Table<StrengthTestSummaryModel>().Where(StrengthTestSummaryModel =>
                                                  StrengthTestSummaryModel.testID == givenTestId).ToList();
                        }
                        else
                        {
                            long startTestID = long.Parse(testID.Split('.')[0]);
                            long endTestID = long.Parse(testID.Split('.')[1]);
                            
                            if (startTestID == endTestID)
                            {
                                ycTestSummaryModels = conn.Table<StrengthTestSummaryModel>().Where(StrengthTestSummaryModel =>
                                                 StrengthTestSummaryModel.testID == startTestID).ToList();
                            }
                            else
                            {
                                for(long i = startTestID; i <= endTestID; i++)
                                {
                                    if (ycTestSummaryModels == null)
                                    {
                                        ycTestSummaryModels = conn.Table<StrengthTestSummaryModel>().Where(StrengthTestSummaryModel =>
                                                     StrengthTestSummaryModel.testID == i).ToList();
                                    }
                                    else
                                    {
                                        ycTestSummaryModels.AddRange(conn.Table<StrengthTestSummaryModel>().Where(StrengthTestSummaryModel =>
                                                        StrengthTestSummaryModel.testID == i).ToList());
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
                        List<StrengthTestSummaryModel> parentList = conn.Table<StrengthTestSummaryModel>().Where(StrengthTestSummaryModel =>
                                                (StrengthTestSummaryModel.createdate >= startDate
                                                && StrengthTestSummaryModel.createdate < endDate))
                                                .OrderBy(StrengthTestSummaryModel => StrengthTestSummaryModel.createdate).ToList();
                        if (parentList.Count == 0)
                        {
                            DisplayAlert("Notice", "No records to display!!!", "OK");
                            return;
                        }

                        //Start of Logic to check last shift for the given end date is logged in end date + 1 day date
                        //Get first record of actual end date + 1 day
                        List<StrengthTestSummaryModel> recs_actualEndDatePlusOne = conn.Table<StrengthTestSummaryModel>()
                                                                        .Where(StrengthTestSummaryModel =>
                                                                        (StrengthTestSummaryModel.createdate >= endDate
                                                                        && StrengthTestSummaryModel.createdate < endDatePlusOne))
                                                                        .OrderBy(StrengthTestSummaryModel => StrengthTestSummaryModel.createdate).ToList();
                        if (recs_actualEndDatePlusOne.Count > 0) { 
                            //Check if the 1st record of actual end date + 1 day is not Shift-1
                            if (recs_actualEndDatePlusOne[0].shift != "Shift-1")
                            {
                                StrengthTestSummaryModel actualEndDatePlusOne_Shift1_Recs = recs_actualEndDatePlusOne.Where(StrengthTestSummaryModel =>
                                                                                             (StrengthTestSummaryModel.shift != recs_actualEndDatePlusOne[0].shift))
                                                                                            .OrderBy(StrengthTestSummaryModel => StrengthTestSummaryModel.createdate)
                                                                                            .FirstOrDefault();

                                //Merge last shift record of actual end date from (actual end date + 1day) with parent list
                                if (actualEndDatePlusOne_Shift1_Recs != null)
                                {
                                    List<StrengthTestSummaryModel> tempSummaryList = recs_actualEndDatePlusOne.Where(StrengthTestSummaryModel =>
                                                                                (StrengthTestSummaryModel.createdate >= endDate
                                                                                && StrengthTestSummaryModel.createdate < actualEndDatePlusOne_Shift1_Recs.createdate
                                                                                && StrengthTestSummaryModel.shift == recs_actualEndDatePlusOne[0].shift))
                                                                                .OrderBy(StrengthTestSummaryModel => StrengthTestSummaryModel.createdate).ToList();
                                    parentList.Concat(tempSummaryList);
                                }
                            }
                        }
                        //End of Logic to check last shift for the given end date is logged in end date + 1 day date

                        //Start of logic to ignore the previous date last shift record from the given actual start date
                        if (parentList[0].shift != "Shift-1")
                        {
                            StrengthTestSummaryModel actualStartDate_Shift1_Recs = parentList.Where(StrengthTestSummaryModel =>
                                                                                         (StrengthTestSummaryModel.shift == "Shift-1"))
                                                                                        .OrderBy(StrengthTestSummaryModel => StrengthTestSummaryModel.createdate)
                                                                                        .FirstOrDefault();
                            if (actualStartDate_Shift1_Recs != null)
                            {
                                List<StrengthTestSummaryModel> lastShiftOfPreviousDay_in_ActualStartDateRecs =
                                                                        parentList.Where(StrengthTestSummaryModel =>
                                                                        (StrengthTestSummaryModel.shift == parentList[0].shift
                                                                        && StrengthTestSummaryModel.createdate < actualStartDate_Shift1_Recs.createdate))
                                                                        .OrderBy(StrengthTestSummaryModel => StrengthTestSummaryModel.createdate)
                                                                        .ToList();
                                parentList.RemoveAll(i => lastShiftOfPreviousDay_in_ActualStartDateRecs.Contains(i));
                            }
                        }
                        //End of logic to ignore the previous date last shift record from the given actual start date




                        if (categoryName == null || categoryName == "")
                        {
                            DisplayAlert("Notice", "Machine category is blank!!!", "OK");
                            return;
                        }
                        else if (categoryName != null && machineID == Guid.Empty)
                        {
                            DisplayAlert("Notice", "Machine name is blank!!!", "OK");
                            return;
                        }

                        if (shift != null && shift != "")
                        {
                            parentList = parentList.Where(StrengthTestSummaryModel =>
                                         (StrengthTestSummaryModel.shift == shift))
                                        .ToList();
                            if (parentList.Count == 0)
                            {
                                DisplayAlert("Notice", "No records to display!!!", "OK");
                                return;
                            }
                        }

                        if (drumNumber!=null && drumNumber != "")
                        {
                            int givenDrumNumber = int.Parse(drumNumber);
                            parentList = parentList.Where(StrengthTestSummaryModel =>
                                         (StrengthTestSummaryModel.drumNumber == givenDrumNumber))
                                        .ToList();
                            if (parentList.Count == 0)
                            {
                                DisplayAlert("Notice", "No records to display!!!", "OK");
                                return;
                            }
                        }

                        if (standardStrength != null && standardStrength != "")
                        {
                            decimal givenStandardStrength = decimal.Parse(standardStrength);
                            parentList = parentList.Where(StrengthTestSummaryModel =>
                                         (StrengthTestSummaryModel.standardStrength == givenStandardStrength))
                                        .ToList();
                            if (parentList.Count == 0)
                            {
                                DisplayAlert("Notice", "No records to display!!!", "OK");
                                return;
                            }
                        }

                        ycTestSummaryModels = parentList;

                    }



                    if (ycTestSummaryModels.Count == 0)
                    {
                        DisplayAlert("Notice", "No records to display!!!", "OK");
                        return;
                    }
                    else if (testID=="" || testID==null)
                    {
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

                    TOT_TEST = ycTestSummaryModels.Count;
                    int counter = 0;
                    foreach (StrengthTestSummaryModel testsummary in ycTestSummaryModels)
                    {
                        OverallReportModelView report = new OverallReportModelView();
                        StrengthTestConsolidatedReportMV consolItems = new StrengthTestConsolidatedReportMV();
                        List<StrengthTestModel> strengthtestlist = conn.Table<StrengthTestModel>().Where(StrengthTestModel => StrengthTestModel.testID == testsummary.testID).ToList();
                        if (strengthtestlist != null)
                        {
                            if (consolidatedReport)
                            {
                                if (counter == 0)
                                {
                                    if (UFVAL1 != "" && UFVAL1 != null)
                                    {
                                        ConfigModel ycConfig_uf = conn.Table<ConfigModel>().
                                                                Where(ConfigModel => (ConfigModel.uf_name_1 != null ||
                                                                ConfigModel.uf_name_1 != "")
                                                                && ConfigModel.machineCategory == testsummary.machineCategory
                                                                && ConfigModel.machineID == testsummary.machineID
                                                                && ConfigModel.machineName == testsummary.machineName).FirstOrDefault();
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
                                        ConfigModel ycConfig_uf = conn.Table<ConfigModel>().
                                                                Where(ConfigModel => (ConfigModel.uf_name_2 != null ||
                                                                ConfigModel.uf_name_2 != "")
                                                                && ConfigModel.machineCategory == testsummary.machineCategory
                                                                && ConfigModel.machineID == testsummary.machineID
                                                                && ConfigModel.machineName == testsummary.machineName).FirstOrDefault();
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
                                        ConfigModel ycConfig_uf = conn.Table<ConfigModel>().
                                                                Where(ConfigModel => (ConfigModel.uf_name_3 != null ||
                                                                ConfigModel.uf_name_3 != "")
                                                                && ConfigModel.machineCategory == testsummary.machineCategory
                                                                && ConfigModel.machineID == testsummary.machineID
                                                                && ConfigModel.machineName == testsummary.machineName).FirstOrDefault();
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
                                        ConfigModel ycConfig_uf = conn.Table<ConfigModel>().
                                                                Where(ConfigModel => (ConfigModel.uf_name_4 != null ||
                                                                ConfigModel.uf_name_4 != "")
                                                                && ConfigModel.machineCategory == testsummary.machineCategory
                                                                && ConfigModel.machineID == testsummary.machineID
                                                                && ConfigModel.machineName == testsummary.machineName).FirstOrDefault();
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




                                decimal maxRangeVal = testsummary.standardStrength + testsummary.strengthDeviation;
                                decimal minRangeVal = testsummary.standardStrength - testsummary.strengthDeviation;


                                if (testsummary.yarnStrength < minRangeVal || testsummary.yarnStrength > maxRangeVal)
                                {
                                    consolItems.strengthColor = "red";
                                    consolItems.strengthColor_text = "white";
                                }
                                else
                                {
                                    consolItems.strengthColor = "green";
                                    consolItems.strengthColor_text = "black";
                                }

                                //consolItems.serialNo = (counter + 1).ToString();
                                consolItems.serialNo = "SP-"+ testsummary.speed.ToString()
                                                        + "\n" + "P1-" + testsummary.p1.ToString()
                                                        + "\n" + "P2 - " + testsummary.p2.ToString()
                                                        + "\n" + "N1 - " + testsummary.n1.ToString();
                                if (testsummary.drumSelectionMethod.ToString() == "Random")
                                {
                                    consolItems.testID = testsummary.testID.ToString() + " (R)";
                                    consolItems.testIDColor = "red";
                                    consolItems.testIDColor_text = "white";
                                }
                                else
                                {
                                    consolItems.testID = testsummary.testID.ToString();
                                    consolItems.testIDColor = "green";
                                    consolItems.testIDColor_text = "black";
                                }
                                consolItems.machineName = testsummary.machineName;
                                consolItems.testDate = testsummary.createdate.Day.ToString() + "-" + testsummary.createdate.Month.ToString() + "-" + testsummary.createdate.Year.ToString();
                                consolItems.shift = testsummary.shift;
                                consolItems.drumNumber = testsummary.drumNumber.ToString();
                                consolItems.drumSelectionMethod = testsummary.drumSelectionMethod.ToString();
                                consolItems.belowLimit = testsummary.belowLimit.ToString();
                                consolItems.totalTestCount = testsummary.totalTestCount.ToString();
                                consolItems.qualifiedTestCount = testsummary.qualifiedTestCount.ToString();
                                consolItems.standardValue = testsummary.standardStrength.ToString();
                                consolItems.deviation = "\u00B1" + testsummary.strengthDeviation.ToString();
                                consolItems.actualStrength = testsummary.yarnStrength.ToString();
                                consolItems.strength = testsummary.yarnStrength.ToString() + " \n"+ testsummary.standardStrength.ToString() + " " + "\u00B1" + testsummary.strengthDeviation.ToString();
                                consolItems.testDuration = formatTime(testsummary.createdate);
                                consolItems.remarks = testsummary.testRemark;

                               


                                counter++;


                            }
                            else
                            {

                                foreach (StrengthTestModel test in strengthtestlist)
                                {
                                    report.Add(test);
                                }
                                report.testID = testsummary.testID;
                                report.userName = testsummary.userName;
                                report.machineCategory = testsummary.machineCategory;
                                report.machineName = testsummary.machineName;
                                report.drumNumber = testsummary.drumNumber;
                                report.standardStrength = testsummary.standardStrength;
                                report.strengthDeviation = testsummary.strengthDeviation;
                                report.belowLimit = testsummary.belowLimit;
                                report.maxRollingCount = testsummary.maxRollingCount;
                                report.totalTestCount = testsummary.totalTestCount;
                                report.drumSelectionMethod = testsummary.drumSelectionMethod;
                                report.yarnStrength = testsummary.yarnStrength;
                                report.scheduledStartDate = testsummary.scheduledStartDate.ToShortDateString();
                                report.scheduledEndDate = testsummary.scheduledEndDate.ToShortDateString();
                                report.settingsUpdatedDate = testsummary.settingsUpdatedDate;
                                report.shift = testsummary.shift;
                                report.createdate = testsummary.createdate;
                                report.testRemark = testsummary.testRemark;
                                report.testDuration = testsummary.testDuration;
                                report.isIndividualReport= true;
                                report.isConsolidatedReport = false;
                                report.deviationPercent = "\u00B1" + testsummary.strengthDeviation;
                               

                                if (testsummary.uf_value_1 != null && testsummary.uf_value_1 != "")
                                {
                                    ConfigModel ycConfig_uf = conn.Table<ConfigModel>().
                                                            Where(ConfigModel => (ConfigModel.uf_name_1 != null ||
                                                            ConfigModel.uf_name_1 != "")
                                                            && ConfigModel.machineCategory == testsummary.machineCategory
                                                            && ConfigModel.machineID == testsummary.machineID
                                                            && ConfigModel.machineName == testsummary.machineName).FirstOrDefault();
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
                                    ConfigModel ycConfig_uf = conn.Table<ConfigModel>().
                                                            Where(ConfigModel =>
                                                            (ConfigModel.uf_name_2 != null ||
                                                            ConfigModel.uf_name_2 != "")
                                                            && ConfigModel.machineCategory == testsummary.machineCategory
                                                            && ConfigModel.machineID == testsummary.machineID
                                                            && ConfigModel.machineName == testsummary.machineName).FirstOrDefault();
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
                                    ConfigModel ycConfig_uf = conn.Table<ConfigModel>().
                                                                Where(ConfigModel =>
                                                                (ConfigModel.uf_name_3 != null ||
                                                                ConfigModel.uf_name_3 != "")
                                                                && ConfigModel.machineCategory == testsummary.machineCategory
                                                                && ConfigModel.machineID == testsummary.machineID
                                                                && ConfigModel.machineName == testsummary.machineName).FirstOrDefault();
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
                                    ConfigModel ycConfig_uf = conn.Table<ConfigModel>().
                                                            Where(ConfigModel =>
                                                            (ConfigModel.uf_name_4 != null ||
                                                            ConfigModel.uf_name_4 != "")
                                                            && ConfigModel.machineCategory == testsummary.machineCategory
                                                            && ConfigModel.machineID == testsummary.machineID
                                                            && ConfigModel.machineName == testsummary.machineName).FirstOrDefault();
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

                                decimal maxRangeVal = testsummary.standardStrength + testsummary.strengthDeviation;
                                decimal minRangeVal = testsummary.standardStrength - testsummary.strengthDeviation;


                                if (testsummary.yarnStrength < minRangeVal || testsummary.yarnStrength > maxRangeVal)
                                {
                                    report.testResultColor = "red";
                                }
                                else
                                {
                                    report.testResultColor = "white";
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
                        //StrengthTestConsolidatedReportMV consolItems = new StrengthTestConsolidatedReportMV()
                        //{
                        //    serialNo = "Average",
                        //    testID = "",
                        //    machineName = "",
                        //    testDate = "",
                        //    drumNumber = "",
                        //    drumSelectionMethod = "",
                        //    belowLimit = "",
                        //    totalTestCount = "",
                        //    qualifiedTestCount = "",
                        //    standardValue = "",
                        //    actualStrength = "",
                        //    testDuration = "",
                        //    remarks = "",
                        //    isWhite = true,
                        //    isRed = false
                        //};


                        //if (machineID != Guid.Empty)
                        //{
                        //    isFinalAvgRowPresent = true;
                        //    OverallConsolidatedReports.Add(consolItems);
                        //}

                        OverallConsolidatedReports = OverallConsolidatedReports
                                                    .OrderBy(StrengthTestConsolidatedReportMV => StrengthTestConsolidatedReportMV.machineName)
                                                    .ThenBy(StrengthTestConsolidatedReportMV => StrengthTestConsolidatedReportMV.drumNumber)
                                                    .ToList();
                        ListOfConsolidatedReports = OverallConsolidatedReports;
                    }
                    else
                    {
                        ListOfReport = OVS;
                    }

                }
                listview_tcreport.ItemsSource = null;
                listview_tcConsolidatedReport.ItemsSource = null;
                if (consolidatedReport)
                {
                    
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

        private void deleteRecords(List<StrengthTestSummaryModel> lstOfRecs)
        {
            if (lstOfRecs.Count == 0)
            {
                return;
            }
            foreach (StrengthTestSummaryModel rec in lstOfRecs)
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.Table<StrengthTestSummaryModel>().
                                           Where(StrengthTestSummaryModel =>
                                           StrengthTestSummaryModel.testID == rec.testID).Delete();
                    conn.Table<StrengthTestModel>().
                                        Where(StrengthTestModel =>
                                        StrengthTestModel.testID == rec.testID).Delete();

                    conn.Table<TestConfigModel>().
                                        Where(TestConfigModel =>
                                        TestConfigModel.testID == rec.testID).Delete();
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
            if (listview_tcreport.ItemsSource == null && listview_tcConsolidatedReport.ItemsSource == null && listview_tcreport_missingDrum ==null)
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

                    List<StrengthTestModel> testList = orl.strengthtestlist;

                    //if (tableNo == int.Parse(entry_reportNo.Text.Trim())) break;
                    PdfGrid pdfGridInfo = new PdfGrid();
                    pdfGridInfo.RepeatHeader = true;
                    pdfGridInfo.Columns.Add(6);
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

                        //pdfGridInfo.Rows[0].Cells[0].Value = "Total Test: " + TOT_TEST;
                        //pdfGridInfo.Rows[0].Cells[1].Value = "Con. HANK: " + CON_HANK;
                        //pdfGridInfo.Rows[0].Cells[2].Value = "Con. SD: " + CON_STD_DEV;
                        //pdfGridInfo.Rows[0].Cells[3].Value = "Con. CV: " + CON_CV;

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
                    pdfGridInfo.Rows[1].Cells[0].Value = "Test ID: " + orl.testID.ToString();
                    pdfGridInfo.Rows[1].Cells[0].ColumnSpan = 2;
                    pdfGridInfo.Rows[1].Cells[2].Value = "Shift: " + orl.shift;
                    pdfGridInfo.Rows[1].Cells[2].ColumnSpan = 1;
                    pdfGridInfo.Rows[1].Cells[3].Value = "Tester: " + orl.userName;
                    pdfGridInfo.Rows[1].Cells[3].ColumnSpan = 3;

                    pdfGridInfo.Rows[2].Cells[0].Value = "Machine Category: " + orl.machineCategory;
                    pdfGridInfo.Rows[2].Cells[0].ColumnSpan = 2;
                    pdfGridInfo.Rows[2].Cells[2].Value = "Machine Name: " + orl.machineName;
                    pdfGridInfo.Rows[2].Cells[2].ColumnSpan = 2;
                    pdfGridInfo.Rows[2].Cells[4].Value = "Drum Number: " + orl.drumNumber.ToString();

                    pdfGridInfo.Rows[3].Cells[0].Value = "Sch. Start Date: " + orl.scheduledStartDate;
                    pdfGridInfo.Rows[3].Cells[0].ColumnSpan = 2;
                    pdfGridInfo.Rows[3].Cells[2].Value = "Sch. End Date: " + orl.scheduledEndDate;
                    pdfGridInfo.Rows[3].Cells[2].ColumnSpan = 2;
                    pdfGridInfo.Rows[3].Cells[4].Value = "Act. Test Date: " + orl.createdate.ToString();
                    pdfGridInfo.Rows[3].Cells[4].ColumnSpan = 2;

                    pdfGridInfo.Rows[4].Cells[0].Value = "Yarn Strength: " + orl.yarnStrength.ToString() + " [STD: "+ orl.standardStrength.ToString()+" "+orl.deviationPercent.ToString()+"]" ;
                    pdfGridInfo.Rows[4].Cells[0].ColumnSpan = 2;

                    if (orl.testResultColor == "red")
                    {
                        pdfGridInfo.Rows[4].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGridInfo.Rows[4].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGridInfo.Rows[4].Cells[0].Style.BackgroundBrush = PdfBrushes.Red;
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                        pdfGridInfo.Rows[4].Cells[0].Style.TextBrush = brush_con;
                    }

                    pdfGridInfo.Rows[4].Cells[2].Value = "Drum Selection Method: " + orl.drumSelectionMethod;
                    pdfGridInfo.Rows[4].Cells[2].ColumnSpan = 2;
                    pdfGridInfo.Rows[4].Cells[4].Value = "Min & Max Limit: " + orl.belowLimit.ToString() + " & " + orl.maxRollingCount.ToString();
                    pdfGridInfo.Rows[4].Cells[4].ColumnSpan = 2;


                    //pdfGridInfo.Rows[3].Cells[0].Value = "Test System: " + orl.countsysname;
                    //pdfGridInfo.Rows[3].Cells[1].Value = "Length Unit: " + orl.yarnlenunit;
                    //pdfGridInfo.Rows[3].Cells[2].Value = "Length: " + orl.yarnlength;
                    //pdfGridInfo.Rows[3].Cells[3].Value = "Total Test: " + orl.totaltestcount;

                    //if (orl.machineCategory == "Spinning" || orl.machineCategory == "Winding")
                    //{
                    //    pdfGridInfo.Rows[4].Cells[0].Value = "Count: " + formatDecimal(orl.testaverage, 2).ToString() +
                    //        " [Std Count: " + formatDecimal(orl.standardHank, 2) + " " + orl.deviationPercent + "]";
                    //}
                    //else
                    //{
                    //    pdfGridInfo.Rows[4].Cells[0].Value = "Hank: " + formatDecimal(orl.testaverage, 4).ToString() +
                    //        " [Std Hank: " + formatDecimal(orl.standardHank, 4) + " " + orl.deviationPercent + "]";
                    //}
                    //pdfGridInfo.Rows[4].Cells[0].ColumnSpan = 2;
                    //if (orl.hankColor == "Red")
                    //{
                    //    pdfGridInfo.Rows[4].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                    //    pdfGridInfo.Rows[4].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    //    pdfGridInfo.Rows[4].Cells[0].Style.BackgroundBrush = PdfBrushes.Red;
                    //    PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                    //    pdfGridInfo.Rows[4].Cells[0].Style.TextBrush = brush_con;
                    //}


                    //pdfGridInfo.Rows[4].Cells[0].Style.TextPen = PdfPens.Red;
                    //pdfGridInfo.Rows[4].Cells[2].Value = "SD: " + orl.testsd;
                    //pdfGridInfo.Rows[4].Cells[1].Style.TextPen = PdfPens.Red;
                    //pdfGridInfo.Rows[4].Cells[3].Value = "CV: " + orl.testcv;
                    //pdfGridInfo.Rows[4].Cells[2].Style.TextPen = PdfPens.Red;
                    //pdfGridInfo.Rows[4].Cells[3].Value = "A%: " + orl.apercent;
                    //pdfGridInfo.Rows[5].Cells[0].Value = "Date: " + orl.createdate;
                    //pdfGridInfo.Rows[5].Cells[0].ColumnSpan = 1;
                    //pdfGridInfo.Rows[5].Cells[1].Value = "Duration: " + orl.testDuration;
                    //pdfGridInfo.Rows[5].Cells[2].Value = "Shift: " + orl.shift;
                    //pdfGridInfo.Rows[5].Cells[3].Value = "Process: " + orl.process;

                    if (orl.remark_1)
                    {
                        pdfGridInfo.Rows[5].Cells[0].Value = "Remark: " + orl.testRemark;
                        pdfGridInfo.Rows[5].Cells[0].ColumnSpan = 4;
                    }
                    else if (orl.remark_1 == false && orl.DispUF_1)
                    {
                        pdfGridInfo.Rows[5].Cells[0].Value = orl.uf_name_1 + ": " + orl.uf_value_1;
                        pdfGridInfo.Rows[5].Cells[0].ColumnSpan = 2;

                        if (orl.DispUF_2)
                        {
                            pdfGridInfo.Rows[5].Cells[2].Value = orl.uf_name_2 + ": " + orl.uf_value_2;
                            pdfGridInfo.Rows[5].Cells[2].ColumnSpan = 2;
                        }

                    }
                    else if (orl.remark_1 == false && orl.DispUF_1 == false && orl.DispUF_2_Col1 == true)
                    {
                        pdfGridInfo.Rows[5].Cells[0].Value = orl.uf_name_2 + ": " + orl.uf_value_2;
                        pdfGridInfo.Rows[5].Cells[0].ColumnSpan = 2;
                    }

                    if (orl.remark_2)
                    {
                        pdfGridInfo.Rows.Add();
                        pdfGridInfo.Rows[6].Cells[0].Value = "Remark: " + orl.testRemark;
                        pdfGridInfo.Rows[6].Cells[0].ColumnSpan = 4;
                    }
                    else if (orl.remark_2 == false && orl.DispUF_3)
                    {
                        pdfGridInfo.Rows.Add();
                        pdfGridInfo.Rows[6].Cells[0].Value = orl.uf_name_3 + ": " + orl.uf_value_3;
                        pdfGridInfo.Rows[6].Cells[0].ColumnSpan = 2;

                        if (orl.DispUF_4)
                        {
                            pdfGridInfo.Rows[6].Cells[2].Value = orl.uf_name_4 + ": " + orl.uf_value_4;
                            pdfGridInfo.Rows[6].Cells[2].ColumnSpan = 2;
                        }

                    }
                    else if (orl.remark_2 == false && orl.DispUF_3 == false && orl.DispUF_4_Col1 == true)
                    {
                        pdfGridInfo.Rows.Add();
                        pdfGridInfo.Rows[6].Cells[0].Value = orl.uf_name_4 + ": " + orl.uf_value_4;
                        pdfGridInfo.Rows[6].Cells[0].ColumnSpan = 2;
                    }

                    if (orl.remark_1 == false && orl.remark_2 == false)
                    {
                        pdfGridInfo.Rows.Add();
                        pdfGridInfo.Rows[7].Cells[0].Value = "Remark: " + orl.testRemark;
                        pdfGridInfo.Rows[7].Cells[0].ColumnSpan = 4;
                    }


                    pdfGridInfo.Rows[0].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[0].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[0].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[0].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[0].Cells[4].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[0].Cells[5].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[4].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[1].Cells[5].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[4].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[2].Cells[5].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[4].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[3].Cells[5].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[4].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[4].Cells[5].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[5].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[5].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[5].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[5].Cells[3].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[5].Cells[4].Style.Borders.All = PdfPens.Transparent;
                    pdfGridInfo.Rows[5].Cells[5].Style.Borders.All = PdfPens.Transparent;
                    //pdfGridInfo.Rows[6].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    //pdfGridInfo.Rows[6].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    //pdfGridInfo.Rows[6].Cells[2].Style.Borders.All = PdfPens.Transparent;
                    //pdfGridInfo.Rows[6].Cells[3].Style.Borders.All = PdfPens.Transparent;

                    if ((orl.remark_1 == false && orl.DispUF_1) || (orl.remark_1 == false && orl.DispUF_1 == false && orl.DispUF_2_Col1))
                    {
                        pdfGridInfo.Rows[6].Cells[0].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[6].Cells[1].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[6].Cells[2].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[6].Cells[3].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[6].Cells[4].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[6].Cells[5].Style.Borders.All = PdfPens.Transparent;
                    }
                    if ((orl.remark_2 == false && orl.DispUF_3) || (orl.remark_2 == false && orl.DispUF_3 == false && orl.DispUF_4_Col1))
                    {
                        pdfGridInfo.Rows[7].Cells[0].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[7].Cells[1].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[7].Cells[2].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[7].Cells[3].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[7].Cells[4].Style.Borders.All = PdfPens.Transparent;
                        pdfGridInfo.Rows[7].Cells[5].Style.Borders.All = PdfPens.Transparent;
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
                    pdfGrid.Rows[0].Cells[1].Value = "No. Of Swing";
                    pdfGrid.Rows[0].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[1].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[1].Style.TextPen = PdfPens.Black;
                    pdfGrid.Rows[0].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    //if (orl.machineCategory == "Spinning" || orl.machineCategory == "Winding")
                    //{
                    //    pdfGrid.Rows[0].Cells[2].Value = "Count";
                    //}
                    //else
                    //{
                    //    pdfGrid.Rows[0].Cells[2].Value = "Hank";
                    //}
                    pdfGrid.Rows[0].Cells[2].Value = "Qualified (Yes/No)";
                    pdfGrid.Rows[0].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[0].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[0].Cells[2].Style.BackgroundBrush = PdfBrushes.LightGray;
                    //pdfGrid.Rows[0].Cells[2].Style.TextPen = PdfPens.Black;
                    pdfGrid.Rows[0].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    //pdfGrid.Rows[0].Cells[0].Style.Borders.All = PdfPens.Transparent;
                    //pdfGrid.Rows[0].Cells[1].Style.Borders.All = PdfPens.Transparent;
                    //pdfGrid.Rows[0].Cells[2].Style.Borders.All = PdfPens.Transparent;



                    int rowCount = 1;
                    foreach (StrengthTestModel test in testList)
                    {
                        row = new PdfGridRow(pdfGrid);
                        pdfGrid.Rows.Add(row);
                        pdfGrid.Rows[rowCount].Cells[0].Value = test.sampleNo.ToString();
                        pdfGrid.Rows[rowCount].Cells[1].Value = test.sampleStrengthCount.ToString();
                        pdfGrid.Rows[rowCount].Cells[2].Value = test.isQualified.ToString();
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
                        overallHeight = overallHeight + result.Bounds.Height + 20;//changed from 20 to 40
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
                                //changed from 5 to 25
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
                                    result = pdfGrid.Draw(result.Page, new PointF(10, (overallHeight + 5)));
                                    //changed from 5 to 25
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
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream, "SVYA_Detailed_Report.pdf");
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

        private bool generatePDFreport_drumDetails()
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
                List<MissingDrumReportModelView> overallReportList = (List<MissingDrumReportModelView>)listview_tcreport_missingDrum.ItemsSource;
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
                foreach (MissingDrumReportModelView orl in overallReportList)
                {
                    if (includeHeader)
                    {
                        includeHeader = false;
                        pdfGrid = new PdfGrid();

                        pdfGrid.Columns.Add(8);
                        row = new PdfGridRow(pdfGrid);
                        pdfGrid.Rows.Add(row);

                        pdfGrid.Rows[0].Cells[0].Value = "Machine Name";
                        pdfGrid.Rows[0].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[0].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[1].Value = "Section No";
                        pdfGrid.Rows[0].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[1].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[2].Value = "Total Drums";
                        pdfGrid.Rows[0].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[2].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[3].Value = "Completed Drums";
                        pdfGrid.Rows[0].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[3].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[4].Value = "Pending Drums";
                        pdfGrid.Rows[0].Cells[4].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[4].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[4].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);

                        pdfGrid.Rows[0].Cells[5].Value = "Sch. Start Date";
                        pdfGrid.Rows[0].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[5].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[6].Value = "Sch. End Date";
                        pdfGrid.Rows[0].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[6].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[7].Value = "Remark";
                        pdfGrid.Rows[0].Cells[7].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[7].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[7].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[7].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        //pdfGrid.Rows[0].Cells[8].Value = "Strength";
                        //pdfGrid.Rows[0].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                        //pdfGrid.Rows[0].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        //pdfGrid.Rows[0].Cells[8].Style.BackgroundBrush = PdfBrushes.LightGray;
                        //pdfGrid.Rows[0].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        //pdfGrid.Rows[0].Cells[9].Value = "Test Time";
                        //pdfGrid.Rows[0].Cells[9].StringFormat.Alignment = PdfTextAlignment.Center;
                        //pdfGrid.Rows[0].Cells[9].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        //pdfGrid.Rows[0].Cells[9].Style.BackgroundBrush = PdfBrushes.LightGray;
                        //pdfGrid.Rows[0].Cells[9].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        //pdfGrid.Rows[0].Cells[10].Value = "Remark";
                        //pdfGrid.Rows[0].Cells[10].StringFormat.Alignment = PdfTextAlignment.Center;
                        //pdfGrid.Rows[0].Cells[10].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        //pdfGrid.Rows[0].Cells[10].Style.BackgroundBrush = PdfBrushes.LightGray;
                        //pdfGrid.Rows[0].Cells[10].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Height = pdfGrid.Rows[0].Height * 2;
                        row = new PdfGridRow(pdfGrid);
                        pdfGrid.Rows.Add(row);
                        rowHeights = rowHeights + pdfGrid.Rows[0].Height;
                    }


                    pdfGrid.Rows.Add();

                    pdfGrid.Rows[pageRecordCount].Cells[0].Value = orl.machineName;
                    pdfGrid.Rows[pageRecordCount].Cells[1].Value = orl.sectionNumber.ToString();
                    pdfGrid.Rows[pageRecordCount].Cells[2].Value = orl.totalDrumNumbers.Replace("."," to ");
                    pdfGrid.Rows[pageRecordCount].Cells[3].Value = orl.testCompletedDrums;
                    pdfGrid.Rows[pageRecordCount].Cells[4].Value = orl.pendingTestDrums;
                    pdfGrid.Rows[pageRecordCount].Cells[5].Value = orl.scheduledStartDate;
                    pdfGrid.Rows[pageRecordCount].Cells[6].Value = orl.scheduledEndDate;
                    pdfGrid.Rows[pageRecordCount].Cells[7].Value = orl.remark;
                    //pdfGrid.Rows[pageRecordCount].Cells[8].Value = orl.strength;
                    //pdfGrid.Rows[pageRecordCount].Cells[9].Value = orl.testDuration;
                    //pdfGrid.Rows[pageRecordCount].Cells[10].Value = orl.remarks;

                    int contentLength = orl.pendingTestDrums.Length;

                    if(orl.testCompletedDrums.Length> orl.pendingTestDrums.Length)
                    {
                        contentLength = orl.testCompletedDrums.Length;
                    }

                    float currentRowHeight = pdfGrid.Rows[pageRecordCount].Height;
                    pdfGrid.Rows[pageRecordCount].Height = currentRowHeight * ((contentLength / 14) + 1);

                    

                    pdfGrid.Rows[pageRecordCount].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

                    //pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

                    if (orl.pendingTestDrumsColor=="green")
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[4].Style.BackgroundBrush = PdfBrushes.White;
                        //pdfGrid.Rows[pageRecordCount].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.Black);
                        pdfGrid.Rows[pageRecordCount].Cells[4].Style.TextBrush = brush_con;
                    }
                    else
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[4].Style.BackgroundBrush = PdfBrushes.Red;
                        //pdfGrid.Rows[pageRecordCount].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                        pdfGrid.Rows[pageRecordCount].Cells[4].Style.TextBrush = brush_con;
                    }

                    pdfGrid.Rows[pageRecordCount].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;



                    pdfGrid.Rows[pageRecordCount].Cells[7].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[7].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    //pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;


                    //if (orl.isWhite)
                    //{
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].Style.BackgroundBrush = PdfBrushes.White;
                    //    //pdfGrid.Rows[pageRecordCount].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    //    PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.Black);
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].Style.TextBrush = brush_con;
                    //}
                    //else
                    //{
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].Style.BackgroundBrush = PdfBrushes.Red;
                    //    //pdfGrid.Rows[pageRecordCount].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    //    PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].Style.TextBrush = brush_con;
                    //}

                    //pdfGrid.Rows[pageRecordCount].Cells[9].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[pageRecordCount].Cells[9].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    //pdfGrid.Rows[pageRecordCount].Cells[10].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[pageRecordCount].Cells[10].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

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
                        //pdfGrid.Rows[pageRecordCount].Cells[8].Style.Borders.All = PdfPens.Transparent;
                        //pdfGrid.Rows[pageRecordCount].Cells[9].Style.Borders.All = PdfPens.Transparent;
                        //pdfGrid.Rows[pageRecordCount].Cells[10].Style.Borders.All = PdfPens.Transparent;

                        pdfGrid.Rows[pageRecordCount].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[7].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        //pdfGrid.Rows[pageRecordCount].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        //pdfGrid.Rows[pageRecordCount].Cells[9].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        //pdfGrid.Rows[pageRecordCount].Cells[10].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
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
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream, "SVYA_Consolidated_Drum_Report.pdf");
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

        private bool generatePDFreport_Maintenance()
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
                List<MaintenanceReportMV> overallReportList = (List<MaintenanceReportMV>)listview_tcreport_maintenance.ItemsSource;
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
                foreach (MaintenanceReportMV orl in overallReportList)
                {
                    if (includeHeader)
                    {
                        includeHeader = false;
                        pdfGrid = new PdfGrid();

                        pdfGrid.Columns.Add(8);
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
                        pdfGrid.Rows[0].Cells[2].Value = "Machine Name";
                        pdfGrid.Rows[0].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[2].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[3].Value = "Speed";
                        pdfGrid.Rows[0].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[3].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[4].Value = "P1";
                        pdfGrid.Rows[0].Cells[4].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[4].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[4].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);

                        pdfGrid.Rows[0].Cells[5].Value = "P2";
                        pdfGrid.Rows[0].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[5].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[6].Value = "N1";
                        pdfGrid.Rows[0].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[6].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[7].Value = "Remark";
                        pdfGrid.Rows[0].Cells[7].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[7].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[7].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[7].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        //pdfGrid.Rows[0].Cells[8].Value = "Strength";
                        //pdfGrid.Rows[0].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                        //pdfGrid.Rows[0].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        //pdfGrid.Rows[0].Cells[8].Style.BackgroundBrush = PdfBrushes.LightGray;
                        //pdfGrid.Rows[0].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        //pdfGrid.Rows[0].Cells[9].Value = "Test Time";
                        //pdfGrid.Rows[0].Cells[9].StringFormat.Alignment = PdfTextAlignment.Center;
                        //pdfGrid.Rows[0].Cells[9].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        //pdfGrid.Rows[0].Cells[9].Style.BackgroundBrush = PdfBrushes.LightGray;
                        //pdfGrid.Rows[0].Cells[9].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        //pdfGrid.Rows[0].Cells[10].Value = "Remark";
                        //pdfGrid.Rows[0].Cells[10].StringFormat.Alignment = PdfTextAlignment.Center;
                        //pdfGrid.Rows[0].Cells[10].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        //pdfGrid.Rows[0].Cells[10].Style.BackgroundBrush = PdfBrushes.LightGray;
                        //pdfGrid.Rows[0].Cells[10].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Height = pdfGrid.Rows[0].Height * 2;
                        row = new PdfGridRow(pdfGrid);
                        pdfGrid.Rows.Add(row);
                        rowHeights = rowHeights + pdfGrid.Rows[0].Height;
                    }


                    pdfGrid.Rows.Add();

                    pdfGrid.Rows[pageRecordCount].Cells[0].Value = orl.serialNo;
                    pdfGrid.Rows[pageRecordCount].Cells[1].Value = orl.date;
                    pdfGrid.Rows[pageRecordCount].Cells[2].Value = orl.machineName;
                    pdfGrid.Rows[pageRecordCount].Cells[3].Value = orl.speed;
                    pdfGrid.Rows[pageRecordCount].Cells[4].Value = orl.p1;
                    pdfGrid.Rows[pageRecordCount].Cells[5].Value = orl.p2;
                    pdfGrid.Rows[pageRecordCount].Cells[6].Value = orl.n1;
                    pdfGrid.Rows[pageRecordCount].Cells[7].Value = orl.remark;
                    //pdfGrid.Rows[pageRecordCount].Cells[8].Value = orl.strength;
                    //pdfGrid.Rows[pageRecordCount].Cells[9].Value = orl.testDuration;
                    //pdfGrid.Rows[pageRecordCount].Cells[10].Value = orl.remarks;

                    int contentLength = orl.date.Length;

                    

                    float currentRowHeight = pdfGrid.Rows[pageRecordCount].Height;
                    pdfGrid.Rows[pageRecordCount].Height = currentRowHeight * ((contentLength / 10) + 1);



                    pdfGrid.Rows[pageRecordCount].Cells[0].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[0].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[1].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[1].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

                    //pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

                    if (orl.p1_bgcolor == "green")
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[4].Style.BackgroundBrush = PdfBrushes.White;
                        //pdfGrid.Rows[pageRecordCount].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.Black);
                        pdfGrid.Rows[pageRecordCount].Cells[4].Style.TextBrush = brush_con;
                    }
                    else
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[4].Style.BackgroundBrush = PdfBrushes.Red;
                        //pdfGrid.Rows[pageRecordCount].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                        pdfGrid.Rows[pageRecordCount].Cells[4].Style.TextBrush = brush_con;
                    }

                    //pdfGrid.Rows[pageRecordCount].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[pageRecordCount].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

                    if (orl.p2_bgcolor == "green")
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[5].Style.BackgroundBrush = PdfBrushes.White;
                        //pdfGrid.Rows[pageRecordCount].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.Black);
                        pdfGrid.Rows[pageRecordCount].Cells[5].Style.TextBrush = brush_con;
                    }
                    else
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[5].Style.BackgroundBrush = PdfBrushes.Red;
                        //pdfGrid.Rows[pageRecordCount].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                        pdfGrid.Rows[pageRecordCount].Cells[5].Style.TextBrush = brush_con;
                    }

                    //pdfGrid.Rows[pageRecordCount].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[pageRecordCount].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

                    if (orl.n1_bgcolor == "green")
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


                    //if (orl.isWhite)
                    //{
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].Style.BackgroundBrush = PdfBrushes.White;
                    //    //pdfGrid.Rows[pageRecordCount].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    //    PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.Black);
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].Style.TextBrush = brush_con;
                    //}
                    //else
                    //{
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].Style.BackgroundBrush = PdfBrushes.Red;
                    //    //pdfGrid.Rows[pageRecordCount].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    //    PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                    //    pdfGrid.Rows[pageRecordCount].Cells[8].Style.TextBrush = brush_con;
                    //}

                    //pdfGrid.Rows[pageRecordCount].Cells[9].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[pageRecordCount].Cells[9].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    //pdfGrid.Rows[pageRecordCount].Cells[10].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[pageRecordCount].Cells[10].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

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
                        //pdfGrid.Rows[pageRecordCount].Cells[8].Style.Borders.All = PdfPens.Transparent;
                        //pdfGrid.Rows[pageRecordCount].Cells[9].Style.Borders.All = PdfPens.Transparent;
                        //pdfGrid.Rows[pageRecordCount].Cells[10].Style.Borders.All = PdfPens.Transparent;

                        pdfGrid.Rows[pageRecordCount].Cells[0].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[1].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        pdfGrid.Rows[pageRecordCount].Cells[7].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        //pdfGrid.Rows[pageRecordCount].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        //pdfGrid.Rows[pageRecordCount].Cells[9].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
                        //pdfGrid.Rows[pageRecordCount].Cells[10].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9, PdfFontStyle.Bold);
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
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream, "SVYA_Consolidated_Maintenance_Report.pdf");
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
                using (var textWriter = new StreamWriter(Path.Combine(downloadsFolder, "SVYA_Consolidated_CSV_Report.csv")))
                {
                   
                    var writer = new CsvWriter(textWriter, CultureInfo.InvariantCulture);
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter=",",
                        HasHeaderRecord=false
                    };
                    //Header
                    writer.WriteField("Mac Parameters");
                    writer.WriteField("Date");
                    writer.WriteField("ID");
                    writer.WriteField("Mac Name");
                    writer.WriteField("Shift");
                    writer.WriteField("Drum No");
                    writer.WriteField("Tot. Sample");
                    writer.WriteField("Qualified");
                    writer.WriteField("Strength");
                    writer.WriteField("Test Time");
                    writer.WriteField("Remark");
                    //Actual Data
                    writer.NextRecord();
                    List<StrengthTestConsolidatedReportMV> overallReportList = (List<StrengthTestConsolidatedReportMV>)listview_tcConsolidatedReport.ItemsSource;
                    foreach (StrengthTestConsolidatedReportMV orl in overallReportList)
                    {
                        writer.WriteField(orl.serialNo);
                        writer.WriteField(orl.testDate);
                        writer.WriteField(orl.testID);
                        writer.WriteField(orl.machineName);
                        writer.WriteField(orl.shift);
                        writer.WriteField(orl.drumNumber);
                        writer.WriteField(orl.totalTestCount);
                        writer.WriteField(orl.qualifiedTestCount);
                        writer.WriteField(orl.strength);
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
                List<StrengthTestConsolidatedReportMV> overallReportList = (List<StrengthTestConsolidatedReportMV>)listview_tcConsolidatedReport.ItemsSource;
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
                foreach (StrengthTestConsolidatedReportMV orl in overallReportList)
                {
                    if (includeHeader)
                    {
                        includeHeader = false;
                        pdfGrid = new PdfGrid();

                        pdfGrid.Columns.Add(11);
                        row = new PdfGridRow(pdfGrid);
                        pdfGrid.Rows.Add(row);

                        pdfGrid.Rows[0].Cells[0].Value = "Mac Parameters";
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
                        pdfGrid.Rows[0].Cells[3].Value = "Machine";
                        pdfGrid.Rows[0].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[3].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[3].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[4].Value = "Shift";
                        pdfGrid.Rows[0].Cells[4].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[4].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[4].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[4].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);

                        pdfGrid.Rows[0].Cells[5].Value = "Drum No";
                        pdfGrid.Rows[0].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[5].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[5].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[6].Value = "Total Sample";
                        pdfGrid.Rows[0].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[6].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[6].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[7].Value = "Qualified";
                        pdfGrid.Rows[0].Cells[7].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[7].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[7].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[7].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[8].Value = "Strength";
                        pdfGrid.Rows[0].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[8].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[8].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[9].Value = "Test Time";
                        pdfGrid.Rows[0].Cells[9].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[9].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[9].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[9].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Cells[10].Value = "Remark";
                        pdfGrid.Rows[0].Cells[10].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[0].Cells[10].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[0].Cells[10].Style.BackgroundBrush = PdfBrushes.LightGray;
                        pdfGrid.Rows[0].Cells[10].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 9);
                        pdfGrid.Rows[0].Height = pdfGrid.Rows[0].Height * 2;
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
                    pdfGrid.Rows[pageRecordCount].Cells[5].Value = orl.drumNumber;
                    pdfGrid.Rows[pageRecordCount].Cells[6].Value = orl.totalTestCount;
                    pdfGrid.Rows[pageRecordCount].Cells[7].Value = orl.qualifiedTestCount;
                    pdfGrid.Rows[pageRecordCount].Cells[8].Value = orl.strength;
                    pdfGrid.Rows[pageRecordCount].Cells[9].Value = orl.testDuration;
                    pdfGrid.Rows[pageRecordCount].Cells[10].Value = orl.remarks;

                    if (orl.remarks != null || orl.strength != null)
                    {
                        //PdfStringFormat format = new PdfStringFormat();
                        //format.WordWrap = PdfWordWrapType.Word;
                        //pdfGrid.Rows[pageRecordCount].Cells[10].Style.StringFormat = format;
                        float currentRowHeight = pdfGrid.Rows[pageRecordCount].Height;
                        int contentLength = 0;
                        if (orl.remarks != null)
                        {
                            contentLength = orl.remarks.Length;
                        }
                        if (orl.strength != null && orl.remarks != null)
                        {
                            if (orl.strength.Length > orl.remarks.Length)
                            {
                                contentLength = orl.strength.Length;
                            }
                        }
                        else if (orl.strength != null && orl.remarks == null)
                        {
                            contentLength = orl.strength.Length;
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
                    //pdfGrid.Rows[pageRecordCount].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[pageRecordCount].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

                    if (orl.testIDColor == "green")
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[2].Style.BackgroundBrush = PdfBrushes.White;
                        //pdfGrid.Rows[pageRecordCount].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.Black);
                        pdfGrid.Rows[pageRecordCount].Cells[2].Style.TextBrush = brush_con;
                    }
                    else
                    {
                        pdfGrid.Rows[pageRecordCount].Cells[2].StringFormat.Alignment = PdfTextAlignment.Center;
                        pdfGrid.Rows[pageRecordCount].Cells[2].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                        pdfGrid.Rows[pageRecordCount].Cells[2].Style.BackgroundBrush = PdfBrushes.Red;
                        //pdfGrid.Rows[pageRecordCount].Cells[2].Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                        PdfBrush brush_con = new PdfSolidBrush(Syncfusion.Drawing.Color.White);
                        pdfGrid.Rows[pageRecordCount].Cells[2].Style.TextBrush = brush_con;
                    }

                    pdfGrid.Rows[pageRecordCount].Cells[3].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[3].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[4].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[5].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[5].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    pdfGrid.Rows[pageRecordCount].Cells[6].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[6].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;

                  

                    pdfGrid.Rows[pageRecordCount].Cells[7].StringFormat.Alignment = PdfTextAlignment.Center;
                    pdfGrid.Rows[pageRecordCount].Cells[7].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
                    //pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.Alignment = PdfTextAlignment.Center;
                    //pdfGrid.Rows[pageRecordCount].Cells[8].StringFormat.LineAlignment = PdfVerticalAlignment.Middle;


                    if (orl.strengthColor=="green")
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
                    pdfGrid.Rows[pageRecordCount].Cells[10].StringFormat.WordWrap = PdfWordWrapType.Word;

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
                string pdfPath = Xamarin.Forms.DependencyService.Get<ISave>().Save(stream, "SVYA_Consolidated_Report.pdf");
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
                    header.Graphics.DrawString("Consolidated Report -" + " (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(135, 20));
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
                else if (drumDetailsReport)
                {
                    header.Graphics.DrawString("SVYA Drum Details Report (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(165, 16));
                }
                else
                {
                    //if (selectedMachineCategory != null)
                    //{
                    //    header.Graphics.DrawString("Wrapping Report - " + selectedMachineCategory + " (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(165, 16));
                    //}
                    //else
                    //{
                    //    header.Graphics.DrawString("Wrapping Report - All (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(165, 16));
                    //}
                    header.Graphics.DrawString("SVYA Detailed Report (" + reportStartDate.Day + "-" + reportStartDate.Month + "-" + reportStartDate.Year + " To " + reportEndDate.Day + "-" + reportEndDate.Month + "-" + reportEndDate.Year + " )", font_rn, brush_rn, new PointF(165, 16));
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
                        string fileName = "SVYA_Consolidated_Report.pdf";
                        string root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                        Java.IO.File myDir = new Java.IO.File(root + "/SVYADownloads");
                        Java.IO.File file = new Java.IO.File(myDir, fileName);
                        string filePath = file.Path;
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqmreport/upload");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        //request.Timeout = Timeout.Infinite;
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("uploadedby", companyName);
                        //if (selectedMachineCategory != null)
                        //{
                        //    request.AddParameter("title", "TQMReportsConsolidated(Wrapping-" + selectedMachineCategory + ")-" + DateTime.Now.ToString());
                        //}
                        //else
                        //{
                        //    request.AddParameter("title", "TQMReportsConsolidated(Wrapping-All)-" + DateTime.Now.ToString());
                        //}
                        request.AddParameter("title", "SVYA-Consolidated-Report-" + DateTime.Now.ToString());
                        request.AddFile("reportpath", filePath);
                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (runConfiguration.getCSVReportStatus())
                            {
                                if (!generateCSVConsolidatedReport()) { showAlert("Error occurred in CSV report generation, hence upload is unsucessful!!!"); await resetBtn(); return; }
                                fileName = "SVYA_Consolidated_CSV_Report.csv";
                                root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                                myDir = new Java.IO.File(root + "/SVYADownloads");
                                file = new Java.IO.File(myDir, fileName);
                                filePath = file.Path;
                                client = new RestClient("https://myconsoleerp.herokuapp.com/tqmreport/upload");
                                request = new RestRequest();
                                request.Method = Method.Post;
                                //request.Timeout = Timeout.Infinite;
                                request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                                request.AddParameter("uploadedby", companyName);
                                //if (selectedMachineCategory != null)
                                //{
                                //    request.AddParameter("title", "TQMReportsConsolidated-CSV-(Wrapping-" + selectedMachineCategory + ")-" + DateTime.Now.ToString());
                                //}
                                //else
                                //{
                                //    request.AddParameter("title", "TQMReportsConsolidated-CSV-(Wrapping-All)-" + DateTime.Now.ToString());
                                //}
                                request.AddParameter("title", "SVYA-Consolidated-CSV-Report-" + DateTime.Now.ToString());
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
                else if (drumDetailsReport)
                {
                    if (!generatePDFreport_drumDetails()) { showAlert("Error occurred in PDF report generation, hence upload is unsucessful!!!"); await resetBtn(); return; }
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
                        string fileName = "SVYA_Consolidated_Drum_Report.pdf";
                        string root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                        Java.IO.File myDir = new Java.IO.File(root + "/SVYADownloads");
                        Java.IO.File file = new Java.IO.File(myDir, fileName);
                        string filePath = file.Path;
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqmreport/upload");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        //request.Timeout = Timeout.Infinite;
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("uploadedby", companyName);
                        request.AddParameter("title", "SVYA Consolidated Drum Report -" + DateTime.Now.ToString());
                        request.AddFile("reportpath", filePath);
                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            showAlert("Report uploaded sucessfully!!!");
                        }
                        else
                        {
                            showAlert("Upload Failed. Please try again!!!", "Error");
                        }
                        await resetBtn();
                    }
                }
                else if (maintenanceReport)
                {
                    if (!generatePDFreport_Maintenance()) { showAlert("Error occurred in PDF report generation, hence upload is unsucessful!!!"); await resetBtn(); return; }
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
                        string fileName = "SVYA_Consolidated_Maintenance_Report.pdf";
                        string root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                        Java.IO.File myDir = new Java.IO.File(root + "/SVYADownloads");
                        Java.IO.File file = new Java.IO.File(myDir, fileName);
                        string filePath = file.Path;
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqmreport/upload");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        //request.Timeout = Timeout.Infinite;
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("uploadedby", companyName);
                        request.AddParameter("title", "SVYA Consolidated Maintenance Report -" + DateTime.Now.ToString());
                        request.AddFile("reportpath", filePath);
                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            showAlert("Report uploaded sucessfully!!!");
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
                        string fileName = "SVYA_Detailed_Report.pdf";
                        string root = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                        Java.IO.File myDir = new Java.IO.File(root + "/SVYADownloads");
                        Java.IO.File file = new Java.IO.File(myDir, fileName);
                        string filePath = file.Path;
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqmreport/upload");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        //request.Timeout = Timeout.Infinite;
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("uploadedby", companyName);
                        //if (selectedMachineCategory != null)
                        //{
                        //    request.AddParameter("title", "TQMReports(Wrapping-" + selectedMachineCategory + ")-" + DateTime.Now.ToString());
                        //}
                        //else
                        //{
                        //    request.AddParameter("title", "TQMReports(Wrapping-All" + DateTime.Now.ToString());
                        //}
                        request.AddParameter("title", "SVYA-Detailed-Report-" + DateTime.Now.ToString());
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