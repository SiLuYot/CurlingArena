using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    private static PhysicsManager instance = null;
    public static PhysicsManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(PhysicsManager)) as PhysicsManager;

            return instance;
        }
    }

    //물리 영향을 받는 모든 오브젝트들의 리스트
    private List<CharacterPhysics> physicsObjectList = new List<CharacterPhysics>();

    public bool isAllStop = false;
    public bool isAllStopEventEnd = false;

    public void Init()
    {
        isAllStop = false;
        isAllStopEventEnd = false;
    }

    private void FixedUpdate()
    {
        if (GameManager.CurRoundStep != RoundStep.MOVE)
            return;

        if (physicsObjectList == null)
            return;

        //방향과 속력 계산
        CalculateForce();
        //계산된 방향과 속력 갱신
        UpdateForce();
        //갱신된 방향과 속력 적용
        ApplyForce();

        AllStopEvent();
    }

    public void AddPhysicsObject(CharacterPhysics obj)
    {
        //인덱스 부여 후
        obj.pid = physicsObjectList.Count;
        //리스트에 추가
        physicsObjectList.Add(obj);

        Debug.Log(string.Format("" +
            "*AddPhysicsObject*\n" +
            "이름 : {0} / PID : {1} / 등급 : {2} / 이름 : {3}",
            obj.characterTransform.name, obj.pid, obj.data.rarity, obj.data.name));
    }

    public void RemovePhysicsObject(CharacterPhysics obj)
    {
        physicsObjectList.Remove(obj);
    }

    //방향과 속력을 계산한다.
    private void CalculateForce()
    {
        bool allStop = true;
        foreach (var moveObj in physicsObjectList)
        {
            if (moveObj == null)
                continue;

            //속도를 마찰력만큼 감소 시킨다.
            moveObj.speed -= moveObj.Friction;

            //0보다 작을경우 0으로 처리
            if (moveObj.speed < 0)
            {
                moveObj.speed = 0;
            }

            //아직 변하는 중이라면
            if (moveObj.speed > 0)
            {
                allStop = false;
                //체크할 오브젝트 리스트 순환
                foreach (var checkObj in physicsObjectList)
                {
                    //자신은 제외한다.
                    if (moveObj.pid == checkObj.pid)
                        continue;

                    //두 오브젝트 충돌 체크
                    var difference = CheckCollision(moveObj, checkObj);

                    //충돌하기에 충분히 가까운 거리라면
                    if (difference >= 0)
                    {
                        //반동 계산
                        CalculateBouncing(moveObj, checkObj, difference);
                    }
                }
            }
        }

        isAllStop = allStop;
    }

    //계산된 방향과 속력을 갱신한다.
    private void UpdateForce()
    {
        foreach (var moveObj in physicsObjectList)
        {
            if (moveObj != null && moveObj.updateForce != null)
            {
                moveObj.updateForce();
                moveObj.updateForce = null;
            }
        }
    }

    //계산된 방향과 속력을 적용한다.
    private void ApplyForce()
    {
        foreach (var moveObj in physicsObjectList)
        {
            if (moveObj == null)
                continue;

            //아직 변하는 중이라면
            if (moveObj.speed > 0)
            {
                //방향과 속력을 곱해서 힘을 계산하고
                var force = (moveObj.dir * moveObj.speed) * Time.fixedDeltaTime;
                //현재 위치에 더해준다.
                moveObj.characterTransform.localPosition += force;
            }
        }
    }

    private void AllStopEvent()
    {
        if (physicsObjectList == null)
            return;

        if (!isAllStop)
            return;

        if (isAllStopEventEnd)
        {
            GameManager.Instance.End();
            return;
        }

        isAllStopEventEnd = true;
        foreach (var moveObj in physicsObjectList)
        {
            if (moveObj == null)
                continue;

            moveObj.allStopEvent(physicsObjectList);
        }
    }

    private float CheckCollision(CharacterPhysics moveObj, CharacterPhysics checkObj)
    {
        //현재 변화중인 오브젝트의 트랜스폼
        var moveObjTrans = moveObj.characterTransform;
        //체크할 오브젝트의 트랜스폼
        var checkObjTrans = checkObj.characterTransform;

        //체크할 오브젝트와 현재 오브젝트의 벡터
        var vc = checkObjTrans.localPosition - moveObjTrans.localPosition;
        //두 오브젝트의 반지름을 더한 값
        float sumRadius = moveObj.Radius + checkObj.Radius;

        //두 오브젝트의 반지름을 더한 값보다
        //vc의 길이가 크다면 서로 충돌하지 않음.
        float difference = sumRadius - vc.magnitude;

        //충분히 가까워서 충돌 가능
        //if (difference >= 0)
        if (difference > 0)
        {
            //오브젝트가 도달할 위치 예상
            var moveObjEndPos = (moveObj.dir * moveObj.speed) + moveObjTrans.localPosition;
            //현재 변화중인 오브젝트의 벡터
            var moveObjVec = moveObjEndPos - moveObjTrans.localPosition;
            //현재 변화중인 오브젝트의 방향
            var moveObjDir = moveObjVec.normalized;

            //충돌 위치 보정
            var correctPosition = GetCorrectPosition(
                moveObjTrans.localPosition,
                checkObjTrans.localPosition,
                vc, moveObjDir, sumRadius);

            Debug.Log(string.Format("{0} 위치 보정\n{1} -> {2}",
                moveObjTrans.name, moveObjTrans.localPosition, correctPosition));

            //현재 오브젝트 위치 갱신
            moveObjTrans.localPosition = correctPosition;
        }

        return difference;
    }

    //올바른 충돌 위치 계산
    private Vector3 GetCorrectPosition(Vector3 moveObjPos, Vector3 checkObjPos, Vector3 vc, Vector3 moveObjNextPosDir, float sumRadius)
    {
        //두 오브젝트 사이의 벡터를 현재 오브젝트 방향 벡터에 투영
        var vp = Vector3.Project(vc, moveObjNextPosDir);
        //현재 오브젝트가 투영된 위치
        var vn = moveObjPos + vp;
        //체크할 오브젝트와 투영된 위치의 벡터
        var checkProjectVec = checkObjPos - vn;

        //투영된 위치에서 실제 충돌 위치로 보정할 값 계산
        float moveBack = Mathf.Sqrt(
            sumRadius * sumRadius -
            checkProjectVec.magnitude * checkProjectVec.magnitude);

        //올바른 충돌 위치
        var correctPosition = vn - moveBack * moveObjNextPosDir;

        return correctPosition;
    }

    //충돌 뒤 반동 계산
    private void CalculateBouncing(CharacterPhysics moveObj, CharacterPhysics checkObj, float difference)
    {
        //현재 변화중인 오브젝트의 트랜스폼
        var moveObjTrans = moveObj.characterTransform;
        //체크할 오브젝트의 트랜스폼
        var checkObjTrans = checkObj.characterTransform;

        //체크할 오브젝트와 현재 오브젝트의 벡터
        var vc = checkObjTrans.localPosition - moveObjTrans.localPosition;

        //현재 오브젝트의 방향 전환
        moveObj.dir -= vc.normalized * difference;

        //vc의 방향 벡터
        var vcd = vc.normalized;
        //vc의 노말 벡터
        var vcn = new Vector3(vcd.z, 0, -vcd.x);

        //오브젝트가 도달할 위치 예상
        var moveObjEndPos = (moveObj.dir * moveObj.speed) + moveObjTrans.localPosition;
        //현재 오브젝트 벡터
        var v1 = moveObjEndPos - moveObjTrans.localPosition;
        //현재 오브젝트 벡터에 vc의 방향 벡터를 투영
        var proj11 = Vector3.Project(v1, vcd);
        //현재 오브젝트 벡터에 vc의 노말 벡터를 투영
        var proj12 = Vector3.Project(v1, vcn);

        //오브젝트가 도달할 위치 예상
        var checkObjEndPos = (checkObj.dir * checkObj.speed) + checkObjTrans.localPosition;
        //충돌된 오브젝트 벡터
        var v2 = checkObjEndPos - checkObjTrans.localPosition;
        //충돌된 오브젝트 벡터에 vc의 방향 벡터를 투영
        var proj21 = Vector3.Project(v2, vcd);
        //충돌된 오브젝트 벡터에 vc의 노말 벡터를 투영
        var proj22 = Vector3.Project(v2, vcn);

        float P = moveObj.Mass * proj11.x + checkObj.Mass * proj21.x;
        float V = proj11.x - proj21.x;
        float v2fx = (P + V * moveObj.Mass) / (moveObj.Mass + checkObj.Mass);
        float v1fx = v2fx - V;

        P = moveObj.Mass * proj11.z + checkObj.Mass * proj21.z;
        V = proj11.z - proj21.z;
        float v2fz = (P + V * moveObj.Mass) / (moveObj.Mass + checkObj.Mass);
        float v1fz = v2fz - V;

        //반동이 계산된 현재 오브젝트 벡터
        var newv1 = new Vector3(proj12.x + v1fx, 0, proj12.z + v1fz);
        //반동이 계산된 충돌된 오브젝트 벡터
        var newv2 = new Vector3(proj22.x + v2fx, 0, proj22.z + v2fz);

        //충돌 이벤트 발생
        moveObj?.collideEvent(checkObj);
        //충돌 된 이벤트 발생
        checkObj?.beCollidedEvent(moveObj);

        //공격력과 공격보너스를 합친 값 (발사단계에서 게이지 퍼센트 만큼 50%~120% 보너스)
        var attackValue = moveObj.character.finalAttack * moveObj.attackBouns;

        //발사 할때와 충돌 했을때의 속도 감소율
        var speedRate = GameManager.Instance.IsCurrentPlayer(moveObj.character) ?
            (moveObj.speed / moveObj.firstSpeed) : 1.0f;

        //최종 공격력
        var finalAttackValue = attackValue * speedRate;

        //방어율
        var defenceValue = checkObj.character.GetDefenceValue();
        //방어율 (퍼센트)
        var pDefenceValue = defenceValue * 0.01f;

        //move오브젝트의 충격량
        var moveImpulse = finalAttackValue * pDefenceValue;
        //check오브젝트의 충격량
        var checkImpulse = finalAttackValue * (1.0f - pDefenceValue);

        //공,방 계산후 추가로 더해지는 충격량, 충격량을 가할 퍼센트까지 고려
        var finalMoveImpulse = (moveImpulse + moveObj.character.ImpulseAddValue) * moveObj.character.ImpulsePercentValue;
        var finalCheckImpulse = (checkImpulse + checkObj.character.ImpulseAddValue) * checkObj.character.ImpulsePercentValue;

        //충격량이 날아갈 총 거리이므로 
        //마찰력을 공차로 가지는 등차수열의 합이 충격량이다.
        var moveSpeed = moveObj.GetQuadraticEquationValue((finalMoveImpulse) * 2);
        var checkSpeed = checkObj.GetQuadraticEquationValue((finalCheckImpulse) * 2);

        //방향과 속력 업데이트 등록
        moveObj.updateForce = () =>
        {
            moveObj.dir = newv1.normalized;
            moveObj.speed = moveSpeed;
            //moveObj.speed = newv1.magnitude;
        };

        //방향과 속력 업데이트 등록
        checkObj.updateForce = () =>
        {
            checkObj.dir = newv2.normalized;
            checkObj.speed = checkSpeed;
            //checkObj.speed = newv2.magnitude;
        };

        moveObj.isFirstCollide = true;
        checkObj.isFirstCollide = true;

        Debug.Log(string.Format("충돌 발생!\n{0} -> {1}", moveObjTrans.name, checkObjTrans.name));
        Debug.Log(moveObjTrans.name + " 충돌 전 속도 : " + moveObj.speed);
        Debug.Log(checkObjTrans.name + " 충돌 전 속도 : " + checkObj.speed);

        Debug.Log(string.Format("{0}의 공격력 : {1} ({2}*{3})",
            moveObjTrans.name, finalAttackValue, attackValue, speedRate));

        Debug.Log(string.Format("{0}의 방어력 : {1}\n방어율 : {2}",
            checkObjTrans.name, checkObj.character.finalDefence, defenceValue));

        Debug.Log(string.Format("{0}의 충격량 : {1} ({2}+{3})", 
            moveObjTrans.name, finalMoveImpulse, moveImpulse, moveObj.character.ImpulseAddValue));
        Debug.Log(string.Format("{0}의 충격량 : {1} ({2}+{3})", 
            checkObjTrans.name, finalCheckImpulse, checkImpulse, checkObj.character.ImpulseAddValue));

        Debug.Log(string.Format("[{0}]\n방향 : {1} 속도 : {2}", 
            moveObjTrans.name, newv1.normalized, moveSpeed));
        Debug.Log(string.Format("[{0}]\n방향 : {1} 속도 : {2}",
           checkObjTrans.name, newv2.normalized, checkSpeed));
    }

    private void OnDestroy()
    {
        physicsObjectList.Clear();

        instance = null;
        physicsObjectList = null;
    }
}
