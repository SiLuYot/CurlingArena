using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : UIBase
{
    public void ClickGameStartButton()
    {

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
