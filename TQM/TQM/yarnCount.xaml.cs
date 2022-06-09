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
        const decimal MIN_VAL = 0.400m;
        const decimal ZERO = 0.0m;
        const int PER_TEST_LOOP_COUNT = 100;
        const int DATA_READ_LOOP_COUNT = 100;
        const int STABLE_DATA_CHECK = 25;
        private decimal current_stable_data = 0;
        private List<YCTestModelView> ycTestModelViewlist;
        private long currentTestID = 0;
        private UserModel currentloggedInUser = null;
        private string selectedMachineCategory = null;
        private Guid selectedMachineID = Guid.Empty;
        private string selectedMachineName = null;
        private string selectedSysName = null;
        private string selectedCountUnit = null;
        private decimal selectedYarnLen = 0m;
        private int selectedTestCount = 0;
        private string selectedApercent = null;
        private const string RED = "#E74C3C";
        private const string GREEN = "#3CE74C";

        public yarnCount()
        {
            InitializeComponent();
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<YarnCountConfigModel>();
                YarnCountConfigModel yarncountconfigmodel = conn.Table<YarnCountConfigModel>().FirstOrDefault();
                if (yarncountconfigmodel != null)
                {
                    lbl_countsysname.Text = yarncountconfigmodel.countsysname;
                    lbl_yarncountunit.Text = yarncountconfigmodel.yarnlenunit;
                    lbl_yarnlen.Text = yarncountconfigmodel.yarnlength.ToString();
                    entry_testcount.Text = yarncountconfigmodel.testcount.ToString();
                }
                else
                {
                    lbl_countsysname.Text = "";
                    lbl_yarncountunit.Text = "";
                    lbl_yarnlen.Text = "";
                    entry_testcount.Text = "";
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

        private async Task refListView(bool visibility = true)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                listview_testresult.ItemsSource = null;
                listview_testresult.IsVisible = visibility;
                listview_testresult.ItemsSource = ycTestModelViewlist;
            });
        }

        private async Task refOverallSummary(decimal mean, decimal sd, decimal cv, bool visibility = true)
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
                        apercent = test.apercent,
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
                }
                if (dbStatus)
                {
                    decimal mean = 0m;
                    decimal sd = 0m;
                    decimal cv = 0m;
                    if (ycTestModelViewlist[0].totaltestcount > 1)
                    {
                        mean = totalCalcCountVal / ycTestModelViewlist[0].totaltestcount;
                        mean = Math.Round(mean, 3);
                        decimal IndividualCalValminusMean = 0m;
                        foreach (YCTestModelView test in ycTestModelViewlist)
                        {
                            IndividualCalValminusMean = IndividualCalValminusMean + ((test.yccalcval - mean) * (test.yccalcval - mean));
                        }
                        sd = (decimal)Math.Sqrt((double)IndividualCalValminusMean / (double)(ycTestModelViewlist[0].totaltestcount - 1));//Standard Deviation
                        sd = Math.Round(sd, 3);
                        cv = (sd / mean) * 100; //Coefficient of Variation
                        cv = Math.Round(cv, 3);
                    }
                    YCTestSummaryModel ycTestSummaryModel = new YCTestSummaryModel()
                    {
                        ID = Guid.NewGuid(),
                        testID = ycTestModelViewlist[0].testID,
                        userID = ycTestModelViewlist[0].userID,
                        userName = ycTestModelViewlist[0].userName,
                        machineID = ycTestModelViewlist[0].machineID,
                        machineCategory = ycTestModelViewlist[0].machineCategory,
                        machineName = ycTestModelViewlist[0].machineName,
                        countsysname = ycTestModelViewlist[0].countsysname,
                        yarnlenunit = ycTestModelViewlist[0].yarnlenunit,
                        yarnlength = ycTestModelViewlist[0].yarnlength,
                        totaltestcount = ycTestModelViewlist[0].totaltestcount,
                        testaverage = mean,
                        testsd = sd,
                        testcv = cv,
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
                        await refListView();
                        await refOverallSummary(mean, sd, cv);
                    }
                }

            }
        }

        private void reset(bool fullreset = true)
        {
            try
            {
                current_stable_data = 0;
                if (fullreset) { ImageNotification(null); UpdateUserNotification(""); disposeble(); }
                Device.BeginInvokeOnMainThread(() =>
                {
                    testYCButton.IsEnabled = true;
                    testYCButton.BackgroundColor = Color.Green;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async void testYCButton_Clicked(object sender, EventArgs e)
        {
            ImageNotification("null");
            UpdateUserNotification("");
            await refListView(false);
            await refOverallSummary(0m, 0m, 0m, false);
            if (selectedMachineID == Guid.Empty || selectedMachineCategory == null)
            {
                await DisplayAlert("Attention", "Please select machine category/ name to proceed!!!", "Ok");
                return;
            }
            if (!initializeBluetooth())
            {
                ImageNotification("red.png");
                UpdateUserNotification("Communication Error!!!");
                return;
            }
            string testCount_str = entry_testcount.Text;
            int testCount = int.Parse(testCount_str);
            if (testCount_str == null || testCount_str == "")
            {
                ImageNotification("red.png");
                UpdateUserNotification("Test count cannot be zero!!!");
                return;
            }
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<YCTestModel>();
                YCTestModel lastTestRecord = conn.Table<YCTestModel>().OrderByDescending(YCTestModel => YCTestModel.testID).FirstOrDefault();
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
            selectedApercent = entry_apercent.Text.Trim();
            ycTestModelViewlist = new List<YCTestModelView>();
            testYCButton.IsEnabled = false;
            testYCButton.BackgroundColor = Color.SlateGray;
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
                            apercent = selectedApercent,
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
                if (ycTestModelViewlist.Count > 0) { updateDB(); }
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
                showAlert("Communication Error. Please start test from begining!!!");
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
                            if (balOutput == "reset")
                            {
                                ImageNotification("red.png");
                                UpdateUserNotification("Remove weigth to ensure zero!!!");
                                Debug.WriteLine("Remove weigth to ensure zero!!!");
                            }
                            else if (balOutput == "fail")
                            {
                                ImageNotification("red.png");
                                UpdateUserNotification("Read data failed!!!");
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
                                        UpdateUserNotification("Place object to start test!!!", GREEN);
                                        Debug.WriteLine("Place object to start test!!!");
                                    }
                                    else
                                    {
                                        ImageNotification("red.png");
                                        UpdateUserNotification("Remove weigth to ensure zero!!!");
                                        Debug.WriteLine("Remove weigth to ensure zero!!!");
                                    }
                                }
                                else
                                {
                                    if (s_op == ZERO)
                                    {
                                        ImageNotification("green.png");
                                        UpdateUserNotification("Place object to start test!!!", GREEN);
                                        Debug.WriteLine("Place object to start test!!!");
                                    }
                                    else if (s_op < MIN_VAL)
                                    {
                                        initialWeigthCheck = false;
                                        ImageNotification("red.png");
                                        UpdateUserNotification("Weigth is below minimum value!!!");
                                        Debug.WriteLine("Weigth is below minimum value!!!");
                                    }
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
                            UpdateUserNotification("Data is unstable!!! Ensure weighing machine is covered properly");
                            Debug.WriteLine("Data is unstable!!! Ensure weighing machine is covered properly");
                        }
                        if (perTestLoopCount > PER_TEST_LOOP_COUNT)
                        {
                            ImageNotification("red.png");
                            UpdateUserNotification("Improper Test!!! Start new test");
                            Debug.WriteLine("Improper Test!!! Start new test");
                            return false;
                        }
                        perTestLoopCount += 1;
                    }
                }
                else
                {
                    ImageNotification("red.png");
                    UpdateUserNotification("Communication Error!!!");
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
                            if (bufferfailedcount > 100)
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
            }
        }

        private void picker_machinename_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<MachineModel> source = (List<MachineModel>)picker_machinename.ItemsSource;
            selectedMachineID = (Guid)source[picker_machinename.SelectedIndex].ID;
            MachineModel selectedMachine = (MachineModel)picker_machinename.SelectedItem;
            selectedMachineName = selectedMachine.machineName;
        }


    }
}