using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UISafekids.Models;

namespace UISafekids.ViewModels
{
    class SettingViewModel : ViewModelBase
    {
        // Là controller giữa view và model
        private ObservableCollection<SettingModel> _settingModel { get; set; }

        //public ObservableCollection<DetailModel> DetailModel { get; set; }

        //Properties
        public ObservableCollection<SettingModel> SettingModel
        {
            get
            {
                return _settingModel;
            }
            set
            {
                _settingModel = value;
                OnPropertyChanged(nameof(SettingModel));
            }
        }
        public SettingViewModel()
        {
            SettingModel = new ObservableCollection<SettingModel>();

        }
    }
}
