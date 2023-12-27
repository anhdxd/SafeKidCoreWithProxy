
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows;
using System.Threading;
namespace KMASafeKidCore
{
    class MonitorWebApp
    {
        [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);
        [DllImport("user32.dll")] private static extern uint MapVirtualKeyA(uint uCode, uint uMapType);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        private static readonly Dictionary<string, string> SafeSearchParam = new Dictionary<string, string>()
        {
            {"google.com/search?",                "&safe=active"},
            {"google.com.vn/search?",             "&safe=active"},
            {"bing.com/search?",                  "&adlt=strict"},
            {"duckduckgo.com/?",                  "&kp=1"},
            {"search.yahoo.com/search;",            "&vm=r"},
            {"vn.search.yahoo.com/search;",         "&vm=r"},
            //{"coccoc.com/search?query=",            "&safe=1"},
            {"search.brave.com/search?q=",          "&safesearch=strict"},
            {"yandex.com/search/?text=",            "&fyandex=1"},
        };
        //private static String NameSQliteDB = "BlockDB.sqlite";
        private static String sPathDB = MonitorWindowApp.sPATH_DB;//System.IO.Directory.GetCurrentDirectory() + "\\" + NameSQliteDB;
                                                                  //private static ModifySQlite m_sQliteApp = new ModifySQlite();
        private static ModifySQlite m_sQliteApp = new ModifySQlite();

        public static bool MonitorEachBrowser() // Tìm handle và chạy safe cho các cái kia
        {
            // Khởi tạo kết nối SQlite
            while (true)
            {
                if (m_sQliteApp.ConnectToDB(MonitorWindowApp.sPATH_DB))
                    break;
                System.Threading.Thread.Sleep(2000);
            }

            Console.WriteLine("Start MonitorEachBrowser");
            while (true)
            {
                try
                {
                    IntPtr ActiveWindow = MonitorWindowApp.hWindowActive;//GetForegroundWindow();
                    if (ActiveWindow == IntPtr.Zero)
                    {
                        System.Threading.Thread.Sleep(502);
                        continue;
                    }

                    AutomationElement elmRoot = AutomationElement.FromHandle(ActiveWindow);
                    switch (elmRoot.Current.ClassName)
                    {
                        case "Chrome_WidgetWin_1":
                            ChromiumSafeSearch(elmRoot, ActiveWindow);
                            break;
                        //case "MozillaWindowClass":
                        //    MozzilaSafeSearch(elmRoot, ActiveWindow);
                        //    break;
                        default:
                            break;
                    }
                    System.Threading.Thread.Sleep(502);
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(502);
                    continue;
                }
            }

        }

        public static bool AddUserHostToDB(string sDomain) 
        {
            return BlockDB.AddUserHostDB(sDomain);
        }
        public static bool DelUserHostFromDB(string sDomain)
        {
            return BlockDB.DeleteUserHostDB(sDomain);
        }
        public static bool ChromiumSafeSearch(AutomationElement elmRoot, IntPtr ActiveWindow)// bao gom edge, coccoc
        {
            try
            {
                //Console.WriteLine("ClassName: " + elmRoot.Current.ClassName);
                DateTime startTime = new DateTime();
                bool bWriteTime = true; // flag để ghi time start 
                string sDomainOld = "";
                string sDomain = "";
                bool bIsThreadRun = false;
                // Close tab ẩn danh nếu có setting
                if (ConfigInApp.bInPrivateBlock)
                {
                    if (elmRoot.Current.Name.IndexOf("[InPrivate]") != -1)
                    {
                        Console.WriteLine("InPrivate: ");
                        Process.GetProcessById(elmRoot.Current.ProcessId).CloseMainWindow();//Close window
                        new Thread(() => { MessageBox.Show($"Hư quá !!! Tab ẩn danh đã bị chặn bởi V Internet Safety.", "Cảnh báo !!"); }).Start();
                        return true;
                    }
                }
                
                // Đặt lại thanh bar thành tìm kiếm an toàn
                //if (ConfigInApp.bSafeSearch || ConfigInApp.bBlockDomain)
                //{
                    AutomationElement elmToolbar = elmRoot.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar));
                    if (elmToolbar == null)
                        return false;

