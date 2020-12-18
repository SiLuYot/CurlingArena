using UnityEngine;

public class IntroUI : UIBase
{
    public void Awake()
    {        
        DataBaseManager.Instance.Init();
        DeckManager.Instance.Init();
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
