using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCharacterSelectUI : UIBase
{
    public UILabel player1Name;
    public UILabel player2Name;

    public UILabel turnCount;
    public UILabel leftTime;
    public UILabel roundCount;

    public UILabel player1Score;
    public UILabel player2Score;

    public GameObject menuRoot;
    public GameObject mainRoot;
    public GameObject characterInfoRoot;
    public GameObject profileRoot;
    public GameObject pauseRoot;
    public GameObject homePopupRoot;
    public GameObject characterPosSelectRoot;
    public GameObject readyButtonRoot;
    public GameObject readyCancelButtonRoot;

    public UILabel profilePlayerName;
    public SlotUI[] profileSlotArray;
    public UILabel profileDeckEffect;

    public SlotUI[] slotArray;

    private PlayerData player1;
    private PlayerData player2;

    private SlotUI curSelectSlot;
    private PlayerData playerData;
    private Character tempCharacter;
    private float leftTimer = 0;

    public void Init(int round, PlayerData curPlayerData, PlayerData player1, PlayerData player2)
    {
        var curDeck = curPlayerData.deckData;

        this.player1Name.text = string.Format("{0}", player1.team);
        this.player2Name.text = string.Format("{0}", player2.team);

        this.turnCount.text = string.Format("{0}/{1}",
            curPlayerData.UseIndexList.Count + 1, curDeck.DataDic.Count);

        this.leftTime.text = this.leftTimer.ToString();

        this.roundCount.text = string.Format("{0}/{1}",
            round, GameManager.ROUND_COUNT);

        this.player1Score.text = player1.Score.ToString();
        this.player2Score.text = player2.Score.ToString();

        this.player1 = player1;
        this.player2 = player2;

        this.curSelectSlot = null;
        this.playerData = curPlayerData;
        this.tempCharacter = null;
        this.leftTimer = GameManager.Turn;

        for (int i = 0; i < slotArray.Length; i++)
        {
            if (curDeck.DataDic.ContainsKey(i))
            {
                var data = curDeck.DataDic[i];
                slotArray[i].Init(i, data, data.NAME, ClickSlot, PressSlot);

                if (curPlayerData.UseIndexList.Contains(i))
                {
                    slotArray[i].ActiveBlock(true);
                }
            }
        }

        CameraManager.Instance.InitCreatePos();
        GameManager.Instance.Select();

        menuRoot.SetActive(true);
        mainRoot.SetActive(true);
        characterInfoRoot.SetActive(false);
        profileRoot.SetActive(false);
        pauseRoot.SetActive(false);
        homePopupRoot.SetActive(false);
        characterPosSelectRoot.SetActive(false);
        readyButtonRoot.SetActive(false);
        readyCancelButtonRoot.SetActive(false);
    }

    public void Update()
    {
        if (GameManager.CurStep != Step.SELECT &&
            GameManager.CurStep != Step.READY)
            return;

        if (leftTime != null && leftTimer > 0)
        {
            leftTimer -= Time.deltaTime;

            if (leftTimer <= 0)
                leftTimer = 0;

            leftTime.text = leftTimer.ToString("00.00");
        }
    }

    public void CreateCharacter(Vector3 pos)
    {
        var team = playerData.team;
        //var pos = GameManager.Instance.playerStartPos;

        tempCharacter = GameManager.Instance.AddCharacter(team, curSelectSlot.Data, pos, true);

        List<SkillData> synergySkillList = new List<SkillData>();
        float synergyAtkValue = 0, synergyDefValue = 0;

        foreach (var data in playerData.deckData.SynergySpeciesDataList)
        {
            switch (data.applyObject)
            {
                case 1:
                    //인간
                    if (tempCharacter.Data.speciesData.ID == 0)
                    {
                        synergyAtkValue += data.attackValue;
                        synergyDefValue += data.defenceValue;
                    }
                    break;
                case 2:
                    //신
                    if (tempCharacter.Data.speciesData.ID == 1)
                    {
                        synergyAtkValue += data.attackValue;
                        synergyDefValue += data.defenceValue;
                    }
                    break;
                //모두
                case 6:
                    {
                        synergyAtkValue += data.attackValue;
                        synergyDefValue += data.defenceValue;
                    }
                    break;
            }

            if (data.skillData != null)
                synergySkillList.Add(data.skillData);
        }

        foreach (var data in playerData.deckData.SynergyAffiliationDataList)
        {
            switch (data.applyObject)
            {
                case 2:
                    //기사단
                    if (tempCharacter.Data.affiliationData.ID == 0)
                    {
                        synergyAtkValue += data.attackValue;
                        synergyDefValue += data.defenceValue;
                    }
                    break;
                case 3:
                    //교단
                    if (tempCharacter.Data.affiliationData.ID == 1)
                    {
                        synergyAtkValue += data.attackValue;
                        synergyDefValue += data.defenceValue;
                    }
                    break;
                case 4:
                    //천사
                    if (tempCharacter.Data.affiliationData.ID == 2)
                    {
                        synergyAtkValue += data.attackValue;
                        synergyDefValue += data.defenceValue;
                    }
                    break;
                //모두
                case 6:
                    {
                        synergyAtkValue += data.attackValue;
                        synergyDefValue += data.defenceValue;
                    }
                    break;
            }

            if (data.skillData != null)
                synergySkillList.Add(data.skillData);
        }

        //직업 시너지는 모든 아군 적용
        foreach (var data in playerData.deckData.SynergyJobDataList)
        {
            synergyAtkValue += data.attackValue;
            synergyDefValue += data.defenceValue;

            if (data.skillData != null)
                synergySkillList.Add(data.skillData);
        }

        tempCharacter.SetSynergyValue(synergyAtkValue, synergyDefValue, synergySkillList);

        //초기화 이벤트
        tempCharacter.InitEvent();

        //사용한 캐릭터가 하나도 없을경우
        if (playerData.UseIndexList.Count == 0)
        {
            tempCharacter.FirstShootEvent();
        }
    }

    public void ClickSlot(SlotUI slotUI)
    {
        if (curSelectSlot != null &&
            curSelectSlot.Index == slotUI.Index)
            return;

        curSelectSlot?.ActiveHighlight(false);
        curSelectSlot = slotUI;

        if (tempCharacter != null)
        {
            GameManager.Instance.RemoveCharacter(tempCharacter.Physics.PID);
            tempCharacter = null;
        }

        readyButtonRoot.SetActive(tempCharacter != null);
        readyCancelButtonRoot.SetActive(false);
        characterPosSelectRoot.SetActive(true);

        CameraManager.Instance.InitCreatePos();
        CameraManager.IsDragAble = false;        
    }

    public void ClickCharacterPosSelectRange()
    {
        var clickWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CreateCharacter(clickWorldPos);

        CameraManager.IsDragAble = true;
        CameraManager.Instance.DragScreen_X(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        readyButtonRoot.SetActive(tempCharacter != null);
        readyCancelButtonRoot.SetActive(true);
        characterPosSelectRoot.SetActive(false);
    }

    public void ClickSelectCancelButton()
    {
        curSelectSlot?.ActiveHighlight(false);
        curSelectSlot = null;

        if (tempCharacter != null)
        {
            GameManager.Instance.RemoveCharacter(tempCharacter.Physics.PID);
            tempCharacter = null;
        }

        readyButtonRoot.SetActive(false);
        readyCancelButtonRoot.SetActive(false);
    }

    public void PressSlot(SlotUI slotUI, bool state)
    {
        characterInfoRoot.SetActive(state);
    }

    public void ClickSelectButton()
    {
        if (tempCharacter == null)
            return;

        if (playerData.UseIndexList.Contains(curSelectSlot.Index))
            return;

        //캐릭터 사용 처리
        playerData.UseCharacter(curSelectSlot.Index);

        //아직 사용 가능한 캐릭터가 남아있다면
        if (playerData.IsLeftCharacter())
        {
            GameManager.Instance.EnqueuePlayerSequenceQueue(playerData);
        }

        menuRoot.SetActive(true);
        mainRoot.SetActive(false);
        profileRoot.SetActive(false);
        pauseRoot.SetActive(false);

        GameManager.Instance.Ready();
    }

    public void ClickPlayer1Profile()
    {
        InitPlayerProfile(player1);
    }

    public void ClickPlayer2Profile()
    {
        InitPlayerProfile(player2);
    }

    public void InitPlayerProfile(PlayerData player)
    {
        menuRoot.SetActive(false);
        mainRoot.SetActive(false);
        profileRoot.SetActive(true);

        profilePlayerName.text = player.team.ToString();
        profileDeckEffect.text = player.deckData.DeckSynergyText;

        var dataDic = player.deckData.DataDic;

        for (int i = 0; i < profileSlotArray.Length; i++)
        {
            CharacterData charData = null;
            string slotName = "없음";

            if (dataDic != null && dataDic.ContainsKey(i))
            {
                charData = dataDic[i];
                slotName = string.Format("{0} 등급\n{1}",
                    charData.rarityData.rarity, charData.name);
            }

            profileSlotArray[i].Init(i, charData, slotName);
        }
    }

    public void ClickProfileBackButton()
    {
        menuRoot.SetActive(true);
        mainRoot.SetActive(true);
        profileRoot.SetActive(false);
    }

    public void ClickPauseButton()
    {
        pauseRoot.SetActive(true);
    }

    public void ClickPauseBackButton()
    {
        pauseRoot.SetActive(false);
    }

    public void ClickPauseGoHomeButton()
    {
        homePopupRoot.SetActive(true);
    }

    public void ClickGoHomeYesButton()
    {
        GameManager.Instance.GameForceEnd();      

        Close();
        UIManager.Instance.Get<MainMenuUI>();
    }

    public void ClickGoHomeNoButton()
    {
        homePopupRoot.SetActive(false);
    }

    public void HideMenu()
    {
        menuRoot.SetActive(false);
    }
}
