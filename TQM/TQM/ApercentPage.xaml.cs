
using Android.Bluetooth;
using Java.IO;
using Java.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TQM.Model;
using TQM.ModelView;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ApercentPage : ContentPage
    {
        private BluetoothSocket _socket;
        BluetoothAdapter adapter;
        BluetoothDevice device;
        const decimal MIN_VAL = 0.400m;
        const decimal ZERO = 0.0m;
        const int PER_TEST_LOOP_COUNT = 100;
        const int DATA_READ_LOOP_COUNT = 100;
        const int STABLE_DATA_CHECK = 5;
        private decimal current_stable_data = 0;
        private List<YCTestApercentModelView> ycTestApercentModelViewlist;
        private long currentTestID = 0;
        private UserModel currentloggedInUser = null;
        private string selectedSysName = null;
        private string selectedCountUnit = null;
        private decimal selectedYarnLen = 0m;
        private int selectedTestCount = 0;
        private string selectedMachineCategory = null;
        private Guid selectedMachineID = Guid.Empty;
        private string selectedMachineName = null;
        private string selectedShift = null;
        private string selectedProcess = null;
        private string currentTestType = null;
        private const string RED = "#FF0000";
        private const string GREEN = "#145A32";
        private const int BUFFER_WAIT_COUNT = 10;
        private int TESTCOUNT = 0;
        private decimal STD_APERCENT_CURR = 0.0000m;
        private int currentTestCount = 0;
        private bool isTestStarted = false;
        private YCTestApercentCalculatedModel apercentCalc = null;
        private RunConfiguration runConfiguration = new RunConfiguration();
        public ApercentPage()
        {
            InitializeComponent();
            lbl_TestID.Text = "";
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                //conn.DropTable<YCTestApercentModel>();
                //conn.DropTable<YCTestApercentSummaryModel>();
                //conn.DropTable<YCTestApercentCalculatedModel>();

                //conn.CreateTable<YarnCountConfigModel>();
                //YarnCountConfigModel yarncountconfigmodel = conn.Table<YarnCountConfigModel>().FirstOrDefault();
                //if (yarncountconfigmodel != null)
                //{
                //    lbl_countsysname.Text = yarncountconfigmodel.countsysname;
                //    lbl_yarncountunit.Text = yarncountconfigmodel.yarnlenunit;
                //    entry_yarnlen.Text = "";
                //    entry_testcount.Text = yarncountconfigmodel.testcountApercent.ToString();
                //    TESTCOUNT = yarncountconfigmodel.testcountApercent;
                //}
                //else
                //{
                //    lbl_countsysname.Text = "";
                //    lbl_yarncountunit.Text = "";
                //    entry_yarnlen.Text = "";
                //    entry_testcount.Text = "";
                //    picker_shift.SelectedIndex = 0;
                //    picker_process.SelectedIndex = 0;
                //}

                conn.CreateTable<YCTestApercentModel>();
                conn.CreateTable<YCTestApercentSummaryModel>();
                conn.CreateTable<YCTestApercentCalculatedModel>();

                int recordCount = conn.Table<YCTestApercentModel>().Count();

                if (recordCount == 0)
                {
                    startTestNm1Button.IsVisible = true;
                }
                else
                {
                    DateTime maxDate = conn.Table<YCTestApercentModel>().Max(YCTestApercentModel => YCTestApercentModel.createdate);
                    YCTestApercentModel lastTest = conn.Table<YCTestApercentModel>()
                        .Where(YCTestApercentModel => YCTestApercentModel.createdate == maxDate).FirstOrDefault();

                    if (lastTest == null)
                    {
                        //to be decided
                    }
                    else
                    {
                        long lastTestID = lastTest.testID;
                        if (lastTest.testType == "nMinus1")
                        {
                            // check nMinus1 is having entry in YCTestApercentSummaryModel table
                            YCTestApercentSummaryModel lastTestSummary = conn.Table<YCTestApercentSummaryModel>().
                                            Where(YCTestApercentSummaryModel =>
                                            (YCTestApercentSummaryModel.testID == lastTest.testID
                                            && YCTestApercentSummaryModel.testType == "nMinus1")).FirstOrDefault();
                            if (lastTestSummary == null)
                            {
                                //get all test for nMinus1 from YCTestApercentModel table and delete 

                                List<YCTestApercentModel> allTest_nMinus1 = conn.Table<YCTestApercentModel>()
                                                                .Where(YCTestApercentModel =>
                                                                (YCTestApercentModel.testID == lastTestID
                                                                && YCTestApercentModel.testType == "nMinus1")).ToList();
                                foreach (YCTestApercentModel test in allTest_nMinus1)
                                {
                                    conn.Delete(test);
                                }
                                startTestNm1Button.IsVisible = true;
                            }
                            else
                            {
                                //Start with N Test
                                picker_machinecategory.SelectedItem = lastTest.machineCategory;
                                List<MachineModel> source = (List<MachineModel>)picker_machinename.ItemsSource;
                                int machineIndex = 0;
                                foreach (MachineModel item in source)
                                {
                                    if (item.machineName == lastTest.machineName)
                                    {
                                        machineIndex = machineIndex + 1;
                                        break;
                                    }
                                }
                                picker_machinename.SelectedIndex = machineIndex - 1;
                                picker_shift.SelectedItem = lastTest.shift;
                                picker_process.SelectedItem = lastTest.process;
                                entry_standardApercent.Text = formatDecimal(lastTest.standardApercent).ToString();
                                picker_machinecategory.IsEnabled = false;
                                picker_machinename.IsEnabled = false;
                                picker_shift.IsEnabled = false;
                                picker_process.IsEnabled = false;
                                entry_standardApercent.IsEnabled = false;
                                currentTestID = lastTest.testID;
                                entry_testcount.Text = lastTest.totaltestcount.ToString();
                                entry_testcount.IsEnabled = false;
                                entry_yarnlen.IsEnabled = false;
                                lbl_TestID.Text = currentTestID.ToString();
                                startTestNButton.IsVisible = true;
                            }
                        }
                        else if (lastTest.testType == "N")
                        {
                            // check N is having entry in YCTestApercentSummaryModel table
                            YCTestApercentSummaryModel lastTestSummary = conn.Table<YCTestApercentSummaryModel>().
                                            Where(YCTestApercentSummaryModel =>
                                            (YCTestApercentSummaryModel.testID == lastTest.testID
                                            && YCTestApercentSummaryModel.testType == "N")).FirstOrDefault();
                            if (lastTestSummary == null)
                            {
                                //get all test for N from YCTestApercentModel table and delete 

                                List<YCTestApercentModel> allTest_N = conn.Table<YCTestApercentModel>()
                                                                .Where(YCTestApercentModel =>
                                                                (YCTestApercentModel.testID == lastTestID
                                                                 && YCTestApercentModel.testType == "N")).ToList();
                                foreach (YCTestApercentModel test in allTest_N)
                                {
                                    conn.Delete(test);
                                }
                                picker_machinecategory.SelectedItem = lastTest.machineCategory;
                                List<MachineModel> source = (List<MachineModel>)picker_machinename.ItemsSource;
                                int machineIndex = 0;
                                foreach (MachineModel item in source)
                                {
                                    if (item.machineName == lastTest.machineName)
                                    {
                                        machineIndex = machineIndex + 1;
                                        break;
                                    }
                                }
                                picker_machinename.SelectedIndex = machineIndex - 1;
                                picker_shift.SelectedItem = lastTest.shift;
                                picker_process.SelectedItem = lastTest.process;
                                entry_standardApercent.Text = formatDecimal(lastTest.standardApercent).ToString();
                                picker_machinecategory.IsEnabled = false;
                                picker_machinename.IsEnabled = false;
                                picker_shift.IsEnabled = false;
                                picker_process.IsEnabled = false;
                                entry_standardApercent.IsEnabled = false;
                                currentTestID = lastTest.testID;
                                entry_testcount.Text = lastTest.totaltestcount.ToString();
                                entry_testcount.IsEnabled = false;
                                entry_yarnlen.IsEnabled = false;
                                lbl_TestID.Text = currentTestID.ToString();
                                startTestNButton.IsVisible = true;
                            }
                            else
                            {
                                //Start with N+1 Test
                                picker_machinecategory.SelectedItem = lastTest.machineCategory;
                                List<MachineModel> source = (List<MachineModel>)picker_machinename.ItemsSource;
                                int machineIndex = 0;
                                foreach (MachineModel item in source)
                                {
                                    if (item.machineName == lastTest.machineName)
                                    {
                                        machineIndex = machineIndex + 1;
                                        break;
                                    }
                                }
                                picker_machinename.SelectedIndex = machineIndex - 1;
                                picker_shift.SelectedItem = lastTest.shift;
                                picker_process.SelectedItem = lastTest.process;
                                entry_standardApercent.Text = formatDecimal(lastTest.standardApercent).ToString();
                                picker_machinecategory.IsEnabled = false;
                                picker_machinename.IsEnabled = false;
                                picker_shift.IsEnabled = false;
                                picker_process.IsEnabled = false;
                                entry_standardApercent.IsEnabled = false;
                                currentTestID = lastTest.testID;
                                entry_testcount.Text = lastTest.totaltestcount.ToString();
                                entry_testcount.IsEnabled = false;
                                entry_yarnlen.IsEnabled = false;
                                lbl_TestID.Text = currentTestID.ToString();
                                startTestNp1Button.IsVisible = true;
                            }
                        }
                        else if (lastTest.testType == "nPlus1")
                        {
                            // check N+1 is having entry in YCTestApercentSummaryModel table
                            YCTestApercentSummaryModel lastTestSummary = conn.Table<YCTestApercentSummaryModel>().
                                            Where(YCTestApercentSummaryModel =>
                                            (YCTestApercentSummaryModel.testID == lastTest.testID
                                            && YCTestApercentSummaryModel.testType == "nPlus1")).FirstOrDefault();
                            if (lastTestSummary == null)
                            {
                                //get all test for N+1 from YCTestApercentModel table and delete 

                                List<YCTestApercentModel> allTest_nPlus1 = conn.Table<YCTestApercentModel>()
                                                                .Where(YCTestApercentModel =>
                                                                (YCTestApercentModel.testID == lastTestID
                                                                 && YCTestApercentModel.testType == "nPlus1")).ToList();
                                foreach (YCTestApercentModel test in allTest_nPlus1)
                                {
                                    conn.Delete(test);
                                }
                                picker_machinecategory.SelectedItem = lastTest.machineCategory;
                                List<MachineModel> source = (List<MachineModel>)picker_machinename.ItemsSource;
                                int machineIndex = 0;
                                foreach (MachineModel item in source)
                                {
                                    if (item.machineName == lastTest.machineName)
                                    {
                                        machineIndex = machineIndex + 1;
                                        break;
                                    }
                                }
                                picker_machinename.SelectedIndex = machineIndex - 1;
                                picker_shift.SelectedItem = lastTest.shift;
                                picker_process.SelectedItem = lastTest.process;
                                entry_standardApercent.Text = formatDecimal(lastTest.standardApercent).ToString();
                                picker_machinecategory.IsEnabled = false;
                                picker_machinename.IsEnabled = false;
                                picker_shift.IsEnabled = false;
                                picker_process.IsEnabled = false;
                                entry_standardApercent.IsEnabled = false;
                                currentTestID = lastTest.testID;
                                entry_testcount.Text = lastTest.totaltestcount.ToString();
                                entry_testcount.IsEnabled = false;
                                entry_yarnlen.IsEnabled = false;
                                lbl_TestID.Text = currentTestID.ToString();
                                startTestNp1Button.IsVisible = true;
                            }
                            else
                            {
                                //Check IB and FB data stored in YCTestApercentCalculatedModel table

                                YCTestApercentCalculatedModel calculatedApercentTest = conn.Table<YCTestApercentCalculatedModel>().
                                                Where(YCTestApercentCalculatedModel =>
                                                YCTestApercentCalculatedModel.testID == lastTest.testID).FirstOrDefault();
                                if (calculatedApercentTest != null)
                                {
                                    startTestNm1Button.IsVisible = true;
                                }
                                else
                                {
                                    //delete records in YCTestApercentModel Table

                                    List<YCTestApercentModel> allLastTest = conn.Table<YCTestApercentModel>().
                                        Where(YCTestApercentModel =>
                                        (YCTestApercentModel.testID == lastTestID)).ToList();
                                    foreach (YCTestApercentModel test in allLastTest)
                                    {
                                        conn.Delete(test);
                                    }

                                    // delete records in StretchTestSummaryModel Table

                                    List<StretchTestSummaryModel> allLastTest_Summary = conn.Table<StretchTestSummaryModel>().
                                        Where(StretchTestSummaryModel =>
                                        (StretchTestSummaryModel.testID == lastTestID)).ToList();
                                    foreach (StretchTestSummaryModel test in allLastTest_Summary)
                                    {
                                        conn.Delete(test);
                                    }

                                    //start new test

                                    startTestNm1Button.IsVisible = true;

                                }
                            }
                        }
                    }
                }


            }
        }

        private void populateTestParams(string mCat, Guid mid, string mac)
        {
            if (mCat == "" && mid == Guid.Empty && mac == "")
            {
                lbl_countsysname.Text = "";
                lbl_yarncountunit.Text = "";
                entry_yarnlen.Text = "";
                entry_testcount.Text = "";
                picker_shift.SelectedIndex = 0;
                picker_process.SelectedIndex = 0;
                entry_standardApercent.Text = "0.0000";
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
                    entry_standardApercent.Text = formatDecimal(yarncountconfigmodel.standardApercent).ToString();
                    STD_APERCENT_CURR = formatDecimal(yarncountconfigmodel.standardApercent);
                    lbl_countsysname.Text = yarncountconfigmodel.countsysname;
                    lbl_yarncountunit.Text = yarncountconfigmodel.yarnlenunit;
                    if (mCat == "Simplex/SpeedFrame")
                    {
                        entry_yarnlen.Text = yarncountconfigmodel.rovinglength.ToString();
                    }
                    else if (mCat == "Spinning")
                    {
                        entry_yarnlen.Text = yarncountconfigmodel.lealength.ToString();
                    }
                    else
                    {
                        entry_yarnlen.Text = yarncountconfigmodel.sliverlength.ToString();
                    }
                    entry_testcount.Text = yarncountconfigmodel.testcount.ToString();
                    TESTCOUNT = yarncountconfigmodel.testcount;

                    TimeSpan shit1time = TimeSpan.FromHours(TimeSpan.Parse(yarncountconfigmodel.shift1time).TotalHours);
                    TimeSpan shit2time = TimeSpan.FromHours(TimeSpan.Parse(yarncountconfigmodel.shift2time).TotalHours);
                    TimeSpan shit3time = TimeSpan.FromHours(TimeSpan.Parse(yarncountconfigmodel.shift3time).TotalHours);
                    TimeSpan currentTime = TimeSpan.FromHours(TimeSpan.Parse(DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString()).TotalHours);
                    if (currentTime >= shit1time && (currentTime < shit2time || shit2time == TimeSpan.Zero))
                    {
                        picker_shift.SelectedItem = "Shift-1";
                    }
                    else if ((currentTime >= shit2time && shit2time != TimeSpan.Zero) && (currentTime < shit3time || shit3time == TimeSpan.Zero))
                    {
                        picker_shift.SelectedItem = "Shift-2";
                    }
                    else if (shit1time != TimeSpan.Zero && shit2time != TimeSpan.Zero && shit3time != TimeSpan.Zero)
                    {
                        picker_shift.SelectedItem = "Shift-3";
                    }
                }
                else
                {
                    lbl_countsysname.Text = "";
                    lbl_yarncountunit.Text = "";
                    entry_yarnlen.Text = "";
                    entry_testcount.Text = "";
                    picker_shift.SelectedIndex = 0;
                    picker_process.SelectedIndex = 0;
                    entry_standardApercent.Text = "0.0000";
                }
            }
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

        private async Task enableTestButton()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (currentTestType == "nMinus1")
                {
                    startTestNm1Button.IsVisible = false;
                    startTestNButton.IsVisible = true;
                    startTestNButton.IsEnabled = true;
                    startTestNButton.BackgroundColor = Color.Green;
                    startTestNp1Button.IsVisible = false;
                }
                else if (currentTestType == "N")
                {
                    startTestNm1Button.IsVisible = false;
                    startTestNButton.IsVisible = false;
                    startTestNp1Button.IsVisible = true;
                    startTestNp1Button.IsEnabled = true;
                    startTestNp1Button.BackgroundColor = Color.Green;
                }
                else if (currentTestType == "nPlus1")
                {
                    startTestNm1Button.IsVisible = true;
                    startTestNm1Button.IsEnabled = true;
                    startTestNm1Button.BackgroundColor = Color.Green;
                    startTestNButton.IsVisible = false;
                    startTestNp1Button.IsVisible = false;
                }
            });
        }

        private async void hideFrames()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                frame_overallSummary.IsVisible = false;
                individualTestResultFrame.IsVisible = false;
                frame_overallTestSummary.IsVisible = false;
                overallTestResultFrame.IsVisible = false;
            });
        }

        private async Task refListView(bool visibility = true, bool showFinalOut = false)
        {
            Device.BeginInvokeOnMainThread(() =>
            {

                if (currentTestType == "nPlus1" && showFinalOut)
                {
                    listview_testresult_individual.ItemsSource = null;
                    listview_testresult_individual.IsVisible = false;
                    individualTestResultFrame.IsVisible = false;

                    overallTestResultFrame.IsVisible = visibility;
                    listview_testresult_overall.ItemsSource = null;
                    listview_testresult_overall.IsVisible = visibility;
                    listview_testresult_overall.ItemsSource = generateResultView();
                }
                else
                {
                    listview_testresult_overall.ItemsSource = null;
                    listview_testresult_overall.IsVisible = false;
                    overallTestResultFrame.IsVisible = false;

                    individualTestResultFrame.IsVisible = visibility;
                    listview_testresult_individual.IsVisible = visibility;
                    if (ycTestApercentModelViewlist != null)
                    {
                        listview_testresult_individual.ItemsSource = null;
                        listview_testresult_individual.ItemsSource = ycTestApercentModelViewlist.OrderByDescending(YCTestApercentModelView => YCTestApercentModelView.testcount); ;
                    }

                    if (selectedMachineCategory == "Spinning")
                    {
                        lbl_testresult_stadHank.Text = "Count";
                    }
                    else
                    {
                        lbl_testresult_stadHank.Text = "Hank";
                    }
                }
            });
        }

        private async Task refOverallSummary(decimal mean = 0m, decimal sd = 0m, decimal cv = 0m, bool visibility = true, bool showFinalOut = false)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (showFinalOut == false)
                {
                    frame_overallSummary.IsVisible = visibility;
                    lbl_average.Text = mean.ToString();
                    lbl_sd.Text = sd.ToString();
                    lbl_cv.Text = cv.ToString();
                }
                else if (currentTestType == "nPlus1" && showFinalOut)
                {
                    frame_overallTestSummary.IsVisible = visibility;
                    lbl_ApercentNminus1.Text = formatDecimal(apercentCalc.apercent_nMinus1).ToString();
                    lbl_ApercentNplus1.Text = formatDecimal(apercentCalc.apercent_nPlus1).ToString();
                }
            });
        }

        private List<ApercentReportModelView> generateResultView()
        {
            List<ApercentReportModelView> OVS = new List<ApercentReportModelView>();
            //YCTestApercentCalculatedModel apercentCalc = null;
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                //apercentCalc = conn.Table<YCTestApercentCalculatedModel>()
                //    .Where(YCTestApercentCalculatedModel =>
                //    (YCTestApercentCalculatedModel.testID == currentTestID &&
                //    YCTestApercentCalculatedModel.status == true)).FirstOrDefault();
                if (apercentCalc == null) return null;
                //OverallApercentReportModelView report = new OverallApercentReportModelView();
                List<YCTestApercentModel> yctestApercentlist_nMinus1 = conn.Table<YCTestApercentModel>().Where(
                    YCTestApercentModel =>
                    (YCTestApercentModel.testID == currentTestID
                    && YCTestApercentModel.testType == "nMinus1"
                    && YCTestApercentModel.status == true)).ToList();
                List<YCTestApercentModel> yctestApercentlist_N = conn.Table<YCTestApercentModel>().Where(
                    YCTestApercentModel =>
                    (YCTestApercentModel.testID == currentTestID
                    && YCTestApercentModel.testType == "N"
                    && YCTestApercentModel.status == true)).ToList();
                List<YCTestApercentModel> yctestApercentlist_nPlus1 = conn.Table<YCTestApercentModel>().Where(
                    YCTestApercentModel =>
                    (YCTestApercentModel.testID == currentTestID
                    && YCTestApercentModel.testType == "nPlus1"
                    && YCTestApercentModel.status == true)).ToList();
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
                        OVS.Add(apercentReportMV);
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
                    OVS.Add(apercentReportModelView);

                    apercentReportModelView = new ApercentReportModelView()
                    {
                        testID = apercentCalc.testID,
                        description = "Weight (Max)",
                        nMinus1 = formatDecimal(apercentCalc.max_nMinus1),
                        N = formatDecimal(apercentCalc.max_N),
                        nPlus1 = formatDecimal(apercentCalc.max_nPlus1),
                    };
                    OVS.Add(apercentReportModelView);

                    apercentReportModelView = new ApercentReportModelView()
                    {
                        testID = apercentCalc.testID,
                        description = "Weight (Min)",
                        nMinus1 = formatDecimal(apercentCalc.min_nMinus1),
                        N = formatDecimal(apercentCalc.min_N),
                        nPlus1 = formatDecimal(apercentCalc.min_nPlus1),
                    };
                    OVS.Add(apercentReportModelView);

                    apercentReportModelView = new ApercentReportModelView()
                    {
                        testID = apercentCalc.testID,
                        description = "Range",
                        nMinus1 = formatDecimal(apercentCalc.range_nMinus1),
                        N = formatDecimal(apercentCalc.range_N),
                        nPlus1 = formatDecimal(apercentCalc.range_nPlus1),
                    };
                    OVS.Add(apercentReportModelView);

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
                        OVS.Add(apercentReportModelView);
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
                        OVS.Add(apercentReportModelView);
                    }

                    apercentReportModelView = new ApercentReportModelView()
                    {
                        testID = apercentCalc.testID,
                        description = "SD",
                        nMinus1 = formatDecimal(apercentCalc.testsd_nMinus1),
                        N = formatDecimal(apercentCalc.testsd_N),
                        nPlus1 = formatDecimal(apercentCalc.testsd_nPlus1),
                    };
                    OVS.Add(apercentReportModelView);

                    apercentReportModelView = new ApercentReportModelView()
                    {
                        testID = apercentCalc.testID,
                        description = "CV",
                        nMinus1 = formatDecimal(apercentCalc.testcv_nMinus1),
                        N = formatDecimal(apercentCalc.testcv_N),
                        nPlus1 = formatDecimal(apercentCalc.testcv_nPlus1),
                    };
                    OVS.Add(apercentReportModelView);



                }
                else
                {
                    return null;
                }
            }
            return OVS;
        }
        private async void updateDB()
        {

            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                bool dbStatus = true;
                decimal totalCalcCountVal = 0m;
                decimal totalWeight = 0m;
                conn.CreateTable<YCTestApercentModel>();
                List<YCTestApercentModel> ycTestApercentModelList = conn.Table<YCTestApercentModel>().Where(
                                YCTestApercentModel => (
                                YCTestApercentModel.status == true &&
                                YCTestApercentModel.testID != currentTestID &&
                                 YCTestApercentModel.machineID == selectedMachineID)).ToList();

                foreach (YCTestApercentModel ycTestApercentModel in ycTestApercentModelList)
                {
                    ycTestApercentModel.status = false;
                    ycTestApercentModel.dataSyncStatus = false;
                    if (conn.Update(ycTestApercentModel) < 1)
                    {
                        dbStatus = false;
                    }
                }


                if (dbStatus)
                {
                    foreach (YCTestApercentModelView test in ycTestApercentModelViewlist)
                    {
                        YCTestApercentModel ycTestApercentModel = new YCTestApercentModel()
                        {
                            ID = Guid.NewGuid(),
                            testID = test.testID,
                            userID = test.userID,
                            userName = test.userName,
                            machineID = test.machineID,
                            machineCategory = test.machineCategory,
                            machineName = test.machineName,
                            countsysname = test.countsysname,
                            yarnlenunit = test.yarnlenunit,
                            yarnlength = test.yarnlength,
                            shift = test.shift,
                            process = test.process,
                            testType = test.testType,
                            totaltestcount = test.totaltestcount,
                            testcount = test.testcount,
                            yarnweight = test.yarnweight,
                            yccalcval = test.yccalcval,
                            standardApercent = test.standardApercent,
                            status = true,
                            createdate = DateTime.Now
                        };
                        int row = conn.Insert(ycTestApercentModel);
                        if (row < 1)
                        {
                            dbStatus = false;
                        }
                        totalCalcCountVal = totalCalcCountVal + test.yccalcval;
                        totalWeight = totalWeight + test.yarnweight;
                    }
                }
                if (dbStatus)
                {
                    conn.CreateTable<YCTestApercentSummaryModel>();
                    List<YCTestApercentSummaryModel> apercentSummaryModelList = conn.Table<YCTestApercentSummaryModel>().Where(
                               YCTestApercentSummaryModel => (
                               YCTestApercentSummaryModel.status == true &&
                               YCTestApercentSummaryModel.testID != currentTestID &&
                                YCTestApercentSummaryModel.machineID == selectedMachineID)).ToList();

                    foreach (YCTestApercentSummaryModel apercentSummaryModel in apercentSummaryModelList)
                    {
                        apercentSummaryModel.status = false;
                        apercentSummaryModel.dataSyncStatus = false;
                        if (conn.Update(apercentSummaryModel) < 1)
                        {
                            dbStatus = false;
                        }
                    }

                    decimal avg_weight = 0m;
                    decimal mean = 0m;
                    decimal sd = 0m;
                    decimal cv = 0m;
                    if (ycTestApercentModelViewlist[0].totaltestcount > 1)
                    {
                        avg_weight = totalWeight / ycTestApercentModelViewlist[0].totaltestcount;
                        avg_weight = formatDecimal(avg_weight);
                        mean = totalCalcCountVal / ycTestApercentModelViewlist[0].totaltestcount;
                        mean = formatDecimal(mean);
                        decimal IndividualCalValminusMean = 0m;
                        foreach (YCTestApercentModelView test in ycTestApercentModelViewlist)
                        {
                            IndividualCalValminusMean = IndividualCalValminusMean + ((test.yarnweight - avg_weight) * (test.yarnweight - avg_weight));
                        }
                        sd = (decimal)Math.Sqrt((double)IndividualCalValminusMean / (double)(ycTestApercentModelViewlist[0].totaltestcount - 1));//Standard Deviation
                        sd = formatDecimal(sd);
                        cv = (sd / avg_weight) * 100m; //Coefficient of Variation
                        cv = formatDecimal(cv);
                    }
                    YCTestApercentSummaryModel ycTestApercentSummaryModel = new YCTestApercentSummaryModel()
                    {
                        ID = Guid.NewGuid(),
                        testID = ycTestApercentModelViewlist[0].testID,
                        userID = ycTestApercentModelViewlist[0].userID,
                        userName = ycTestApercentModelViewlist[0].userName,
                        machineID = ycTestApercentModelViewlist[0].machineID,
                        machineCategory = ycTestApercentModelViewlist[0].machineCategory,
                        machineName = ycTestApercentModelViewlist[0].machineName,
                        process = ycTestApercentModelViewlist[0].process,
                        countsysname = ycTestApercentModelViewlist[0].countsysname,
                        yarnlenunit = ycTestApercentModelViewlist[0].yarnlenunit,
                        yarnlength = ycTestApercentModelViewlist[0].yarnlength,
                        shift = ycTestApercentModelViewlist[0].shift,
                        testType = ycTestApercentModelViewlist[0].testType,
                        totaltestcount = ycTestApercentModelViewlist[0].totaltestcount,
                        standardApercent = ycTestApercentModelViewlist[0].standardApercent,
                        avg_weight = avg_weight,
                        testaverage = mean,
                        testsd = sd,
                        testcv = cv,
                        status = true,
                        createdate = DateTime.Now
                    };
                    conn.CreateTable<YCTestApercentSummaryModel>();
                    int row = conn.Insert(ycTestApercentSummaryModel);
                    if (row < 1)
                    {
                        dbStatus = false;
                    }
                    if (dbStatus)
                    {
                        //await enableTestButton();
                        //await refListView();
                        //await refOverallSummary(mean, sd, cv);
                        if (currentTestType == "nPlus1")
                        {
                            conn.CreateTable<YCTestApercentCalculatedModel>();
                            List<YCTestApercentCalculatedModel> apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(
                               YCTestApercentCalculatedModel => (
                               YCTestApercentCalculatedModel.status == true &&
                                YCTestApercentCalculatedModel.testID != currentTestID &&
                               YCTestApercentCalculatedModel.machineID == selectedMachineID)).ToList();
                            foreach (YCTestApercentCalculatedModel apercent in apercentCalcList)
                            {
                                apercent.status = false;
                                apercent.dataSyncStatus = false;
                                if (conn.Update(apercent) < 1)
                                {
                                    //to be decided if apercent calculated active records failed to deactive
                                }
                            }
                            YCTestApercentSummaryModel nMinus1Summary = conn.Table<YCTestApercentSummaryModel>().Where(
                                                            YCTestApercentSummaryModel => (
                                                            YCTestApercentSummaryModel.testType == "nMinus1"
                                                            && YCTestApercentSummaryModel.status == true
                                                            && YCTestApercentSummaryModel.testID == currentTestID)
                                                            ).FirstOrDefault();
                            if (nMinus1Summary != null)
                            {
                                YCTestApercentSummaryModel NSummary = conn.Table<YCTestApercentSummaryModel>().Where(
                                                            YCTestApercentSummaryModel => (
                                                            YCTestApercentSummaryModel.testType == "N"
                                                            && YCTestApercentSummaryModel.status == true
                                                            && YCTestApercentSummaryModel.testID == currentTestID)
                                                            ).FirstOrDefault();
                                if (NSummary != null)
                                {
                                    YCTestApercentSummaryModel nPlus1Summary = conn.Table<YCTestApercentSummaryModel>().Where(
                                                                YCTestApercentSummaryModel => (
                                                                YCTestApercentSummaryModel.testType == "nPlus1"
                                                                && YCTestApercentSummaryModel.status == true
                                                                && YCTestApercentSummaryModel.testID == currentTestID)
                                                                ).FirstOrDefault();
                                    if (nPlus1Summary != null)
                                    {
                                        decimal apercent_nMinus1 = ((nMinus1Summary.avg_weight - NSummary.avg_weight) / nMinus1Summary.avg_weight) * 100m;
                                        apercent_nMinus1 = formatDecimal(apercent_nMinus1);
                                        decimal apercent_nPlus1 = ((nPlus1Summary.avg_weight - NSummary.avg_weight) / nPlus1Summary.avg_weight) * 100m;
                                        apercent_nPlus1 = formatDecimal(apercent_nPlus1);
                                        YCTestApercentModel Max_nMinus1 = conn.Table<YCTestApercentModel>().Where(
                                            YCTestApercentModel =>
                                            (YCTestApercentModel.testID == currentTestID &&
                                            YCTestApercentModel.status == true &&
                                            YCTestApercentModel.testType == "nMinus1")).OrderByDescending(YCTestApercentModel => YCTestApercentModel.yarnweight).First();
                                        YCTestApercentModel Min_nMinus1 = conn.Table<YCTestApercentModel>().Where(
                                            YCTestApercentModel =>
                                            (YCTestApercentModel.testID == currentTestID &&
                                            YCTestApercentModel.status == true &&
                                            YCTestApercentModel.testType == "nMinus1")).OrderBy(YCTestApercentModel => YCTestApercentModel.yarnweight).First();
                                        YCTestApercentModel Max_N = conn.Table<YCTestApercentModel>().Where(
                                            YCTestApercentModel =>
                                            (YCTestApercentModel.testID == currentTestID &&
                                            YCTestApercentModel.status == true &&
                                            YCTestApercentModel.testType == "N")).OrderByDescending(YCTestApercentModel => YCTestApercentModel.yarnweight).First();
                                        YCTestApercentModel Min_N = conn.Table<YCTestApercentModel>().Where(
                                            YCTestApercentModel =>
                                            (YCTestApercentModel.testID == currentTestID &&
                                            YCTestApercentModel.status == true &&
                                            YCTestApercentModel.testType == "N")).OrderBy(YCTestApercentModel => YCTestApercentModel.yarnweight).First();
                                        YCTestApercentModel Max_nPlus1 = conn.Table<YCTestApercentModel>().Where(
                                           YCTestApercentModel =>
                                           (YCTestApercentModel.testID == currentTestID &&
                                           YCTestApercentModel.status == true &&
                                           YCTestApercentModel.testType == "nPlus1")).OrderByDescending(YCTestApercentModel => YCTestApercentModel.yarnweight).First();
                                        YCTestApercentModel Min_nPlus1 = conn.Table<YCTestApercentModel>().Where(
                                            YCTestApercentModel =>
                                            (YCTestApercentModel.testID == currentTestID &&
                                            YCTestApercentModel.status == true &&
                                            YCTestApercentModel.testType == "nPlus1")).OrderBy(YCTestApercentModel => YCTestApercentModel.yarnweight).First();
                                        decimal range_nMinus1 = Max_nMinus1.yarnweight - Min_nMinus1.yarnweight;
                                        decimal range_N = Max_N.yarnweight - Min_N.yarnweight;
                                        decimal range_nPlus1 = Max_nPlus1.yarnweight - Min_nPlus1.yarnweight;
                                        YCTestApercentCalculatedModel yCTestApercentCalculatedModel = new YCTestApercentCalculatedModel()
                                        {
                                            ID = Guid.NewGuid(),
                                            testID = nMinus1Summary.testID,
                                            userID = nMinus1Summary.userID,
                                            userName = nMinus1Summary.userName,
                                            machineID = nMinus1Summary.machineID,
                                            machineCategory = nMinus1Summary.machineCategory,
                                            machineName = nMinus1Summary.machineName,
                                            process = nMinus1Summary.process,
                                            countsysname = nMinus1Summary.countsysname,
                                            yarnlenunit = nMinus1Summary.yarnlenunit,
                                            yarnlength = nMinus1Summary.yarnlength,
                                            shift = nMinus1Summary.shift,
                                            testType = nMinus1Summary.testType,
                                            totaltestcount = nMinus1Summary.totaltestcount,
                                            standardApercent = nMinus1Summary.standardApercent,
                                            avg_weight_nMinus1 = nMinus1Summary.avg_weight,
                                            testaverage_nMinus1 = nMinus1Summary.testaverage,
                                            testsd_nMinus1 = nMinus1Summary.testsd,
                                            testcv_nMinus1 = nMinus1Summary.testcv,
                                            max_nMinus1 = Max_nMinus1.yarnweight,
                                            min_nMinus1 = Min_nMinus1.yarnweight,
                                            range_nMinus1 = range_nMinus1,
                                            apercent_nMinus1 = apercent_nMinus1,
                                            avg_weight_N = NSummary.avg_weight,
                                            testaverage_N = NSummary.testaverage,
                                            testsd_N = NSummary.testsd,
                                            testcv_N = NSummary.testcv,
                                            max_N = Max_N.yarnweight,
                                            min_N = Min_N.yarnweight,
                                            range_N = range_N,
                                            avg_weight_nPlus1 = nPlus1Summary.avg_weight,
                                            testaverage_nPlus1 = nPlus1Summary.testaverage,
                                            testsd_nPlus1 = nPlus1Summary.testsd,
                                            testcv_nPlus1 = nPlus1Summary.testcv,
                                            max_nPlus1 = Max_nPlus1.yarnweight,
                                            min_nPlus1 = Min_nPlus1.yarnweight,
                                            range_nPlus1 = range_nPlus1,
                                            apercent_nPlus1 = apercent_nPlus1,
                                            status = true,
                                            createdate = DateTime.Now
                                        };
                                        int row_nMinus1 = conn.Insert(yCTestApercentCalculatedModel);
                                        if (row_nMinus1 < 1)
                                        {
                                            // To be decieded if nMinus1Summary failed to insert to db
                                        }
                                        else
                                        {
                                            apercentCalc = yCTestApercentCalculatedModel;
                                        }
                                    }
                                    else
                                    {
                                        // To be decieded if nPlus1Summary active record is not available in db
                                    }
                                }
                                else
                                {
                                    // To be decieded if NSummary active record is not available in db
                                }
                            }
                            else
                            {
                                // To be decieded if nMinus1Summary active record is not available in db
                            }
                        }
                        await enableTestButton();
                        await refListView(true, true);
                        await refOverallSummary(mean, sd, cv, true, true);
                    }
                }

            }
        }

        private void reset(bool fullreset = true, bool dispose = true)
        {
            try
            {
                current_stable_data = 0;
                if (fullreset)
                {
                    ImageNotification(null);
                    UpdateUserNotification("");
                }
                else
                {
                    if (currentTestType == "nMinus1")
                    {
                        startTestNm1Button.IsVisible = true;
                        startTestNm1Button.IsEnabled = true;
                        startTestNm1Button.BackgroundColor = Color.Green;
                        startTestNButton.IsVisible = false;
                        startTestNp1Button.IsVisible = false;
                    }
                    else if (currentTestType == "N")
                    {
                        startTestNButton.IsVisible = true;
                        startTestNButton.IsEnabled = true;
                        startTestNButton.BackgroundColor = Color.Green;
                        startTestNm1Button.IsVisible = false;
                        startTestNp1Button.IsVisible = false;
                    }
                    else if (currentTestType == "nPlus1")
                    {
                        startTestNp1Button.IsVisible = true;
                        startTestNp1Button.IsEnabled = true;
                        startTestNp1Button.BackgroundColor = Color.Green;
                        startTestNm1Button.IsVisible = false;
                        startTestNButton.IsVisible = false;
                    }
                }
                if (dispose) { disposeble(); }
                Device.BeginInvokeOnMainThread(() =>
                {
                    string str_testType = "";
                    if (currentTestType == "nMinus1")
                    {
                        str_testType = "Test Completed for (N-1)!!! Start test for (N)";
                    }
                    else if (currentTestType == "N")
                    {
                        str_testType = "Test Completed for (N)!!! Start test for (N+1)";
                    }
                    else if (currentTestType == "nPlus1")
                    {
                        str_testType = "All Test Completed!!!";
                    }
                    if (isTestStarted)
                    {
                        isTestStarted = false;
                        if (ycTestApercentModelViewlist != null)
                        {
                            if (selectedTestCount != ycTestApercentModelViewlist.Count())
                            {
                                ImageNotification("red.png");
                                UpdateUserNotification("IMPROPER TEST!!!");
                                showAlert("Improper Test!!!");
                            }
                            else
                            {
                                showAlert(str_testType);
                                if (currentTestType == "nPlus1")
                                {
                                    currentTestID = 0;
                                    entry_yarnlen.IsEnabled = true;
                                    entry_testcount.IsEnabled = true;
                                    entry_testcount.Text = TESTCOUNT.ToString();
                                    picker_machinecategory.IsEnabled = true;
                                    picker_machinecategory.SelectedIndex = 0;
                                    picker_machinename.IsEnabled = true;
                                    picker_machinename.SelectedIndex = 0;
                                    picker_shift.IsEnabled = true;
                                    picker_shift.SelectedIndex = 0;
                                    picker_process.SelectedIndex = 0;
                                    picker_process.IsEnabled = true;
                                    entry_standardApercent.Text = "0.0000";
                                    entry_standardApercent.IsEnabled = true;
                                }
                            }
                        }
                        else
                        {
                            ImageNotification("red.png");
                            UpdateUserNotification("IMPROPER TEST!!!");
                            showAlert("Improper Test!!!");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        [Obsolete]
        private async void startTestNm1Button_Clicked(object sender, EventArgs e)
        {
            lbl_TestID.Text = "";
            isTestStarted = true;
            currentTestType = "nMinus1";
            ImageNotification("null");
            UpdateUserNotification("");
            hideFrames();
            await refListView(false);
            await refOverallSummary(0m, 0m, 0m, false);
            if (entry_yarnlen.Text.Trim().Contains(".") || entry_yarnlen.Text.Trim().Contains("-"))
            {
                await DisplayAlert("Attention", "Yarn Length should not be a decimal or negative value!!!", "Ok");
                return;
            }
            if (entry_yarnlen.Text.Trim() == "" || int.Parse(entry_yarnlen.Text.Trim()) == 0)
            {
                await DisplayAlert("Attention", "Yarn Length should not be blank or zero!!!", "Ok");
                return;
            }
            if (entry_testcount.Text.Trim().Contains(".") || entry_testcount.Text.Trim().Contains("-"))
            {
                await DisplayAlert("Attention", "Total test count should not be a decimal or negative value!!!", "Ok");
                return;
            }
            if (entry_testcount.Text.Trim() == "" || int.Parse(entry_testcount.Text.Trim()) == 0)
            {
                await DisplayAlert("Attention", "Total test count should not be blank or zero!!!", "Ok");
                return;
            }
            if (selectedMachineID == Guid.Empty || selectedMachineCategory == null || selectedMachineCategory == "")
            {
                await DisplayAlert("Attention", "Please select machine category/ name to proceed!!!", "Ok");
                return;
            }
            if (picker_shift.SelectedIndex <= 0)
            {
                await DisplayAlert("Attention", "Please select shift!!!", "Ok");
                return;
            }
            if (entry_standardApercent.Text.Trim() == "-")
            {
                await DisplayAlert("Attention", "Standard A% is invalid. Please check!!!", "Ok");
                return;
            }
            if (entry_standardApercent.Text.Trim() == "" || decimal.Parse(entry_standardApercent.Text.Trim()) <= 0m)
            {
                await DisplayAlert("Attention", "Standard A% should not be blank or zero or negative!!!", "Ok");
                return;
            }
            //if (picker_process.SelectedIndex <= 0)
            //{
            //    await DisplayAlert("Attention", "Please enter process info!!!", "Ok");
            //    return;
            //}
            if (!initializeBluetooth())
            {
                ImageNotification("red.png");
                UpdateUserNotification("COMMUNICATION ERROR!!!");
                return;
            }
            string testCount_str = entry_testcount.Text;
            int testCount = int.Parse(testCount_str);

            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                YCTestApercentModel lastTestRecord = null;
                conn.CreateTable<YCTestApercentModel>();
                int recordCount = conn.Table<YCTestApercentModel>().Count();

                if (recordCount == 0)
                {
                    currentTestID = 1;
                }
                else
                {
                    DateTime maxDate = conn.Table<YCTestApercentModel>().Max(YCTestApercentModel => YCTestApercentModel.createdate);
                    lastTestRecord = conn.Table<YCTestApercentModel>()
                        .Where(YCTestApercentModel => YCTestApercentModel.createdate == maxDate).FirstOrDefault();
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
                lbl_TestID.Text = currentTestID.ToString();
                UserModel loggedInUser = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                if (loggedInUser == null)
                {
                    await DisplayAlert("Attention", "Unable to get logged user information!!!", "OK");
                    return;
                }
                else
                {
                    currentloggedInUser = loggedInUser;
                }
            }
            selectedSysName = lbl_countsysname.Text;
            selectedCountUnit = lbl_yarncountunit.Text;
            selectedYarnLen = int.Parse(entry_yarnlen.Text);
            selectedTestCount = int.Parse(entry_testcount.Text);
            selectedShift = picker_shift.SelectedItem.ToString();
            selectedProcess = "";
            if (picker_process.SelectedIndex > 0)
            {
                selectedProcess = picker_process.SelectedItem.ToString();
            }
            STD_APERCENT_CURR = decimal.Parse(entry_standardApercent.Text);
            ycTestApercentModelViewlist = new List<YCTestApercentModelView>();
            startTestNm1Button.IsEnabled = false;
            startTestNm1Button.BackgroundColor = Color.SlateGray;
            entry_yarnlen.IsEnabled = false;
            entry_testcount.IsEnabled = false;
            picker_machinecategory.IsEnabled = false;
            picker_machinename.IsEnabled = false;
            picker_shift.IsEnabled = false;
            picker_process.IsEnabled = false;
            entry_standardApercent.IsEnabled = false;
            CancellationTokenSource src = new CancellationTokenSource();
            CancellationToken ct = src.Token;
            ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
            await Task.Run(async () => await HandleTest(testCount), ct);
            src.Cancel();
        }

        private async Task HandleTest(int testCount)
        {
            try
            {
                ImageNotification("loading.gif");
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
                        decimal currentCalculatedValue = 0;
                        switch (selectedSysName)
                        {
                            case "Nec":
                                switch (selectedCountUnit)
                                {
                                    case "Yard":
                                        decimal drivedVal = (selectedYarnLen / 840m) * (1m / ((current_stable_data * 15.4324m) / 7000m));
                                        currentCalculatedValue = formatDecimal(drivedVal);
                                        break;
                                    case "Meter":
                                        decimal drivedVal_meter = ((selectedYarnLen * 1.09361m) / 840m) * (1m / ((current_stable_data * 15.4324m) / 7000m));
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
                                        decimal drivedVal = current_stable_data * 1000m / (selectedYarnLen * 0.9144m) * 1m;
                                        currentCalculatedValue = formatDecimal(drivedVal);
                                        break;
                                    case "Meter":
                                        decimal drivedVal_meter = current_stable_data * 1000m / selectedYarnLen * 1m;
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
                                        decimal drivedVal = current_stable_data * 9000m / (selectedYarnLen * 0.9144m) * 1m;
                                        currentCalculatedValue = formatDecimal(drivedVal);
                                        break;
                                    case "Meter":
                                        decimal drivedVal_meter = current_stable_data * 9000m / selectedYarnLen * 1m;
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
                                        decimal drivedVal = ((selectedYarnLen * 0.9144m) * 1m) / ((current_stable_data * 0.001m) * 1000m);
                                        currentCalculatedValue = formatDecimal(drivedVal);
                                        break;
                                    case "Meter":
                                        decimal drivedVal_meter = (selectedYarnLen * 1m) / ((current_stable_data * 0.001m) * 1000m);
                                        currentCalculatedValue = formatDecimal(drivedVal_meter);
                                        break;
                                    default:
                                        break;
                                };
                                break;
                            default:
                                break;
                        };
                        YCTestApercentModelView ycTestApercentModelView = new YCTestApercentModelView()
                        {
                            testID = currentTestID,
                            userID = currentloggedInUser.ID,
                            userName = displayusername,
                            machineID = selectedMachineID,
                            machineCategory = selectedMachineCategory,
                            machineName = selectedMachineName,
                            countsysname = selectedSysName,
                            yarnlenunit = selectedCountUnit,
                            yarnlength = selectedYarnLen,
                            shift = selectedShift,
                            process = selectedProcess,
                            testType = currentTestType,
                            totaltestcount = selectedTestCount,
                            testcount = i + 1,
                            yarnweight = current_stable_data,
                            yccalcval = currentCalculatedValue,
                            standardApercent = STD_APERCENT_CURR
                        };
                        ycTestApercentModelViewlist.Add(ycTestApercentModelView);
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

                if (ycTestApercentModelViewlist.Count > 0 && passCount == testCount)
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

        private async void startTestNButton_Clicked(object sender, EventArgs e)
        {
            isTestStarted = true;
            currentTestType = "N";
            ImageNotification("null");
            UpdateUserNotification("");
            await refListView(false);
            await refOverallSummary(0m, 0m, 0m, false);
            if (entry_yarnlen.Text.Trim().Contains(".") || entry_yarnlen.Text.Trim().Contains("-"))
            {
                await DisplayAlert("Attention", "Yarn Length should not be a decimal or negative value!!!", "Ok");
                return;
            }
            if (entry_yarnlen.Text.Trim() == "" || int.Parse(entry_yarnlen.Text.Trim()) == 0)
            {
                await DisplayAlert("Attention", "Yarn Length should not be blank or zero!!!", "Ok");
                return;
            }
            if (entry_testcount.Text.Trim().Contains(".") || entry_testcount.Text.Trim().Contains("-"))
            {
                await DisplayAlert("Attention", "Total test count should not be a decimal or negative value!!!", "Ok");
                return;
            }
            if (entry_testcount.Text.Trim() == "" || int.Parse(entry_testcount.Text.Trim()) == 0)
            {
                await DisplayAlert("Attention", "Total test count should not be blank or zero!!!", "Ok");
                return;
            }
            if (selectedMachineID == Guid.Empty || selectedMachineCategory == null || selectedMachineCategory == "")
            {
                await DisplayAlert("Attention", "Please select machine category/ name to proceed!!!", "Ok");
                return;
            }
            if (picker_shift.SelectedIndex <= 0)
            {
                await DisplayAlert("Attention", "Please select shift!!!", "Ok");
                return;
            }
            if (entry_standardApercent.Text.Trim() == "-")
            {
                await DisplayAlert("Attention", "Standard A% is invalid. Please check!!!", "Ok");
                return;
            }
            if (entry_standardApercent.Text.Trim() == "" || decimal.Parse(entry_standardApercent.Text.Trim()) <= 0m)
            {
                await DisplayAlert("Attention", "Standard A% should not be blank or zero or negative!!!", "Ok");
                return;
            }
            //if (picker_process.SelectedIndex <= 0)
            //{
            //    await DisplayAlert("Attention", "Please enter process info!!!", "Ok");
            //    return;
            //}
            if (!initializeBluetooth())
            {
                ImageNotification("red.png");
                UpdateUserNotification("COMMUNICATION ERROR!!!");
                return;
            }
            string testCount_str = entry_testcount.Text;
            int testCount = int.Parse(testCount_str);

            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<YCTestApercentModel>();
                DateTime maxDate = conn.Table<YCTestApercentModel>().Max(YCTestApercentModel => YCTestApercentModel.createdate);
                YCTestApercentModel lastTestRecord = conn.Table<YCTestApercentModel>()
                    .Where(YCTestApercentModel => YCTestApercentModel.createdate == maxDate).FirstOrDefault();
                //YCTestApercentModel lastTestRecord = conn.Table<YCTestApercentModel>().OrderByDescending(YCTestApercentModel => YCTestApercentModel.testID).FirstOrDefault();
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
                    currentTestID = 1;
                }
                UserModel loggedInUser = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                if (loggedInUser == null)
                {
                    await DisplayAlert("Attention", "Unable to get logged user information!!!", "OK");
                    return;
                }
                else
                {
                    currentloggedInUser = loggedInUser;
                }
            }
            selectedSysName = lbl_countsysname.Text;
            selectedCountUnit = lbl_yarncountunit.Text;
            selectedYarnLen = int.Parse(entry_yarnlen.Text);
            selectedTestCount = int.Parse(entry_testcount.Text);
            selectedShift = picker_shift.SelectedItem.ToString();
            selectedProcess = "";
            if (picker_process.SelectedIndex > 0)
            {
                selectedProcess = picker_process.SelectedItem.ToString();
            }
            STD_APERCENT_CURR = decimal.Parse(entry_standardApercent.Text);
            ycTestApercentModelViewlist = new List<YCTestApercentModelView>();
            startTestNButton.IsEnabled = false;
            startTestNButton.BackgroundColor = Color.SlateGray;
            entry_yarnlen.IsEnabled = false;
            entry_testcount.IsEnabled = false;
            picker_machinecategory.IsEnabled = false;
            picker_machinename.IsEnabled = false;
            picker_shift.IsEnabled = false;
            picker_process.IsEnabled = false;
            entry_standardApercent.IsEnabled = false;
            CancellationTokenSource src = new CancellationTokenSource();
            CancellationToken ct = src.Token;
            ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
            await Task.Run(async () => await HandleTest(testCount), ct);
            src.Cancel();
        }

        private async void startTestNp1Button_Clicked(object sender, EventArgs e)
        {
            isTestStarted = true;
            currentTestType = "nPlus1";
            ImageNotification("null");
            UpdateUserNotification("");
            await refListView(false);
            await refOverallSummary(0m, 0m, 0m, false);
            if (entry_yarnlen.Text.Trim().Contains(".") || entry_yarnlen.Text.Trim().Contains("-"))
            {
                await DisplayAlert("Attention", "Yarn Length should not be a decimal or negative value!!!", "Ok");
                return;
            }
            if (entry_yarnlen.Text.Trim() == "" || int.Parse(entry_yarnlen.Text.Trim()) == 0)
            {
                await DisplayAlert("Attention", "Yarn Length should not be blank or zero!!!", "Ok");
                return;
            }
            if (entry_testcount.Text.Trim().Contains(".") || entry_testcount.Text.Trim().Contains("-"))
            {
                await DisplayAlert("Attention", "Total test count should not be a decimal or negative value!!!", "Ok");
                return;
            }
            if (entry_testcount.Text.Trim() == "" || int.Parse(entry_testcount.Text.Trim()) == 0)
            {
                await DisplayAlert("Attention", "Total test count should not be blank or zero!!!", "Ok");
                return;
            }
            if (selectedMachineID == Guid.Empty || selectedMachineCategory == null || selectedMachineCategory == "")
            {
                await DisplayAlert("Attention", "Please select machine category/ name to proceed!!!", "Ok");
                return;
            }
            if (picker_shift.SelectedIndex <= 0)
            {
                await DisplayAlert("Attention", "Please select shift!!!", "Ok");
                return;
            }
            if (entry_standardApercent.Text.Trim() == "-")
            {
                await DisplayAlert("Attention", "Standard A% is invalid. Please check!!!", "Ok");
                return;
            }
            if (entry_standardApercent.Text.Trim() == "" || decimal.Parse(entry_standardApercent.Text.Trim()) <= 0m)
            {
                await DisplayAlert("Attention", "Standard A% should not be blank or zero or negative!!!", "Ok");
                return;
            }
            //if (picker_process.SelectedIndex <= 0)
            //{
            //    await DisplayAlert("Attention", "Please enter process info!!!", "Ok");
            //    return;
            //}
            if (!initializeBluetooth())
            {
                ImageNotification("red.png");
                UpdateUserNotification("COMMUNICATION ERROR!!!");
                return;
            }
            string testCount_str = entry_testcount.Text;
            int testCount = int.Parse(testCount_str);

            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<YCTestApercentModel>();
                DateTime maxDate = conn.Table<YCTestApercentModel>().Max(YCTestApercentModel => YCTestApercentModel.createdate);
                YCTestApercentModel lastTestRecord = conn.Table<YCTestApercentModel>()
                    .Where(YCTestApercentModel => YCTestApercentModel.createdate == maxDate).FirstOrDefault();
                //YCTestApercentModel lastTestRecord = conn.Table<YCTestApercentModel>().OrderByDescending(YCTestApercentModel => YCTestApercentModel.testID).FirstOrDefault();
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
                    currentTestID = 1;
                }
                UserModel loggedInUser = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                if (loggedInUser == null)
                {
                    await DisplayAlert("Attention", "Unable to get logged user information!!!", "OK");
                    return;
                }
                else
                {
                    currentloggedInUser = loggedInUser;
                }
            }
            selectedSysName = lbl_countsysname.Text;
            selectedCountUnit = lbl_yarncountunit.Text;
            selectedYarnLen = int.Parse(entry_yarnlen.Text);
            selectedTestCount = int.Parse(entry_testcount.Text);
            selectedShift = picker_shift.SelectedItem.ToString();
            selectedProcess = "";
            if (picker_process.SelectedIndex > 0)
            {
                selectedProcess = picker_process.SelectedItem.ToString();
            }
            STD_APERCENT_CURR = decimal.Parse(entry_standardApercent.Text);
            ycTestApercentModelViewlist = new List<YCTestApercentModelView>();
            startTestNp1Button.IsEnabled = false;
            startTestNp1Button.BackgroundColor = Color.SlateGray;
            entry_yarnlen.IsEnabled = false;
            entry_testcount.IsEnabled = false;
            picker_machinecategory.IsEnabled = false;
            picker_machinename.IsEnabled = false;
            picker_shift.IsEnabled = false;
            picker_process.IsEnabled = false;
            entry_standardApercent.IsEnabled = false;
            CancellationTokenSource src = new CancellationTokenSource();
            CancellationToken ct = src.Token;
            ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
            await Task.Run(async () => await HandleTest(testCount), ct);
            src.Cancel();
        }

        private void picker_machinecategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedMachineCategory = picker_machinecategory.SelectedItem.ToString();
                if (selectedMachineCategory == "" || selectedMachineCategory == null)
                {
                    picker_machinename.ItemsSource = null;
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
                populateTestParams("", Guid.Empty, "");
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
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
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

        private async void btn_comment_Clicked(object sender, EventArgs e)
        {
            try
            {
                string header = "Note [Test ID : " + lbl_TestID.Text + "]";
                long testID = long.Parse(lbl_TestID.Text.ToString());
                string comment = await DisplayPromptAsync(header, "Please type your remark", "Save", "Discard", null, 100);
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    YCTestApercentCalculatedModel apercentCalculatedModel = conn.Table<YCTestApercentCalculatedModel>().Where(
                        YCTestApercentCalculatedModel => YCTestApercentCalculatedModel.testID == testID).FirstOrDefault();
                    if (apercentCalculatedModel != null)
                    {
                        apercentCalculatedModel.testRemark = comment;
                        int row = conn.Update(apercentCalculatedModel);
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
    }
}