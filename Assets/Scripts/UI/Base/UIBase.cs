using UnityEngine;

public class UIBase : MonoBehaviour
{
    public int index;
    public UIData uiData;

    public void Init(int index, UIData uiData)
    {
        this.index = index;
        this.uiData = uiData;
    }

    public void Close()
    {
        UIManager.Instance?.Close(index);
    }
}
