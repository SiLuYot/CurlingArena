﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCharacterSelectUI : UIBase
{
    public UILabel player1Name;
    public UILabel player2Name;

    public UILabel turnCount;
    public UILabel roundCount;

    public UILabel player1Score;
    public UILabel player2Score;

    public GameObject menuRoot;
    public GameObject mainRoot;
    public GameObject characterInfoRoot;
    public GameObject profileRoot;
    public GameObject pauseRoot;
    public GameObject homePopupRoot;

    public UILabel profilePlayerName;
    public SlotUI[] profileSlotArray;
    public UILabel profileDeckEffect;

    public SlotUI[] slotArray;

    private PlayerData player1;
    private PlayerData player2;

    private SlotUI curSelectSlot;
    private PlayerData playerData;
    private Character tempCharacter;

    public void Init(int round, PlayerData curPlayerData, PlayerData player1, PlayerData player2)
    {
        var curDeck = curPlayerData.deckData;

        this.player1Name.text = string.Format("{0} :", player1.team);
        this.player2Name.text = string.Format("{0} :", player2.team);

        this.turnCount.text = string.Format("{0}/{1}",
            curPlayerData.UseIndexList.Count + 1, curDeck.DataDic.Count);
        this.roundCount.text = string.Format("{0}/{1}",
            round, GameManager.ROUND_COUNT);

        this.player1Score.text = player1.Score.ToString();
        this.player2Score.text = player2.Score.ToString();

        this.player1 = player1;
        this.player2 = player2;

        this.curSelectSlot = null;
        this.playerData = curPlayerData;
        this.tempCharacter = null;

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

        CameraManager.Instance.InitSelectPos();

        menuRoot.SetActive(true);
        mainRoot.SetActive(true);
        characterInfoRoot.SetActive(false);
        profileRoot.SetActive(false);
        pauseRoot.SetActive(false);
        homePopupRoot.SetActive(false);
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
        }

        var team = playerData.team;
        var pos = GameManager.Instance.playerStartPos;

        tempCharacter = GameManager.Instance.AddCharacter(team, curSelectSlot.Data, pos.position, true);

        var findData = playerData.deckData.SynergyDataList.Find(v => v.species == curSelectSlot.Data.speciesData.ID);
        if (findData != null)
        {
            tempCharacter.SetSynergyValue(findData.synergyAtkValue, findData.synergyDefValue);
        }
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
}
