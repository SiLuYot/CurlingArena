using UnityEngine;

public class UIBase : MonoBehaviour
{
    protected UIData uiData;

    public void Init(UIData uiData)
    {
        this.uiData = uiData;
    }

    public void Close()
    {
        UIManager.Instance.Close(uiData.id);
    }
}
