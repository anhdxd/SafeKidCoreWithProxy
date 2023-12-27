using Microsoft.Win32;
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
using UISafekids.Views;
using UISafekids.Source;
using uts = UISafekids.Source.UtilsFunction;
namespace UISafekids.Windows
{
    /// <summary>
    /// Interaction logic for PasswordLocal.xaml
    /// </summary>
    public partial class PasswordLocal : Window
    {
        private readonly string keynameEmail = "email";
        private readonly string keynamePwd = "pwd";
        public PasswordLocal()
        {
            InitializeComponent();

            notify.Text = "";
            login_button.Visibility = Visibility.Visible;
            string? email = uts.ReadSafeKidKey(keynameEmail) as string;
            txtUser.Text = email;
        }

        private bool IsPasswordExists()
        {
            try
            {
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey subkey = key.CreateSubKey("SOFTWARE\\KMASafe");

                if (subkey.GetValue(keynamePwd) == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Button_Click_Login(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(pwdBox.Password))
            {
                notify.Text = "Vui lòng nhập mật khẩu";
                return;
            }

            try
            {
                string? hashPwd = uts.ReadSafeKidKey(keynamePwd) as string;
                if (String.IsNullOrEmpty(hashPwd))
                    return;

                // Hash SHA256
                string hashPwdInput = SHA.EncoderSHA256(pwdBox.Password);
                if (String.IsNullOrEmpty(hashPwdInput))
                {
                    MessageBox.Show("Hash password failed");
                    return;
                }

                // Compare hash
                if (hashPwdInput != hashPwd)
                {
                    notify.Text = "Sai mật khẩu, vui lòng nhập lại";
                    return;
                }

                MainView mainView = new MainView();
                mainView.Show();
                Close();
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void Create_login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey subkey = key.CreateSubKey("SOFTWARE\\KMASafe");

                // Hash SHA256
                string hashPwd = SHA.EncoderSHA256(pwdBox.Password);

                subkey.SetValue(keynamePwd, hashPwd);

                login_button.Visibility = Visibility.Visible;
                notify.Text = "Nhập mật khẩu để đăng nhập";
            }
            catch (Exception)
            {
                MessageBox.Show("Create password failed");
                throw;
            }
        }
    }
}
