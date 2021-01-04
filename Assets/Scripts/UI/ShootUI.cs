using UnityEngine;

public class ShootUI : UIBase
{
    public UISlider slider;
    public Transform rotateRoot;
    public GameObject xMark;

    private Character selectedCharacter;
    private float maxPower;

    public void Init(Vector3 pos, Character selectedCharacter)
    {
        xMark.SetActive(false);
        this.gameObject.SetActive(false);

        var e1 = Camera.main.ScreenToViewportPoint(pos);
        var e2 = UICamera.mainCamera.ViewportToWorldPoint(e1);
        transform.position = e2;

        this.selectedCharacter = selectedCharacter;
        this.maxPower = selectedCharacter.GetShootSpeed();
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

        this.gameObject.SetActive(dir != Vector3.zero);
    }

    public void SetSliderValue(Vector3 clickStartPos)
    {
        var dragVector = clickStartPos - Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float power = dragVector.magnitude * (maxPower * 0.05f);
        power = power > maxPower ? maxPower : power;

        slider.value = power / maxPower;        
    }

    public float GetAttackBouns()
    {
        var attackBouns = slider.value * 1.2f;
        if (attackBouns < 0.5f)
            attackBouns = 0.5f;

        return attackBouns;
    }

    public float GetPowerValue()
    {
        return selectedCharacter.GetShootSpeed(GetAttackBouns());
    }
}
