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
        const decimal MIN_VAL = 0.0m;
        const decimal ZERO = 0.0m;
        const int PER_TEST_LOOP_COUNT = 100;
        const int DATA_READ_LOOP_COUNT = 100;
        const int STABLE_DATA_CHECK = 25;
        private decimal current_stable_data = 0;
        private List<YCTestModelView> ycTestModelViewlist;
        private long currentTestID = 0;
        private UserModel currentloggedInUser = null;

        public yarnCount()
        {
            InitializeComponent();
        }

        private async void UpdateUserNotification(string msg)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lbl_error.Text = msg;
            });
        }

        private async void showAlert(string msg)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("Notice", msg, "Ok");
            });
        }

        private void reset(bool fullreset = true)
        {
            try
            {
                current_stable_data = 0;
                if (fullreset) { UpdateUserNotification(""); disposeble(); }
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
            UpdateUserNotification("");
            if (!initializeBluetooth())
            {
                UpdateUserNotification("Communication Error!!!");
                return;
            }
            string testCount_str = entry_testcount.Text;
            int testCount = int.Parse(testCount_str);
            if (testCount_str == null || testCount_str == "")
            {
                UpdateUserNotification("Test count cannot be zero!!!");
                return;
            }
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<YCTestModel>();
                YCTestModel lastTestRecord = conn.Table<YCTestModel>().Last<YCTestModel>();
                if (lastTestRecord != null)
                {
                    currentTestID = lastTestRecord.testID + 1;
                }
                else
                {
                    currentTestID = 0;
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
                        YCTestModelView ycTestModelView = new YCTestModelView()
                        {
                            testID = currentTestID,
                            userID = currentloggedInUser.ID,
                            userName = displayusername,
                        };
                        showAlert("Test - [" + (i + 1) + "] Completed!!! [" + current_stable_data + "]");
                    }
                    else
                    {
                        showAlert("Test - [" + (i + 1) + "] Failed!!! Please start test from begining!!!");
                        reset(false);
                        break;
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
                                UpdateUserNotification("Remove weigth to ensure zero!!!");
                                Debug.WriteLine("Remove weigth to ensure zero!!!");
                            }
                            else if (balOutput == "fail")
                            {
                                UpdateUserNotification("Read data failed!!!");
                                Debug.WriteLine("Read data failed");
                                return false;
                            }
                            else
                            {
                                decimal s_op = decimal.Parse(balOutput);
                                if (!initialWeigthCheck)
                                {
                                    if (s_op == ZERO || s_op <= MIN_VAL)
                                    {
                                        initialWeigthCheck = true;
                                        UpdateUserNotification("Place object to start test!!!");
                                        Debug.WriteLine("Place object to start test!!!");
                                    }
                                    else
                                    {
                                        UpdateUserNotification("Remove weigth to ensure zero!!!");
                                        Debug.WriteLine("Remove weigth to ensure zero!!!");
                                    }
                                }
                                else
                                {
                                    if (s_op == ZERO)
                                    {
                                        UpdateUserNotification("Place object to start test!!!");
                                        Debug.WriteLine("Place object to start test!!!");
                                    }
                                    else
                                    {
                                        current_stable_data = s_op;
                                        UpdateUserNotification("");
                                        return true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            UpdateUserNotification("Data is unstable!!! Ensure weighing machine is covered properly");
                            Debug.WriteLine("Data is unstable!!! Ensure weighing machine is covered properly");
                        }
                        if (perTestLoopCount > PER_TEST_LOOP_COUNT)
                        {
                            UpdateUserNotification("Improper Test!!! Start new test");
                            Debug.WriteLine("Improper Test!!! Start new test");
                            return false;
                        }
                        perTestLoopCount += 1;
                    }
                }
                else
                {
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

        }
    }
}