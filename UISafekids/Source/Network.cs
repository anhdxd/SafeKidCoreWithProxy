using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace UISafekids.Source
{
    static class CNetwork
    {

        private static HttpClientHandler handler = new HttpClientHandler();
        private static readonly string urlServer = "https://127.0.0.1:443";
        private static readonly HttpClient clientFirebase = new HttpClient();
        private static HttpClient? client;
        private static readonly string urlFirebase = "https://anhdz-3fe31-default-rtdb.firebaseio.com/";

        static async Task<Dictionary<string, object>> GetFirebaseData(string node, string idToken)
        {
            string url = $"{urlFirebase}{node}.json?auth={idToken}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                string json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                }
                else
                {
                    throw new Exception($"Failed to retrieve data: {json}");
                }
            }
        }

        public static void InitNetwork()
        {
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            client = new(handler);
        }

        public static async Task<string> LoginWithEmailAsync(string email, string password)
        {
            string url = urlServer + "/login";
            var jsonObject = new
            {
                email,
                password,
            };

            try
                {

                // Post json data to server
                string jsonData = JsonConvert.SerializeObject(jsonObject);
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                return responseContent;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception login: " + ex.Message);
                return "";
            }
        }

        // create get request response string
        public static string GetRequest(string url)
        {
            // Get json data from server
            var response = clientFirebase.GetAsync(url);

            // Get response string
            var responseString = response.Result.Content.ReadAsStringAsync().Result;

            return responseString;
        }

        // create post request response string
        public static string PostRequest(string url, List<KeyValuePair<string, string>> parameters)
        {
            // Post json data to server
            var content = new FormUrlEncodedContent(parameters);
            var response = clientFirebase.PostAsync(url, content);

            // Get response string
            var responseString = response.Result.Content.ReadAsStringAsync().Result;

            return responseString;
        }
    }
}
