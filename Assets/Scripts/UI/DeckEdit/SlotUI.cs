﻿using System;
using UnityEngine;

public class SlotUI : MonoBehaviour
{
    public UILabel slotName;
    public GameObject selectHighlightRoot;
    
    public int Index { get; private set; }
    public CharacterData Data { get; private set; }
    private Action<SlotUI> clickAction;

    public void Init(int index, CharacterData data, string slotName, Action<SlotUI> clickAction)
    {
        this.Index = index;
        this.Data = data;
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
