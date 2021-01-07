using UnityEngine;

public class IntroUI : BaseUI
{
    public void ClickGameStartButton()
    {
        base.Close();
        UIManager.Instance.Get<MainMenuUI>();
    }

    public override void Close()
    {
        var popup = UIManager.Instance.Get<BasePopupUI>() as BasePopupUI;
        popup.Init("정말 게임을 종료할까요?",
            () =>
            {
                Application.Quit();
            });
    }
}
