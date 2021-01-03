﻿using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Character selectedCharacter;
    private ShootUI shootUI;

    private Vector3 clickStartWorldPos = Vector3.zero;
    private Vector3 clickStartScreenPos = Vector3.zero;    

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        selectedCharacter = null;
        shootUI = null;
    }

    private void Update()
    {
        //캐릭터를 고르는 단계일때
        if (GameManager.CurStep == Step.SELECT)
        {        
            //누를때
            if (Input.GetMouseButtonDown(0))
            {
                clickStartWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            //누르는 중일때
            else if (Input.GetMouseButton(0))
            {
                //누르는 위치가 UI가 아니고 캐릭터를 누른것도 아닐때
                if (!UICamera.isOverUI && selectedCharacter == null)
                {
                    CameraManager.Instance.DragScreen_X(clickStartWorldPos);
                }
            }
              
        }

        //준비 단계일때
        if (GameManager.CurStep == Step.READY)
        {
            //누를때
            if (Input.GetMouseButtonDown(0))
            {
                clickStartScreenPos = Input.mousePosition;
                clickStartWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    var obj = hit.transform.gameObject.GetComponent<Character>();
                    if (obj != null && GameManager.Instance.IsCurrentPlayer(obj))
                    {
                        selectedCharacter = obj;

                        shootUI = UIManager.Instance.Get<ShootUI>() as ShootUI;
                        shootUI.Init(clickStartScreenPos, selectedCharacter);
                    }
                }
            }
            //누르는 중일때
            else if (Input.GetMouseButton(0))
            {
                //누르는 위치가 UI가 아니고 캐릭터를 누른것도 아닐때
                if (!UICamera.isOverUI && selectedCharacter == null)
                {
                    CameraManager.Instance.DragScreen_X(clickStartWorldPos);
                }                
                else if (selectedCharacter != null)
                {
                    shootUI.RotateArrow(clickStartScreenPos);
                    shootUI.SetSliderValue(clickStartWorldPos);
                }
            }
            //땔때
            else if (Input.GetMouseButtonUp(0))
            {
                if (selectedCharacter != null)
                {
                    var dragVector =
                        clickStartWorldPos -
                        Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    var dir = dragVector.normalized;
                    var force = shootUI.GetPowerValue();
                    var attackBouns = shootUI.GetAttackBouns();

                    //하우스 방향(전방)일때만 발사 허용
                    var dot = Vector3.Dot(Vector3.right, dir);
                    if (dot > 0)
                    {
                        GameManager.Instance.Shoot(selectedCharacter, dir, force, attackBouns);
                    }

                    selectedCharacter = null;

                    shootUI.Close();
                    shootUI = null;
                }
            }
        }

        //스윕 중
        if (GameManager.CurStep == Step.SWEEP)
        {
            //누를때
            if (Input.GetMouseButtonDown(0))
            {
                clickStartScreenPos = Input.mousePosition;
            }
            //누르는 중일때
            else if (Input.GetMouseButton(0))
            {
                //누르는 위치가 UI가 아닐때
                if (!UICamera.isOverUI)
                {
                    var curCharacter = GameManager.Instance.CurrentCharacter;
                    if (curCharacter != null)
                    {
                        var curMousePos = Input.mousePosition;
                        var dragVector = clickStartScreenPos - curMousePos;

                        //벡터의 길이가 최소 길이보다 길다면
                        if (dragVector.magnitude > GameManager.SWEEP_MIN_DISTACNE)
                        {
                            var dragVectorWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                            var dir = curCharacter.Physics.dir;
                            var diff = dragVectorWorldPos - curCharacter.transform.position;
                            var cross = Vector3.Cross(dir, diff);

                            //방향 벡터의 노말
                            var dirNormal = new Vector3(dir.z, 0, -dir.x);

                            //오브젝트 기준 오른쪽
                            if (cross.y > 0)
                            {
                                curCharacter.Physics.ApplyDir(dirNormal * Time.deltaTime);
                                curCharacter.Physics.Sweep(GameManager.SWEEP);
                                //curCharacter.Physics.Sweep(GameManager.SWEEP * Time.deltaTime);
                            }
                            //오브젝트 기준 왼쪽
                            else
                            {
                                curCharacter.Physics.ApplyDir(-dirNormal * Time.deltaTime);
                                curCharacter.Physics.Sweep(GameManager.SWEEP);
                                //curCharacter.Physics.Sweep(GameManager.SWEEP * Time.deltaTime);
                            }

                            //첫 클릭 위치 초기화
                            clickStartScreenPos = Input.mousePosition;
                        }
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                clickStartScreenPos = Input.mousePosition;
            }
        }
    }
}
