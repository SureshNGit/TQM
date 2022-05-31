using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothConfig : ContentPage
    {
        IBluetoothLE ble;
        IAdapter adapter;
        ObservableCollection<IDevice> deviceList;
        IDevice device;
        IService Service;
        IList<IService> Services;

        public BluetoothConfig()
        {
            InitializeComponent();
            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            deviceList = new ObservableCollection<IDevice>();
            lv.ItemsSource = deviceList;

        }

        private async void btnConnect_Clicked(object sender, System.EventArgs e)
        {

            try
            {
                if (device != null)
                {
                    await adapter.ConnectToDeviceAsync(device);
                }
                else
                {
                    await DisplayAlert("Notice", "No Device Selected!!!", "OK");
                }
            }
            catch (DeviceConnectionException ex)
            {
                await DisplayAlert("Notice", ex.Message.ToString(), "Error!");
            }

        }

        private void btnStatus_Clicked(object sender, System.EventArgs e)
        {
            var state = ble.State;
            this.DisplayAlert("Notice", state.ToString(), "OK");

        }

        private async void btnScan_Clicked(object sender, System.EventArgs e)
        {
            deviceList.Clear();
            adapter.DeviceDiscovered += (s, a) =>
            {
                deviceList.Add(a.Device);
            };
            if (!ble.Adapter.IsScanning)
            {
                await adapter.StartScanningForDevicesAsync();
            }
        }

        //private async void btnKnow_Clicked(object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        await adapter.ConnectToKnownDeviceAsync(new Guid("guid"));
        //    }
        //    catch (DeviceConnectionException ex)
        //    {
        //        await DisplayAlert("Notice", ex.Message.ToString(), "OK");
        //    }
        //}

        //private async void btnGetServices_Clicked(object sender, EventArgs e)
        //{
        //    Services = (IList<IService>)await device.GetServicesAsync();
        //    Service = (IService)await device.GetServicesAsync();
        //}

        private void lv_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (lv.SelectedItem == null)
            {
                return;
            }
            device = lv.SelectedItem as IDevice;
        }
    }
}