using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform stoneRoot;
    public Transform playerStartPos;

    public GameObject characterPrefab;
    public int playerID = 0;
    public int EnemyID = 0;

    public Character CurrentCharacter;

    void Start()
    {
        var characterList = DataBaseManager.Instance.loader.GetDataBase("CharacterDB");

        var enemyData = characterList.GetDataList().Find(v => v.ID == EnemyID) as CharacterData;
        var characterData = characterList.GetDataList().Find(v => v.ID == playerID) as CharacterData;

        var enemy1 = Instantiate(characterPrefab, stoneRoot).GetComponent<Character>();
        enemy1.transform.localPosition = new Vector3(18.5f, 0, 0);
        enemy1.name = "enemy1";
        enemy1.Init(enemyData);

        var enemy2 = Instantiate(characterPrefab, stoneRoot).GetComponent<Character>();
        enemy2.transform.localPosition = new Vector3(17.5f, 0, 1);
        enemy2.name = "enemy2";
        enemy2.Init(enemyData);

        var enemy3 = Instantiate(characterPrefab, stoneRoot).GetComponent<Character>();
        enemy3.transform.localPosition = new Vector3(19.5f, 0, -1);
        enemy3.name = "enemy3";
        enemy3.Init(enemyData);

        var player = Instantiate(characterPrefab, stoneRoot).GetComponent<Character>();
        player.transform.localPosition = playerStartPos.localPosition;
        player.name = "player";
        player.Init(characterData);

        CurrentCharacter = player;
    }

}
