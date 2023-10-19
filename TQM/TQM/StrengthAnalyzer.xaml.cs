using Android.Bluetooth;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Renderscripts;
using Android.Text;
using Android.Views;
using Android.Widget;
//using Foundation;
using Java.IO;
using Java.Util;
using Javax.Crypto;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TQM.Model;
using TQM.ModelView;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;
using static Android.Icu.Text.IDNA;
using static Android.Preferences.PreferenceActivity;
using static System.Net.Mime.MediaTypeNames;
using Color = Xamarin.Forms.Color;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StrengthAnalyzer : ContentPage, INotifyPropertyChanged
    {

        private static readonly DateTime DEFAULTDATE = new DateTime(2000, 01, 01);
        private BluetoothSocket _socket;
        BluetoothAdapter adapter;
        BluetoothDevice device;
        const decimal MIN_VAL = 0.4000m;
        const decimal ZERO = 0.0000m;
        const int PER_TEST_LOOP_COUNT = 100;
        const int DATA_READ_LOOP_COUNT = 100;
        const int STABLE_DATA_CHECK = 5;
        private int current_stable_data = 0;
        private List<StrengthTestModelView> StrengthTestModelViewlist;
        private long currentTestID = 0;
        private UserModel currentloggedInUser = null;
        private string selectedMachineCategory = null;
        private Guid selectedMachineID = Guid.Empty;
        private string selectedMachineName = null;
        private int selectedSpeed = 0;
        private decimal selectedP1 = 0.0m;
        private decimal selectedP1Deviation = 0.0m;
        private decimal selectedP2 = 0.0m;
        private decimal selectedP2Deviation = 0.0m;
        private decimal selectedN1 = 0.0m;
        private decimal selectedN1Deviation = 0.0m;
        private int selectedSectionNumber = 0;
        public string selectedTotalDrumNumbers = "";
        private int selectedDrumNumber = 0;
        private int selectedDrumStartNo = 0;
        private int selectedDrumEndNo = 0;
        private decimal selectedStandardStrength = 0.0m;
        private decimal selectedStrengthDeviation = 0.0m;
        private int selectedBelowLimit = 0;
        private int selectedTotalTestCount = 0;
        private string selectedDrumSelectionMethod = null;
        private string selectedShift = null;
        private DateTime scheduledStartDate = DEFAULTDATE;
        private DateTime scheduledEndDate = DEFAULTDATE;
        private DateTime settingsUpdatedDate = DEFAULTDATE;
        private int selectedMaxRollingCount = 0;
        private string selectedMaterialCount = null;
        private string UFVAL1 = null;
        private string UFVAL2 = null;
        private string UFVAL3 = null;
        private string UFVAL4 = null;
        private const string RED = "#FF0000";
        private const string GREEN = "#145A32";
        private const int BUFFER_WAIT_COUNT = 10;
        private int TESTCOUNT = 0;
        private decimal STD_HANK = 0.0000m;
        private decimal STD_HANK_CURR = 0.0000m;
        private int currentTestCount = 0;
        private bool isTestStarted = false;
        private dynamic currentTestStartTime = null;
        private RunConfiguration runConfiguration = new RunConfiguration();
        private bool toastInitialize = false;
        private bool isTestCompleted = false;
        private bool isTestResume = false;

        public StrengthAnalyzer()
        {
            InitializeComponent();
            initializer();
        }

        public StrengthAnalyzer(string machineCat, Guid machineID, string machineName, int sectionNo, int drumNo,int drumStartNo, int drumEndNo)
        {   InitializeComponent();
            initializer();
            selectedMachineCategory = machineCat;
            selectedMachineID = machineID;
            selectedMachineName = machineName;
            selectedSectionNumber = sectionNo;
            selectedDrumNumber = drumNo;
            selectedDrumStartNo = drumStartNo;
            selectedDrumEndNo = drumEndNo;
            IList<string> machineCategorylist = picker_machinecategory.Items;
            int machineCatindex = 0;
            foreach (string mCat in machineCategorylist)
            {
                if (mCat != machineCat)
                {
                    machineCatindex++;
                }
                else { break; }
            }
            picker_machinecategory.SelectedIndex = machineCatindex;

            IList<string> machinelist = picker_machinename.Items;
            int machineindex = 0;
            foreach (string m in machinelist)
            {
                if (m != machineName)
                {
                    machineindex++;
                }
                else { break; }
            }
            picker_machinename.SelectedIndex = machineindex;

            IList<string> drumNumberlist = picker_drumNumber.Items;
            int drumNumberindex = 0;
            foreach (string drum in drumNumberlist)
            {
                if (drum != drumNo.ToString())
                {
                    drumNumberindex++;
                }
                else { break; }
            }
            picker_drumNumber.SelectedIndex = drumNumberindex;
        }


        private void initializer()
        {
            picker_machinecategory.SelectedItem = "OE Auto Coner";
            lbl_TestID.Text = "";
            //lbl_TestID.Text = "999999999";

            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.DropTable<StrengthTestModel>();
                conn.DropTable<StrengthTestSummaryModel>();
                //conn.DropTable<TestConfigModel>();

                UserModel loggedInUser = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                if (loggedInUser == null)
                {
                    DisplayAlert("Attention", "Unable to get logged user information!!!", "OK");
                    return;
                }
                else
                {
                    currentloggedInUser = loggedInUser;
                    if (loggedInUser.lastname.Trim() != "")
                    {
                        lbl_un.Text = "Logged in user: " + loggedInUser.firstname + ", " + loggedInUser.lastname + " [" + loggedInUser.userId + "]";
                    }
                    else
                    {
                        lbl_un.Text = "Logged in user: " + loggedInUser.firstname + " [" + loggedInUser.userId + "]";
                    }
                }
                ///*******************************Jaganatha Unit-3, bhagirath Test Reset Issue Issue - Auto Correction**************************

                conn.CreateTable<StrengthTestModel>();
                conn.CreateTable<StrengthTestSummaryModel>();
                conn.CreateTable<TestConfigModel>();

                int recordCount = conn.Table<StrengthTestModel>().Count();

                if (recordCount > 0)
                {
                    StrengthTestModel zeroTest = conn.Table<StrengthTestModel>().Where(StrengthTestModel => (StrengthTestModel.testID == 0))
                                            .OrderBy(StrengthTestModel => StrengthTestModel.sampleNo).FirstOrDefault();

                    if (zeroTest != null)
                    {
                        DateTime startDate = zeroTest.createdate;
                        StrengthTestModel beforeZeroTest = conn.Table<StrengthTestModel>().Where(StrengthTestModel => (StrengthTestModel.createdate < startDate))
                                            .OrderByDescending(StrengthTestModel => StrengthTestModel.testID).FirstOrDefault();

                        if (beforeZeroTest != null)
                        {
                            long lastProperTestID = beforeZeroTest.testID + 1;
                            List<StrengthTestModel> resetTestList = conn.Table<StrengthTestModel>().Where(StrengthTestModel => (StrengthTestModel.createdate >= startDate))
                                                              .OrderBy(StrengthTestModel => StrengthTestModel.testID).ToList();
                            int failCounter = 0;
                            if (resetTestList[0].testID == 0)
                            {
                                foreach (StrengthTestModel test in resetTestList)
                                {
                                    test.testID = lastProperTestID + test.testID;
                                    int row = conn.Update(test);
                                    if (row < 1)
                                    {
                                        failCounter++;
                                    }
                                }
                            }

                            List<StrengthTestSummaryModel> resetTestSummaryList = conn.Table<StrengthTestSummaryModel>().Where(StrengthTestSummaryModel =>
                                                                    (StrengthTestSummaryModel.createdate >= startDate))
                                                                    .OrderBy(StrengthTestSummaryModel => StrengthTestSummaryModel.testID).ToList();
                            int failCounter_Summary = 0;
                            if (resetTestSummaryList[0].testID == 0)
                            {
                                foreach (StrengthTestSummaryModel testsummary in resetTestSummaryList)
                                {
                                    testsummary.testID = lastProperTestID + testsummary.testID;
                                    int row = conn.Update(testsummary);
                                    if (row < 1)
                                    {
                                        failCounter_Summary++;
                                    }
                                }
                            }

                            if (failCounter == 0 && failCounter_Summary == 0)
                            {
                                DisplayAlert("Test Reset Warning!!!", "Test reset to Zero and it is corrected automatically for " + resetTestList.Count.ToString() + " records and " + resetTestSummaryList.Count.ToString() + " wrapping tests", "Okay");
                            }
                            else if (failCounter > 0 || failCounter_Summary > 0)
                            {
                                DisplayAlert("Test Reset Error!!!", "Test reset to Zero and auto-correction failed for " + failCounter.ToString() + "/" + resetTestList.Count.ToString() + " records and " + failCounter_Summary.ToString() + "/" + resetTestSummaryList.Count.ToString() + " wrapping tests", "Okay");
                            }
                        }
                    }
                }

                //*************************************************************************************
                //****************** Resume Test **********************
                conn.CreateTable<StrengthTestModel>();
                int totalRecords = conn.Table<StrengthTestModel>().Count();
                if (totalRecords > 0)
                {
                    DateTime maxDate = conn.Table<StrengthTestModel>().Max(StrengthTestModel => StrengthTestModel.createdate);
                    if (DateTime.Now <= maxDate)
                    {
                        DisplayAlert("Attention", "Tablet date time was modified. Please change it to actual current date and time to proceed!!!", "OK");
                        return;
                    }
                    StrengthTestModel lastTestRecord = conn.Table<StrengthTestModel>()
                        .Where(StrengthTestModel => StrengthTestModel.createdate == maxDate).FirstOrDefault();
                    if (lastTestRecord.totalTestCount != lastTestRecord.sampleNo)
                    {
                        isTestResume = true;
                        testYCButton.Text = "Resume";
                        testYCButton.BackgroundColor = Color.IndianRed;

                        currentTestID = lastTestRecord.testID;
                        lbl_TestID.Text = lastTestRecord.testID.ToString();
                        currentTestStartTime = null;
                        currentTestStartTime = lastTestRecord.createdate;

                        scheduledStartDate = lastTestRecord.scheduledStartDate;
                        scheduledEndDate = lastTestRecord.scheduledEndDate;
                        settingsUpdatedDate = lastTestRecord.settingsUpdatedDate;

                        IList<string> machineCategorylist = picker_machinecategory.Items;
                        int machineCatindex = 0;
                        foreach (string mCat in machineCategorylist)
                        {
                            if (mCat != lastTestRecord.machineCategory.ToString())
                            {
                                machineCatindex++;
                            }
                            else
                            {
                                selectedMachineCategory = lastTestRecord.machineCategory.ToString();
                                break;
                            }
                        }
                        picker_machinecategory.SelectedIndex = machineCatindex;
                        if (machineCatindex != 0) { selectedMachineCategory = machineCategorylist[machineCatindex]; }

                        IList<string> machinelist = picker_machinename.Items;
                        int machineindex = 0;
                        foreach (string m in machinelist)
                        {
                            if (m != lastTestRecord.machineName.ToString())
                            {
                                machineindex++;
                            }
                            else
                            {
                                selectedMachineID = lastTestRecord.machineID;
                                selectedMachineName = lastTestRecord.machineName.ToString();
                                break;
                            }
                        }
                        picker_machinename.SelectedIndex = machineindex;

                        selectedSpeed = lastTestRecord.speed;
                        selectedP1 = lastTestRecord.p1;
                        selectedP1Deviation = lastTestRecord.p1Deviation;
                        selectedP2 = lastTestRecord.p2;
                        selectedP2Deviation = lastTestRecord.p2Deviation;
                        selectedN1 = lastTestRecord.n1;
                        selectedN1Deviation = lastTestRecord.n1Deviation;

                        selectedSectionNumber = lastTestRecord.sectionNumber;
                        selectedTotalDrumNumbers = lastTestRecord.totalDrumNumbers;

                        IList<string> drumNumberlist = picker_drumNumber.Items;
                        int drumNumberindex = 0;
                        foreach (string drum in drumNumberlist)
                        {
                            if (drum != lastTestRecord.drumNumber.ToString())
                            {
                                drumNumberindex++;
                            }
                            else
                            {
                                selectedDrumNumber = lastTestRecord.drumNumber;
                                break;
                            }
                        }
                        picker_drumNumber.SelectedIndex = drumNumberindex;

                        entry_stdStrength.Text = lastTestRecord.standardStrength.ToString();
                        entry_strengthDeviation.Text = lastTestRecord.strengthDeviation.ToString();
                        entry_belowLimit.Text = lastTestRecord.belowLimit.ToString() + " & " + lastTestRecord.maxRollingCount.ToString();
                        entry_numberOfTest.Text = lastTestRecord.totalTestCount.ToString();

                        IList<string> drumSelectionMethodlist = picker_drumSelection.Items;
                        int drumSelectionMethodindex = 0;
                        foreach (string drum in drumSelectionMethodlist)
                        {
                            if (drum != lastTestRecord.drumSelectionMethod.ToString())
                            {
                                drumSelectionMethodindex++;
                            }
                            else
                            {
                                selectedDrumSelectionMethod = lastTestRecord.drumSelectionMethod;
                                break;
                            }
                        }
                        picker_drumSelection.SelectedIndex = drumSelectionMethodindex;
                        picker_drumSelection.IsEnabled = false;

                        selectedBelowLimit = lastTestRecord.belowLimit;
                        selectedMaxRollingCount = lastTestRecord.maxRollingCount;
                        selectedMaterialCount = lastTestRecord.materialCount;

                        picker_machinename.IsEnabled = false;
                        picker_drumNumber.IsEnabled = false;
                        entry_stdStrength.IsEnabled = false;
                        entry_strengthDeviation.IsEnabled = false;
                        entry_belowLimit.IsEnabled = false;
                        entry_numberOfTest.IsEnabled = false;
                        picker_shift.IsEnabled = false;


                        List<StrengthTestModel> allIncompleteTests = conn.Table<StrengthTestModel>().Where(StrengthTestModel =>
                                                                     (StrengthTestModel.testID == lastTestRecord.testID))
                                                                    .OrderBy(StrengthTestModel => StrengthTestModel.testID).ToList();

                        if (allIncompleteTests.Count > 0)
                        {
                            StrengthTestModelViewlist = new List<StrengthTestModelView>();
                            foreach (StrengthTestModel test in allIncompleteTests)
                            {
                                StrengthTestModelView strengthTestModelView = new StrengthTestModelView()
                                {
                                    testID = currentTestID,
                                    userID = test.userID,
                                    userName = test.userName,
                                    machineID = test.machineID,
                                    machineCategory = test.machineCategory,
                                    machineName = test.machineName,
                                    speed = test.speed,
                                    p1 = test.p1,
                                    p1Deviation = test.p1Deviation,
                                    p2 = test.p2,
                                    p2Deviation = test.p2Deviation,
                                    n1 = test.n1,
                                    n1Deviation = test.n1Deviation,
                                    sectionNumber = test.sectionNumber,
                                    totalDrumNumbers = test.totalDrumNumbers,
                                    drumNumber = test.drumNumber,
                                    standardStrength = test.standardStrength,
                                    strengthDeviation = test.strengthDeviation,
                                    belowLimit = test.belowLimit,
                                    totalTestCount = test.totalTestCount,
                                    drumSelectionMethod = test.drumSelectionMethod,
                                    sampleNo = test.sampleNo,
                                    sampleStrengthCount = test.sampleStrengthCount,
                                    isQualified = test.isQualified,
                                    maxRollingCount = test.maxRollingCount,
                                    materialCount = test.materialCount,
                                    shift = test.shift,
                                    scheduledStartDate = test.scheduledStartDate,
                                    scheduledEndDate = test.scheduledEndDate
                                };
                                StrengthTestModelViewlist.Add(strengthTestModelView);
                            }
                            refListView(true);
                        }


                    }
                    else
                    {
                        isTestResume = false;
                    }
                }
                //*************************************************************************************
            }
        }

        private void getUserfieldConfig(string mCat, Guid mid, string mac)
        {
            if (mCat == "" && mid == Guid.Empty && mac == "")
            {
                UFVAL1 = null;
                UFVAL2 = null;
                UFVAL3 = null;
                UFVAL4 = null;
            }
            else
            {
                ConfigModel ycConfig_uf = null;
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    ycConfig_uf = conn.Table<ConfigModel>().
                                Where(ConfigModel => (ConfigModel.uf_name_1 != null ||
                                ConfigModel.uf_name_1 != "") && ConfigModel.machineCategory == mCat
                                && ConfigModel.machineID == mid && ConfigModel.machineName == mac).FirstOrDefault();
                }
                if (ycConfig_uf != null)
                {
                    UFVAL1 = ycConfig_uf.uf_value_1;
                    UFVAL2 = ycConfig_uf.uf_value_2;
                    UFVAL3 = ycConfig_uf.uf_value_3;
                    UFVAL4 = ycConfig_uf.uf_value_4;
                }
            }
        }

        private void populateTestParams(string mCat, Guid mid, string mac)
        {
            if (!isTestCompleted) { hideFrames(); }
            if (mCat == "" && mid == Guid.Empty && mac == "")
            {
                selectedSpeed = 0;
                selectedP1 = 0.0m;
                selectedP1Deviation = 0.0m;
                selectedP2 = 0.0m;
                selectedP2Deviation = 0.0m;
                selectedN1 = 0.0m;
                selectedN1Deviation = 0.0m;
                selectedMaxRollingCount = 0;
                selectedMaterialCount = null;
                scheduledStartDate = DEFAULTDATE;
                scheduledEndDate = DEFAULTDATE;
                settingsUpdatedDate = DEFAULTDATE;
                picker_drumNumber.ItemsSource = null;
                entry_pressure.Text = "";
                entry_stdStrength.Text = "";
                entry_strengthDeviation.Text = "";
                entry_belowLimit.Text = "";
                entry_numberOfTest.Text = "";
                picker_shift.SelectedIndex = 0;
                picker_drumSelection.SelectedIndex = 0;
                return;
            }
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<ConfigModel>();
                ConfigModel yarncountconfigmodel = conn.Table<ConfigModel>().Where(ConfigModel =>
                                                            (ConfigModel.machineCategory == mCat &&
                                                            ConfigModel.machineID == mid &&
                                                            ConfigModel.machineName == mac)).FirstOrDefault();
                if (yarncountconfigmodel != null)
                {

                    entry_pressure.Text = yarncountconfigmodel.speed.ToString() + ", "
                                            + yarncountconfigmodel.p1.ToString() + ", "
                                            + yarncountconfigmodel.p2.ToString() + ", "
                                            + yarncountconfigmodel.n1.ToString();


                    for (int d = int.Parse(yarncountconfigmodel.drumNumbers_s1.Split('.')[0]); d <= int.Parse(yarncountconfigmodel.drumNumbers_s1.Split('.')[1]); d++)
                    {
                        picker_drumNumber.Items.Add((d).ToString());
                    }
                    TimeSpan shit1time = TimeSpan.FromHours(TimeSpan.Parse(yarncountconfigmodel.shift1time).TotalHours);
                    TimeSpan shit2time = TimeSpan.FromHours(TimeSpan.Parse(yarncountconfigmodel.shift2time).TotalHours);
                    TimeSpan shit3time = TimeSpan.FromHours(TimeSpan.Parse(yarncountconfigmodel.shift3time).TotalHours);
                    TimeSpan currentTime = TimeSpan.FromHours(TimeSpan.Parse(DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString()).TotalHours);

                    int duration = 0;

                    if (yarncountconfigmodel.shiftCount == 1)
                    {
                        duration = 24;
                    }
                    else if (yarncountconfigmodel.shiftCount == 2)
                    {
                        duration = 12;
                    }
                    if (yarncountconfigmodel.shiftCount == 3)
                    {
                        duration = 8;
                    }

                    if (getTimeList(shit1time, duration).Contains(currentTime))
                    {
                        picker_shift.SelectedItem = "Shift-1";
                    }
                    else if (getTimeList(shit2time, duration).Contains(currentTime))
                    {
                        picker_shift.SelectedItem = "Shift-2";
                    }
                    else if (getTimeList(shit3time, duration).Contains(currentTime))
                    {
                        picker_shift.SelectedItem = "Shift-3";
                    }


                }
                else
                {
                    DisplayAlert("Settings Alert!!!", "Settings not saved for selected machine (" + mac + ")", "Okay");
                    picker_drumNumber.ItemsSource = null;
                    entry_stdStrength.Text = "";
                    entry_strengthDeviation.Text = "";
                    entry_belowLimit.Text = "";
                    entry_numberOfTest.Text = "";
                    picker_shift.SelectedIndex = 0;
                    picker_drumSelection.SelectedIndex = 0;
                }
            }
        }

        public List<TimeSpan> getTimeList(TimeSpan targetTime, int timeDuration)
        {
            List<TimeSpan> returnTimeList = new List<TimeSpan>();
            for (int i = 0; i < timeDuration; i++)
            {
                int hrs = targetTime.Hours + i;
                if (hrs >= 24)
                {
                    hrs = hrs - 24;
                };
                for (int j = 0; j < 60; j++)
                {//minutes
                    for (int k = 0; k < 60; k++)
                    {//seconds
                        returnTimeList.Add(TimeSpan.Parse(hrs.ToString() + ":" + j.ToString() + ":" + k.ToString()));
                    }
                }
            }
            return returnTimeList;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            disposeble();
            reset();
        }

        private async void UpdateUserNotification(string msg, string color = RED)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lbl_error.TextColor = Color.FromHex(color);
                lbl_error.Text = msg;
            });
        }

        private async void ImageNotification(string src, bool visibility = true)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                img_notification.Source = null;
                img_notification.IsVisible = visibility;
                img_notification.Source = src;
            });
        }

        private async void showAlert(string msg)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("Notice", msg, "Ok");
            });
        }

        private async void hideFrames()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                individualTestResultFrame_FinalOut.IsVisible = false;
                individualTestResultFrame.IsVisible = false;
                frame_overallSummary_FinalOut.IsVisible = false;
                frame_overallSummary.IsVisible = false;
            });
        }

        private async Task refListView(bool visibility = true, bool showFinalOut = false)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (showFinalOut)
                {
                    listview_testresult.ItemsSource = null;
                    listview_testresult.IsVisible = false;
                    individualTestResultFrame.IsVisible = false;

                    individualTestResultFrame_FinalOut.IsVisible = true;
                    listview_testresult_FinalOut.ItemsSource = null;
                    listview_testresult_FinalOut.IsVisible = visibility;
                    listview_testresult_FinalOut.ItemsSource = StrengthTestModelViewlist;

                    //lbl_testresult_Final_stadHank.Text = "Strength (" + selectedStandardStrength.ToString() + "\u00B1" + selectedStrengthDeviation + ")";
                    //span_head.Text = "Strength";
                    //span_stdValue.Text = " (" + STD_HANK.ToString();
                    //span_deviation.Text = " \u00B1" + selectedDeviationPercent + ")";

                }
                else
                {
                    listview_testresult_FinalOut.ItemsSource = null;
                    listview_testresult_FinalOut.IsVisible = false; ;
                    individualTestResultFrame_FinalOut.IsVisible = false;

                    individualTestResultFrame.IsVisible = true;
                    listview_testresult.IsVisible = visibility;
                    if (StrengthTestModelViewlist != null)
                    {
                        listview_testresult.ItemsSource = null;
                        listview_testresult.ItemsSource = StrengthTestModelViewlist.OrderByDescending(StrengthTestModelView => StrengthTestModelView.sampleNo);
                    }

                    //if (selectedMachineCategory == "Spinning" || selectedMachineCategory == "Winding")
                    //{
                    //    //lbl_testresult_stadHank.Text = "Count " + STD_HANK.ToString() + "\u00B1" + selectedDeviationPercent + ")";
                    //    span_head_ind.Text = "Count";
                    //    span_stdValue_ind.Text = " (" + STD_HANK.ToString();
                    //    //span_deviation_ind.Text = " \u00B1" + selectedDeviationPercent + ")";
                    //}
                    //else
                    //{
                    //    //lbl_testresult_stadHank.Text = "Hank " + STD_HANK.ToString() + "\u00B1" + selectedDeviationPercent + ")";
                    //    span_head_ind.Text = "Hank";
                    //    span_stdValue_ind.Text = " (" + STD_HANK.ToString();
                    //    //span_deviation_ind.Text = " \u00B1" + selectedDeviationPercent + ")";
                    //}
                }
            });
        }

        private async Task refOverallSummary(decimal strength = 0m, decimal qulaifiedTest = 0m, bool visibility = true, bool showFinalOut = false)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (showFinalOut)
                {
                    frame_overallSummary_FinalOut.IsVisible = visibility;
                    lbl_strength_FinalOut.Text = strength.ToString();
                    lbl_qualifiedTest_FinalOut.Text = qulaifiedTest.ToString();


                    //decimal maxRangeVal = STD_HANK + selectedDeviationPercent;
                    //decimal minRangeVal = STD_HANK - selectedDeviationPercent;


                    //if (mean < minRangeVal || mean > maxRangeVal)
                    //{
                    //    listview_testresult_FinalOut.BackgroundColor = Color.FromHex("#ffc3c0");
                    //    lbl_average_FinalOut.TextColor = Color.Red;
                    //}
                    //else
                    //{
                    //    listview_testresult_FinalOut.BackgroundColor = Color.White;
                    //    lbl_average_FinalOut.TextColor = Color.DarkSlateGray;
                    //}
                }
                else
                {
                    frame_overallSummary.IsVisible = visibility;
                    lbl_strength.Text = strength.ToString();
                    lbl_qualifiedTest.Text = qulaifiedTest.ToString();
                    //lbl_cv.Text = cv.ToString();
                }
            });
        }


        private string formatTime(DateTime startDateTime)
        {
            TimeSpan duration = (DateTime.Now - startDateTime).Duration();
            string hrs = duration.Hours.ToString();
            if (hrs.Length < 2)
            {
                hrs = "0" + hrs;
            }
            string mins = duration.Minutes.ToString();
            if (mins.Length < 2)
            {
                mins = "0" + mins;
            }
            string sec = duration.Seconds.ToString();
            if (sec.Length < 2)
            {
                sec = "0" + sec;
            }
            //return hrs + "h:" + mins + "m:" + sec + "s";
            return hrs + ":" + mins + ":" + sec;
        }

        private async void updateDB()
        {
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                bool dbStatus = true;
                conn.CreateTable<StrengthTestModel>();

                List<StrengthTestModel> st_list = conn.Table<StrengthTestModel>().Where(
                                                    StrengthTestModel => (StrengthTestModel.testID == currentTestID
                                                    && StrengthTestModel.machineCategory == selectedMachineCategory
                                                    && StrengthTestModel.machineID == selectedMachineID)).ToList();
                if (st_list.Count == 0)
                {
                    ImageNotification("red.png");
                    UpdateUserNotification("Overall Strength Test Data failed to save in database!!!");
                    showAlert("Overall Strength Test Data failed to save in database!!!");
                    return;
                }


                int sum_sampleStrengthCount = 0;
                int qualifiedTest = 0;
                foreach (StrengthTestModel test in st_list)
                {
                    sum_sampleStrengthCount += test.sampleStrengthCount;
                    if (test.sampleStrengthCount >= test.belowLimit)
                    {
                        qualifiedTest++;
                    }
                }

                decimal avg = formatDecimal(decimal.Parse(sum_sampleStrengthCount.ToString()) / decimal.Parse(st_list[0].totalTestCount.ToString()), 2);

                decimal strength = formatDecimal(avg + decimal.Parse(qualifiedTest.ToString()), 2);

                string testDuration = formatTime(currentTestStartTime);

                StrengthTestSummaryModel StrengthTestSummaryModel = new StrengthTestSummaryModel()
                {
                    ID = Guid.NewGuid(),
                    testID = StrengthTestModelViewlist[0].testID,
                    userID = StrengthTestModelViewlist[0].userID,
                    userName = StrengthTestModelViewlist[0].userName,
                    machineID = StrengthTestModelViewlist[0].machineID,
                    machineCategory = StrengthTestModelViewlist[0].machineCategory,
                    machineName = StrengthTestModelViewlist[0].machineName,
                    speed = StrengthTestModelViewlist[0].speed,
                    p1 = StrengthTestModelViewlist[0].p1,
                    p1Deviation = StrengthTestModelViewlist[0].p1Deviation,
                    p2 = StrengthTestModelViewlist[0].p2,
                    p2Deviation = StrengthTestModelViewlist[0].p2Deviation,
                    n1 = StrengthTestModelViewlist[0].n1,
                    n1Deviation = StrengthTestModelViewlist[0].n1Deviation,
                    sectionNumber = StrengthTestModelViewlist[0].sectionNumber,
                    totalDrumNumbers = StrengthTestModelViewlist[0].totalDrumNumbers,
                    drumNumber = StrengthTestModelViewlist[0].drumNumber,
                    standardStrength = StrengthTestModelViewlist[0].standardStrength,
                    strengthDeviation = StrengthTestModelViewlist[0].strengthDeviation,
                    belowLimit = StrengthTestModelViewlist[0].belowLimit,
                    qualifiedTestCount = qualifiedTest,
                    totalTestCount = StrengthTestModelViewlist[0].totalTestCount,
                    drumSelectionMethod = StrengthTestModelViewlist[0].drumSelectionMethod,
                    maxRollingCount = StrengthTestModelViewlist[0].maxRollingCount,
                    materialCount = StrengthTestModelViewlist[0].materialCount,
                    yarnStrength = strength,
                    shift = StrengthTestModelViewlist[0].shift,
                    testDuration = testDuration,
                    uf_value_1 = UFVAL1,
                    uf_value_2 = UFVAL2,
                    uf_value_3 = UFVAL3,
                    uf_value_4 = UFVAL4,
                    scheduledStartDate = scheduledStartDate,
                    scheduledEndDate = scheduledEndDate,
                    settingsUpdatedDate = settingsUpdatedDate,
                    createdate = DateTime.Now
                };

                conn.CreateTable<StrengthTestSummaryModel>();
                int row = conn.Insert(StrengthTestSummaryModel);
                if (row < 1)
                {
                    dbStatus = false;
                }
                else
                {
                    ConfigModel cm = conn.Table<ConfigModel>().Where(ConfigModel =>
                                    (ConfigModel.machineID == selectedMachineID)).FirstOrDefault();

                    if (cm != null)
                    {
                        TestConfigModel tcm = new TestConfigModel()
                        {
                            ID = Guid.NewGuid(),
                            testID = StrengthTestSummaryModel.testID,
                            machineID = cm.machineID,
                            machineCategory = cm.machineCategory,
                            machineName = cm.machineName,
                            speed = cm.speed,
                            p1 = cm.p1,
                            p1Deviation = cm.p1Deviation,
                            p2 = cm.p2,
                            p2Deviation = cm.p2Deviation,
                            n1 = cm.n1,
                            n1Deviation = cm.n1Deviation,
                            totalDrumCount = cm.totalDrumCount,
                            totalSections = cm.totalSections,
                            stdRollingStrength = cm.stdRollingStrength,
                            strengthDeviation = cm.strengthDeviation,
                            belowLimit = cm.belowLimit,
                            maxLimit = cm.maxLimit,
                            totalSamples = cm.totalSamples,
                            scheduledStartDate = cm.scheduledStartDate,
                            scheduledEndDate = cm.scheduledEndDate,
                            materialCount = cm.materialCount,
                            drumNumbers_s1 = cm.drumNumbers_s1,
                            drumNumbers_s2 = cm.drumNumbers_s2,
                            drumNumbers_s3 = cm.drumNumbers_s3,
                            drumNumbers_s4 = cm.drumNumbers_s4,
                            shiftCount = cm.shiftCount,
                            shift1time = cm.shift1time,
                            shift2time = cm.shift2time,
                            shift3time = cm.shift3time,
                            uf_name_1 = cm.uf_name_1,
                            uf_value_1 = cm.uf_value_1,
                            uf_name_2 = cm.uf_name_2,
                            uf_value_2 = cm.uf_value_2,
                            uf_name_3 = cm.uf_name_3,
                            uf_value_3 = cm.uf_value_3,
                            uf_name_4 = cm.uf_name_4,
                            uf_value_4 = cm.uf_value_4,
                            updateddate = cm.updateddate,
                            createdate = cm.createdate,
                        };
                        int row_tcm = conn.Insert(tcm);
                        if (row_tcm <1)
                        {
                            dbStatus = false;
                        }
                    }
                    else
                    {
                        dbStatus = false;
                    }
                }
                if (dbStatus)
                {
                    await refListView(true, true);
                    await refOverallSummary(strength, qualifiedTest , true, true);
                }
                

            }
        }

        private void reset(bool fullreset = true, bool dispose = true)
        {
            try
            {
                current_stable_data = 0;
                
                if (fullreset) { ImageNotification(null); UpdateUserNotification(""); isTestCompleted = true; }
                if (dispose) { disposeble(); }
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (isTestStarted)
                    {
                        if (StrengthTestModelViewlist != null)
                        {
                            if (selectedTotalTestCount != StrengthTestModelViewlist.Count())
                            {
                                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                                {
                                    StrengthTestModel lastTestRecord = conn.Table<StrengthTestModel>()
                                                                        .Where(StrengthTestModel => StrengthTestModel.testID == currentTestID)
                                                                        .OrderByDescending(StrengthTestModel=>StrengthTestModel.sampleNo)
                                                                        .FirstOrDefault();
                                    if (lastTestRecord ==null || (lastTestRecord.totalTestCount != lastTestRecord.sampleNo))
                                    {
                                        //ImageNotification("red.png");
                                        //UpdateUserNotification("IN-COMPLETE TEST!!!");
                                        //showAlert("In-complete Test!!!");
                                        isTestResume = true;
                                        testYCButton.Text = "Resume";
                                        testYCButton.BackgroundColor = Color.IndianRed;
                                        testYCButton.TextColor = Color.White;
                                        testYCButton.IsEnabled = true;
                                    }
                                    else
                                    {
                                        isTestResume = false;
                                    }
                                }
                            }
                            else
                            {
                                isTestResume = false;
                                frame_overallSummary.IsVisible = false;
                                individualTestResultFrame.IsVisible = false;
                                currentTestID = 0;
                                currentTestStartTime = null;
                                showAlert("Test Completed!!! Start new test");
                                picker_drumSelection.IsEnabled = false;
                                testYCButton.Text = "Start";
                                testYCButton.IsEnabled = true;
                                testYCButton.BackgroundColor = Color.Green;
                                //picker_machinecategory.IsEnabled = true;
                                //picker_machinecategory.SelectedIndex = -1;
                                picker_machinename.IsEnabled = true;
                                picker_machinename.SelectedIndex = -1;
                                picker_drumNumber.IsEnabled = true;
                                entry_stdStrength.IsEnabled = false;
                                entry_strengthDeviation.IsEnabled = false;
                                entry_belowLimit.IsEnabled = false;
                                entry_numberOfTest.IsEnabled = true;
                                picker_shift.IsEnabled = false;
                            }
                        }
                        else
                        {
                            //ImageNotification("red.png");
                            //UpdateUserNotification("IMPROPER TEST!!!");
                            //showAlert("Improper Test!!!");
                        }
                        isTestStarted = false;
                        //showAlert("Test Completed!!! Start new test");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        [Obsolete]
        private async Task showToast(string msg)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
               Toast.MakeText(Android.App.Application.Context,
                    Html.FromHtml("<font color='#4AFD02'><b>" + msg + "</b></font>"),
                    ToastLength.Short).Show();
            });
        }

        private async Task showProgress(bool visibility=false)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                act_id.IsVisible = visibility;
                act_id.IsRunning = visibility;
            });
        }


        [Obsolete]
        private async void testYCButton_Clicked(object sender, EventArgs e)
        {
            

            if (!isTestResume)
            {
                hideFrames();
                await refListView(false);
                await refOverallSummary(0.0000m, 0.0000m, false);
            }
            else
            {
                if(StrengthTestModelViewlist != null && StrengthTestModelViewlist.Count > 0)
                {
                    List<StrengthTestModelView> tempList = new List<StrengthTestModelView>();
                    tempList = StrengthTestModelViewlist;
                    StrengthTestModelView toBeDeletedTest = null;
                    int lastSampleNo = tempList.Max(StrengthTestModelView => StrengthTestModelView.sampleNo);
                    foreach(StrengthTestModelView test in tempList)
                    {
                        if(test.sampleNo == lastSampleNo && test.maxRollingCount<test.sampleStrengthCount)
                        {
                            toBeDeletedTest = test;
                        }
                    }
                    if (toBeDeletedTest != null)
                    {
                        StrengthTestModelViewlist.Remove(toBeDeletedTest);
                        await refListView();
                    }
                }
            }

            ImageNotification("null");
            UpdateUserNotification("");

            //showProgress
            CancellationTokenSource src_p = new CancellationTokenSource();
            CancellationToken ct_p = src_p.Token;
            ct_p.Register(() => Debug.WriteLine("Show progress"));
            await Task.Run(async () => await Task.FromResult(showProgress(true)), ct_p);
            src_p.Cancel();
            //showProgress End

            //showToast
            CancellationTokenSource src_t = new CancellationTokenSource();
            CancellationToken ct_t = src_t.Token;
            ct_t.Register(() => Debug.WriteLine("Initializing Test"));
            await Task.Run(async () => await Task.FromResult(showToast("Initializing. Please wait......Do not press START again")), ct_t);
            src_t.Cancel();
            //showToast End

            if (!isTestResume)
            {
                currentTestStartTime = null;
                currentTestStartTime = DateTime.Now;
                lbl_TestID.Text = "";
            }

            isTestStarted = true;
            isTestCompleted = false;

            if (selectedMachineID == Guid.Empty || selectedMachineCategory == null || selectedMachineCategory == "")
            {
                await DisplayAlert("Attention", "Please select machine category/ name to proceed!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (entry_pressure.Text.Trim() == "")
            {
                await DisplayAlert("Attention", "Invaild Speed, P1, P2 and N1!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            else
            {
                string pressureData = entry_pressure.Text.Trim();
                string[] pressureList = pressureData.Split(',');
                if (pressureList.Length != 4)
                {
                    await DisplayAlert("Attention", "Invaild Speed, P1, P2 and N1!!!", "Ok");
                    _ = showProgress(false);
                    return;
                }
                else
                {
                    int.TryParse(pressureList[0].Trim(), out selectedSpeed);
                    if (selectedSpeed <= 0)
                    {
                        await DisplayAlert("Attention", "Invaild Speed!!!", "Ok");
                        _ = showProgress(false);
                        return;
                    }
                    decimal.TryParse(pressureList[1].Trim(), out selectedP1);
                    if (selectedP1 <= 0.0m)
                    {
                        await DisplayAlert("Attention", "Invaild P1!!!", "Ok");
                        _ = showProgress(false);
                        return;
                    }
                    decimal.TryParse(pressureList[2].Trim(), out selectedP2);
                    if (selectedP2 <= 0.0m)
                    {
                        await DisplayAlert("Attention", "Invaild P2!!!", "Ok");
                        _ = showProgress(false);
                        return;
                    }
                    decimal.TryParse(pressureList[3].Trim(), out selectedN1);
                    if (selectedN1 <= 0.0m)
                    {
                        await DisplayAlert("Attention", "Invaild N1!!!", "Ok");
                        _ = showProgress(false);
                        return;
                    }
                }
            }
            if (picker_drumNumber.SelectedIndex < 0)
            {
                await DisplayAlert("Attention", "Please select drum number!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (entry_stdStrength.Text.Trim().Contains("-"))
            {
                await DisplayAlert("Attention", "Standard Strength should not be a negative value!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (entry_stdStrength.Text.Trim() == "" || decimal.Parse(entry_stdStrength.Text.Trim()) == 0)
            {
                await DisplayAlert("Attention", "Standard Strength should not be blank or zero!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (entry_strengthDeviation.Text.Trim().Contains("-"))
            {
                await DisplayAlert("Attention", "Strength Deviation should not be a negative value!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (entry_strengthDeviation.Text.Trim() == "" || decimal.Parse(entry_strengthDeviation.Text.Trim()) == 0)
            {
                await DisplayAlert("Attention", "Strength Deviation should not be blank or zero!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (entry_belowLimit.Text.Trim() == "")
            {
                await DisplayAlert("Attention", "Invalid Min & Max limit!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            else {
                string givenMinMaxLimit = entry_belowLimit.Text.Trim();
                string[] limitList = givenMinMaxLimit.Split('&');
                if (limitList.Length != 2)
                {
                    await DisplayAlert("Attention", "Invalid Min & Max limit!!!", "Ok");
                    _ = showProgress(false);
                    return;
                }
                else
                {
                    int.TryParse(limitList[0].Trim(), out selectedBelowLimit);
                    int.TryParse(limitList[1].Trim(), out selectedMaxRollingCount);
                    if (selectedBelowLimit<=0)
                    {
                        await DisplayAlert("Attention", "Invalid Minimum limit!!!", "Ok");
                        _ = showProgress(false);
                        return;
                    }
                    if (selectedMaxRollingCount <= 0)
                    {
                        await DisplayAlert("Attention", "Invalid Maximum limit!!!", "Ok");
                        _ = showProgress(false);
                        return;
                    }
                    if (selectedMaxRollingCount < selectedBelowLimit)
                    {
                        await DisplayAlert("Attention", "Maximum limit should always greater than Minimum limit!!!", "Ok");
                        _ = showProgress(false);
                        return;
                    }
                }
            }
            if (entry_numberOfTest.Text.Trim().Contains(".") || entry_numberOfTest.Text.Trim().Contains("-"))
            {
                await DisplayAlert("Attention", "No. Of Test should not be a decimal or negative value!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (entry_numberOfTest.Text.Trim() == "" || int.Parse(entry_numberOfTest.Text.Trim()) == 0)
            {
                await DisplayAlert("Attention", "No. Of Test should not be blank or zero!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (picker_drumSelection.SelectedIndex < 0)
            {
                await DisplayAlert("Attention", "Invalid drum selection!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (picker_shift.SelectedIndex < 0)
            {
                await DisplayAlert("Attention", "Please select shift!!!", "Ok");
                _ = showProgress(false);
                return;
            }

           

            //showToast
            src_t = new CancellationTokenSource();
            ct_t = src_t.Token;
            ct_t.Register(() => Debug.WriteLine("Initializing Test"));
            await Task.Run(async () => await Task.FromResult(showToast("Checking communication. Please wait......Do not press START again")), ct_t);
            src_t.Cancel();
            //showToast End

            if (!initializeBluetooth())
            {
                ImageNotification("red.png");
                UpdateUserNotification("COMMUNICATION ERROR!!!");
                _ = showProgress(false);
                return;
            }



            string testCount_str = entry_numberOfTest.Text;
            int testCount = int.Parse(testCount_str);
            if (testYCButton.Text != "Resume") {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    StrengthTestModel lastTestRecord = null;
                    conn.CreateTable<StrengthTestModel>();
                    int recordCount = conn.Table<StrengthTestModel>().Count();

                    if (recordCount == 0)
                    {
                        currentTestID = 1;
                    }
                    else
                    {
                        DateTime maxDate = conn.Table<StrengthTestModel>().Max(StrengthTestModel => StrengthTestModel.createdate);
                        if (DateTime.Now <= maxDate)
                        {
                            await DisplayAlert("Attention", "Tablet date time was modified. Please change it to actual current date and time to proceed!!!", "OK");
                            _ = showProgress(false);
                            return;
                        }
                        lastTestRecord = conn.Table<StrengthTestModel>()
                            .Where(StrengthTestModel => StrengthTestModel.createdate == maxDate).FirstOrDefault();
                        if (lastTestRecord != null)
                        {
                            if (currentTestID == 0)
                            {
                                currentTestID = lastTestRecord.testID + 1;
                            }
                            else if (currentTestID == lastTestRecord.testID)
                            {
                                currentTestID = lastTestRecord.testID;
                            }
                        }
                        else
                        {
                            ///to be decided
                        }
                    }
                }
                lbl_TestID.Text = currentTestID.ToString();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    UserModel loggedInUser = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                    if (loggedInUser == null)
                    {
                        await DisplayAlert("Attention", "Unable to get logged user information!!!", "OK");
                        _ = showProgress(false);
                        return;
                    }
                    else
                    {
                        currentloggedInUser = loggedInUser;
                    }
                }
            }


            selectedDrumNumber = int.Parse(picker_drumNumber.SelectedItem.ToString());
            selectedStandardStrength = decimal.Parse(entry_stdStrength.Text);
            selectedStrengthDeviation = decimal.Parse(entry_strengthDeviation.Text);
            //selectedBelowLimit = int.Parse(entry_belowLimit.Text);
            selectedTotalTestCount = int.Parse(entry_numberOfTest.Text);
            selectedDrumSelectionMethod = picker_drumSelection.SelectedItem.ToString();
            selectedShift = picker_shift.SelectedItem.ToString();
            if (testYCButton.Text != "Resume") { StrengthTestModelViewlist = new List<StrengthTestModelView>(); }
            testYCButton.IsEnabled = false;
            testYCButton.BackgroundColor = Color.SlateGray;

            
            picker_machinename.IsEnabled = false;
            picker_drumNumber.IsEnabled = false;
            entry_stdStrength.IsEnabled = false;
            entry_strengthDeviation.IsEnabled = false;
            entry_belowLimit.IsEnabled = false;
            entry_numberOfTest.IsEnabled = false;
            picker_shift.IsEnabled = false;
            picker_drumSelection.IsEnabled = false;

            //showToast
            src_t = new CancellationTokenSource();
            ct_t = src_t.Token;
            ct_t.Register(() => Debug.WriteLine("Initializing Test"));
            await Task.Run(async () => await Task.FromResult(showToast("Reading data......Do not press START again")), ct_t);
            src_t.Cancel();
            //showToast End

            _ = showProgress(false);

            CancellationTokenSource src = new CancellationTokenSource();
            CancellationToken ct = src.Token;
            ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
            await Task.Run(async () => await HandleTest(testCount), ct);
            src.Cancel();
        }

        [Obsolete]
        private async Task HandleTest(int testCount)
        {
            try
            {
                //ImageNotification("loading.gif");
                bool runResult = false;
                int passCount = 0;
                int loopStartNo = 0;
                if (isTestResume) { loopStartNo = StrengthTestModelViewlist.Count; passCount= StrengthTestModelViewlist.Count; }
                for (int i = loopStartNo; i < testCount; i++)
                {
                    currentTestCount = i + 1;
                    runResult = false;
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await RunTest(), ct).ContinueWith((t) =>
                    {
                        t.Wait();
                        if (t.IsFaulted)
                        {
                            runResult = false;
                        };
                        if (t.IsCompleted)
                        {
                            runResult = t.Result;
                        };
                    });
                    src.Cancel();
                    if (runResult)
                    {
                       
                        passCount++;
                        string displayusername = currentloggedInUser.firstname + " [" + currentloggedInUser.userId + "]";
                        if (currentloggedInUser.firstname != "")
                        {
                            displayusername = currentloggedInUser.firstname + ", " + currentloggedInUser.lastname + " [" + currentloggedInUser.userId + "]";
                        }

                        string isQualified = "Yes";

                        if (current_stable_data < selectedBelowLimit) { isQualified = "No"; }


                        StrengthTestModelView strengthTestModelView = new StrengthTestModelView()
                        {
                            testID = currentTestID,
                            userID = currentloggedInUser.ID,
                            userName = displayusername,
                            machineID = selectedMachineID,
                            machineCategory = selectedMachineCategory,
                            machineName = selectedMachineName,
                            speed = selectedSpeed,
                            p1=selectedP1,
                            p1Deviation=selectedP1Deviation,
                            p2=selectedP2,
                            p2Deviation=selectedP2Deviation,
                            n1 = selectedN1,
                            n1Deviation=selectedN1Deviation,
                            sectionNumber = selectedSectionNumber,
                            totalDrumNumbers = selectedTotalDrumNumbers,
                            drumNumber = selectedDrumNumber,
                            standardStrength = selectedStandardStrength,
                            strengthDeviation = selectedStrengthDeviation,
                            belowLimit = selectedBelowLimit,
                            totalTestCount = selectedTotalTestCount,
                            drumSelectionMethod=selectedDrumSelectionMethod,
                            sampleNo = i + 1,
                            sampleStrengthCount = current_stable_data,
                            isQualified = isQualified,
                            maxRollingCount = selectedMaxRollingCount,
                            materialCount = selectedMaterialCount,
                            shift = selectedShift,
                            scheduledStartDate = scheduledStartDate,
                            scheduledEndDate = scheduledEndDate
                        };
                        StrengthTestModelViewlist.Add(strengthTestModelView);

                        if (selectedMaxRollingCount < current_stable_data)
                        {
                            await refListView();
                            ImageNotification("red.png");
                            UpdateUserNotification("Hardware Error. Please contact manufacturer");
                            Debug.WriteLine("Hardware Error. Please contact manufacturer");
                            reset(false);
                            return;
                        }

                        //showAlert("Test - [" + (i + 1) + "] Completed!!! [" + current_stable_data + "]");

                        

                        StrengthTestModel strengthTestModel = new StrengthTestModel()
                        {
                            ID = Guid.NewGuid(),
                            testID = currentTestID,
                            userID = currentloggedInUser.ID,
                            userName = displayusername,
                            machineID = selectedMachineID,
                            machineCategory = selectedMachineCategory,
                            machineName = selectedMachineName,
                            speed = selectedSpeed,
                            p1 = selectedP1,
                            p1Deviation=selectedP1Deviation,
                            p2 = selectedP2,
                            p2Deviation=selectedP2Deviation,
                            n1 = selectedN1,
                            n1Deviation=selectedN1Deviation,
                            sectionNumber = selectedSectionNumber,
                            totalDrumNumbers = selectedTotalDrumNumbers,
                            drumNumber = selectedDrumNumber,
                            standardStrength = selectedStandardStrength,
                            strengthDeviation = selectedStrengthDeviation,
                            belowLimit = selectedBelowLimit,
                            totalTestCount = selectedTotalTestCount,
                            drumSelectionMethod = selectedDrumSelectionMethod,
                            sampleNo = i + 1,
                            sampleStrengthCount = current_stable_data,
                            maxRollingCount = selectedMaxRollingCount,
                            materialCount = selectedMaterialCount,
                            isQualified = isQualified,
                            shift = selectedShift,
                            scheduledStartDate=scheduledStartDate,
                            scheduledEndDate=scheduledEndDate,
                            settingsUpdatedDate=settingsUpdatedDate,
                            createdate = DateTime.Now
                        };
                        int row = 0;
                        using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                        {
                            row= conn.Insert(strengthTestModel);
                            if (row < 1)
                            {
                                showAlert("Test - [" + (i + 1) + "] failed to save in database!!! Please re-start the test!!!");
                                reset(false);
                                break;
                            }
                            else
                            {
                                if (StrengthTestModelViewlist.Count > 0 && passCount < testCount) { 
                                    await refListView();
                                    ImageNotification("yellow.png");
                                    UpdateUserNotification("Waiting for start command", "#FFBF00");
                                    Debug.WriteLine("Waiting for start command");
                                }
                            }
                        }
                        
                    }
                    else
                    {
                        //showAlert("Test - [" + (i + 1) + "] Failed!!! Please start test from begining!!!");
                        reset(false);
                        break;
                    }
                }

                if (StrengthTestModelViewlist.Count > 0 && passCount == testCount)
                {
                    updateDB();
                }
                else
                {
                    if (passCount > 0)
                    {
                        await refListView();
                        await refOverallSummary();
                    }
                }
                if (runResult)
                {
                    reset();
                }
                else
                {
                    reset(false);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                if (ex.ToString().Contains("SQLite")) { showAlert("Database error!!!"); }
                else
                {
                    showAlert("COMMUNICATION ERROR!!!");
                }
                reset();
            }
        }

        private async Task<bool> RunTest()
        {
            try
            {
                string balOutput = Listen();
                Debug.WriteLine("Recieved from Bluetooth adapter is [" + balOutput + "]");
                if (balOutput != "")
                {
                   if (balOutput == "fail")
                    {
                        ImageNotification("red.png");
                        UpdateUserNotification("COMMUNICATION ERROR!!!");
                        Debug.WriteLine("Read data failed");
                        return false;
                    }
                    else
                    {
                        string s_op = balOutput;

                        if (s_op == "ITO")
                        {
                            ImageNotification("red.png");
                            UpdateUserNotification("START COMMAND NOT RECEIVED");
                            Debug.WriteLine("Start command not received!!!");
                            return false;
                        }
                        else if (s_op == "PNR")
                        {
                            ImageNotification("red.png");
                            UpdateUserNotification("PULSE NOT RECEIVED");
                            Debug.WriteLine("Pulse not received!!!");
                            return false;
                        }
                        else if (s_op.Contains("C"))
                        {
                            int parseOut = 0;
                            int.TryParse(s_op.Split('C')[1], out parseOut);
                            current_stable_data = parseOut;
                            ImageNotification(null);
                            UpdateUserNotification("");
                            return true;
                        }
                        else
                        {
                            ImageNotification("red.png");
                            UpdateUserNotification("INVALID COMMAND");
                            Debug.WriteLine("Invalid command received!!!");
                            return false;
                        }
                    }
                }
                else
                {
                    ImageNotification("red.png");
                    UpdateUserNotification("UNSTABLE DATA!!!");
                    Debug.WriteLine("Data is unstable!!! Ensure weighing machine is covered properly");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        private void disposeble()
        {
            try
            {
                if (_socket != null) { _socket.Close(); _socket.Dispose(); }
                if (device != null) { device.Dispose(); }
                if (adapter != null) { adapter.Dispose(); }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception handled!!!! Error:" + ex.Message.ToString());
            }
        }

        [Obsolete]
        public bool initializeBluetooth()
        {
            try
            {
                adapter = BluetoothAdapter.DefaultAdapter;

                if (adapter == null)
                {
                    Debug.WriteLine("No Bluetooth adapter found.");
                    return false;
                }

                if (!adapter.IsEnabled)
                {
                    Debug.WriteLine("Bluetooth adapter is not enabled.");
                    adapter.Enable();
                    Debug.WriteLine("Bluetooth is turned on!!!!");
                }



                device = (from bd in adapter.BondedDevices
                          where bd.Name == runConfiguration.getBalanceSerialNo()
                          select bd).FirstOrDefault();


                if (device == null)
                {
                    Debug.WriteLine("Named device not found.");
                    return false;
                }

                _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                _socket.Connect();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return false;
            }
        }


        private string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '-')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private string Listen()
        {
            string op = "";
            string prevop = "";
            bool startSignalReceived = false;
            bool Listening = true;
            Debug.WriteLine("Listening has been started.");
            int bufferfailedcount = 0;
            int WAITFORSTART = 600;
            int IdealCount = 0;
            while (Listening)
            {
                try
                {
                    var buffer = new BufferedReader(new InputStreamReader(_socket.InputStream));
                    System.Threading.Thread.Sleep(100);
                    if (buffer.Ready())
                    {
                        bufferfailedcount = 0;
                        op = RemoveSpecialCharacters(buffer.ReadLine());
                        Debug.WriteLine("Output: " + op);
                        if (op == "ID")
                        {
                            IdealCount++;
                            if (IdealCount > WAITFORSTART) { return "ITO"; } //Ideal Time Out
                            ImageNotification("yellow.png");
                            UpdateUserNotification("Waiting for start command", "#FFBF00");
                            Debug.WriteLine("Waiting for start command");
                            continue;
                        }
                        if (op == "ST")
                        {
                            //if (startSignalReceived)
                            //{
                            //    ImageNotification("yellow.png");
                            //    UpdateUserNotification("Waiting for pulse", "#DEA808");
                            //    Debug.WriteLine("Waiting for pulse");
                            //    continue;
                            //}
                            //else
                            //{
                                startSignalReceived = true;
                                ImageNotification("green.png");
                                UpdateUserNotification("Start command received", "#008000");
                                Debug.WriteLine("Start command received");
                                continue;
                            //}
                        }
                        if (startSignalReceived && op.Contains('C'))
                        {
                            prevop = op;
                            ImageNotification("blue.png");
                            UpdateUserNotification("Reading pulse, please wait...", "#0e0273");
                            Debug.WriteLine("Reading pulse, please wait...");
                            continue;
                        }
                        if(startSignalReceived && op == "TE")
                        {
                            if (prevop == "") { return "PNR"; } //Pulse Not Received
                            else { return prevop; }
                        }
                    }
                    else
                    {
                        if (bufferfailedcount > BUFFER_WAIT_COUNT)
                        {
                            Debug.WriteLine("Buffer is not ready!!!");
                            return "fail";
                        }
                        else { bufferfailedcount++; }
                    }
                   
                }
                catch (Java.IO.IOException e)
                {
                    Debug.WriteLine("Error: " + e.Message);
                    Listening = false;
                    return "fail";
                }
            }
            Debug.WriteLine("Listening has ended....");
            return op;
        }

        private void picker_machinecategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                hideFrames();
                picker_drumNumber.SelectedIndex = -1;
                picker_drumNumber.Items.Clear();
                selectedMachineCategory = "";
                if (picker_machinecategory.SelectedIndex > 0)
                {
                    selectedMachineCategory = picker_machinecategory.SelectedItem.ToString();
                }
                if (selectedMachineCategory == "" || selectedMachineCategory == null)
                {
                    picker_machinename.ItemsSource = null;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<MachineModel>();
                    List<MachineModel> machineModelList = conn.Table<MachineModel>().Where(MachineModel => MachineModel.machineCategory == selectedMachineCategory).ToList();
                    picker_machinename.ItemsSource = machineModelList;
                }
                populateTestParams("", Guid.Empty, "");
                getUserfieldConfig("", Guid.Empty, "");
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Machine Category - Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }

        private async void picker_machinename_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                entry_pressure.Text = "";

                picker_drumNumber.SelectedIndex = -1;
                picker_drumNumber.Items.Clear();
                
                if (picker_machinename.SelectedIndex < 0)
                {
                    selectedMachineID = Guid.Empty;
                    selectedMachineName = null;
                    populateTestParams("", Guid.Empty, "");
                    return;
                }
                List<MachineModel> source = (List<MachineModel>)picker_machinename.ItemsSource;
                selectedMachineID = (Guid)source[picker_machinename.SelectedIndex].ID;
                MachineModel selectedMachine = (MachineModel)picker_machinename.SelectedItem;
                selectedMachineName = selectedMachine.machineName;
                populateTestParams(selectedMachineCategory, selectedMachineID, selectedMachineName);
                getUserfieldConfig(selectedMachineCategory, selectedMachineID, selectedMachineName);
                if (!isTestResume)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                    {
                        int macRecordForToday = conn.Table<StrengthTestSummaryModel>().Where(StrengthTestSummaryModel =>
                                                (StrengthTestSummaryModel.createdate >= DateTime.Now.Date
                                                && StrengthTestSummaryModel.machineID == selectedMachineID)).Count();
                        if (macRecordForToday == 0)
                        {
                            await getPressureConfirmation();
                            if (entry_pressure.Text == "")
                            {
                                picker_machinename.SelectedIndex = -1;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Attention", "Machine Name - Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }

        private decimal formatDecimal(decimal inputVal, int afterDecimalCount = 4)
        {
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

        private async void btn_comment_Clicked(object sender, EventArgs e)
        {
            try
            {
                string header = "Note [Test ID : " + lbl_TestID.Text + "]";
                long testID = long.Parse(lbl_TestID.Text.ToString());
                string comment = await DisplayPromptAsync(header, "Please type your remark", "Save", "Discard", null, 100);
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    StrengthTestSummaryModel summaryModel = conn.Table<StrengthTestSummaryModel>().Where(
                        StrengthTestSummaryModel => StrengthTestSummaryModel.testID == testID).FirstOrDefault();
                    if (summaryModel != null)
                    {
                        summaryModel.testRemark = comment;
                        int row = conn.Update(summaryModel);
                        if (row < 1)
                        {
                            await DisplayAlert("Attention!!!", "Unable to save test remark. Please try again!!!", "Ok");
                        }
                        else
                        {
                            await DisplayAlert("Success!!!", "Test remark added successfully!!!", "Ok");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Attention!!!", "Unable to save test remark. Please try again!!!", "Ok");
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }

        private async void picker_drumNumber_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                
                if (picker_drumNumber.SelectedIndex==-1 || picker_drumNumber.SelectedItem.ToString() == "" || picker_drumNumber.SelectedItem.ToString() == null)
                {
                    selectedTotalDrumNumbers = "";
                    selectedSectionNumber = 0;
                    scheduledStartDate = DEFAULTDATE;
                    scheduledEndDate = DEFAULTDATE;
                    settingsUpdatedDate = DEFAULTDATE;
                    entry_stdStrength.Text = "";
                    entry_strengthDeviation.Text = "";
                    entry_belowLimit.Text = "";
                    entry_numberOfTest.Text = "";
                    picker_drumSelection.SelectedIndex = 0;
                    return;
                }

                if (entry_pressure.Text.Trim() == "")
                {
                    await DisplayAlert("Attention!!!", "Please add Speed, P1, P2 and N1 to proceed!!!", "Ok");
                    picker_drumNumber.SelectedIndex = -1;
                    selectedTotalDrumNumbers = "";
                    selectedSectionNumber = 0;
                    scheduledStartDate = DEFAULTDATE;
                    scheduledEndDate = DEFAULTDATE;
                    settingsUpdatedDate = DEFAULTDATE;
                    entry_stdStrength.Text = "";
                    entry_strengthDeviation.Text = "";
                    entry_belowLimit.Text = "";
                    entry_numberOfTest.Text = "";
                    picker_drumSelection.SelectedIndex = 0;
                    return;
                }

                selectedDrumNumber = int.Parse(picker_drumNumber.SelectedItem.ToString());
                DateTime startDate = DEFAULTDATE;
                DateTime endDate = DEFAULTDATE;

                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<ConfigModel>();
                    ConfigModel yarncountconfigmodel = conn.Table<ConfigModel>().Where(ConfigModel =>
                                                                (ConfigModel.machineCategory == selectedMachineCategory &&
                                                                ConfigModel.machineID == selectedMachineID &&
                                                                ConfigModel.machineName == selectedMachineName)).FirstOrDefault();
                    if (yarncountconfigmodel != null)
                    {
                        if (yarncountconfigmodel.totalSections == 4)
                        {
                            int sec1_lowerLimit =int.Parse(yarncountconfigmodel.drumNumbers_s1.ToString().Split('.')[0]);
                            int sec1_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s1.ToString().Split('.')[1]);

                            int sec2_lowerLimit = int.Parse(yarncountconfigmodel.drumNumbers_s2.ToString().Split('.')[0]);
                            int sec2_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s2.ToString().Split('.')[1]);

                            int sec3_lowerLimit = int.Parse(yarncountconfigmodel.drumNumbers_s3.ToString().Split('.')[0]);
                            int sec3_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s3.ToString().Split('.')[1]);

                            int sec4_lowerLimit = int.Parse(yarncountconfigmodel.drumNumbers_s4.ToString().Split('.')[0]);
                            int sec4_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s4.ToString().Split('.')[1]);

                            if (selectedDrumNumber>=sec1_lowerLimit && selectedDrumNumber <= sec1_upperLimit)
                            {
                                selectedSectionNumber = 1;
                                selectedTotalDrumNumbers = yarncountconfigmodel.drumNumbers_s1;
                                scheduledStartDate = yarncountconfigmodel.scheduledStartDate;
                                scheduledEndDate = yarncountconfigmodel.scheduledEndDate;
                                settingsUpdatedDate = yarncountconfigmodel.updateddate;

                                selectedSpeed = yarncountconfigmodel.speed;
                                selectedP1 = yarncountconfigmodel.p1;
                                selectedP1Deviation = yarncountconfigmodel.p1Deviation;
                                selectedP2 = yarncountconfigmodel.p2;
                                selectedP2Deviation = yarncountconfigmodel.p2Deviation;
                                selectedN1 = yarncountconfigmodel.n1;
                                selectedN1Deviation = yarncountconfigmodel.n1Deviation;

                                selectedBelowLimit = yarncountconfigmodel.belowLimit;
                                selectedMaxRollingCount = yarncountconfigmodel.maxLimit;
                                selectedMaterialCount = yarncountconfigmodel.materialCount;

                                if(DateTime.Now.Date > scheduledEndDate && isTestResume ==false)
                                {
                                    picker_drumNumber.SelectedIndex = -1;
                                    picker_drumSelection.IsEnabled = false;
                                    DisplayAlert("Attention", "The scheduled date is expired for the selected drum ["+
                                        selectedDrumNumber.ToString()+"]. Please reach admin to change the machine settings", "OK");
                                    return;
                                }

                                startDate = yarncountconfigmodel.scheduledStartDate;
                                endDate = yarncountconfigmodel.scheduledEndDate;
                                entry_stdStrength.Text = yarncountconfigmodel.stdRollingStrength.ToString();
                                entry_strengthDeviation.Text = yarncountconfigmodel.strengthDeviation.ToString();
                                entry_belowLimit.Text = yarncountconfigmodel.belowLimit.ToString() + " & " + yarncountconfigmodel.maxLimit.ToString();
                                entry_numberOfTest.Text = yarncountconfigmodel.totalSamples.ToString();
                                picker_drumSelection.SelectedItem = "Scheduled";
                            }

                            if (selectedDrumNumber >= sec2_lowerLimit && selectedDrumNumber <= sec2_upperLimit)
                            {
                                selectedSectionNumber = 2;
                                selectedTotalDrumNumbers = yarncountconfigmodel.drumNumbers_s2;
                                scheduledStartDate = yarncountconfigmodel.scheduledStartDate;
                                scheduledEndDate = yarncountconfigmodel.scheduledEndDate;
                                settingsUpdatedDate = yarncountconfigmodel.updateddate;

                                selectedSpeed = yarncountconfigmodel.speed;
                                selectedP1 = yarncountconfigmodel.p1;
                                selectedP1Deviation = yarncountconfigmodel.p1Deviation;
                                selectedP2 = yarncountconfigmodel.p2;
                                selectedP2Deviation = yarncountconfigmodel.p2Deviation;
                                selectedN1 = yarncountconfigmodel.n1;
                                selectedN1Deviation = yarncountconfigmodel.n1Deviation;

                                selectedBelowLimit = yarncountconfigmodel.belowLimit;
                                selectedMaxRollingCount = yarncountconfigmodel.maxLimit;
                                selectedMaterialCount = yarncountconfigmodel.materialCount;

                                if (DateTime.Now.Date > scheduledEndDate && isTestResume == false)
                                {
                                    picker_drumNumber.SelectedIndex = -1;
                                    picker_drumSelection.IsEnabled = false;
                                    DisplayAlert("Attention", "The scheduled date is expired for the selected drum [" +
                                        selectedDrumNumber.ToString() + "]. Please reach admin to change the machine settings", "OK");
                                    return;
                                }

                                startDate = yarncountconfigmodel.scheduledStartDate;
                                endDate = yarncountconfigmodel.scheduledEndDate;
                                entry_stdStrength.Text = yarncountconfigmodel.stdRollingStrength.ToString();
                                entry_strengthDeviation.Text = yarncountconfigmodel.strengthDeviation.ToString();
                                entry_belowLimit.Text = yarncountconfigmodel.belowLimit.ToString() + " & " + yarncountconfigmodel.maxLimit.ToString();
                                entry_numberOfTest.Text = yarncountconfigmodel.totalSamples.ToString();
                                picker_drumSelection.SelectedItem = "Scheduled";
                            }

                            if (selectedDrumNumber >= sec3_lowerLimit && selectedDrumNumber <= sec3_upperLimit)
                            {
                                selectedSectionNumber = 3;
                                selectedTotalDrumNumbers = yarncountconfigmodel.drumNumbers_s3;
                                scheduledStartDate = yarncountconfigmodel.scheduledStartDate;
                                scheduledEndDate = yarncountconfigmodel.scheduledEndDate;
                                settingsUpdatedDate = yarncountconfigmodel.updateddate;

                                selectedSpeed = yarncountconfigmodel.speed;
                                selectedP1 = yarncountconfigmodel.p1;
                                selectedP1Deviation = yarncountconfigmodel.p1Deviation;
                                selectedP2 = yarncountconfigmodel.p2;
                                selectedP2Deviation = yarncountconfigmodel.p2Deviation;
                                selectedN1 = yarncountconfigmodel.n1;
                                selectedN1Deviation = yarncountconfigmodel.n1Deviation;

                                selectedBelowLimit = yarncountconfigmodel.belowLimit;
                                selectedMaxRollingCount = yarncountconfigmodel.maxLimit;
                                selectedMaterialCount = yarncountconfigmodel.materialCount;

                                if (DateTime.Now.Date > scheduledEndDate && isTestResume==false)
                                {
                                    picker_drumNumber.SelectedIndex = -1;
                                    picker_drumSelection.IsEnabled = false;
                                    DisplayAlert("Attention", "The scheduled date is expired for the selected drum [" +
                                        selectedDrumNumber.ToString() + "]. Please reach admin to change the machine settings", "OK");
                                    return;
                                }

                                startDate = yarncountconfigmodel.scheduledStartDate;
                                endDate = yarncountconfigmodel.scheduledEndDate;
                                entry_stdStrength.Text = yarncountconfigmodel.stdRollingStrength.ToString();
                                entry_strengthDeviation.Text = yarncountconfigmodel.strengthDeviation.ToString();
                                entry_belowLimit.Text = yarncountconfigmodel.belowLimit.ToString() + " & " + yarncountconfigmodel.maxLimit.ToString();
                                entry_numberOfTest.Text = yarncountconfigmodel.totalSamples.ToString();
                                picker_drumSelection.SelectedItem = "Scheduled";
                            }

                            if (selectedDrumNumber >= sec4_lowerLimit && selectedDrumNumber <= sec4_upperLimit)
                            {
                                selectedSectionNumber = 4;
                                selectedTotalDrumNumbers = yarncountconfigmodel.drumNumbers_s4;
                                scheduledStartDate = yarncountconfigmodel.scheduledStartDate;
                                scheduledEndDate = yarncountconfigmodel.scheduledEndDate;
                                settingsUpdatedDate = yarncountconfigmodel.updateddate;

                                selectedSpeed = yarncountconfigmodel.speed;
                                selectedP1 = yarncountconfigmodel.p1;
                                selectedP1Deviation = yarncountconfigmodel.p1Deviation;
                                selectedP2 = yarncountconfigmodel.p2;
                                selectedP2Deviation = yarncountconfigmodel.p2Deviation;
                                selectedN1 = yarncountconfigmodel.n1;
                                selectedN1Deviation = yarncountconfigmodel.n1Deviation;

                                selectedBelowLimit = yarncountconfigmodel.belowLimit;
                                selectedMaxRollingCount = yarncountconfigmodel.maxLimit;
                                selectedMaterialCount = yarncountconfigmodel.materialCount;

                                if (DateTime.Now.Date > scheduledEndDate && isTestResume == false)
                                {
                                    picker_drumNumber.SelectedIndex = -1;
                                    picker_drumSelection.IsEnabled = false;
                                    DisplayAlert("Attention", "The scheduled date is expired for the selected drum [" +
                                        selectedDrumNumber.ToString() + "]. Please reach admin to change the machine settings", "OK");
                                    return;
                                }

                                startDate = yarncountconfigmodel.scheduledStartDate;
                                endDate = yarncountconfigmodel.scheduledEndDate;
                                entry_stdStrength.Text = yarncountconfigmodel.stdRollingStrength.ToString();
                                entry_strengthDeviation.Text = yarncountconfigmodel.strengthDeviation.ToString();
                                entry_belowLimit.Text = yarncountconfigmodel.belowLimit.ToString() + " & " + yarncountconfigmodel.maxLimit.ToString();
                                entry_numberOfTest.Text = yarncountconfigmodel.totalSamples.ToString();
                                picker_drumSelection.SelectedItem = "Scheduled";
                            }
                        }
                        else if (yarncountconfigmodel.totalSections == 3)
                        {
                            int sec1_lowerLimit = int.Parse(yarncountconfigmodel.drumNumbers_s1.ToString().Split('.')[0]);
                            int sec1_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s1.ToString().Split('.')[1]);

                            int sec2_lowerLimit = int.Parse(yarncountconfigmodel.drumNumbers_s2.ToString().Split('.')[0]);
                            int sec2_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s2.ToString().Split('.')[1]);

                            int sec3_lowerLimit = int.Parse(yarncountconfigmodel.drumNumbers_s3.ToString().Split('.')[0]);
                            int sec3_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s3.ToString().Split('.')[1]);

                            if (selectedDrumNumber >= sec1_lowerLimit && selectedDrumNumber <= sec1_upperLimit)
                            {
                                selectedSectionNumber = 1;
                                selectedTotalDrumNumbers = yarncountconfigmodel.drumNumbers_s1;
                                scheduledStartDate = yarncountconfigmodel.scheduledStartDate;
                                scheduledEndDate = yarncountconfigmodel.scheduledEndDate;
                                settingsUpdatedDate = yarncountconfigmodel.updateddate;

                                selectedSpeed = yarncountconfigmodel.speed;
                                selectedP1 = yarncountconfigmodel.p1;
                                selectedP1Deviation = yarncountconfigmodel.p1Deviation;
                                selectedP2 = yarncountconfigmodel.p2;
                                selectedP2Deviation = yarncountconfigmodel.p2Deviation;
                                selectedN1 = yarncountconfigmodel.n1;
                                selectedN1Deviation = yarncountconfigmodel.n1Deviation;

                                selectedBelowLimit = yarncountconfigmodel.belowLimit;
                                selectedMaxRollingCount = yarncountconfigmodel.maxLimit;
                                selectedMaterialCount = yarncountconfigmodel.materialCount;

                                if (DateTime.Now.Date > scheduledEndDate && isTestResume == false)
                                {
                                    picker_drumNumber.SelectedIndex = -1;
                                    picker_drumSelection.IsEnabled = false;
                                    DisplayAlert("Attention", "The scheduled date is expired for the selected drum [" +
                                        selectedDrumNumber.ToString() + "]. Please reach admin to change the machine settings", "OK");
                                    return;
                                }

                                startDate = yarncountconfigmodel.scheduledStartDate;
                                endDate = yarncountconfigmodel.scheduledEndDate;
                                entry_stdStrength.Text = yarncountconfigmodel.stdRollingStrength.ToString();
                                entry_strengthDeviation.Text = yarncountconfigmodel.strengthDeviation.ToString();
                                entry_belowLimit.Text = yarncountconfigmodel.belowLimit.ToString() + " & " + yarncountconfigmodel.maxLimit.ToString();
                                entry_numberOfTest.Text = yarncountconfigmodel.totalSamples.ToString();
                                picker_drumSelection.SelectedItem = "Scheduled";
                            }

                            if (selectedDrumNumber >= sec2_lowerLimit && selectedDrumNumber <= sec2_upperLimit)
                            {
                                selectedSectionNumber = 2;
                                selectedTotalDrumNumbers = yarncountconfigmodel.drumNumbers_s2;
                                scheduledStartDate = yarncountconfigmodel.scheduledStartDate;
                                scheduledEndDate = yarncountconfigmodel.scheduledEndDate;
                                settingsUpdatedDate = yarncountconfigmodel.updateddate;

                                selectedSpeed = yarncountconfigmodel.speed;
                                selectedP1 = yarncountconfigmodel.p1;
                                selectedP1Deviation = yarncountconfigmodel.p1Deviation;
                                selectedP2 = yarncountconfigmodel.p2;
                                selectedP2Deviation = yarncountconfigmodel.p2Deviation;
                                selectedN1 = yarncountconfigmodel.n1;
                                selectedN1Deviation = yarncountconfigmodel.n1Deviation;

                                selectedBelowLimit = yarncountconfigmodel.belowLimit;
                                selectedMaxRollingCount = yarncountconfigmodel.maxLimit;
                                selectedMaterialCount = yarncountconfigmodel.materialCount;

                                if (DateTime.Now.Date > scheduledEndDate && isTestResume == false)
                                {
                                    picker_drumNumber.SelectedIndex = -1;
                                    picker_drumSelection.IsEnabled = false;
                                    DisplayAlert("Attention", "The scheduled date is expired for the selected drum [" +
                                        selectedDrumNumber.ToString() + "]. Please reach admin to change the machine settings", "OK");
                                    return;
                                }

                                startDate = yarncountconfigmodel.scheduledStartDate;
                                endDate = yarncountconfigmodel.scheduledEndDate;
                                entry_stdStrength.Text = yarncountconfigmodel.stdRollingStrength.ToString();
                                entry_strengthDeviation.Text = yarncountconfigmodel.strengthDeviation.ToString();
                                entry_belowLimit.Text = yarncountconfigmodel.belowLimit.ToString() + " & " + yarncountconfigmodel.maxLimit.ToString();
                                entry_numberOfTest.Text = yarncountconfigmodel.totalSamples.ToString();
                                picker_drumSelection.SelectedItem = "Scheduled";
                            }

                            if (selectedDrumNumber >= sec3_lowerLimit && selectedDrumNumber <= sec3_upperLimit)
                            {
                                selectedSectionNumber = 3;
                                selectedTotalDrumNumbers = yarncountconfigmodel.drumNumbers_s3;
                                scheduledStartDate = yarncountconfigmodel.scheduledStartDate;
                                scheduledEndDate = yarncountconfigmodel.scheduledEndDate;
                                settingsUpdatedDate = yarncountconfigmodel.updateddate;

                                selectedSpeed = yarncountconfigmodel.speed;
                                selectedP1 = yarncountconfigmodel.p1;
                                selectedP1Deviation = yarncountconfigmodel.p1Deviation;
                                selectedP2 = yarncountconfigmodel.p2;
                                selectedP2Deviation = yarncountconfigmodel.p2Deviation;
                                selectedN1 = yarncountconfigmodel.n1;
                                selectedN1Deviation = yarncountconfigmodel.n1Deviation;

                                selectedBelowLimit = yarncountconfigmodel.belowLimit;
                                selectedMaxRollingCount = yarncountconfigmodel.maxLimit;
                                selectedMaterialCount = yarncountconfigmodel.materialCount;

                                if (DateTime.Now.Date > scheduledEndDate && isTestResume == false)
                                {
                                    picker_drumNumber.SelectedIndex = -1;
                                    picker_drumSelection.IsEnabled = false;
                                    DisplayAlert("Attention", "The scheduled date is expired for the selected drum [" +
                                        selectedDrumNumber.ToString() + "]. Please reach admin to change the machine settings", "OK");
                                    return;
                                }

                                startDate = yarncountconfigmodel.scheduledStartDate;
                                endDate = yarncountconfigmodel.scheduledEndDate;
                                entry_stdStrength.Text = yarncountconfigmodel.stdRollingStrength.ToString();
                                entry_strengthDeviation.Text = yarncountconfigmodel.strengthDeviation.ToString();
                                entry_belowLimit.Text = yarncountconfigmodel.belowLimit.ToString() + " & " + yarncountconfigmodel.maxLimit.ToString();
                                entry_numberOfTest.Text = yarncountconfigmodel.totalSamples.ToString();
                                picker_drumSelection.SelectedItem = "Scheduled";
                            }
                        }
                        else if (yarncountconfigmodel.totalSections == 2)
                        {
                            int sec1_lowerLimit = int.Parse(yarncountconfigmodel.drumNumbers_s1.ToString().Split('.')[0]);
                            int sec1_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s1.ToString().Split('.')[1]);

                            int sec2_lowerLimit = int.Parse(yarncountconfigmodel.drumNumbers_s2.ToString().Split('.')[0]);
                            int sec2_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s2.ToString().Split('.')[1]);

                            if (selectedDrumNumber >= sec1_lowerLimit && selectedDrumNumber <= sec1_upperLimit)
                            {
                                selectedSectionNumber = 1;
                                selectedTotalDrumNumbers = yarncountconfigmodel.drumNumbers_s1;
                                scheduledStartDate = yarncountconfigmodel.scheduledStartDate;
                                scheduledEndDate = yarncountconfigmodel.scheduledEndDate;
                                settingsUpdatedDate = yarncountconfigmodel.updateddate;

                                selectedSpeed = yarncountconfigmodel.speed;
                                selectedP1 = yarncountconfigmodel.p1;
                                selectedP1Deviation = yarncountconfigmodel.p1Deviation;
                                selectedP2 = yarncountconfigmodel.p2;
                                selectedP2Deviation = yarncountconfigmodel.p2Deviation;
                                selectedN1 = yarncountconfigmodel.n1;
                                selectedN1Deviation = yarncountconfigmodel.n1Deviation;

                                selectedBelowLimit = yarncountconfigmodel.belowLimit;
                                selectedMaxRollingCount = yarncountconfigmodel.maxLimit;
                                selectedMaterialCount = yarncountconfigmodel.materialCount;

                                if (DateTime.Now.Date > scheduledEndDate && isTestResume == false)
                                {
                                    picker_drumNumber.SelectedIndex = -1;
                                    picker_drumSelection.IsEnabled = false;
                                    DisplayAlert("Attention", "The scheduled date is expired for the selected drum [" +
                                        selectedDrumNumber.ToString() + "]. Please reach admin to change the machine settings", "OK");
                                    return;
                                }

                                startDate = yarncountconfigmodel.scheduledStartDate;
                                endDate = yarncountconfigmodel.scheduledEndDate;
                                entry_stdStrength.Text = yarncountconfigmodel.stdRollingStrength.ToString();
                                entry_strengthDeviation.Text = yarncountconfigmodel.strengthDeviation.ToString();
                                entry_belowLimit.Text = yarncountconfigmodel.belowLimit.ToString() + " & " + yarncountconfigmodel.maxLimit.ToString();
                                entry_numberOfTest.Text = yarncountconfigmodel.totalSamples.ToString();
                                picker_drumSelection.SelectedItem = "Scheduled";
                            }

                            if (selectedDrumNumber >= sec2_lowerLimit && selectedDrumNumber <= sec2_upperLimit)
                            {
                                selectedSectionNumber = 2;
                                selectedTotalDrumNumbers = yarncountconfigmodel.drumNumbers_s2;
                                scheduledStartDate = yarncountconfigmodel.scheduledStartDate;
                                scheduledEndDate = yarncountconfigmodel.scheduledEndDate;
                                settingsUpdatedDate = yarncountconfigmodel.updateddate;

                                selectedSpeed = yarncountconfigmodel.speed;
                                selectedP1 = yarncountconfigmodel.p1;
                                selectedP1Deviation = yarncountconfigmodel.p1Deviation;
                                selectedP2 = yarncountconfigmodel.p2;
                                selectedP2Deviation = yarncountconfigmodel.p2Deviation;
                                selectedN1 = yarncountconfigmodel.n1;
                                selectedN1Deviation = yarncountconfigmodel.n1Deviation;

                                selectedBelowLimit = yarncountconfigmodel.belowLimit;
                                selectedMaxRollingCount = yarncountconfigmodel.maxLimit;
                                selectedMaterialCount = yarncountconfigmodel.materialCount;

                                if (DateTime.Now.Date > scheduledEndDate && isTestResume == false)
                                {
                                    picker_drumNumber.SelectedIndex = -1;
                                    picker_drumSelection.IsEnabled = false;
                                    DisplayAlert("Attention", "The scheduled date is expired for the selected drum [" +
                                        selectedDrumNumber.ToString() + "]. Please reach admin to change the machine settings", "OK");
                                    return;
                                }

                                startDate = yarncountconfigmodel.scheduledStartDate;
                                endDate = yarncountconfigmodel.scheduledEndDate;
                                entry_stdStrength.Text = yarncountconfigmodel.stdRollingStrength.ToString();
                                entry_strengthDeviation.Text = yarncountconfigmodel.strengthDeviation.ToString();
                                entry_belowLimit.Text = yarncountconfigmodel.belowLimit.ToString() + " & " + yarncountconfigmodel.maxLimit.ToString();
                                entry_numberOfTest.Text = yarncountconfigmodel.totalSamples.ToString();
                                picker_drumSelection.SelectedItem = "Scheduled";
                            }
                        }
                        else if (yarncountconfigmodel.totalSections == 1)
                        {
                            int sec1_lowerLimit = int.Parse(yarncountconfigmodel.drumNumbers_s1.ToString().Split('.')[0]);
                            int sec1_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s1.ToString().Split('.')[1]);

                            if (selectedDrumNumber >= sec1_lowerLimit && selectedDrumNumber <= sec1_upperLimit)
                            {
                                selectedSectionNumber = 1;
                                selectedTotalDrumNumbers = yarncountconfigmodel.drumNumbers_s1;
                                scheduledStartDate = yarncountconfigmodel.scheduledStartDate;
                                scheduledEndDate = yarncountconfigmodel.scheduledEndDate;
                                settingsUpdatedDate = yarncountconfigmodel.updateddate;

                                selectedSpeed = yarncountconfigmodel.speed;
                                selectedP1 = yarncountconfigmodel.p1;
                                selectedP1Deviation = yarncountconfigmodel.p1Deviation;
                                selectedP2 = yarncountconfigmodel.p2;
                                selectedP2Deviation = yarncountconfigmodel.p2Deviation;
                                selectedN1 = yarncountconfigmodel.n1;
                                selectedN1Deviation = yarncountconfigmodel.n1Deviation;

                                selectedBelowLimit = yarncountconfigmodel.belowLimit;
                                selectedMaxRollingCount = yarncountconfigmodel.maxLimit;
                                selectedMaterialCount = yarncountconfigmodel.materialCount;

                                if (DateTime.Now.Date > scheduledEndDate && isTestResume == false)
                                {
                                    picker_drumNumber.SelectedIndex = -1;
                                    picker_drumSelection.IsEnabled = false;
                                    DisplayAlert("Attention", "The scheduled date is expired for the selected drum [" +
                                        selectedDrumNumber.ToString() + "]. Please reach admin to change the machine settings", "OK");
                                    return;
                                }

                                startDate = yarncountconfigmodel.scheduledStartDate;
                                endDate = yarncountconfigmodel.scheduledEndDate;
                                entry_stdStrength.Text = yarncountconfigmodel.stdRollingStrength.ToString();
                                entry_strengthDeviation.Text = yarncountconfigmodel.strengthDeviation.ToString();
                                entry_belowLimit.Text = yarncountconfigmodel.belowLimit.ToString() + " & " + yarncountconfigmodel.maxLimit.ToString();
                                entry_numberOfTest.Text = yarncountconfigmodel.totalSamples.ToString();
                                picker_drumSelection.SelectedItem = "Scheduled";
                            }
                        }

                        if(startDate!=DEFAULTDATE && endDate != DEFAULTDATE)
                        {
                            conn.CreateTable<StrengthTestModel>();
                            StrengthTestModel selectedDrumTest = conn.Table<StrengthTestModel>().Where(StrengthTestModel =>
                                                                (StrengthTestModel.drumNumber == selectedDrumNumber
                                                                && StrengthTestModel.machineID == selectedMachineID
                                                                && StrengthTestModel.drumSelectionMethod == "Scheduled"
                                                                && (StrengthTestModel.createdate >= startDate
                                                                || StrengthTestModel.createdate <= endDate)))
                                                                .OrderByDescending(StrengthTestModel=>StrengthTestModel.sampleNo)
                                                                .FirstOrDefault();
                            if (selectedDrumTest != null && (selectedDrumTest.totalTestCount== selectedDrumTest.sampleNo) && isTestResume==false)
                            {
                                //DisplayAlert("Attention", "Test already completed for Drum Number ("
                                //                + selectedDrumNumber.ToString() + ") on "
                                //                + selectedDrumTest.createdate.ToShortDateString()
                                //                + ". Still want to conduct test for this drum ?", "OK");
                                bool userDecision = await DisplayAlert("Attention",
                                                "Test already completed for Drum Number ("
                                                + selectedDrumNumber.ToString() + ") on "
                                                + selectedDrumTest.createdate.ToShortDateString()
                                                + ". Still do you want to conduct test for this drum in Random method?",
                                                "Yes",
                                                "No");
                                if (userDecision)
                                {
                                    //using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                                    //{
                                    UserModel loggedInUser = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                                        if (loggedInUser == null)
                                        {
                                            DisplayAlert("Attention", "Unable to get logged user information!!!", "OK");
                                            return;
                                        }
                                        else
                                        {
                                            if (loggedInUser.isAdmin)
                                            {
                                                picker_drumSelection.SelectedItem = "Random";
                                                picker_drumSelection.IsEnabled = false;
                                            }
                                            else
                                            {
                                                Environment.SetEnvironmentVariable("DrumSelectionMethodChange", null);
                                                var result = await Navigation.ShowPopupAsync(new AdminCredPopUp());
                                                if (result != null)
                                                {
                                                    if (result.ToString() == "Success")
                                                    {
                                                        picker_drumSelection.SelectedItem = "Random";
                                                        picker_drumSelection.IsEnabled = false;
                                                    }
                                                    else
                                                    {
                                                        //reset test params;
                                                        picker_drumNumber.SelectedIndex = -1;
                                                        picker_drumSelection.IsEnabled = false;
                                                        DisplayAlert("Attention", result.ToString(), "OK");
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    //reset test params;
                                                    picker_drumNumber.SelectedIndex = -1;
                                                    picker_drumSelection.IsEnabled = false;
                                                    return;
                                                }
                                            }
                                        }
                                    //}
                                }
                                else
                                {
                                    //reset test params;
                                    picker_drumNumber.SelectedIndex = -1;
                                    picker_drumSelection.IsEnabled = false;
                                }
                            }
                            else
                            {
                                picker_drumSelection.IsEnabled = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }

       

        private async void btn_drumSelectionMethod_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (isTestStarted) { return; }
                if (picker_machinename.SelectedIndex < 0) { return; }
                if (picker_drumNumber.SelectedIndex < 0) { return; }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                { 
                    UserModel loggedInUser = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                    if (loggedInUser == null)
                    {
                        DisplayAlert("Attention", "Unable to get logged user information!!!", "OK");
                        return;
                    }
                    else
                    {
                        if (loggedInUser.isAdmin)
                        {
                            picker_drumSelection.IsEnabled = true;
                        }
                        else
                        {
                            var result = await Navigation.ShowPopupAsync(new AdminCredPopUp());
                            if (result != null)
                            {
                                if (result.ToString() == "Success")
                                {
                                    picker_drumSelection.IsEnabled = true;
                                }
                                else
                                {
                                    picker_drumSelection.IsEnabled = false;
                                    DisplayAlert("Attention", result.ToString(), "OK");
                                    return;
                                }
                            }
                            else
                            {
                                picker_drumSelection.IsEnabled = false;
                                return;
                            }
                        }
                        
                    }
                }
            }
            catch(Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }

        private void btn_pressure_Clicked(System.Object sender, System.EventArgs e)
        {
            _ = getPressureConfirmation();
        }

        private async Task getPressureConfirmation()
        {
            try
            {
                if (isTestStarted) { return; }
                if (picker_machinename.SelectedIndex < 0) { return; }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    var result = await Navigation.ShowPopupAsync(new PressureInfoPopUp(selectedMachineID, selectedMachineName));
                    if (result != null)
                    {
                        if (!result.ToString().Contains('~'))
                        {
                            entry_pressure.Text = "";
                            DisplayAlert("Attention", result.ToString(), "OK");
                            return;
                        }

                        string res_msg = result.ToString().Split('~')[0];
                        string mac_params = result.ToString().Split('~')[1];


                        if (res_msg == "Success")
                        {
                            ConfigModel macDetails = conn.Table<ConfigModel>().Where(ConfigModel =>
                                        (ConfigModel.machineID == selectedMachineID
                                        && ConfigModel.machineName == selectedMachineName)).FirstOrDefault();
                            if (macDetails != null)
                            {

                                if (mac_params.Contains('|'))
                                {
                                    string speed = mac_params.Split('|')[0];
                                    string p1 = mac_params.Split('|')[1];
                                    string p2 = mac_params.Split('|')[2];
                                    string n1 = mac_params.Split('|')[3];

                                    entry_pressure.Text = speed + ", "
                                                          + p1 + ", "
                                                          + p2 + ", "
                                                          + n1;
                                    selectedSpeed = int.Parse(speed);
                                    selectedP1 = decimal.Parse(p1);
                                    selectedP1Deviation = macDetails.p1Deviation;
                                    selectedP2 = decimal.Parse(p2);
                                    selectedP2Deviation = macDetails.p2Deviation;
                                    selectedN1 = decimal.Parse(n1);
                                    selectedN1Deviation = macDetails.n1Deviation;
                                    return;
                                }
                                else
                                {
                                    entry_pressure.Text = "";
                                    selectedSpeed = 0;
                                    selectedP1 = 0.0m;
                                    selectedN1Deviation = 0.0m;
                                    selectedP2 = 0.0m;
                                    selectedP2Deviation = 0.0m;
                                    selectedN1 = 0.0m;
                                    selectedN1Deviation = 0.0m;
                                    DisplayAlert("Attention", "Unable to reterive machine details. Please try again", "OK");
                                    return;
                                }
                            }
                            else
                            {
                                entry_pressure.Text = "";
                                selectedSpeed = 0;
                                selectedP1 = 0.0m;
                                selectedN1Deviation = 0.0m;
                                selectedP2 = 0.0m;
                                selectedP2Deviation = 0.0m;
                                selectedN1 = 0.0m;
                                selectedN1Deviation = 0.0m;
                                DisplayAlert("Attention", "Unable to reterive machine details. Please try again", "OK");
                                return;
                            }
                        }
                        else
                        {
                            entry_pressure.Text = "";
                            DisplayAlert("Attention", result.ToString(), "OK");
                            return;
                        }
                    }
                    else
                    {
                        entry_pressure.Text = "";
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
                return;
            } 
        }

        private async void picker_drumSelection_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (picker_drumSelection.IsEnabled == false) { return; }
                if (picker_drumNumber.SelectedIndex < 0) { picker_drumSelection.SelectedIndex = -1; return; }
                if (picker_drumSelection.SelectedIndex > 0)
                {
                    if(scheduledStartDate==DEFAULTDATE || scheduledEndDate == DEFAULTDATE)
                    {
                        if (picker_drumSelection.SelectedItem == "Scheduled")
                        {
                            picker_drumSelection.SelectedItem = "Random";
                        }
                        return;
                    }

                    if (picker_drumSelection.SelectedItem == "Scheduled")
                    {
                        using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                        {
                            conn.CreateTable<StrengthTestModel>();
                            StrengthTestModel selectedDrumTest = conn.Table<StrengthTestModel>().Where(StrengthTestModel =>
                                                                (StrengthTestModel.drumNumber == selectedDrumNumber
                                                                && StrengthTestModel.machineID == selectedMachineID
                                                                && StrengthTestModel.drumSelectionMethod == "Scheduled"
                                                                && (StrengthTestModel.createdate >= scheduledStartDate
                                                                || StrengthTestModel.createdate <= scheduledEndDate)))
                                                                .OrderByDescending(StrengthTestModel => StrengthTestModel.sampleNo)
                                                                .FirstOrDefault();
                            if (selectedDrumTest != null
                                && (selectedDrumTest.totalTestCount == selectedDrumTest.sampleNo)
                                && isTestResume == false)
                            {
                                bool userDecision = await DisplayAlert("Attention",
                                                                        "Already a scheduled test was taken for the selected Drum [" + selectedDrumNumber + "]. So do you want to continue test using random option?",
                                                                        "Yes",
                                                                        "No");
                                if (userDecision)
                                {
                                    picker_drumSelection.SelectedItem = "Random";
                                }
                                else
                                {
                                    picker_drumSelection.IsEnabled = false;
                                    picker_machinename.SelectedIndex = -1;
                                }
                            }
                        }
                    }
                }
            }catch(Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
                return;
            }
        }
    }
}