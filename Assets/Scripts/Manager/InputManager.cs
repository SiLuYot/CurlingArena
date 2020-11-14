using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Character selectedCharacter = null;
    private ShootUI shootUI = null;

    private Vector3 clickStartWorldPos = Vector3.zero;
    private Vector3 clickStartScreenPos = Vector3.zero;

    public float sweep = 0.0001f;

    private void Update()
    {
        //누를떄
        if (Input.GetMouseButtonDown(0))
        {
            clickStartScreenPos = Input.mousePosition;
            clickStartWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var obj = hit.transform.gameObject.GetComponent<Character>();
                if (obj != null && !CameraManager.IsFixed)
                {
                    shootUI = UIManager.Instance.Get<ShootUI>() as ShootUI;
                    shootUI.Init(clickStartScreenPos);
                    selectedCharacter = obj;
                }
            }
        }
        //누르는 중일때
        else if (Input.GetMouseButton(0))
        {
            if (shootUI == null)
            {
                if (!UICamera.isOverUI)
                {
                    CameraManager.Instance.DragScreen(clickStartWorldPos);
                }                                                
            }
            else if (shootUI != null)
            {
                shootUI.RotateArrow(clickStartScreenPos);
                shootUI.SetSliderValue(clickStartWorldPos);
            }
        }
        //땔때
        else if (Input.GetMouseButtonUp(0))
        {
            if (selectedCharacter != null && shootUI != null)
            {
                var dragVector = clickStartWorldPos - Camera.main.ScreenToWorldPoint(Input.mousePosition);

                var dir = dragVector.normalized;
                var force = shootUI.PowerValue;
                var attackBouns = shootUI.slider.value * 1.2f;
                if (attackBouns < 0.5f)
                    attackBouns = 0.5f;

                selectedCharacter.Physics.AddForce(dir, force);
                selectedCharacter.Physics.SetAttackBonus(attackBouns);
                selectedCharacter = null;

                shootUI.Close();
                shootUI = null;

                CameraManager.IsFixed = true;
            }
        }
    }

    private void FixedUpdate()
    {
        //누르는 중일때
        //if (Input.GetMouseButton(0))
        //{
        //    if (selectedCharacter != null && shootUI == null)
        //    {
        //        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //        if (mousePos.z > selectedCharacter.transform.position.z)
        //        {
        //            selectedCharacter.Physics.Sweep(sweep);
        //        }
        //        else if (mousePos.z < selectedCharacter.transform.position.z)
        //        {
        //            selectedCharacter.Physics.Sweep(-sweep);
        //        }
        //    }
        //}
    }
}
