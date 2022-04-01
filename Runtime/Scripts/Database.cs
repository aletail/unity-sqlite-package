
using UnityEngine;
using System.Data;
using System;
using System.IO;
using Mono.Data.Sqlite;

public class Database {

    public string DatabaseFileName = "";

    IDbConnection dbconn;
    IDbCommand dbcmd;
    IDataReader reader;

    /*
    * Database Constructors
    */
    public Database() { }

    /*
    * Database
    */
    public Database(string SaveFileName)
    {
        this.DatabaseFileName = SaveFileName;
        this.OpenDatabase(SaveFileName);
    }

    /*
    * Open Database
    */
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

    /*
    * Execute Query
    */
    public IDataReader ExecuteQuery(string sqlQuery)
    {
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = sqlQuery;
        reader = dbcmd.ExecuteReader();
        return reader;
    }


    /*
    * Close Database
    */
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

}