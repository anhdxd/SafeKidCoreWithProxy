using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UISafekids.Models;

namespace UISafekids.ViewModels
{
    class DeviceConnectViewModel : ViewModelBase
    {
        private ObservableCollection<DeviceConnectModel> _deviceConnectModel { get; set; }

        //public ObservableCollection<DetailModel> DetailModel { get; set; }

        //Properties
        public ObservableCollection<DeviceConnectModel> DeviceConnectModel
        {
            get
            {
                return _deviceConnectModel;
            }
            set
            {
                _deviceConnectModel = value;
                OnPropertyChanged(nameof(DeviceConnectModel));
            }
        }
        public DeviceConnectViewModel()
        {
            DeviceConnectModel = new ObservableCollection<DeviceConnectModel>();

            DeviceConnectModel.Add(new DeviceConnectModel { ID = "1", DeviceName = "ASUS", DeviceType = "Android" });
            DeviceConnectModel.Add(new DeviceConnectModel { ID = "2", DeviceName = "LENOVO", DeviceType = "Android" });
            DeviceConnectModel.Add(new DeviceConnectModel { ID = "3", DeviceName = "SAMSUNG", DeviceType = "Android" });
            DeviceConnectModel.Add(new DeviceConnectModel { ID = "4", DeviceName = "IPHONE", DeviceType = "IOS" });
        }
    }
}
