using Android.Bluetooth;
using Java.IO;
using Java.Util;
using Plugin.BluetoothClassic.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothConfig : ContentPage
    {
        //Memory<byte> BufferSize;

        private CancellationTokenSource cs;
        private IBluetoothConnection connection;
        private BluetoothSocket _socket;

        [System.Obsolete]
        public BluetoothConfig()
        {
            InitializeComponent();
            //lv_devices.ItemsSource = outputList;
            bleConnect();

            //FillDevices();
        }
        [System.Obsolete]
        private async void bleConnect()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

            if (adapter == null)
                Debug.WriteLine("No Bluetooth adapter found.");

            if (!adapter.IsEnabled)
                Debug.WriteLine("Bluetooth adapter is not enabled.");


            BluetoothDevice device = (from bd in adapter.BondedDevices
                                      where bd.Name == "G85219651634"
                                      select bd).FirstOrDefault();

            if (device == null)
                Debug.WriteLine("Named device not found.");

            _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));

            await _socket.ConnectAsync();
            Listen(_socket.InputStream);
        }


        private async void Listen(Stream inStream)
        {
            bool Listening = true;
            Debug.WriteLine("Listening has been started.");
            //byte[] uintBuffer = new byte[sizeof(uint)]; // This reads the first 4 bytes which form an uint that indicates the length of the string message.
            //byte[] textBuffer; // This will contain the string message.
            //var readLength = 0;
            // Keep listening to the InputStream while connected.

            while (Listening)
            {
                try
                {
                    //textBuffer = new byte[256];
                    //int read = await inStream.ReadAsync(textBuffer, 0, textBuffer.Length - readLength);
                    //readLength = readLength + read;
                    //string s = Encoding.UTF8.GetString(textBuffer);
                    //Debug.WriteLine("Received: " + s.ToString());

                    var mReader = new InputStreamReader(inStream);
                    var buffer = new BufferedReader(mReader);
                    //string output = "";
                    while (true)
                    {
                        if (buffer.Ready())
                        {

                            String op = await buffer.ReadLineAsync();
                            Debug.WriteLine("Output: " + op);
                            lbl_output.Text = op;
                            //char[] chr = new char[256];
                            //foreach (char c in chr)
                            //{

                            //    if (c == '\0')
                            //        break;
                            //    output += c;
                            //}

                        }
                        //Debug.WriteLine("Output: " + output);
                    }
                }
                catch (Java.IO.IOException e)
                {
                    Debug.WriteLine("Error: " + e.Message);
                    Listening = false;
                    break;
                }
            }
            Debug.WriteLine("Listening has ended.");
        }

        private void FillDevices()
        {
            var adapter = DependencyService.Resolve<IBluetoothAdapter>();
            //lv_devices.ItemsSource = adapter.BondedDevices;

        }

        private void lv_devices_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            BluetoothDeviceModel device = (BluetoothDeviceModel)e.SelectedItem;
            if (device != null)
            {
                var _bluetoothAdapter = DependencyService.Resolve<IBluetoothAdapter>();
                connection = _bluetoothAdapter.CreateConnection(device);

                Debug.WriteLine("Connected to $$$$$$$$$$$$$$$$$" + device.Name);
                read();
            }
        }

        private async void read()
        {

            byte[] buffer = new byte[256];
            //BufferSize = new Memory<byte>(buffer);
            await connection.ConnectAsync();
            cs = new CancellationTokenSource();
            //if (connection.DataAvailable)
            //{
            //    int response = await connection.ReciveAsync(buffer, cs.Token);
            //    Debug.WriteLine("Byes Read $$$$$$$$$$$$$$$$$" + response);
            //    foreach (byte b in buffer)
            //    {
            //        Debug.WriteLine("Bytes..........>" + b.ToString());
            //    }
            //}

            while (connection.DataAvailable)
            {
                int response = await connection.ReciveAsync(buffer, cs.Token);
                Debug.WriteLine("Bytes..........>" + buffer.GetValue(0).ToString());
            }
        }




    }
}