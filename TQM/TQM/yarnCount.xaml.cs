using Android.Bluetooth;
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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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

        public yarnCount()
        {
            InitializeComponent();
            lbl_TestID.Text = "";
            //using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            //{
            //    //conn.DropTable<YCTestModel>();
            //    //conn.DropTable<YCTestSummaryModel>();

            //    conn.CreateTable<YarnCountConfigModel>();
            //    YarnCountConfigModel yarncountconfigmodel = conn.Table<YarnCountConfigModel>().FirstOrDefault();
            //    if (yarncountconfigmodel != null)
            //    {
            //        lbl_countsysname.Text = yarncountconfigmodel.countsysname;
            //        lbl_yarncountunit.Text = yarncountconfigmodel.yarnlenunit;
            //        entry_yarnlen.Text = "";
            //        entry_testcount.Text = yarncountconfigmodel.testcount.ToString();
            //        TESTCOUNT = yarncountconfigmodel.testcount;
            //        entry_standardHank.Text = formatDecimal(yarncountconfigmodel.standardHank).ToString();
            //        STD_HANK = formatDecimal(yarncountconfigmodel.standardHank);

            //    }
            //    else
            //    {
            //        lbl_countsysname.Text = "";
            //        lbl_yarncountunit.Text = "";
            //        entry_yarnlen.Text = "";
            //        entry_testcount.Text = "";
            //        picker_shift.SelectedIndex = 0;
            //        picker_process.SelectedIndex = 0;
            //        entry_standardHank.Text = "0.000";
            //    }
            //}
        }

        private void populateTestParams(string mCat, Guid mid, string mac)
        {
            hideFrames();
            if (mCat == "" && mid == Guid.Empty && mac == "")
            {
                selectedDeviationPercent = 0m;
                lbl_countsysname.Text = "";
                lbl_yarncountunit.Text = "";
                entry_yarnlen.Text = "";
                entry_testcount.Text = "";
                picker_shift.SelectedIndex = 0;
                picker_process.SelectedIndex = 0;
                entry_standardHank.Text = "0.000";
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
                    lbl_yarncountunit.Text = yarncountconfigmodel.yarnlenunit;
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
                    entry_standardHank.Text = "0.000";
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
                    listview_testresult_FinalOut.ItemsSource = ycTestModelViewlist;
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
                        span_head.Text = "Count";
                        span_stdValue.Text = " (" + STD_HANK.ToString();
                        span_deviation.Text = " \u00B1" + selectedDeviationPercent + ")";
                    }
                    else
                    {
                        //lbl_testresult_stadHank.Text = "Hank " + STD_HANK.ToString() + "\u00B1" + selectedDeviationPercent + ")";
                        span_head.Text = "Hank";
                        span_stdValue.Text = " (" + STD_HANK.ToString();
                        span_deviation.Text = " \u00B1" + selectedDeviationPercent + ")";
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
                        individualTestResultFrame_FinalOut.BackgroundColor = Color.FromHex("#ffc3c0");
                    }
                    else
                    {
                        individualTestResultFrame_FinalOut.BackgroundColor = Color.White;
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
                }
                if (dbStatus)
                {
                    decimal mean = 0.0000m;
                    decimal sd = 0.0000m;
                    decimal cv = 0.0000m;
                    if (ycTestModelViewlist[0].totaltestcount > 1)
                    {
                        mean = totalCalcCountVal / ycTestModelViewlist[0].totaltestcount;
                        decimal IndividualCalValminusMean = 0m;
                        foreach (YCTestModelView test in ycTestModelViewlist)
                        {
                            IndividualCalValminusMean = IndividualCalValminusMean + ((test.yccalcval - mean) * (test.yccalcval - mean));
                        }
                        sd = (decimal)Math.Sqrt((double)IndividualCalValminusMean / (double)(ycTestModelViewlist[0].totaltestcount - 1));//Standard Deviation
                        sd = formatDecimal(sd);
                        mean = formatDecimal(mean);
                        cv = (sd / mean) * 100.0000m; //Coefficient of Variation
                        cv = formatDecimal(cv);
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
                        testaverage = mean,
                        testsd = sd,
                        testcv = cv,
                        standardHank = STD_HANK_CURR,
                        deviationPercent = selectedDeviationPercent,
                        testDuration = testDuration,
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
                    testYCButton.IsEnabled = true;
                    testYCButton.BackgroundColor = Color.Green;
                    entry_yarnlen.IsEnabled = false;
                    entry_testcount.IsEnabled = true;
                    entry_testcount.Text = TESTCOUNT.ToString();
                    entry_standardHank.IsEnabled = false;
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
        private async void testYCButton_Clicked(object sender, EventArgs e)
        {
            currentTestStartTime = null;
            currentTestStartTime = DateTime.Now;
            lbl_TestID.Text = "";
            isTestStarted = true;
            ImageNotification("null");
            UpdateUserNotification("");
            hideFrames();
            await refListView(false);
            await refOverallSummary(0.0000m, 0.0000m, 0.0000m, false);
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
            if (entry_standardHank.Text.Trim() == "." || entry_standardHank.Text.Trim() == "-")
            {
                await DisplayAlert("Attention", "Standard Hank is invalid. Please check!!!", "Ok");
                return;
            }
            if (entry_standardHank.Text.Trim() == "" || decimal.Parse(entry_standardHank.Text.Trim()) <= 0m)
            {
                await DisplayAlert("Attention", "Standard Hank should not be blank or zero or negative!!!", "Ok");
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
            //if (entry_standardHank.Text.Trim() == "" || int.Parse(entry_standardHank.Text.Trim()) == 0)
            //{
            //    await DisplayAlert("Attention", "Standard Hank should not be blank or zero!!!", "Ok");
            //    return;
            //}
            //if (picker_process.SelectedIndex <= 0)
            //{
            //    await DisplayAlert("Attention", "Please select process info!!!", "Ok");
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
            //if (testCount_str == null || testCount_str == "")
            //{
            //    ImageNotification("red.png");
            //    UpdateUserNotification("Test count cannot be zero!!!");
            //    return;
            //}
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
                DisplayAlert("Attention", "Error Occurred!!!Error: " + ex.Message.ToString(), "OK");
            }
        }
    }
}