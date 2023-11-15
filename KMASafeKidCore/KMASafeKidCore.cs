using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Management;

//using System.Text.RegularExpressions;

namespace KMASafeKidCore
{
    class KMASafeKidCore 
    {

        static void Main(string[] args)
        {
            Initialize(); // khoi tao


            //AsynchronousClient.StartClient();
            //CDataSync.SyncDiary();
            //return;
            ////1:************************ LOAD DB block, Chi cho chay 1 tien trinh
            var current_process = System.Diagnostics.Process.GetCurrentProcess();
            var other_process = System.Diagnostics.Process.GetProcessesByName(current_process.ProcessName).FirstOrDefault(p => p.Id != current_process.Id);
            if (other_process != null && other_process.MainWindowHandle != IntPtr.Zero)
            {
                current_process.Kill();
            }
            //PermissFile
            ConfigInApp.ReadConfigFile();
            GrantAccess(ConfigInApp.CONFIG_NAME);
            BlockDB.LoadDB();

            //MonitorWindowApp.AddBlockAppToDB(@"C:\Program Files\Google\Chrome\Application\chrome.exe");
            //PipesClient.PipeConnect();


            Thread ThreadSafeBrowser = new Thread(() =>
            {
                //MonitorWebApp.MonitorEachBrowser();
                ProxyFilter.StartProxyFilter("blocklist.txt");
            });
            Thread ThreadApp = new Thread(() =>
            {
                MonitorWindowApp.AppMonitor();
            });

            Thread ThreadSync = new Thread(() =>
            {
                CDataSync.SyncDiary(); // code của thread bên trong này
            });

            ThreadSafeBrowser.IsBackground = true;
            ThreadSafeBrowser.Start();

            ThreadApp.IsBackground = true;
            ThreadApp.Start();

            ThreadSync.IsBackground = true;
            ThreadSync.Start();
            ////MonitorWindowApp.CheckAppInstall();

            //BlockDB.LoadDB();
            //BlockDB.AddUserHostDB("addtest");
            //AES.TestAes();
            PipesServer.PipeServer();
            
            //Console.ReadKey();
            return;
        }

        private static void Initialize()
        {
            string pcId = Utils.GetPcIdentify();
            Utils.AddRegistry("pcId", pcId);

        }
        private static void GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            dSecurity.SetAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            //new SecurityIdentifier(WellKnownSidType.WorldSid, null)
            //dSecurity.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User, FileSystemRights.FullControl| FileSystemRights.Modify, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
            
        }
    }

}
