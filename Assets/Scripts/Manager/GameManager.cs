using Boo.Lang;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform stoneRoot;
    public Transform playerStartPos;
    public Transform housePos;

    public GameObject characterPrefab1;
    public GameObject characterPrefab2;
    public int playerID = 0;
    public int EnemyID = 0;

    public Character CurrentCharacter;
    private List<Character> enemyList = new List<Character>();

    private static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(GameManager)) as GameManager;

            return instance;
        }
    }

    void Start()
    {
        TestSceneStart();
    }

    public void TestSceneStart()
    {
        CameraManager.Instance.Init();

        var characterList = DataBaseManager.Instance.loader.GetDataBase("CharacterDB");

        var enemyData = characterList.GetDataList().Find(v => v.ID == EnemyID) as CharacterData;
        var characterData = characterList.GetDataList().Find(v => v.ID == playerID) as CharacterData;

        var player = Instantiate(characterPrefab1, stoneRoot).GetComponent<Character>();
        player.transform.localPosition = playerStartPos.localPosition;
        player.name = "player";
        player.Init(characterData, 0);

        var enemy1 = Instantiate(characterPrefab2, stoneRoot).GetComponent<Character>();
        enemy1.transform.position = new Vector3(housePos.position.x, enemy1.transform.position.y, housePos.position.z);        
        enemy1.name = "enemy1";
        enemy1.Init(enemyData, 1);
        enemyList.Add(enemy1);

        var enemy2 = Instantiate(characterPrefab2, stoneRoot).GetComponent<Character>();
        enemy2.transform.position = new Vector3(housePos.position.x + 3f, enemy2.transform.position.y, housePos.position.z);
        enemy2.name = "enemy2";
        enemy2.Init(enemyData, 1);
        enemyList.Add(enemy2);

        var enemy3 = Instantiate(characterPrefab2, stoneRoot).GetComponent<Character>();
        enemy3.transform.position = new Vector3(housePos.position.x, enemy3.transform.position.y, housePos.position.z + 3f);
        enemy3.name = "enemy3";
        enemy3.Init(enemyData, 1);
        enemyList.Add(enemy3);

        CurrentCharacter = player;
    }

    public void TestSceneRefresh()
    {
        CurrentCharacter.RemoveCharacterPhysics();
        Destroy(CurrentCharacter.gameObject);
        CurrentCharacter = null;

        foreach (var enemy in enemyList)
        {
            enemy.RemoveCharacterPhysics();
            Destroy(enemy.gameObject);
        }
        enemyList.Clear();

        TestSceneStart();
    }

    public void SetPlayerCharacter(CharacterData data)
    {
        playerID = data.id;
        CurrentCharacter.RefreshData(data);
    }

    public void SetEnemyCharacter(CharacterData data)
    {
        EnemyID = data.id;
        foreach (var enemy in enemyList)
        {
            enemy.RefreshData(data);
        }
    }
}
