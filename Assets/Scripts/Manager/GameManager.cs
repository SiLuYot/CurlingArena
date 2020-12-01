using System.Collections.Generic;
using UnityEngine;

public enum RoundStep
{
    NONE = 0,
    READY = 1,
    SHOOT = 2,
    MOVE = 3,
    END = 4,
}

public enum Team
{
    NONE = 0,
    PLAYER = 1,
    ENEMY = 2,
}

public class CharacterCreateData
{
    public Team team;
    public CharacterData data;
    public Vector3 pos;
    public bool isPlayerChacter;

    public CharacterCreateData(Team team, CharacterData data, Vector3 pos, bool isPlayerChacter)
    {
        this.team = team;
        this.data = data;
        this.pos = pos;
        this.isPlayerChacter = isPlayerChacter;
    }
}

public class GameManager : MonoBehaviour
{
    //임시 상수값
    public static float DISTACNE = 1.45f;
    public static float MASS = 1f;
    public static float SWEEP = 0.0001f;

    public static RoundStep CurRoundStep;

    public Transform stoneRoot;
    public Transform playerStartPos;
    public Transform housePos;

    public GameObject characterPrefab1;
    public GameObject characterPrefab2;

    private Character currentCharacter;
    private List<CharacterCreateData> characterCreateDataList = new List<CharacterCreateData>();
    private List<Character> chracterList = new List<Character>();

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

    private TestBattleSceneUI testSceneUI;

    void Start()
    {
        //Ready();

        TestSceneStart();
    }

    public void None()
    {
        CurRoundStep = RoundStep.NONE;
    }

    public void Ready()
    {
        CurRoundStep = RoundStep.READY;
        Debug.Log("Round Step : " + CurRoundStep);

        CameraManager.Instance.Init();
        PhysicsManager.Instance.Init();
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
        if (testSceneUI == null)
        {
            testSceneUI = UIManager.Instance.Get<TestBattleSceneUI>() as TestBattleSceneUI;
            testSceneUI.Init();
        }
    }

    public void AddCharacter(Team team, CharacterData data, Vector3 pos, bool isPlayerChacter)
    {
        var newData = new CharacterCreateData(team, data, pos, isPlayerChacter);
        characterCreateDataList.Add(newData);

        CreateCharacter(newData);
    }

    public void CreateCharacter(CharacterCreateData createData)
    {
        GameObject prefab = null;

        if (createData.team == Team.PLAYER)
            prefab = characterPrefab1;
        else if (createData.team == Team.ENEMY)
            prefab = characterPrefab2;

        var newCharacter = Instantiate(prefab, stoneRoot).GetComponent<Character>();
        newCharacter.transform.position = new Vector3(createData.pos.x, stoneRoot.transform.position.y, createData.pos.z);

        var findList = chracterList.FindAll(v => (int)v.Team == (int)createData.team);
        string name = createData.team.ToString() + "_" + findList.Count;

        newCharacter.name = name;
        newCharacter.Init(createData.data, createData.team);

        chracterList.Add(newCharacter);

        if (createData.isPlayerChacter)
            SetCurrentCharacter(newCharacter);
    }

    public void SetCurrentCharacter(Character character)
    {
        currentCharacter = character;
    }

    public void RemoveCharacter(int pid)
    {
        var findCharacter = chracterList.Find(v => v.Physics.PID == pid);

        if (findCharacter != null)
        {
            if (currentCharacter != null)
            {
                if (findCharacter.Physics.PID == currentCharacter.Physics.PID)
                {
                    var activeObj = chracterList.Find(v => v.Physics.isInActive == false);
                    CameraManager.Instance.SetFollowTrans(activeObj.transform);
                    currentCharacter = null;
                }
            }

            chracterList.Remove(findCharacter);
            findCharacter.RemoveCharacterPhysics();
            Destroy(findCharacter.gameObject);
            findCharacter = null;
        }
    }

    public void RemoveCreateCharacterData(Team team, CharacterData data, Vector3 pos)
    {
        var findData = characterCreateDataList.Find(
            v => v.team == team && 
            v.data.id == data.id && 
            v.pos.x == pos.x &&
            v.pos.z == pos.z);

        characterCreateDataList.Remove(findData);
        findData = null;
    }

    public void TestSceneReset()
    {
        foreach (var player in chracterList)
        {
            player.RemoveCharacterPhysics();
            Destroy(player.gameObject);
        }
        chracterList.Clear();

        currentCharacter = null;

        foreach (var data in characterCreateDataList)
        {
            CreateCharacter(data);
        }
    }

    public bool IsCurrentPlayer(Character character)
    {
        return currentCharacter == null ? 
            false : 
            currentCharacter.Physics.PID == character.Physics.PID;
    }
}
