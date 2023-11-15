using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KMASafeKidCore
{
    public class Utils
    {
        public static string EncoderSHA256(string dataenc)
        {
            string input = dataenc;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return hash;
            }
        }
        public static string GetPcIdentify()
        {
            // Lấy thông tin hệ thống
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            ManagementObjectCollection collection = searcher.Get();

            String serialmainBoard = "";
            String macAddress = "";

            searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE MACAddress IS NOT NULL");
            ManagementObjectCollection serialPC = searcher.Get();

            foreach (ManagementObject obj in collection)
            {
                serialmainBoard = obj["SerialNumber"].ToString();
            }

            foreach (var item in serialPC)
            {
                macAddress = item["MACAddress"].ToString();
            }
            string pcId = Utils.EncoderSHA256(serialmainBoard + macAddress);
            return pcId is null ? "" : pcId;
        }

        public static string GetRegistry(string keyName)
        {
            using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey subkey = key.CreateSubKey("SOFTWARE\\KMASafe"))
                {
                    return (subkey.GetValue(keyName) == null) ? "" : subkey.GetValue(keyName).ToString();
                }
            }
        }

        public static bool AddRegistry(string keyName, string value)
        {
            using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey subkey = key.CreateSubKey("SOFTWARE\\KMASafe"))
                {
                    subkey.SetValue(keyName, value);
                    return true;
                }
            }
        }
    }
}
