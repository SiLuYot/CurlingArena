using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance = null;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(UIManager)) as UIManager;

            return instance;
        }
    }

    public Camera UICamera;
    public UIRoot uiRoot;

    private Dictionary<Type, UIData> uiData = new Dictionary<Type, UIData>();
    private Dictionary<int, UIBase> openedUI = new Dictionary<int, UIBase>();

    private void Start()
    {
        AddUIPath(UIData.ShootUI);
    }

    private void AddUIPath(UIData data)
    {
        uiData.Add(data.type, data);
    }

    public UIBase Get<T>() where T : UIBase
    {
        UIBase loadObj = null;

        if (uiData.ContainsKey(typeof(T)))
        {
            var data = uiData[typeof(T)];
            if (openedUI.ContainsKey(data.id))
            {
                loadObj = openedUI[data.id];
            }
            else
            {
                var loadGameObj = Instantiate(Resources.Load(data.path), uiRoot.transform) as GameObject;
                if (loadGameObj == null)
                {
                    Debug.LogError(string.Format("{0} 잘못된 경로", data.path));
                }

                loadObj = loadGameObj.GetComponent<UIBase>();
                if (loadObj == null)
                {
                    Debug.LogError(string.Format("UIBase 컴포넌트가 없거나 UIBase를 상속받은 UI가 아님"));
                }

                loadObj.Init(data);
                openedUI.Add(data.id, loadObj);
            }

        }
        else Debug.LogError(string.Format("{0} 타입의 UI DATA 없음", typeof(T)));

        return loadObj;
    }

    public void Close(int id)
    {
        if (openedUI.ContainsKey(id))
        {
            Destroy(openedUI[id].gameObject);
            openedUI.Remove(id);
        }
    }
}

public class UIData
{
    public int id;
    public Type type;
    public string path;

    public UIData(int index, Type type, string path)
    {
        this.id = index;
        this.type = type;
        this.path = path;
    }

    public static UIData ShootUI = new UIData(0, typeof(ShootUI), "Prefabs/UI/ShootUI");
}