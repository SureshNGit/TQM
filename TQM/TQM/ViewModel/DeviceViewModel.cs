using Plugin.BLE.Abstractions.Contracts;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TQM.ViewModel
{
    internal class DeviceViewModel : INotifyPropertyChanged
    {
        private IDevice _nativeDevice;
        public event PropertyChangedEventHandler PropertyChanged;


        public IDevice NativeDevice
        {
            get { return _nativeDevice; }
            set { _nativeDevice = value; RaisePropertyChnaged(); }
        }

        protected void RaisePropertyChnaged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

    }
}
