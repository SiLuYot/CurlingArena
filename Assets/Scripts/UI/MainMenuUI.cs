using UnityEngine;

public class MainMenuUI : UIBase
{
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

    public void ClickGameEndButton()
    {
        Application.Quit();
    }
}
