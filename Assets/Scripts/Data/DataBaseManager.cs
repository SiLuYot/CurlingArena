﻿using SQLiteLoadHelper;
using SQLiteLoadHelper.Database;
using SQLiteLoadHelper.Sqlite;
using System.IO;
using UnityEngine;

public class DataBaseManager : MonoBehaviour
{

    private static DataBaseManager instance = null;
    public static DataBaseManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(DataBaseManager)) as DataBaseManager;

            return instance;
        }
    }

    public DataBaseLoader loader;

    public void Start()
    {
        DataLoadStart();
    }

    public void DataLoadStart()
    {
        if (loader == null)
            loader = new DataBaseLoader("CurlingArena", new MonoSQLite());

        IDataBase[] dataBase = new IDataBase[]
        {            
            new AffiliationDataBase(),
            new CharacterDataBase(),
            new JobDataBase(),
            new RarityDataBase(),
            new SizeDataBase(),
            new SkillDataBase(),
            new SpeciesDataBase(),
        };

        loader.Process(dataBase);

        string msg = loader.ErrorMsg;
        if (msg != string.Empty)
        {
            Debug.Log(msg);
        }
    }
}

public class DataBaseLoader : SQLiteLoadHelperClass
{
    public DataBaseLoader(string projectName, ISQLIte usingSqlite) : base(projectName, usingSqlite)
    {

    }

    public override string ReadVersionFile(string path)
    {
        var filePath = Application.streamingAssetsPath + "/" + path;

        if (Application.platform == RuntimePlatform.Android)
        {
            var www = new WWW(filePath);
            while (!www.isDone)
            {

            }

            return www.text;
        }
        else
        {
            return base.ReadVersionFile(filePath);
        }
    }

    public override string GetDBFullPath(string path)
    {
        var filePath = Application.streamingAssetsPath + "/" + path;
        var copyFilePath = Application.persistentDataPath + "/" + path;

        if (Application.platform == RuntimePlatform.Android)
        {            
            var www = new WWW(filePath);
            while (!www.isDone) 
            { 
            
            }

            var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(copyFilePath));
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            File.WriteAllBytes(copyFilePath, www.bytes);

            return copyFilePath;
        }
        else return base.GetDBFullPath(filePath);
    }
}