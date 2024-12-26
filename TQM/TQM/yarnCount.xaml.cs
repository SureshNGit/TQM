using Android.Bluetooth;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
using static System.Net.Mime.MediaTypeNames;
using Color = Xamarin.Forms.Color;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class yarnCount : ContentPage, INotifyPropertyChanged
    {
        private BluetoothSocket _socket;
        BluetoothAdapter adapter;
        BluetoothDevice device;
        const decimal MIN_VAL = 0.4000m;
        const decimal ZERO = 0.0000m;
        const int PER_TEST_LOOP_COUNT = 100;
        const int DATA_READ_LOOP_COUNT = 100;
        const int STABLE_DATA_CHECK = 5;
        private decimal current_stable_data = 0;
        private List<YCTestModelView> ycTestModelViewlist;
        private long currentTestID = 0;
        private UserModel currentloggedInUser = null;
        private string selectedMachineCategory = null;
        private Guid selectedMachineID = Guid.Empty;
        private string selectedMachineName = null;
        private string selectedSysName = null;
        private string selectedCountUnit = null;
        private decimal selectedYarnLen = 0.0000m;
        private int selectedTestCount = 0;
        private string selectedShift = null;
        private string selectedProcess = null;
        private decimal selectedDeviationPercent = 0m;
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
        private decimal STD_CV = 0.0000m;
        private decimal STD_CV_DEVIATION = 0.0000m;
        private int currentTestCount = 0;
        private bool isTestStarted = false;
        private dynamic currentTestStartTime = null;
        private RunConfiguration runConfiguration = new RunConfiguration();
        private bool toastInitialize = false;

        public yarnCount()
        {
            InitializeComponent();
            lbl_TestID.Text = "";
            //lbl_TestID.Text = "999999999";

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

                conn.CreateTable<YCTestModel>();
                int recordCount = conn.Table<YCTestModel>().Count();

                if (recordCount > 0)
                {
                    YCTestModel zeroTest = conn.Table<YCTestModel>().Where(YCTestModel => (YCTestModel.testID == 0))
                                            .OrderBy(YCTestModel => YCTestModel.testcount).FirstOrDefault();

                    if (zeroTest != null)
                    {
                        DateTime startDate = zeroTest.createdate;
                        YCTestModel beforeZeroTest = conn.Table<YCTestModel>().Where(YCTestModel => (YCTestModel.createdate < startDate))
                                            .OrderByDescending(YCTestModel => YCTestModel.testID).FirstOrDefault();

                        if (beforeZeroTest != null)
                        {
                            long lastProperTestID = beforeZeroTest.testID + 1;
                            List<YCTestModel> resetTestList = conn.Table<YCTestModel>().Where(YCTestModel => (YCTestModel.createdate >= startDate))
                                                              .OrderBy(YCTestModel => YCTestModel.testID).ToList();
                            int failCounter = 0;
                            if (resetTestList[0].testID == 0)
                            {
                                foreach (YCTestModel test in resetTestList)
                                {
                                    test.testID = lastProperTestID + test.testID;
                                    int row = conn.Update(test);
                                    if (row < 1)
                                    {
                                        failCounter++;
                                    }
                                }
                            }

                            List<YCTestSummaryModel> resetTestSummaryList = conn.Table<YCTestSummaryModel>().Where(YCTestSummaryModel =>
                                                                    (YCTestSummaryModel.createdate >= startDate))
                                                                    .OrderBy(YCTestSummaryModel => YCTestSummaryModel.testID).ToList();
                            int failCounter_Summary = 0;
                            if (resetTestSummaryList[0].testID == 0)
                            {
                                foreach (YCTestSummaryModel testsummary in resetTestSummaryList)
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
                YarnCountConfigModel ycConfig_uf = null;
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    ycConfig_uf = conn.Table<YarnCountConfigModel>().
                                Where(YarnCountConfigModel => (YarnCountConfigModel.uf_name_1 != null ||
                                YarnCountConfigModel.uf_name_1 != "") && YarnCountConfigModel.machineCategory == mCat
                                && YarnCountConfigModel.machineID == mid && YarnCountConfigModel.machineName == mac).FirstOrDefault();
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
            hideFrames();
            if (mCat == "" && mid == Guid.Empty && mac == "")
            {
                selectedDeviationPercent = 0m;
                lbl_countsysname.Text = "";
                picker_yarncountunit.SelectedIndex = 0;
                entry_yarnlen.Text = "";
                entry_testcount.Text = "";
                picker_shift.SelectedIndex = 0;
                picker_process.SelectedIndex = 0;
                entry_standardHank.Text = "0.0000";
                return;
            }
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<YarnCountConfigModel>();
                YarnCountConfigModel yarncountconfigmodel = conn.Table<YarnCountConfigModel>().Where(YarnCountConfigModel =>
                                                            (YarnCountConfigModel.machineCategory == mCat &&
                                                            YarnCountConfigModel.machineID == mid &&
                                                            YarnCountConfigModel.machineName == mac)).FirstOrDefault();
                if (yarncountconfigmodel != null)
                {
                    selectedDeviationPercent = yarncountconfigmodel.deviationPercent;
                    lbl_countsysname.Text = yarncountconfigmodel.countsysname;
                    picker_yarncountunit.SelectedItem = yarncountconfigmodel.yarnlenunit.ToString();
                    if (mCat == "Simplex/SpeedFrame")
                    {
                        entry_yarnlen.Text = yarncountconfigmodel.rovinglength.ToString();
                    }
                    else if (mCat == "Spinning" || mCat == "Winding")
                    {
                        entry_yarnlen.Text = yarncountconfigmodel.lealength.ToString();
                    }
                    else
                    {
                        entry_yarnlen.Text = yarncountconfigmodel.sliverlength.ToString();
                    }
                    entry_testcount.Text = yarncountconfigmodel.testcount.ToString();
                    TESTCOUNT = yarncountconfigmodel.testcount;
                    entry_standardHank.Text = formatDecimal(yarncountconfigmodel.standardHank).ToString();
                    STD_HANK = formatDecimal(yarncountconfigmodel.standardHank);
                    STD_HANK_CURR = formatDecimal(yarncountconfigmodel.standardHank);
                    STD_CV = formatDecimal(yarncountconfigmodel.standardCV);
                    STD_CV_DEVIATION = formatDecimal(yarncountconfigmodel.CVDeviationPercent);

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
                    lbl_countsysname.Text = "";
                    picker_yarncountunit.SelectedIndex = 0;
                    entry_yarnlen.Text = "";
                    entry_testcount.Text = "";
                    picker_shift.SelectedIndex = 0;
                    picker_process.SelectedIndex = 0;
                    entry_standardHank.Text = "0.0000";
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

        private List<YCTestReportModelView> generateResultView()
        {
            try
            {
                List<YCTestReportModelView> OVS = new List<YCTestReportModelView>();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {

                    conn.CreateTable<YCTestModel>();
                    conn.CreateTable<YCTestSummaryModel>();


                    if (ycTestModelViewlist == null)
                    {
                        return null;
                    }


                    List<YCTestModel> id_test = conn.Table<YCTestModel>().Where(
                        YCTestModel => YCTestModel.testID == currentTestID).ToList();


                    List<YCTestSummaryModel> ts = conn.Table<YCTestSummaryModel>().Where(
                        YCTestSummaryModel => YCTestSummaryModel.testID == currentTestID).ToList();

                    if (id_test.Count>0 && ts.Count>0)
                    {

                        int loopCount = 0;
                        decimal totalWeight = 0.0000m;
                        decimal totalCalcCountVal = 0.0000m;
                        foreach (YCTestModel id in id_test)
                        {
                            YCTestReportModelView ycTestReportMV = new YCTestReportModelView()
                            {
                                testID = id.testID,
                                description = id.testcount.ToString(),
                                weight = formatDecimal(id.yarnweight).ToString(),
                                hank = formatDecimal(id.yccalcval).ToString(),
                            };
                            OVS.Add(ycTestReportMV);
                            loopCount += 1;
                            totalCalcCountVal = totalCalcCountVal + id.yccalcval;
                            totalCalcCountVal = formatDecimal(totalCalcCountVal);
                            totalWeight = totalWeight + id.yarnweight;
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

                        if (ts[0].yarnWeightAvg==0.0m)
                        {
                            mean = totalCalcCountVal / id_test[0].totaltestcount;
                            decimal IndividualCalValminusMean = 0m;
                            foreach (YCTestModel test in id_test)
                            {
                                IndividualCalValminusMean = IndividualCalValminusMean + ((test.yccalcval - mean) * (test.yccalcval - mean));
                            }
                            sd = (decimal)Math.Sqrt((double)IndividualCalValminusMean / (double)(id_test[0].totaltestcount - 1));//Standard Deviation
                            sd = formatDecimal(sd);
                            mean = formatDecimal(mean);
                            cv = (sd / mean) * 100.0000m; //Coefficient of Variation
                            cv = formatDecimal(cv);

                            min = id_test.Min(YCTestModel => YCTestModel.yccalcval);
                            max = id_test.Max(YCTestModel => YCTestModel.yccalcval);
                            range = max - min;


                            mean_weight = totalWeight / id_test[0].totaltestcount;
                            decimal IndividualWeightminusMean = 0m;
                            foreach (YCTestModel test in id_test)
                            {
                                IndividualWeightminusMean = IndividualWeightminusMean + ((test.yarnweight - mean) * (test.yarnweight - mean));
                            }
                            sd_weight = (decimal)Math.Sqrt((double)IndividualWeightminusMean / (double)(id_test[0].totaltestcount - 1));//Standard Deviation
                            sd_weight = formatDecimal(sd_weight);
                            mean_weight = formatDecimal(mean_weight);
                            cv_weight = (sd_weight / mean_weight) * 100.0000m; //Coefficient of Variation
                            cv_weight = formatDecimal(cv_weight);

                            min_weight = id_test.Min(YCTestModel => YCTestModel.yarnweight);
                            max_weight = id_test.Max(YCTestModel => YCTestModel.yarnweight);
                            range_weight = max_weight - min_weight;
                        }
                        else
                        {
                            mean_weight = ts[0].yarnWeightAvg;
                            mean = ts[0].testaverage;

                            max_weight = ts[0].yarnWeightMax;
                            max = ts[0].testMax;

                            min_weight = ts[0].yarnWeightMin;
                            min = ts[0].testMin;

                            range_weight = ts[0].yarnWeightRange;
                            range = ts[0].testRange;

                            sd_weight = ts[0].yarnWeightSD;
                            sd = ts[0].testsd;

                            cv_weight = ts[0].yarnWeightCV;
                            cv = ts[0].testcv;
                        }

                        YCTestReportModelView testMV = new YCTestReportModelView()
                        {
                            testID = ts[0].testID,
                            description = "Average",
                            weight = formatDecimal(mean_weight).ToString(),
                            hank = formatDecimal(mean).ToString(),
                        };
                        OVS.Add(testMV);

                        testMV = new YCTestReportModelView()
                        {
                            testID = ts[0].testID,
                            description = "Max",
                            weight = formatDecimal(max_weight).ToString(),
                            hank = formatDecimal(max).ToString(),
                        };
                        OVS.Add(testMV);

                        testMV = new YCTestReportModelView()
                        {
                            testID = ts[0].testID,
                            description = "Min",
                            weight = formatDecimal(min_weight).ToString(),
                            hank = formatDecimal(min).ToString(),
                        };
                        OVS.Add(testMV);

                        testMV = new YCTestReportModelView()
                        {
                            testID = ts[0].testID,
                            description = "Range",
                            weight = formatDecimal(range_weight).ToString(),
                            hank = formatDecimal(range).ToString(),
                        };
                        OVS.Add(testMV);


                        testMV = new YCTestReportModelView()
                        {
                            testID = ts[0].testID,
                            description = "SD",
                            weight = formatDecimal(sd_weight).ToString(),
                            hank = formatDecimal(sd).ToString(),
                        };
                        OVS.Add(testMV);

                        testMV = new YCTestReportModelView()
                        {
                            testID = ts[0].testID,
                            description = "CV",
                            weight = formatDecimal(cv_weight).ToString(),
                            hank = formatDecimal(cv).ToString(),
                        };
                        OVS.Add(testMV);

                    }
                    else
                    {
                        return null;
                    }
                }
                return OVS;
            }
            catch (Exception ex)
            {
                //DisplayAlert("Attention", "Error Occurred!!! Error:" + ex.Message.ToString(), "OK");
                return null;
            }
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
                    //listview_testresult_FinalOut.ItemsSource = ycTestModelViewlist;
                    listview_testresult_FinalOut.ItemsSource = generateResultView();
                    if (selectedMachineCategory == "Spinning" || selectedMachineCategory == "Winding")
                    {
                        //lbl_testresult_Final_stadHank.Text = "Count (" + STD_HANK.ToString() + "\u00B1" + selectedDeviationPercent + ")";
                        span_head.Text = "Count";
                        span_stdValue.Text = " (" + STD_HANK.ToString();
                        span_deviation.Text = " \u00B1" + selectedDeviationPercent + ")";
                    }
                    else
                    {
                        //lbl_testresult_Final_stadHank.Text = "Hank (" + STD_HANK.ToString() + "\u00B1" + selectedDeviationPercent + ")";
                        span_head.Text = "Hank";
                        span_stdValue.Text = " (" + STD_HANK.ToString();
                        span_deviation.Text = " \u00B1" + selectedDeviationPercent + ")";
                    }
                }
                else
                {
                    listview_testresult_FinalOut.ItemsSource = null;
                    listview_testresult_FinalOut.IsVisible = false; ;
                    individualTestResultFrame_FinalOut.IsVisible = false;

                    individualTestResultFrame.IsVisible = true;
                    listview_testresult.IsVisible = visibility;
                    if (ycTestModelViewlist != null)
                    {
                        listview_testresult.ItemsSource = null;
                        listview_testresult.ItemsSource = ycTestModelViewlist.OrderByDescending(YCTestModelView => YCTestModelView.testcount);
                    }

                    if (selectedMachineCategory == "Spinning" || selectedMachineCategory == "Winding")
                    {
                        //lbl_testresult_stadHank.Text = "Count " + STD_HANK.ToString() + "\u00B1" + selectedDeviationPercent + ")";
                        span_head_ind.Text = "Count";
                        span_stdValue_ind.Text = " (" + STD_HANK.ToString();
                        span_deviation_ind.Text = " \u00B1" + selectedDeviationPercent + ")";
                    }
                    else
                    {
                        //lbl_testresult_stadHank.Text = "Hank " + STD_HANK.ToString() + "\u00B1" + selectedDeviationPercent + ")";
                        span_head_ind.Text = "Hank";
                        span_stdValue_ind.Text = " (" + STD_HANK.ToString();
                        span_deviation_ind.Text = " \u00B1" + selectedDeviationPercent + ")";
                    }
                }
            });
        }

        private async Task refOverallSummary(decimal mean = 0m, decimal sd = 0m, decimal cv = 0m, bool visibility = true, bool showFinalOut = false)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (showFinalOut)
                {
                    frame_overallSummary_FinalOut.IsVisible = visibility;
                    lbl_average_FinalOut.Text = mean.ToString();
                    lbl_sd_FinalOut.Text = sd.ToString();
                    lbl_cv_FinalOut.Text = cv.ToString();

                    decimal maxRangeVal = STD_HANK + selectedDeviationPercent;
                    decimal minRangeVal = STD_HANK - selectedDeviationPercent;


                    if (mean < minRangeVal || mean > maxRangeVal)
                    {
                        listview_testresult_FinalOut.BackgroundColor = Color.FromHex("#ffc3c0");
                        lbl_average_FinalOut.TextColor = Color.Red;
                    }
                    else
                    {
                        listview_testresult_FinalOut.BackgroundColor = Color.White;
                        lbl_average_FinalOut.TextColor = Color.DarkSlateGray;
                    }
                }
                else
                {
                    frame_overallSummary.IsVisible = visibility;
                    lbl_average.Text = mean.ToString();
                    lbl_sd.Text = sd.ToString();
                    lbl_cv.Text = cv.ToString();
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
                decimal totalWeight = 0.0000m;
                decimal totalCalcCountVal = 0.0000m;
                conn.CreateTable<YCTestModel>();
                foreach (YCTestModelView test in ycTestModelViewlist)
                {
                    YCTestModel ycTestModel = new YCTestModel()
                    {
                        ID = Guid.NewGuid(),
                        testID = test.testID,
                        userID = test.userID,
                        userName = test.userName,
                        machineID = test.machineID,
                        machineCategory = test.machineCategory,
                        machineName = test.machineName,
                        //apercent = test.apercent,
                        shift = test.shift,
                        process = test.process,
                        countsysname = test.countsysname,
                        yarnlenunit = test.yarnlenunit,
                        yarnlength = test.yarnlength,
                        totaltestcount = test.totaltestcount,
                        testcount = test.testcount,
                        yarnweight = test.yarnweight,
                        yccalcval = test.yccalcval,
                        createdate = DateTime.Now
                    };
                    int row = conn.Insert(ycTestModel);
                    if (row < 1)
                    {
                        dbStatus = false;
                    }
                    totalCalcCountVal = totalCalcCountVal + test.yccalcval;
                    totalCalcCountVal = formatDecimal(totalCalcCountVal);
                    totalWeight = totalWeight + test.yarnweight;
                    totalWeight = formatDecimal(totalWeight);
                }
                if (dbStatus)
                {
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
                    if (ycTestModelViewlist[0].totaltestcount > 1)
                    {
                        mean = totalCalcCountVal / ycTestModelViewlist[0].totaltestcount;
                        //decimal IndividualCalValminusMean = 0m;
                        //foreach (YCTestModelView test in ycTestModelViewlist)
                        //{
                        //    IndividualCalValminusMean = IndividualCalValminusMean + ((test.yccalcval - mean) * (test.yccalcval - mean));
                        //}
                        //sd = (decimal)Math.Sqrt((double)IndividualCalValminusMean / (double)(ycTestModelViewlist[0].totaltestcount - 1));//Standard Deviation
                        decimal[] yccalcvalArray = ycTestModelViewlist.Select(m => m.yccalcval).ToArray();
                        sd = CalculateStandardDeviation(yccalcvalArray);
                        sd = formatDecimal(sd);
                        mean = formatDecimal(mean);
                        cv = (sd / mean) * 100.0000m; //Coefficient of Variation
                        cv = formatDecimal(cv);

                        min = ycTestModelViewlist.Min(YCTestModelView => YCTestModelView.yccalcval);
                        max = ycTestModelViewlist.Max(YCTestModelView => YCTestModelView.yccalcval);
                        range = max - min;


                        mean_weight = totalWeight / ycTestModelViewlist[0].totaltestcount;
                        //decimal IndividualWeightminusMean = 0m;
                        //foreach (YCTestModelView test in ycTestModelViewlist)
                        //{
                        //    IndividualWeightminusMean = IndividualWeightminusMean + ((test.yarnweight - mean) * (test.yarnweight - mean));
                        //}
                        //sd_weight = (decimal)Math.Sqrt((double)IndividualWeightminusMean / (double)(ycTestModelViewlist[0].totaltestcount - 1));//Standard Deviation

                        decimal[] sd_weightArray = ycTestModelViewlist.Select(m => m.yarnweight).ToArray();
                        sd_weight = CalculateStandardDeviation(sd_weightArray);
                        sd_weight = formatDecimal(sd_weight);
                        mean_weight = formatDecimal(mean_weight);
                        cv_weight = (sd_weight / mean_weight) * 100.0000m; //Coefficient of Variation
                        cv_weight = formatDecimal(cv_weight);

                        min_weight = ycTestModelViewlist.Min(YCTestModelView => YCTestModelView.yarnweight);
                        max_weight = ycTestModelViewlist.Max(YCTestModelView => YCTestModelView.yarnweight);
                        range_weight = max_weight - min_weight;


                    }
                    //TimeSpan duration = (DateTime.Now - currentTestStartTime).Duration();
                    //string testDuration = duration.Hours.ToString() + ":" + duration.Minutes.ToString() + ":" + duration.Seconds.ToString();

                    string testDuration = formatTime(currentTestStartTime);

                    YCTestSummaryModel ycTestSummaryModel = new YCTestSummaryModel()
                    {
                        ID = Guid.NewGuid(),
                        testID = ycTestModelViewlist[0].testID,
                        userID = ycTestModelViewlist[0].userID,
                        userName = ycTestModelViewlist[0].userName,
                        machineID = ycTestModelViewlist[0].machineID,
                        machineCategory = ycTestModelViewlist[0].machineCategory,
                        machineName = ycTestModelViewlist[0].machineName,
                        shift = ycTestModelViewlist[0].shift,
                        process = ycTestModelViewlist[0].process,
                        countsysname = ycTestModelViewlist[0].countsysname,
                        yarnlenunit = ycTestModelViewlist[0].yarnlenunit,
                        yarnlength = ycTestModelViewlist[0].yarnlength,
                        totaltestcount = ycTestModelViewlist[0].totaltestcount,
                        yarnWeightAvg = mean_weight,
                        yarnWeightMin = min_weight,
                        yarnWeightMax = max_weight,
                        yarnWeightRange = range_weight,
                        yarnWeightSD = sd_weight,
                        yarnWeightCV = cv_weight,
                        testaverage = mean,
                        testMin = min,
                        testMax = max,
                        testRange = range,
                        testsd = sd,
                        testcv = cv,
                        standardHank = STD_HANK_CURR,
                        deviationPercent = selectedDeviationPercent,
                        standardCV=STD_CV,
                        CVDeviationPercent=STD_CV_DEVIATION,
                        testDuration = testDuration,
                        uf_value_1 = UFVAL1,
                        uf_value_2 = UFVAL2,
                        uf_value_3 = UFVAL3,
                        uf_value_4 = UFVAL4,
                        createdate = DateTime.Now
                    };
                    conn.CreateTable<YCTestSummaryModel>();
                    int row = conn.Insert(ycTestSummaryModel);
                    if (row < 1)
                    {
                        dbStatus = false;
                    }
                    if (dbStatus)
                    {
                        await refListView(true, true);
                        await refOverallSummary(mean, sd, cv, true, true);
                    }
                }

            }
        }


        // Method to calculate standard deviation
        private decimal CalculateStandardDeviation(decimal[] values)
        {
            decimal avg = values.Average();
            decimal sumOfSquares = (decimal)values.Select(val => Math.Pow((double)(val - avg), 2)).Sum();
            return (decimal)Math.Sqrt((double)(sumOfSquares / (values.Length - 1))); // Using sample SD (N-1)
        }


        private void reset(bool fullreset = true, bool dispose = true)
        {
            try
            {
                current_stable_data = 0;
                currentTestStartTime = null;
                if (fullreset) { ImageNotification(null); UpdateUserNotification(""); }
                if (dispose) { disposeble(); }
                Device.BeginInvokeOnMainThread(() =>
                {
                    btn_UF.IsVisible = true;
                    testYCButton.IsEnabled = true;
                    testYCButton.BackgroundColor = Color.Green;
                    entry_yarnlen.IsEnabled = true;
                    entry_testcount.IsEnabled = true;
                    entry_testcount.Text = TESTCOUNT.ToString();
                    entry_standardHank.IsEnabled = true;
                    entry_standardHank.Text = STD_HANK_CURR.ToString();
                    picker_machinecategory.IsEnabled = true;
                    //picker_machinecategory.SelectedIndex = 0;
                    picker_machinename.IsEnabled = true;
                    //picker_machinename.SelectedIndex = 0;
                    picker_shift.IsEnabled = false;
                    //picker_shift.SelectedIndex = 0;
                    //picker_process.SelectedIndex = 0;
                    picker_process.IsEnabled = true;
                    if (isTestStarted)
                    {
                        if (ycTestModelViewlist != null)
                        {
                            if (selectedTestCount != ycTestModelViewlist.Count())
                            {
                                ImageNotification("red.png");
                                UpdateUserNotification("IMPROPER TEST!!!");
                                showAlert("Improper Test!!!");
                            }
                            else
                            {
                                currentTestID = 0;
                                showAlert("Test Completed!!! Start new test");
                            }
                        }
                        else
                        {
                            ImageNotification("red.png");
                            UpdateUserNotification("IMPROPER TEST!!!");
                            showAlert("Improper Test!!!");
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
            ImageNotification("null");
            UpdateUserNotification("");
            hideFrames();
            await refListView(false);
            await refOverallSummary(0.0000m, 0.0000m, 0.0000m, false);

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

            currentTestStartTime = null;
            currentTestStartTime = DateTime.Now;
            lbl_TestID.Text = "";
            isTestStarted = true;

            if (selectedMachineID == Guid.Empty || selectedMachineCategory == null || selectedMachineCategory == "")
            {
                await DisplayAlert("Attention", "Please select machine category/ name to proceed!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (picker_yarncountunit.SelectedIndex <= 0)
            {
                await DisplayAlert("Attention", "Please select test unit!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (entry_yarnlen.Text.Trim().Contains("-"))
            {
                await DisplayAlert("Attention", "Yarn Length should not be a negative value!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (entry_yarnlen.Text.Trim() == "" || decimal.Parse(entry_yarnlen.Text.Trim()) == 0)
            {
                await DisplayAlert("Attention", "Yarn Length should not be blank or zero!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (entry_testcount.Text.Trim().Contains(".") || entry_testcount.Text.Trim().Contains("-"))
            {
                await DisplayAlert("Attention", "Total test count should not be a decimal or negative value!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (entry_testcount.Text.Trim() == "" || int.Parse(entry_testcount.Text.Trim()) == 0)
            {
                await DisplayAlert("Attention", "Total test count should not be blank or zero!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (entry_standardHank.Text.Trim() == "." || entry_standardHank.Text.Trim() == "-")
            {
                await DisplayAlert("Attention", "Standard Hank is invalid. Please check!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (entry_standardHank.Text.Trim() == "" || decimal.Parse(entry_standardHank.Text.Trim()) <= 0m)
            {
                await DisplayAlert("Attention", "Standard Hank should not be blank or zero or negative!!!", "Ok");
                _ = showProgress(false);
                return;
            }
            if (picker_shift.SelectedIndex <= 0)
            {
                await DisplayAlert("Attention", "Please select shift!!!", "Ok");
                _ = showProgress(false);
                return;
            }

            if (entry_standardHank.Text.Trim() == "" || decimal.Parse(entry_standardHank.Text.Trim()) == 0.0m)
            {
                await DisplayAlert("Attention", "Standard Hank should not be blank or zero!!!", "Ok");
                _ = showProgress(false);
                return;
            }

            //if (picker_process.SelectedIndex <= 0)
            //{
            //    await DisplayAlert("Attention", "Please select process info!!!", "Ok");
            //    return;
            //}

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

           

            string testCount_str = entry_testcount.Text;
            int testCount = int.Parse(testCount_str);

            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                YCTestModel lastTestRecord = null;
                conn.CreateTable<YCTestModel>();
                int recordCount = conn.Table<YCTestModel>().Count();

                if (recordCount == 0)
                {
                    currentTestID = 1;
                }
                else
                {
                    DateTime maxDate = conn.Table<YCTestModel>().Max(YCTestModel => YCTestModel.createdate);
                    if (DateTime.Now <= maxDate)
                    {
                        await DisplayAlert("Attention", "Tablet date time was modified. Please change it to actual current date and time to proceed!!!", "OK");
                        _ = showProgress(false);
                        return;
                    }
                    lastTestRecord = conn.Table<YCTestModel>()
                        .Where(YCTestModel => YCTestModel.createdate == maxDate).FirstOrDefault();
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
                        Debug.WriteLine("Data integrity check failed. Please logout, close and re-launch app to avoid data issues");
                        await DisplayAlert("Attention", "Data integrity check failed. Please logout, close and re-launch app to avoid data issues", "OK");
                        _ = showProgress(false);
                        return;
                    }
                }
                lbl_TestID.Text = currentTestID.ToString();
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
            

            selectedSysName = lbl_countsysname.Text;
            selectedCountUnit = picker_yarncountunit.SelectedItem.ToString();
            selectedYarnLen = decimal.Parse(entry_yarnlen.Text);
            selectedTestCount = int.Parse(entry_testcount.Text);
            STD_HANK_CURR = decimal.Parse(entry_standardHank.Text);
            selectedShift = picker_shift.SelectedItem.ToString();
            selectedProcess = "";
            if (picker_process.SelectedIndex > 0)
            {
                selectedProcess = picker_process.SelectedItem.ToString();
            }
            ycTestModelViewlist = new List<YCTestModelView>();
            testYCButton.IsEnabled = false;
            testYCButton.BackgroundColor = Color.SlateGray;
            entry_yarnlen.IsEnabled = false;
            entry_testcount.IsEnabled = false;
            entry_standardHank.IsEnabled = false;
            picker_shift.IsEnabled = false;
            picker_process.IsEnabled = false;
            picker_machinecategory.IsEnabled = false;
            picker_machinename.IsEnabled = false;
            btn_UF.IsVisible = false;

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
                for (int i = 0; i < testCount; i++)
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
                        decimal currentCalculatedValue = 0.0000m;
                        switch (selectedSysName)
                        {
                            case "Nec":
                                switch (selectedCountUnit)
                                {
                                    case "Yard":
                                        decimal drivedVal = (selectedYarnLen / 840.0000m) * (1.0000m / ((current_stable_data * 15.4324m) / 7000.0000m));
                                        currentCalculatedValue = formatDecimal(drivedVal);
                                        break;
                                    case "Meter":
                                        decimal drivedVal_meter = ((selectedYarnLen * 1.09361m) / 840.0000m) * (1.0000m / ((current_stable_data * 15.4324m) / 7000.0000m));
                                        currentCalculatedValue = formatDecimal(drivedVal_meter);
                                        break;
                                    default:
                                        break;
                                };
                                break;
                            case "Tex":
                                switch (selectedCountUnit)
                                {
                                    case "Yard":
                                        decimal drivedVal = current_stable_data * 1000.0000m / (selectedYarnLen * 0.9144m) * 1.0000m;
                                        currentCalculatedValue = formatDecimal(drivedVal);
                                        break;
                                    case "Meter":
                                        decimal drivedVal_meter = current_stable_data * 1000.0000m / selectedYarnLen * 1.0000m;
                                        currentCalculatedValue = formatDecimal(drivedVal_meter);
                                        break;
                                    default:
                                        break;
                                };
                                break;
                            case "Den":
                                switch (selectedCountUnit)
                                {
                                    case "Yard":
                                        decimal drivedVal = current_stable_data * 9000.0000m / (selectedYarnLen * 0.9144m) * 1.0000m;
                                        currentCalculatedValue = formatDecimal(drivedVal);
                                        break;
                                    case "Meter":
                                        decimal drivedVal_meter = current_stable_data * 9000.0000m / selectedYarnLen * 1.0000m;
                                        currentCalculatedValue = formatDecimal(drivedVal_meter);
                                        break;
                                    default:
                                        break;
                                };
                                break;
                            case "Nm":
                                switch (selectedCountUnit)
                                {
                                    case "Yard":
                                        decimal drivedVal = ((selectedYarnLen * 0.9144m) * 1.0000m) / ((current_stable_data * 0.0010m) * 1000.0000m);
                                        currentCalculatedValue = formatDecimal(drivedVal);
                                        break;
                                    case "Meter":
                                        decimal drivedVal_meter = (selectedYarnLen * 1.0000m) / ((current_stable_data * 0.0010m) * 1000.0000m);
                                        currentCalculatedValue = formatDecimal(drivedVal_meter);
                                        break;
                                    default:
                                        break;
                                };
                                break;
                            default:
                                break;
                        };
                        YCTestModelView ycTestModelView = new YCTestModelView()
                        {
                            testID = currentTestID,
                            userID = currentloggedInUser.ID,
                            userName = displayusername,
                            machineID = selectedMachineID,
                            machineCategory = selectedMachineCategory,
                            machineName = selectedMachineName,
                            shift = selectedShift,
                            process = selectedProcess,
                            countsysname = selectedSysName,
                            yarnlenunit = selectedCountUnit,
                            yarnlength = selectedYarnLen,
                            totaltestcount = selectedTestCount,
                            testcount = i + 1,
                            yarnweight = current_stable_data,
                            yccalcval = currentCalculatedValue
                        };
                        ycTestModelViewlist.Add(ycTestModelView);
                        //showAlert("Test - [" + (i + 1) + "] Completed!!! [" + current_stable_data + "]");
                        await refListView();
                    }
                    else
                    {
                        //showAlert("Test - [" + (i + 1) + "] Failed!!! Please start test from begining!!!");
                        reset(false);
                        break;
                    }
                }

                if (ycTestModelViewlist.Count > 0 && passCount == testCount)
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
                showAlert("COMMUNICATION ERROR!!!");
                reset();
            }
        }

        private async Task<bool> RunTest()
        {
            try
            {
                bool blueState = true;
                if (blueState)
                {
                    bool initialWeigthCheck = false;
                    int perTestLoopCount = 0;
                    while (true)
                    {
                        String balOutput = Listen(initialWeigthCheck);
                        Debug.WriteLine("Recieved from Bluetooth adapter is [" + balOutput + "]");
                        if (balOutput != "")
                        {
                            if (balOutput == "reset" && initialWeigthCheck) //Added to ignore negative values after placing weight
                            {
                                continue;
                            }

                            if (balOutput == "reset")
                            {
                                ImageNotification("red.png");
                                UpdateUserNotification("REMOVE WEIGHT");
                                Debug.WriteLine("Remove weigth to ensure zero!!!");
                            }
                            else if (balOutput == "fail")
                            {
                                ImageNotification("red.png");
                                UpdateUserNotification("COMMUNICATION ERROR!!!");
                                Debug.WriteLine("Read data failed");
                                return false;
                            }
                            else
                            {
                                decimal s_op = decimal.Parse(balOutput);
                                s_op = formatDecimal(s_op);
                                if (!initialWeigthCheck)
                                {
                                    if (s_op == ZERO)
                                    {
                                        initialWeigthCheck = true;
                                        ImageNotification("green.png");
                                        UpdateUserNotification("PLACE WEIGHT" + " (S.No - " + currentTestCount + ")", GREEN);
                                        Debug.WriteLine("Place object to start test!!!");
                                    }
                                    else
                                    {
                                        ImageNotification("red.png");
                                        UpdateUserNotification("REMOVE WEIGHT");
                                        Debug.WriteLine("Remove weigth to ensure zero!!!");
                                    }
                                }
                                else
                                {
                                    if (s_op == ZERO || s_op < MIN_VAL)
                                    {
                                        ImageNotification("green.png");
                                        UpdateUserNotification("PLACE WEIGHT" + " (S.No - " + currentTestCount + ")", GREEN);
                                        Debug.WriteLine("Place object to start test!!!");
                                    }
                                    //else if (s_op < MIN_VAL)
                                    //{
                                    //    initialWeigthCheck = false;
                                    //    ImageNotification("red.png");
                                    //    UpdateUserNotification("Weigth is below minimum value!!!");
                                    //    Debug.WriteLine("Weigth is below minimum value!!!");
                                    //}
                                    else
                                    {
                                        current_stable_data = s_op;
                                        ImageNotification(null);
                                        UpdateUserNotification("");
                                        return true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ImageNotification("red.png");
                            UpdateUserNotification("UNSTABLE DATA!!!");
                            Debug.WriteLine("Data is unstable!!! Ensure weighing machine is covered properly");
                        }
                        if (perTestLoopCount > PER_TEST_LOOP_COUNT)
                        {
                            ImageNotification("red.png");
                            UpdateUserNotification("IMPROPER TEST!!!");
                            Debug.WriteLine("Improper Test!!! Start new test");
                            return false;
                        }
                        perTestLoopCount += 1;
                    }
                }
                else
                {
                    ImageNotification("red.png");
                    UpdateUserNotification("COMMUNICATION ERROR!!!");
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

        private string Listen(bool iwc)
        {
            string op = "";
            string prevOp = "";
            bool Listening = true;
            Debug.WriteLine("Listening has been started.");
            while (Listening)
            {
                try
                {
                    int loopCount = 0;
                    int stableCount = 0;
                    int bufferfailedcount = 0;
                    while (true)
                    {
                        var buffer = new BufferedReader(new InputStreamReader(_socket.InputStream));
                        System.Threading.Thread.Sleep(1000);
                        if (buffer.Ready())
                        {
                            op = RemoveSpecialCharacters(buffer.ReadLine());
                            Debug.WriteLine("Output: " + op);
                            while (op != null)
                            {
                                if (op.Contains("-")) { return "reset"; }
                                else
                                {
                                    decimal op_dec = decimal.Parse(op);
                                    Debug.WriteLine("Output Modifed to int: [" + op_dec + "]: greater than 0?: " + (op_dec > 0));
                                    if (!iwc) { return op; }
                                }
                                if (prevOp == op)
                                {
                                    stableCount += 1;
                                    if (stableCount > STABLE_DATA_CHECK)
                                    {
                                        Debug.WriteLine("Stable Output: " + op.Trim('\0'));
                                        return op;
                                    }
                                }
                                else
                                {
                                    stableCount = 0;
                                }
                                prevOp = op;
                                op = RemoveSpecialCharacters(buffer.ReadLine());

                                if (loopCount > DATA_READ_LOOP_COUNT)
                                {
                                    return "";
                                }
                                loopCount += 1;
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
                selectedMachineCategory = picker_machinecategory.SelectedItem.ToString();
                if (selectedMachineCategory == "" || selectedMachineCategory == null)
                {
                    picker_machinename.ItemsSource = null;
                    lbl_standHank.Text = "Standard Hank";
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<MachineModel>();
                    List<MachineModel> machineModelList = conn.Table<MachineModel>().Where(MachineModel => MachineModel.machineCategory == selectedMachineCategory).ToList();
                    picker_machinename.ItemsSource = machineModelList;

                    //conn.CreateTable<YarnCountConfigModel>();
                    //YarnCountConfigModel yarncountconfigmodel = conn.Table<YarnCountConfigModel>().FirstOrDefault();
                    //if (yarncountconfigmodel != null)
                    //{
                    //    if (selectedMachineCategory == "Simplex/SpeedFrame")
                    //    {
                    //        entry_yarnlen.Text = yarncountconfigmodel.rovinglength.ToString();
                    //    }
                    //    else
                    //    {
                    //        entry_yarnlen.Text = yarncountconfigmodel.sliverlength.ToString();
                    //    }

                    //}
                    //else
                    //{
                    //    entry_yarnlen.Text = "";
                    //}
                }
                if (selectedMachineCategory == "Spinning" || selectedMachineCategory == "Winding")
                {
                    lbl_standHank.Text = "Standard Count";
                }
                else
                {
                    lbl_standHank.Text = "Standard Hank";
                }
                populateTestParams("", Guid.Empty, "");
                getUserfieldConfig("", Guid.Empty, "");
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }

        private void picker_machinename_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                List<MachineModel> source = (List<MachineModel>)picker_machinename.ItemsSource;
                if (picker_machinename.SelectedIndex < 0)
                {
                    selectedMachineID = Guid.Empty;
                    selectedMachineName = null;
                    populateTestParams("", Guid.Empty, "");
                    return;
                }
                selectedMachineID = (Guid)source[picker_machinename.SelectedIndex].ID;
                MachineModel selectedMachine = (MachineModel)picker_machinename.SelectedItem;
                selectedMachineName = selectedMachine.machineName;
                populateTestParams(selectedMachineCategory, selectedMachineID, selectedMachineName);
                getUserfieldConfig(selectedMachineCategory, selectedMachineID, selectedMachineName);
                btn_UF.IsVisible = true;
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
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

        private async void btn_comment_Clicked(object sender, EventArgs e)
        {
            try
            {
                string header = "Note [Test ID : " + lbl_TestID.Text + "]";
                long testID = long.Parse(lbl_TestID.Text.ToString());
                string comment = await DisplayPromptAsync(header, "Please type your remark", "Save", "Discard", null, 100);
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    YCTestSummaryModel summaryModel = conn.Table<YCTestSummaryModel>().Where(
                        YCTestSummaryModel => YCTestSummaryModel.testID == testID).FirstOrDefault();
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
                await DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }

        async void btn_UF_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                if(selectedMachineCategory==null || selectedMachineCategory=="" ||
                    selectedMachineID==Guid.Empty || selectedMachineName == null ||
                    selectedMachineName == "")
                {
                    await DisplayAlert("Attention", "Please select machine category and machine name to modify machine parameters", "OK");
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    YarnCountConfigModel macDetails = conn.Table<YarnCountConfigModel>().Where(YarnCountConfigModel =>
                                        (YarnCountConfigModel.machineCategory == selectedMachineCategory
                                        && YarnCountConfigModel.machineID == selectedMachineID
                                        && YarnCountConfigModel.machineName == selectedMachineName)).FirstOrDefault();
                    if (macDetails == null) { await DisplayAlert("Attention", "Error Occurred!!!Error: Unable to reterive machine details", "OK"); return; }
                    var result = await Navigation.ShowPopupAsync(new UserFieldsPopup(selectedMachineCategory,
                                                                                        selectedMachineID,
                                                                                        selectedMachineName,
                                                                                        macDetails.uf_name_1,
                                                                                        macDetails.uf_name_2,
                                                                                        macDetails.uf_name_3,
                                                                                        macDetails.uf_name_4,
                                                                                        macDetails.uf_value_1,
                                                                                        macDetails.uf_value_2,
                                                                                        macDetails.uf_value_3,
                                                                                        macDetails.uf_value_4));
                    if (result != null)
                    {
                        
                        if (!result.ToString().Contains("|"))
                        {
                            UFVAL1 = macDetails.uf_value_1;
                            UFVAL2 = macDetails.uf_value_2;
                            UFVAL3 = macDetails.uf_value_3;
                            UFVAL4 = macDetails.uf_value_4;
                            await DisplayAlert("Attention", result.ToString(), "OK");
                            return;
                        }

                        string res_msg = result.ToString().Split('~')[0];
                        string user_params = result.ToString().Split('~')[1];


                        if (res_msg == "Success")
                        {
                            UFVAL1 = user_params.Split('|')[0];
                            UFVAL2 = user_params.Split('|')[1];
                            UFVAL3 = user_params.Split('|')[2];
                            UFVAL4 = user_params.Split('|')[3];
                            return;
                        }
                        else
                        {
                            UFVAL1 = macDetails.uf_value_1;
                            UFVAL2 = macDetails.uf_value_2;
                            UFVAL3 = macDetails.uf_value_3;
                            UFVAL4 = macDetails.uf_value_4;
                            await DisplayAlert("Attention", result.ToString(), "OK");
                            return;
                        }
                    }
                    else
                    {
                        UFVAL1 = macDetails.uf_value_1;
                        UFVAL2 = macDetails.uf_value_2;
                        UFVAL3 = macDetails.uf_value_3;
                        UFVAL4 = macDetails.uf_value_4;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }
    }
}