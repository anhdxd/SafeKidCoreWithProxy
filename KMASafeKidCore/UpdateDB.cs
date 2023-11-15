using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace KMASafeKidCore
{
    class UpdateDB
    {
        private static WebClient myWebClient = new WebClient();
        private static string sPathDown = System.IO.Directory.GetCurrentDirectory() + "\\Download";
        public static void DownDBFromServer()
        {
            string sAddr = @"https://actvneduvn-my.sharepoint.com/personal/at160104_actvn_edu_vn/_layouts/15/download.aspx?SourceUrl=%2Fpersonal%2Fat160104%5Factvn%5Fedu%5Fvn%2FDocuments%2FTaiLieu%2FBKAV%2Fcode%20Schedule%2Etxt";
            myWebClient.DownloadFile(sAddr,sPathDown+"\\Filetest.txt");
        }
    }
}
