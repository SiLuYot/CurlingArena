using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager instance = null;
    public static CameraManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(CameraManager)) as CameraManager;                
            }                
            return instance;
        }
    }

    public Camera mainCamera;
    public Transform stoneRoot;
    public Transform playerCreatePos;
    public Transform playerSelectPos;
    public float y = 10.0f;

    public static bool IsFixed = false;

    private Transform followTrans;

    private const int DEFAULT_SIZE = 15;
    private const int SELECT_SIZE = 10;

    public void Start()
    {
        
    }

    public void InitCreatePos()
    {
        Init(playerCreatePos.position);

        mainCamera.orthographicSize = DEFAULT_SIZE;
    }

    public void InitSelectPos()
    {
        Init(playerSelectPos.position);

        mainCamera.orthographicSize = SELECT_SIZE;
    }

    public void Init(Vector3 pos)
    {
        IsFixed = false;

        var newPos = new Vector3(
                pos.x,
                pos.y + y,
                pos.z);

        mainCamera.transform.position = newPos;
    }

    public void LateUpdate()
    {
        if (IsFixed)
        {
            var findObj = followTrans;

            var newPos = new Vector3(
                findObj.position.x,
                findObj.position.y + y,
                findObj.position.z);

            mainCamera.transform.position = newPos;
        }
    }

    public void SetFollowTrans(Transform followTrans)
    {
        this.followTrans = followTrans;
    }

    public void DragScreen_X(Vector3 clickStartPos)
    {
        var dragVector = clickStartPos - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Camera.main.transform.position += new Vector3(dragVector.x, 0, 0);
    }

    public void DragScreen_XZ(Vector3 clickStartPos)
    {
        var dragVector = clickStartPos - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Camera.main.transform.position += new Vector3(dragVector.x, 0, dragVector.z);
    }
}
