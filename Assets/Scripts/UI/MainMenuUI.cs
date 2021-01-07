using UnityEngine;

public class MainMenuUI : BaseUI
{
    public GameObject mainRoot;
    public GameObject gameRoot;

    public void Start()
    {
        mainRoot.SetActive(true);
        gameRoot.SetActive(false);
    }

    public override void Close()
    {
        if (gameRoot.activeSelf)
        {
            ClickBackButton();
        }
        else
        {
            var popup = UIManager.Instance.Get<BasePopupUI>() as BasePopupUI;
            popup.Init("정말 게임을 종료할까요?",
                () =>
                {
                    Application.Quit();
                });
        }
    }

    public void ClickGameStartButton()
    {
        base.Close();
        UIManager.Instance.Get<GameReadyUI>();
    }

    public void ClickDeckEditButton()
    {
        base.Close();
        UIManager.Instance.Get<DeckEditUI>();
    }

    public void ClickTestSceneButton()
    {
        base.Close();
        GameManager.Instance.TestSceneStart();
    }

    public void ClickGameButton()
    {
        //mainRoot.SetActive(false);
        gameRoot.SetActive(true);
    }

    public void ClickBackButton()
    {
        mainRoot.SetActive(true);
        gameRoot.SetActive(false);
    }
}
