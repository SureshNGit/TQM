using Android.Bluetooth;
using Java.IO;
using Java.Util;
using Plugin.BluetoothClassic.Abstractions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class yarnCount : ContentPage, INotifyPropertyChanged
    {
        private CancellationTokenSource cs;
        private IBluetoothConnection connection;
        private BluetoothSocket _socket;
        BluetoothAdapter adapter;
        BluetoothDevice device;
        const decimal MIN_VAL = 0.0m;
        const decimal ZERO = 0.0m;
        private string userMsg;

        public string UserMsg
        {
            get
            {
                return userMsg;
            }
            set
            {
                if (userMsg != value)
                {
                    userMsg = value;
                    this.OnPropertyChanged("UserMsg");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            };
        }

        private async void updateLabelText(string msg)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lbl_error.Text = msg;
            });
        }

        private async void updateOutputLabelText(string msg)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lbl_output.Text = msg;
                testYCButton.IsEnabled = true;
                testYCButton.BackgroundColor = Color.Green;
            });
        }

        public yarnCount()
        {

            InitializeComponent();
            if (!initializeBluetooth())
            {
                _socket.Dispose();
                device.Dispose();
                adapter.Dispose();
                DisplayAlert("Alert", "Communication Error!!!", "OK");
            }


        }




        private async void testYCButton_Clicked(object sender, EventArgs e)
        {
            testYCButton.IsEnabled = false;
            testYCButton.BackgroundColor = Color.SlateGray;
            CancellationTokenSource src = new CancellationTokenSource();
            CancellationToken ct = src.Token;
            ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
            await Task.Run(async () => await runTest(entry_testcount.Text), ct);
            src.Cancel();
        }



        private async Task runTest(string testCount_str)
        {
            if (testCount_str == null || testCount_str == "")
            {
                updateLabelText("Test count cannot be zero!!!");
                return;
            }
            int testCount = int.Parse(testCount_str);
            CancellationTokenSource src = new CancellationTokenSource();
            CancellationToken ct = src.Token;
            ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
            bool blueState = Task.Run(async () => await blueConnect(), ct).Result;
            src.Cancel();
            if (blueState)
            {


                for (int i = 0; i < testCount; i++)
                {
                    lbl_output.Text = "";
                    bool testStatus = true;
                    bool initialWeigthCheck = false;
                    int perTestLoopCount = 0;
                    while (testStatus)
                    {
                        CancellationTokenSource src_1 = new CancellationTokenSource();
                        CancellationToken ct_1 = src_1.Token;
                        ct_1.Register(() => Debug.WriteLine("ConnectBluetoothToken-1"));
                        blueState = Task.Run(async () => await blueConnect(), ct_1).Result;
                        src_1.Cancel();
                        if (!blueState)
                        {
                            disposeble();
                            await DisplayAlert("Alert", "Communication Error!!!", "OK");
                            break;
                        }


                        String balOutput = Listen();
                        Debug.WriteLine("Recieved from Bluetooth adapter is [" + balOutput + "]");
                        if (balOutput != "")
                        {
                            if (balOutput == "reset")
                            {
                                updateLabelText("Remove weigth to ensure zero!!!");
                                Debug.WriteLine("Remove weigth to ensure zero!!!");
                                //lbl_error.Text = "Remove weigth to ensure zero!!!";
                                //lbl_error.TextColor = Color.Red;
                                disposeble();
                                //break;
                            }
                            else if (balOutput == "fail")
                            {
                                updateLabelText("Read data failed!!!");
                                Debug.WriteLine("Read data failed");
                                //lbl_error.Text = "Read data failed";
                                //lbl_error.TextColor = Color.Red;
                                disposeble();
                                return;
                            }
                            else
                            {
                                decimal s_op = decimal.Parse(balOutput);
                                if (!initialWeigthCheck)
                                {
                                    if (s_op == ZERO || s_op <= MIN_VAL)
                                    {
                                        initialWeigthCheck = true;
                                        updateLabelText("Place object to start test!!!");
                                        Debug.WriteLine("Place object to start test!!!");
                                        //lbl_error.Text = "Place object to start test!!!";
                                        //lbl_error.TextColor = Color.Green;
                                        disposeble();

                                    }
                                    else
                                    {
                                        updateLabelText("Remove weigth to ensure zero!!!");
                                        Debug.WriteLine("Remove weigth to ensure zero!!!");
                                        //lbl_error.Text = "Remove weigth to ensure zero!!!";
                                        //lbl_error.TextColor = Color.Red;
                                        disposeble();
                                        //break;
                                    }
                                }
                                else
                                {
                                    if (s_op == ZERO)
                                    {
                                        updateLabelText("Place object to start test!!!");
                                        Debug.WriteLine("Place object to start test!!!");
                                        //lbl_error.Text = "Place object to start test!!!";
                                        //lbl_error.TextColor = Color.Green;
                                        disposeble();
                                    }
                                    else
                                    {
                                        //lbl_output.Text = "Stable value : " + balOutput;
                                        updateOutputLabelText("Stable value : " + balOutput);
                                        updateLabelText("");
                                        disposeble();
                                        break;
                                    }
                                }

                            }
                        }
                        else
                        {
                            updateLabelText("Data is unstable!!! Ensure weighing machine is covered properly");
                            Debug.WriteLine("Data is unstable!!! Ensure weighing machine is covered properly");
                            //lbl_error.Text = "Data is unstable!!! Ensure weighing machine is covered properly";
                            //lbl_error.TextColor = Color.Red;
                            disposeble();
                            //break;
                        }
                        if (perTestLoopCount > 25)
                        {
                            updateLabelText("Improper Test!!! Start new test");
                            Debug.WriteLine("Improper Test!!! Start new test");
                            //lbl_error.Text = "Improper Test!!! Start new test";
                            //lbl_error.TextColor = Color.Red;
                            disposeble();
                            return;
                        }
                        perTestLoopCount += 1;
                    }

                }
            }
            else
            {
                disposeble();
                updateLabelText("Communication Error!!!");
                //lbl_error.Text = "Communication Error!!!";
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

        private void addMachine_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new machinePage());
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
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return false;
            }
        }

        private async Task<bool> blueConnect()
        {
            try
            {
                try
                {
                    if (!_socket.IsConnected)
                    {
                        await _socket.ConnectAsync();
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                    if (!initializeBluetooth()) { return false; } else { await _socket.ConnectAsync(); return true; }
                }
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

        private string Listen()
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
                                if (op.Contains("-"))
                                {
                                    return "reset";

                                }
                                else
                                {
                                    decimal op_dec = decimal.Parse(op);
                                    Debug.WriteLine("Output Modifed to int: [" + op_dec + "]: greater than 0?: " + (op_dec > 0));

                                }


                                if (prevOp == op)
                                {
                                    stableCount += 1;
                                    if (stableCount > 15)
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

                                if (loopCount > 50)
                                {
                                    return "";
                                }
                                loopCount += 1;
                            }



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
            Debug.WriteLine("Listening has ended.");

            return op;
        }
    }
}