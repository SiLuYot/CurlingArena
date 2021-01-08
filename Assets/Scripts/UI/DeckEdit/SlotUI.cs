using System;
using UnityEngine;

public class SlotUI : MonoBehaviour
{
    public UILabel slotName;
    public GameObject selectHighlightRoot;
    public GameObject blockRoot;
    public UISprite rankSprite;

    public int Index { get; private set; }
    public CharacterData Data { get; private set; }

    private Action<SlotUI> clickAction;
    private Action<SlotUI, bool> pressAction;

    private bool isPress;
    private float pressTime;

    public void Init(int index, CharacterData data, string slotName, Action<SlotUI> clickAction = null, Action<SlotUI, bool> pressAction = null)
    {       
        this.Index = index;
        this.Data = data;

        this.slotName.text = slotName;

        this.clickAction = clickAction;
        this.pressAction = pressAction;

        this.isPress = false;
        this.pressTime = 0;

        if (rankSprite != null)
        {
            Color color = Color.white;

            if (Data != null)
            {
                switch (Data.rarityData.id)
                {
                    //case 0:
                    //    break;
                    case 1:
                        color = Color.green;
                        break;
                    case 2:
                        color = new Color(1f, 0, 1f);
                        break;
                    case 3:
                        color = Color.red;
                        break;
                    case 4:
                        color = new Color(1f, 0.498f, 0);
                        break;
                    case 5:
                        color = Color.yellow;
                        break;
                }
            }

            rankSprite.color = color;
        }

        ActiveHighlight(false);
        ActiveBlock(false);
    }

    private void Update()
    {
        if (isPress)
        {
            pressTime += Time.deltaTime;
            if (pressTime > 0.5f)
            {
                pressAction?.Invoke(this, isPress);
            }
        }
    }

    public void ActiveHighlight(bool active)
    {
        if (selectHighlightRoot != null)
            selectHighlightRoot.SetActive(active);
    }

    public void ActiveBlock(bool active)
    {
        if (blockRoot != null)
            blockRoot.SetActive(active);
    }

    public void ClickSlot()
    {
        clickAction?.Invoke(this);

        ActiveHighlight(true);
    }

    public void PressSlot(bool state)
    {
        isPress = true;
        pressTime = 0;
        //pressAction?.Invoke(this, isPress);
    }

    public void ReleaseSlot(bool state)
    {
        isPress = false;
        pressAction?.Invoke(this, isPress);
    }
}
