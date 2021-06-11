using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using System.Data.Common;

static class MyDataBase
{
    private const string fileName = "db.db";
    private static string DBPath;
    private static SqliteConnection connection;
    private static SqliteCommand command;

    static MyDataBase()
    {
        DBPath = GetDatabasePath();
    }

    private static string GetDatabasePath()
    {
    #if UNITY_EDITOR
        return Path.Combine(Application.streamingAssetsPath, fileName);
    #elif UNITY_STANDALONE
        string filePath = Path.Combine(Application.dataPath + "/StreamingAssets" ,fileName);
        if (!File.Exists(filePath)) UnpackDatabase(filePath);
        return filePath;
    #endif
    }

    private static void UnpackDatabase(string toPath)
    {
        string fromPath = Path.Combine(Application.streamingAssetsPath, fileName);

        WWW reader = new WWW(fromPath);
        while (!reader.isDone) { }

        File.WriteAllBytes(toPath, reader.bytes);
    }

    private static void OpenConnection()
    {
        connection = new SqliteConnection("Data Source=" + DBPath);
        command = new SqliteCommand(connection);
        connection.Open();
    }

    public static void CloseConnection()
    {
        connection.Close();
        command.Dispose();
    }

    public static void RegisterNewPlayer(string query)
    {
        OpenConnection();
        command.CommandText = query;
        command.ExecuteNonQuery();
        CloseConnection();
    }

    public static List<DbDataRecord> GetDataPlayer(string query)
    {
        OpenConnection();
        command.CommandText = query;
        List<DbDataRecord> dataPlayer = new List<DbDataRecord>();
        SqliteDataReader dataDB = command.ExecuteReader();
        foreach(DbDataRecord reader in dataDB)
        {
            dataPlayer.Add(reader);
        }
        CloseConnection();
        return dataPlayer;
    }

}