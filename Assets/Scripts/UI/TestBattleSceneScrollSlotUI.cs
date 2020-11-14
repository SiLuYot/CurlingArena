using System;
using UnityEngine;

public class TestBattleSceneScrollSlotUI : MonoBehaviour
{
    public UILabel uILabel;
    private CharacterData data;
    private Action<TestBattleSceneScrollSlotUI, CharacterData> clickEvent;

    public void Init(CharacterData data, Action<TestBattleSceneScrollSlotUI, CharacterData> clickEvent)
    {
        this.data = data;
        this.clickEvent = clickEvent;

        uILabel.text = GetName();
    }

    public string GetName()
    {
        if (data == null)
            return string.Empty;

        return string.Format("{0} / {1} / {2}",
                data.id, data.rarity, data.name);
    }

    public void ClickSlot()
    {
        clickEvent?.Invoke(this, data);
    }
}
