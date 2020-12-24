using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCharacterSelectUI : UIBase
{
    public UILabel roundCount;
    public UILabel title;
    public SlotUI[] slotArray;

    private SlotUI curSelectSlot;
    private PlayerData playerData;
    private Character tempCharacter;

    public void Init(int round, PlayerData playerData)
    {
        this.roundCount.text = round.ToString();
        this.title.text = playerData.team.ToString();

        this.playerData = playerData;
        this.tempCharacter = null;

        var curDeck = playerData.deckData;

        for (int i = 0; i < slotArray.Length; i++)
        {
            if (curDeck.DataDic.ContainsKey(i))
            {
                var data = curDeck.DataDic[i];
                slotArray[i].Init(i, data, data.NAME, ClickSlot);

                if (playerData.UseIndexList.Contains(i))
                {
                    slotArray[i].ActiveBlock(true);
                }                
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

        var team = playerData.team;
        var pos = GameManager.Instance.playerStartPos;

        tempCharacter = GameManager.Instance.AddCharacter(team, curSelectSlot.Data, pos.position, true);
    }

    public void ClickSelectButton()
    {
        if (tempCharacter == null)
            return;

        if (playerData.UseIndexList.Contains(curSelectSlot.Index))
            return;

        //캐릭터 사용 처리
        playerData.UseCharacter(curSelectSlot.Index);

        //아직 사용 가능한 캐릭터가 남아있다면
        if (playerData.IsLeftCharacter())
        {
            GameManager.Instance.EnqueuePlayerSequenceQueue(playerData);
        }

        Close();
        GameManager.Instance.Ready();
    }
}
