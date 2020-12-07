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
        Close();
        UIManager.Instance.Get<MainMenuUI>();
    }

    public void ClickGameEndButton()
    {
        Application.Quit();
    }
}
