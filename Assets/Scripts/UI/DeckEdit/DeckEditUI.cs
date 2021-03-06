﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckEditUI : BaseUI
{
    public GameObject mainRoot;
    public GameObject deckEditRoot;
    public GameObject SlotEditRoot;

    public DeckSlotUI[] mainSlotUIArray;
    public SlotUI[] mainInfoSlotUIArray;
    public UILabel mainInfoLabel;

    public SlotUI[] deckEditSlotUIArray;
    public UILabel deckEditInfoLabel;
    public UIInput deckEditNameInput;

    public UISprite slotEditInfoSprite;
    public UILabel slotEditInfoLabel;
    public UILabel slotEditCharacterInfoLabel;
    public SlotUI slotEditSlotPrefab;
    public UIGrid scrollRoot;    
    private bool isInitEditSlotList;

    //저장 전 임시 덱
    private DeckData tempDeckData;
    //선택된 덱
    private DeckSlotUI curSelectedMainSlot;
    //현재 덱의 선택된 슬롯
    private SlotUI curSelectedEditDeckSlot;
    //덱에 넣을 슬롯 데이터
    private SlotUI curSelectedEditSlot;

    public void Start()
    {
        tempDeckData = null;
        isInitEditSlotList = false;

        ActiveMainUI();
    }

    public void ActiveMainUI()
    {
        //덱 세팅
        var deckDataDic = DeckManager.Instance.DeckDic;
        for (int i = 0; i < mainSlotUIArray.Length; i++)
        {
            DeckData deckData = null;
            if (deckDataDic.ContainsKey(i))
            {
                deckData = deckDataDic[i];
            }

            //데이터가 없다면 비어있는 덱
            mainSlotUIArray[i].Init(i, deckData, ClickDeckSlot);
        }

        //사용중인 덱 선택
        if (curSelectedMainSlot == null)
        {
            int index = DeckManager.Instance.CurDeckIndex;
            curSelectedMainSlot = mainSlotUIArray.ElementAtOrDefault(index);
        }
        curSelectedMainSlot?.ClickSlot();

        //각 UI 루트 활성화/비활성화
        mainRoot.SetActive(true);
        deckEditRoot.SetActive(false);
        SlotEditRoot.SetActive(false);

        deckEditNameInput.value = string.Empty;
    }

    public void ActiveDeckEditUI()
    {
        string infoText = "효과없음";
        string nameText = "비어있음";
        Dictionary<int, CharacterData> dataDic = null;

        if (tempDeckData != null)
        {
            dataDic = tempDeckData.DataDic;

            infoText = tempDeckData.DeckSynergyText;
            nameText = tempDeckData.DeckName;
        }

        deckEditInfoLabel.text = infoText;
        deckEditNameInput.label.text = nameText;

        for (int i = 0; i < deckEditSlotUIArray.Length; i++)
        {
            CharacterData charData = null;
            string slotName = "없음";

            if (dataDic != null && dataDic.ContainsKey(i))
            {
                charData = dataDic[i];
                slotName = string.Format("{0} 등급\n{1}",
                    charData.rarityData.rarity, charData.name);
            }

            deckEditSlotUIArray[i].Init(i, charData, slotName, ClickDeckEditSlot);
        }

        //각 UI 루트 활성화/비활성화
        mainRoot.SetActive(false);
        deckEditRoot.SetActive(true);
        SlotEditRoot.SetActive(false);
    }

    public void ActiveSlotEditUI()
    {
        if (!isInitEditSlotList)
        {
            isInitEditSlotList = true;

            //인벤에 그냥 캐릭다 넣어둠
            var dataList = DataBaseManager.Instance.loader.GetDataList("CharacterDB");

            for (int i = 0; i < dataList.Count; i++)
            {
                var data = dataList[i] as CharacterData;

                var newSlot = Instantiate(slotEditSlotPrefab, scrollRoot.transform);
                newSlot.Init(i, data, data.NAME, ClickSlotEditSlot);
                newSlot.gameObject.SetActive(true);
            }
        }

        slotEditInfoLabel.text = "없음";
        slotEditCharacterInfoLabel.text = string.Empty;
        slotEditInfoSprite.gameObject.SetActive(false);

        //각 UI 루트 활성화/비활성화
        mainRoot.SetActive(false);
        deckEditRoot.SetActive(false);
        SlotEditRoot.SetActive(true);
    }

    public void ClickDeckEditButton()
    {
        if (curSelectedMainSlot == null)
            return;

        if (curSelectedMainSlot.Data == null)
        {
            tempDeckData = new DeckData("new deck");
        }
        else
        {
            tempDeckData = new DeckData(curSelectedMainSlot.Data.DeckName);
            tempDeckData.CopyData(curSelectedMainSlot.Data.DataDic);
            DeckManager.Instance.CheckDeckEffect(tempDeckData);
        }

        ActiveDeckEditUI();
    }

    public void ClickDeckSelectButton()
    {
        if (curSelectedMainSlot == null)
            return;

        if (curSelectedMainSlot.Data == null)
            return;

        if (!curSelectedMainSlot.Data.IsCompleteDeck())
            return;

        int deckIndex = curSelectedMainSlot.Index;
        if (DeckManager.Instance.ChangeUseDeck(deckIndex))
        {
            ClickExitButton();
        }
    }

    public void ClickDeckSlot(DeckSlotUI slot)
    {
        curSelectedMainSlot?.ActiveHighlight(false);

        curSelectedMainSlot = slot;
        curSelectedMainSlot?.ActiveHighlight(true);

        string infoText = "비어있음";
        Dictionary<int, CharacterData> dataDic = null;

        if (curSelectedMainSlot.Data != null)
        {
            dataDic = curSelectedMainSlot.Data.DataDic;
            infoText = curSelectedMainSlot.Data.DeckSynergyText;
        }

        mainInfoLabel.text = infoText;

        for (int i = 0; i < mainInfoSlotUIArray.Length; i++)
        {
            CharacterData charData = null;
            string slotName = "없음";

            if (dataDic != null && dataDic.ContainsKey(i))
            {
                charData = dataDic[i];
                slotName = charData.NAME;
            }

            mainInfoSlotUIArray[i].Init(i, charData, slotName, null);
        }
    }

    public void ClickEditCancelButton()
    {
        tempDeckData = null;

        ActiveMainUI();
    }

    public void ClickEditSaveButton()
    {
        var deckDataDic = DeckManager.Instance.DeckDic;

        int index = curSelectedMainSlot.Index;
        if (deckDataDic.ContainsKey(index))
        {
            deckDataDic[index].CopyData(tempDeckData.DataDic);
            deckDataDic[index].ChangeDeckName(tempDeckData.DeckName);
            DeckManager.Instance.CheckDeckEffect(deckDataDic[index]);
        }
        else DeckManager.Instance.AddNewDeck(index, tempDeckData);

        tempDeckData = null;

        ActiveMainUI();
    }

    public void ClickEditDeckNameButton()
    {
        if (curSelectedMainSlot == null)
            return;

        var newName = deckEditNameInput.label.text;
        tempDeckData.ChangeDeckName(newName);

        var deckDataDic = DeckManager.Instance.DeckDic;

        int index = curSelectedMainSlot.Index;
        if (deckDataDic.ContainsKey(index))
        {
            deckDataDic[index].ChangeDeckName(tempDeckData.DeckName);
        }
        else DeckManager.Instance.AddNewDeck(index, tempDeckData);
    }

    public void ClickDeckEditSlot(SlotUI slot)
    {
        curSelectedEditDeckSlot = slot;

        ActiveSlotEditUI();
    }

    public void ClickSlotEditSelect()
    {
        if (curSelectedEditDeckSlot == null)
            return;

        if (curSelectedEditSlot == null)
            return;

        int index = curSelectedEditDeckSlot.Index;
        if (tempDeckData.DataDic.ContainsKey(index))
        {
            tempDeckData.DataDic[index] = curSelectedEditSlot.Data;
        }
        else tempDeckData.AddData(index, curSelectedEditSlot.Data);

        DeckManager.Instance.CheckDeckEffect(tempDeckData);

        curSelectedEditSlot.ActiveHighlight(false);
        curSelectedEditSlot = null;

        ActiveDeckEditUI();
    }

    public void ClickSlotEditCancel()
    {
        curSelectedEditSlot?.ActiveHighlight(false);
        curSelectedEditSlot = null;

        ActiveDeckEditUI();
    }

    public void ClickSlotEditSlot(SlotUI slot)
    {
        curSelectedEditSlot?.ActiveHighlight(false);

        curSelectedEditSlot = slot;
        //curSelectedEditSlot?.ActiveHighlight(true);

        var charData = curSelectedEditSlot.Data;
        if (charData.skillDataArray != null)
        {
            string Info = string.Empty;

            var skillArray = charData.skillDataArray;
            for (int i = 0; i < skillArray.Length; i++)
            {
                if (skillArray[i].id != 0)
                {
                    if (Info != string.Empty)
                        Info = string.Format("{0}, ", Info);

                    Info = string.Format("{0}{1}", Info, skillArray[i].desc);
                }
            }

            if (Info == string.Empty)
                Info = "스킬 없음";

            slotEditInfoLabel.text = Info;

            slotEditCharacterInfoLabel.text =
                string.Format("이름 : {0}\n등급 : {1}\n\n공격력 : {2}\n방어력 : {3}\n\n크기 : {4}\n\n종족 : {5}\n소속 : {6}\n직업 : {7}",
                charData.name, charData.rarityData.NAME, 
                charData.attack, charData.defence, charData.sizeData.NAME, 
                charData.speciesData.NAME, charData.affiliationData.NAME, charData.jobData.NAME);
        }

        //스프라이트 그냥 활성화..
        slotEditInfoSprite.gameObject.SetActive(true);
    }

    public void ClickExitButton()
    {
        Close();
        UIManager.Instance.Get<MainMenuUI>();
    }

    public override void Close()
    {
        if (SlotEditRoot.activeSelf)
        {
            ClickSlotEditCancel();
        }
        else if (deckEditRoot.activeSelf)
        {
            ClickEditCancelButton();
        }
        else if (mainRoot.activeSelf)
        {
            base.Close();
            UIManager.Instance.Get<MainMenuUI>();
        }
    }
}
