using System.Collections.Generic;
using UnityEngine;

public class SynergyData
{
    public int species;
    public float synergyAtkValue;
    public float synergyDefValue;

    public SynergyData(int species, float synergyAtkValue, float synergyDefValue)
    {
        this.species = species;
        this.synergyAtkValue = synergyAtkValue;
        this.synergyDefValue = synergyDefValue;
    }
}

public class DeckData
{
    public Dictionary<int, CharacterData> DataDic { get; private set; }
    public List<SynergyData> SynergyDataList { get; private set; }
    public string DeckName { get; private set; }
    public string DeckSynergyText { get; private set; }

    public DeckData(string deckName)
    {
        DataDic = new Dictionary<int, CharacterData>();
        SynergyDataList = new List<SynergyData>();

        this.DeckName = deckName;
        this.DeckSynergyText = "시너지 효과 없음";
    }

    public void AddData(int key, CharacterData data)
    {
        if (!DataDic.ContainsKey(key))
        {
            DataDic.Add(key, data);
        }
    }

    public void SetDeckEffectText(string text)
    {
        DeckSynergyText = text;
    }

    public void CopyData(Dictionary<int, CharacterData> dataDic)
    {
        this.DataDic = new Dictionary<int, CharacterData>(dataDic);
    }

    public void ChangeDeckName(string deckName)
    {
        this.DeckName = deckName;
    }

    public bool IsCompleteDeck()
    {
        if (DataDic.Count >= GameManager.DECK_CHARACTER_COUNT)
            return true;

        return false;
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

    public int CurDeckIndex { get; private set; }

    public DeckData CurDeck
    {
        get
        {
            return DeckDic[CurDeckIndex];
        }
    }

    public void Init()
    {
        //테스트 용
        AddTestDeck();

        //현재 선택된 덱 세팅
        CurDeckIndex = 0;
    }

    public void AddNewDeck(int key, DeckData data)
    {
        if (!deckDic.ContainsKey(key))
        {
            deckDic.Add(key, data);
            CheckDeckEffect(data);
        }
    }

    public void EditDeck(int key, DeckData data)
    {
        if (deckDic.ContainsKey(key))
        {
            deckDic[key] = data;
        }
    }

    public bool ChangeUseDeck(int index)
    {
        if (DeckDic.ContainsKey(index))
        {
            this.CurDeckIndex = index;
            return true;
        }

        return false;
    }

    public void CheckDeckEffect(DeckData deckData)
    {
        //덱 시너지 하드코딩..
        deckData.SynergyDataList.Clear();

        var speciesDic = new Dictionary<int, int>();
        foreach (var data in deckData.DataDic)
        {
            var species = data.Value.speciesData;
            if (speciesDic.ContainsKey(species.ID))
            {
                speciesDic[species.ID] += 1;
            }
            else speciesDic.Add(species.ID, 1);
        }

        string text = string.Empty;

        //인간
        if (speciesDic.ContainsKey(0))
        {
            if (speciesDic[0] >= 2)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 인간의 능력치 공격력 +5%, 방어력 +5%";
                deckData.SynergyDataList.Add(new SynergyData(0, 0.05f, 0.05f));
            }
        }

        //신
        if (speciesDic.ContainsKey(1))
        {
            if (speciesDic[1] >= 2)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 신의 능력치 공격력 +5%, 방어력 +5%";
                deckData.SynergyDataList.Add(new SynergyData(1, 0.05f, 0.05f));
            }
        }

        if (text != string.Empty)
        {
            deckData.SetDeckEffectText(text);
        }
        else deckData.SetDeckEffectText("시너지 효과 없음");
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
