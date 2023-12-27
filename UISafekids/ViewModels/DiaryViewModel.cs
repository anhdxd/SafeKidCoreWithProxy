using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UISafekids.Models;
using System.Data.SQLite;
using UISafekids.Source;
namespace UISafekids.ViewModels
{
    class DiaryViewModel : ViewModelBase
    {
        // Là controller giữa view và model
        private ObservableCollection<DetailModel>? _detailModel { get; set; }
        private static readonly string CONFIG_NAME = "config.ini";

        private static SQLiteConnection sQLiteCon = new SQLiteConnection();

        //Properties
        public ObservableCollection<DetailModel> DetailModel
        {
            get
            {
                return _detailModel;
            }
            set
            {
                _detailModel = value;
                OnPropertyChanged(nameof(DetailModel));
            }
        }
        public DiaryViewModel()
        {
            DetailModel = new ObservableCollection<DetailModel>();

            sQLiteCon.ConnectionString = "Data Source = " + @".\BlockDB.sqlite";
            sQLiteCon.Open();
            SQLiteCommand command = new SQLiteCommand(sQLiteCon);
            command.CommandText = string.Format("SELECT * FROM tb_DiaryApp ");
            SQLiteDataReader DataReader = command.ExecuteReader();
            DateTime dStart = new DateTime();
            TimeSpan timeSpanUsed = new TimeSpan();

            if (DetailModel.Count > 0)
                DetailModel.Clear();

            while (DataReader.Read())
            {
                dStart = DateTime.FromFileTime((long)DataReader["TimeStart"]);
                timeSpanUsed = TimeSpan.FromSeconds((long)DataReader["TimeUsed"]);
                App.Current.Dispatcher.Invoke(() =>
                {
                    DetailModel.Add(new DetailModel() { ID = "0", AppName = AES.DecryptBase64ToString(DataReader["AppName"].ToString()), TimeStart = dStart.ToString(), TimeUsed = timeSpanUsed.ToString() + "s" });
                });
            }

            sQLiteCon.Close();
        }
    }
}
