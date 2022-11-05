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
    public partial class StretchPage : ContentPage
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
        private List<StretchTestModelView> stretchTestModelViewList;
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
        private int currentTestCount = 0;
        private bool isTestStarted = false;
        private StretchTestCalculatedModel stretchCalcList_finalOut = null;

        public StretchPage()
        {
            InitializeComponent();
            lbl_TestID.Text = "";
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                //conn.DropTable<StretchTestModel>();
                //conn.DropTable<StretchTestSummaryModel>();
                //conn.DropTable<StretchTestCalculatedModel>();

                conn.CreateTable<YarnCountConfigModel>();
                YarnCountConfigModel yarncountconfigmodel = conn.Table<YarnCountConfigModel>().FirstOrDefault();
                if (yarncountconfigmodel != null)
                {
                    lbl_countsysname.Text = yarncountconfigmodel.countsysname;
                    lbl_yarncountunit.Text = yarncountconfigmodel.yarnlenunit;
                    entry_yarnlen.Text = "";
                    entry_testcount.Text = yarncountconfigmodel.testcountStretch.ToString();
                    TESTCOUNT = yarncountconfigmodel.testcountStretch;
                }
                else
                {
                    lbl_countsysname.Text = "";
                    lbl_yarncountunit.Text = "";
                    entry_yarnlen.Text = "";
                    entry_testcount.Text = "";
                    picker_shift.SelectedIndex = 0;
                    picker_process.SelectedIndex = 0;
                }
                conn.CreateTable<StretchTestModel>();
                conn.CreateTable<StretchTestSummaryModel>();
                conn.CreateTable<StretchTestCalculatedModel>();

                int recordCount = conn.Table<StretchTestModel>().Count();

                if (recordCount == 0)
                {
                    startInitialBobbinButton.IsVisible = true;
                }
                else
                {
                    DateTime maxDate = conn.Table<StretchTestModel>().Max(StretchTestModel => StretchTestModel.createdate);
                    StretchTestModel lastTest = conn.Table<StretchTestModel>().Where(StretchTestModel => StretchTestModel.createdate == maxDate).FirstOrDefault();
                    //StretchTestModel lastTest = conn.Table<StretchTestModel>().
                    //    Where(StretchTestModel => StretchTestModel.ID == maxTest.ID).FirstOrDefault();

                    if (lastTest.testType == "IB")
                    {
                        StretchTestSummaryModel lastTestSummary = conn.Table<StretchTestSummaryModel>().
                                            Where(StretchTestSummaryModel =>
                                            (StretchTestSummaryModel.testID == lastTest.testID
                                            && StretchTestSummaryModel.testType == "IB")).FirstOrDefault();
                        if (lastTestSummary == null)
                        {
                            lastTest.status = false;
                            if (conn.Update(lastTest) < 1)
                            {
                                //to be decided
                            }
                            else
                            {
                                startInitialBobbinButton.IsVisible = true;
                            }
                        }
                        else
                        {
                            //resume with full bobbin test
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
                            picker_machinecategory.IsEnabled = false;
                            picker_machinename.IsEnabled = false;
                            picker_shift.IsEnabled = false;
                            picker_process.IsEnabled = false;
                            currentTestID = lastTest.testID;
                            lbl_TestID.Text = currentTestID.ToString();
                            startFullBobbinButton.IsVisible = true;
                        }
                    }
                    else // to check last full bobbin test was completed properly
                    {
                        long lastTestID = lastTest.testID;
                        StretchTestSummaryModel lastTestSummary = conn.Table<StretchTestSummaryModel>().
                                            Where(StretchTestSummaryModel =>
                                            (StretchTestSummaryModel.testID == lastTest.testID
                                            && StretchTestSummaryModel.testType == "FB")).FirstOrDefault();
                        if (lastTestSummary == null)
                        {
                            if (conn.Delete(lastTest) < 1)
                            {
                                //to be decided
                            }
                            else
                            {
                                //start new full bobbin test
                                StretchTestModel lastTest_IB = conn.Table<StretchTestModel>().
                                    Where(StretchTestModel =>
                                    (StretchTestModel.testID == lastTestID && StretchTestModel.testType == "IB")).FirstOrDefault();
                                picker_machinecategory.SelectedItem = lastTest_IB.machineCategory;
                                picker_machinename.SelectedItem = lastTest_IB.machineName;
                                picker_shift.SelectedItem = lastTest_IB.shift;
                                picker_process.SelectedItem = lastTest_IB.process;
                                picker_machinecategory.IsEnabled = false;
                                picker_machinename.IsEnabled = false;
                                picker_shift.IsEnabled = false;
                                picker_process.IsEnabled = false;
                                currentTestID = lastTest_IB.testID;
                                lbl_TestID.Text = currentTestID.ToString();
                                startFullBobbinButton.IsVisible = true;
                            }
                        }
                        else
                        {
                            //Check IB and FB data stored in StretchTestCalculatedModel table

                            StretchTestCalculatedModel calculatedStretchTest = conn.Table<StretchTestCalculatedModel>().
                                            Where(StretchTestCalculatedModel =>
                                            StretchTestCalculatedModel.testID == lastTest.testID).FirstOrDefault();
                            if (calculatedStretchTest != null)
                            {
                                startInitialBobbinButton.IsVisible = true;
                            }
                            else
                            {
                                StretchTestModel lastTest_IB = conn.Table<StretchTestModel>().
                                    Where(StretchTestModel =>
                                    (StretchTestModel.testID == lastTestID && StretchTestModel.testType == "IB")).FirstOrDefault();
                                if (conn.Delete(lastTest_IB) < 1)
                                {
                                    //to be decided
                                }
                                else
                                {
                                    StretchTestModel lastTest_FB = conn.Table<StretchTestModel>().
                                    Where(StretchTestModel =>
                                    (StretchTestModel.testID == lastTestID && StretchTestModel.testType == "FB")).FirstOrDefault();
                                    if (conn.Delete(lastTest_FB) < 1)
                                    {
                                        //to be decided
                                    }
                                    else
                                    {
                                        StretchTestSummaryModel lastTestSummary_IB = conn.Table<StretchTestSummaryModel>().
                                            Where(StretchTestSummaryModel =>
                                            (StretchTestSummaryModel.testID == lastTestID
                                            && StretchTestSummaryModel.testType == "IB")).FirstOrDefault();
                                        if (conn.Delete(lastTestSummary_IB) < 1)
                                        {
                                            //to be decided
                                        }
                                        else
                                        {
                                            StretchTestSummaryModel lastTestSummary_FB = conn.Table<StretchTestSummaryModel>().
                                            Where(StretchTestSummaryModel =>
                                            (StretchTestSummaryModel.testID == lastTestID
                                            && StretchTestSummaryModel.testType == "FB")).FirstOrDefault();
                                            if (conn.Delete(lastTestSummary_FB) < 1)
                                            {
                                                //to be decided
                                            }
                                            else
                                            {
                                                startInitialBobbinButton.IsVisible = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
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
                if (currentTestType == "IB")
                {
                    startInitialBobbinButton.IsVisible = false;
                    startFullBobbinButton.IsVisible = true;
                    startFullBobbinButton.IsEnabled = true;
                    startFullBobbinButton.BackgroundColor = Color.Green;
                }
                else
                {
                    startFullBobbinButton.IsVisible = false;
                    startInitialBobbinButton.IsVisible = true;
                    startInitialBobbinButton.IsEnabled = true;
                    startInitialBobbinButton.BackgroundColor = Color.Green;
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
                if (currentTestType == "FB" && showFinalOut)
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
                    if (stretchTestModelViewList != null)
                    {
                        listview_testresult_individual.ItemsSource = null;
                        listview_testresult_individual.ItemsSource = stretchTestModelViewList.OrderByDescending(StretchTestModelView => StretchTestModelView.testcount);
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
                else if (currentTestType == "FB" && showFinalOut)
                {
                    frame_overallTestSummary.IsVisible = visibility;
                    lbl_stretchPercent.Text = stretchCalcList_finalOut.stretch.ToString();
                }
            });
        }


        private List<StretchReportModelView> generateResultView()
        {
            try
            {
                List<StretchReportModelView> OVS = new List<StretchReportModelView>();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {

                    conn.CreateTable<StretchTestModel>();
                    conn.CreateTable<StretchTestCalculatedModel>();


                    if (stretchCalcList_finalOut == null)
                    {
                        return null;
                    }

                    List<StretchTestModel> yctestStretchlist_IB = conn.Table<StretchTestModel>().Where(
                        StretchTestModel =>
                        (StretchTestModel.testType == "IB"
                        && StretchTestModel.status == true
                        && StretchTestModel.testID == currentTestID)).ToList();

                    List<StretchTestModel> yctestStretchlist_FB = conn.Table<StretchTestModel>().Where(
                        StretchTestModel =>
                        (StretchTestModel.testType == "FB"
                        && StretchTestModel.status == true
                        && StretchTestModel.testID == currentTestID)).ToList();

                    if (yctestStretchlist_IB != null && yctestStretchlist_FB != null)
                    {

                        int loopCount = 0;
                        foreach (StretchTestModel test in yctestStretchlist_IB)
                        {
                            StretchReportModelView stretchReportMV = new StretchReportModelView()
                            {
                                testID = test.testID,
                                description = test.testcount.ToString(),
                                IB = yctestStretchlist_IB[loopCount].yarnweight,
                                FB = yctestStretchlist_FB[loopCount].yarnweight,
                            };
                            OVS.Add(stretchReportMV);
                            loopCount += 1;
                        }

                        StretchReportModelView StretchReportModelView = new StretchReportModelView()
                        {
                            testID = stretchCalcList_finalOut.testID,
                            description = "Average Weight",
                            IB = stretchCalcList_finalOut.avg_weight_IB,
                            FB = stretchCalcList_finalOut.avg_weight_FB,
                        };
                        OVS.Add(StretchReportModelView);

                        StretchReportModelView = new StretchReportModelView()
                        {
                            testID = stretchCalcList_finalOut.testID,
                            description = "Weight (Max)",
                            IB = stretchCalcList_finalOut.max_IB,
                            FB = stretchCalcList_finalOut.max_FB,
                        };
                        OVS.Add(StretchReportModelView);

                        StretchReportModelView = new StretchReportModelView()
                        {
                            testID = stretchCalcList_finalOut.testID,
                            description = "Weight (Min)",
                            IB = stretchCalcList_finalOut.min_IB,
                            FB = stretchCalcList_finalOut.min_FB,
                        };
                        OVS.Add(StretchReportModelView);

                        StretchReportModelView = new StretchReportModelView()
                        {
                            testID = stretchCalcList_finalOut.testID,
                            description = "Range",
                            IB = stretchCalcList_finalOut.range_IB,
                            FB = stretchCalcList_finalOut.range_FB,
                        };
                        OVS.Add(StretchReportModelView);

                        StretchReportModelView = new StretchReportModelView()
                        {
                            testID = stretchCalcList_finalOut.testID,
                            description = "HANK",
                            IB = stretchCalcList_finalOut.testaverage_IB,
                            FB = stretchCalcList_finalOut.testaverage_FB,
                        };
                        OVS.Add(StretchReportModelView);

                        StretchReportModelView = new StretchReportModelView()
                        {
                            testID = stretchCalcList_finalOut.testID,
                            description = "SD",
                            IB = stretchCalcList_finalOut.testsd_IB,
                            FB = stretchCalcList_finalOut.testsd_FB,
                        };
                        OVS.Add(StretchReportModelView);

                        StretchReportModelView = new StretchReportModelView()
                        {
                            testID = stretchCalcList_finalOut.testID,
                            description = "CV",
                            IB = stretchCalcList_finalOut.testcv_IB,
                            FB = stretchCalcList_finalOut.testcv_FB,
                        };
                        OVS.Add(StretchReportModelView);

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
                conn.CreateTable<StretchTestModel>();
                List<StretchTestModel> stretchTestModelList = conn.Table<StretchTestModel>().Where(
                                StretchTestModel => (StretchTestModel.status == true &&
                                StretchTestModel.testID != currentTestID &&
                                StretchTestModel.machineID == selectedMachineID)).ToList();

                foreach (StretchTestModel stretchTestModel in stretchTestModelList)
                {
                    stretchTestModel.status = false;
                    if (conn.Update(stretchTestModel) < 1)
                    {
                        dbStatus = false;
                    }
                }


                if (dbStatus)
                {
                    foreach (StretchTestModelView test in stretchTestModelViewList)
                    {
                        StretchTestModel stretchTestModel = new StretchTestModel()
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
                            status = true,
                            createdate = DateTime.Now
                        };
                        int row = conn.Insert(stretchTestModel);
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
                    conn.CreateTable<StretchTestSummaryModel>();
                    List<StretchTestSummaryModel> stretchTestSMList = conn.Table<StretchTestSummaryModel>().Where(
                                    StretchTestSummaryModel => (StretchTestSummaryModel.status == true &&
                                    StretchTestSummaryModel.testID != currentTestID &&
                                    StretchTestSummaryModel.machineID == selectedMachineID)).ToList();

                    foreach (StretchTestSummaryModel stretchTestSM in stretchTestSMList)
                    {
                        stretchTestSM.status = false;
                        if (conn.Update(stretchTestSM) < 1)
                        {
                            dbStatus = false;
                        }
                    }

                    decimal avg_weight = 0m;
                    decimal mean = 0m;
                    decimal sd = 0m;
                    decimal cv = 0m;
                    if (stretchTestModelViewList[0].totaltestcount > 1)
                    {
                        avg_weight = totalWeight / stretchTestModelViewList[0].totaltestcount;
                        mean = totalCalcCountVal / stretchTestModelViewList[0].totaltestcount;
                        decimal IndividualCalValminusMean = 0m;
                        foreach (StretchTestModelView test in stretchTestModelViewList)
                        {
                            IndividualCalValminusMean = IndividualCalValminusMean + ((test.yarnweight - avg_weight) * (test.yarnweight - avg_weight));
                        }
                        sd = (decimal)Math.Sqrt((double)IndividualCalValminusMean / (double)(stretchTestModelViewList[0].totaltestcount - 1));//Standard Deviation
                        cv = (sd / avg_weight) * 100; //Coefficient of Variation
                        avg_weight = Math.Round(avg_weight, 3);
                        mean = Math.Round(mean, 3);
                        sd = Math.Round(sd, 3);
                        cv = Math.Round(cv, 3);
                    }
                    StretchTestSummaryModel stretchTestSummaryModel = new StretchTestSummaryModel()
                    {
                        ID = Guid.NewGuid(),
                        testID = stretchTestModelViewList[0].testID,
                        userID = stretchTestModelViewList[0].userID,
                        userName = stretchTestModelViewList[0].userName,
                        machineID = stretchTestModelViewList[0].machineID,
                        machineCategory = stretchTestModelViewList[0].machineCategory,
                        machineName = stretchTestModelViewList[0].machineName,
                        process = stretchTestModelViewList[0].process,
                        countsysname = stretchTestModelViewList[0].countsysname,
                        yarnlenunit = stretchTestModelViewList[0].yarnlenunit,
                        yarnlength = stretchTestModelViewList[0].yarnlength,
                        shift = stretchTestModelViewList[0].shift,
                        testType = stretchTestModelViewList[0].testType,
                        totaltestcount = stretchTestModelViewList[0].totaltestcount,
                        avg_weight = avg_weight,
                        testaverage = mean,
                        testsd = sd,
                        testcv = cv,
                        status = true,
                        createdate = DateTime.Now
                    };
                    conn.CreateTable<StretchTestSummaryModel>();
                    int row = conn.Insert(stretchTestSummaryModel);
                    if (row < 1)
                    {
                        dbStatus = false;
                    }
                    if (dbStatus)
                    {
                        //await enableTestButton();
                        //await refListView();
                        //await refOverallSummary(mean, sd, cv);
                        if (currentTestType == "FB")
                        {
                            conn.CreateTable<StretchTestCalculatedModel>();
                            List<StretchTestCalculatedModel> stretchCalcList = conn.Table<StretchTestCalculatedModel>().Where(
                               StretchTestCalculatedModel =>
                               (StretchTestCalculatedModel.status == true &&
                               StretchTestCalculatedModel.testID != currentTestID &&
                               StretchTestCalculatedModel.machineID == selectedMachineID)).ToList();

                            foreach (StretchTestCalculatedModel stretch in stretchCalcList)
                            {
                                stretch.status = false;
                                if (conn.Update(stretch) < 1)
                                {
                                    //to be decided if stretch calculated active records failed to deactive
                                }
                            }
                            StretchTestSummaryModel ibSummary = conn.Table<StretchTestSummaryModel>().Where(
                                                            StretchTestSummaryModel => (
                                                            StretchTestSummaryModel.testType == "IB"
                                                            && StretchTestSummaryModel.status == true
                                                            && StretchTestSummaryModel.testID == currentTestID)
                                                            ).FirstOrDefault();
                            if (ibSummary != null)
                            {
                                StretchTestSummaryModel fbSummary = conn.Table<StretchTestSummaryModel>().Where(
                                                            StretchTestSummaryModel => (
                                                            StretchTestSummaryModel.testType == "FB"
                                                            && StretchTestSummaryModel.status == true
                                                            && StretchTestSummaryModel.testID == currentTestID)
                                                            ).FirstOrDefault();
                                if (fbSummary != null)
                                {
                                    decimal stretch = ((ibSummary.avg_weight - fbSummary.avg_weight) / ((ibSummary.avg_weight + fbSummary.avg_weight) / 2m)) * 100m;
                                    stretch = Math.Round(stretch, 3);

                                    StretchTestModel Max_IB = conn.Table<StretchTestModel>().Where(
                                        StretchTestModel =>
                                        (StretchTestModel.testID == currentTestID &&
                                        StretchTestModel.status == true &&
                                        StretchTestModel.testType == "IB")).OrderByDescending(StretchTestModel => StretchTestModel.yarnweight).First();
                                    StretchTestModel Min_IB = conn.Table<StretchTestModel>().Where(
                                        StretchTestModel =>
                                        (StretchTestModel.testID == currentTestID &&
                                        StretchTestModel.status == true &&
                                        StretchTestModel.testType == "IB")).OrderBy(StretchTestModel => StretchTestModel.yarnweight).First();
                                    StretchTestModel Max_FB = conn.Table<StretchTestModel>().Where(
                                        StretchTestModel =>
                                        (StretchTestModel.testID == currentTestID &&
                                        StretchTestModel.status == true &&
                                        StretchTestModel.testType == "FB")).OrderByDescending(StretchTestModel => StretchTestModel.yarnweight).First();
                                    StretchTestModel Min_FB = conn.Table<StretchTestModel>().Where(
                                        StretchTestModel =>
                                        (StretchTestModel.testID == currentTestID &&
                                        StretchTestModel.status == true &&
                                        StretchTestModel.testType == "FB")).OrderBy(StretchTestModel => StretchTestModel.yarnweight).First();

                                    decimal range_IB = Max_IB.yarnweight - Min_IB.yarnweight;
                                    decimal range_FB = Max_FB.yarnweight - Min_FB.yarnweight;

                                    StretchTestCalculatedModel stretchTestCalculatedModel = new StretchTestCalculatedModel()
                                    {
                                        ID = Guid.NewGuid(),
                                        testID = ibSummary.testID,
                                        userID = ibSummary.userID,
                                        userName = ibSummary.userName,
                                        machineID = ibSummary.machineID,
                                        machineCategory = ibSummary.machineCategory,
                                        machineName = ibSummary.machineName,
                                        process = ibSummary.process,
                                        countsysname = ibSummary.countsysname,
                                        yarnlenunit = ibSummary.yarnlenunit,
                                        yarnlength = ibSummary.yarnlength,
                                        shift = ibSummary.shift,
                                        testType = ibSummary.testType,
                                        totaltestcount = ibSummary.totaltestcount,
                                        avg_weight_IB = ibSummary.avg_weight,
                                        testaverage_IB = ibSummary.testaverage,
                                        testsd_IB = ibSummary.testsd,
                                        testcv_IB = ibSummary.testcv,
                                        max_IB = Max_IB.yarnweight,
                                        min_IB = Min_IB.yarnweight,
                                        range_IB = range_IB,
                                        avg_weight_FB = fbSummary.avg_weight,
                                        testaverage_FB = fbSummary.testaverage,
                                        testsd_FB = fbSummary.testsd,
                                        testcv_FB = fbSummary.testcv,
                                        max_FB = Max_FB.yarnweight,
                                        min_FB = Min_FB.yarnweight,
                                        range_FB = range_FB,
                                        stretch = stretch,
                                        status = true,
                                        createdate = DateTime.Now
                                    };
                                    int row_TestCalc = conn.Insert(stretchTestCalculatedModel);
                                    if (row_TestCalc < 1)
                                    {
                                        // To be decieded if stretch test calculated value failed to insert to db
                                    }
                                    else
                                    {
                                        stretchCalcList_finalOut = stretchTestCalculatedModel;
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
                    if (currentTestType == "IB")
                    {
                        startInitialBobbinButton.IsVisible = false;
                        startFullBobbinButton.IsVisible = true;
                        startFullBobbinButton.IsEnabled = true;
                        startFullBobbinButton.BackgroundColor = Color.Green;
                    }
                    else
                    {
                        startFullBobbinButton.IsVisible = false;
                        startInitialBobbinButton.IsVisible = true;
                        startInitialBobbinButton.IsEnabled = true;
                        startInitialBobbinButton.BackgroundColor = Color.Green;
                    }
                }
                if (dispose) { disposeble(); }
                Device.BeginInvokeOnMainThread(() =>
                {

                    if (currentTestType == "FB")
                    {
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
                    }
                    string str_testType = "";
                    if (currentTestType == "IB")
                    {
                        str_testType = "Test Completed for Initial Bobbin!!! Start test for Full Bobbin";
                    }
                    else
                    {
                        currentTestID = 0;
                        str_testType = "All Test Completed!!!";
                    }
                    if (isTestStarted)
                    {
                        isTestStarted = false;
                        showAlert(str_testType);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        [Obsolete]
        private async void startInitialBobbinButton_Clicked(object sender, EventArgs e)
        {
            lbl_TestID.Text = "";
            isTestStarted = true;
            currentTestType = "IB";
            ImageNotification("null");
            UpdateUserNotification("");
            hideFrames();
            await refListView(false);
            await refOverallSummary(0m, 0m, 0m, false);
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
            if (picker_process.SelectedIndex <= 0)
            {
                await DisplayAlert("Attention", "Please enter process info!!!", "Ok");
                return;
            }
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
                StretchTestModel lastTestRecord = null;
                conn.CreateTable<StretchTestModel>();
                int recordCount = conn.Table<StretchTestModel>().Count();

                if (recordCount == 0)
                {
                    currentTestID = 1;
                }
                else
                {
                    DateTime maxDate = conn.Table<StretchTestModel>().Max(StretchTestModel => StretchTestModel.createdate);
                    lastTestRecord = conn.Table<StretchTestModel>()
                        .Where(StretchTestModel => StretchTestModel.createdate == maxDate).FirstOrDefault();
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
            selectedProcess = picker_process.SelectedItem.ToString();
            stretchTestModelViewList = new List<StretchTestModelView>();
            startInitialBobbinButton.IsEnabled = false;
            startInitialBobbinButton.BackgroundColor = Color.SlateGray;
            entry_testcount.IsEnabled = false;
            picker_machinecategory.IsEnabled = false;
            picker_machinename.IsEnabled = false;
            picker_shift.IsEnabled = false;
            picker_process.IsEnabled = false;
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
                                        currentCalculatedValue = Math.Round(drivedVal, 3);
                                        break;
                                    case "Meter":
                                        decimal drivedVal_meter = ((selectedYarnLen * 1.09361m) / 840m) * (1m / ((current_stable_data * 15.4324m) / 7000m));
                                        currentCalculatedValue = Math.Round(drivedVal_meter, 3);
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
                                        currentCalculatedValue = Math.Round(drivedVal, 3);
                                        break;
                                    case "Meter":
                                        decimal drivedVal_meter = current_stable_data * 1000m / selectedYarnLen * 1m;
                                        currentCalculatedValue = Math.Round(drivedVal_meter, 3);
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
                                        currentCalculatedValue = Math.Round(drivedVal, 3);
                                        break;
                                    case "Meter":
                                        decimal drivedVal_meter = current_stable_data * 9000m / selectedYarnLen * 1m;
                                        currentCalculatedValue = Math.Round(drivedVal_meter, 3);
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
                                        currentCalculatedValue = Math.Round(drivedVal, 3);
                                        break;
                                    case "Meter":
                                        decimal drivedVal_meter = (selectedYarnLen * 1m) / ((current_stable_data * 0.001m) * 1000m);
                                        currentCalculatedValue = Math.Round(drivedVal_meter, 3);
                                        break;
                                    default:
                                        break;
                                };
                                break;
                            default:
                                break;
                        };
                        StretchTestModelView stretchTestModelView = new StretchTestModelView()
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
                            yccalcval = currentCalculatedValue
                        };
                        stretchTestModelViewList.Add(stretchTestModelView);
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

                if (stretchTestModelViewList.Count > 0 && passCount == testCount)
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
                                s_op = Math.Round(s_op, 3);
                                if (!initialWeigthCheck)
                                {
                                    if (s_op == ZERO)
                                    {
                                        initialWeigthCheck = true;
                                        ImageNotification("green.png");
                                        UpdateUserNotification("PLACE WEIGHT" + " (T.No - " + currentTestCount + ")", GREEN);
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
                                        UpdateUserNotification("PLACE WEIGHT" + " (T.No - " + currentTestCount + ")", GREEN);
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
                _socket.Close();
                _socket.Dispose();
                device.Dispose();
                adapter.Dispose();
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
                          where bd.Name == "G85219651634"
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

        private async void startFullBobbinButton_Clicked(object sender, EventArgs e)
        {
            isTestStarted = true;
            currentTestType = "FB";
            ImageNotification("null");
            UpdateUserNotification("");
            await refListView(false);
            await refOverallSummary(0m, 0m, 0m, false);
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
            if (picker_process.SelectedIndex <= 0)
            {
                await DisplayAlert("Attention", "Please enter process info!!!", "Ok");
                return;
            }
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
                conn.CreateTable<StretchTestModel>();
                DateTime maxDate = conn.Table<StretchTestModel>().Max(StretchTestModel => StretchTestModel.createdate);
                StretchTestModel lastTestRecord = conn.Table<StretchTestModel>().Where(StretchTestModel => StretchTestModel.createdate == maxDate).FirstOrDefault();
                //StretchTestModel lastTestRecord = conn.Table<StretchTestModel>().OrderByDescending(StretchTestModel => StretchTestModel.testID).FirstOrDefault();
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
            selectedProcess = picker_process.SelectedItem.ToString();
            stretchTestModelViewList = new List<StretchTestModelView>();
            startFullBobbinButton.IsEnabled = false;
            startFullBobbinButton.BackgroundColor = Color.SlateGray;
            entry_testcount.IsEnabled = false;
            picker_machinecategory.IsEnabled = false;
            picker_machinename.IsEnabled = false;
            picker_shift.IsEnabled = false;
            picker_process.IsEnabled = false;
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

                    conn.CreateTable<YarnCountConfigModel>();
                    YarnCountConfigModel yarncountconfigmodel = conn.Table<YarnCountConfigModel>().FirstOrDefault();
                    if (yarncountconfigmodel != null)
                    {
                        if (selectedMachineCategory == "Simplex/SpeedFrame")
                        {
                            entry_yarnlen.Text = yarncountconfigmodel.rovinglength.ToString();
                        }
                        else
                        {
                            entry_yarnlen.Text = yarncountconfigmodel.sliverlength.ToString();
                        }

                    }
                    else
                    {
                        entry_yarnlen.Text = "";
                    }
                }
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
                    return;
                }
                selectedMachineID = (Guid)source[picker_machinename.SelectedIndex].ID;
                MachineModel selectedMachine = (MachineModel)picker_machinename.SelectedItem;
                selectedMachineName = selectedMachine.machineName;
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }
    }
}