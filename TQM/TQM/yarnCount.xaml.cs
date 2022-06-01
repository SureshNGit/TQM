using Android.Bluetooth;
using Java.IO;
using Java.Util;
using Plugin.BluetoothClassic.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class yarnCount : ContentPage
    {
        private CancellationTokenSource cs;
        private IBluetoothConnection connection;
        private BluetoothSocket _socket;
        BluetoothAdapter adapter;
        BluetoothDevice device;

        public yarnCount()
        {
            InitializeComponent();
        }

        private async void testYCButton_Clicked(object sender, EventArgs e)
        {


            //Check serial port communication
            //Check initial value is zero
            //Obtain the weight from weight scale via serial port

            bool blueState = await blueConnect();
            if (blueState)
            {
                string balOutput = await Listen();
                if (balOutput == null)
                {
                    _socket.Dispose();
                    device.Dispose();
                    adapter.Dispose();
                    await DisplayAlert("Error", "Unable to get weight from weighing machine!!!", "OK");
                    return;
                }
                else
                {
                    lbl_output.Text = "Stable Output is " + balOutput;
                    _socket.Dispose();
                    device.Dispose();
                    adapter.Dispose();
                }
                //int initialWeight = 0;
                //if (initialWeight == 0)
                //{
                //    string testCount_str = await DisplayPromptAsync("Enter Test Count", "", initialValue: "5", maxLength: 2, keyboard: Keyboard.Numeric);
                //    if (testCount_str == null || testCount_str == "")
                //    {
                //        await DisplayAlert("Alert", "Test count cannot be zero!!!", "OK");
                //        return;
                //    }
                //    int testCount = int.Parse(testCount_str);
                //    for (int i = 0; i < testCount; i++)
                //    {

                //    }
                //}
                //else
                //{
                //    await DisplayAlert("Alert", "Remove weigth to ensure zero!!!", "OK");
                //}
            }
            else
            {
                _socket.Dispose();
                device.Dispose();
                adapter.Dispose();
                await DisplayAlert("Alert", "Communication Error!!!", "OK");
            }
        }

        private void addMachine_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new machinePage());
        }

        private async Task<bool> blueConnect()
        {
            try
            {
                adapter = BluetoothAdapter.DefaultAdapter;

                if (adapter == null)
                    Debug.WriteLine("No Bluetooth adapter found.");

                if (!adapter.IsEnabled)
                    Debug.WriteLine("Bluetooth adapter is not enabled.");


                device = (from bd in adapter.BondedDevices
                          where bd.Name == "G85219651634"
                          select bd).FirstOrDefault();


                if (device == null)
                    Debug.WriteLine("Named device not found.");

                _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                await _socket.ConnectAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return false;
            }

        }


        private async Task<string> Listen()
        {
            String op = null;
            Stream inStream = _socket.InputStream;
            bool Listening = true;
            Debug.WriteLine("Listening has been started.");

            while (Listening)
            {
                try
                {
                    var mReader = new InputStreamReader(inStream);
                    var buffer = new BufferedReader(mReader);
                    int loopCount = 0;
                    int stableCount = 0;
                    while (true)
                    {
                        if (buffer.Ready())
                        {

                            op = await buffer.ReadLineAsync();
                            Debug.WriteLine("Output: " + op);
                            if (lbl_output.Text == op)
                            {
                                stableCount += 1;
                                if (stableCount > 10)
                                {
                                    return op;
                                }
                            }
                            lbl_output.Text = op;

                        }

                        if (loopCount > 50)
                        {
                            return op;
                        }
                        loopCount += 1;
                    }
                }
                catch (Java.IO.IOException e)
                {
                    Debug.WriteLine("Error: " + e.Message);
                    Listening = false;
                    return op;
                }

            }
            Debug.WriteLine("Listening has ended.");

            return op;
        }
    }
}