
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
                Debug.Log("Database Name Set: " + this.DatabaseFileName);
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
            dbcmd = dbconn.CreateCommand();
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            return reader;
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

        /// <summary>
        /// Insert into the database
        /// </summary>
        /// <param name="TableName">String name of the table to insert into</param>
        /// <param name="Data">Associated Array</param>
        /// <returns>ID of the row</returns>
        public int Insert(string TableName, Dictionary<string, Dictionary<string, string>> Data)
        {
            string columns = "(";
            string values = "(";
            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in Data)
            {
                //Debug.Log("Key = "+ kvp.Key + ", Value = " + kvp.Value);
                if(kvp.Key != "ID")
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
            columns+= ")";
            values += ")";

            // Execute Query
            string query = "INSERT INTO " + TableName + " " + columns + " VALUES " + values + " returning ID;";
            //Debug.Log(query);
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


    }
}