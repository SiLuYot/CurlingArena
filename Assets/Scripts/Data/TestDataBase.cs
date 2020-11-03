using SQLiteLoadHelper.Data;
using SQLiteLoadHelper.Database;

public class TestData : ReadData
{
    public int id;
    public string name;
    public int count;
    public float value;

    public override int ID => id;
    public override string NAME => name;
}

public class TestDataBase : DataBase<TestData>
{
    public override string DBKey => "test";
}
