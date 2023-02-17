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
        const decimal MIN_VAL_LOAD_CELL = 2.0000m;
        const decimal ZERO = 0.0000m;
        private decimal INITIAL_LOAD_CELL_VALUE = 0.0000m;
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
        private decimal selectedYarnLen = 0.0000m;
        private int selectedTestCount = 0;
        private string selectedShift = null;
        private string selectedProcess = null;
        private const string RED = "#FF0000";
        private const string GREEN = "#145A32";
        private const int BUFFER_WAIT_COUNT = 10;
        private const int BUFFER_WAIT_COUNT_CSP = 200;
        private int TESTCOUNT = 0;
        private decimal STD_HANK = 0.0000m;
        private decimal STD_HANK_CURR = 0.0000m;
        private int currentTestCount = 0;
        private bool isTestStarted = false;
        private string currentTarget = null;
        private RunConfiguration runConfiguration = new RunConfiguration();

        public yarnCountWithCSP()
        {
            InitializeComponent();
            lbl_TestID.Text = "";
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                //conn.DropTable<YCTestModel>();
                //conn.DropTable<YCTestSummaryModel>();

                conn.CreateTable<YarnCountConfigModel>();
                YarnCountConfigModel yarncountconfigmodel = conn.Table<YarnCountConfigModel>().FirstOrDefault();
                if (yarncountconfigmodel != null)
                {
                    lbl_countsysname.Text = yarncountconfigmodel.countsysname;
                    lbl_yarncountunit.Text = yarncountconfigmodel.yarnlenunit;
                    entry_yarnlen.Text = "";
                    entry_testcount.Text = yarncountconfigmodel.testcount.ToString();
                    TESTCOUNT = yarncountconfigmodel.testcount;
                    entry_standardHank.Text = formatDecimal(yarncountconfigmodel.standardHank).ToString();
                    STD_HANK = formatDecimal(yarncountconfigmodel.standardHank);
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
                    listview_testresult_FinalOut.ItemsSource = ycStrengthTestModelViewList;
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
                        listview_testresult.ItemsSource = ycStrengthTestModelViewList.OrderByDescending(YCStrengthTestModelView => YCStrengthTestModelView.testcount);
                    }

                    //if (listview_testresult.ItemsSource != null)
                    //{
                    //    YCTestModelView lastRow = listview_testresult.ItemsSource.Cast<YCTestModelView>().LastOrDefault();
                    //    listview_testresult.ScrollTo(lastRow, ScrollToPosition.MakeVisible, true);
                    //}
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

        private async void updateDB()
        {
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                bool dbStatus = true;
                decimal totalCalcCountVal = 0.0000m;
                decimal CSPSum = 0.0000m;
                conn.CreateTable<YCStrengthTestModel>();
                foreach (YCStrengthTestModelView test in ycStrengthTestModelViewList)
                {
                    YCStrengthTestModel ycStrengthTestModel = new YCStrengthTestModel()
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
                        yarnstrength = test.yarnstrength,
                        CSP = test.CSP,
                        createdate = DateTime.Now
                    };
                    int row = conn.Insert(ycStrengthTestModel);
                    if (row < 1)
                    {
                        dbStatus = false;
                    }
                    totalCalcCountVal = totalCalcCountVal + test.yccalcval;
                    totalCalcCountVal = formatDecimal(totalCalcCountVal);
                    CSPSum = CSPSum + test.CSP;
                    CSPSum = formatDecimal(CSPSum);
                }
                if (dbStatus)
                {
                    decimal mean = 0.0000m;
                    decimal sd = 0.0000m;
                    decimal cv = 0.0000m;

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
                        yarnlength = ycStrengthTestModelViewList[0].yarnlength,
                        totaltestcount = ycStrengthTestModelViewList[0].totaltestcount,
                        testaverage = mean,
                        testsd = sd,
                        testcv = cv,
                        standardHank = STD_HANK_CURR,
                        testRemark = "",
                        avgCSP = mean_CSP,
                        sdCSP = sd_CSP,
                        cvCSP = cv_CSP,
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
                        await refOverallSummary(mean_CSP, sd_CSP, cv_CSP, true, true);
                    }
                }

            }
        }

        private void reset(bool fullreset = true, bool dispose = true)
        {
            try
            {
                current_stable_data = 0;
                if (fullreset) { ImageNotification(null); UpdateUserNotification(""); }
                if (dispose) { disposeble(); }
                Device.BeginInvokeOnMainThread(() =>
                {
                    testYCButton.IsEnabled = true;
                    testYCButton.BackgroundColor = Color.Green;
                    entry_yarnlen.IsEnabled = true;
                    entry_testcount.IsEnabled = true;
                    entry_testcount.Text = TESTCOUNT.ToString();
                    entry_standardHank.IsEnabled = true;
                    entry_standardHank.Text = STD_HANK_CURR.ToString();
                    picker_machinecategory.IsEnabled = true;
                    picker_machinecategory.SelectedIndex = 0;
                    picker_machinename.IsEnabled = true;
                    picker_machinename.SelectedIndex = 0;
                    picker_shift.IsEnabled = true;
                    picker_shift.SelectedIndex = 0;
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
            currentTarget = "YCB";
            string testCount_str = entry_testcount.Text;
            int testCount = int.Parse(testCount_str);
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
            ycStrengthTestModelViewList = new List<YCStrengthTestModelView>();
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
                            yarnlength = selectedYarnLen,
                            totaltestcount = selectedTestCount,
                            testcount = i + 1,
                            yarnweight = current_stable_data,
                            yccalcval = currentCalculatedValue,
                            yarnstrength = 0.0000m,
                            CSP = 0.0000m
                        };
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
                            UpdateUserNotification("CSP - COMMUNICATION ERROR!!!");
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
                            decimal yarnstrength = formatDecimal(current_stable_data);
                            decimal CSP = formatDecimal(ycStrengthTestModelViewList[i].yccalcval * (yarnstrength * 2.20462m));
                            ycStrengthTestModelViewList[i].yarnstrength = yarnstrength;
                            ycStrengthTestModelViewList[i].CSP = CSP;

                            await refListView();
                        }
                        else
                        {
                            reset(false);
                            break;
                        }
                    }
                    else
                    {
                        reset(false);
                        break;
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
                        await refOverallSummary();
                    }
                }

                //if (runResult)
                //{
                //    reset();
                //}
                //else
                //{
                //    reset(false);
                //}

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
                UpdateUserNotification("Waiting for CSP data" + " (S.No - " + currentTestCount + ")", GREEN);

                List<decimal> balOutput = ListenCSP();

                Debug.WriteLine("Recieved from Bluetooth adapter is [" + balOutput + "]");
                if (balOutput.Count != 0)
                {
                    balOutput.Sort();
                    current_stable_data = balOutput[balOutput.Count - 1];
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
            List<decimal> ipList = new List<decimal>();
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

                            while (op != null)
                            {
                                op = RemoveSpecialCharacters(buffer.ReadLine());
                                decimal op_dec = decimal.Parse(op);

                                if (!initialValueCheck)
                                {
                                    if (op_dec == INITIAL_LOAD_CELL_VALUE || op_dec < MIN_VAL_LOAD_CELL)
                                    {
                                        ImageNotification("green.png");
                                        UpdateUserNotification("Waiting for CSP data" + " (S.No - " + currentTestCount + ")", GREEN);
                                    }
                                    else
                                    {
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