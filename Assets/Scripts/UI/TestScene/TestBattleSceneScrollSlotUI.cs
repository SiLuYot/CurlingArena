using System;
using UnityEngine;

public class TestBattleSceneScrollSlotUI : MonoBehaviour
{
    public UILabel uILabel;

    private CharacterData data;
    private Action<TestBattleSceneScrollSlotUI, CharacterData> clickCharacterEvent;

    private Team team;
    private Action<TestBattleSceneScrollSlotUI, Team> clickTeamEvent;

    public void Init(CharacterData data, Action<TestBattleSceneScrollSlotUI, CharacterData> clickCharacterEvent)
    {
        this.data = data;
        this.clickCharacterEvent = clickCharacterEvent;

        uILabel.text = GetCharacterName();
    }

    public void Init(Team team, Action<TestBattleSceneScrollSlotUI, Team> clickTeamEvent)
    {
        this.team = team;
        this.clickTeamEvent = clickTeamEvent;

        uILabel.text = GetTeamName();
    }

    public string GetCharacterName()
    {
        if (data == null)
            return string.Empty;

        return string.Format("{0} / {1} / {2}",
                data.id, data.rarity, data.name);
    }

    public string GetTeamName()
    {
        return string.Format("{0}", team.ToString());
    }

    public void ClickCharacterSlot()
    {
        clickCharacterEvent?.Invoke(this, data);
    }

    public void ClickTeamSlot()
    {
        clickTeamEvent?.Invoke(this, team);
    }
}
