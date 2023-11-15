using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace KMASafeKidCore
{
    class ConfigInApp
    {
        public static string CONFIG_NAME = "config.ini";
        private static string PATH_CONFIG_FILE = System.IO.Directory.GetCurrentDirectory() + "\\" + CONFIG_NAME;
        //CHANGE SETTING
        public static bool bConfigChange = false;
        // Web Config
        public static bool bSafeSearch = false; 
        public static bool bInPrivateBlock = false;
        public static bool bBlockDomain = false;
        public static bool bBlockAdult = false;
        public static bool bBlockSocial = false;
        public static bool bBlockGame = false;
        // App Config
        public static bool bDiaryApp = false;
        public static bool bAppInstall = false;

        public static readonly string[] arrWebConfig = { "Safe", "Inprivate", "BlockAdult", "BlockSocial", "BlockGame" };
        public static bool ReadConfigFile()
        {
            var iniFile = new FileIniDataParser();
            if (!System.IO.File.Exists(PATH_CONFIG_FILE))
            {
                System.IO.File.Create(PATH_CONFIG_FILE).Close();
              
                IniData data = new IniData();
                // App 
                data["BlockConfig"]["BlockGame"] = "1";
                data["BlockConfig"]["DiaryApp"] = "1";
                // Web
                data["BlockConfig"]["BlockDomain"] = "1";
                data["BlockConfig"]["Safe"] = "1";
                data["BlockConfig"]["Inprivate"] = "1";
                data["BlockConfig"]["BlockAdult"] = "1";
                data["BlockConfig"]["BlockSocial"] = "1";
                iniFile.WriteFile(PATH_CONFIG_FILE, data);
            }
            IniData dataINI = iniFile.ReadFile(PATH_CONFIG_FILE);
            // App Config
            bBlockGame = (dataINI["BlockConfig"]["BlockGame"] == "1") ? true : false;
            bDiaryApp = (dataINI["BlockConfig"]["DiaryApp"] == "1") ? true : false;

            // Web Config
            bBlockDomain = (dataINI["BlockConfig"]["BlockDomain"] == "1") ? true : false;
            bSafeSearch = (dataINI["BlockConfig"]["Safe"] == "1") ? true : false;
            bInPrivateBlock = (dataINI["BlockConfig"]["Inprivate"] == "1") ? true : false;
            bBlockAdult = (dataINI["BlockConfig"]["BlockAdult"] == "1") ? true : false;
            bBlockSocial = (dataINI["BlockConfig"]["BlockSocial"] == "1") ? true : false;
            return true;
        }
        public static bool ChangeConfig(string name, string value)
        {
            try
            {
                var iniFile = new FileIniDataParser();
                IniData data = iniFile.ReadFile(PATH_CONFIG_FILE);
                data["BlockConfig"][name] = value;
                iniFile.WriteFile(PATH_CONFIG_FILE, data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool ImportConfigFromJson(string config)
        {
            var JData = JArray.Parse(config);
            var iniFile = new FileIniDataParser();
            IniData data = iniFile.ReadFile(PATH_CONFIG_FILE);
 
            iniFile.WriteFile(PATH_CONFIG_FILE, data);
            foreach (var item in arrWebConfig)
            {
                if (JData[item] != null)
                {
                    data["BlockConfig"][item] = JData[item].ToString();
                }
            }
            ConfigInApp.ReadConfigFile();
            return true;
        }


    }
}
