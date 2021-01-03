using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Step
{
    NONE = 0,
    SELECT = 1,
    READY = 2,
    SHOOT = 3,
    SWEEP = 4,
    MOVE = 5,
    END = 6,
}

public enum Team
{
    NONE = 0,
    PLAYER1 = 1,
    PLAYER2 = 2,
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

public class PlayerData
{
    public int index;
    public int order;
    public Team team;
    public DeckData deckData;

    public List<int> UseIndexList { get; private set; }
    public int Score { get; private set; }

    public PlayerData(int index, int order, Team team, DeckData deckData)
    {
        this.index = index;
        this.order = order;
        this.team = team;
        this.deckData = deckData;
        this.UseIndexList = new List<int>();
    }

    public void UseCharacter(int index)
    {
        UseIndexList.Add(index);
    }

    public bool IsLeftCharacter()
    {
        if (UseIndexList.Count >= deckData.DataDic.Count)
        {
            return false;
        }

        return true;
    }

    public void GetScore(int score)
    {
        this.Score += score;
    }
}

public class GameManager : MonoBehaviour
{
    public static Step CurStep;
    public static int CurRound;

    public Transform stoneRoot;
    public Transform playerStartPos;
    public Transform housePos;

    public Transform endLine1;
    public Transform endLine2;
    public Transform endLine3;
    public Transform endLine4;

    public GameObject characterPrefab1;
    public GameObject characterPrefab2;

    public GameObject createPos;

    //거리 1 = 1.45f = 크기 중의 반지름 모두 동일
    public static float DISTACNE => basicData.Distacne;
    //물리 오브젝트들의 질량
    public static float MASS => basicData.Mass;
    //기본 마찰력
    public static float BASE_FRICTION => basicData.Base_Friction;
    //초당 마찰력 회복 수치
    public static float FRICTION => basicData.Friction;
    //스윕시 감소시킬 마찰력
    public static float SWEEP => basicData.Sweep;
    //스윕으로 감소되는 마찰력의 최솟값
    public static float SWEEP_MAX => basicData.Sweep_Max;
    //스윕으로 판정되는 최소 거리
    public static float SWEEP_MIN_DISTACNE => basicData.Sweep_Min_Distance;
    //총 라운드 수
    public static float ROUND_COUNT => basicData.Round_Count;
    //던진 스톤이 멈춘 후 잠시 대기할 시간
    public static float ROUND_WAIT_TIME => basicData.Round_Wait_Time;
    //라운드 남은 시간
    public static float Turn => basicData.Turn;
    //덱에 들어가는 캐릭터의 최대 수
    public static float DECK_CHARACTER_COUNT = 4;
    //하우스 가장 큰 원의 반지름
    public static float IN_HOUSE_DISTANCE = 18.288f;

    public Character CurrentCharacter { get; private set; }
    public List<Character> ChracterList { get; private set; }
    public List<CharacterCreateData> TestCharacterCreateDataList { get; private set; }

    public PlayerData Player1 { get; private set; }
    public PlayerData Player2 { get; private set; }
    public List<PlayerData> PlayerDataList { get; private set; }
    public Queue<PlayerData> PlayerSequenceQueue { get; private set; }

