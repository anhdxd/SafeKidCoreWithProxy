using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UISafekids.Views
{
    /// <summary>
    /// Interaction logic for DeviceConnectView.xaml
    /// </summary>
    public partial class DeviceConnectView : UserControl
    {
        public string? Image64 { get; set; } = "";
        public DeviceConnectView()
        {
            InitializeComponent();

            ////Image64 = File.ReadAllText(@"D:\OneDrive - danganhcompany\1.BIG_WORK_AT160104\0.DO_AN\UISafekids\UISafekids\Images\qr.txt");
            //// Read file txt get all data

            //byte[] binaryData = Convert.FromBase64String(Image64);

            //BitmapImage bi = new BitmapImage();
            //bi.BeginInit();
            //bi.StreamSource = new MemoryStream(binaryData);
            //bi.EndInit();

            //ImageQr.Source = bi;

        }

    }
}
