using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScoreUI : BaseUI
{
    public UILabel player1Name;
    public UILabel player2Name;

    public UILabel player1Score;
    public UILabel player2Score;

    public SlotUI[] player1SlotArray;
    public SlotUI[] player2SlotArray;

    public GameObject player1WinRoot;
    public GameObject player2WinRoot;

    public GameObject player1LoseRoot;
    public GameObject player2LoseRoot;

    public GameObject player1FirstAttackRoot;
    public GameObject player2FirstAttackRoot;

    public void Init(PlayerData player1, PlayerData player2)
    {
        this.player1Name.text = player1.team.ToString();
        this.player2Name.text = player2.team.ToString();

        this.player1Score.text = player1.Score.ToString();
        this.player2Score.text = player2.Score.ToString();

        var dataDic1 = player1.deckData.DataDic;
        for (int i = 0; i < player1SlotArray.Length; i++)
        {
            CharacterData charData = null;
            string slotName = "없음";

            if (dataDic1 != null && dataDic1.ContainsKey(i))
            {
                charData = dataDic1[i];
                slotName = charData.name;
            }
            player1SlotArray[i].Init(i, charData, slotName);
        }

        var dataDic2 = player2.deckData.DataDic;
        for (int i = 0; i < player2SlotArray.Length; i++)
        {
            CharacterData charData = null;
            string slotName = "없음";

            if (dataDic2 != null && dataDic2.ContainsKey(i))
            {
                charData = dataDic2[i];
                slotName = charData.name;
            }
            player2SlotArray[i].Init(i, charData, slotName);
        }

        player1FirstAttackRoot.SetActive(false);
        player2FirstAttackRoot.SetActive(false);
        player1WinRoot.SetActive(false);
        player2WinRoot.SetActive(false);
        player1LoseRoot.SetActive(false);
        player2LoseRoot.SetActive(false);
    }

    public void SetFirstAttackRoot(bool isPlayer1FirstAttack)
    {
        player1FirstAttackRoot.SetActive(isPlayer1FirstAttack);
        player2FirstAttackRoot.SetActive(!isPlayer1FirstAttack);
    }

    public void SetWinLoseRoot(bool isPlayer1Win)
    {
        player1WinRoot.SetActive(isPlayer1Win);
        player2WinRoot.SetActive(!isPlayer1Win);

        player1LoseRoot.SetActive(!isPlayer1Win);
        player2LoseRoot.SetActive(isPlayer1Win);
    }

    public void NextRoundWaitEnd()
    {
        base.Close();
    }

    public override void Close()
    {
        var popup = UIManager.Instance.Get<BasePopupUI>() as BasePopupUI;
        popup.Init("정말 게임을 포기할까요?",
            () =>
            {
                GameManager.Instance.GameForceEnd();

                base.Close();
                UIManager.Instance.Get<MainMenuUI>();
            });
    }
}
