using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckEditUI : UIBase
{
    public GameObject mainRoot;
    public GameObject deckEditRoot;
    public GameObject SlotEditRoot;

    public DeckSlotUI[] deckSlotUIArray;
    private DeckSlotUI curSelectedSlot;

    public SlotUI[] infoSlotUIArray;

    public UILabel infoLabel;

    public void Start()
    {
        ActiveMainUI();
    }

    public void ActiveMainUI()
    {
        //테스트 용
        DeckManager.Instance.AddTestDeck();

        //덱 세팅
        var deckDataDic = DeckManager.Instance.DeckDic;
        for (int i = 0; i < deckSlotUIArray.Length; i++)
        {
            DeckData deckData = null;
            if (deckDataDic.ContainsKey(i))
            {
                deckData = deckDataDic[i];
            }

            //데이터가 없다면 비어있는 덱
            deckSlotUIArray[i].Init(deckData, ClickDeckSlot);
        }

        //가장 처음 덱을 기본으로 선택한다.
        curSelectedSlot = deckSlotUIArray.FirstOrDefault();
        curSelectedSlot?.ClickSlot();

        //각 UI 루트 활성화/비활성화
        mainRoot.SetActive(true);
        deckEditRoot.SetActive(false);
        SlotEditRoot.SetActive(false);
    }

    public void ClickDeckSlot(DeckSlotUI slot)
    {
        curSelectedSlot?.ActiveHighlight(false);
        curSelectedSlot = slot;

        string infoText = "비어있음";
        Dictionary<int, CharacterData> dataDic = null;        

        if (curSelectedSlot.Data != null)
        {
            dataDic = curSelectedSlot.Data.DataDic;
            infoText = string.Format("{0}의 효과 출력 예정", curSelectedSlot.Data.DeckName);
        }

        infoLabel.text = infoText;

        for (int i = 0; i < infoSlotUIArray.Length; i++)
        {
            string slotName = "없음";
            if (dataDic != null && dataDic.ContainsKey(i))
            {
                slotName = dataDic[i].NAME;
            }

            infoSlotUIArray[i].Init(slotName, null);
        }

        curSelectedSlot?.ActiveHighlight(true);
    }

    public void ClickExitButton()
    {
        Close();
        UIManager.Instance.Get<MainMenuUI>();
    }
}
