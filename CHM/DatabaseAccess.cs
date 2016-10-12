using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;

namespace CHM
{
    class DatabaseAccess
    {

        private const string DATABASEQUOTE = "\"";
        protected struct DatabaseData
        {
            internal bool MainDBOpen;
            internal SQLiteConnection MainDB;
            internal Exception MainDBLastError;
        }

        private DatabaseData DBData;
        private string LastError="";

        internal DatabaseAccess()
        {
            DBData.MainDBOpen = false;
        }
        
        internal int OpenMainDB(string Directory, ref string Version, string Password)
        /// <returns> -1 if can't open Database (primary attempt)
        /// <returns>-2 cannot open because password is wrong (Secondary Attempt)
        /// <returns>0 if open successful
        {
            try
            {
                DBData.MainDB = new SQLiteConnection("Data Source=" + Directory + "\\CHMDatabase.3db; FailIfMissing=true");
                MainCHMForm MForm = new MainCHMForm();
                bool Encryptok;
                MForm.UnEncrypt(Password, out Encryptok);
                if (Encryptok)
                {

                    DBData.MainDB.SetPassword(MForm.UnEncrypt(Password, out Encryptok));

                    DBData.MainDB.Open();

                    DBData.MainDBOpen = true;
                    Version = "-SQLite Version "+SQLiteConnection.SQLiteVersion;
                }
                MForm.Close();
            }
            catch (Exception err)
            {
                DBData.MainDBOpen = false; 
                DBData.MainDBLastError=err;
                return (-1);
            }

            try//Verifyies that it is actually opened and the password is correct
            {
                SQLiteCommand mycommand = new SQLiteCommand(DBData.MainDB);
                mycommand.CommandText = "SELECT " + DATABASEQUOTE + "Configuration" + DATABASEQUOTE + " FROM sqlite_master WHERE type = " + DATABASEQUOTE + "table" + DATABASEQUOTE;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                reader.Close();
                return (0);
            }
            catch (Exception err)
            {

            }

            try
            {
                DBData.MainDB = new SQLiteConnection("Data Source=" + Directory + "\\CHMDatabase.3db; FailIfMissing=true");
                DBData.MainDB.Open();
                DBData.MainDBOpen = true;
                Version = "-SQLite Version " + SQLiteConnection.SQLiteVersion; 
                SQLiteCommand mycommand = new SQLiteCommand(DBData.MainDB);
                mycommand.CommandText = "SELECT " + DATABASEQUOTE + "Configuration" + DATABASEQUOTE + " FROM sqlite_master WHERE type = " + DATABASEQUOTE + "table" + DATABASEQUOTE;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                reader.Close();

                MainCHMForm MForm = new MainCHMForm();
                bool Encryptok=ChangeDatabasePassword(Password);
                if (!Encryptok)
                {
                    DBData.MainDB.Close();
                    DBData.MainDBOpen = false;
                    DBData.MainDBLastError = null;
                    MForm.Close();
                    return (-3);
                }
                MForm.Close();
                return (0);

            }
            catch (Exception err)
            {
                DBData.MainDBOpen = false;
                DBData.MainDBLastError = err;
                return (-2);
            }            
        }        
   

        internal bool ClearDatabasePassword()
        {
            return (ChangeDatabasePassword(""));
        }

        internal bool ChangeDatabasePassword(string Password)
        {
            try
            {
                if(Password.Length>0)
                { 
                    MainCHMForm MForm = new MainCHMForm();
                    bool Encryptok;
                    MForm.UnEncrypt(Password,out Encryptok);
                    if (!Encryptok)
                        return (false);
                    DBData.MainDB.ChangePassword(MForm.UnEncrypt(Password, out Encryptok));
                    return (true);
                }
                else
                {
                    DBData.MainDB.ChangePassword(Password);
                    return (true);
                }
            }
            catch
            {
                return (false);
            }

        }
        

        //internal int GetHTMLObjectByName(string Name, string Owner, ref string  HTMLpage)
        //{
        //    try
        //    {
        //        SQLiteCommand mycommand = new SQLiteCommand(DBData.MainDB);
        //        mycommand.CommandText = "Select * from HTMLPages where ObjectName=" + DATABASEQUOTE + Name + DATABASEQUOTE;
        //        SQLiteDataReader reader = mycommand.ExecuteReader();
        //        if (!reader.HasRows)
        //        {
        //            reader.Close();
        //            return (-1);
        //        }
        //        reader.Read();
        //        int o = reader.GetOrdinal("Object");
        //        byte[] BY = (byte[])reader[o];
        //        HTMLpage = System.Text.Encoding.UTF8.GetString(BY);
        //        return (HTMLpage.Length);
        //    }
        //    catch (Exception err)
        //    {
        //        DBData.MainDBLastError = err;
        //        return (-99); //Indicates error
        //    }
        //}

