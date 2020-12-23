using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameReadyUI : UIBase
{
    public DeckSlotUI deckSlotPrefab;
    public UIGrid uiGrid;
    public UICenterOnChild centerOnChild;
    public UIScrollView scrollView;

    private Dictionary<int, DeckSlotUI> deckSlotDic;
    private DeckSlotUI curSelectedMainSlot;

    public void Awake()
    {
        deckSlotDic = new Dictionary<int, DeckSlotUI>();
    }

    public void Start()
    {
        var deckDataDic = DeckManager.Instance.DeckDic;

        var deckKeyList = deckDataDic.Keys.ToList();
        deckKeyList.Sort();

        foreach (var key in deckKeyList)
        {
            var newDeckSlot = Instantiate(deckSlotPrefab, uiGrid.transform);
            newDeckSlot.Init(key, deckDataDic[key], ClickDeckSlot);
            newDeckSlot.gameObject.SetActive(true);

            deckSlotDic.Add(key, newDeckSlot);
        }

        int index = DeckManager.Instance.CurDeckIndex;
        if (deckSlotDic.ContainsKey(index))
        {
            curSelectedMainSlot = deckSlotDic[index];
            curSelectedMainSlot.ClickSlot();
        }

        centerOnChild.onCenter += MoveToCurrentDeckPos;
    }

    public void MoveToCurrentDeckPos(GameObject centeredObject)
    {
        centerOnChild.onCenter -= MoveToCurrentDeckPos;

        SpringPanel.Stop(scrollView.gameObject);
        centerOnChild.CenterOn(curSelectedMainSlot.transform);        
    }

    public void ClickDeckSlot(DeckSlotUI deckSlot)
    {
        curSelectedMainSlot?.ActiveHighlight(false);

        curSelectedMainSlot = deckSlot;
        curSelectedMainSlot?.ActiveHighlight(true);

        int deckIndex = curSelectedMainSlot.Index;
        DeckManager.Instance.ChangeUseDeck(deckIndex);
    }

    public void ClickDeckEditButton()
    {
        Close();
        UIManager.Instance.Get<DeckEditUI>();
    }

    public void ClickReadyButton()
    {
        Close();
        UIManager.Instance.Get<GameCharacterSelectUI>();
    }

    public void ClickBackButton()
    {
        Close();
        UIManager.Instance.Get<MainMenuUI>();
    }
}
