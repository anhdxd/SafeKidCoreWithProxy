using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UISafekids.Models
{
    public class DetailModel
    {
        //Properties
        public string ID { get; set; }
        public string AppName { get; set; }
        public string TimeStart { get; set; }
        public string TimeUsed { get; set; }
    }
    public class AppInstalledModel
    {
        public string ID { get; set; }
        public string AppName { get; set; }
        public string TimeInstall { get; set; }
    }

    public class CustomBlockModel
    {
        public string ID { get; set; }
        public string AppName { get; set; }
        public string AppDetail { get; set; }
        public string TimeBlock { get; set; }
        public string TypeApp { get; set; }
    }

    public class SettingModel
    {
        public string BlockAdult { get; set; }
        public string BlockSocial { get; set; }
        public string BlockGame { get; set; }
        public string BlockReco { get; set; }
    }

    public class DeviceConnectModel
    {
        public string ID { get; set;}
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
    }
}
