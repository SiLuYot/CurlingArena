using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroUI : UIBase
{
    public void Awake()
    {
        DataBaseManager.Instance.Init();
    }

    public void ClickGameStartButton()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void ClickGameEndButton()
    {
        Application.Quit();
    }
}
