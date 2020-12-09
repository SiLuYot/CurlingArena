using System;
using UnityEngine;

public class DeckSlotUI : MonoBehaviour
{
    public UILabel deckName;
    public GameObject selectHighlightRoot;

    public DeckData Data { get; private set; }
    private Action<DeckSlotUI> clickAction;

    public void Init(DeckData data, Action<DeckSlotUI> clickAction)
    {
        this.Data = data;
        this.deckName.text = data?.DeckName;
        this.clickAction = clickAction;

        if (data == null)
        {
            this.deckName.text = "비어있음";
        }

        ActiveHighlight(false);
    }

    public void ActiveHighlight(bool active)
    {
        if (selectHighlightRoot != null)
            selectHighlightRoot.SetActive(active);
    }

    public void ClickSlot()
    {
        clickAction?.Invoke(this);
    }
}
