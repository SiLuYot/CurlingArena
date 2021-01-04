using System.Collections.Generic;
using UnityEngine;

public class DeckData
{
    public Dictionary<int, CharacterData> DataDic { get; private set; }
    public List<SynergyData> SynergySpeciesDataList { get; private set; }
    public List<SynergyData> SynergyAffiliationDataList { get; private set; }
    public List<SynergyData> SynergyJobDataList { get; private set; }
    public string DeckName { get; private set; }
    public string DeckSynergyText { get; private set; }

    public DeckData(string deckName)
    {
        DataDic = new Dictionary<int, CharacterData>();
        SynergySpeciesDataList = new List<SynergyData>();
        SynergyAffiliationDataList = new List<SynergyData>();
        SynergyJobDataList = new List<SynergyData>();

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
        var speciesDataDic = new Dictionary<int, int>();
        var affiliationDataDic = new Dictionary<int, int>();
        var jobDataDic = new Dictionary<int, int>();

        foreach (var data in deckData.DataDic)
        {
            var species = data.Value.speciesData;
            if (speciesDataDic.ContainsKey(species.ID))
            {
                speciesDataDic[species.ID] += 1;
            }
            else speciesDataDic.Add(species.ID, 1);

            var affiliation = data.Value.affiliationData;
            if (affiliationDataDic.ContainsKey(affiliation.ID))
            {
                affiliationDataDic[affiliation.ID] += 1;
            }
            else affiliationDataDic.Add(affiliation.ID, 1);

            var job = data.Value.jobData;
            if (jobDataDic.ContainsKey(job.ID))
            {
                jobDataDic[job.ID] += 1;
            }
            else jobDataDic.Add(job.ID, 1);
        }

        deckData.SynergySpeciesDataList.Clear();
        deckData.SynergyAffiliationDataList.Clear();
        deckData.SynergyJobDataList.Clear();

        string text = string.Empty;

        var synergyDataList = DataBaseManager.Instance.loader.GetDataList("SynergyDB");
        foreach (var data in synergyDataList)
        {
            var synergyData = data as SynergyData;

            if (synergyData.speciesType != -1)
            {
                if (speciesDataDic.ContainsKey(synergyData.speciesType))
                {
                    var count = speciesDataDic[synergyData.speciesType];
                    if (count >= synergyData.typeNumber)
                    {
                        if (text != string.Empty)
                            text += "\n";

                        text += synergyData.text;
                        deckData.SynergySpeciesDataList.Add(synergyData);
                    }
                }
            }
            if (synergyData.affiliationType != -1)
            {
                if (affiliationDataDic.ContainsKey(synergyData.affiliationType))
                {
                    var count = affiliationDataDic[synergyData.affiliationType];
                    if (count >= synergyData.typeNumber)
                    {
                        if (text != string.Empty)
                            text += "\n";

                        text += synergyData.text;
                        deckData.SynergyAffiliationDataList.Add(synergyData);
                    }
                }
            }
            if (synergyData.jobType != -1)
            {
                if (jobDataDic.ContainsKey(synergyData.jobType))
                {
                    var count = jobDataDic[synergyData.jobType];
                    if (count >= synergyData.typeNumber)
                    {
                        if (text != string.Empty)
                            text += "\n";

                        text += synergyData.text;
                        deckData.SynergyJobDataList.Add(synergyData);
                    }
                }
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
