using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SynergyData
{
    public int id;
    public float synergyAtkValue;
    public float synergyDefValue;
    public SkillData skill;

    public SynergyData(int id, float synergyAtkValue, float synergyDefValue, SkillData skill = null)
    {
        this.id = id;
        this.synergyAtkValue = synergyAtkValue;
        this.synergyDefValue = synergyDefValue;
        this.skill = skill;
    }
}

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
        int index = 0;
        string text = string.Empty;

        //db 안만들기로 해서 덱 시너지 하드코딩
        deckData.SynergySpeciesDataList.Clear();
        deckData.SynergyAffiliationDataList.Clear();
        deckData.SynergyJobDataList.Clear();

        //species
        var dataDic = new Dictionary<int, int>();
        foreach (var data in deckData.DataDic)
        {
            var species = data.Value.speciesData;
            if (dataDic.ContainsKey(species.ID))
            {
                dataDic[species.ID] += 1;
            }
            else dataDic.Add(species.ID, 1);
        }

        //인간
        index = 0;
        if (dataDic.ContainsKey(index))
        {
            if (dataDic[index] >= 2)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 인간의 능력치 공격력 +5%, 방어력 +5%";
                deckData.SynergySpeciesDataList.Add(new SynergyData(index, 0.05f, 0.05f));
            }
            if (dataDic[index] >= 4)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 인간의 능력치 공격력 +5%, 방어력 +5%";
                deckData.SynergySpeciesDataList.Add(new SynergyData(index, 0.05f, 0.05f));
            }
        }

        //신
        index = 1;
        if (dataDic.ContainsKey(index))
        {
            if (dataDic[index] >= 2)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 신의 능력치 공격력 +5%, 방어력 +5%";
                deckData.SynergySpeciesDataList.Add(new SynergyData(index, 0.05f, 0.05f));
            }
            if (dataDic[index] >= 4)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 신의 능력치 공격력 +5%, 방어력 +5%";
                deckData.SynergySpeciesDataList.Add(new SynergyData(index, 0.05f, 0.05f));
            }
        }

        //affiliation        
        dataDic.Clear();
        foreach (var data in deckData.DataDic)
        {
            var affiliation = data.Value.affiliationData;
            if (dataDic.ContainsKey(affiliation.ID))
            {
                dataDic[affiliation.ID] += 1;
            }
            else dataDic.Add(affiliation.ID, 1);
        }

        //기사단
        index = 0;
        if (dataDic.ContainsKey(index))
        {
            if (dataDic[index] >= 4)
            {
                if (text != string.Empty)
                    text += "\n";

                //검사 스킬2
                //var skill = DataBaseManager.Instance.loader.GetDataBase("SkillDB").
                //    GetDataList().Find(v => v.ID == 4) as SkillData;

                //var newSkill = new SkillData(skill);
                //newSkill.applyValue = 0.3f;

                text += "기사단 캐릭터가 충돌당할 시 상대 캐릭터를 공격력의 30%만큼 밀어낸다.";
                deckData.SynergyAffiliationDataList.Add(new SynergyData(index, 0, 0, null));
            }
        }

        //교단
        index = 1;
        if (dataDic.ContainsKey(index))
        {
            if (dataDic[index] >= 4)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "처음으로 발사한 캐릭터가 스킬에 1회 면역";
                deckData.SynergyAffiliationDataList.Add(new SynergyData(index, 0, 0, null));
            }
            if (dataDic[index] >= 2)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 교단 캐릭터가 스킬에 1회 면역";
                deckData.SynergyAffiliationDataList.Add(new SynergyData(index, 0, 0, null));
            }
        }

        //천사
        index = 2;
        if (dataDic.ContainsKey(index))
        {
            if (dataDic[index] >= 4)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "천사가 아군과 충돌하면 해당 라운드에 아군에게 공격력 +10%, 방어력 10%를 부여함";
                deckData.SynergyAffiliationDataList.Add(new SynergyData(index, 0, 0, null));
            }
            if (dataDic[index] >= 2)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 천사의 능력치 10%";
                deckData.SynergyAffiliationDataList.Add(new SynergyData(index, 0.1f, 0.1f));
            }
        }

        //job      
        dataDic.Clear();
        foreach (var data in deckData.DataDic)
        {
            var job = data.Value.jobData;
            if (dataDic.ContainsKey(job.ID))
            {
                dataDic[job.ID] += 1;
            }
            else dataDic.Add(job.ID, 1);
        }

        //기사
        index = 0;
        if (dataDic.ContainsKey(index))
        {
            if (dataDic[index] >= 2)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 아군의 능력치 공격력 +5%, 방어력 +5%";
                deckData.SynergyJobDataList.Add(new SynergyData(index, 0.05f, 0.05f));
            }
            if (dataDic[index] >= 4)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 아군의 능력치 공격력 +5%, 방어력 +5%";
                deckData.SynergyJobDataList.Add(new SynergyData(index, 0.05f, 0.05f));
            }
        }

        //전사
        index = 1;
        if (dataDic.ContainsKey(index))
        {
            if (dataDic[index] >= 2)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 아군의 능력치 공격력 +5%, 방어력 +5%";
                deckData.SynergyJobDataList.Add(new SynergyData(index, 0.05f, 0.05f));
            }
            if (dataDic[index] >= 4)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 아군의 능력치 공격력 +5%, 방어력 +5%";
                deckData.SynergyJobDataList.Add(new SynergyData(index, 0.05f, 0.05f));
            }
        }

        //마법사
        index = 2;
        if (dataDic.ContainsKey(index))
        {
            if (dataDic[index] >= 2)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 아군의 능력치 공격력 +10%";
                deckData.SynergyJobDataList.Add(new SynergyData(index, 0.1f, 0));
            }
            if (dataDic[index] >= 4)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 아군의 능력치 공격력 +10%";
                deckData.SynergyJobDataList.Add(new SynergyData(index, 0.1f, 0));
            }
        }

        //방패병
        index = 3;
        if (dataDic.ContainsKey(index))
        {
            if (dataDic[index] >= 2)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 아군의 능력치 방어력 +10%";
                deckData.SynergyJobDataList.Add(new SynergyData(index, 0, 0.1f));
            }
            if (dataDic[index] >= 4)
            {
                if (text != string.Empty)
                    text += "\n";

                text += "모든 아군의 능력치 방어력 +10%";
                deckData.SynergyJobDataList.Add(new SynergyData(index, 0, 0.1f));
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
