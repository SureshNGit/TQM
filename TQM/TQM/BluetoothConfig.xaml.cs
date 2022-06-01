using Plugin.BluetoothClassic.Abstractions;
using System;
using System.Diagnostics;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothConfig : ContentPage
    {
        Memory<byte> BufferSize;
        private CancellationTokenSource cs;
        private IBluetoothConnection connection;

        public BluetoothConfig()
        {
            InitializeComponent();

            FillDevices();
        }

        private void FillDevices()
        {
            var adapter = DependencyService.Resolve<IBluetoothAdapter>();
            lv_devices.ItemsSource = adapter.BondedDevices;
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

            var buffer = new byte[8192];
            BufferSize = new Memory<byte>(buffer);
            await connection.ConnectAsync();
            cs = new CancellationTokenSource();
            if (connection.DataAvailable)
            {
                int response = await connection.ReciveAsync(BufferSize, cs.Token);
                Debug.WriteLine("Byes Read $$$$$$$$$$$$$$$$$" + response);
                foreach (char c in BufferSize)
                {

                }
            }
        }




    }
}