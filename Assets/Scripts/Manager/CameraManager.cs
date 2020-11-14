using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager instance = null;
    public static CameraManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(CameraManager)) as CameraManager;

            return instance;
        }
    }

    public Camera mainCamera;
    public Transform stoneRoot;
    public Transform playerCreatePos;
    public float y = 10.0f;

    public static bool IsFixed = false;

    public void Start()
    {
        Init();
    }

    public void LateUpdate()
    {
        if (IsFixed)
        {
            var findObj = stoneRoot.Find("player").gameObject.transform;

            var newPos = new Vector3(
                findObj.position.x,
                findObj.position.y + y,
                findObj.position.z);

            mainCamera.transform.position = newPos;
        }
    }

    public void Init()
    {
        IsFixed = false;

        var newPos = new Vector3(
                playerCreatePos.position.x,
                playerCreatePos.position.y + y,
                playerCreatePos.position.z);

        mainCamera.transform.position = newPos;
    }

    public void DragScreen(Vector3 clickStartPos)
    {
        var dragVector = clickStartPos - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Camera.main.transform.position += new Vector3(dragVector.x, 0, 0);
    }
}
