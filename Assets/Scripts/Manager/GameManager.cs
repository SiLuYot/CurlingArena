﻿using System.Collections.Generic;
using UnityEngine;

public enum RoundStep
{
    NONE = 0,
    READY = 1,
    SHOOT = 2,
    SWEEP = 3,
    MOVE = 4,
    END = 5,
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
    //임시 상수값들
    //거리 1 = 1.45f = 크기 중의 반지름 모두 동일
    public static float DISTACNE = 1.45f;
    //물리 오브젝트들의 질량
    public static float MASS = 1f;
    //스윕시 감소시킬 마찰력
    public static float SWEEP = 0.001f;
    //스윕으로 감소되는 마찰력의 최솟값
    public static float SWEEP_MAX = 0.01f;
    //스윕으로 판정되는 최소 거리
    public static float SWEEP_MIN_DISTACNE = 100.0f;

    public static RoundStep CurRoundStep;

    public Transform stoneRoot;
    public Transform playerStartPos;
    public Transform housePos;

    public GameObject characterPrefab1;
    public GameObject characterPrefab2;

    public Character CurrentCharacter { get; private set; }
    public List<CharacterCreateData> CharacterCreateDataList { get; private set; }
    public List<Character> ChracterList { get; private set; }    

    private static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            //인스턴스가 없다면 씬에서 찾는다.
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(GameManager)) as GameManager;
            }

            //인스턴스가 찾아도 없다면 새로 만든다.
            if (instance == null)
            {
                var newObj = new GameObject("GameManager");
                instance = newObj.AddComponent<GameManager>();
            }

            return instance;
        }
    }

    public void Awake()
    {
        //DontDestroyOnLoad(gameObject);

        CharacterCreateDataList = new List<CharacterCreateData>();
        ChracterList = new List<Character>();
    }

    void Start()
    {
        UIManager.Instance.Get<IntroUI>();
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

        Sweep();
    }

    public void Sweep()
    {
        CurRoundStep = RoundStep.SWEEP;
        Debug.Log("Round Step : " + CurRoundStep);

        UIManager.Instance.Get<SweepUI>();
    }

    public void Move()
    {
        CurRoundStep = RoundStep.MOVE;
        Debug.Log("Round Step : " + CurRoundStep);

        var sweepUI = UIManager.Instance.IsUIOpened<SweepUI>();
        if (sweepUI != null)
        {
            sweepUI.Close();
        }
    }

    public void End()
    {
        CurRoundStep = RoundStep.END;
        Debug.Log("Round Step : " + CurRoundStep);
    }

    public void TestSceneStart()
    {
        var testUI = UIManager.Instance.Get<TestBattleSceneUI>() as TestBattleSceneUI;
        testUI?.Init();
    }

    public void AddCharacter(Team team, CharacterData data, Vector3 pos, bool isPlayerChacter)
    {
        var newData = new CharacterCreateData(team, data, pos, isPlayerChacter);
        CharacterCreateDataList.Add(newData);

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
        newCharacter.transform.localScale = new Vector3(createData.data.sizeData.GetCharacterScale(), 0.1f, createData.data.sizeData.GetCharacterScale());

        var findList = GameManager.Instance.ChracterList.FindAll(v => (int)v.Team == (int)createData.team);
        string name = createData.team.ToString() + "_" + findList.Count;

        newCharacter.name = name;
        newCharacter.Init(createData.data, createData.team);

        ChracterList.Add(newCharacter);

        if (createData.isPlayerChacter)
            SetCurrentCharacter(newCharacter);
    }

    public void SetCurrentCharacter(Character character)
    {
        CurrentCharacter = character;

        Debug.Log(string.Format("Set Player Character\nPID : {0} Name : {1}",
            CurrentCharacter.Physics.PID, CurrentCharacter.name));
    }

    public void RemoveCharacter(int pid)
    {
        var findCharacter = ChracterList.Find(v => v.Physics.PID == pid);

        if (findCharacter != null)
        {
            if (CurrentCharacter != null)
            {
                if (findCharacter.Physics.PID == CurrentCharacter.Physics.PID)
                {
                    var activeObj = ChracterList.Find(v => v.Physics.isInActive == false);
                    CameraManager.Instance.SetFollowTrans(activeObj.transform);
                    CurrentCharacter = null;
                }
            }

            ChracterList.Remove(findCharacter);
            findCharacter.RemoveCharacterPhysics();
            Destroy(findCharacter.gameObject);
            findCharacter = null;
        }
    }

    public void RemoveCreateCharacterData(Team team, CharacterData data, Vector3 pos)
    {
        var findData = CharacterCreateDataList.Find(
            v => v.team == team && 
            v.data.id == data.id && 
            v.pos.x == pos.x &&
            v.pos.z == pos.z);

        CharacterCreateDataList.Remove(findData);
        findData = null;
    }

    public void RemoveAllData()
    {
        foreach (var player in ChracterList)
        {
            player.RemoveCharacterPhysics();
            Destroy(player.gameObject);
        }
        ChracterList.Clear();

        CurrentCharacter = null;

        CharacterCreateDataList.Clear();
    }

    public void TestSceneReset()
    {
        foreach (var player in ChracterList)
        {
            player.RemoveCharacterPhysics();
            Destroy(player.gameObject);
        }
        ChracterList.Clear();

        CurrentCharacter = null;

        foreach (var data in CharacterCreateDataList)
        {
            CreateCharacter(data);
        }

        var sweepUI = UIManager.Instance.IsUIOpened<SweepUI>();
        if (sweepUI != null)
        {
            sweepUI.Close();
        }
    }

    public bool IsCurrentPlayer(Character character)
    {
        return CurrentCharacter == null ? 
            false : 
            CurrentCharacter.Physics.PID == character.Physics.PID;
    }
}
