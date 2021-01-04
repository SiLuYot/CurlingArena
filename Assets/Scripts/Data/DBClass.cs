using SQLiteLoadHelper.Data;
using SQLiteLoadHelper.Database;
using System.Linq;

public class AffiliationData : ReadData
{
    public int id;
    public string affiliation;

    public override int ID => id;
    public override string NAME => affiliation;
}

public class AffiliationDataBase : DataBase<AffiliationData>
{
    public override string DBKey => "AffiliationDB";
}

public class CharacterData : ReadData
{
    public int id;
    public int rarity;
    public string name;
    public float attack;
    public float defence;
    public int size;
    public int species;
    public int affiliation;
    public int job;
    public int skill1;
    public int skill2;

    public RarityData rarityData;
    public SizeData sizeData;
    public SpeciesData speciesData;
    public AffiliationData affiliationData;
    public JobData jobData;
    public SkillData[] skillDataArray;

    public override int ID => id;
    public override string NAME => name;
}

public class CharacterDataBase : DataBase<CharacterData>
{
    public override string DBKey => "CharacterDB";

    public override bool CombinedData()
    {
        var dbLoader = DataBaseManager.Instance.loader;

        var dataList = GetDataList();
        var rarityDataList = dbLoader.GetDataList("RarityDB");
        var sizeDataList = dbLoader.GetDataList("SizeDB");
        var speciesDataList = dbLoader.GetDataList("SpeciesDB");
        var affiliationDataList = dbLoader.GetDataList("AffiliationDB");
        var jobDataList = dbLoader.GetDataList("JobDB");
        var skillDataList = dbLoader.GetDataList("SkillDB");

        foreach (var data in dataList)
        {
            var characterData = data as CharacterData;
            characterData.rarityData = rarityDataList.Find(v => v.ID == characterData.rarity) as RarityData;
            characterData.sizeData = sizeDataList.Find(v => v.ID == characterData.size) as SizeData;
            characterData.speciesData = speciesDataList.Find(v => v.ID == characterData.species) as SpeciesData;
            characterData.affiliationData = affiliationDataList.Find(v => v.ID == characterData.affiliation) as AffiliationData;
            characterData.jobData = jobDataList.Find(v => v.ID == characterData.job) as JobData;

            var skillData1 = skillDataList.Find(v => v.ID == characterData.skill1) as SkillData;
            var skillData2 = skillDataList.Find(v => v.ID == characterData.skill2) as SkillData;

            characterData.skillDataArray = new SkillData[]
            {
                skillData1,
                skillData2
            };
        }

        return true;
    }
}

public class JobData : ReadData
{
    public int id;
    public string job;

    public override int ID => id;
    public override string NAME => job;
}

public class JobDataBase : DataBase<JobData>
{
    public override string DBKey => "JobDB";
}

public class RarityData : ReadData
{
    public int id;
    public string rarity;

    public override int ID => id;
    public override string NAME => rarity;
}

public class RarityDataBase : DataBase<RarityData>
{
    public override string DBKey => "RarityDB";
}

public class SizeData : ReadData
{
    public int id;
    public string name;
    public float size;

    public override int ID => id;
    public override string NAME => name;

    public float GetCharacterScale()
    {
        return GetCharacterRadius() * 2.0f;
    }

    public float GetCharacterRadius()
    {
        return GameManager.DISTACNE * size;
    }
}

public class SizeDataBase : DataBase<SizeData>
{
    public override string DBKey => "SizeDB";
}

public class SkillData : ReadData
{
    public int id;
    public string name;
    public int conditionType;
    public int conditionObjectType;
    public int occurStandard;
    public int applyObject;
    public float applyAngle;
    public float applyRange;
    public int applyType;
    public int applyValueType;
    public float applyValue;
    public float applyPercentValue;
    //public int changingValue;
    public int checkSize;
    public int checkOtherRarity;
    public int isDefanceOver;
    public int isOneShot;
    public int isFirstCollide;
    public string desc;

    public SkillData()
    {

    }

    public SkillData(SkillData skillData)
    {
        this.id = skillData.id;
        this.name = skillData.name;
        this.conditionType = skillData.conditionType;
        this.conditionObjectType = skillData.conditionObjectType;
        this.occurStandard = skillData.occurStandard;
        this.applyObject = skillData.applyObject;
        this.applyAngle = skillData.applyAngle;
        this.applyRange = skillData.applyRange;
        this.applyType = skillData.applyType;
        this.applyValueType = skillData.applyValueType;
        this.applyValue = skillData.applyValue;
        this.applyPercentValue = skillData.applyPercentValue;
        this.checkSize = skillData.checkSize;
        this.checkOtherRarity = skillData.checkOtherRarity;
        this.isDefanceOver = skillData.isDefanceOver;
        this.isOneShot = skillData.isOneShot;
        this.isFirstCollide = skillData.isFirstCollide;
        this.desc = skillData.desc;
    }

    public ConditionType cType => (ConditionType)conditionType;

    public enum ConditionType
    {
        None,
        Init,
        Collide,
        BeCollide,
        AllStop,
        FirstShoot,
    }

    public override int ID => id;
    public override string NAME => name;
}

public class SkillDataBase : DataBase<SkillData>
{
    public override string DBKey => "SkillDB";
}

public class SpeciesData : ReadData
{
    public int id;
    public string species;

    public override int ID => id;
    public override string NAME => species;
}

public class SpeciesDataBase : DataBase<SpeciesData>
{
    public override string DBKey => "SpeciesDB";
}

public class BasicData : ReadData
{
    public float Distacne;
    public float Mass;
    public float Base_Friction;
    public float Friction;
    public float Sweep;
    public float Sweep_Max;
    public float Sweep_Min_Distance;
    public float Sweep_Dir;
    public float Sweep_Fix_Distance;
    public float Impulse;
    public float Round_Count;
    public float Turn_Wait_Time;
    public float Round_Wait_Time;
    public float Turn;
    
    public override int ID => 0;
    public override string NAME => "basic";
}

public class BasicDataBase : DataBase<BasicData>
{
    public override string DBKey => "BasicDB";

    public BasicData GetBasicData() => GetDataList().First() as BasicData;
}

public class SynergyData : ReadData
{
    public int id;
    public string name;
    public int speciesType;
    public int affiliationType;
    public int jobType;
    public int typeNumber;
    public int applyObject;
    public float attackValue;
    public float defenceValue;
    public int skillID;
    public string text;

    public SkillData skillData;

    public override int ID => 0;
    public override string NAME => name;
}

public class SynergyDataBase : DataBase<SynergyData>
{
    public override string DBKey => "SynergyDB";

    public override bool CombinedData()
    {
        var dbLoader = DataBaseManager.Instance.loader;

        foreach (var data in GetDataList())
        {
            var synergyData = data as SynergyData;

            var skillData = dbLoader.GetDataBase("SkillDB").GetDataList().
                Find(v => v.ID == synergyData.skillID) as SkillData;

            if (skillData != null && skillData.id != 0)
                synergyData.skillData = skillData;
        }

        return true;
    }
}