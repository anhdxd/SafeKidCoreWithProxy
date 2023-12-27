using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UISafekids.Models;

namespace UISafekids.ViewModels
{
    class AppInstalledViewModel : ViewModelBase
    {
        [DllImport("advapi32.dll", SetLastError = true)] extern private static IntPtr RegCloseKey(UIntPtr hKey);
        [DllImport("advapi32.dll", SetLastError = true, EntryPoint = "RegOpenKeyEx")]
        extern private static int RegOpenKeyEx_DllImport(UIntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out UIntPtr phkResult);
        [DllImport("advapi32.dll")]
        extern private static int RegQueryInfoKey(
            UIntPtr hkey, StringBuilder lpClass, ref uint lpcbClass,
            IntPtr lpReserved, out uint lpcSubKeys, out uint lpcbMaxSubKeyLen,
            out uint lpcbMaxClassLen, out uint lpcValues, out uint lpcbMaxValueNameLen,
            out uint lpcbMaxValueLen, out uint lpcbSecurityDescriptor, out long lpftLastWriteTime
            );

        // Là controller giữa view và model
        private ObservableCollection<AppInstalledModel> _appInstalledModel { get; set; }
        private static List<DataAppInstall> appInstalls = new List<DataAppInstall>();

        //public ObservableCollection<DetailModel> DetailModel { get; set; }

        //Properties
        public ObservableCollection<AppInstalledModel> AppInstalledModel
        {
            get
            {
                return _appInstalledModel;
            }
            set
            {
                _appInstalledModel = value;
                OnPropertyChanged(nameof(AppInstalledModel));
            }
        }
        public AppInstalledViewModel()
        {
            AppInstalledModel = new ObservableCollection<AppInstalledModel>();
            CheckAppInstall();
            int id = 1;
            AppInstalledModel.Clear();
            foreach (var item in appInstalls)
            {
                //AppInstalledModel.Add(new AppInstalledModel { ID = id.ToString(), AppName = item.AppName, TimeInstall = item.TimeInstall });
                AppInstalledModel.Add(new AppInstalledModel { ID = "0", AppName = "Garena", TimeInstall = item.TimeInstall });
                id += 1;
                return;
            }
            return;

        }
        public static bool CheckAppInstall()
        {
            appInstalls.Clear();
            DateTime dateNow = DateTime.Now;

            string sSubKeyx64 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            string sSubKeyx86 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            List<string> sReturnList = new List<string>();

            try
            {
                // 
                dateNow = DateTime.Now;
                RegistryKey regKeyx64 = Registry.LocalMachine.OpenSubKey(sSubKeyx64);
                RegistryKey regKeyx86 = Registry.LocalMachine.OpenSubKey(sSubKeyx86);

                //if (regKeyx64 != null)
                //{
                //    Int32 subKeyCountx64 = regKeyx64.SubKeyCount;
                //    String[] subKeyNamex64 = regKeyx64.GetSubKeyNames();
                //    //if (KeyCountOldx64 != subKeyCountx64)
                //    //{
                //    foreach (string keyName in subKeyNamex64)
                //    {
                //        object oValueName = regKeyx64.OpenSubKey(keyName).GetValue("DisplayName");
                //        if (oValueName != null)
                //        {
                //            DateTime TimeLastWriteKey = QueryTimeReg(sSubKeyx64 + "\\" + keyName);
                //            if (dateNow.Ticks - TimeLastWriteKey.Ticks <= 6048000000000) // 7 days
                //            {
                //                //sReturnList.Add(oValueName.ToString() + ":" + TimeLastWriteKey.ToString());
                //                //Console.WriteLine(oValueName + ": " + TimeLastWriteKey);
                //                appInstalls.Add(new DataAppInstall { AppName = oValueName.ToString(), TimeInstall = TimeLastWriteKey.ToString() });

                //            }
                //        }
                //    }
                //    //    KeyCountOldx64 = subKeyCountx64;
                //    //}
                //}
                if (regKeyx86 != null)
                {
                    Int32 subKeyCountx86 = regKeyx86.SubKeyCount;
                    String[] subKeyNamex86 = regKeyx86.GetSubKeyNames();
                    //if (KeyCountOldx86 != subKeyCountx86)
                    //{
                    foreach (var keyName in subKeyNamex86)
                    {
                        object oValueName = regKeyx86.OpenSubKey(keyName).GetValue("DisplayName");
                        if (oValueName != null)
                        {
                            DateTime TimeLastWriteKey = QueryTimeReg(sSubKeyx86 + "\\" + keyName);
                            if (dateNow.Ticks - TimeLastWriteKey.Ticks <= 6048000000000) // 7 days
                            {
                                //sReturnList.Add(oValueName.ToString() + ":" + TimeLastWriteKey.ToString());
                                //Console.WriteLine(oValueName + ": " + TimeLastWriteKey);
                                appInstalls.Add(new DataAppInstall { AppName = oValueName.ToString(), TimeInstall = TimeLastWriteKey.ToString() });

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("App Install Error: " + e.Message);
                return false;
            }
            return true;
        }
        private static DateTime QueryTimeReg(string sSubKey)
        {
            UIntPtr hKeyVal;
            StringBuilder classStr = new StringBuilder(255);
            uint classSize = (uint)classStr.Capacity + 1;
            uint lpcSubKeys;
            uint lpcbMaxSubKeyLen;
            uint lpcbMaxClassLen;
            uint lpcValues;
            uint lpcbMaxValueNameLen;
            uint lpcbMaxValueLen;
            uint lpcbSecurityDescriptor;
            long lpftLastWriteTime;

            RegOpenKeyEx_DllImport((UIntPtr)0x80000002, sSubKey, 0, 0x1, out hKeyVal);
            RegQueryInfoKey(hKeyVal, classStr, ref classSize, IntPtr.Zero, out lpcSubKeys, out lpcbMaxSubKeyLen, out lpcbMaxClassLen, out lpcValues, out lpcbMaxValueNameLen, out lpcbMaxValueLen, out lpcbSecurityDescriptor, out lpftLastWriteTime);

            //Console.WriteLine(lpftLastWriteTime);
            System.DateTime fCreationTime = System.DateTime.FromFileTime(lpftLastWriteTime);
            //Console.WriteLine(fCreationTime.ToString());

            IntPtr err = RegCloseKey(hKeyVal);
            return fCreationTime;
        } // Lấy time của reg mới cài
    }
    public class DataAppInstall
    {
        public string AppName { get; set; }
        public string TimeInstall { get; set; }
    }
}
