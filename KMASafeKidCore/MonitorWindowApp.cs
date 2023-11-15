using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Automation;
using Microsoft.Win32;
using System.Threading;
using System.Windows;
// Dùng SQLite3
namespace KMASafeKidCore
{
    class MonitorWindowApp
    {
        [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        //[DllImport("user32.dll", SetLastError = true)] private static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("advapi32.dll", SetLastError = true)] extern private static IntPtr RegCloseKey(UIntPtr hKey);
        [DllImport("advapi32.dll", SetLastError = true, EntryPoint = "RegOpenKeyEx")] 
        extern private static int RegOpenKeyEx_DllImport(UIntPtr hKey, string lpSubKey,uint ulOptions, int samDesired, out UIntPtr phkResult);
        [DllImport("advapi32.dll")] 
        extern private static int RegQueryInfoKey(
            UIntPtr hkey,StringBuilder lpClass,ref uint lpcbClass,
            IntPtr lpReserved,out uint lpcSubKeys,out uint lpcbMaxSubKeyLen,
            out uint lpcbMaxClassLen,out uint lpcValues,out uint lpcbMaxValueNameLen,
            out uint lpcbMaxValueLen,out uint lpcbSecurityDescriptor,out long lpftLastWriteTime
            );
        private static String sCURRENT_DIR     =  System.IO.Directory.GetCurrentDirectory();
        private static String sNAME_DB_SQLITE    =  "BlockDB.sqlite";
        //private static String sFOLDER_DIARY_APP  =  sCURRENT_DIR + "\\" + "DiaryApp";
        public static String sPATH_DB = sCURRENT_DIR + "\\" + sNAME_DB_SQLITE;
        private static UInt32 IdProcActive;
        public static IntPtr hWindowActive;
        private static Process procActive = null;
        private static ModifySQlite m_sQliteApp = new ModifySQlite();

        // Ta sẽ có thread AppUsed và Thread Check App Install
        public static void AppMonitor()
        {

            // Khởi tạo kết nối SQlite
            while (true)
            {
                if (m_sQliteApp.ConnectToDB(sPATH_DB))
                    break;
                System.Threading.Thread.Sleep(2000);
            }
            // Thread lấy app ground
            Thread ThreadGetBackGroundApp = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        hWindowActive = GetForegroundWindow();
                        GetWindowThreadProcessId(hWindowActive, out IdProcActive);
                        procActive = Process.GetProcessById((int)IdProcActive);
                        System.Threading.Thread.Sleep(500);
                    }
                    catch (Exception)
                    {
                        System.Threading.Thread.Sleep(500);
                        continue;
                    }
                }
            });
            ThreadGetBackGroundApp.IsBackground = true;
            ThreadGetBackGroundApp.Start();

            AppUsedAndBlock();
        }
        private static bool AppUsedAndBlock() // App được bật ngày hôm đó 
        {
            String  FileDescription = ""; // Dùng cái này để exit exe nếu nó có tên như zay
            DateTime startTime = new DateTime();
            bool bWriteTime = true; // flag để ghi time start 
            string FileDescriptionOld;
            //Process[] listProc = new Process[255];

            while (true)
            {
                try
                {
                    // Tên app cũ mới
                    
                    FileDescriptionOld = FileDescription;
                    if (procActive != null)
                        FileDescription = procActive.MainModule.FileVersionInfo.FileDescription; // Tên app
                    else continue;

                    //*************************BLOCK APP**************************
                    if (FileDescription == FileDescriptionOld)
                        goto _DIARY;
                    if (BlockDB.IsUserApp(FileDescription))
                    {
                        Console.WriteLine($"User App: {FileDescription}");
                        KillWindows(procActive);
                        new Thread(() => { MessageBox.Show($"Phần mềm {FileDescription} bị chặn bởi V Internet Safety !!!", "Cảnh báo !!"); }).Start();
                    }

                    if (ConfigInApp.bBlockGame)
                        if (BlockDB.IsAppGame(FileDescription))// nếu app có trong db
                        {
                            KillWindows(procActive);

                            new Thread(() =>{ MessageBox.Show($"Phần mềm {FileDescription} bị chặn bởi V Internet Safety !!!", "Cảnh báo !!"); }).Start();
                        }

                    _DIARY:
                    // ************************Diary App **********************
                    if (ConfigInApp.bDiaryApp)
                    {
                        if (bWriteTime)
                        {
                            startTime = DateTime.Now;
                            bWriteTime = false;
                        }
                        // Time Used Cập nhật theo giây
                        if (FileDescription != FileDescriptionOld)
                        {
                            if (FileDescriptionOld == "")
                                continue;
                            DateTime endTime = DateTime.Now;
                            var z = endTime.ToLongDateString();
                            TimeSpan timeUsed = endTime.Subtract(startTime);
                            m_sQliteApp.AddAppDiaryToDB(FileDescriptionOld, (long)Math.Round(timeUsed.TotalSeconds),DateTime.Today.Date.ToFileTime());
                            bWriteTime = true;
                            // ghi vào
                        }    
                    }
                    System.Threading.Thread.Sleep(503);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error UsedApp Block with err: " + ex.Message);
                    System.Threading.Thread.Sleep(503);
                    continue;
                }
            } 
        }
        public static List<string> CheckAppInstall()
        {


            DateTime    dateNow = DateTime.Now;
            //int         KeyCountOldx64 = 0;
            //int         KeyCountOldx86 = 0;
            //string      sFolderAppInstalled = sCURRENT_DIR + "\\AppInstalled";
            //string      s7DayPathFile = sFolderAppInstalled + "\\" + "7days.dat";
            string      sSubKeyx64 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            string      sSubKeyx86 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            List<string> sReturnList = new List<string>();
            // Create Folder ném App đã install
            //if (!Directory.Exists(sFolderAppInstalled))
            //{
            //    Directory.CreateDirectory(sFolderAppInstalled);
            //}
            // Check file Ngày tháng có tồn tại không

            try
            {
                // 
                dateNow = DateTime.Now;
                RegistryKey regKeyx64 = Registry.LocalMachine.OpenSubKey(sSubKeyx64);
                RegistryKey regKeyx86 = Registry.LocalMachine.OpenSubKey(sSubKeyx86);

                if (regKeyx64 != null)
                {
                    Int32 subKeyCountx64 = regKeyx64.SubKeyCount;
                    String[] subKeyNamex64 = regKeyx64.GetSubKeyNames();
                    //if (KeyCountOldx64 != subKeyCountx64)
                    //{
                    foreach (var keyName in subKeyNamex64)
                    {
                        object oValueName = regKeyx64.OpenSubKey(keyName).GetValue("DisplayName");
                        if (oValueName != null)
                        {
                            DateTime TimeLastWriteKey = QueryTimeReg(sSubKeyx64 + "\\" + keyName);
                            if (dateNow.Ticks - TimeLastWriteKey.Ticks <= 6048000000000) // 7 days
                            {
                                sReturnList.Add(oValueName.ToString() + ":" + TimeLastWriteKey.ToString());
                                Console.WriteLine(oValueName + ": " + TimeLastWriteKey);
                            }
                        }
                    }
                    //    KeyCountOldx64 = subKeyCountx64;
                    //}
                }
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
                                sReturnList.Add(oValueName.ToString() + ":" + TimeLastWriteKey.ToString());
                                Console.WriteLine(oValueName + ": " + TimeLastWriteKey);
                            }
                        }
                    }
                    //    KeyCountOldx86 = subKeyCountx86;
                    //}
                }
                //using (StreamWriter streamFile = new StreamWriter(s7DayPathFile))
                //{
                //    foreach (var item in sWriteFile)
                //    {
                //        streamFile.WriteLine(item); // ghi mới
                //    }
                //    sWriteFile.Clear();
                //}
                //System.Threading.Thread.Sleep(60000*5);
            }
            catch (Exception e)
            {
                Console.WriteLine("App Install Error: " + e.Message);
                throw;
            }
            return sReturnList;
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
        private static bool KillWindows(Process process)
        {
            try
            {
                process.CloseMainWindow();
                process.Kill();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
