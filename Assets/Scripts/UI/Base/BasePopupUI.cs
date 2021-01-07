using System;

public class BasePopupUI : BaseUI
{
    public UILabel label;

    private Action yesAction;
    private Action noAction;

    public void Init(string text, Action yesAction = null, Action noAction = null)
    {
        this.label.text = text;
        this.yesAction = yesAction;
        this.noAction = noAction;
    }

    public void ClickYseButton()
    {
        Close();
        yesAction?.Invoke();        
    }

    public void ClickNoButton()
    {
        Close();
        noAction?.Invoke();        
    }
}
