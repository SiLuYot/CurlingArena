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

    public SizeData sizeData;
    public SpeciesData speciesData;
    public AffiliationData affiliationData;
    public JobData jobData;
    public SkillData skillData1;
    public SkillData skillData2;

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
        var sizeDataList = dbLoader.GetDataList("SizeDB");
        var speciesDataList = dbLoader.GetDataList("SpeciesDB");
        var affiliationDataList = dbLoader.GetDataList("AffiliationDB");
        var jobDataList = dbLoader.GetDataList("JobDB");
        var skillDataList = dbLoader.GetDataList("SkillDB");

        foreach (var data in dataList)
        {
            var characterData = data as CharacterData;
            characterData.sizeData = sizeDataList.Find(v => v.ID == characterData.size) as SizeData;
            characterData.speciesData = speciesDataList.Find(v => v.ID == characterData.species) as SpeciesData;
            characterData.affiliationData = affiliationDataList.Find(v => v.ID == characterData.affiliation) as AffiliationData;
            characterData.jobData = jobDataList.Find(v => v.ID == characterData.job) as JobData;
            characterData.skillData1 = skillDataList.Find(v => v.ID == characterData.skill1) as SkillData;
            characterData.skillData2 = skillDataList.Find(v => v.ID == characterData.skill2) as SkillData;
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
}

public class SizeDataBase : DataBase<SizeData>
{
    public override string DBKey => "SizeDB";
}

public class SkillData : ReadData
{
    public int id;
    public string skillname;
    public int skillTrigger;
    public int condition1;
    public int condition2;
    public int condition3;
    public int skillTriggerObject;
    public int skillTarget;
    public int changingType;
    public int changingValue;
    public int figureType;
    public float figureValue;
    public int keep;
    public int checkSize;
    public int checkRarity;
    public float Range;
    public string text;

    public override int ID => id;
    public override string NAME => skillname;
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

