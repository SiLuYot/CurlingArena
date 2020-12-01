using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class PositionSelectUI : UIBase
{
    public UIButton button;
    public UILabel titleLabel;

    public CharacterPhysics newPosObj;

    private bool isClick = false;
    private Vector3 clickStartWorldPos = Vector3.zero;

    public void Init(string title, CharacterPhysics obj)
    {
        this.newPosObj = obj;
        this.isClick = false;

        titleLabel.text = title;

        CameraManager.IsFixed = false;
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickStartWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            CameraManager.Instance.DragScreen_XZ(clickStartWorldPos);
        }
    }

    public void ClickButton()
    {
        var e1 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Debug.Log(e1);

        newPosObj.characterTransform.localPosition = new Vector3(e1.x, 0, e1.z);

        isClick = true;
        CameraManager.IsFixed = true;
    }

    public IEnumerator WaitSelectPosition()
    {
        while (!isClick)
        {
            yield return null;
        }
    }
}
