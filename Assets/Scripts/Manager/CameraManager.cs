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
            var originPos = mainCamera.transform.position;

            var newPos = new Vector3(
                followTrans.position.x,
                followTrans.position.y + y,
                followTrans.position.z);

            var finalPos = new Vector3(originPos.x, newPos.y, originPos.z);

            var width = mainCamera.orthographicSize * Screen.width / Screen.height;
            var height = width * Screen.height / Screen.width;

            bool isInIce_X1 = GameManager.Instance.IsInIcePlate_X1(newPos.x - width);
            bool isInIce_X2 = GameManager.Instance.IsInIcePlate_X2(newPos.x + width);
            bool isInIce_Z1 = GameManager.Instance.IsInIcePlate_Z1(newPos.z + height);
            bool isInIce_Z2 = GameManager.Instance.IsInIcePlate_Z2(newPos.z - height);

            if (isInIce_X1 && isInIce_X2)
            {
                //x축 변경
                finalPos.Set(newPos.x, finalPos.y, finalPos.z);
            }

            if (isInIce_Z1 && isInIce_Z2)
            {
                //z축 변경
                finalPos.Set(finalPos.x, finalPos.y, newPos.z);
            }

            mainCamera.transform.position = finalPos;
        }
    }

    public void SetFollowTrans(Transform followTrans)
    {
        this.followTrans = followTrans;
    }

    public void DragScreen_X(Vector3 clickStartPos)
    {      
        var dragVector = clickStartPos - mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var newPos = mainCamera.transform.position + new Vector3(dragVector.x, 0, 0);

        var width = mainCamera.orthographicSize * Screen.width / Screen.height;

        bool isInIce_X1 = GameManager.Instance.IsInIcePlate_X1(newPos.x - width);
        bool isInIce_X2 = GameManager.Instance.IsInIcePlate_X2(newPos.x + width);

        if (isInIce_X1 && isInIce_X2)
        {
            mainCamera.transform.position = newPos;
        }

        if (!isInIce_X1)        
        {
            var diff = GameManager.Instance.endLine1.position.x -
                (mainCamera.transform.position.x - width);

            mainCamera.transform.position += new Vector3(diff, 0, 0);
        }
        else if (!isInIce_X2)
        {
            var diff = GameManager.Instance.endLine2.position.x -
                (mainCamera.transform.position.x + width);

            mainCamera.transform.position += new Vector3(diff, 0, 0);
        }
    }

    public void DragScreen_XZ(Vector3 clickStartPos)
    {
        var dragVector = clickStartPos - mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mainCamera.transform.position += new Vector3(dragVector.x, 0, dragVector.z);
    }
}
