using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using uts = UISafekids.Source.UtilsFunction;
using UISafekids.Source;
using Newtonsoft.Json;
using IniParser;
using IniParser.Model;
using System.IO;
using UISafekids.Views;
using Microsoft.Win32;
using System.Net;

namespace UISafekids.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly string keynamePwd = "pwd";
        private FileIniDataParser iniFile = new FileIniDataParser();
        private static readonly string path_login_config = ".\\login.ini";

        public LoginWindow()
        {
            InitializeComponent();
            CNetwork.InitNetwork();
            if (!File.Exists(path_login_config))
            {
                File.Create(path_login_config).Close();
            }

            if (!CheckLoginExists()) // nếu chưa login
            {
                return;
            }

            // Đã login, chuyển gui và close
            PasswordLocal passwordLocal = new();
            passwordLocal.Show();
            Close();
        }

        private async void Button_Click_Login(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(email.Text) || string.IsNullOrEmpty(password.Password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin");
                return;
            }

            // Server da tra ve thong tin login
            try
            {
                LoginSession? login;
                Task<string> taskLoginInfo = CNetwork.LoginWithEmailAsync(email.Text, password.Password);
                string loginInfo = await taskLoginInfo;
                if (String.IsNullOrEmpty(loginInfo))
                {
                    MessageBox.Show("Sai mật khẩu");
                    return;
                }

                login = JsonConvert.DeserializeObject<LoginSession>(loginInfo);

                if (login is null)
                {
                    MessageBox.Show("LoginSession null");
                    return;
                }

                string hashPwd = SHA.EncoderSHA256(password.Password);
                uts.WriteToSafeKidKey("email", login.email); // write tai khoan
                uts.WriteToSafeKidKey(keynamePwd, hashPwd); // write mat khau hash

                // create ini
                IniData data = iniFile.ReadFile(path_login_config);
                if (login != null)
                {
                    data["login"]["idToken"] = login.idToken;
                    data["login"]["localId"] = login.localId;
                    data["login"]["refreshToken"] = login.refreshToken;
                    data["login"]["email"] = login.email;
                }

                iniFile.WriteFile(path_login_config, data);
                MainView mainView = new();
                mainView.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show ("Login exception:" + ex.Message);
                return;
            }

            return;
        }

        private bool CheckLoginExists()
        {
            IniData data = iniFile.ReadFile(path_login_config);
            if (data["login"]["idToken"] != null)
            {
                return true;
            }
            return false;
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
