using UnityEngine;

public class SweepUI : BaseSystemUI
{
    public UILabel title;
    public GameObject arrowUp;
    public GameObject arrowDown;

    private float speed;

    private Vector3 upDir;
    private Vector3 downDir;

    private float arrowMoveTime;
    private float timer;

    void Start()
    {
        speed = 1f;

        upDir = Vector3.up;
        downDir = Vector3.down;

        timer = 0;
        arrowMoveTime = 0.5f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > arrowMoveTime)
        {
            timer = 0;

            upDir *= -1;
            downDir *= -1;
        }
        
        arrowUp.transform.position += upDir * speed * Time.deltaTime;
        arrowDown.transform.position += downDir * speed * Time.deltaTime;
    }
}
