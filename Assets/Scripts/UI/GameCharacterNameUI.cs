using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCharacterNameUI : UIBase
{
    public UILabel label;
    private Transform followTrans;

    public void Awake()
    {
        this.gameObject.SetActive(false);
    }

    public void Init(CharacterData data, Transform followTrans)
    {
        this.followTrans = followTrans;
        label.text = string.Format("{0} {1}", data.rarityData.NAME, data.NAME);

        UpdatePos();
        this.gameObject.SetActive(true);
    }

    public void FixedUpdate()
    {
        if (followTrans == null)
            return;

        UpdatePos();
    }

    private void UpdatePos()
    {
        var e1 = Camera.main.WorldToViewportPoint(new Vector3(followTrans.position.x, 0, followTrans.position.z));
        var e2 = UICamera.mainCamera.ViewportToWorldPoint(e1);
        transform.position = e2;
    }
}
