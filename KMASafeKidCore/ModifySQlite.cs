using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Diagnostics;

namespace KMASafeKidCore
{
    class ModifySQlite
    {
        public enum ObjectType
        {
            APP = 0,
            WEB
        }
        private SQLiteConnection sQLiteCon = new SQLiteConnection();
        public bool isConnect = false;
        private static String NAME_DB_SQLITE = "BlockDB.sqlite";
        public bool ConnectToDB(string sPathDB)
        { 
            try
            {
                if (!System.IO.File.Exists(sPathDB))
                {
                    SQLiteConnection.CreateFile(sPathDB);
                }
                
                sQLiteCon.ConnectionString = "Data Source = " + sPathDB;
                sQLiteCon.Open();
                isConnect = true;
                SQLiteCommand command = new SQLiteCommand(sQLiteCon);
                command.CommandText = "CREATE TABLE IF NOT EXISTS tb_Blw(ObjectName TEXT UNIQUE NOT NULL, PathApp TEXT, ObjectType INTEGER)";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE TABLE IF NOT EXISTS tb_DiaryApp(AppName TEXT NOT NULL, DayUse INTEGER, TimeStart INTEGER, TimeUsed INTEGER)";
                command.ExecuteNonQuery();
                command.Cancel();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Error Connect DB error: "+ ex.Message);
                return false;
            }
            return true;
        }
        public bool AddAppDiaryToDB(string sAppName, long TimeUsed, long DayUse)
        {
            try
            {
                if (sAppName == "" || sAppName is null) return false;
                sAppName = AES.EncryptStringToBase64(sAppName);
                SQLiteCommand command = new SQLiteCommand(sQLiteCon);
                command.CommandText = string.Format("SELECT AppName FROM tb_DiaryApp WHERE (DayUse={0} AND AppName=\"{1}\")", DayUse, sAppName);
                SQLiteDataReader DataReader = command.ExecuteReader();
                bool bUpdateFlag = false;
                if (DataReader.HasRows)
                    bUpdateFlag = true;
                else bUpdateFlag = false;

                DataReader.Close();
                if (bUpdateFlag)
                    command.CommandText = string.Format("UPDATE tb_DiaryApp SET TimeUsed=({1}+tb_DiaryApp.TimeUsed) WHERE (AppName=\"{0}\" AND DayUse={2})", sAppName, TimeUsed, DayUse);
                else
                    command.CommandText = string.Format("INSERT INTO tb_DiaryApp(AppName,TimeUsed,DayUse,TimeStart) VALUES(\"{0}\",{1},{2},{3})", sAppName, TimeUsed, DayUse,DateTime.Now.ToFileTime());
                
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Error Add Diary to DB: " + ex.Message);
                return false;
            }
            return true;
        }
        public bool AddAppBlockToDB(string sPathApp)
        {
            try
            {
                if (sPathApp is null) return false;
                string sAppName = FileVersionInfo.GetVersionInfo(sPathApp).FileDescription;
                if (sAppName is null) return false;

                sAppName = AES.EncryptStringToBase64(sAppName);
                sPathApp = AES.EncryptStringToBase64(sPathApp);

                SQLiteCommand command = new SQLiteCommand(sQLiteCon);
                command.CommandText = string.Format("INSERT INTO tb_Blw (ObjectName, PathApp, ObjectType) VALUES (\"{0}\", \"{1}\", {2})", sAppName, sPathApp, ((int)ObjectType.APP));
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error add Block To DB: {0}",e.Message);
                return false;
            }
            return true;
        }
        public bool DeleteAppFromDB(string sAppName)
        {
            if (sAppName is null) return false;
            try
            {
                sAppName = AES.EncryptStringToBase64(sAppName);
                SQLiteCommand command = new SQLiteCommand(sQLiteCon);

                command.CommandText = string.Format("DELETE FROM tb_Blw WHERE ObjectName = \"{0}\"", sAppName);
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Error Delete App from DB: " + ex.Message);
                return false;
            }
            return true;
        }

        public bool AddWebBlockToDB(string sDomain)
        {
            try
            {
                if (sDomain is null) return false;

                sDomain = AES.EncryptStringToBase64(sDomain);

                SQLiteCommand command = new SQLiteCommand(sQLiteCon);
                command.CommandText = string.Format("INSERT INTO tb_Blw (ObjectName, ObjectType) VALUES (\"{0}\",\"{1}\")", sDomain, ((int)ObjectType.WEB));
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error add Website To DB: {0}", e.Message);
                return false;
            }
            return true;
        }
        public bool DeleteWebFromDB(string sDomain)
        {
            try
            {
                if (sDomain is null) return false;
                sDomain = AES.EncryptStringToBase64(sDomain);
                SQLiteCommand command = new SQLiteCommand(sQLiteCon);

                command.CommandText = string.Format("DELETE FROM tb_Blw WHERE ObjectName = '{0}'", sDomain);
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Error Delete App from DB: " + ex.Message);
                return false;
            }
            return true;
        }
        public bool IsExistObjectInDB(string ObjectName)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(sQLiteCon);
                SQLiteDataReader DataReader;
                ObjectName = AES.EncryptStringToBase64(ObjectName);
                command.CommandText = string.Format("SELECT ObjectName FROM tb_Blw WHERE ObjectName='{0}' LIMIT 1", ObjectName);
                DataReader = command.ExecuteReader();
                if (DataReader.HasRows)
                {
                    DataReader.Close();
                    return true;
                }
                DataReader.Close();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Error Select in DB: " + ex.Message);
                return false;
            }
            return false;
        }
        public SortedSet<string> CGetAllBlockRule(ObjectType objectType)
        {
            SortedSet<string> sortList = new SortedSet<string>();
            try
            {
                SQLiteCommand command = new SQLiteCommand(sQLiteCon);
                SQLiteDataReader DataReader;

                command.CommandText = string.Format("SELECT ObjectName FROM tb_Blw WHERE ObjectType={0}", ((int)objectType));
                DataReader = command.ExecuteReader();

                while (DataReader.Read())
                {
                    sortList.Add(AES.DecryptBase64ToString(DataReader[0].ToString()));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error Get All Rule Block: {0}", e.Message);
            }
            return sortList;
        }
        public static SortedSet<string> GetAllBlockRule(ObjectType objectType)
        {
            SortedSet<string> sortList = new SortedSet<string>();
            SQLiteConnection sQ = new SQLiteConnection();
            try
            {
                sQ.ConnectionString = String.Format("Data Source = {0}\\{1}", System.IO.Directory.GetCurrentDirectory(), NAME_DB_SQLITE);
                sQ.Open();
                SQLiteCommand command = new SQLiteCommand(sQ);
                SQLiteDataReader DataReader;

                command.CommandText = string.Format("SELECT ObjectName FROM tb_Blw WHERE ObjectType={0}", ((int)objectType));
                DataReader = command.ExecuteReader();

                while (DataReader.Read())
                {
                    sortList.Add(AES.DecryptBase64ToString(DataReader[0].ToString()));
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Get All Rule Block: {0}",e.Message);
            }
            finally
            {
                sQ.Close();
            }
            return sortList;
        }
        public bool CloseConnect()
        {
            sQLiteCon.Close();
            isConnect = false;
            return true;
        }
    }
}
