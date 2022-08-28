
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
        const int STABLE_DATA_CHECK = 15;
        private decimal current_stable_data = 0;
        private List<YCTestApercentModelView> ycTestApercentModelViewlist;
        private long currentTestID = 0;
        private UserModel currentloggedInUser = null;
        private string selectedSysName = null;
        private string selectedCountUnit = null;
        private decimal selectedYarnLen = 0m;
        private int selectedTestCount = 0;
        private string selectedShift = null;
        private string selectedProcess = null;
        private string currentTestType = null;
        private const string RED = "#FF0000";
        private const string GREEN = "#145A32";
        private const int BUFFER_WAIT_COUNT = 10;
        private int TESTCOUNT = 0;
        private bool isTestStarted = false;

        public ApercentPage()
        {
            InitializeComponent();
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                //conn.DropTable<YCTestApercentModel>();
                //conn.DropTable<YCTestApercentSummaryModel>();
                //conn.DropTable<YCTestApercentCalculatedModel>();

                conn.CreateTable<YarnCountConfigModel>();
                YarnCountConfigModel yarncountconfigmodel = conn.Table<YarnCountConfigModel>().FirstOrDefault();
                if (yarncountconfigmodel != null)
                {
                    lbl_countsysname.Text = yarncountconfigmodel.countsysname;
                    lbl_yarncountunit.Text = yarncountconfigmodel.yarnlenunit;
                    lbl_yarnlen.Text = yarncountconfigmodel.yarnlength.ToString();
                    entry_testcount.Text = yarncountconfigmodel.testcountApercent.ToString();
                    TESTCOUNT = yarncountconfigmodel.testcountApercent;
                }
                else
                {
                    lbl_countsysname.Text = "";
                    lbl_yarncountunit.Text = "";
                    lbl_yarnlen.Text = "";
                    entry_testcount.Text = "";
                    picker_shift.SelectedIndex = 0;
                    entry_process.Text = "";
                }
                conn.CreateTable<YCTestApercentCalculatedModel>();
                YCTestApercentCalculatedModel ycTestApercentCalculatedModel = conn.Table<YCTestApercentCalculatedModel>().Where(
                    YCTestApercentCalculatedModel => (YCTestApercentCalculatedModel.testType == "nMinus1" &&
                    YCTestApercentCalculatedModel.status == false)).FirstOrDefault();
                if (ycTestApercentCalculatedModel != null)
                {
                    startTestNm1Button.IsVisible = true;
                }
                else
                {
                    ycTestApercentCalculatedModel = conn.Table<YCTestApercentCalculatedModel>().Where(
                        YCTestApercentCalculatedModel =>
                        (YCTestApercentCalculatedModel.testType == "nMinus1" && YCTestApercentCalculatedModel.status == true) &&
                        (YCTestApercentCalculatedModel.testType == "N" && YCTestApercentCalculatedModel.status == false)
                        ).FirstOrDefault();
                    if (ycTestApercentCalculatedModel != null)
                    {
                        startTestNButton.IsVisible = true;
                    }
                    else
                    {
                        ycTestApercentCalculatedModel = conn.Table<YCTestApercentCalculatedModel>().Where(
                       YCTestApercentCalculatedModel =>
                       (YCTestApercentCalculatedModel.testType == "nMinus1" && YCTestApercentCalculatedModel.status == true) &&
                       (YCTestApercentCalculatedModel.testType == "N" && YCTestApercentCalculatedModel.status == true) &&
                       (YCTestApercentCalculatedModel.testType == "nPlus1" && YCTestApercentCalculatedModel.status == false)
                       ).FirstOrDefault();
                        if (ycTestApercentCalculatedModel != null)
                        {
                            startTestNp1Button.IsVisible = true;
                        }
                        else
                        {
                            startTestNm1Button.IsVisible = true;
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

        private async Task refListView(bool visibility = true)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                listview_testresult.ItemsSource = null;
                listview_testresult.IsVisible = visibility;
                listview_testresult.ItemsSource = ycTestApercentModelViewlist;
            });
        }

        private async Task refOverallSummary(decimal mean = 0m, decimal sd = 0m, decimal cv = 0m, bool visibility = true)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                frame_overallSummary.IsVisible = visibility;
                lbl_average.Text = mean.ToString();
                lbl_sd.Text = sd.ToString();
                lbl_cv.Text = cv.ToString();
            });
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
                                YCTestApercentModel => (YCTestApercentModel.status == true && YCTestApercentModel.testID != currentTestID)).ToList();

                foreach (YCTestApercentModel ycTestApercentModel in ycTestApercentModelList)
                {
                    ycTestApercentModel.status = false;
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
                    decimal avg_weight = 0m;
                    decimal mean = 0m;
                    decimal sd = 0m;
                    decimal cv = 0m;
                    if (ycTestApercentModelViewlist[0].totaltestcount > 1)
                    {
                        avg_weight = totalWeight / ycTestApercentModelViewlist[0].totaltestcount;
                        mean = totalCalcCountVal / ycTestApercentModelViewlist[0].totaltestcount;
                        decimal IndividualCalValminusMean = 0m;
                        foreach (YCTestApercentModelView test in ycTestApercentModelViewlist)
                        {
                            IndividualCalValminusMean = IndividualCalValminusMean + ((test.yccalcval - mean) * (test.yccalcval - mean));
                        }
                        sd = (decimal)Math.Sqrt((double)IndividualCalValminusMean / (double)(ycTestApercentModelViewlist[0].totaltestcount - 1));//Standard Deviation
                        cv = (sd / mean) * 100; //Coefficient of Variation
                        avg_weight = Math.Round(avg_weight, 3);
                        mean = Math.Round(mean, 3);
                        sd = Math.Round(sd, 3);
                        cv = Math.Round(cv, 3);
                    }
                    YCTestApercentSummaryModel ycTestApercentSummaryModel = new YCTestApercentSummaryModel()
                    {
                        ID = Guid.NewGuid(),
                        testID = ycTestApercentModelViewlist[0].testID,
                        userID = ycTestApercentModelViewlist[0].userID,
                        userName = ycTestApercentModelViewlist[0].userName,
                        process = ycTestApercentModelViewlist[0].process,
                        countsysname = ycTestApercentModelViewlist[0].countsysname,
                        yarnlenunit = ycTestApercentModelViewlist[0].yarnlenunit,
                        yarnlength = ycTestApercentModelViewlist[0].yarnlength,
                        shift = ycTestApercentModelViewlist[0].shift,
                        testType = ycTestApercentModelViewlist[0].testType,
                        totaltestcount = ycTestApercentModelViewlist[0].totaltestcount,
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
                            List<YCTestApercentCalculatedModel> apercentCalcList = conn.Table<YCTestApercentCalculatedModel>().Where(
                               YCTestApercentCalculatedModel => (YCTestApercentCalculatedModel.status == true)).ToList();
                            foreach (YCTestApercentCalculatedModel apercent in apercentCalcList)
                            {
                                apercent.status = false;
                                if (conn.Update(apercent) < 1)
                                {
                                    //to be decided if apercent calculated active records failed to deactive
                                }
                            }
                            YCTestApercentSummaryModel nMinus1Summary = conn.Table<YCTestApercentSummaryModel>().Where(
                                                            YCTestApercentSummaryModel => (
                                                            YCTestApercentSummaryModel.testType == "nMinus1" && YCTestApercentSummaryModel.status == true)
                                                            ).FirstOrDefault();
                            if (nMinus1Summary != null)
                            {
                                YCTestApercentSummaryModel NSummary = conn.Table<YCTestApercentSummaryModel>().Where(
                                                            YCTestApercentSummaryModel => (
                                                            YCTestApercentSummaryModel.testType == "N" && YCTestApercentSummaryModel.status == true)
                                                            ).FirstOrDefault();
                                if (NSummary != null)
                                {
                                    YCTestApercentSummaryModel nPlus1Summary = conn.Table<YCTestApercentSummaryModel>().Where(
                                                                YCTestApercentSummaryModel => (
                                                                YCTestApercentSummaryModel.testType == "nPlus1" && YCTestApercentSummaryModel.status == true)
                                                                ).FirstOrDefault();
                                    if (nPlus1Summary != null)
                                    {
                                        decimal apercent_nMinus1 = ((nMinus1Summary.testaverage - NSummary.testaverage) / nMinus1Summary.testaverage) * 100;
                                        apercent_nMinus1 = Math.Round(apercent_nMinus1, 3);
                                        decimal apercent_nPlus1 = ((nPlus1Summary.testaverage - NSummary.testaverage) / nPlus1Summary.testaverage) * 100;
                                        apercent_nPlus1 = Math.Round(apercent_nPlus1, 3);
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
                                            process = nMinus1Summary.process,
                                            countsysname = nMinus1Summary.countsysname,
                                            yarnlenunit = nMinus1Summary.yarnlenunit,
                                            yarnlength = nMinus1Summary.yarnlength,
                                            shift = nMinus1Summary.shift,
                                            testType = nMinus1Summary.testType,
                                            totaltestcount = nMinus1Summary.totaltestcount,
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
                        await refListView();
                        await refOverallSummary(mean, sd, cv);
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

                    if (currentTestType == "nPlus1")
                    {
                        entry_testcount.IsEnabled = true;
                        entry_testcount.Text = TESTCOUNT.ToString();
                        picker_shift.IsEnabled = true;
                        picker_shift.SelectedIndex = 0;
                        entry_process.Text = "";
                    }
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
        private async void startTestNm1Button_Clicked(object sender, EventArgs e)
        {

            isTestStarted = true;
            currentTestType = "nMinus1";
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
            if (picker_shift.SelectedIndex <= 0)
            {
                await DisplayAlert("Attention", "Please select shift!!!", "Ok");
                return;
            }
            if (entry_process.Text.Trim() == "")
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
                conn.CreateTable<YCTestApercentModel>();
                YCTestApercentModel lastTestRecord = conn.Table<YCTestApercentModel>().OrderByDescending(YCTestApercentModel => YCTestApercentModel.testID).FirstOrDefault();
                if (lastTestRecord != null)
                {
                    currentTestID = lastTestRecord.testID + 1;
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
            selectedYarnLen = int.Parse(lbl_yarnlen.Text);
            selectedTestCount = int.Parse(entry_testcount.Text);
            selectedShift = picker_shift.SelectedItem.ToString();
            selectedProcess = entry_process.Text;
            ycTestApercentModelViewlist = new List<YCTestApercentModelView>();
            startTestNm1Button.IsEnabled = false;
            startTestNm1Button.BackgroundColor = Color.SlateGray;
            entry_testcount.IsEnabled = false;
            picker_shift.IsEnabled = false;
            entry_process.IsEnabled = false;
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
                        YCTestApercentModelView ycTestApercentModelView = new YCTestApercentModelView()
                        {
                            testID = currentTestID,
                            userID = currentloggedInUser.ID,
                            userName = displayusername,
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
                                s_op = Math.Round(s_op, 3);
                                if (!initialWeigthCheck)
                                {
                                    if (s_op == ZERO)
                                    {
                                        initialWeigthCheck = true;
                                        ImageNotification("green.png");
                                        UpdateUserNotification("PLACE WEIGHT", GREEN);
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
                                        UpdateUserNotification("PLACE WEIGHT", GREEN);
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

        private async void startTestNButton_Clicked(object sender, EventArgs e)
        {
            isTestStarted = true;
            currentTestType = "N";
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
            if (picker_shift.SelectedIndex <= 0)
            {
                await DisplayAlert("Attention", "Please select shift!!!", "Ok");
                return;
            }
            if (entry_process.Text.Trim() == "")
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
                conn.CreateTable<YCTestApercentModel>();
                YCTestApercentModel lastTestRecord = conn.Table<YCTestApercentModel>().OrderByDescending(YCTestApercentModel => YCTestApercentModel.testID).FirstOrDefault();
                if (lastTestRecord != null)
                {
                    currentTestID = lastTestRecord.testID;
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
            selectedYarnLen = int.Parse(lbl_yarnlen.Text);
            selectedTestCount = int.Parse(entry_testcount.Text);
            selectedShift = picker_shift.SelectedItem.ToString();
            selectedProcess = entry_process.Text;
            ycTestApercentModelViewlist = new List<YCTestApercentModelView>();
            startTestNButton.IsEnabled = false;
            startTestNButton.BackgroundColor = Color.SlateGray;
            entry_testcount.IsEnabled = false;
            picker_shift.IsEnabled = false;
            entry_process.IsEnabled = false;
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
            if (picker_shift.SelectedIndex <= 0)
            {
                await DisplayAlert("Attention", "Please select shift!!!", "Ok");
                return;
            }
            if (entry_process.Text.Trim() == "")
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
                conn.CreateTable<YCTestApercentModel>();
                YCTestApercentModel lastTestRecord = conn.Table<YCTestApercentModel>().OrderByDescending(YCTestApercentModel => YCTestApercentModel.testID).FirstOrDefault();
                if (lastTestRecord != null)
                {
                    currentTestID = lastTestRecord.testID;
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
            selectedYarnLen = int.Parse(lbl_yarnlen.Text);
            selectedTestCount = int.Parse(entry_testcount.Text);
            selectedShift = picker_shift.SelectedItem.ToString();
            selectedProcess = entry_process.Text;
            ycTestApercentModelViewlist = new List<YCTestApercentModelView>();
            startTestNp1Button.IsEnabled = false;
            startTestNp1Button.BackgroundColor = Color.SlateGray;
            entry_testcount.IsEnabled = false;
            picker_shift.IsEnabled = false;
            entry_process.IsEnabled = false;
            CancellationTokenSource src = new CancellationTokenSource();
            CancellationToken ct = src.Token;
            ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
            await Task.Run(async () => await HandleTest(testCount), ct);
            src.Cancel();
        }
    }
}