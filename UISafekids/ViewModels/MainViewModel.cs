using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using UISafekids.Models;
using UISafekids.ViewModels;

namespace UISafekids.ViewModels
{
    class MainViewModel:ViewModelBase
    {
        //Fields
        //private UserAccountModel _currentUserAccount;
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;
        //private IUserRepository userRepository;

        //Properties
        public ViewModelBase CurrentChildView
        {
            get
            {
                return _currentChildView;
            }
            set
            {
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }
        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                _caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }
        public IconChar Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        //--> Commands
        public ICommand ShowHomeViewCommand { get; }
        public ICommand ShowDiaryViewCommand { get; }
        public ICommand ShowAppInstallViewCommand { get; }
        public ICommand ShowCustomBlockViewCommand { get; }
        public ICommand ShowSettingViewCommand { get; }
        public ICommand ShowDeviceConnectViewCommand { get; }


        public MainViewModel()
        {
            //CurrentDetailModel = new DetailModel("Dashboard");
            //Initialize commands
            ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            ShowDiaryViewCommand = new ViewModelCommand(ExecuteShowCustomerViewCommand);
            ShowAppInstallViewCommand = new ViewModelCommand(ExecuteShowAppInstallViewCommand);
            ShowCustomBlockViewCommand = new ViewModelCommand(ExecuteShowCustomBlockViewCommand);
            ShowSettingViewCommand = new ViewModelCommand(ExecuteShowSettingViewCommand);
            ShowDeviceConnectViewCommand = new ViewModelCommand(ExecuteShowDeviceConnectViewCommand);

            //Default view
            ExecuteShowHomeViewCommand(null);
            //ExecuteShowCustomerViewCommand(null);
            //LoadCurrentUserData();
        }
        private void ExecuteShowHomeViewCommand(object obj)
        {
            CurrentChildView = new HomeViewModel();
            Caption = "Dashboard";
            Icon = IconChar.Home;
        }
        private void ExecuteShowCustomerViewCommand(object obj)
        {
            CurrentChildView = new DiaryViewModel();
            Caption = "Diary Detail";
            Icon = IconChar.List;
        }
        private void ExecuteShowAppInstallViewCommand(object obj)
        {
            CurrentChildView = new AppInstalledViewModel();
            Caption = "App installed";
            Icon = IconChar.AppStore;
        }
        private void ExecuteShowCustomBlockViewCommand(object obj)
        {
            CurrentChildView = new CustomBlockViewModel();
            Caption = "Custom Block";
            Icon = IconChar.Ban;
        }
        private void ExecuteShowSettingViewCommand(object obj)
        {
            CurrentChildView = new SettingViewModel();
            Caption = "Setting";
            Icon = IconChar.Gear;
        }
        private void ExecuteShowDeviceConnectViewCommand(object obj)
        {
            CurrentChildView = new DeviceConnectViewModel();
            Caption = "Account Info";
            Icon = IconChar.Connectdevelop;
        }
    }
}

