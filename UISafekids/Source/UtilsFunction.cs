using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Windows;
using Microsoft.Win32;

namespace UISafekids.Source
{
    public static class UtilsFunction
    {
        // Get http request use System.net
        public static async Task<HttpResponseMessage> GetResponse(string url)
        {
            using HttpClient client = new();
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        // Post json data to server
        public static async Task<HttpResponseMessage> PostJsonData(string url, string json)
        {
            using HttpClient client = new();
            try
            {
                HttpResponseMessage response = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public static bool WriteToSafeKidKey(string name, string value, RegistryValueKind valueKind = RegistryValueKind.String)
        {
            try
            {
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey subkey = key.CreateSubKey("SOFTWARE\\KMASafe");

                subkey.SetValue(name, value);

                subkey.Close();
                key.Close();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Write To safekid reg failed" + e.Message);
                return false;
                
            }
        }
        public static object ReadSafeKidKey(string name)
        {
            try
            {
                object value;
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey subkey = key.CreateSubKey("SOFTWARE\\KMASafe");

                value = subkey.GetValue(name);

                subkey.Close();
                key.Close();
                return value;
            }
            catch (Exception e)
            {
                MessageBox.Show("Get safekid reg failed: " + e.Message);
                return "";

            }
        }
    }
}
