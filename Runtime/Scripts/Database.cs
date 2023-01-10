
using UnityEngine;
using System.Data;
using System;
using System.IO;
using Mono.Data.Sqlite;
using System.Linq;
using System.Collections.Generic;

namespace Aletail.General {
    /// <summary>
    /// Database Class, used to manage a database
    /// </summary>
    public class Database {

        public string DatabaseFileName = "";

        IDbConnection dbconn;
        IDbCommand dbcmd;
        IDataReader reader;

        /// <summary>
        /// Constructor
        /// </summary>
        public Database() { }

        /// <summary>
        /// Database constructor, opens requested database
        /// </summary>
        /// <param name="SaveFileName"></param>
        public Database(string SaveFileName)
        {
            this.DatabaseFileName = SaveFileName;
            this.OpenDatabase(SaveFileName);
        }

        /// <summary>
        /// Opens the database (SaveFileName), creates it if necessary
        /// </summary>
        /// <param name="SaveFileName"></param>
        /// <param name="Stream"></param>
        public void OpenDatabase(string SaveFileName = "", Boolean Stream = false)
        {
            if (this.DatabaseFileName == "")
            {
                this.DatabaseFileName = SaveFileName;
                //Debug.Log("Database Name Set: " + this.DatabaseFileName);
            }

            // The following block of code for locating the referenced database file was taken from somewhere it is probably out of date. If you know the original author please contact
            // https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
            string filepath = Application.persistentDataPath + "/" + this.DatabaseFileName;
            if (!File.Exists(filepath))
            {
                if (Stream)
                {
#if UNITY_ANDROID
                //COPY EXISTING
                var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + this.DatabaseFileName);  // this is the path to your StreamingAssets in android
                while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
                                            // then save to Application.persistentDataPath
                File.WriteAllBytes(filepath, loadDb.bytes); 
#endif
#if UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_WIN
                //COPY EXISTING
                string _loaddb = Application.streamingAssetsPath + "/" + this.DatabaseFileName;
                File.Copy(_loaddb, filepath, true);
#endif
                }
                else
                {
#if UNITY_ANDROID
                //COPY EXISTING
                var loadDb = new WWW("jar:file://" + Application.persistentDataPath + "!/assets/" + this.DatabaseFileName);  // this is the path to your StreamingAssets in android
                while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
                                            // then save to Application.persistentDataPath
                File.WriteAllBytes(filepath, loadDb.bytes);
#endif
                }
            }
            String conn = "URI=file:" + filepath;
            dbconn = new SqliteConnection(conn);
            dbconn.Open();
        }

        /// <summary>
        /// Performs the ExecuteReader command
        /// </summary>
        /// <param name="sqlQuery"></param>
        /// <returns>IDataReader</returns>
        public IDataReader ExecuteQuery(string sqlQuery)
        {
            try
            {
                //Debug.Log(sqlQuery);
                dbcmd = dbconn.CreateCommand();
                dbcmd.CommandText = sqlQuery;
                reader = dbcmd.ExecuteReader();
                return reader;
            }
            catch (Exception e)
            {
                throw new Exception(
                    string.Format("Error with: " + sqlQuery)
                );
            }

        }
        
        /// <summary>
        /// Performs the ExecuteScalar command
        /// </summary>
        /// <param name="sqlQuery"></param>
        /// <returns>System.object</returns>
        public object ExecuteScalarQuery(string sqlQuery)
        {
            dbcmd = dbconn.CreateCommand();
            dbcmd.CommandText = sqlQuery;
            return dbcmd.ExecuteScalar();
        }

        /// <summary>
        /// Closes the database connection
        /// </summary>
        public void CloseDatabase()
        {
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
            if (dbcmd != null)
            {
                dbcmd.Dispose();
                dbcmd = null;
            }
            dbconn.Close();
            dbconn = null;
        }

        /// <summary>
        /// Create Table If Not Exists
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="Data"></param>
        public void CreateTableIfNotExists(string TableName, Dictionary<string, Dictionary<string, string>> Data)
        {
            string columns = "";
            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in Data)
            {
                //Debug.Log("Key = "+ kvp.Key + ", Value = " + kvp.Value);
                columns += "" + kvp.Key + " " + kvp.Value["Type"] + "";
                if (kvp.Key != Data.Last().Key)
                {
                    columns += ",";
                }
            }

