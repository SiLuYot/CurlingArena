using System.Collections.Generic;
using UnityEngine;

public enum RoundStep
{
    READY = 0,
    SHOOT = 1,
    MOVE = 2,
    END = 3,
}

public class GameManager : MonoBehaviour
{
    //임시 상수값
    public static float DISTACNE = 1.5f;

    public static RoundStep CurRoundStep;    

    public Transform stoneRoot;
    public Transform playerStartPos;
    public Transform housePos;

    public GameObject characterPrefab1;
    public GameObject characterPrefab2;
    public int playerID = 0;
    public int EnemyID = 0;

    private Character currentCharacter;
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
        Ready();
    }

    public void Ready()
    {
        CurRoundStep = RoundStep.READY;
        Debug.Log("Round Step : " + CurRoundStep);

        CameraManager.Instance.Init();
        PhysicsManager.Instance.Init();

        TestSceneStart();        
    }

    public void Shoot(Character character, Vector3 dir, float force, float attackBouns)
    {
        CurRoundStep = RoundStep.SHOOT;
        Debug.Log("Round Step : " + CurRoundStep);

        CameraManager.IsFixed = true;
        CameraManager.Instance.SetFollowTrans(character.transform);        

        character.Physics.ApplyForce(dir, force, attackBouns);

        Move();
    }

    public void Move()
    {
        CurRoundStep = RoundStep.MOVE;
        Debug.Log("Round Step : " + CurRoundStep);
    }

    public void End()
    {
        CurRoundStep = RoundStep.END;
        Debug.Log("Round Step : " + CurRoundStep);
    }

    public void TestSceneStart()
    {        
        var characterList = DataBaseManager.Instance.loader.GetDataBase("CharacterDB");

        var enemyData = characterList.GetDataList().Find(v => v.ID == EnemyID) as CharacterData;
        var characterData = characterList.GetDataList().Find(v => v.ID == playerID) as CharacterData;

        var player = Instantiate(characterPrefab1, stoneRoot).GetComponent<Character>();
        player.transform.localPosition = playerStartPos.localPosition;
        player.name = "player";
        player.Init(characterData, 0);

        var enemy1 = Instantiate(characterPrefab2, stoneRoot).GetComponent<Character>();
        enemy1.transform.position = new Vector3(housePos.position.x, enemy1.transform.position.y, housePos.position.z - 5f);        
        enemy1.name = "enemy1";
        enemy1.Init(enemyData, 1);
        enemyList.Add(enemy1);

        var enemy2 = Instantiate(characterPrefab2, stoneRoot).GetComponent<Character>();
        enemy2.transform.position = new Vector3(housePos.position.x + 3f, enemy2.transform.position.y, housePos.position.z);
        enemy2.name = "enemy2";
        enemy2.Init(enemyData, 1);
        enemyList.Add(enemy2);

        var enemy3 = Instantiate(characterPrefab2, stoneRoot).GetComponent<Character>();
        enemy3.transform.position = new Vector3(housePos.position.x, enemy3.transform.position.y, housePos.position.z + 5f);
        enemy3.name = "enemy3";
        enemy3.Init(enemyData, 1);
        enemyList.Add(enemy3);

        currentCharacter = player;
    }

    public void TestSceneRefresh()
    {
        currentCharacter.RemoveCharacterPhysics();
        Destroy(currentCharacter.gameObject);
        currentCharacter = null;

        foreach (var enemy in enemyList)
        {
            enemy.RemoveCharacterPhysics();
            Destroy(enemy.gameObject);
        }
        enemyList.Clear();

        Ready();
    }

    public void SetPlayerCharacter(CharacterData data)
    {
        playerID = data.id;
        currentCharacter.RefreshData(data);
    }

    public void SetEnemyCharacter(CharacterData data)
    {
        EnemyID = data.id;
        foreach (var enemy in enemyList)
        {
            enemy.RefreshData(data);
        }
    }

    public bool IsCurrentPlayer(Character character)
    {
        return currentCharacter.Physics.pid == character.Physics.pid;
    }
}
