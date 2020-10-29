using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootUI : UIBase
{
    public UISlider slider;
    public Transform rotateRoot;
    
    private readonly float maxPower = 10.0f;
    private readonly float multiply = 2.0f;
    public float PowerValue { get => slider.value * maxPower * multiply; }

    public void init(Vector3 pos)
    {
        transform.position = pos;
    }

    public void RotateArrow(Vector3 clickStartPixelPos)
    {
        var e1 = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        var e2 = UICamera.mainCamera.ViewportToWorldPoint(e1);

        var s1 = Camera.main.ScreenToViewportPoint(clickStartPixelPos);
        var s2 = UICamera.mainCamera.ViewportToWorldPoint(s1);

        var dir = s2 - e2;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rotateRoot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void SetSliderValue(Vector3 clickStartPos)
    {
        var dragVector = clickStartPos - Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float power = dragVector.magnitude;
        power = power > maxPower ? maxPower : power;

        slider.value = power / maxPower;
    }
}
