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
    public Transform targetPos;
    public float y = 10.0f;

    public static bool IsFixed = false;

    public void Start()
    {
        var newPos = new Vector3(
                targetPos.position.x,
                targetPos.position.y + y,
                targetPos.position.z);

        mainCamera.transform.position = newPos;
    }

    public void LateUpdate()
    {
        if (IsFixed)
        {
            var newPos = new Vector3(
                targetPos.position.x,
                targetPos.position.y + y,
                targetPos.position.z);

            mainCamera.transform.position = newPos;
        }
    }

    public void DragScreen(Vector3 clickStartPos)
    {
        var dragVector = clickStartPos - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Camera.main.transform.position += new Vector3(dragVector.x, 0, 0);
    }
}