                    AutomationElement elmUrlBar = elmToolbar.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Address and search bar", PropertyConditionFlags.None));
                    if (elmUrlBar != null)
                    {
                        //Console.WriteLine("go too: ");
                        goto _SETURLBAR;
                    }
                    else if ((elmUrlBar = elmToolbar.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Thanh địa chỉ và tìm kiếm", PropertyConditionFlags.None))) == null)
                    {
                        //Console.WriteLine("RETURN elmUrlBar: ");
                        return false;
                    }
                _SETURLBAR:
                    AutomationPattern[] patterns = elmUrlBar.GetSupportedPatterns();// Lấy patterns
                    if (patterns.Length > 0)
                    {
                        ValuePattern valUrlBar = (ValuePattern)elmUrlBar.GetCurrentPattern(patterns[0]); // chỉ sử dụng patterns đầu tiên
                        IntPtr ActiveWindowOld = ActiveWindow;

                        // Vì cùng là 1 element handle nên valUrlBar giữ cho tới khi out ra màn hình khác
                        
                        while (true)
                        {
                            if(sDomain != "")
                                sDomainOld = sDomain;
                            //Check handle cũ mới
                            ActiveWindow = GetForegroundWindow();
                            if (ActiveWindow == IntPtr.Zero) continue;
                            if (ActiveWindowOld != ActiveWindow) return true;
                            
                            // Get domain Block
                            sDomain = BlockDomainByWeb.SplitDomain(valUrlBar.Current.Value);
                            if (sDomain == "")
                                goto _END;


                            //if (BlockDB.IsUserHost(sDomain))
                            //    goto _BLOCKURL;
                            //if (ConfigInApp.bBlockAdult)
                            //    if (BlockDB.IsAdultHost(sDomain))
                            //        goto _BLOCKURL;
                            //if(ConfigInApp.bBlockSocial)
                            //    if (BlockDB.IsSocialHost(sDomain))
                            //        goto _BLOCKURL;
                            //goto _SAFESEARCH;

                            //_BLOCKURL:
                            //string sUrlBlock = BlockDomainByWeb.m_gProtocol + sDomain + BlockDomainByWeb.m_gHtmlBlock;
                            //valUrlBar.SetValue(sUrlBlock);

                            //var vrk = MapVirtualKeyA(0x0D, 0); //0x0D: VK_RETURN
                            //PostMessage(ActiveWindow, 0x0100, 0x0D, 0x0001 | vrk >> 16);
                            //PostMessage(ActiveWindow, 0x0101, 0x0D, 0x0001 | vrk >> 16 | 0xC0 >> 24);

                            //_SAFESEARCH:
                            //if (ConfigInApp.bSafeSearch)
                            //{
                            //    foreach (KeyValuePair<string, string> item in SafeSearchParam) // check domain nào
                            //    {
                            //        int bDomainSearch = valUrlBar.Current.Value.IndexOf(item.Key);
                            //        if (bDomainSearch != -1)
                            //        {
                            //            if (valUrlBar.Current.Value.IndexOf(item.Value) == -1) // Nếu chưa vào link safe thì add vào
                            //            {
                            //                string sUrlSafe = valUrlBar.Current.Value + item.Value;
                            //                valUrlBar.SetValue(sUrlSafe);

                            //                var VirtualKey = MapVirtualKeyA(0x0D, 0); //0x0D: VK_RETURN
                            //                PostMessage(ActiveWindow, 0x0100, 0x0D, 0x0001 | VirtualKey >> 16);
                            //                PostMessage(ActiveWindow, 0x0101, 0x0D, 0x0001 | VirtualKey >> 16 | 0xC0 >> 24);
                            //            }
                            //            break;
                            //        }
                            //    }
                            //}
                        _END:

                            // DIARY WEB *********************************************88
                            if (!bIsThreadRun)
                            {
                                Thread DiaryWebThread = new Thread(() =>
                                {
                                    try
                                    {
                                        while (true)
                                        {
                                            ActiveWindow = GetForegroundWindow();
                                            if (ActiveWindow == IntPtr.Zero) continue;
                                            if (ActiveWindowOld != ActiveWindow)
                                            {
                                                if (sDomainOld == "")
                                                    return;
                                                DateTime endTime = DateTime.Now;
                                                var z = endTime.ToLongDateString();
                                                TimeSpan timeUsed = endTime.Subtract(startTime);
                                                m_sQliteApp.AddAppDiaryToDB(sDomainOld, (long)Math.Round(timeUsed.TotalSeconds), DateTime.Today.Date.ToFileTime());
                                                return;
                                            }
                                            Console.WriteLine("sDomain: " + sDomain);
                                            if (bWriteTime)
                                            {
                                                startTime = DateTime.Now;
                                                bWriteTime = false;
                                            }
                                            if (sDomain != "" && sDomainOld != sDomain && sDomainOld != "")
                                            {
                                                DateTime endTime = DateTime.Now;
                                                var z = endTime.ToLongDateString();
                                                TimeSpan timeUsed = endTime.Subtract(startTime);
                                                m_sQliteApp.AddAppDiaryToDB(sDomainOld, (long)Math.Round(timeUsed.TotalSeconds), DateTime.Today.Date.ToFileTime());
                                                bWriteTime = true;
                                            // ghi vào
                                            }
                                            System.Threading.Thread.Sleep(2000);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Exception Diary Web: " + e.Message);
                                        bIsThreadRun = false;
                                    //return false;
                                        return;
                                    }
                                });
                                DiaryWebThread.IsBackground = true;
                                DiaryWebThread.Start();
                                bIsThreadRun = true;
                            }
                            System.Threading.Thread.Sleep(202);
                        }
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                //return false;
                throw;
            }

            return true;
        }

        public static bool MozzilaSafeSearch(AutomationElement elmRoot, IntPtr ActiveWindow)
        {
            try
            {
                // Close tab ẩn danh nếu có setting
                DateTime startTime = new DateTime();
                bool bWriteTime = true; // flag để ghi time start 
                string sDomainOld = "";
                string sDomain = "";
                bool bIsThreadRun = false;
                if (ConfigInApp.bInPrivateBlock)
                {
                    if (elmRoot.Current.Name.IndexOf("(Private Browsing)") != -1)
                    {
                        Console.WriteLine("Private Browsing: ");
                        Process.GetProcessById(elmRoot.Current.ProcessId).CloseMainWindow();//Close window
                        return true;
                    }
                }
                // Đặt lại thanh bar thành tìm kiếm an toàn
                if (ConfigInApp.bSafeSearch || ConfigInApp.bBlockDomain)
                {
                    // Dùng thread để timeout tăng hiệu năng
                    AutomationElement elmUrlBar = null;
                    System.Threading.Thread threadFindUrlBar = new System.Threading.Thread(() =>
                    {
                        Console.WriteLine("tim kiem elmBAR ");
                        AutomationElement elmToolbar = elmRoot.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Navigation", PropertyConditionFlags.None));
                        if (elmToolbar == null)
                            return;

                        Console.WriteLine("tim kiem Combobox ");
                        AutomationElement elmComboBox = elmToolbar.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox));
                        if (elmComboBox == null)
                            return;
                        Console.WriteLine("tim kiem elmBar ");
                        elmUrlBar = elmComboBox.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Search with Google or enter address", PropertyConditionFlags.None));
                        if (elmUrlBar == null)
                        {
                            Console.WriteLine("return urlbar: ");
                            return;
                        }

                    });
                    threadFindUrlBar.IsBackground = true;
                    threadFindUrlBar.Start();
                    // Điều kiện set timeout cho tìm kiếm
                    for (int i = 0; i <= 20; i++)
                    {
                        if (elmUrlBar == null)
                            System.Threading.Thread.Sleep(100);
                        else break;
                        if (i == 20 && elmUrlBar == null)
                            return false;
                    }
                    // _SETURLSAFE: // Set Url cho safe
                    Console.WriteLine("tim kiem Patterns ");
                    AutomationPattern[] patterns = elmUrlBar.GetSupportedPatterns();// Lấy patterns
                    if (patterns.Length > 0)
                    {
                        ValuePattern valUrlBar = (ValuePattern)elmUrlBar.GetCurrentPattern(patterns[1]); // chỉ sử dụng patterns đầu tiên
                        IntPtr ActiveWindowOld = ActiveWindow;
                        while (true)
                        {
                            if (sDomain != "")
                                sDomainOld = sDomain;
                            //Check handle cũ mới Sử dụng lại element để không bị quá tải
                            ActiveWindow = GetForegroundWindow();
                            if (ActiveWindow == IntPtr.Zero) continue;
                            if (ActiveWindowOld != ActiveWindow) return true;

                            sDomain = BlockDomainByWeb.SplitDomain(valUrlBar.Current.Value);
                            if (sDomain == "")
                                goto _END;

                            if (BlockDB.IsUserHost(sDomain))
                                goto _BLOCKURL;
                            if (ConfigInApp.bBlockAdult)
                                if (BlockDB.IsAdultHost(sDomain))
                                    goto _BLOCKURL;
                            if (ConfigInApp.bBlockSocial)
                                if (BlockDB.IsSocialHost(sDomain))
                                    goto _BLOCKURL;
                            goto _SAFESEARCH;

                        _BLOCKURL:
                            string sUrlBlock = BlockDomainByWeb.m_gProtocol + sDomain + BlockDomainByWeb.m_gHtmlBlock;
                            valUrlBar.SetValue(sUrlBlock);

                            var vrk = MapVirtualKeyA(0x0D, 0); //0x0D: VK_RETURN
                            PostMessage(ActiveWindow, 0x0100, 0x0D, 0x0001 | vrk >> 16);
                            PostMessage(ActiveWindow, 0x0101, 0x0D, 0x0001 | vrk >> 16 | 0xC0 >> 24);

                        _SAFESEARCH:
                            if (ConfigInApp.bSafeSearch)
                            {
                                foreach (KeyValuePair<string, string> item in SafeSearchParam) // check domain nào
                                {
                                    int bDomainSearch = valUrlBar.Current.Value.IndexOf(item.Key);
                                    if (bDomainSearch != -1)
                                    {
                                        if (valUrlBar.Current.Value.IndexOf(item.Value) == -1) // Nếu chưa vào link safe thì add vào
                                        {
                                            string sUrlSafe = valUrlBar.Current.Value + item.Value;
                                            valUrlBar.SetValue(sUrlSafe);

                                            var VirtualKey = MapVirtualKeyA(0x0D, 0); //0x0D: VK_RETURN
                                            PostMessage(ActiveWindow, 0x0100, 0x0D, 0x0001 | VirtualKey >> 16);
                                            PostMessage(ActiveWindow, 0x0101, 0x0D, 0x0001 | VirtualKey >> 16 | 0xC0 >> 24);
                                        }
                                        break;
                                    }
                                }
                            }
                            _END:

                            // DIARY WEB *********************************************88
                            if (!bIsThreadRun)
                            {
                                Thread DiaryWebThread = new Thread(() =>
                                {
                                    try
                                    {
                                        while (true)
                                        {
                                            ActiveWindow = GetForegroundWindow();
                                            if (ActiveWindow == IntPtr.Zero) continue;
                                            if (ActiveWindowOld != ActiveWindow)
                                            {
                                                if (sDomainOld == "")
                                                    return;
                                                DateTime endTime = DateTime.Now;
                                                var z = endTime.ToLongDateString();
                                                TimeSpan timeUsed = endTime.Subtract(startTime);
                                                m_sQliteApp.AddAppDiaryToDB(sDomainOld, (long)Math.Round(timeUsed.TotalSeconds), DateTime.Today.Date.ToFileTime());
                                                return;
                                            }
                                            Console.WriteLine("sDomain: " + sDomain);
                                            if (bWriteTime)
                                            {
                                                startTime = DateTime.Now;
                                                bWriteTime = false;
                                            }
                                            if (sDomain != "" && sDomainOld != sDomain && sDomainOld != "")
                                            {
                                                DateTime endTime = DateTime.Now;
                                                var z = endTime.ToLongDateString();
                                                TimeSpan timeUsed = endTime.Subtract(startTime);
                                                m_sQliteApp.AddAppDiaryToDB(sDomainOld, (long)Math.Round(timeUsed.TotalSeconds), DateTime.Today.Date.ToFileTime());
                                                bWriteTime = true;
                                                // ghi vào
                                            }
                                            System.Threading.Thread.Sleep(2000);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Exception Diary Web: " + e.Message);
                                        bIsThreadRun = false;
                                        //return false;
                                        return;
                                    }
                                });
                                DiaryWebThread.IsBackground = true;
                                DiaryWebThread.Start();
                                bIsThreadRun = true;
                            }
                            System.Threading.Thread.Sleep(502);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Mozzila: " + e.Message);
                //return false;
                throw;
            }

            return true;

        }

            
    }
}
