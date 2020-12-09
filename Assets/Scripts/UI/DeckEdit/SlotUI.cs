using System;
using UnityEngine;

public class SlotUI : MonoBehaviour
{
    public UILabel slotName;
    public GameObject selectHighlightRoot;

    private Action<SlotUI> clickAction;

    public void Init(string slotName, Action<SlotUI> clickAction)
    {
        this.slotName.text = slotName;
        this.clickAction = clickAction;

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
