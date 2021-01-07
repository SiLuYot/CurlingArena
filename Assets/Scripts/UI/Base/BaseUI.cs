using UnityEngine;

public class BaseUI : MonoBehaviour
{
    public int index;
    public UIData uiData;

    public void Init(int index, UIData uiData)
    {
        this.index = index;
        this.uiData = uiData;
    }

    public virtual void Close()
    {
        UIManager.Instance?.Close();        
    }
}
