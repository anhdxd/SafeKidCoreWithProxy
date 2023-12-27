using IniParser.Model;
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
using IniParser;
using UISafekids.Source;

namespace UISafekids.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private FileIniDataParser iniFile = new FileIniDataParser();
        private static readonly string PATH_CONFIG_FILE = ".\\config.ini";

        public SettingsView()
        {
            InitializeComponent();
            SetupSeting();
        }

        private void SetupSeting()
        {
            // Setting
            IniData data = iniFile.ReadFile(PATH_CONFIG_FILE);
            Toggle_Adult.IsChecked = data["BlockConfig"]["BlockAdult"].ToString() == "1" ? true : false;
            Toggle_Social.IsChecked = data["BlockConfig"]["BlockSocial"].ToString() == "1" ? true : false;
            Toggle_Inprivate.IsChecked = data["BlockConfig"]["Inprivate"].ToString() == "1" ? true : false;
            Toggle_Game.IsChecked = data["BlockConfig"]["BlockGame"].ToString() == "1" ? true : false;
            Toggle_SafeSearch.IsChecked = data["BlockConfig"]["Safe"].ToString() == "1" ? true : false;
        }
        private void Adult_Setting_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                //var iniFile = new FileIniDataParser();
                IniData data = iniFile.ReadFile(PATH_CONFIG_FILE);
                if (Toggle_Adult.IsChecked == true)
                {
                    data["BlockConfig"]["BlockAdult"] = "1";
                }
                else
                {
                    data["BlockConfig"]["BlockAdult"] = "0";
                }
                iniFile.WriteFile(PATH_CONFIG_FILE, data);
                PipeClient.SendRequestChangeSetting();
            }
            catch (Exception)
            {
                return;
            }

        }
        // Social----------------------------------------------------
        private void Social_Setting_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                //var iniFile = new FileIniDataParser();
                IniData data = iniFile.ReadFile(PATH_CONFIG_FILE);
                data["BlockConfig"]["BlockSocial"] = Toggle_Social.IsChecked == true ? "1" : "0";
                iniFile.WriteFile(PATH_CONFIG_FILE, data);
                PipeClient.SendRequestChangeSetting();
            }
            catch (Exception)
            {
                return;
            }
        }
        // Game-------------------------------------------------------
        private void Game_Setting_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                //var iniFile = new FileIniDataParser();
                IniData data = iniFile.ReadFile(PATH_CONFIG_FILE);
                data["BlockConfig"]["BlockGame"] = Toggle_Game.IsChecked == true ? "1" : "0";
                iniFile.WriteFile(PATH_CONFIG_FILE, data);
                PipeClient.SendRequestChangeSetting();
            }
            catch (Exception)
            {
                return;
            }
        }
        //Safe Search-------------------------------------------------------
        private void SafeSearch_Setting_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                //var iniFile = new FileIniDataParser();
                IniData data = iniFile.ReadFile(PATH_CONFIG_FILE);
                data["BlockConfig"]["Safe"] = Toggle_SafeSearch.IsChecked == true ? "1" : "0";
                iniFile.WriteFile(PATH_CONFIG_FILE, data);
                PipeClient.SendRequestChangeSetting();
            }
            catch (Exception)
            {
                return;
            }

        }
        //Inprivate ---------------------------------------------------------------------
        private void Inprivate_Setting_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                //var iniFile = new FileIniDataParser();
                IniData data = iniFile.ReadFile(PATH_CONFIG_FILE);
                data["BlockConfig"]["Inprivate"] = Toggle_Inprivate.IsChecked == true ? "1" : "0";
                iniFile.WriteFile(PATH_CONFIG_FILE, data);
                PipeClient.SendRequestChangeSetting();
            }
            catch (Exception)
            {
                return;
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
