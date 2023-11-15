using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using IniParser;
using IniParser.Model;
using System.Security.Cryptography;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using Newtonsoft.Json;
using System.Net;
using Org.BouncyCastle.Utilities;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Forms;

namespace KMASafeKidCore
{
    internal class CDataSync
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string urlServer = "https://127.0.0.1:443";
        private static readonly string urlFirebase = "https://anhdz-3fe31-default-rtdb.firebaseio.com/";
        private static string idToken;
        private static string uId;
        private static string loginConfig = System.IO.Directory.GetCurrentDirectory() + "\\" + "login.ini";
        private static string appConfig = System.IO.Directory.GetCurrentDirectory() + "\\" + "config.ini";

        private static string GetPasswordLocal()
        {
            try
            {
                using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (RegistryKey subkey = key.CreateSubKey("SOFTWARE\\KMASafe"))
                    {
                        if (subkey.GetValue("pwd") == null)
                        {
                            return "";
                        }
                        else
                        {
                            return subkey.GetValue("pwd").ToString();
                        }
                    }
                }
             
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetPasswordLocal" + ex.Message);
                return "";
            }
        }

        public static void SyncDiary()
        {
            //  Get ID From ini ->  Get Diary -> AES

            string pwdEncrypt = GetPasswordLocal();
            var iniFile = new FileIniDataParser();
            bool IsRefreshToken = false;
            while (true)
            {
                try
                {
                    IniData loginIni = iniFile.ReadFile(loginConfig);
                    // login
                    idToken = loginIni["login"]["idToken"];
                    uId = loginIni["login"]["localId"];

                    // Check token
                    if (IsRefreshToken)
                    {
                        string refreshToken = loginIni["login"]["refreshToken"];
                        if (!String.IsNullOrEmpty(refreshToken))
                        {
                            var data = RefreshIdToken(refreshToken);
                            LoginSession login = JsonConvert.DeserializeObject<LoginSession>(data);

                            if (String.IsNullOrEmpty(login.idToken) || String.IsNullOrEmpty(login.refreshToken))
                            {
                                Console.WriteLine("Error RefreshToken...null");
                                System.Threading.Thread.Sleep(10 * 1000);
                                continue;
                            }
                            loginIni["login"]["idToken"] = login.idToken;
                            loginIni["login"]["refreshToken"] = login.refreshToken;
                            iniFile.WriteFile(loginConfig, loginIni);
                        }
                        IsRefreshToken = false;
                        continue;
                    }

                    if (String.IsNullOrEmpty(idToken) || String.IsNullOrEmpty(uId))
                    {
                        Console.WriteLine("Account do not exists");
                        return;
                    }
                    JArray jDiary = GetDiary();
                    JArray jSettings = GetSettings();

                    // Get AES random
                    var crypt = Aes.Create();

                    // Encrypt Diary, aeskey
                    var aes_key = pwdEncrypt.Substring(0, 32);
                    string diary_encrypt = AES.EncryptStringToBase64(jDiary.ToString(), aes_key);
                    string settings_encrypt = AES.EncryptStringToBase64(jSettings.ToString(), aes_key);

                    // upload direct to server
                    if (String.IsNullOrEmpty(diary_encrypt) || String.IsNullOrEmpty(settings_encrypt))
                    {
                        Console.WriteLine("Error Encrypt");
                        System.Threading.Thread.Sleep(10 * 1000);
                        continue;
                    }

                    string pcId = Utils.GetRegistry("pcId");
                    string pcName = Environment.MachineName;
                    SetFirebaseData($"users/{uId}/{pcId}", idToken, new { diarys = diary_encrypt });
                    SetFirebaseData($"users/{uId}/{pcId}", idToken, new { settings = settings_encrypt });
                    SetFirebaseData($"users/{uId}/{pcId}", idToken, new { pcname = pcName });

                    crypt.Clear();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error SyncDiary:" + e.Message);
                    IsRefreshToken = true;
                    System.Threading.Thread.Sleep(10 * 1000);
                    continue;
                    //System.Threading.Thread.Sleep(30 * 1000);
                }

                System.Threading.Thread.Sleep(30 * 1000);
            }
        }
        private static JArray GetSettings()
        {
            var iniFile = new FileIniDataParser();
            // settings

            var jArray = new JArray();
            IniData configIni = iniFile.ReadFile(appConfig);

            JObject jChild = new JObject
            {
                ["DiaryApp"] = configIni["BlockConfig"]["DiaryApp"],
                ["BlockGame"] = configIni["BlockConfig"]["BlockGame"],
                ["BlockDomain"] = configIni["BlockConfig"]["BlockDomain"],
                ["Safe"] = configIni["BlockConfig"]["Safe"],
                ["Inprivate"] = configIni["BlockConfig"]["Inprivate"],
                ["BlockAdult"] = configIni["BlockConfig"]["BlockAdult"],
                ["BlockSocial"] = configIni["BlockConfig"]["BlockSocial"],
            };
            jArray.Add(jChild);

            return jArray;
        }
        private static JArray GetDiary()
        {
            var jArray = new JArray();
            SQLiteConnection sqlite_conn = new SQLiteConnection
            {
                ConnectionString = "Data Source = " + @".\BlockDB.sqlite"
            };
            sqlite_conn.Open();
            SQLiteCommand command = new SQLiteCommand(sqlite_conn)
            {
                CommandText = string.Format("SELECT * FROM tb_DiaryApp ")
            };
            SQLiteDataReader DataReader = command.ExecuteReader();

            while (DataReader.Read())
            {
                JObject jChild = new JObject
                {
                    ["appname"] = AES.DecryptBase64ToString(DataReader["AppName"].ToString()),
                    ["timestart"] = DateTime.FromFileTime((long)DataReader["TimeStart"]).ToString(),
                    ["timeused"] = TimeSpan.FromSeconds((long)DataReader["TimeUsed"])
                };
                jArray.Add(jChild);
            }

            sqlite_conn.Close();
            return jArray;
        }

        static string SetFirebaseData(string node, string idToken, object data)
        {
            // add thêm dữ liệu nếu có, ko xoá dữ liệu khác
            string url = urlFirebase + node + ".json?auth=" + idToken;
            string jsonData = JsonConvert.SerializeObject(data);

            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                string response = client.UploadString(url, "PATCH", jsonData);
                return response;
            }
        }

        static string UpdateFirebaseData(string node, string idToken, object data)
        {
            // Xoá cmn dữ liệu khác ko có trong data gửi lên
            string url = urlFirebase + node + ".json?auth=" + idToken;
            string jsonData = JsonConvert.SerializeObject(data);

            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                string response = client.UploadString(url, "PUT", jsonData);
                return response;
            }
        }

        static dynamic GetFirebaseData(string node, string idToken)
        {
            string url = urlFirebase + node + ".json?auth=" + idToken;

            using (WebClient client = new WebClient())
            {
                string response = client.DownloadString(url);
                dynamic data = JsonConvert.DeserializeObject(response);
                return data;
            }
        }

        static string PushFirebaseData(string node, string idToken, object data)
        {
            string url = urlFirebase + node + ".json?auth=" + idToken;
            string jsonData = JsonConvert.SerializeObject(data);

            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                string response = client.UploadString(url, "POST", jsonData);
                return response;
            }
        }

        static dynamic RefreshIdToken(string refreshToken)
        {
            try
            {
                string url = urlServer + "/refresh_token";

                string jsonData = JsonConvert.SerializeObject(new {refreshToken});
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Encoding = Encoding.UTF8;
                    string response = client.UploadString(url, "POST", jsonData);
                    return response;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception RefreshIdToken : " + e.Message);
                return "";
            }
        }
    }
    class LoginSession
    {
        public string idToken = string.Empty;
        public string localId = string.Empty;
        public string refreshToken = string.Empty;
        public string expiresIn = string.Empty;
        public string email = string.Empty;
        public string kind = string.Empty;
        public string displayName = string.Empty;
        public bool registered = false;
    }
}
