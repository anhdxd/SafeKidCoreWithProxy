using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
using UISafekids.Models;
using UISafekids.Source;

namespace UISafekids.Views
{
    /// <summary>
    /// Interaction logic for CustomBlockView.xaml
    /// </summary>
    public partial class CustomBlockView : UserControl
    {
        ModifySQlite SQlite = new ModifySQlite();
        private List<DataBlock> WebBlock = new List<DataBlock>();
        private List<DataBlock> AppBlock = new List<DataBlock>();

        public CustomBlockView()
        {
            InitializeComponent();
            SQlite.ConnectToDB(ModifySQlite.PATH_DB);
        }

        private void Button_Click_AddWeb(object sender, RoutedEventArgs e)
        {
            
            string InputText = txtAddWeb.Text;
            if (Uri.IsWellFormedUriString(InputText, UriKind.Absolute))
                InputText = new Uri(InputText).Host;

            if (InputText.StartsWith("www."))
                InputText = InputText.Substring("www.".Length);

            JObject jSendToSV = new JObject();
            jSendToSV["flag"] = (int)PipeClient.fText.AddHostToDB;
            jSendToSV["sDomain"] = InputText;
            PipeClient.SendRequestToServer(jSendToSV.ToString());

            System.Threading.Thread.Sleep(50);
            SortedSet<string> ssUserHost = SQlite.CGetAllBlockRule(ModifySQlite.ObjectType.WEB);//AES.DecryptFileToSortedSet(".\\DBB\\UDB.dat");
            WebBlock.Clear();
            foreach (var item in ssUserHost)
            {
                WebBlock.Add(new DataBlock { WebName = item });
            }
            //dgBlockShow.ItemsSource = WebBlock;
            //dgBlockShow.Items.Refresh();
        }

        private void Button_Click_AddApp(object sender, RoutedEventArgs e)
        {
            JObject jSendToSV = new JObject();
            jSendToSV["flag"] = (int)PipeClient.fText.AddAppToDB;
            jSendToSV["sPath"] = txtAddApp.Text;

            PipeClient.SendRequestToServer(jSendToSV.ToString());

            System.Threading.Thread.Sleep(50);
            SortedSet<string> ssUserApp = SQlite.CGetAllBlockRule(ModifySQlite.ObjectType.APP);//AES.DecryptFileToSortedSet(".\\DBB\\UADB.dat");
            AppBlock.Clear();
            foreach (var item in ssUserApp)
            {
                AppBlock.Add(new DataBlock { AppName = item });
            }
            //dgAppShow.ItemsSource = AppBlock;
            //dgAppShow.Items.Refresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.Filter = "Execute (*.exe)|*.exe|All files (*.*)|*.*";

            Nullable<bool> result = openFileDlg.ShowDialog();

            if (result == true)
            {
                txtAddApp.Text = openFileDlg.FileName;
                txtAddApp.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF000000");
            }
        }
    }
    public class DataBlock
    {
        public string WebName { get; set; }
        public string AppName { get; set; }
        public string PathApp { get; set; }
    }

}
