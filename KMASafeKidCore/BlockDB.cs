using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
namespace KMASafeKidCore
{
    
    class BlockDB
    {
        private static SortedSet<string> listAdult = new SortedSet<string>();
        private static SortedSet<string> listSocial = new SortedSet<string>();
        private static SortedSet<string> listUserHost = new SortedSet<string>();
        private static SortedSet<string> listUserApp = new SortedSet<string>();
        private static SortedSet<string> listGameApp = new SortedSet<string>();

        public static SortedSet<string> ListAdult { get => listAdult; set => listAdult = value; }
        public static SortedSet<string> ListSocial { get => listSocial; set => listSocial = value; }
        public static SortedSet<string> ListUserHost { get => listUserHost; set => listUserHost = value; }
        public static SortedSet<string> ListUserApp { get => listUserApp; set => listUserApp = value; }
        public static SortedSet<string> ListGameApp { get => listGameApp; set => listGameApp = value; }

        private static ModifySQlite sQlite = new ModifySQlite();
        public static bool LoadDB()
        {
            if (!sQlite.isConnect)
                sQlite.ConnectToDB(MonitorWindowApp.sPATH_DB);

            if (!System.IO.Directory.Exists(".\\DBB"))  
                System.IO.Directory.CreateDirectory(".\\DBB");

            ListAdult = AES.DecryptFileToSortedSet(@".\DBB\ADB.dat");
            ListSocial = AES.DecryptFileToSortedSet(@".\DBB\SDB.dat");
            ListGameApp = AES.DecryptFileToSortedSet(@".\DBB\GDB.dat");
            ListUserHost = sQlite.CGetAllBlockRule(ModifySQlite.ObjectType.WEB);
            ListUserApp = sQlite.CGetAllBlockRule(ModifySQlite.ObjectType.APP); 

            return true;
        }
        // Dung truc tiep tai class nay
        public static bool AddUserHostDB(string sDomainBlock)
        {
            sQlite.AddWebBlockToDB(sDomainBlock);
            ListUserHost = sQlite.CGetAllBlockRule(ModifySQlite.ObjectType.WEB);
            return true;
        }
        public static bool DeleteUserHostDB(string sDomainBlock)
        {
            sQlite.DeleteWebFromDB(sDomainBlock);
            ListUserHost = sQlite.CGetAllBlockRule(ModifySQlite.ObjectType.WEB);

            return true;
        }
        // Dung trong class MonitorWindowsApp
        public static bool AddUserAppDB(string PathApp)
        {
            sQlite.AddAppBlockToDB(PathApp);
            ListUserApp = sQlite.CGetAllBlockRule(ModifySQlite.ObjectType.APP);
            return true;
        }
        public static bool DeleteUserAppDB(string PathApp)
        {
            sQlite.DeleteAppFromDB(PathApp);
            ListUserApp = sQlite.CGetAllBlockRule(ModifySQlite.ObjectType.APP);
            return true;
        }
        // Kiem tra DB
        public static bool IsAdultHost(string Host)
        {
            return ListAdult.Contains(Host);
        }
        public static bool IsSocialHost(string Host)
        {

            return ListSocial.Contains(Host);
        }
        public static bool IsUserHost(string Host)
        {

            return ListUserHost.Contains(Host);
        }
        public static bool IsAppGame(string AppName)
        {

            return ListGameApp.Contains(AppName);
        }
        public static bool IsUserApp(string AppName)
        {
            //Console.WriteLine($"isUserApp {AppName}");
            return ListUserApp.Contains(AppName);
        }
    }
}
