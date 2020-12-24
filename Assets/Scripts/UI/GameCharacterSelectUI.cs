using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCharacterSelectUI : UIBase
{
    public SlotUI[] slotArray;
    public SlotUI curSelectSlot;

    private Character tempCharacter;

    public void Start()
    {
        tempCharacter = null;
        var curDeck = DeckManager.Instance.CurDeck;

        for (int i = 0; i < slotArray.Length; i++)
        {
            if (curDeck.DataDic.ContainsKey(i))
            {
                var data = curDeck.DataDic[i];
                slotArray[i].Init(i, data, data.NAME, ClickSlot);
            }
        }

        CameraManager.Instance.InitSelectPos();
    }

    public void ClickSlot(SlotUI slotUI)
    {
        curSelectSlot?.ActiveHighlight(false);
        curSelectSlot = slotUI;

        if (tempCharacter != null)
        {
            GameManager.Instance.RemoveCharacter(tempCharacter.Physics.PID);
        }

        var team = Team.PLAYER;
        var pos = GameManager.Instance.playerStartPos;

        tempCharacter = GameManager.Instance.AddCharacter(team, curSelectSlot.Data, pos.position, true);
    }

    public void ClickSelectButton()
    {
        if (tempCharacter == null)
            return;

        Close();
        GameManager.Instance.Ready();
    }
}
