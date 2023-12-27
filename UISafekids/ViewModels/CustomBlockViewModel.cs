using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UISafekids.Models;
using UISafekids.Source;

namespace UISafekids.ViewModels
{
    class CustomBlockViewModel : ViewModelBase
    {
        // Là controller giữa view và model
        private ObservableCollection<CustomBlockModel> _customBlockModel { get; set; }
        private ModifySQlite SQlite = new ModifySQlite();

        //public ObservableCollection<DetailModel> DetailModel { get; set; }

        //Properties
        public ObservableCollection<CustomBlockModel> CustomBlockModel
        {
            get
            {
                return _customBlockModel;
            }
            set
            {
                _customBlockModel = value;
                OnPropertyChanged(nameof(CustomBlockModel));
            }
        }
        public CustomBlockViewModel()
        {
            SQlite.ConnectToDB(ModifySQlite.PATH_DB);
            SortedSet<string> ssUserApp = SQlite.CGetAllBlockRule(ModifySQlite.ObjectType.APP);
            SortedSet<string> ssUserHost = SQlite.CGetAllBlockRule(ModifySQlite.ObjectType.WEB);
            SQlite.CloseConnect();

            CustomBlockModel = new ObservableCollection<CustomBlockModel>();
            foreach (string app in ssUserApp)
            {
                CustomBlockModel.Add(new CustomBlockModel { ID = "0", AppName = app, TimeBlock = "00:00", TypeApp = "App" });
            }
            foreach (string host in ssUserHost)
            {
                CustomBlockModel.Add(new CustomBlockModel { ID = "0", AppName = host, TimeBlock = "00:00", TypeApp = "Web" });
            }
            //CustomBlockModel.Add(new CustomBlockModel { ID = "0", AppName = "youtube.com", TimeBlock = "10:00", TypeApp = "App" });
            //CustomBlockModel.Add(new CustomBlockModel { ID = "0", AppName = "facebook.com", TimeBlock = "10:00", TypeApp = "App" });
            //CustomBlockModel.Add(new CustomBlockModel { ID = "0", AppName = "zalo.me", TimeBlock = "10:00", TypeApp = "Web" });

        }
    }
}