            //CREATE TABLE IF NOT EXISTS Character (ID INTEGER PRIMARY KEY, FirstName TEXT, LastName TEXT, SuperName TEXT, Job TEXT, Age INTEGER
            string query = "CREATE TABLE IF NOT EXISTS " + TableName + "(" + columns + ");";
            //Debug.Log(query);
            this.ExecuteQuery(query);
        }


        public Dictionary<string, string> GetByID(string TableName, int ID)
        {
            var data = new Dictionary<string, string>();

            string query = "SELECT * FROM " + TableName + " WHERE ID = " + ID;
            IDataReader result = this.ExecuteQuery(query);

            while (result.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetFieldType(i).Name == "Int64") // TODO, when could int32 come into play here
                    {
                        data.Add(reader.GetName(i), reader.GetInt64(i).ToString());
                    }
                    else if (reader.GetFieldType(i).Name == "String")
                    {
                        data.Add(reader.GetName(i), reader.GetString(i));
                    }
                    else if (reader.GetFieldType(i).Name == "Single") //TODO: Real is sqlite
                    {
                        data.Add(reader.GetName(i), reader.GetFloat(i).ToString());
                    }
                }
            }

            return data;
        }

        public Dictionary<string, string> GetFirst(string TableName, string SQL)
        {
            var data = new Dictionary<string, string>();

            string query = "SELECT * FROM " + TableName + " " + SQL;
            IDataReader result = this.ExecuteQuery(query);

            while (result.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetFieldType(i).Name == "Int64") // TODO, when could int32 come into play here
                    {
                        data.Add(reader.GetName(i), reader.GetInt64(i).ToString());
                    }
                    else if (reader.GetFieldType(i).Name == "String")
                    {
                        data.Add(reader.GetName(i), reader.GetString(i));
                    }
                    else if (reader.GetFieldType(i).Name == "Single") //TODO: Real is sqlite
                    {
                        data.Add(reader.GetName(i), reader.GetFloat(i).ToString());
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public List<IDictionary<string, string>> Select(string TableName, int ID)
        {
            var list = new List<IDictionary<string, string>>();

            string query = "SELECT * FROM " + TableName + " WHERE ID = " + ID;
            IDataReader result = this.ExecuteQuery(query);

            while (result.Read())
            {
                var data = new Dictionary<string, string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetFieldType(i).Name == "Int64") // TODO, when could int32 come into play here
                    {
                        data.Add(reader.GetName(i), reader.GetInt64(i).ToString());
                    }
                    else if (reader.GetFieldType(i).Name == "String")
                    {
                        data.Add(reader.GetName(i), reader.GetString(i));
                    }
                    else if (reader.GetFieldType(i).Name == "Single") //TODO: Real is sqlite
                    {
                        data.Add(reader.GetName(i), reader.GetFloat(i).ToString());
                    }
                }
                list.Add(data);
            }


            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Query"></param>
        /// <returns></returns>
        public List<IDictionary<string, string>> SelectQuery(string Query)
        {
            var list = new List<IDictionary<string, string>>();

            IDataReader result = this.ExecuteQuery(Query);

            while (result.Read())
            {
                var data = new Dictionary<string, string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetFieldType(i).Name == "Int64") // TODO, when could int32 come into play here
                    {
                        data.Add(reader.GetName(i), reader.GetInt64(i).ToString());
                    }
                    else if (reader.GetFieldType(i).Name == "String")
                    {
                        data.Add(reader.GetName(i), reader.GetString(i));
                    }
                    else if (reader.GetFieldType(i).Name == "Single") //TODO: Real is sqlite
                    {
                        data.Add(reader.GetName(i), reader.GetFloat(i).ToString());
                    }
                }
                list.Add(data);
            }


            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public List<IDictionary<string, string>> Select(string TableName, string Where)
        {
            var list = new List<IDictionary<string, string>>();

            string query = "SELECT * FROM " + TableName + " " + Where;
            IDataReader result = this.ExecuteQuery(query);

            while (result.Read())
            {
                var data = new Dictionary<string, string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetFieldType(i).Name == "Int64") // TODO, when could int32 come into play here
                    {
                        data.Add(reader.GetName(i), reader.GetInt64(i).ToString());
                    }
                    else if (reader.GetFieldType(i).Name == "String")
                    {
                        data.Add(reader.GetName(i), reader.GetString(i));
                    }
                    else if (reader.GetFieldType(i).Name == "Single") //TODO: Real is sqlite
                    {
                        data.Add(reader.GetName(i), reader.GetFloat(i).ToString());
                    }
                }
                list.Add(data);
            }


            return list;
        }

        /// <summary>
        /// Insert into the database
        /// </summary>
        /// <param name="TableName">String name of the table to insert into</param>
        /// <param name="Data">Associated Array</param>
        /// <returns>ID of the row</returns>
        public int Insert(string TableName, Dictionary<string, Dictionary<string, string>> Data)
        {
            this.CreateTableIfNotExists(TableName, Data);
            string query = "";
            string columns = "(";
            string values = "(";
            if (Data.Count > 1)
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> kvp in Data)
                {
                    //Debug.Log("Key = "+ kvp.Key + ", Value = " + kvp.Value);
                    if (kvp.Key != "ID")
                    {
                        columns += "`" + kvp.Key + "`";
                        values += "\"" + kvp.Value["Value"] + "\"";

                        if (kvp.Key != Data.Last().Key)
                        {
                            columns += ",";
                            values += ",";
                        }
                    }
                }
                columns += ")";
                values += ")";
                query = "INSERT INTO " + TableName + " " + columns + " VALUES " + values + " returning ID;";
            }
            else
            {
                Debug.Log("No properties to insert");
            }
            

            // Execute Query
            IDataReader result = this.ExecuteQuery(query);

            // Retrieve and return the ID
            Int64 LastRowID64 = (Int64)this.ExecuteScalarQuery("SELECT last_insert_rowid()");
            return (int)LastRowID64;
        }

        /// <summary>
        /// Updates the requested table (TableName)
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="ID"></param>
        /// <param name="Data"></param>
        public void Update(string TableName, int ID, Dictionary<string, Dictionary<string, string>> Data)
        {
            this.CreateTableIfNotExists(TableName, Data);

            string columns = "";
            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in Data)
            {
                //Debug.Log("Key = "+ kvp.Key + ", Value = " + kvp.Value);
                if (kvp.Key != "ID")
                {
                    columns += kvp.Key + "=" + "\"" + kvp.Value["Value"] + "\"";
                    if (kvp.Key != Data.Last().Key)
                    {
                        columns += ",";
                    }
                }
            }

            // Execute Query
            string query = "UPDATE " + TableName + " SET " + columns + " WHERE ID = " + ID;
            //Debug.Log(query);
            IDataReader result = this.ExecuteQuery(query);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="ID"></param>
        public void Delete(string TableName, int ID)
        {
            // Execute Query
            string query = "DELETE FROM " + TableName + " WHERE ID = " + ID;
            IDataReader result = this.ExecuteQuery(query);
        }


    }
}