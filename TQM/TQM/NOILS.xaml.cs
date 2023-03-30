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
    public partial class NOILS : ContentPage
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
        private List<NoilsTestModelView> noilsTestModelViewList;
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
        private decimal STD_NOILS = 0.0000m;
        private int currentTestCount = 0;
        private bool isTestStarted = false;
        private NoilsTestCalculatedModel noilsCalcList_finalOut = null;
        private RunConfiguration runConfiguration = new RunConfiguration();

        public NOILS()
        {
            InitializeComponent();
            lbl_TestID.Text = "";
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                //conn.DropTable<NoilsTestModel>();
                //conn.DropTable<NoilsTestSummaryModel>();
                //conn.DropTable<NoilsTestCalculatedModel>();
                //conn.DropTable<NoilsTestFinalModel>();

                //conn.CreateTable<YarnCountConfigModel>();
                //YarnCountConfigModel yarncountconfigmodel = conn.Table<YarnCountConfigModel>().FirstOrDefault();
                //if (yarncountconfigmodel != null)
                //{
                //    lbl_countsysname.Text = yarncountconfigmodel.countsysname;
                //    lbl_yarncountunit.Text = yarncountconfigmodel.yarnlenunit;
                //    entry_yarnlen.Text = "";
                //    entry_testcount.Text = yarncountconfigmodel.testcountNoils.ToString();
                //    TESTCOUNT = yarncountconfigmodel.testcountNoils;
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

                conn.CreateTable<NoilsTestModel>();
                conn.CreateTable<NoilsTestSummaryModel>();
                conn.CreateTable<NoilsTestFinalModel>();
                conn.CreateTable<NoilsTestCalculatedModel>();

                int recordCount = conn.Table<NoilsTestModel>().Count();

                if (recordCount == 0)
                {
                    startSliverButton.IsVisible = true;
                }
                else
                {
                    DateTime maxDate = conn.Table<NoilsTestModel>().Max(NoilsTestModel => NoilsTestModel.createdate);
                    NoilsTestModel lastTest = conn.Table<NoilsTestModel>()
                        .Where(NoilsTestModel => NoilsTestModel.createdate == maxDate).FirstOrDefault();

                    if (lastTest == null)
                    {
                        //to be decided
                    }
                    else
                    {
                        long lastTestID = lastTest.testID;
                        if (lastTest.testType == "Sliver")
                        {
                            // check Sliver is having entry in NoilsTestSummaryModel table
                            NoilsTestSummaryModel lastTestSummary = conn.Table<NoilsTestSummaryModel>().
                                            Where(NoilsTestSummaryModel =>
                                            (NoilsTestSummaryModel.testID == lastTest.testID
                                            && NoilsTestSummaryModel.testType == "Sliver")).FirstOrDefault();
                            if (lastTestSummary == null)
                            {
                                //get all test for Sliver from NoilsTestModel table and delete 

                                List<NoilsTestModel> allTest_Sliver = conn.Table<NoilsTestModel>()
                                                                .Where(NoilsTestModel =>
                                                                (NoilsTestModel.testID == lastTestID
                                                                && NoilsTestModel.testType == "Sliver")).ToList();
                                foreach (NoilsTestModel test in allTest_Sliver)
                                {
                                    conn.Delete(test);
                                }
                                startSliverButton.IsVisible = true;
                            }
                            else
                            {
                                //Start with Noils Test
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
                                entry_standardNoils.Text = formatDecimal(lastTest.standardNoils).ToString();
                                picker_machinecategory.IsEnabled = false;
                                picker_machinename.IsEnabled = false;
                                picker_shift.IsEnabled = false;
                                picker_process.IsEnabled = false;
                                entry_standardNoils.IsEnabled = false;
                                currentTestID = lastTest.testID;
                                entry_testcount.Text = lastTest.totaltestcount.ToString();
                                entry_testcount.IsEnabled = false;
                                entry_yarnlen.IsEnabled = false;
                                lbl_TestID.Text = currentTestID.ToString();
                                startNoilsButton.IsVisible = true;
                            }
                        }
                        else if (lastTest.testType == "Noils")
                        {
                            // check Noils is having entry in NoilsTestSummaryModel table
                            NoilsTestSummaryModel lastTestSummary = conn.Table<NoilsTestSummaryModel>().
                                            Where(NoilsTestSummaryModel =>
                                            (NoilsTestSummaryModel.testID == lastTest.testID
                                            && NoilsTestSummaryModel.testType == "Noils")).FirstOrDefault();
                            if (lastTestSummary == null)
                            {
                                //get all test for Noils from NoilsTestModel table and delete 

                                List<NoilsTestModel> allTest_Noils = conn.Table<NoilsTestModel>()
                                                                .Where(NoilsTestModel =>
                                                                (NoilsTestModel.testID == lastTestID
                                                                 && NoilsTestModel.testType == "Noils")).ToList();
                                foreach (NoilsTestModel test in allTest_Noils)
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
                                entry_standardNoils.Text = formatDecimal(lastTest.standardNoils).ToString();
                                picker_machinecategory.IsEnabled = false;
                                picker_machinename.IsEnabled = false;
                                picker_shift.IsEnabled = false;
                                picker_process.IsEnabled = false;
                                entry_standardNoils.IsEnabled = false;
                                currentTestID = lastTest.testID;
                                entry_testcount.Text = lastTest.totaltestcount.ToString();
                                entry_testcount.IsEnabled = false;
                                entry_yarnlen.IsEnabled = false;
                                lbl_TestID.Text = currentTestID.ToString();
                                startNoilsButton.IsVisible = true;
                            }
                            else
                            {
                                //Check Sliver and Noils data stored in NoilsTestFinalModel & NoilsTestCalculatedModel table

                                List<NoilsTestFinalModel> NoilsTestFinal = conn.Table<NoilsTestFinalModel>().
                                               Where(NoilsTestFinalModel =>
                                               NoilsTestFinalModel.testID == lastTest.testID).ToList();

                                bool deleteLastTest = false;

                                if (NoilsTestFinal != null)
                                {
                                    NoilsTestCalculatedModel calculatedNoilsTest = conn.Table<NoilsTestCalculatedModel>().
                                                Where(NoilsTestCalculatedModel =>
                                                NoilsTestCalculatedModel.testID == lastTest.testID).FirstOrDefault();
                                    if (calculatedNoilsTest != null)
                                    {
                                        startSliverButton.IsVisible = true;
                                    }
                                    else
                                    {
                                        deleteLastTest = true;

                                        // delete records in NoilsTestFinalModel Table

                                        List<NoilsTestFinalModel> allLastTest_Final = conn.Table<NoilsTestFinalModel>().
                                            Where(NoilsTestFinalModel =>
                                            (NoilsTestFinalModel.testID == lastTestID)).ToList();
                                        foreach (NoilsTestFinalModel test in allLastTest_Final)
                                        {
                                            conn.Delete(test);
                                        }
                                    }
                                }
                                else
                                {
                                    deleteLastTest = true;
                                }



                                //delete records in NoilsTestModel Table
                                if (deleteLastTest)
                                {
                                    List<NoilsTestModel> allLastTest = conn.Table<NoilsTestModel>().
                                        Where(NoilsTestModel =>
                                        (NoilsTestModel.testID == lastTestID)).ToList();
                                    foreach (NoilsTestModel test in allLastTest)
                                    {
                                        conn.Delete(test);
                                    }

                                    // delete records in NoilsTestSummaryModel Table

                                    List<NoilsTestSummaryModel> allLastTest_Summary = conn.Table<NoilsTestSummaryModel>().
                                        Where(NoilsTestSummaryModel =>
                                        (NoilsTestSummaryModel.testID == lastTestID)).ToList();
                                    foreach (NoilsTestSummaryModel test in allLastTest_Summary)
                                    {
                                        conn.Delete(test);
                                    }



                                    //start new test

                                    startSliverButton.IsVisible = true;
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
                entry_standardNoils.Text = "0.0000";
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
                    entry_standardNoils.Text = formatDecimal(yarncountconfigmodel.standardNoils).ToString();
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
                    entry_standardNoils.Text = "0.0000";
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
                if (currentTestType == "Sliver")
                {
                    startSliverButton.IsVisible = false;
                    startNoilsButton.IsVisible = true;
                    startNoilsButton.IsEnabled = true;
                    startNoilsButton.BackgroundColor = Color.Green;
                }
                else
                {
                    startNoilsButton.IsVisible = false;
                    startSliverButton.IsVisible = true;
                    startSliverButton.IsEnabled = true;
                    startSliverButton.BackgroundColor = Color.Green;
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
                if (currentTestType == "Noils" && showFinalOut)
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
                    if (noilsTestModelViewList != null)
                    {
                        listview_testresult_individual.ItemsSource = null;
                        listview_testresult_individual.ItemsSource = noilsTestModelViewList.OrderByDescending(NoilsTestModelView => NoilsTestModelView.testcount);
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
                else if (currentTestType == "Noils" && showFinalOut)
                {
                    frame_overallTestSummary.IsVisible = visibility;
                    lbl_noilsPercent.Text = formatDecimal(noilsCalcList_finalOut.average_wt_noils).ToString();
                }
            });
        }

        private List<NoilsReportModelView> generateResultView()
        {
            try
            {
                List<NoilsReportModelView> OVS = new List<NoilsReportModelView>();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {

                    conn.CreateTable<NoilsTestModel>();
                    conn.CreateTable<NoilsTestCalculatedModel>();

                    if (noilsCalcList_finalOut == null)
                    {
                        return null;
                    }

                    List<NoilsTestFinalModel> list_finalNoils = conn.Table<NoilsTestFinalModel>().Where(
                        NoilsTestFinalModel =>
                        (NoilsTestFinalModel.testID == currentTestID && NoilsTestFinalModel.status == true)).ToList();

                    if (list_finalNoils != null)
                    {

                        int loopCount = 0;
                        foreach (NoilsTestFinalModel test in list_finalNoils)
                        {
                            NoilsReportModelView noilsReportMV = new NoilsReportModelView()
                            {
                                testID = test.testID,
                                description = test.testcount.ToString(),
                                silver_wt = formatDecimal(test.weigth_sliver),
                                noils_wt = formatDecimal(test.weigth_noils),
                                noils = formatDecimal(test.noils)
                            };
                            OVS.Add(noilsReportMV);
                            loopCount += 1;
                        }

                        NoilsReportModelView noilsReportModelView = new NoilsReportModelView()
                        {
                            testID = noilsCalcList_finalOut.testID,
                            description = "Average Weight",
                            silver_wt = formatDecimal(noilsCalcList_finalOut.average_wt_sliverwt),
                            noils_wt = formatDecimal(noilsCalcList_finalOut.average_wt_noilswt),
                            noils = formatDecimal(noilsCalcList_finalOut.average_wt_noils)
                        };
                        OVS.Add(noilsReportModelView);

                        noilsReportModelView = new NoilsReportModelView()
                        {
                            testID = noilsCalcList_finalOut.testID,
                            description = "Weight (Max)",
                            silver_wt = formatDecimal(noilsCalcList_finalOut.max_sliverwt),
                            noils_wt = formatDecimal(noilsCalcList_finalOut.max_noilswt),
                            noils = formatDecimal(noilsCalcList_finalOut.max_noils)
                        };
                        OVS.Add(noilsReportModelView);

                        noilsReportModelView = new NoilsReportModelView()
                        {
                            testID = noilsCalcList_finalOut.testID,
                            description = "Weight (Min)",
                            silver_wt = formatDecimal(noilsCalcList_finalOut.min_sliverwt),
                            noils_wt = formatDecimal(noilsCalcList_finalOut.min_noilswt),
                            noils = formatDecimal(noilsCalcList_finalOut.min_noils)
                        };
                        OVS.Add(noilsReportModelView);

                        noilsReportModelView = new NoilsReportModelView()
                        {
                            testID = noilsCalcList_finalOut.testID,
                            description = "Range",
                            silver_wt = formatDecimal(noilsCalcList_finalOut.range_sliverwt),
                            noils_wt = formatDecimal(noilsCalcList_finalOut.range_noilswt),
                            noils = formatDecimal(noilsCalcList_finalOut.range_noils)
                        };
                        OVS.Add(noilsReportModelView);

                        //noilsReportModelView = new NoilsReportModelView()
                        //{
                        //    testID = noilsCalcList_finalOut.testID,
                        //    description = "HANK",
                        //    silver_wt = noilsCalcList_finalOut.testaverage_sliverwt,
                        //    noils_wt = noilsCalcList_finalOut.testaverage_noilswt,
                        //    noils = 0.00m
                        //};
                        //OVS.Add(noilsReportModelView);

                        noilsReportModelView = new NoilsReportModelView()
                        {
                            testID = noilsCalcList_finalOut.testID,
                            description = "SD",
                            silver_wt = formatDecimal(noilsCalcList_finalOut.testsd_sliverwt),
                            noils_wt = formatDecimal(noilsCalcList_finalOut.testsd_noilswt),
                            noils = formatDecimal(noilsCalcList_finalOut.testsd_noils)
                        };
                        OVS.Add(noilsReportModelView);

                        noilsReportModelView = new NoilsReportModelView()
                        {
                            testID = noilsCalcList_finalOut.testID,
                            description = "CV",
                            silver_wt = formatDecimal(noilsCalcList_finalOut.testcv_sliverwt),
                            noils_wt = formatDecimal(noilsCalcList_finalOut.testcv_noilswt),
                            noils = formatDecimal(noilsCalcList_finalOut.testcv_noils)
                        };
                        OVS.Add(noilsReportModelView);
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

        private async void updateDB()
        {
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                bool dbStatus = true;
                decimal totalCalcCountVal = 0m;
                decimal totalWeight = 0m;
                conn.CreateTable<NoilsTestModel>();
                List<NoilsTestModel> noilsTestModelList = conn.Table<NoilsTestModel>().Where(
                                NoilsTestModel => (NoilsTestModel.status == true &&
                                NoilsTestModel.testID != currentTestID)).ToList();

                foreach (NoilsTestModel noilsTestModel in noilsTestModelList)
                {
                    noilsTestModel.status = false;
                    noilsTestModel.dataSyncStatus = false;
                    if (conn.Update(noilsTestModel) < 1)
                    {
                        dbStatus = false;
                    }
                }


                if (dbStatus)
                {
                    foreach (NoilsTestModelView test in noilsTestModelViewList)
                    {
                        NoilsTestModel noilsTestModel = new NoilsTestModel()
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
                            standardNoils = test.standardNoils,
                            status = true,
                            createdate = DateTime.Now
                        };
                        int row = conn.Insert(noilsTestModel);
                        if (row < 1)
                        {
                            dbStatus = false;
                        }
                        totalCalcCountVal = totalCalcCountVal + test.yccalcval;
                        totalCalcCountVal = formatDecimal(totalCalcCountVal);
                        totalWeight = totalWeight + test.yarnweight;
                        totalWeight = formatDecimal(totalWeight);
                    }
                }
                if (dbStatus)
                {
                    conn.CreateTable<NoilsTestSummaryModel>();
                    List<NoilsTestSummaryModel> noilsTestSMList = conn.Table<NoilsTestSummaryModel>().Where(
                                    NoilsTestSummaryModel => (NoilsTestSummaryModel.status == true &&
                                    NoilsTestSummaryModel.testID != currentTestID)).ToList();

                    foreach (NoilsTestSummaryModel noilsTestSM in noilsTestSMList)
                    {
                        noilsTestSM.status = false;
                        noilsTestSM.dataSyncStatus = false;
                        if (conn.Update(noilsTestSM) < 1)
                        {
                            dbStatus = false;
                        }
                    }

                    decimal avg_weight = 0m;
                    decimal mean = 0m;
                    decimal sd = 0m;
                    decimal cv = 0m;
                    if (noilsTestModelViewList[0].totaltestcount > 1)
                    {
                        avg_weight = totalWeight / noilsTestModelViewList[0].totaltestcount;
                        avg_weight = formatDecimal(avg_weight);
                        mean = totalCalcCountVal / noilsTestModelViewList[0].totaltestcount;
                        mean = formatDecimal(mean);
                        decimal IndividualCalValminusMean = 0m;
                        foreach (NoilsTestModelView test in noilsTestModelViewList)
                        {
                            IndividualCalValminusMean = IndividualCalValminusMean + ((test.yarnweight - avg_weight) * (test.yarnweight - avg_weight));
                        }
                        sd = (decimal)Math.Sqrt((double)IndividualCalValminusMean / (double)(noilsTestModelViewList[0].totaltestcount - 1));//Standard Deviation
                        sd = formatDecimal(sd);
                        cv = (sd / avg_weight) * 100m; //Coefficient of Variation
                        cv = formatDecimal(cv);
                    }
                    NoilsTestSummaryModel noilsTestSummaryModel = new NoilsTestSummaryModel()
                    {
                        ID = Guid.NewGuid(),
                        testID = noilsTestModelViewList[0].testID,
                        userID = noilsTestModelViewList[0].userID,
                        userName = noilsTestModelViewList[0].userName,
                        machineID = noilsTestModelViewList[0].machineID,
                        machineCategory = noilsTestModelViewList[0].machineCategory,
                        machineName = noilsTestModelViewList[0].machineName,
                        process = noilsTestModelViewList[0].process,
                        countsysname = noilsTestModelViewList[0].countsysname,
                        yarnlenunit = noilsTestModelViewList[0].yarnlenunit,
                        yarnlength = noilsTestModelViewList[0].yarnlength,
                        shift = noilsTestModelViewList[0].shift,
                        testType = noilsTestModelViewList[0].testType,
                        totaltestcount = noilsTestModelViewList[0].totaltestcount,
                        standardNoils = noilsTestModelViewList[0].standardNoils,
                        avg_weight = avg_weight,
                        testaverage = mean,
                        testsd = sd,
                        testcv = cv,
                        status = true,
                        createdate = DateTime.Now
                    };
                    conn.CreateTable<NoilsTestSummaryModel>();
                    int row = conn.Insert(noilsTestSummaryModel);
                    if (row < 1)
                    {
                        dbStatus = false;
                    }
                    if (dbStatus)
                    {
                        //await enableTestButton();
                        //await refListView();
                        //await refOverallSummary(mean, sd, cv);
                        if (currentTestType == "Noils")
                        {
                            conn.CreateTable<NoilsTestCalculatedModel>();
                            List<NoilsTestCalculatedModel> noilsCalcList = conn.Table<NoilsTestCalculatedModel>().Where(
                               NoilsTestCalculatedModel =>
                               (NoilsTestCalculatedModel.status == true &&
                               NoilsTestCalculatedModel.testID != currentTestID)).ToList();

                            foreach (NoilsTestCalculatedModel noilsCalc in noilsCalcList)
                            {
                                noilsCalc.status = false;
                                noilsCalc.dataSyncStatus = false;
                                if (conn.Update(noilsCalc) < 1)
                                {
                                    //to be decided if noils calculated active records failed to deactive
                                }
                            }
                            NoilsTestSummaryModel sliver_Summary = conn.Table<NoilsTestSummaryModel>().Where(
                                                            NoilsTestSummaryModel => (
                                                            NoilsTestSummaryModel.testType == "Sliver" &&
                                                            NoilsTestSummaryModel.status == true &&
                                                            NoilsTestSummaryModel.testID == currentTestID)
                                                            ).FirstOrDefault();
                            if (sliver_Summary != null)
                            {
                                NoilsTestSummaryModel noils_Summary = conn.Table<NoilsTestSummaryModel>().Where(
                                                            NoilsTestSummaryModel => (
                                                            NoilsTestSummaryModel.testType == "Noils" &&
                                                            NoilsTestSummaryModel.status == true &&
                                                            NoilsTestSummaryModel.testID == currentTestID)
                                                            ).FirstOrDefault();
                                if (noils_Summary != null)
                                {
                                    List<NoilsTestModel> noilsTest_sliverList = conn.Table<NoilsTestModel>().Where(
                                                            NoilsTestModel => (
                                                            NoilsTestModel.testType == "Sliver" &&
                                                            NoilsTestModel.status == true &&
                                                            NoilsTestModel.testID == currentTestID)
                                                            ).ToList();

                                    List<NoilsTestModel> noilsTest_noilsList = conn.Table<NoilsTestModel>().Where(
                                                           NoilsTestModel => (
                                                           NoilsTestModel.testType == "Noils" &&
                                                           NoilsTestModel.status == true &&
                                                           NoilsTestModel.testID == currentTestID)
                                                           ).ToList();

                                    if (noilsTest_sliverList.Count == 0 || noilsTest_noilsList.Count == 0)
                                    {

                                        // to be decided if sliver and noils test are blank
                                    }
                                    else
                                    {
                                        int testRecCount = 0;
                                        decimal totalWeight_Noils = 0.00m;
                                        foreach (NoilsTestModel noilsTest_sliver in noilsTest_sliverList)
                                        {
                                            decimal noils = (noilsTest_noilsList[testRecCount].yarnweight / (noilsTest_noilsList[testRecCount].yarnweight + noilsTest_sliver.yarnweight)) * 100m;
                                            noils = formatDecimal(noils);
                                            totalWeight_Noils = formatDecimal(totalWeight_Noils + noils);
                                            NoilsTestFinalModel noilsTestFinalModel = new NoilsTestFinalModel()
                                            {
                                                ID = Guid.NewGuid(),
                                                testID = noilsTest_sliver.testID,
                                                testcount = noilsTest_sliver.testcount,
                                                weigth_sliver = formatDecimal(noilsTest_sliver.yarnweight),
                                                weigth_noils = formatDecimal(noilsTest_noilsList[testRecCount].yarnweight),
                                                noils = noils,
                                                status = true,
                                                createdate = DateTime.Now
                                            };
                                            conn.CreateTable<NoilsTestFinalModel>();
                                            int row_final = conn.Insert(noilsTestFinalModel);
                                            if (row_final < 1)
                                            {
                                                // to be decided if final rec failed to insert
                                            }
                                            testRecCount += 1;
                                        }

                                        decimal avg_weight_noils = totalWeight_Noils / noilsTest_sliverList[0].totaltestcount;
                                        avg_weight_noils = formatDecimal(avg_weight_noils);



                                        NoilsTestFinalModel Max_noils = conn.Table<NoilsTestFinalModel>().Where(
                                            NoilsTestFinalModel =>
                                            (NoilsTestFinalModel.testID == currentTestID &&
                                            NoilsTestFinalModel.status == true)).OrderByDescending(NoilsTestFinalModel => NoilsTestFinalModel.noils).First();
                                        NoilsTestFinalModel Min_noils = conn.Table<NoilsTestFinalModel>().Where(
                                            NoilsTestFinalModel =>
                                            (NoilsTestFinalModel.testID == currentTestID &&
                                            NoilsTestFinalModel.status == true)).OrderBy(NoilsTestFinalModel => NoilsTestFinalModel.noils).First();

                                        NoilsTestFinalModel Max_sliver = conn.Table<NoilsTestFinalModel>().Where(
                                             NoilsTestFinalModel =>
                                             (NoilsTestFinalModel.testID == currentTestID &&
                                             NoilsTestFinalModel.status == true)).OrderByDescending(NoilsTestFinalModel => NoilsTestFinalModel.weigth_sliver).First();
                                        NoilsTestFinalModel Min_sliver = conn.Table<NoilsTestFinalModel>().Where(
                                            NoilsTestFinalModel =>
                                            (NoilsTestFinalModel.testID == currentTestID &&
                                            NoilsTestFinalModel.status == true)).OrderBy(NoilsTestFinalModel => NoilsTestFinalModel.weigth_sliver).First();

                                        NoilsTestFinalModel Max_noilswt = conn.Table<NoilsTestFinalModel>().Where(
                                            NoilsTestFinalModel =>
                                            (NoilsTestFinalModel.testID == currentTestID &&
                                            NoilsTestFinalModel.status == true)).OrderByDescending(NoilsTestFinalModel => NoilsTestFinalModel.weigth_noils).First();
                                        NoilsTestFinalModel Min_noilswt = conn.Table<NoilsTestFinalModel>().Where(
                                            NoilsTestFinalModel =>
                                            (NoilsTestFinalModel.testID == currentTestID &&
                                            NoilsTestFinalModel.status == true)).OrderBy(NoilsTestFinalModel => NoilsTestFinalModel.weigth_noils).First();

                                        decimal range_sliver = formatDecimal(Max_sliver.weigth_sliver - Min_sliver.weigth_sliver);
                                        decimal range_noilswt = formatDecimal(Max_noilswt.weigth_noils - Min_noilswt.weigth_noils);
                                        decimal range_noils = formatDecimal(Max_noils.noils - Min_noils.noils);

                                        List<NoilsTestFinalModel> noilsFinal_list = conn.Table<NoilsTestFinalModel>().Where(
                                                          NoilsTestFinalModel => (
                                                          NoilsTestFinalModel.status == true &&
                                                          NoilsTestFinalModel.testID == currentTestID)
                                                          ).ToList();

                                        decimal IndividualCalValminusMean = 0m;
                                        foreach (NoilsTestFinalModel test in noilsFinal_list)
                                        {
                                            IndividualCalValminusMean = IndividualCalValminusMean + ((test.noils - avg_weight_noils) * (test.noils - avg_weight_noils));
                                        }
                                        decimal sd_noils = (decimal)Math.Sqrt((double)IndividualCalValminusMean / (double)(noilsTest_sliverList[0].totaltestcount - 1));//Standard Deviation
                                        decimal cv_noils = (sd_noils / avg_weight_noils) * 100m; //Coefficient of Variation
                                        sd_noils = formatDecimal(sd_noils);
                                        cv_noils = formatDecimal(cv_noils);

                                        NoilsTestCalculatedModel noilsTestCalculatedModel = new NoilsTestCalculatedModel()
                                        {
                                            ID = Guid.NewGuid(),
                                            testID = sliver_Summary.testID,
                                            userID = sliver_Summary.userID,
                                            userName = sliver_Summary.userName,
                                            machineID = sliver_Summary.machineID,
                                            machineCategory = sliver_Summary.machineCategory,
                                            machineName = sliver_Summary.machineName,
                                            process = sliver_Summary.process,
                                            countsysname = sliver_Summary.countsysname,
                                            yarnlenunit = sliver_Summary.yarnlenunit,
                                            yarnlength = sliver_Summary.yarnlength,
                                            shift = sliver_Summary.shift,
                                            totaltestcount = sliver_Summary.totaltestcount,
                                            standardNoils = sliver_Summary.standardNoils,
                                            average_wt_sliverwt = sliver_Summary.avg_weight,
                                            max_sliverwt = Max_sliver.weigth_sliver,
                                            min_sliverwt = Min_sliver.weigth_sliver,
                                            range_sliverwt = range_sliver,
                                            testaverage_sliverwt = sliver_Summary.testaverage,
                                            testsd_sliverwt = sliver_Summary.testsd,
                                            testcv_sliverwt = sliver_Summary.testcv,
                                            average_wt_noilswt = noils_Summary.avg_weight,
                                            max_noilswt = Max_noilswt.weigth_noils,
                                            min_noilswt = Min_noilswt.weigth_noils,
                                            range_noilswt = range_noilswt,
                                            testaverage_noilswt = noils_Summary.testaverage,
                                            testsd_noilswt = noils_Summary.testsd,
                                            testcv_noilswt = noils_Summary.testcv,
                                            average_wt_noils = avg_weight_noils,
                                            max_noils = Max_noils.noils,
                                            min_noils = Min_noils.noils,
                                            range_noils = range_noils,
                                            testsd_noils = sd_noils,
                                            testcv_noils = cv_noils,
                                            status = true,
                                            createdate = DateTime.Now
                                        };
                                        int row_TestCalc = conn.Insert(noilsTestCalculatedModel);
                                        if (row_TestCalc < 1)
                                        {
                                            // To be decieded if noils test calculated value failed to insert to db
                                        }
                                        else
                                        {
                                            noilsCalcList_finalOut = noilsTestCalculatedModel;
                                        }
                                    }
                                }
                                else
                                {
                                    // To be decieded if FB Summary active record is not available in db
                                }
                            }
                            else
                            {
                                // To be decieded if IB Summary active record is not available in db
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
                    if (currentTestType == "Sliver")
                    {
                        startNoilsButton.IsVisible = false;
                        startSliverButton.IsVisible = true;
                        startSliverButton.IsEnabled = true;
                        startSliverButton.BackgroundColor = Color.Green;
                    }
                    else
                    {
                        startSliverButton.IsVisible = false;
                        startNoilsButton.IsVisible = true;
                        startNoilsButton.IsEnabled = true;
                        startNoilsButton.BackgroundColor = Color.Green;
                    }
                }
                if (dispose) { disposeble(); }
                Device.BeginInvokeOnMainThread(() =>
                {
                    string str_testType = "";
                    if (currentTestType == "Sliver")
                    {
                        str_testType = "Test Completed for Sliver!!! Start test for Noils";
                    }
                    else
                    {
                        str_testType = "All Test Completed!!!";
                    }
                    if (isTestStarted)
                    {
                        isTestStarted = false;
                        if (noilsTestModelViewList != null)
                        {
                            if (selectedTestCount != noilsTestModelViewList.Count())
                            {
                                ImageNotification("red.png");
                                UpdateUserNotification("IMPROPER TEST!!!");
                                showAlert("Improper Test!!!");
                            }
                            else
                            {
                                showAlert(str_testType);
                                if (currentTestType == "Noils")
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
                                    entry_standardNoils.Text = "0.0000";
                                    entry_standardNoils.IsEnabled = true;
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
        private async void startSliverButton_Clicked(object sender, EventArgs e)
        {
            lbl_TestID.Text = "";
            isTestStarted = true;
            currentTestType = "Sliver";
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
            if (entry_standardNoils.Text.Trim() == "-")
            {
                await DisplayAlert("Attention", "Standard Noils is invalid. Please check!!!", "Ok");
                return;
            }
            if (entry_standardNoils.Text.Trim() == "" || decimal.Parse(entry_standardNoils.Text.Trim()) <= 0m)
            {
                await DisplayAlert("Attention", "Standard Noils should not be blank or zero or negative!!!", "Ok");
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
                List<NoilsTestModel> allRecords = conn.Table<NoilsTestModel>().ToList();
                NoilsTestModel lastTestRecord = null;
                conn.CreateTable<NoilsTestModel>();
                int recordCount = conn.Table<NoilsTestModel>().Count();

                if (recordCount == 0)
                {
                    currentTestID = 1;
                }
                else
                {
                    DateTime maxDate = conn.Table<NoilsTestModel>().Max(NoilsTestModel => NoilsTestModel.createdate);
                    lastTestRecord = conn.Table<NoilsTestModel>()
                        .Where(NoilsTestModel => NoilsTestModel.createdate == maxDate).FirstOrDefault();
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
            STD_NOILS = formatDecimal(decimal.Parse(entry_standardNoils.Text));
            noilsTestModelViewList = new List<NoilsTestModelView>();
            startSliverButton.IsEnabled = false;
            startSliverButton.BackgroundColor = Color.SlateGray;
            entry_yarnlen.IsEnabled = false;
            entry_testcount.IsEnabled = false;
            picker_machinecategory.IsEnabled = false;
            picker_machinename.IsEnabled = false;
            picker_shift.IsEnabled = false;
            picker_process.IsEnabled = false;
            entry_standardNoils.IsEnabled = false;
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
                        NoilsTestModelView noilsTestModelView = new NoilsTestModelView()
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
                            standardNoils = STD_NOILS
                        };
                        noilsTestModelViewList.Add(noilsTestModelView);
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

                if (noilsTestModelViewList.Count > 0 && passCount == testCount)
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

        private async void startNoilsButton_Clicked(object sender, EventArgs e)
        {
            isTestStarted = true;
            currentTestType = "Noils";
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
            if (entry_standardNoils.Text.Trim() == "-")
            {
                await DisplayAlert("Attention", "Standard Noils is invalid. Please check!!!", "Ok");
                return;
            }
            if (entry_standardNoils.Text.Trim() == "" || decimal.Parse(entry_standardNoils.Text.Trim()) <= 0m)
            {
                await DisplayAlert("Attention", "Standard Noils should not be blank or zero or negative!!!", "Ok");
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
                List<NoilsTestModel> allRecords = conn.Table<NoilsTestModel>().ToList();
                conn.CreateTable<NoilsTestModel>();
                DateTime maxDate = conn.Table<NoilsTestModel>().Max(NoilsTestModel => NoilsTestModel.createdate);
                NoilsTestModel lastTestRecord = conn.Table<NoilsTestModel>()
                    .Where(NoilsTestModel => NoilsTestModel.createdate == maxDate).FirstOrDefault();
                //NoilsTestModel lastTestRecord = conn.Table<NoilsTestModel>().OrderByDescending(NoilsTestModel => NoilsTestModel.testID).FirstOrDefault();
                if (lastTestRecord != null)
                {
                    allRecords = conn.Table<NoilsTestModel>().ToList();
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
            STD_NOILS = formatDecimal(decimal.Parse(entry_standardNoils.Text));
            noilsTestModelViewList = new List<NoilsTestModelView>();
            startNoilsButton.IsEnabled = false;
            startNoilsButton.BackgroundColor = Color.SlateGray;
            entry_yarnlen.IsEnabled = false;
            entry_testcount.IsEnabled = false;
            picker_machinecategory.IsEnabled = false;
            picker_machinename.IsEnabled = false;
            picker_shift.IsEnabled = false;
            picker_process.IsEnabled = false;
            entry_standardNoils.IsEnabled = false;
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
                    NoilsTestCalculatedModel noilsCalculatedModel = conn.Table<NoilsTestCalculatedModel>().Where(
                        NoilsTestCalculatedModel => NoilsTestCalculatedModel.testID == testID).FirstOrDefault();
                    if (noilsCalculatedModel != null)
                    {
                        noilsCalculatedModel.testRemark = comment;
                        int row = conn.Update(noilsCalculatedModel);
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