        internal bool GetPasswordInfo(string PluginNumber, string PWCode, ref string UserName, ref string Password)
        {
            try
            {
                SQLiteCommand mycommand = new SQLiteCommand(DBData.MainDB);
                mycommand.CommandText = "Select * from AccessCodes where PluginNumber=" + DATABASEQUOTE + PluginNumber + DATABASEQUOTE + " and PWCode=" + DATABASEQUOTE + PWCode + DATABASEQUOTE;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    return (false);
                }
                reader.Read();
                UserName = reader[reader.GetOrdinal("UserName")].ToString();
                Password = reader[reader.GetOrdinal("Password")].ToString();
                return (true);
            }
            catch (Exception err)
            {
                DBData.MainDBLastError = err;
                return (false); //Indicates error
            }
        }

        internal bool GetEncryptionCodesInfo(string PluginNumber, string TargetName, ref string EncryptionCode, ref string EncryptionType)
        {
            try
            {
                SQLiteCommand mycommand = new SQLiteCommand(DBData.MainDB);
                mycommand.CommandText = "Select * from EncryptionCodes where PluginNumber=" + DATABASEQUOTE + PluginNumber + DATABASEQUOTE + " and TargetName=" + DATABASEQUOTE + TargetName + DATABASEQUOTE;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    return (false);
                }
                reader.Read();
                EncryptionCode = reader[reader.GetOrdinal("EncryptionCode")].ToString();
                EncryptionType = reader[reader.GetOrdinal("EncryptionType")].ToString();
                return (true);
            }
            catch (Exception err)
            {
                DBData.MainDBLastError = err;
                return (false); //Indicates error
            }
        }

        internal int    GetMessageByCode(string Language, string ModuleSerialNumber, int Code, ref string Message, ref string Error)
        {
            try
            {
                SQLiteCommand mycommand = new SQLiteCommand(DBData.MainDB);
                mycommand.CommandText = "Select " + DATABASEQUOTE+ Language +  DATABASEQUOTE+ ", \"Error\" from Messages where ModuleSerialNumber=" + DATABASEQUOTE + ModuleSerialNumber + DATABASEQUOTE + " and MessageNumber=" + DATABASEQUOTE + Code.ToString() + DATABASEQUOTE;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                if(!reader.HasRows)
                {
                    reader.Close();
                    return(-1);
                }
                reader.Read();
                Message = reader[reader.GetOrdinal(Language)].ToString();
                Error = reader[reader.GetOrdinal("Error")].ToString();
                reader.Close();
                return (0);
            }
            catch (Exception err)
            {
                DBData.MainDBLastError = err;
                return (-1); //Indicates error
            }
        }
    
        internal int CalculateLargestField(string TableName, string FieldName)
        {
            long largest = 0, l;
            if (VerifyIfTableExists(TableName) == false)
                return (-1);
            try
            {
                SQLiteCommand mycommand = new SQLiteCommand(DBData.MainDB);
                mycommand.CommandText = "Select " + DATABASEQUOTE + FieldName +DATABASEQUOTE + " from "+ DATABASEQUOTE + TableName +DATABASEQUOTE ;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                if(!reader.HasRows)
                {
                    reader.Close();
                    return(-1);
                }
                while (reader.Read())
                {
                    l = reader.GetBytes(0, 0L, null, 0, 0);
                    if (l > largest)
                        largest = l;
                }
                reader.Close();
                return ((int) largest);
            }
            catch (Exception err)
            {
                LastError = err.Message;
                return (-1); //Indicates error
            }
        }

        internal bool LoadDictionarytable(string TableName, string Field1Name, string Field2Name, Dictionary<string, string> Dict)
        {
            if (VerifyIfTableExists(TableName) == false)
                return (false);
            try
            {
                SQLiteCommand mycommand = new SQLiteCommand(DBData.MainDB);
                mycommand.CommandText = "Select " + DATABASEQUOTE + Field1Name + DATABASEQUOTE + "," + DATABASEQUOTE + Field2Name + DATABASEQUOTE + " from " + DATABASEQUOTE + TableName + DATABASEQUOTE;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    return (false);
                }
                while (reader.Read())
                {
                    Dict[reader.GetString(0)] = reader.GetString(1);
                }
                reader.Close();
                return (true);
            }
            catch (Exception err)
            {
                LastError = err.Message;
                return (true); //Indicates error
            }
        }

        internal bool LoadConfigurationData(string ModuleSerialNumber, Func<string, string, string, char, bool, bool> SetMethod)
        {
              string S, T, C,x;
              char c;
            
             
            try
              {
                SQLiteCommand mycommand = new SQLiteCommand(DBData.MainDB);
                mycommand.CommandText = "Select * from Configuration where ModuleSerialNumber=" + DATABASEQUOTE + ModuleSerialNumber + DATABASEQUOTE;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    return (false);
                }
                while (reader.Read())
                {
                    S = "";
                    T = "";
                    c = 'S';
                    x = "";
                    C = "";

                    try
                    {
                        S = reader[1].ToString();
                        T = reader[3].ToString();
                        C = reader[4].ToString();
                        x = reader[2].ToString();
                        if (C.Length == 1)
                            c = Convert.ToChar(C);

                    }
                    catch 
                    {
                        if (S.Length == 0)
                            continue;

                    }
                    SetMethod(S, x, T, c, true);              
                }
                reader.Close();
                return (true);
            }
            catch (Exception err)
            {
                LastError = err.Message;
                return (true); //Indicates error
            }
        
        }

        internal bool LoadPluginConfigurationData(string ModuleSerialNumber, out string[] FieldName, out string[] FieldValue, out string[] SubFields)
        {
            int Count;
            SQLiteCommand mycommand = new SQLiteCommand(DBData.MainDB);

            try
            {

                mycommand.CommandText = "Select Count(ModuleSerialNumber)  from Configuration where ModuleSerialNumber=" + DATABASEQUOTE + ModuleSerialNumber + DATABASEQUOTE;
                SQLiteDataReader countreader = mycommand.ExecuteReader();
                if (!countreader.HasRows)
                {
                    countreader.Close();
                    FieldName = null;
                    FieldValue = null;
                    SubFields = null;
                    return (false);
                }
                countreader.Read();
                Count = countreader.GetInt32(0);
                FieldName = new string[Count];
                FieldValue = new string[Count];
                SubFields = new string[Count];
                countreader.Close();
            }
            catch (Exception err)
            {
                LastError = err.Message;
                FieldName = null;
                FieldValue = null;
                SubFields = null;
                return (false);
            }


            try
            {
               
                mycommand.CommandText = "Select * from Configuration where ModuleSerialNumber=" + DATABASEQUOTE + ModuleSerialNumber + DATABASEQUOTE;

                SQLiteDataReader reader = mycommand.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    FieldName = null;
                    FieldValue = null;
                    SubFields = null;
                    return (false);
                }


                int i = 0;
                while (reader.Read() && i<Count)
                {
 
                    try
                    {
                        FieldName[i] = reader.GetString(1);
                        FieldValue[i] = reader[3].ToString();
                        SubFields[i] = reader[2].ToString();
                        i++;

                    }
                    catch 
                    {
                        if (FieldName[i].Length == 0)
                            continue;

                    }
                }
                reader.Close();
                return (true);
            }
            catch (Exception err)
            {
                LastError = err.Message;
                return (false); //Indicates error
            }

        }
        
        
        internal bool AddorUpdateConfiguration(string ModuleSerialNumber, string FieldName, string Value, char ValueType)
        {

            string S = "insert or replace into Configuration values (" + DATABASEQUOTE + ModuleSerialNumber + DATABASEQUOTE + "," + DATABASEQUOTE + FieldName + DATABASEQUOTE + "," + DATABASEQUOTE + Value + DATABASEQUOTE+ "," + DATABASEQUOTE + ValueType + DATABASEQUOTE+")";
            try
            {
                SQLiteCommand mycommand = new SQLiteCommand(S, DBData.MainDB);
                mycommand.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                DBData.MainDBLastError = err;
                return (false); //Indicates error
            }
            return (true);
        }

        internal SQLiteDataReader ExecuteSQLCommandWithReader(string TableName, string Conditions, out bool ValidData)
        {
            return (ExecuteSQLCommandWithReaderandFields(TableName, "*", Conditions, out ValidData));
        }


        internal SQLiteDataReader ExecuteSQLCommandWithReaderandFields(string TableName, string fields, string Conditions, out bool ValidData)
        {
            SQLiteDataReader reader=null;
            ValidData = false;
            if (VerifyIfTableExists(TableName) == false)
            {
                return (reader);
            }

            try
            {
                SQLiteCommand mycommand = new SQLiteCommand(DBData.MainDB);
                mycommand.CommandText = "Select "+fields+ " from " + TableName;
                if(Conditions.Length>0)
                    mycommand.CommandText = mycommand.CommandText + " where " + Conditions;
                reader = mycommand.ExecuteReader();
            }
            catch (Exception err)
            {
                LastError = err.Message;
                return (null);
            }
            ValidData = true; 
            return (reader);
        
        }


        internal SQLiteDataReader GetNextRecordWithReaderWithLanguageValue(ref SQLiteDataReader reader, out string[] Fields, out bool ValidData, out string LangField, string LangName)
        {
            try
            {
                SQLiteDataReader SQLRDR = GetNextRecordWithReader(ref reader, out Fields, out ValidData);
                if (!ValidData)
                {
                    reader.Close();
                    Fields = null;
                    LangField = null;
                    return (null);
                }
                LangField = reader[reader.GetOrdinal(LangName)].ToString();
                return (SQLRDR);
            }
            catch
            {
                reader.Close();
                Fields = null;
                LangField = null;
                ValidData = false;
                return (null);
            }

        }           
 
        internal SQLiteDataReader GetNextRecordWithReader(ref SQLiteDataReader reader, out string[] Fields, out bool ValidData)
        {
            ValidData = false;
            try
              {
                if (!reader.HasRows)
                {
                    reader.Close();
                    Fields = null;
                    return (null);
                }
                if (!reader.Read())
                {
                    reader.Close();
                    Fields = null;
                    return (null);
                }
                Fields = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Fields[i] = reader[i].ToString();
                }
                ValidData = true; 
                return (reader);
              }
                catch(Exception err)
              {
                  LastError = err.Message;
                  Fields = null;
                      return (null);
              }
        }


        internal SQLiteDataReader GetNextRecordWithReader(ref SQLiteDataReader reader, out bool ValidData)
        {
            ValidData = false;
            try
            {
                if (!reader.HasRows)
                {
                    reader.Close();
                    return (null);
                }
                if (!reader.Read())
                {
                    reader.Close();
                    return (null);
                }
                ValidData = true;
                return (reader);
            }
            catch (Exception err)
            {
                LastError = err.Message;
                return (null);
            }
        }

        internal string GetStringFieldByReader(SQLiteDataReader reader, string FieldName)
        {
            try
            {
                return (reader[reader.GetOrdinal(FieldName)].ToString());
            }
            catch
            {
                return ("");
            }
  
        }

        internal bool DeleteRecord(string TableName, string FieldName, string FieldValue)
        {
            String Command = "DELETE FROM " + TableName + " WHERE " + FieldName + " = " + DATABASEQUOTE + FieldValue + DATABASEQUOTE;

            SQLiteCommand Cmd = DBData.MainDB.CreateCommand();
            Cmd.CommandText = Command.ToString();
 //           Cmd.ExecuteNonQuery();
            return (true);
        }

        internal bool WriteRecord(string TableName, string[] FieldNames, string[] FieldValues)
        {

            string t;
            StringBuilder Command = new StringBuilder("Insert or Replace into " + TableName + " (");
            foreach (string s in FieldNames)
            {
                Command.Append(s);
                Command.Append(", ");
            }
            Command.Remove(Command.Length - 2, 2);
            Command.Append(") Values (");
            foreach (string s in FieldValues)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    t = s.Replace(DATABASEQUOTE, DATABASEQUOTE + DATABASEQUOTE);
                }
                else
                {
                    t = s;
                }
                Command.Append(DATABASEQUOTE);
                Command.Append(t);
                Command.Append(DATABASEQUOTE);
                Command.Append(", ");
            }
            Command.Remove(Command.Length - 2, 2);
            Command.Append(")");
            SQLiteCommand Cmd = DBData.MainDB.CreateCommand();
            Cmd.CommandText = Command.ToString();
            Cmd.ExecuteNonQuery();
            return (true);

        }

        internal bool WriteBlobRecord( string TableName, string[] FieldNames, string[] FieldValues, string BlobName, byte[] Blob)
        {
            StringBuilder Command= new StringBuilder("Insert or Replace into "+TableName+" (");
            foreach(string s in FieldNames)
            {
                Command.Append(s);
                Command.Append(", ");
            }
            Command.Append(BlobName);

            Command.Append(") Values (");
            foreach (string s in FieldValues)
            {
                Command.Append(DATABASEQUOTE);
                Command.Append(s);
                Command.Append(DATABASEQUOTE);
                Command.Append(", ");
            }
            Command.Append("@0)");
            SQLiteCommand Cmd = DBData.MainDB.CreateCommand();
            Cmd.CommandText = Command.ToString(); 
            SQLiteParameter parameter = new SQLiteParameter("@0", System.Data.DbType.Binary);
            parameter.Value = Blob;
            Cmd.Parameters.Add(parameter);
            Cmd.ExecuteNonQuery();
            return (true);
  
        }


        internal int GetBlobFieldByReader(SQLiteDataReader reader, string FieldName, out  byte[] Blob)
        {
            try
            {
                int o = reader.GetOrdinal(FieldName);
                if(reader.IsDBNull(o))
                {
                    Blob = null;
                    return (0);
                }
                object ob = reader[o];

               // Type t = ob.GetType();

                if (ob.GetType() == typeof(Byte[]))
                {
                    Blob = (Byte[]) ob;
                    return Blob.Length;
                }

                if (ob.GetType()==typeof(string))
                {
                    Blob= (byte[]) Encoding.ASCII.GetBytes((string)ob);
                    return Blob.Length;
                }
                TypeConverter objConverter = TypeDescriptor.GetConverter(ob.GetType());
                Blob = (byte[])objConverter.ConvertTo(ob, typeof(byte[]));
                return Blob.Length;
            }
            catch (Exception err)
            {
                DBData.MainDBLastError = err;
                Blob = null;
                return (-99); //Indicates error
            }

        }


        internal void CloseNextRecordWithReader(ref SQLiteDataReader reader)
        {
            try
            {
                    if(reader!=null)
                        reader.Close();
                    return;
            }
            catch (Exception err)
            {
                LastError = err.Message;
                return;
            }
        }

        private bool VerifyIfTableExists(string TableName)
        {
            try
            {
                SQLiteCommand mycommand = new SQLiteCommand(DBData.MainDB);
                mycommand.CommandText = "SELECT name FROM sqlite_master WHERE type = " + DATABASEQUOTE + "table" + DATABASEQUOTE;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    return (false);
                }
                while (reader.Read())
                {
                    if(reader["name"].ToString().ToLower()==TableName.ToLower())
                    {
                        reader.Close();
                        return (true);
                    }
                }
                reader.Close();
                return (false);
            }
            catch (Exception err)
            {
                LastError = err.Message;
                return (false); //Indicates error
            }
        }

        internal bool GetItemByFieldsIntoString(string TableName, string[] KeyFields, string[] KeyValues, string ReturnFieldName, out string Field)
        {
            try
            {
                if (VerifyIfTableExists(TableName) == false)
                {
                    Field = "";
                    return (false);
                }
                if (KeyFields.Length == 0 || KeyValues.Length == 0 || KeyFields.Length != KeyValues.Length)
                {
                    Field = "";
                    return (false);
                }

                string DBstmt = "";
                for (int i = 0; i < KeyFields.Length; i++)
                {
                    if (string.IsNullOrEmpty(KeyValues[i]) || string.IsNullOrEmpty(KeyFields[i]))
                        continue;

                    if (DBstmt.Length > 0)
                        DBstmt = DBstmt + " and ";

                    DBstmt = DBstmt + DATABASEQUOTE + KeyFields[i] + DATABASEQUOTE + " = " + DATABASEQUOTE + KeyValues[i] + DATABASEQUOTE;
                }
                SQLiteCommand mycommand = new SQLiteCommand(DBData.MainDB);
                mycommand.CommandText = "Select * from " + TableName + " where " + DBstmt;
                SQLiteDataReader reader = mycommand.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    Field = "";
                    return (false);
                }
                try
                {
                    reader.Read();
                    int o = reader.GetOrdinal(ReturnFieldName);
                    Field = reader.GetString(o);
                    reader.Close();
                    return (true);
                }
                catch
                {
                    reader.Close();
                    Field = "";
                    return (false);

                }
            }
            catch (Exception err)
            {
                Field = "";
                return (false);
            }
        }

    }
}
