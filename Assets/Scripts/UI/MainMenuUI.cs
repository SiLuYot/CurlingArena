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
        SceneManager.LoadScene("TestBattleScene");
    }

    public void ClickGameEndButton()
    {
        Application.Quit();
    }
}
