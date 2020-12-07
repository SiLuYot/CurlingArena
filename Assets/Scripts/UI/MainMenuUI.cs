using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : UIBase
{
    public void ClickGameStartButton()
    {

    }

    public void ClickDeckEditButton()
    {

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
