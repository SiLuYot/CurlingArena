using System.Collections.Generic;
using UnityEngine;

public class DeckData
{
    public Dictionary<int, CharacterData> DataDic { get; private set; }

    public string DeckName { get; private set; }

    public DeckData(string deckName)
    {
        this.DeckName = deckName;
        DataDic = new Dictionary<int, CharacterData>();
    }

    public void AddData(int key, CharacterData data)
    {
        if (!DataDic.ContainsKey(key))
        {
            DataDic.Add(key, data);
        }        
    }
}

public class DeckManager : MonoBehaviour
{
    private static DeckManager instance = null;
    public static DeckManager Instance
    {
        get
        {
            //인스턴스가 없다면 씬에서 찾는다.
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(DeckManager)) as DeckManager;
            }
            return instance;
        }
    }

    private Dictionary<int, DeckData> deckDic = new Dictionary<int, DeckData>();
    public Dictionary<int, DeckData> DeckDic { get => deckDic; }

    public void AddNewDeck(int key, DeckData data)
    {
        if (!deckDic.ContainsKey(key))
        {
            deckDic.Add(key, data);
        }
    }

    public void EditDeck(int key, DeckData data)
    {
        if (deckDic.ContainsKey(key))
        {
            deckDic[key] = data;
        }
    }

    public void AddTestDeck()
    {
        var dataList = DataBaseManager.Instance.loader.GetDataList("CharacterDB");
        
        var newDeck = new DeckData("test deck");
        newDeck.AddData(0, dataList[1] as CharacterData);
        newDeck.AddData(1, dataList[3] as CharacterData);
        newDeck.AddData(2, dataList[7] as CharacterData);
        newDeck.AddData(3, dataList[10] as CharacterData);

        AddNewDeck(0, newDeck);
    }
}
