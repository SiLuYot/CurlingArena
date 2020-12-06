using SQLiteLoadHelper;
using SQLiteLoadHelper.Database;
using SQLiteLoadHelper.Sqlite;
using System;
using System.IO;
using UnityEngine;

public class DataBaseManager : MonoBehaviour
{
    private static DataBaseManager instance = null;
    public static DataBaseManager Instance
    {
        get
        {
            //인스턴스가 없다면 씬에서 찾는다.
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(DataBaseManager)) as DataBaseManager;
            }

            //인스턴스가 찾아도 없다면 새로 만든다.
            if (instance == null)
            {
                var newObj = new GameObject("DataBaseManager");
                instance = newObj.AddComponent<DataBaseManager>();
            }

            //초기화가 안된경우 초기화
            if (instance != null && !instance.IsInit)
            {
                instance.Init();
            }

            return instance;
        }
    }

    public DataBaseLoader loader;

    public event Action EndDataLoadEvent;

    public bool IsInit { get; private set; }

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Init()
    {
        if (!IsInit)
        {
            IsInit = true;

            DataLoadStart();            
        }        
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

        EndDataLoadEvent?.Invoke();
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