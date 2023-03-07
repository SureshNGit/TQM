using Android.Bluetooth;
using Java.IO;
//using Java.Lang;
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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class yarnCountWithCSP : ContentPage, INotifyPropertyChanged
    {
        private BluetoothSocket _socket;
        BluetoothAdapter adapter;
        BluetoothDevice device;
        const decimal MIN_VAL = 0.4000m;
        const decimal MIN_VAL_LOAD_CELL = 10.0000m;
        const decimal ZERO = 0.0000m;
        private decimal INITIAL_LOAD_CELL_VALUE = 0.0000m;
        private decimal INITIAL_LOAD_CELL_CHECK = 1.0000m;
        const int PER_TEST_LOOP_COUNT = 100;
        const int DATA_READ_LOOP_COUNT = 100;
        const int STABLE_DATA_CHECK = 5;
        private decimal current_stable_data = 0;
        private List<YCStrengthTestModelView> ycStrengthTestModelViewList;
        private long currentTestID = 0;
        private UserModel currentloggedInUser = null;
        private string selectedMachineCategory = null;
        private Guid selectedMachineID = Guid.Empty;
        private string selectedMachineName = null;
        private string selectedSysName = null;
        private string selectedCountUnit = null;
        private string selectedStrengthUnit = null;
        private decimal selectedYarnLen = 0.0000m;
        private int selectedTestCount = 0;
        private string selectedShift = null;
        private string selectedProcess = null;
        private const string RED = "#FF0000";
        private const string GREEN = "#145A32";
        private const int BUFFER_WAIT_COUNT = 10;
        private const int BUFFER_WAIT_COUNT_CSP = 200;
        private int TESTCOUNT = 0;
        private int STD_CSP = 0;
        private int STD_CSP_CURR = 0;
        private int currentTestCount = 0;
        private bool isTestStarted = false;
        //private string currentTarget = null;
        private bool resumeTest = false;
        private bool pageNavigated = true;
        private RunConfiguration runConfiguration = new RunConfiguration();

        public yarnCountWithCSP()
        {
            InitializeComponent();
            //initializeTest();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            initializeTest();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            //disposeble();
            reset();
        }

        private void updateShift()
        {
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<YarnCountConfigModel>();
                YarnCountConfigModel yarncountconfigmodel = conn.Table<YarnCountConfigModel>().FirstOrDefault();
                if (yarncountconfigmodel != null)
                {
                    TimeSpan shit1time = TimeSpan.FromHours(TimeSpan.Parse(yarncountconfigmodel.shift1time).TotalHours);
                    TimeSpan shit2time = TimeSpan.FromHours(TimeSpan.Parse(yarncountconfigmodel.shift2time).TotalHours);
                    TimeSpan shit3time = TimeSpan.FromHours(TimeSpan.Parse(yarncountconfigmodel.shift3time).TotalHours);
                    TimeSpan currentTime = TimeSpan.FromHours(TimeSpan.Parse(DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString()).TotalHours);
                    if (currentTime >= shit1time && currentTime < shit2time)
                    {
                        picker_shift.SelectedItem = "Shift-1";
                    }
                    else if (currentTime >= shit2time && currentTime < shit3time)
                    {
                        picker_shift.SelectedItem = "Shift-2";
                    }
                    else
                    {
                        picker_shift.SelectedItem = "Shift-3";
                    }
                }
                else
                {
                    picker_shift.SelectedIndex = 0;
                }
            }
        }


        private void initializeTest()
        {
            lbl_TestID.Text = "";
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                //conn.DropTable<YCStrengthTestModel>();
                //conn.DropTable<YCStrengthTestSummaryModel>();
                //conn.DropTable<TestResumeCheck>();

                conn.CreateTable<YCStrengthTestModel>();
                conn.CreateTable<YCStrengthTestSummaryModel>();
                conn.CreateTable<TestResumeCheck>();


                conn.CreateTable<YarnCountConfigModel>();
                YarnCountConfigModel yarncountconfigmodel = conn.Table<YarnCountConfigModel>().FirstOrDefault();
                if (yarncountconfigmodel != null)
                {
                    lbl_countsysname.Text = yarncountconfigmodel.countsysname;
                    if (yarncountconfigmodel.yarnStrengthUnit == null || yarncountconfigmodel.yarnlenunit == null)
                    {
                        lbl_yarncountunit.Text = "";
                    }
                    else
                    {
                        lbl_yarncountunit.Text = yarncountconfigmodel.yarnlenunit.ToString() + "/ " + yarncountconfigmodel.yarnStrengthUnit.ToString();
                    }
                    entry_yarnlen.Text = yarncountconfigmodel.yarnLength.ToString();
                    entry_testcount.Text = yarncountconfigmodel.testcount.ToString();
                    TESTCOUNT = yarncountconfigmodel.testcount;
                    entry_standardHank.Text = yarncountconfigmodel.standardCSP.ToString();
                    STD_CSP = yarncountconfigmodel.standardCSP;
                    updateShift();
                }
                else
                {
                    lbl_countsysname.Text = "";
                    lbl_yarncountunit.Text = "";
                    entry_yarnlen.Text = "";
                    entry_testcount.Text = "";
                    picker_shift.SelectedIndex = 0;
                    picker_process.SelectedIndex = 0;
                    entry_standardHank.Text = "0.000";
                }


                UserModel loggedInUser = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                if (loggedInUser == null)
                {
                    DisplayAlert("Attention", "Unable to get logged user information!!!", "OK");
                    return;
                }
                else
                {
                    currentloggedInUser = loggedInUser;
                }



                List<YCStrengthTestModel> allTest = conn.Table<YCStrengthTestModel>().ToList();

                if (allTest.Count > 0)
                {
                    DateTime maxDate = conn.Table<YCStrengthTestModel>().Max(YCStrengthTestModel => YCStrengthTestModel.createdate);
                    YCStrengthTestModel lastTestRecord = conn.Table<YCStrengthTestModel>()
                                        .Where(YCStrengthTestModel => YCStrengthTestModel.createdate == maxDate).FirstOrDefault();
                    if (lastTestRecord != null)
                    {
                        List<YCStrengthTestModel> lastTest = conn.Table<YCStrengthTestModel>()
                                                            .Where(YCStrengthTestModel => YCStrengthTestModel.testID == lastTestRecord.testID).ToList();

                        //all sample tests are not completed so there will not an entry in test summary table
                        //resume test 

                        lastTest.OrderBy(YCStrengthTestModel => YCStrengthTestModel.testcount);

                        int lastTestTotalCount = lastTest.Count - 1;
                        Guid lastTestPK = lastTest[lastTestTotalCount].ID;
                        long lastTestID = lastTest[lastTestTotalCount].testID;
                        int lastTestCount = lastTest[lastTestTotalCount].testcount;

                        if (lastTest[lastTestTotalCount].yarnstrength == 0.0000m && lastTest[lastTestTotalCount].CSP == 0)
                        {

                            YCStrengthTestModel inValidRec = conn.Table<YCStrengthTestModel>()
                                    .Where(YCStrengthTestModel => (YCStrengthTestModel.ID == lastTestPK
                                                                    && YCStrengthTestModel.testcount == lastTestCount)).FirstOrDefault();
                            if (inValidRec != null)
                            {

                                int row = conn.Delete(inValidRec);


                                if (row > 0)
                                {
                                    string displayusername = currentloggedInUser.firstname + " [" + currentloggedInUser.userId + "]";
                                    if (currentloggedInUser.firstname != "")
                                    {
                                        displayusername = currentloggedInUser.firstname + ", " + currentloggedInUser.lastname + " [" + currentloggedInUser.userId + "]";
                                    }
                                    TestResumeCheck testResume = new TestResumeCheck()
                                    {
                                        ID = Guid.NewGuid(),
                                        testID = lastTestID,
                                        testcount = lastTestCount,
                                        testType = "CSP",
                                        userID = currentloggedInUser.ID,
                                        userName = displayusername,
                                        createdate = DateTime.Now
                                    };
                                    int res = conn.Insert(testResume);
                                    if (res > 0)
                                    {
                                        List<YCStrengthTestModel> partialTest = conn.Table<YCStrengthTestModel>()
                                                      .Where(YCStrengthTestModel => YCStrengthTestModel.testID == inValidRec.testID).ToList();
                                        if (partialTest.Count > 0)
                                        {
                                            resumeTest = true;
                                            testYCButton.Text = "Resume";
                                            lbl_TestID.Text = inValidRec.testID.ToString();
                                            currentTestID = inValidRec.testID;
                                            currentTestCount = inValidRec.testcount;
                                            lbl_countsysname.Text = inValidRec.countsysname;
                                            lbl_yarncountunit.Text = inValidRec.yarnlenunit.ToString() + "/ " + inValidRec.yarnstrengthunit.ToString();
                                            selectedStrengthUnit = inValidRec.yarnstrengthunit.ToString();
                                            entry_yarnlen.Text = inValidRec.yarnlength.ToString();
                                            entry_testcount.Text = inValidRec.totaltestcount.ToString();
                                            TESTCOUNT = inValidRec.totaltestcount;
                                            entry_standardHank.Text = inValidRec.standardCSP.ToString();
                                            STD_CSP = inValidRec.standardCSP;

                                            IList<string> mclist = picker_machinecategory.Items;
                                            int mcindex = 0;
                                            foreach (string mc in mclist)
                                            {
                                                if (mc != inValidRec.machineCategory)
                                                {
                                                    mcindex++;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            picker_machinecategory.SelectedIndex = mcindex;


                                            updateShift();

                                            IList<string> mlist = picker_machinename.Items;
                                            int mindex = 0;
                                            foreach (string m in mlist)
                                            {
                                                if (m != inValidRec.machineName)
                                                {
                                                    mindex++;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            picker_machinename.SelectedIndex = mindex;
                                            selectedMachineID = inValidRec.machineID;

                                            IList<string> plist = picker_process.Items;
                                            int pindex = 0;
                                            foreach (string p in plist)
                                            {
                                                if (p != inValidRec.process)
                                                {
                                                    pindex++;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            picker_process.SelectedIndex = pindex;
                                            selectedProcess = inValidRec.process;



                                            entry_yarnlen.IsEnabled = false;
                                            entry_testcount.IsEnabled = false;
                                            entry_standardHank.IsEnabled = false;
                                            picker_shift.IsEnabled = false;
                                            picker_process.IsEnabled = false;
                                            picker_machinecategory.IsEnabled = false;
                                            picker_machinename.IsEnabled = false;

                                            ycStrengthTestModelViewList = new List<YCStrengthTestModelView>();
                                            foreach (YCStrengthTestModel pt in partialTest)
                                            {
                                                YCStrengthTestModelView stvm = new YCStrengthTestModelView()
                                                {
                                                    testID = pt.testID,
                                                    userID = pt.userID,
                                                    userName = pt.userName,
                                                    machineID = pt.machineID,
                                                    machineCategory = pt.machineCategory,
                                                    machineName = pt.machineName,
                                                    shift = pt.shift,
                                                    process = pt.process,
                                                    countsysname = pt.countsysname,
                                                    yarnlenunit = pt.yarnlenunit,
                                                    yarnstrengthunit = pt.yarnstrengthunit,
                                                    yarnlength = formatDecimal(pt.yarnlength),
                                                    totaltestcount = pt.totaltestcount,
                                                    testcount = pt.testcount,
                                                    yarnweight = formatDecimal(pt.yarnweight),
                                                    yccalcval = formatDecimal(pt.yccalcval),
                                                    standardCSP = pt.standardCSP,
                                                    yarnstrength = formatDecimal(pt.yarnstrength),
                                                    CSP = Convert.ToInt32(pt.CSP)
                                                };
                                                ycStrengthTestModelViewList.Add(stvm);
                                            }
                                            refListView(true);
                                        }
                                        else
                                        {
                                            //need to decide 
                                            //This condition will occur when the CSP machine off at the 1st sample 
                                            resumeTest = true;
                                            testYCButton.Text = "Resume";
                                            lbl_TestID.Text = inValidRec.testID.ToString();
                                            currentTestID = inValidRec.testID;
                                            currentTestCount = inValidRec.testcount;
                                            lbl_countsysname.Text = inValidRec.countsysname;
                                            lbl_yarncountunit.Text = inValidRec.yarnlenunit.ToString() + "/ " + inValidRec.yarnstrengthunit.ToString();
                                            selectedStrengthUnit = inValidRec.yarnstrengthunit.ToString();
                                            entry_yarnlen.Text = inValidRec.yarnlength.ToString();
                                            entry_testcount.Text = inValidRec.totaltestcount.ToString();
                                            TESTCOUNT = inValidRec.totaltestcount;
                                            entry_standardHank.Text = inValidRec.standardCSP.ToString();
                                            STD_CSP = inValidRec.standardCSP;

                                            IList<string> mclist = picker_machinecategory.Items;
                                            int mcindex = 0;
                                            foreach (string mc in mclist)
                                            {
                                                if (mc != inValidRec.machineCategory)
                                                {
                                                    mcindex++;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            picker_machinecategory.SelectedIndex = mcindex;


                                            updateShift();

                                            IList<string> mlist = picker_machinename.Items;
                                            int mindex = 0;
                                            foreach (string m in mlist)
                                            {
                                                if (m != inValidRec.machineName)
                                                {
                                                    mindex++;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            picker_machinename.SelectedIndex = mindex;
                                            selectedMachineID = inValidRec.machineID;

                                            IList<string> plist = picker_process.Items;
                                            int pindex = 0;
                                            foreach (string p in plist)
                                            {
                                                if (p != inValidRec.process)
                                                {
                                                    pindex++;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            picker_process.SelectedIndex = pindex;
                                            selectedProcess = inValidRec.process;



                                            entry_yarnlen.IsEnabled = false;
                                            entry_testcount.IsEnabled = false;
                                            entry_standardHank.IsEnabled = false;
                                            picker_shift.IsEnabled = false;
                                            picker_process.IsEnabled = false;
                                            picker_machinecategory.IsEnabled = false;
                                            picker_machinename.IsEnabled = false;
                                            ycStrengthTestModelViewList = new List<YCStrengthTestModelView>();
                                            refListView(true);
                                        }
                                    }
                                    else
                                    {
                                        //need to decide 
                                    }

                                }
                                else
                                {
                                    // To be decided how to proceed!!!
                                }
                            }
                            else
                            {
                                // To be decided how to proceed!!!
                            }
                        }
                        else
                        {
                            //this condition is possible when the invalid record was deleted and user navigated to some other
                            //screens without resuming the test and come back to count+strength screen

                            //int lastTestTotalCount = lastTest.Count - 1;
                            //Guid lastTestPK = lastTest[lastTestTotalCount].ID;
                            //long lastTestID = lastTest[lastTestTotalCount].testID;
                            //int lastTestCount = lastTest[lastTestTotalCount].testcount;

                            if (lastTest.Count != lastTestRecord.totaltestcount)
                            {

                                List<YCStrengthTestModel> partialTest = conn.Table<YCStrengthTestModel>()
                                                      .Where(YCStrengthTestModel => YCStrengthTestModel.testID == lastTestID).ToList();
                                if (partialTest.Count > 0)
                                {
                                    resumeTest = true;
                                    testYCButton.Text = "Resume";
                                    lbl_TestID.Text = lastTest[lastTestTotalCount].testID.ToString();
                                    currentTestID = lastTest[lastTestTotalCount].testID;
                                    currentTestCount = lastTest[lastTestTotalCount].testcount + 1;
                                    lbl_countsysname.Text = lastTest[lastTestTotalCount].countsysname;
                                    lbl_yarncountunit.Text = lastTest[lastTestTotalCount].yarnlenunit.ToString() + "/ " + lastTest[lastTestTotalCount].yarnstrengthunit.ToString();
                                    selectedStrengthUnit = lastTest[lastTestTotalCount].yarnstrengthunit.ToString();
                                    selectedStrengthUnit = lastTest[lastTestTotalCount].yarnstrengthunit.ToString();
                                    entry_yarnlen.Text = lastTest[lastTestTotalCount].yarnlength.ToString();
                                    entry_testcount.Text = lastTest[lastTestTotalCount].totaltestcount.ToString();
                                    TESTCOUNT = lastTest[lastTestTotalCount].totaltestcount;
                                    entry_standardHank.Text = lastTest[lastTestTotalCount].standardCSP.ToString();
                                    STD_CSP = lastTest[lastTestTotalCount].standardCSP;

                                    IList<string> mclist = picker_machinecategory.Items;
                                    int mcindex = 0;
                                    foreach (string mc in mclist)
                                    {
                                        if (mc != lastTest[lastTestTotalCount].machineCategory)
                                        {
                                            mcindex++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    picker_machinecategory.SelectedIndex = mcindex;


                                    updateShift();

                                    IList<string> mlist = picker_machinename.Items;
                                    int mindex = 0;
                                    foreach (string m in mlist)
                                    {
                                        if (m != lastTest[lastTestTotalCount].machineName)
                                        {
                                            mindex++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    picker_machinename.SelectedIndex = mindex;
                                    selectedMachineID = lastTest[lastTestTotalCount].machineID;

                                    IList<string> plist = picker_process.Items;
                                    int pindex = 0;
                                    foreach (string p in plist)
                                    {
                                        if (p != lastTest[lastTestTotalCount].process)
                                        {
                                            pindex++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    picker_process.SelectedIndex = pindex;
                                    selectedProcess = lastTest[lastTestTotalCount].process;



                                    entry_yarnlen.IsEnabled = false;
                                    entry_testcount.IsEnabled = false;
                                    entry_standardHank.IsEnabled = false;
                                    picker_shift.IsEnabled = false;
                                    picker_process.IsEnabled = false;
                                    picker_machinecategory.IsEnabled = false;
                                    picker_machinename.IsEnabled = false;

                                    ycStrengthTestModelViewList = new List<YCStrengthTestModelView>();
                                    foreach (YCStrengthTestModel pt in partialTest)
                                    {
                                        YCStrengthTestModelView stvm = new YCStrengthTestModelView()
                                        {
                                            testID = pt.testID,
                                            userID = pt.userID,
                                            userName = pt.userName,
                                            machineID = pt.machineID,
                                            machineCategory = pt.machineCategory,
                                            machineName = pt.machineName,
                                            shift = pt.shift,
                                            process = pt.process,
                                            countsysname = pt.countsysname,
                                            yarnlenunit = pt.yarnlenunit,
                                            yarnstrengthunit = pt.yarnstrengthunit,
                                            yarnlength = formatDecimal(pt.yarnlength),
                                            totaltestcount = pt.totaltestcount,
                                            testcount = pt.testcount,
                                            yarnweight = formatDecimal(pt.yarnweight),
                                            yccalcval = formatDecimal(pt.yccalcval),
                                            standardCSP = pt.standardCSP,
                                            yarnstrength = formatDecimal(pt.yarnstrength),
                                            CSP = Convert.ToInt32(pt.CSP)
                                        };
                                        ycStrengthTestModelViewList.Add(stvm);
                                    }
                                    refListView(true);
                                }
                            }
                        }
                    }


                }
            }
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
                //frame_overallSummary_FinalOut.IsVisible = false;
                //frame_overallSummary.IsVisible = false;
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

                    List<YCStrengthTestModelView> currentTestList = ycStrengthTestModelViewList.OrderBy(YCStrengthTestModelView => YCStrengthTestModelView.testcount).ToList();
                    List<YCStrengthTestReportModelView> finalReportList = new List<YCStrengthTestReportModelView>();

                    YCStrengthTestReportModelView reportView = new YCStrengthTestReportModelView()
                    {
                        testDescription = "",
                        count = "grams",
                        strength = selectedStrengthUnit,
                        CSP = "lbs"
                    };

                    finalReportList.Add(reportView);

                    foreach (YCStrengthTestModelView test in currentTestList)
                    {
                        reportView = new YCStrengthTestReportModelView()
                        {
                            testDescription = test.testcount.ToString(),
                            count = formatDecimal(test.yccalcval).ToString(),
                            strength = formatDecimal(test.yarnstrength).ToString(),
                            CSP = test.CSP.ToString()
                        };
                        finalReportList.Add(reportView);
                    };

                    using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                    {
                        YCStrengthTestSummaryModel testSummary = conn.Table<YCStrengthTestSummaryModel>().Where(YCStrengthTestSummaryModel => YCStrengthTestSummaryModel.testID == currentTestID).FirstOrDefault();
                        if (testSummary != null)
                        {
                            reportView = new YCStrengthTestReportModelView()
                            {
                                testDescription = "AVG",
                                count = formatDecimal(testSummary.testaverage).ToString(),
                                strength = formatDecimal(testSummary.avgStrength).ToString(),
                                CSP = formatDecimal(testSummary.avgCSP).ToString()
                            };
                            finalReportList.Add(reportView);

                            reportView = new YCStrengthTestReportModelView()
                            {
                                testDescription = "SD",
                                count = formatDecimal(testSummary.testsd).ToString(),
                                strength = formatDecimal(testSummary.sdStrength).ToString(),
                                CSP = formatDecimal(testSummary.sdCSP).ToString()
                            };
                            finalReportList.Add(reportView);

                            reportView = new YCStrengthTestReportModelView()
                            {
                                testDescription = "CV",
                                count = formatDecimal(testSummary.testcv).ToString(),
                                strength = formatDecimal(testSummary.cvStrength).ToString(),
                                CSP = formatDecimal(testSummary.cvCSP).ToString()
                            };
                            finalReportList.Add(reportView);

                            reportView = new YCStrengthTestReportModelView()
                            {
                                testDescription = "MIN",
                                count = formatDecimal(testSummary.testMin).ToString(),
                                strength = formatDecimal(testSummary.StrengthMin).ToString(),
                                CSP = Convert.ToInt32(testSummary.CSPMin).ToString()
                            };
                            finalReportList.Add(reportView);

                            reportView = new YCStrengthTestReportModelView()
                            {
                                testDescription = "MAX",
                                count = formatDecimal(testSummary.testMax).ToString(),
                                strength = formatDecimal(testSummary.StrengthMax).ToString(),
                                CSP = Convert.ToInt32(testSummary.CSPMax).ToString()
                            };
                            finalReportList.Add(reportView);

                            reportView = new YCStrengthTestReportModelView()
                            {
                                testDescription = "RANGE",
                                count = formatDecimal(testSummary.testRange).ToString(),
                                strength = formatDecimal(testSummary.StrengthRange).ToString(),
                                CSP = Convert.ToInt32(testSummary.CSPRange).ToString()
                            };
                            finalReportList.Add(reportView);
                        }
                    }
                    listview_testresult_FinalOut.ItemsSource = finalReportList;
                }
                else
                {
                    listview_testresult_FinalOut.ItemsSource = null;
                    listview_testresult_FinalOut.IsVisible = false; ;
                    individualTestResultFrame_FinalOut.IsVisible = false;

                    individualTestResultFrame.IsVisible = true;
                    listview_testresult.IsVisible = visibility;
                    if (ycStrengthTestModelViewList != null)
                    {
                        listview_testresult.ItemsSource = null;
                        List<YCStrengthTestModelView> currentTestList = ycStrengthTestModelViewList.OrderByDescending(YCStrengthTestModelView => YCStrengthTestModelView.testcount).ToList();
                        List<YCStrengthTestReportModelView> finalReportList = new List<YCStrengthTestReportModelView>();

                        YCStrengthTestReportModelView reportView = new YCStrengthTestReportModelView()
                        {
                            testDescription = "",
                            count = "grams",
                            strength = selectedStrengthUnit,
                            CSP = "lbs"
                        };

                        finalReportList.Add(reportView);

                        foreach (YCStrengthTestModelView test in currentTestList)
                        {
                            reportView = new YCStrengthTestReportModelView()
                            {
                                testDescription = test.testcount.ToString(),
                                count = formatDecimal(test.yccalcval).ToString(),
                                strength = formatDecimal(test.yarnstrength).ToString(),
                                CSP = test.CSP.ToString()
                            };
                            finalReportList.Add(reportView);
                        };

                        listview_testresult.ItemsSource = finalReportList;


                        //if (listview_testresult.ItemsSource != null)
                        //{
                        //    YCTestModelView lastRow = listview_testresult.ItemsSource.Cast<YCTestModelView>().LastOrDefault();
                        //    listview_testresult.ScrollTo(lastRow, ScrollToPosition.MakeVisible, true);
                        //}
                    }
                }
            });
        }

        //private async Task refOverallSummary(decimal mean = 0m, decimal sd = 0m, decimal cv = 0m, bool visibility = true, bool showFinalOut = false)
        //{
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        if (showFinalOut)
        //        {
        //            frame_overallSummary_FinalOut.IsVisible = visibility;
        //            lbl_average_FinalOut.Text = mean.ToString();
        //            lbl_sd_FinalOut.Text = sd.ToString();
        //            lbl_cv_FinalOut.Text = cv.ToString();
        //        }
        //        else
        //        {
        //            frame_overallSummary.IsVisible = visibility;
        //            lbl_average.Text = mean.ToString();
        //            lbl_sd.Text = sd.ToString();
        //            lbl_cv.Text = cv.ToString();
        //        }
        //    });
        //}

        private async void updateDB()
        {
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                bool dbStatus = true;
                decimal totalCalcCountVal = 0.0000m;
                decimal StrengthSum = 0.0000m;
                decimal CSPSum = 0.0000m;

                decimal min_Count = 0.0000m;
                decimal max_Count = 0.0000m;
                decimal range_Count = 0.0000m;

                decimal min_Strength = 0.0000m;
                decimal max_Strength = 0.0000m;
                decimal range_Strength = 0.0000m;

                decimal min_CSP = 0.0000m;
                decimal max_CSP = 0.0000m;
                decimal range_CSP = 0.0000m;



                conn.CreateTable<YCStrengthTestModel>();
                foreach (YCStrengthTestModelView test in ycStrengthTestModelViewList)
                {
                    totalCalcCountVal = totalCalcCountVal + test.yccalcval;
                    totalCalcCountVal = formatDecimal(totalCalcCountVal);
                    StrengthSum = CSPSum + test.yarnstrength;
                    StrengthSum = formatDecimal(StrengthSum);
                    CSPSum = CSPSum + test.CSP;
                    CSPSum = formatDecimal(CSPSum);
                }
                if (dbStatus)
                {
                    decimal mean = 0.0000m;
                    decimal sd = 0.0000m;
                    decimal cv = 0.0000m;


                    decimal mean_Strength = 0.0000m;
                    decimal sd_Strength = 0.0000m;
                    decimal cv_Strength = 0.0000m;


                    decimal mean_CSP = 0.0000m;
                    decimal sd_CSP = 0.0000m;
                    decimal cv_CSP = 0.0000m;


                    if (ycStrengthTestModelViewList[0].totaltestcount > 1)
                    {
                        mean = totalCalcCountVal / ycStrengthTestModelViewList[0].totaltestcount;
                        decimal IndividualCalValminusMean = 0m;
                        foreach (YCStrengthTestModelView test in ycStrengthTestModelViewList)
                        {
                            IndividualCalValminusMean = IndividualCalValminusMean + ((test.yccalcval - mean) * (test.yccalcval - mean));
                        }
                        sd = (decimal)Math.Sqrt((double)IndividualCalValminusMean / (double)(ycStrengthTestModelViewList[0].totaltestcount - 1));//Standard Deviation
                        sd = formatDecimal(sd);
                        mean = formatDecimal(mean);
                        cv = (sd / mean) * 100.0000m; //Coefficient of Variation
                        cv = formatDecimal(cv);



                        mean_CSP = CSPSum / ycStrengthTestModelViewList[0].totaltestcount;
                        decimal IndividualCSPminusMean = 0m;
                        foreach (YCStrengthTestModelView test in ycStrengthTestModelViewList)
                        {
                            IndividualCSPminusMean = IndividualCSPminusMean + ((test.CSP - mean_CSP) * (test.CSP - mean_CSP));
                        }
                        sd_CSP = (decimal)Math.Sqrt((double)IndividualCSPminusMean / (double)(ycStrengthTestModelViewList[0].totaltestcount - 1));//Standard Deviation
                        sd_CSP = formatDecimal(sd_CSP);
                        mean_CSP = formatDecimal(mean_CSP);
                        cv_CSP = (sd_CSP / mean_CSP) * 100.0000m; //Coefficient of Variation
                        cv_CSP = formatDecimal(cv_CSP);

                        mean_Strength = StrengthSum / ycStrengthTestModelViewList[0].totaltestcount;
                        decimal IndividualStrengthminusMean = 0m;
                        foreach (YCStrengthTestModelView test in ycStrengthTestModelViewList)
                        {
                            IndividualStrengthminusMean = IndividualStrengthminusMean + ((test.yarnstrength - mean_Strength) * (test.yarnstrength - mean_Strength));
                        }
                        sd_Strength = (decimal)Math.Sqrt((double)IndividualStrengthminusMean / (double)(ycStrengthTestModelViewList[0].totaltestcount - 1));//Standard Deviation
                        sd_Strength = formatDecimal(sd_Strength);
                        mean_Strength = formatDecimal(mean_Strength);
                        cv_Strength = (sd_Strength / mean_Strength) * 100.0000m; //Coefficient of Variation
                        cv_Strength = formatDecimal(cv_Strength);


                        YCStrengthTestModelView countMinRec = ycStrengthTestModelViewList.OrderBy(YCStrengthTestModel => YCStrengthTestModel.yccalcval).First();
                        min_Count = formatDecimal(countMinRec.yccalcval);
                        YCStrengthTestModelView countMaxRec = ycStrengthTestModelViewList.OrderByDescending(YCStrengthTestModel => YCStrengthTestModel.yccalcval).First();
                        max_Count = formatDecimal(countMaxRec.yccalcval);
                        range_Count = formatDecimal(max_Count - min_Count);

                        YCStrengthTestModelView countMinRec_Strength = ycStrengthTestModelViewList.OrderBy(YCStrengthTestModel => YCStrengthTestModel.yarnstrength).First();
                        min_Strength = formatDecimal(countMinRec_Strength.yarnstrength);
                        YCStrengthTestModelView countMaxRec_Strength = ycStrengthTestModelViewList.OrderByDescending(YCStrengthTestModel => YCStrengthTestModel.yarnstrength).First();
                        max_Strength = formatDecimal(countMaxRec_Strength.yarnstrength);
                        range_Strength = formatDecimal(max_Strength - min_Strength);

                        YCStrengthTestModelView countMinRec_CSP = ycStrengthTestModelViewList.OrderBy(YCStrengthTestModel => YCStrengthTestModel.CSP).First();
                        min_CSP = formatDecimal(countMinRec_CSP.CSP);
                        YCStrengthTestModelView countMaxRec_CSP = ycStrengthTestModelViewList.OrderByDescending(YCStrengthTestModel => YCStrengthTestModel.CSP).First();
                        max_CSP = formatDecimal(countMaxRec_CSP.CSP);
                        range_CSP = formatDecimal(max_CSP - min_CSP);


                    }
                    YCStrengthTestSummaryModel ycStrengthTestSummaryModel = new YCStrengthTestSummaryModel()
                    {
                        ID = Guid.NewGuid(),
                        testID = ycStrengthTestModelViewList[0].testID,
                        userID = ycStrengthTestModelViewList[0].userID,
                        userName = ycStrengthTestModelViewList[0].userName,
                        machineID = ycStrengthTestModelViewList[0].machineID,
                        machineCategory = ycStrengthTestModelViewList[0].machineCategory,
                        machineName = ycStrengthTestModelViewList[0].machineName,
                        shift = ycStrengthTestModelViewList[0].shift,
                        process = ycStrengthTestModelViewList[0].process,
                        countsysname = ycStrengthTestModelViewList[0].countsysname,
                        yarnlenunit = ycStrengthTestModelViewList[0].yarnlenunit,
                        yarnstrengthunit = ycStrengthTestModelViewList[0].yarnstrengthunit,
                        yarnlength = ycStrengthTestModelViewList[0].yarnlength,
                        totaltestcount = ycStrengthTestModelViewList[0].totaltestcount,
                        testaverage = mean,
                        testsd = sd,
                        testcv = cv,
                        testMin = min_Count,
                        testMax = max_Count,
                        testRange = range_Count,
                        standardCSP = STD_CSP_CURR,
                        avgStrength = mean_Strength,
                        sdStrength = sd_Strength,
                        cvStrength = cv_Strength,
                        StrengthMin = min_Strength,
                        StrengthMax = max_Strength,
                        StrengthRange = range_Strength,
                        avgCSP = mean_CSP,
                        sdCSP = sd_CSP,
                        cvCSP = cv_CSP,
                        CSPMin = min_CSP,
                        CSPMax = max_CSP,
                        CSPRange = range_CSP,
                        testRemark = "",
                        createdate = DateTime.Now
                    };
                    conn.CreateTable<YCStrengthTestSummaryModel>();
                    int row = conn.Insert(ycStrengthTestSummaryModel);
                    if (row < 1)
                    {
                        dbStatus = false;
                    }
                    if (dbStatus)
                    {
                        await refListView(true, true);
                        //await refOverallSummary(mean_CSP, sd_CSP, cv_CSP, true, true);
                    }
                }
            }
        }


        private void initializeResumeTest()
        {
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                //conn.DropTable<YCStrengthTestModel>();
                //conn.DropTable<YCStrengthTestSummaryModel>();
                //conn.DropTable<TestResumeCheck>();

                conn.CreateTable<YCStrengthTestModel>();
                conn.CreateTable<YCStrengthTestSummaryModel>();
                conn.CreateTable<TestResumeCheck>();

                UserModel loggedInUser = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                if (loggedInUser == null)
                {
                    DisplayAlert("Attention", "Unable to get logged user information!!!", "OK");
                    return;
                }
                else
                {
                    currentloggedInUser = loggedInUser;
                }

                List<YCStrengthTestModel> allTest = conn.Table<YCStrengthTestModel>().ToList();

                if (allTest.Count > 0)
                {
                    DateTime maxDate = conn.Table<YCStrengthTestModel>().Max(YCStrengthTestModel => YCStrengthTestModel.createdate);
                    YCStrengthTestModel lastTestRecord = conn.Table<YCStrengthTestModel>()
                                        .Where(YCStrengthTestModel => YCStrengthTestModel.createdate == maxDate).FirstOrDefault();
                    if (lastTestRecord != null)
                    {
                        List<YCStrengthTestModel> lastTest = conn.Table<YCStrengthTestModel>()
                                                            .Where(YCStrengthTestModel => YCStrengthTestModel.testID == lastTestRecord.testID).ToList();
                        //if (lastTest.Count != lastTestRecord.totaltestcount)
                        //{
                        //all sample tests are not completed so there will not an entry in test summary table
                        //resume test 

                        lastTest.OrderBy(YCStrengthTestModel => YCStrengthTestModel.testcount);

                        int lastTestTotalCount = lastTest.Count - 1;
                        Guid lastTestPK = lastTest[lastTestTotalCount].ID;
                        long lastTestID = lastTest[lastTestTotalCount].testID;
                        int lastTestCount = lastTest[lastTestTotalCount].testcount;

                        if (lastTest[lastTestTotalCount].yarnstrength == 0.0000m && lastTest[lastTestTotalCount].CSP == 0)
                        {

                            YCStrengthTestModel inValidRec = conn.Table<YCStrengthTestModel>()
                                    .Where(YCStrengthTestModel => (YCStrengthTestModel.ID == lastTestPK
                                                                    && YCStrengthTestModel.testcount == lastTestCount)).FirstOrDefault();
                            if (inValidRec != null)
                            {

                                int row = row = conn.Delete(inValidRec);

                                if (row > 0)
                                {
                                    string displayusername = currentloggedInUser.firstname + " [" + currentloggedInUser.userId + "]";
                                    if (currentloggedInUser.firstname != "")
                                    {
                                        displayusername = currentloggedInUser.firstname + ", " + currentloggedInUser.lastname + " [" + currentloggedInUser.userId + "]";
                                    }
                                    TestResumeCheck testResume = new TestResumeCheck()
                                    {
                                        ID = Guid.NewGuid(),
                                        testID = lastTestID,
                                        testcount = lastTestCount,
                                        testType = "CSP",
                                        userID = currentloggedInUser.ID,
                                        userName = displayusername,
                                        createdate = DateTime.Now
                                    };
                                    int res = conn.Insert(testResume);
                                    if (res > 0)
                                    {
                                        List<YCStrengthTestModel> partialTest = conn.Table<YCStrengthTestModel>()
                                                      .Where(YCStrengthTestModel => YCStrengthTestModel.testID == inValidRec.testID).ToList();
                                        if (partialTest.Count > 0)
                                        {
                                            resumeTest = true;
                                            testYCButton.Text = "Resume";
                                            currentTestID = inValidRec.testID;
                                            currentTestCount = inValidRec.testcount;
                                            lbl_countsysname.Text = inValidRec.countsysname;
                                            lbl_yarncountunit.Text = inValidRec.yarnlenunit.ToString() + "/ " + inValidRec.yarnstrengthunit.ToString();
                                            selectedStrengthUnit = inValidRec.yarnstrengthunit.ToString();
                                            entry_yarnlen.Text = inValidRec.yarnlength.ToString();
                                            entry_testcount.Text = inValidRec.totaltestcount.ToString();
                                            TESTCOUNT = inValidRec.totaltestcount;
                                            entry_standardHank.Text = inValidRec.standardCSP.ToString();
                                            STD_CSP = inValidRec.standardCSP;

                                            IList<string> mclist = picker_machinecategory.Items;
                                            int mcindex = 0;
                                            foreach (string mc in mclist)
                                            {
                                                if (mc != inValidRec.machineCategory)
                                                {
                                                    mcindex++;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            picker_machinecategory.SelectedIndex = mcindex;


                                            updateShift();

                                            IList<string> mlist = picker_machinename.Items;
                                            int mindex = 0;
                                            foreach (string m in mlist)
                                            {
                                                if (m != inValidRec.machineName)
                                                {
                                                    mindex++;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            picker_machinename.SelectedIndex = mindex;
                                            selectedMachineID = inValidRec.machineID;

                                            IList<string> plist = picker_process.Items;
                                            int pindex = 0;
                                            foreach (string p in plist)
                                            {
                                                if (p != inValidRec.process)
                                                {
                                                    pindex++;
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            picker_process.SelectedIndex = pindex;
                                            selectedProcess = inValidRec.process;


                                            entry_yarnlen.IsEnabled = false;
                                            entry_testcount.IsEnabled = false;
                                            entry_standardHank.IsEnabled = false;
                                            picker_shift.IsEnabled = false;
                                            picker_process.IsEnabled = false;
                                            picker_machinecategory.IsEnabled = false;
                                            picker_machinename.IsEnabled = false;

                                            ycStrengthTestModelViewList = new List<YCStrengthTestModelView>();
                                            foreach (YCStrengthTestModel pt in partialTest)
                                            {
                                                YCStrengthTestModelView stvm = new YCStrengthTestModelView()
                                                {
                                                    testID = pt.testID,
                                                    userID = pt.userID,
                                                    userName = pt.userName,
                                                    machineID = pt.machineID,
                                                    machineCategory = pt.machineCategory,
                                                    machineName = pt.machineName,
                                                    shift = pt.shift,
                                                    process = pt.process,
                                                    countsysname = pt.countsysname,
                                                    yarnlenunit = pt.yarnlenunit,
                                                    yarnstrengthunit = pt.yarnstrengthunit,
                                                    yarnlength = formatDecimal(pt.yarnlength),
                                                    totaltestcount = pt.totaltestcount,
                                                    testcount = pt.testcount,
                                                    yarnweight = formatDecimal(pt.yarnweight),
                                                    yccalcval = formatDecimal(pt.yccalcval),
                                                    standardCSP = pt.standardCSP,
                                                    yarnstrength = formatDecimal(pt.yarnstrength),
                                                    CSP = Convert.ToInt32(pt.CSP)
                                                };
                                                ycStrengthTestModelViewList.Add(stvm);
                                            }
                                            refListView(true);
                                        }
                                        else
                                        {
                                            //need to decide 
                                            //This condition will occur when the CSP machine off at the 1st sample 
                                            ycStrengthTestModelViewList = new List<YCStrengthTestModelView>();
                                            refListView(true);
                                        }
                                    }
                                    else
                                    {
                                        //need to decide 
                                    }

                                }
                                else
                                {
                                    // To be decided how to proceed!!!
                                }
                            }
                            else
                            {
                                // To be decided how to proceed!!!
                            }
                        }
                        //}

                    }
                }
            }
        }

        private void reset(bool fullreset = true, bool dispose = true)
        {
            try
            {
                current_stable_data = 0;
                pageNavigated = true;
                resumeTest = false;
                if (fullreset) { ImageNotification(null); UpdateUserNotification(""); }
                if (dispose) { disposeble(); }
                Device.BeginInvokeOnMainThread(() =>
                {
                    testYCButton.Text = "Start";
                    testYCButton.IsEnabled = true;
                    testYCButton.BackgroundColor = Color.Green;
                    entry_yarnlen.IsEnabled = true;
                    entry_testcount.IsEnabled = true;
                    entry_testcount.Text = TESTCOUNT.ToString();
                    entry_standardHank.IsEnabled = true;
                    entry_standardHank.Text = STD_CSP_CURR.ToString();
                    picker_machinecategory.IsEnabled = true;
                    picker_machinecategory.SelectedIndex = 0;
                    picker_machinename.IsEnabled = true;
                    picker_machinename.SelectedIndex = 0;
                    updateShift();
                    //picker_shift.IsEnabled = true;
                    //picker_shift.SelectedIndex = 0;
                    picker_process.SelectedIndex = 0;
                    picker_process.IsEnabled = true;
                    if (isTestStarted)
                    {
                        if (ycStrengthTestModelViewList != null)
                        {
                            if (selectedTestCount != ycStrengthTestModelViewList.Count())
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
        private async void testYCButton_Clicked(object sender, EventArgs e)
        {

            ImageNotification("null");
            UpdateUserNotification("");


            isTestStarted = true;

            if (resumeTest == true && pageNavigated == false)
            {
                initializeResumeTest();
            }

            if (!resumeTest)
            {
                lbl_TestID.Text = "";
                hideFrames();
                updateShift();
                await refListView(false);
                //await refOverallSummary(0.0000m, 0.0000m, 0.0000m, false);
            }

            if (lbl_countsysname.Text.Trim() != "Nec")
            {
                await DisplayAlert("Attention", "Count system name/method should be 'NEC'. Please change it in Settings!!!", "Ok");
                return;
            }

            if (!lbl_yarncountunit.Text.Trim().Contains("Yard"))
            {
                await DisplayAlert("Attention", "Lea measuring unit should be 'Yard'. Please change it in Settings !!!", "Ok");
                return;
            }

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
            if (int.Parse(entry_yarnlen.Text.Trim()) != 120 || int.Parse(entry_yarnlen.Text.Trim()) != 60)
            {
                await DisplayAlert("Attention", "Yarn Length should be either 120 or 60 yards!!!", "Ok");
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
            if (entry_standardHank.Text.Trim() == "." || entry_standardHank.Text.Trim() == "-")
            {
                await DisplayAlert("Attention", "Standard Count is invalid. Please check!!!", "Ok");
                return;
            }
            if (entry_standardHank.Text.Trim() == "" || decimal.Parse(entry_standardHank.Text.Trim()) <= 0m)
            {
                await DisplayAlert("Attention", "Standard Count should not be blank or zero or negative!!!", "Ok");
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
            disposeble();
            if (!initializeBluetooth(runConfiguration.getLoadCellSerailNo()))
            {
                ImageNotification("red.png");
                UpdateUserNotification("CSP - COMMUNICATION ERROR!!!");
                return;
            }
            //READ INITIAL LOAD CELL VALUE

            INITIAL_LOAD_CELL_VALUE = 0.0000m;
            String balOutput = getInitialCSPValue();

            Debug.WriteLine("Recieved from Bluetooth adapter is [" + balOutput + "]");
            if (balOutput != "")
            {
                if (balOutput == "fail")
                {
                    ImageNotification("red.png");
                    UpdateUserNotification("COMMUNICATION ERROR!!!");
                    Debug.WriteLine("Read data failed");
                    return;
                }
                else
                {
                    decimal s_op = decimal.Parse(balOutput);
                    INITIAL_LOAD_CELL_VALUE = formatDecimal(s_op);
                }
            }

            //END OF READ
            disposeble();
            if (!initializeBluetooth(runConfiguration.getBalanceSerialNo()))
            {
                ImageNotification("red.png");
                UpdateUserNotification("Balance - COMMUNICATION ERROR!!!");
                return;
            }
            //currentTarget = "YCB";
            string testCount_str = entry_testcount.Text;
            int testCount = int.Parse(testCount_str);

            if (!resumeTest)
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    YCStrengthTestModel lastTestRecord = null;
                    conn.CreateTable<YCStrengthTestModel>();
                    int recordCount = conn.Table<YCStrengthTestModel>().Count();

                    if (recordCount == 0)
                    {
                        currentTestID = 1;
                    }
                    else
                    {
                        DateTime maxDate = conn.Table<YCStrengthTestModel>().Max(YCStrengthTestModel => YCStrengthTestModel.createdate);
                        lastTestRecord = conn.Table<YCStrengthTestModel>()
                            .Where(YCStrengthTestModel => YCStrengthTestModel.createdate == maxDate).FirstOrDefault();
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
            }

            selectedSysName = lbl_countsysname.Text;
            selectedCountUnit = lbl_yarncountunit.Text.ToString().Split('/')[0].Trim();
            selectedStrengthUnit = lbl_yarncountunit.Text.ToString().Split('/')[1].Trim();
            selectedYarnLen = int.Parse(entry_yarnlen.Text);
            selectedTestCount = int.Parse(entry_testcount.Text);
            STD_CSP_CURR = Convert.ToInt32(entry_standardHank.Text);
            selectedShift = picker_shift.SelectedItem.ToString();
            selectedProcess = "";
            if (picker_process.SelectedIndex > 0)
            {
                selectedProcess = picker_process.SelectedItem.ToString();
            }
            testYCButton.IsEnabled = false;
            testYCButton.BackgroundColor = Color.SlateGray;

            if (!resumeTest)
            {
                ycStrengthTestModelViewList = new List<YCStrengthTestModelView>();
                entry_yarnlen.IsEnabled = false;
                entry_testcount.IsEnabled = false;
                entry_standardHank.IsEnabled = false;
                picker_shift.IsEnabled = false;
                picker_process.IsEnabled = false;
                picker_machinecategory.IsEnabled = false;
                picker_machinename.IsEnabled = false;
            }

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
                int startLoopCount = 0;
                if (resumeTest) { startLoopCount = currentTestCount - 1; passCount = currentTestCount - 1; resumeTest = false; }
                for (int i = startLoopCount; i < testCount; i++)
                {
                    runResult = false;
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));

                    currentTestCount = i + 1;
                    disposeble();
                    if (!initializeBluetooth(runConfiguration.getBalanceSerialNo()))
                    {
                        ImageNotification("red.png");
                        UpdateUserNotification("Balance - COMMUNICATION ERROR!!!");

                        //call resume test method
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            testYCButton.Text = "Resume";
                            testYCButton.IsEnabled = true;
                            testYCButton.BackgroundColor = Color.Green;
                        });
                        resumeTest = true;
                        pageNavigated = false;
                        return;
                    }
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
                        YCStrengthTestModelView ycStrengthTestModelView = new YCStrengthTestModelView()
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
                            yarnstrengthunit = selectedStrengthUnit,
                            yarnlength = selectedYarnLen,
                            totaltestcount = selectedTestCount,
                            testcount = i + 1,
                            yarnweight = current_stable_data,
                            yccalcval = currentCalculatedValue,
                            standardCSP = STD_CSP_CURR,
                            yarnstrength = 0.0000m,
                            CSP = 0,
                        };

                        using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                        {
                            YCStrengthTestModel ycStrengthTestModel = new YCStrengthTestModel()
                            {
                                ID = Guid.NewGuid(),
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
                                yarnstrengthunit = selectedStrengthUnit,
                                yarnlength = selectedYarnLen,
                                totaltestcount = selectedTestCount,
                                testcount = i + 1,
                                yarnweight = current_stable_data,
                                yccalcval = currentCalculatedValue,
                                standardCSP = STD_CSP_CURR,
                                yarnstrength = 0.0000m,
                                CSP = 0,
                                createdate = DateTime.Now
                            };
                            int row = conn.Insert(ycStrengthTestModel);
                            if (row < 1)
                            {
                                ImageNotification("red.png");
                                UpdateUserNotification("Count - DB ERROR!!!");
                                return;
                            }
                        }

                        ycStrengthTestModelViewList.Add(ycStrengthTestModelView);
                        await refListView();

                        runResult = false;
                        src = new CancellationTokenSource();
                        ct = src.Token;
                        ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                        disposeble();
                        if (!initializeBluetooth(runConfiguration.getLoadCellSerailNo()))
                        {
                            ImageNotification("red.png");
                            UpdateUserNotification("CSP - Communication error!!!");
                            //call resume test method
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                testYCButton.Text = "Resume";
                                testYCButton.IsEnabled = true;
                                testYCButton.BackgroundColor = Color.Green;
                            });
                            resumeTest = true;
                            pageNavigated = false;
                            return;
                        }
                        await Task.Run(async () => await RunCSPTest(), ct).ContinueWith((t) =>
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
                        if (runResult)
                        {
                            var buffer = new BufferedReader(new InputStreamReader(_socket.InputStream));
                            System.Threading.Thread.Sleep(1000);

                            if (!buffer.Ready())
                            {

                                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                                {
                                    conn.CreateTable<TestResumeCheck>();
                                    TestResumeCheck resumeCheck = conn.Table<TestResumeCheck>()
                                        .Where(TestResumeCheck => (TestResumeCheck.testID == currentTestID
                                                                        && TestResumeCheck.testcount == currentTestCount)).FirstOrDefault();
                                    if (resumeCheck != null)
                                    {
                                        int row = conn.Delete(resumeCheck);
                                        if (row < 0)
                                        {
                                            ImageNotification("red.png");
                                            UpdateUserNotification("CSP - RESUME TEST DELETE ERROR!!!");
                                            return;
                                        }
                                    }
                                }


                                //reset(false);
                                ImageNotification("red.png");
                                UpdateUserNotification("CSP - Machine Off!!!");
                                //call resume test method
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    testYCButton.Text = "Resume";
                                    testYCButton.IsEnabled = true;
                                    testYCButton.BackgroundColor = Color.Green;
                                });
                                resumeTest = true;
                                pageNavigated = false;
                                return;
                            }
                            else
                            {

                                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                                {
                                    conn.CreateTable<TestResumeCheck>();
                                    TestResumeCheck testResume = new TestResumeCheck()
                                    {
                                        ID = Guid.NewGuid(),
                                        testID = currentTestID,
                                        testcount = currentTestCount,
                                        testType = "CSP",
                                        userID = currentloggedInUser.ID,
                                        userName = displayusername,
                                        createdate = DateTime.Now
                                    };
                                    int res = conn.Insert(testResume);
                                    if (res < 0)
                                    {
                                        ImageNotification("red.png");
                                        UpdateUserNotification("CSP - RESUME TEST ERROR!!!");
                                        return;
                                    }
                                }
                            }

                            decimal yarnstrength = 0.0000m;

                            decimal CSP = 0.0000m;
                            if (selectedStrengthUnit == "Kg")
                            {
                                if (selectedYarnLen == 120.0000m)
                                {
                                    yarnstrength = formatDecimal(current_stable_data);
                                    CSP = formatDecimal(ycStrengthTestModelViewList[i].yccalcval * (yarnstrength * 2.20462m));
                                }
                                else if (selectedYarnLen == 60.0000m)
                                {
                                    yarnstrength = formatDecimal(current_stable_data * 2.0000m);
                                    CSP = formatDecimal(ycStrengthTestModelViewList[i].yccalcval * (yarnstrength * 2.20462m));
                                }
                            }
                            else if (selectedStrengthUnit == "lbs")
                            {
                                if (selectedYarnLen == 120.0000m)
                                {
                                    yarnstrength = formatDecimal(current_stable_data * 2.20462m);
                                    CSP = formatDecimal(ycStrengthTestModelViewList[i].yccalcval * yarnstrength);
                                }
                                else if (selectedYarnLen == 60.0000m)
                                {
                                    yarnstrength = formatDecimal((current_stable_data * 2) * 2.20462m);
                                    CSP = formatDecimal(ycStrengthTestModelViewList[i].yccalcval * yarnstrength);
                                }
                            }



                            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                            {
                                YCStrengthTestModel curTestRec = conn.Table<YCStrengthTestModel>()
                                    .Where(YCStrengthTestModel => (YCStrengthTestModel.testID == currentTestID
                                                                    && YCStrengthTestModel.testcount == currentTestCount)).FirstOrDefault();
                                if (curTestRec != null)
                                {
                                    curTestRec.yarnstrength = yarnstrength;
                                    curTestRec.CSP = Convert.ToInt32(CSP);
                                    int row = conn.Update(curTestRec);
                                    if (row < 0)
                                    {
                                        ImageNotification("red.png");
                                        UpdateUserNotification("CSP - UPDATE DB ERROR!!!");
                                        return;
                                    }
                                }
                                else
                                {
                                    ImageNotification("red.png");
                                    UpdateUserNotification("CSP - DB ERROR!!!");
                                    return;
                                }
                            }

                            ycStrengthTestModelViewList[i].yarnstrength = yarnstrength;
                            ycStrengthTestModelViewList[i].CSP = Convert.ToInt32(CSP);

                            await refListView();
                        }
                        else
                        {
                            //reset(false);
                            //break;
                            disposeble();
                            ImageNotification("red.png");
                            UpdateUserNotification("CSP - Machine Off!!!");
                            //call resume test method
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                testYCButton.Text = "Resume";
                                testYCButton.IsEnabled = true;
                                testYCButton.BackgroundColor = Color.Green;
                            });
                            resumeTest = true;
                            pageNavigated = false;
                            return;
                        }
                    }
                    else
                    {
                        //reset(false);
                        //break;
                        disposeble();
                        ImageNotification("red.png");
                        UpdateUserNotification("IMPROPER TEST!!!");
                        //call resume test method
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            testYCButton.Text = "Resume";
                            testYCButton.IsEnabled = true;
                            testYCButton.BackgroundColor = Color.Green;
                        });
                        resumeTest = true;
                        pageNavigated = false;
                        return;
                    }
                }

                if (ycStrengthTestModelViewList.Count > 0 && passCount == testCount)
                {
                    updateDB();
                }
                else
                {
                    if (passCount > 0)
                    {
                        await refListView();
                        //await refOverallSummary();
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
                showAlert("Handle Test - COMMUNICATION ERROR!!! " + ex.ToString());
                reset();
            }
        }

        private async Task<bool> RunTest()
        {
            try
            {
                //bool blueState = true;
                //if (blueState)
                //{
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
                            UpdateUserNotification("Balance - COMMUNICATION ERROR!!! Data reception failure");
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
                //}
                //else
                //{
                //    ImageNotification("red.png");
                //    UpdateUserNotification("COMMUNICATION ERROR!!!");
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                ImageNotification("red.png");
                UpdateUserNotification("Balance-COMMUNICATION ERROR!!! " + ex.ToString());
                return false;
            }
        }


        private async Task<bool> RunCSPTest()
        {
            try
            {
                ImageNotification("green.png");
                UpdateUserNotification("Checking CSP data. Please wait...", GREEN);

                List<decimal> balOutput = ListenCSP();
                if (balOutput == null)
                {
                    ImageNotification("red.png");
                    UpdateUserNotification("CSP-COMMUNICATION ERROR!!! Data reception failure");
                    Debug.WriteLine("Read data failed");
                    return false;
                }
                Debug.WriteLine("Recieved from Bluetooth adapter is [" + balOutput + "]");
                if (balOutput.Count != 0)
                {
                    balOutput.Sort();
                    current_stable_data = balOutput[balOutput.Count - 1];

                    //if (INITIAL_LOAD_CELL_VALUE < 0.0000m)
                    //{
                    //    current_stable_data = balOutput[balOutput.Count - 1] + INITIAL_LOAD_CELL_VALUE;
                    //}
                    //else
                    //{
                    //    current_stable_data = balOutput[balOutput.Count - 1] - INITIAL_LOAD_CELL_VALUE;
                    //}



                    ImageNotification(null);
                    UpdateUserNotification("");
                    return true;
                }
                else
                {
                    ImageNotification("red.png");
                    UpdateUserNotification("CSP-COMMUNICATION ERROR!!! Data reception failure");
                    Debug.WriteLine("Read data failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                ImageNotification("red.png");
                UpdateUserNotification("CSP-COMMUNICATION ERROR!!! " + ex.ToString());
                return false;
            }
        }



        private void disposeble()
        {
            try
            {
                if (_socket != null) { _socket.Close(); _socket.Dispose(); }
                if (device != null) device.Dispose();
                if (adapter != null) adapter.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception handled!!!! Error:" + ex.Message.ToString());
            }
        }

        [Obsolete]
        public bool initializeBluetooth(string deviceName)
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
                          where bd.Name == deviceName
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

        private string getInitialCSPValue()
        {
            string op = "";
            bool Listening = true;
            Debug.WriteLine("Listening has been started.");
            while (Listening)
            {
                try
                {
                    int dataLoop = 0;
                    while (true)
                    {
                        var buffer = new BufferedReader(new InputStreamReader(_socket.InputStream));
                        System.Threading.Thread.Sleep(1000);
                        if (buffer.Ready())
                        {
                            op = RemoveSpecialCharacters(buffer.ReadLine());
                            while (op != null)
                            {
                                decimal op_dec = decimal.Parse(op);
                                return op_dec.ToString();

                            }
                        }
                        else
                        {

                            disposeble();
                            if (!initializeBluetooth(runConfiguration.getLoadCellSerailNo()))
                            {
                                ImageNotification("red.png");
                                UpdateUserNotification("CSP - Communication Error!!!");
                                return "fail";
                            }
                            continue;
                        }
                        dataLoop = dataLoop + 1;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error: " + e.Message);
                    Listening = false;
                    return "fail";
                }
            }
            Debug.WriteLine("Listening has ended....");
            return "fail";
        }

        private List<decimal> ListenCSP()
        {
            string op = "";
            List<decimal> ipList = null;
            bool Listening = true;
            Debug.WriteLine("Listening has been started.");
            while (Listening)
            {
                try
                {
                    bool initialValueCheck = false;
                    while (true)
                    {
                        var buffer = new BufferedReader(new InputStreamReader(_socket.InputStream));
                        System.Threading.Thread.Sleep(1000);
                        if (buffer.Ready())
                        {
                            decimal prevInitalVal = 0.0000m;
                            int initCounter = 0;
                            int initCounterBreakVal = 10;
                            bool initialAssigned = false;
                            while (op != null)
                            {
                                string ipData = buffer.ReadLine();
                                if (ipData.Contains("-"))
                                {
                                    ImageNotification("red.png");
                                    UpdateUserNotification("CSP-Remove lea and ensure zero...", RED);
                                    continue;
                                }
                                op = RemoveSpecialCharacters(ipData);
                                decimal op_dec = decimal.Parse(op);

                                if (!initialValueCheck)
                                {
                                    if (INITIAL_LOAD_CELL_VALUE > MIN_VAL_LOAD_CELL || INITIAL_LOAD_CELL_VALUE >= INITIAL_LOAD_CELL_CHECK)
                                    {
                                        if (op_dec > INITIAL_LOAD_CELL_CHECK)
                                        {
                                            ImageNotification("red.png");
                                            UpdateUserNotification("CSP-Remove lea and wait...", RED);
                                        }
                                        else
                                        {
                                            if (prevInitalVal > op_dec || (prevInitalVal == 0.0000m && initialAssigned == false))
                                            {
                                                prevInitalVal = op_dec;
                                                initCounter = 0;
                                                initialAssigned = true;
                                            }
                                            else if (prevInitalVal == op_dec || op_dec == 0.000m)
                                            {
                                                if (initCounter >= initCounterBreakVal)
                                                {
                                                    INITIAL_LOAD_CELL_VALUE = op_dec;
                                                }
                                                else
                                                {
                                                    initCounter = initCounter + 1;
                                                }
                                            }
                                            else
                                            {
                                                prevInitalVal = op_dec;
                                                initCounter = 0;
                                            }
                                        }
                                    }
                                    else if (op_dec == INITIAL_LOAD_CELL_VALUE || op_dec < MIN_VAL_LOAD_CELL)
                                    {
                                        ImageNotification("green.png");
                                        UpdateUserNotification("Waiting for CSP data" + " (S.No - " + currentTestCount + ")", GREEN);
                                    }
                                    else
                                    {
                                        ipList = new List<decimal>();
                                        ipList.Add(op_dec);
                                        initialValueCheck = true;
                                        ImageNotification("green.png");
                                        UpdateUserNotification("Reading CSP data" + " (S.No - " + currentTestCount + ")", GREEN);
                                    }
                                }
                                else
                                {
                                    ipList.Add(op_dec);
                                    if (op_dec <= INITIAL_LOAD_CELL_VALUE || op_dec == 0.0000m || op_dec < MIN_VAL_LOAD_CELL)
                                    {
                                        return ipList;
                                    }
                                    else
                                    {
                                        ImageNotification("green.png");
                                        UpdateUserNotification("Reading CSP data" + " (S.No - " + currentTestCount + ")", GREEN);
                                    }
                                }


                            }
                        }
                        else
                        {

                            disposeble();
                            if (!initializeBluetooth(runConfiguration.getLoadCellSerailNo()))
                            {
                                ImageNotification("red.png");
                                UpdateUserNotification("CSP - Communication Error!!!");
                                return ipList;
                            }
                            continue;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error: " + e.Message);
                    Listening = false;
                    return ipList;
                }
            }
            Debug.WriteLine("Listening has ended....");
            return ipList;
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
                    YCStrengthTestSummaryModel summaryModel = conn.Table<YCStrengthTestSummaryModel>().Where(
                        YCStrengthTestSummaryModel => YCStrengthTestSummaryModel.testID == testID).FirstOrDefault();
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
    }
}