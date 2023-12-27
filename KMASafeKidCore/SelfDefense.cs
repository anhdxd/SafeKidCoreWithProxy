using EaseFilter.FilterControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;


namespace KMASafeKidCore
{
    internal class SelfDefense
    {
        static List<string> listProcess;
        static FilterControl filterControl = new FilterControl();
        public static void StartFilter()
        {
            string lastError = string.Empty;
            string licenseKey = "0B08E-13A2A-4B503-C096E-C8EC0-68BA2-83B3F-142";

            FilterAPI.FilterType filterType = FilterAPI.FilterType.MONITOR_FILTER | FilterAPI.FilterType.CONTROL_FILTER
                | FilterAPI.FilterType.PROCESS_FILTER;

            int serviceThreads = 5;
            int connectionTimeOut = 10; //seconds

            try
            {
                //copy the right Dlls to the current folder.
                EaseFilter.FilterControl.Utils.CopyOSPlatformDependentFiles(ref lastError);

                if (!filterControl.StartFilter(filterType, serviceThreads, connectionTimeOut, licenseKey, ref lastError))
                {
                    Console.WriteLine("Start Filter Service failed with error:" + lastError);
                    return;
                }

                //the watch path can use wildcard to be the file path filter mask.i.e. '*.txt' only monitor text file.
                // Get current directory
                string watchPath = System.IO.Directory.GetCurrentDirectory() + "\\*";

                //create a file protector filter rule, every filter rule must have the unique watch path. 
                FileFilter fileProtectorFilter = new FileFilter(watchPath);

                //configure the access right for the protected folder

                //prevent the file from being deleted.

                fileProtectorFilter.EnableDeleteFile = false;
                //prevent the file from being renamed.
                fileProtectorFilter.EnableRenameOrMoveFile = false;

                //prevent the file from being written.
                fileProtectorFilter.EnableWriteToFile = false;
                fileProtectorFilter.EnableReadFileData = false;

                //listProcess = new List<string>();
                //listProcess.Add("explorer");
                //listProcess.Add("KMASafeKidCore");
                //listProcess.Add("UISafekids");
                //listProcess.Add("MsMpEng");
                //listProcess.Add("csrss");
                //listProcess.Add("devenv");
                //listProcess.Add("OneDrive");
                //listProcess.Add("WerFault");

                //authorize process with full access right
                fileProtectorFilter.ProcessNameAccessRightList.Add("explorer", FilterAPI.ALLOW_MAX_RIGHT_ACCESS);
                fileProtectorFilter.ProcessNameAccessRightList.Add("KMASafeKidCore", FilterAPI.ALLOW_MAX_RIGHT_ACCESS);
                fileProtectorFilter.ProcessNameAccessRightList.Add("UISafekids", FilterAPI.ALLOW_MAX_RIGHT_ACCESS);
                fileProtectorFilter.ExcludeProcessNameList.Add("explorer.exe");
                fileProtectorFilter.ExcludeProcessNameList.Add("KMASafeKidCore.exe");
                fileProtectorFilter.ExcludeProcessNameList.Add("KMASafeKidCore");
                fileProtectorFilter.ExcludeProcessNameList.Add("UISafekids.exe");
                fileProtectorFilter.ExcludeProcessNameList.Add("UISafekids");
                fileProtectorFilter.ExcludeProcessNameList.Add("MsMpEng");
                fileProtectorFilter.ExcludeProcessNameList.Add("csrss");
                fileProtectorFilter.ExcludeProcessNameList.Add("devenv");
                fileProtectorFilter.ExcludeProcessNameList.Add("OneDrive");
                fileProtectorFilter.ExcludeProcessNameList.Add("WerFault");

                //you can enable/disalbe more access right by setting the properties of the fileProtectorFilter.

                //Filter the callback file IO events, here get callback before the file was opened/created, and file was deleted.
                fileProtectorFilter.ControlFileIOEventFilter = (ulong)(ControlFileIOEvents.OnPreFileCreate | ControlFileIOEvents.OnPreDeleteFile);

                //fileProtectorFilter.OnPreCreateFile += OnPreCreateFile;
                //fileProtectorFilter.OnPreDeleteFile += OnPreDeleteFile;

                filterControl.AddFilter(fileProtectorFilter);

                if (!filterControl.SendConfigSettingsToFilter(ref lastError))
                {
                    Console.WriteLine("SendConfigSettingsToFilter failed." + lastError);

                    return;
                }

                Console.WriteLine("Start filter service succeeded.");

                // Wait for the user to quit the program.
                Console.WriteLine("Press 'q' to quit the sample.");
                while (Console.Read() != 'q') ;

                filterControl.StopFilter();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Start filter service failed with error:" + ex.Message);
            }
        }

        static void OnPreCreateFile(object sender, FileCreateEventArgs e)
        {

            //Console.WriteLine("OnPreCreateFile:" + e.FileName + ",processName:" + e.ProcessName);

            //Console.WriteLine(e.ProcessName);
            //you can block the file open here by returning below status.
            e.ReturnStatus = NtStatus.Status.AccessDenied;

        }

        /// 

        /// Fires this event before the file was deleted.
        /// 

        static void OnPreDeleteFile(object sender, FileIOEventArgs e)
        {
            //Console.WriteLine("OnPreDeleteFile:" + e.FileName + ",userName:" + e.UserName + ",processName:" + e.ProcessName);

            //you can block the file being deleted here by returning below status.
            e.ReturnStatus = NtStatus.Status.AccessDenied;
        }
    }
}
