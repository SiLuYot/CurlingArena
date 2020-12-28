using UnityEngine;

public class MainMenuUI : UIBase
{
    public GameObject mainRoot;
    public GameObject gameRoot;

    public void Start()
    {
        mainRoot.SetActive(true);
        gameRoot.SetActive(false);
    }

    public void ClickGameStartButton()
    {
        Close();
        UIManager.Instance.Get<GameReadyUI>();
    }

    public void ClickDeckEditButton()
    {
        Close();
        UIManager.Instance.Get<DeckEditUI>();
    }

    public void ClickTestSceneButton()
    {
        Close();
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

    public void ClickGameEndButton()
    {
        Application.Quit();
    }
}