    public bool IsGameEnd { get; private set; }

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
            return instance;
        }
    }

    private static BasicData basicData;

    public void Awake()
    {
        ChracterList = new List<Character>();
        TestCharacterCreateDataList = new List<CharacterCreateData>();
        PlayerDataList = new List<PlayerData>();
        PlayerSequenceQueue = new Queue<PlayerData>();
    }

    void Start()
    {
        UIManager.Instance.Get<IntroUI>();

        basicData = (DataBaseManager.Instance.loader.GetDataBase("BasicDB") as BasicDataBase).GetBasicData();
    }

    public void GameInit()
    {
        createPos.SetActive(false);

        ChracterList.Clear();
        PlayerDataList.Clear();
        PlayerSequenceQueue.Clear();

        IsGameEnd = false;
        CurRound = 1;

        Player1 = new PlayerData(0, 0, Team.PLAYER1, DeckManager.Instance.CurDeck);
        Player2 = new PlayerData(1, 1, Team.PLAYER2, DeckManager.Instance.CurDeck);

        PlayerDataList.Add(Player1);
        PlayerDataList.Add(Player2);

        PlayerDataList.Sort((x, y) =>
        {
            return x.order.CompareTo(y.order);
        });

        foreach (var player in PlayerDataList)
        {
            EnqueuePlayerSequenceQueue(player);
        }

        NextSequencePlayerStart();
    }

    public void EnqueuePlayerSequenceQueue(PlayerData playerData)
    {
        PlayerSequenceQueue.Enqueue(playerData);
    }

    public void NextSequencePlayerStart()
    {
        None();

        if (IsGameEnd)
            return;

        if (PlayerSequenceQueue.Count > 0)
        {
            var player = PlayerSequenceQueue.Dequeue();

            var selectUI = UIManager.Instance.Get<GameCharacterSelectUI>() as GameCharacterSelectUI;
            selectUI.Init(CurRound, player, Player1, Player2);
        }
        else
        {
            //스코어 매니저 게임 매니저랑 합칠 예정
            ScoreManager.Instance.GetScoreProcess(ChracterList, housePos.position);
        }
    }

    public void GetScore(Team team, int score)
    {
        var findPlayer = PlayerDataList.Find(v => v.team == team);
        findPlayer?.GetScore(score);

        //전 라운드의 후공 플레이어
        var pevLastPlayer = PlayerDataList
            .Find(v => v.order == (PlayerDataList.Count - 1));

        var firstTeam = Team.NONE;
        var winTeam = Team.NONE;

        //후공 플레이어가 점수를 획득했다면
        if (findPlayer != null &&
            pevLastPlayer.index == findPlayer.index)
        {
            //역순으로 정렬
            PlayerDataList.Sort((x, y) =>
            {
                return y.order.CompareTo(x.order);
            });

            firstTeam = PlayerDataList.First().team;

            //전 라운드 순서의 역순으로 인큐
            for (int i = 0; i < PlayerDataList.Count; i++)
            {
                PlayerDataList[i].order = i;
                PlayerDataList[i].UseIndexList.Clear();
                EnqueuePlayerSequenceQueue(PlayerDataList[i]);
            }
        }
        //후공 플레이어가 점수를 획득하지 못했다면
        else
        {
            //전판 순서 그대로 정렬
            PlayerDataList.Sort((x, y) =>
            {
                return x.order.CompareTo(y.order);
            });

            firstTeam = PlayerDataList.First().team;

            //전 라운드 순서로 인큐
            for (int i = 0; i < PlayerDataList.Count; i++)
            {
                PlayerDataList[i].order = i;
                PlayerDataList[i].UseIndexList.Clear();
                EnqueuePlayerSequenceQueue(PlayerDataList[i]);
            }
        }

        SetCameraToHousePos();

        CurRound += 1;
        if (CurRound > ROUND_COUNT)
        {
            //게임 종료
            IsGameEnd = true;

            winTeam = Player1.Score > Player2.Score ? Team.PLAYER1 : Team.PLAYER2;

            StartCoroutine(NextRoundWait(
                (scoreUI) => scoreUI.SetWinLoseRoot(winTeam == Player1.team),
                () =>
                {
                    RemoveAllData();
                    None();

                    var selectUI = UIManager.Instance.IsUIOpened<GameCharacterSelectUI>();
                    if (selectUI != null)
                    {
                        selectUI.Close();
                    }

                    UIManager.Instance.Get<MainMenuUI>();
                }));
        }
        else
        {
            StartCoroutine(NextRoundWait(
                (scoreUI) => scoreUI.SetFirstAttackRoot(firstTeam == Player1.team),
                () => 
                {
                    RemoveAllData();
                    NextSequencePlayerStart();                    
                }));
        }
    }

    public void None()
    {
        CurStep = Step.NONE;
        Debug.Log("Step : " + CurStep);
    }

    public void Select()
    {
        CurStep = Step.SELECT;
        Debug.Log("Step : " + CurStep);
    }

    public void Ready()
    {
        CurStep = Step.READY;
        Debug.Log("Step : " + CurStep);

        CameraManager.Instance.InitCreatePos();
        PhysicsManager.Instance.Init();
    }

    public void Shoot(Character character, Vector3 dir, float force, float attackBouns)
    {
        CurStep = Step.SHOOT;
        Debug.Log("Step : " + CurStep);

        CameraManager.IsFixed = true;
        CameraManager.Instance.SetFollowTrans(character.transform);

        character.Physics.ApplyForce(dir, force, attackBouns);

        Sweep();
    }

    public void Sweep()
    {
        CurStep = Step.SWEEP;
        Debug.Log("Step : " + CurStep);

        UIManager.Instance.Get<SweepUI>();
    }

    public void Move()
    {
        CurStep = Step.MOVE;
        Debug.Log("Step : " + CurStep);

        var sweepUI = UIManager.Instance.IsUIOpened<SweepUI>();
        if (sweepUI != null)
        {
            sweepUI.Close();
        }
    }

    public void End()
    {
        CurStep = Step.END;
        Debug.Log("Step : " + CurStep);

        StartCoroutine(NextPlayerWait());
    }

    public IEnumerator NextPlayerWait()
    {
        yield return new WaitForSeconds(ROUND_WAIT_TIME);

        NextSequencePlayerStart();
    }

    public IEnumerator NextRoundWait(Action<GameScoreUI> callback1, Action callback2)
    {
        var selectUI = UIManager.Instance.Get<GameCharacterSelectUI>() as GameCharacterSelectUI;
        selectUI.HideMenu();

        var gameScoreUI = UIManager.Instance.Get<GameScoreUI>() as GameScoreUI;
        gameScoreUI.Init(Player1, Player2);

        callback1?.Invoke(gameScoreUI);

        yield return new WaitForSeconds(ROUND_WAIT_TIME);

        gameScoreUI.Close();
        callback2?.Invoke();
    }

    public void TestSceneStart()
    {
        var testUI = UIManager.Instance.Get<TestBattleSceneUI>() as TestBattleSceneUI;
        testUI?.Init();

        createPos.SetActive(true);
    }

    public Character AddCharacter(Team team, CharacterData data, Vector3 pos, bool isPlayerChacter)
    {
        var newData = new CharacterCreateData(team, data, pos, isPlayerChacter);

        TestCharacterCreateDataList.Add(newData);

        return CreateCharacter(newData);
    }

    public Character CreateCharacter(CharacterCreateData createData)
    {
        GameObject prefab = null;

        if (createData.team == Team.PLAYER1)
            prefab = characterPrefab1;
        else if (createData.team == Team.PLAYER2)
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

        return newCharacter;
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
                    if (activeObj != null)
                    {
                        CameraManager.Instance.SetFollowTrans(activeObj.transform);
                    }
                    else CameraManager.Instance.SetFollowTrans(housePos);

                    CurrentCharacter = null;
                }
            }

            ChracterList.Remove(findCharacter);
            findCharacter.RemoveCharacterPhysics();
            Destroy(findCharacter.gameObject);
            findCharacter = null;
        }
    }

    public void RemoveTestCreateCharacterData(Team team, CharacterData data, Vector3 pos)
    {
        var findData = TestCharacterCreateDataList.Find(
            v => v.team == team &&
            v.data.id == data.id &&
            v.pos.x == pos.x &&
            v.pos.z == pos.z);

        TestCharacterCreateDataList.Remove(findData);
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

        TestCharacterCreateDataList.Clear();
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

        foreach (var data in TestCharacterCreateDataList)
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

    public void SetCameraToHousePos()
    {
        CameraManager.Instance.Init(housePos.position);
    }

    public void GameForceEnd()
    {
        RemoveAllData();
        SetCameraToHousePos();
        None();

        IsGameEnd = true;

        var sweepUI = UIManager.Instance.IsUIOpened<SweepUI>();
        if (sweepUI != null)
        {
            sweepUI.Close();
        }
    }

    public bool IsInIcePlate(Vector3 pos)
    {
        if (IsInIcePlate_X1(pos.x) &&
            IsInIcePlate_X2(pos.x) &&
            IsInIcePlate_Z1(pos.z) &&
            IsInIcePlate_Z2(pos.z))
        {
            return true;
        }

        return false;
    }

    public bool IsInIcePlate_X1(float x)
    {
        if (x < endLine1.position.x)
        {
            return false;
        }

        return true;
    }

    public bool IsInIcePlate_X2(float x)
    {
        if (x > endLine2.position.x)
        {
            return false;
        }

        return true;
    }

    public bool IsInIcePlate_Z1(float z)
    {
        if (z > endLine3.position.z)
        {
            return false;
        }

        return true;
    }

    public bool IsInIcePlate_Z2(float z)
    {
        if (z < endLine4.position.z)
        {
            return false;
        }

        return true;
    }
}
