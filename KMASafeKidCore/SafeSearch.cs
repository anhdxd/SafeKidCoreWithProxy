
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows;

namespace TestSafeKidServices
{
    class SafeSearch
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
        
        public static bool SafeEachBrowser(bool bSafeSearch, bool bInPrivateBlock) // Tìm handle và chạy safe cho các cái kia
        {
            try
            {
                while (true)
                {
                    if (!bSafeSearch && !bInPrivateBlock)
                        return true;
                    Console.WriteLine("Start");
                    IntPtr ActiveWindow = GetForegroundWindow();
                    if (ActiveWindow == IntPtr.Zero) continue;

                    AutomationElement elmRoot = AutomationElement.FromHandle(ActiveWindow);
                    switch (elmRoot.Current.ClassName)
                    {
                        case "Chrome_WidgetWin_1":
                            ChromiumSafeSearch(elmRoot,ActiveWindow,bSafeSearch, bInPrivateBlock);
                            break;
                        case "MozillaWindowClass":
                            MozzilaSafeSearch(elmRoot, ActiveWindow,bSafeSearch, bInPrivateBlock);
                            break;
                        default:
                            break;
                    }
                    System.Threading.Thread.Sleep(1002);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public static bool ChromiumSafeSearch(AutomationElement elmRoot, IntPtr ActiveWindow, bool bSafeSearch, bool bInPrivateBlock)// bao gom edge, coccoc
        {
            try
            {
            /**
                //Process[] procsChrome = Process.GetProcessesByName("msedge");
                Console.WriteLine("Start");
                IntPtr ActiveWindow = GetForegroundWindow();
                if (ActiveWindow == IntPtr.Zero) return false;

                AutomationElement elmRoot = AutomationElement.FromHandle(ActiveWindow);
                if (elmRoot.Current.ClassName != "Chrome_WidgetWin_1")
                {
                    Console.WriteLine("Khong phai edge => return");
                    return true;
                }
                    //var buffer = new StringBuilder(4096);
                    //int namemodule = GetModuleFileNameEx(ActiveWindow, IntPtr.Zero,buffer);

                    //foreach (Process chrome in procsChrome)
                    //{
                    //    if (chrome.MainWindowHandle != ActiveWindow)
                    //    {
                    //        Console.WriteLine("EDGE not active ");
                    //        continue;
                    //    }
                    //    // the chrome process must have a window
                    //    if (chrome.MainWindowHandle == IntPtr.Zero)
                    //    {
                    //        continue;
                    //    }
            **/
                Console.WriteLine("ClassName: " + elmRoot.Current.ClassName);

                // Close tab ẩn danh nếu có setting
                if (bInPrivateBlock)
                {
                    if (elmRoot.Current.Name.IndexOf("[InPrivate]") != -1)
                    {
                        Console.WriteLine("InPrivate: ");
                        Process.GetProcessById(elmRoot.Current.ProcessId).CloseMainWindow();//Close window
                        return true;
                    }
                }
                // Đặt lại thanh bar thành tìm kiếm an toàn
                if (bSafeSearch)
                {
                    AutomationElement elmToolbar = elmRoot.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar));
                    if (elmToolbar == null)
                        return false;

                    AutomationElement elmUrlBar = elmToolbar.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Thanh địa chỉ và tìm kiếm", PropertyConditionFlags.None));
                    if (elmUrlBar != null)
                    {
                        Console.WriteLine("go too: ");
                        goto _SETURLSAFE;
                    }
                    else if ((elmUrlBar = elmRoot.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Address and search bar", PropertyConditionFlags.None))) == null)
                    {
                        Console.WriteLine("RETURN elmUrlBar: ");
                        return false;
                    }
                _SETURLSAFE:
                    AutomationPattern[] patterns = elmUrlBar.GetSupportedPatterns();// Lấy patterns
                    if (patterns.Length > 0)
                    {
                        ValuePattern valUrlBar = (ValuePattern)elmUrlBar.GetCurrentPattern(patterns[0]); // chỉ sử dụng patterns đầu tiên
                        IntPtr ActiveWindowOld = ActiveWindow;
                        // Vì cùng là 1 element handle nên valUrlBar giữ cho tới khi out ra màn hình khác
                        while (true)
                        {   
                            //Check handle cũ mới
                            ActiveWindow = GetForegroundWindow();
                            if (ActiveWindow == IntPtr.Zero) continue;
                            if (ActiveWindowOld != ActiveWindow) return true;
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
                            System.Threading.Thread.Sleep(1002);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                throw;
            }

            return true;
        }

        public static bool MozzilaSafeSearch(AutomationElement elmRoot, IntPtr ActiveWindow, bool bSafeSearch, bool bInPrivateBlock)
        {
            try
            {
                // Close tab ẩn danh nếu có setting
                if (bInPrivateBlock)
                {
                    if (elmRoot.Current.Name.IndexOf("(Private Browsing)") != -1)
                    {
                        Console.WriteLine("Private Browsing: ");
                        Process.GetProcessById(elmRoot.Current.ProcessId).CloseMainWindow();//Close window
                        return true;
                    }
                }
                // Đặt lại thanh bar thành tìm kiếm an toàn
                if (bSafeSearch)
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
                            return ;
                        Console.WriteLine("tim kiem elmBar ");
                        elmUrlBar = elmComboBox.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Search with Google or enter address",PropertyConditionFlags.None));
                        if (elmUrlBar == null)
                        {
                            Console.WriteLine("return urlbar: ");
                            return ;
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
                            //Check handle cũ mới Sử dụng lại element để không bị quá tải
                            Console.WriteLine("Vong lap while; ");
                            ActiveWindow = GetForegroundWindow();
                            if (ActiveWindow == IntPtr.Zero) continue;
                            if (ActiveWindowOld != ActiveWindow) return true;
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
                            System.Threading.Thread.Sleep(1002);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
                //throw;
            }

            return true;

        }
    }
}
