using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance = null;
    public static UIManager Instance
    {
        get
        {
            //인스턴스가 없다면 씬에서 찾는다.
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(UIManager)) as UIManager;
            }
            return instance;
        }
    }

    public UIRoot uiRoot;

    private int accumulateIndex;
    private Dictionary<Type, UIData> uiData;
    private List<BaseUI> openedUI;
    private Stack<int> openedUIIdx;

    public void Awake()
    {
        accumulateIndex = 0;
        uiData = new Dictionary<Type, UIData>();
        openedUI = new List<BaseUI>();
        openedUIIdx = new Stack<int>();

        AddUIPath(UIData.ShootUI);
        AddUIPath(UIData.TestSceneUI);
        AddUIPath(UIData.PositionSelectUI);
        AddUIPath(UIData.SweepUI);
        AddUIPath(UIData.MainMenuUI);
        AddUIPath(UIData.IntroUI);
        AddUIPath(UIData.DeckEditUI);
        AddUIPath(UIData.GameReadyUI);
        AddUIPath(UIData.GameMainUI);
        AddUIPath(UIData.GameScoreUI);
        AddUIPath(UIData.GameCharacterNameUI);
        AddUIPath(UIData.BasePopupUI);
    }

    private void Update()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyUp(KeyCode.Home))
            {
                //home button
            }
            else if (Input.GetKeyUp(KeyCode.Escape))
            {
                //back button
                if (openedUIIdx.Count > 0)
                {
                    int index = openedUIIdx.Peek();
                    
                    var findUI = openedUI.Find(v => v.index == index);
                    if (findUI != null)
                    {
                        findUI.Close();
                    }
                }
            }
            else if (Input.GetKeyUp(KeyCode.Menu))
            {
                //menu button
            }
        }
    }

    public void Close()
    {
        int index = openedUIIdx.Pop();

        var findUI = openedUI.Find(v => v.index == index);
        if (findUI != null)
        {
            openedUI.Remove(findUI);
            Destroy(findUI.gameObject);
        }
    }

    public void Close(int index)
    {
        var findUI = openedUI.Find(v => v.index == index);
        if (findUI != null)
        {
            openedUI.Remove(findUI);
            Destroy(findUI.gameObject);
        }
    }

    private void AddUIPath(UIData data)
    {
        uiData.Add(data.type, data);
    }

    public BaseUI Get<T>(bool isForceCreate = false) where T : BaseUI
    {
        BaseUI loadObj = null;

        if (uiData.ContainsKey(typeof(T)))
        {
            var data = uiData[typeof(T)];

            var findOpenedUI = openedUI.Find(v => v.uiData.id == data.id);            
            if (!isForceCreate && findOpenedUI != null)
            {
                loadObj = findOpenedUI;
            }
            else
            {
                var loadGameObj = Instantiate(Resources.Load(data.path), uiRoot.transform) as GameObject;
                if (loadGameObj == null)
                {
                    Debug.LogError(string.Format("{0} 잘못된 경로", data.path));
                }

                loadObj = loadGameObj.GetComponent<BaseUI>();
                if (loadObj == null)
                {
                    Debug.LogError(string.Format("UIBase 컴포넌트가 없거나 UIBase를 상속받은 UI가 아님"));
                }

                loadObj.Init(accumulateIndex, data);
                openedUI.Add(loadObj);

                //시스템UI가 아닌경우
                if (!(loadObj is BaseSystemUI))
                {
                    openedUIIdx.Push(accumulateIndex);
                }                

                accumulateIndex++;
            }

        }
        else Debug.LogError(string.Format("{0} 타입의 UI DATA 없음", typeof(T)));

        return loadObj;
    }

    public void SetActive(int index, bool active)
    {
        var findUI = openedUI.Find(v => v.index == index);
        if (findUI != null)
        {
            findUI.gameObject.SetActive(active);
        }
    }

    public void SetAllActive(bool active)
    {
        foreach (var ui in openedUI)
        {
            SetActive(ui.index, active);
        }
    }

    public BaseUI IsUIOpened<T>() where T : BaseUI
    {
        BaseUI loadObj = null;

        if (uiData.ContainsKey(typeof(T)))
        {
            var data = uiData[typeof(T)];

            var findOpenedUI = openedUI.Find(v => v.uiData.id == data.id);
            if (findOpenedUI != null)
            {
                loadObj = findOpenedUI;
            }
        }
        else Debug.LogError(string.Format("{0} 타입의 UI DATA 없음", typeof(T)));

        return loadObj;
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
    public static UIData TestSceneUI = new UIData(1, typeof(TestBattleSceneUI), "Prefabs/UI/TestBattleSceneUI");
    public static UIData PositionSelectUI = new UIData(2, typeof(PositionSelectUI), "Prefabs/UI/PositionSelectUI");
    public static UIData SweepUI = new UIData(3, typeof(SweepUI), "Prefabs/UI/SweepUI");
    public static UIData MainMenuUI = new UIData(4, typeof(MainMenuUI), "Prefabs/UI/MainMenuUI");
    public static UIData IntroUI = new UIData(5, typeof(IntroUI), "Prefabs/UI/IntroUI");
    public static UIData DeckEditUI = new UIData(6, typeof(DeckEditUI), "Prefabs/UI/DeckEditUI");
    public static UIData GameReadyUI = new UIData(7, typeof(GameReadyUI), "Prefabs/UI/GameReadyUI");
    public static UIData GameMainUI = new UIData(8, typeof(GameMainUI), "Prefabs/UI/GameMainUI");
    public static UIData GameScoreUI = new UIData(9, typeof(GameScoreUI), "Prefabs/UI/GameScoreUI");
    public static UIData GameCharacterNameUI = new UIData(10, typeof(GameCharacterNameUI), "Prefabs/UI/GameCharacterNameUI");
    public static UIData BasePopupUI = new UIData(11, typeof(BasePopupUI), "Prefabs/UI/BasePopupUI");

}