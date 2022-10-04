using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class CollisionData
{
    public int index;

    public int collideObjPID;
    public int beCollidedPID;

    public CharacterPhysicsData collideObjData;
    public CharacterPhysicsData beCollidedObjData;

    public CollisionData(int index, int collideObjPID, int beCollidedPID,
        CharacterPhysicsData collideObjData, CharacterPhysicsData beCollidedObjData)
    {
        this.index = index;
        this.collideObjPID = collideObjPID;
        this.beCollidedPID = beCollidedPID;
        this.collideObjData = collideObjData;
        this.beCollidedObjData = beCollidedObjData;
    }
}

public class PhysicsManager : MonoBehaviour
{
    public Transform hoglineLine;
    public Transform backLine;

    private static PhysicsManager instance = null;
    public static PhysicsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(PhysicsManager)) as PhysicsManager;
            }
            return instance;
        }
    }

    //물리 영향을 받는 모든 오브젝트들의 리스트
    private List<CharacterPhysics> physicsObjectList = new List<CharacterPhysics>();
    //충돌 발생 시 순서대로 기록되는 리스트
    private List<CollisionData> collisionDataList = new List<CollisionData>();

    private bool isAllStop = false;
    private bool isAllStopEventEnd = false;
    private Coroutine allStopCoroutine = null;

    public void Init()
    {
        isAllStop = false;
        isAllStopEventEnd = false;
        allStopCoroutine = null;

        collisionDataList.Clear();
    }

    private void FixedUpdate()
    {
        if (GameManager.CurStep == Step.SWEEP)
        {
            var obj = GameManager.Instance.CurrentCharacter;
            if (obj != null)
            {
                //플레이어 캐릭터가 호그라인을 넘긴 경우
                if (obj.transform.position.x > hoglineLine.position.x)
                {
                    //스윕 중지
                    GameManager.Instance.Move();
                }
            }
            else
            {
                //플레이어 캐릭터가 호그라인을 넘지 못해서 삭제되고
                //모두 멈춰 있는 경우
                if (isAllStop)
                {
                    //스윕 중지
                    GameManager.Instance.Move();
                }
            }
        }

        if (GameManager.CurStep == Step.SWEEP ||
            GameManager.CurStep == Step.MOVE)
        {
            //방향과 속력 계산
            CalculateForce();
            //갱신된 방향과 속력 적용
            ApplyForce();
            //모두 멈춘 경우 이벤트 발생
            AllStopEvent();
        }
    }

    public void AddPhysicsObject(CharacterPhysics obj)
    {
        //가장 마지막 데이터의 다음 id 부여
        var firstData = physicsObjectList.LastOrDefault();
        var newPID = firstData != null ? firstData.PID + 1 : 0;
        //인덱스 부여 후
        obj.SetPID(newPID);
        //리스트에 추가
        physicsObjectList.Add(obj);

        Debug.Log(string.Format("" +
            "[AddPhysicsObject]\n" +
            "이름 : {0} / PID : {1} / 등급 : {2} / 이름 : {3}",
            obj.characterTransform.name, obj.PID, obj.character.Data.rarity, obj.character.Data.name));
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

            //삭제 예정 오브젝트 제외
            if (moveObj.isInActive)
                continue;

            //빙판을 벗어난 경우
            if (!GameManager.Instance.IsInIcePlate(moveObj.characterTransform.position))
            {
                StopPhysicsObj(moveObj);
                continue;
            }

            //아직 변하는 중이라면
            if (moveObj.speed > 0 &&
                moveObj.dir != Vector3.zero)
            {
                allStop = false;
                //체크할 오브젝트 리스트 순환
                foreach (var checkObj in physicsObjectList)
                {
                    //삭제 예정 오브젝트 제외
                    if (checkObj.isInActive)
                        continue;

                    //자신은 제외한다.
                    if (moveObj.PID == checkObj.PID)
                        continue;

                    //충돌 조건 체크
                    if (CheckCollision(moveObj, checkObj))
                    {
                        //반동 계산
                        CalculateBouncing(moveObj, checkObj);
                    }
                }
            }

            //속도를 마찰력만큼 감소 시킨다.
            moveObj.speed -= moveObj.Friction;

            //스윕으로 인한 마찰력 회복
            moveObj.RecoverySweepFriction(Time.fixedDeltaTime);

            //0보다 작을경우 0으로 처리
            if (moveObj.speed < 0)
            {
                StopPhysicsObj(moveObj);
            }
        }

        isAllStop = allStop;
    }

    //계산된 방향과 속력을 적용한다.
    private void ApplyForce()
    {
        foreach (var moveObj in physicsObjectList)
        {
            if (moveObj == null)
                continue;

            //방향과 속력을 갱신한다.
            moveObj.ExcuteUpdateForce();

            //아직 변하는 중이라면
            if (moveObj.speed > 0 &&
                moveObj.dir != Vector3.zero)
            {
                //이전 위치 저장 후
                moveObj.prevPostion = moveObj.characterTransform.localPosition;
                //방향과 속력을 곱해서 힘을 계산하고
                var force = (moveObj.dir * moveObj.speed) * Time.fixedDeltaTime;
                //현재 위치에 더해준다.
                moveObj.characterTransform.localPosition += force;
            }
        }
    }

    //모두 멈추면 발생하는 이벤트 (수정필요)
    private void AllStopEvent()
    {
        if (!isAllStop)
            return;

        //코루틴이 끝나고 단 한번만 발생한다.
        if (isAllStopEventEnd)
        {
            var removeList = GetRemoveObjList();
            RemoveObj(removeList);

            GameManager.Instance.End();
            return;
        }

        if (allStopCoroutine == null)
        {
            allStopCoroutine = StartCoroutine(AllStopEventCoroutine());
        }
    }

    private IEnumerator AllStopEventCoroutine()
    {
        var removeList = GetRemoveObjList();

        List<int> collisionPIDList = null;
        foreach (var coll in collisionDataList)
        {
            if (collisionPIDList == null)
                collisionPIDList = new List<int>();

            //중복체크
            if (!collisionPIDList.Contains(coll.collideObjPID))
                collisionPIDList.Add(coll.collideObjPID);
            //중복체크
            if (!collisionPIDList.Contains(coll.beCollidedPID))
                collisionPIDList.Add(coll.beCollidedPID);
        }

        if (collisionPIDList != null)
        {
            //충돌이 일어난 오브젝트들만 이벤트 발생
            foreach (var pid in collisionPIDList)
            {
                var moveObj = GetPhysicsObject(pid);
                var findObj = GetFirstCollisionObject(pid);
                yield return StartCoroutine(moveObj.allStopEvent(findObj, physicsObjectList));
            }
        }

        RemoveObj(removeList);

        //스윕 값 초기화
        foreach (var obj in physicsObjectList)
        {
            obj.sweepValue = 0;
        }

        isAllStopEventEnd = true;
        allStopCoroutine = null;
    }

    private bool CheckCollision(CharacterPhysics moveObj, CharacterPhysics checkObj)
    {
        //현재 변화중인 오브젝트의 트랜스폼
        var moveObjTrans = moveObj.characterTransform;
        //체크할 오브젝트의 트랜스폼
        var checkObjTrans = checkObj.characterTransform;

        //체크할 오브젝트와 현재 오브젝트의 벡터
        var move_vc = checkObjTrans.localPosition - moveObjTrans.localPosition;
        //현재 오브젝트와 체크할 오브젝트의 벡터
        var check_vc = moveObjTrans.localPosition - checkObjTrans.localPosition;

        //두 오브젝트의 반지름을 더한 값
        float sumRadius = moveObj.Radius + checkObj.Radius;

        //두 오브젝트의 반지름을 더한 값보다
        //vc의 길이가 크다면 서로 충돌하지 않음.
        var difference = sumRadius - move_vc.magnitude;

        //충돌 플래그
        bool isCollision = false;

        //두 오브젝트가 충분히 가까울때
        if (difference > 0)
        {
            //상대가 움직이고 있을때
            if (checkObj.speed > 0)
            {
                //현재 오브젝트의 방향과 두 오브젝트 사이 벡터의 내적
                var move_dot = Vector3.Dot(moveObj.dir, move_vc.normalized);
                //체크할 오브젝트의 방향과 두 오브젝트 사이 벡터의 내적
                var check_dot = Vector3.Dot(checkObj.dir, check_vc.normalized);

                //서로의 앞에 있거나 뒤에 있다면
                if ((move_dot > 0 && check_dot > 0) ||
                    (move_dot < 0 && check_dot < 0))
                {
                    //중복 충돌 방지를 위해
                    //먼저 만들어진 오브젝트에게만 충돌 발생
                    if (moveObj.PID < checkObj.PID)
                    {
                        isCollision = true;
                    }
                }
                //상대방이 내 앞에 있고 내가 상대방의 뒤에 있다면
                else if (move_dot > 0 && check_dot < 0)
                {
                    isCollision = true;
                }
            }
            //상대가 멈춰 있으면
            else isCollision = true;
        }

        //충돌이 일어났다면
        if (isCollision)
        {
            Debug.Log(string.Format("충돌 발생!\n{0} -> {1}", moveObjTrans.name, checkObjTrans.name));

            //현재 변화중인 오브젝트의 벡터
            var moveObjVec = moveObjTrans.localPosition - moveObj.prevPostion;

            //현재 변화중인 오브젝트의 방향
            var moveObjDir = moveObjVec.normalized;

            //충돌 위치 보정
            var correctPosition = GetCorrectPosition(
                moveObjTrans.localPosition, checkObjTrans.localPosition,
                move_vc, moveObjDir, sumRadius);

            Debug.Log(string.Format("{0} 위치 보정\n{1} -> {2}",
                moveObjTrans.name, moveObjTrans.localPosition, correctPosition));

            //현재 오브젝트 위치 갱신
            moveObjTrans.localPosition = correctPosition;
        }

        return isCollision;
    }

    //올바른 충돌 위치 계산
    private Vector3 GetCorrectPosition(Vector3 moveObjPos, Vector3 checkObjPos, Vector3 vc, Vector3 moveObjDir, float sumRadius)
    {
        //두 오브젝트 사이의 벡터를 현재 오브젝트 방향 벡터에 투영
        var vp = Vector3.Project(vc, moveObjDir);
        //현재 오브젝트가 투영된 위치
        var vn = moveObjPos + vp;
        //체크할 오브젝트와 투영된 위치의 벡터
        var checkProjectVec = checkObjPos - vn;

        //투영된 위치에서 실제 충돌 위치로 보정할 값 계산
        float moveBack = Mathf.Sqrt(
            sumRadius * sumRadius -
            checkProjectVec.magnitude * checkProjectVec.magnitude);

        //올바른 충돌 위치
        var correctPosition = vn - moveBack * moveObjDir;

        return correctPosition;
    }

    //충돌 뒤 반동 계산
    private void CalculateBouncing(CharacterPhysics moveObj, CharacterPhysics checkObj)
    {
        //충돌 이벤트 발생
        moveObj.collideEvent(checkObj, physicsObjectList);
        //충돌 된 이벤트 발생
        checkObj.beCollidedEvent(moveObj, physicsObjectList);

        //현재 변화중인 오브젝트의 트랜스폼
        var moveObjTrans = moveObj.characterTransform;
        //체크할 오브젝트의 트랜스폼
        var checkObjTrans = checkObj.characterTransform;

        //체크할 오브젝트와 현재 오브젝트의 벡터
        var vc = checkObjTrans.localPosition - moveObjTrans.localPosition;
        //vc의 방향 벡터
        var vcd = vc.normalized;
        //vc의 노말 벡터
        var vcn = new Vector3(vcd.z, 0, -vcd.x);

        //현재 오브젝트 벡터
        var v1 = moveObjTrans.localPosition - moveObj.prevPostion;
        //충돌된 오브젝트 벡터
        var v2 = checkObjTrans.localPosition - checkObj.prevPostion;

        //두 오브젝트 모두 변화 없을 경우 무시
        if (v1 == Vector3.zero &&
            v2 == Vector3.zero)
        {
            moveObj.SetUpdateForce(Vector3.zero, 0);
            checkObj.SetUpdateForce(Vector3.zero, 0);
            return;
        }

        //현재 오브젝트 벡터를 vc의 방향 벡터에 투영
        var proj11 = Vector3.Project(v1, vcd);
        //현재 오브젝트 벡터를 vc의 노말 벡터에 투영
        var proj12 = Vector3.Project(v1, vcn);

        //충돌된 오브젝트 벡터를 vc의 방향 벡터에 투영
        var proj21 = Vector3.Project(v2, vcd);
        //충돌된 오브젝트 벡터를 vc의 노말 벡터에 투영
        var proj22 = Vector3.Project(v2, vcn);

        //반동이 계산된 현재 오브젝트 벡터
        var newv1 = proj21 + proj12;
        //반동이 계산된 충돌된 오브젝트 벡터
        var newv2 = proj22 + proj11;

        //공격력과 공격보너스를 합친 값 (발사단계에서 게이지 퍼센트 만큼 50%~120% 보너스)
        var attackValue = moveObj.character.finalAttack * moveObj.AttackBouns;

        //처음 속도와 충돌 했을때 속도 비율 (감소율)
        var speedRate = moveObj.speed / moveObj.FirstSpeed;

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
        var moveSpeed = moveObj.GetQuadraticEquationValue(finalMoveImpulse);
        var checkSpeed = checkObj.GetQuadraticEquationValue(finalCheckImpulse);

        //추가로 더해지는 충격량의 방향까지 고려
        var moveDir = (newv1.normalized + moveObj.character.ImpulseAddDir).normalized;
        var checkDir = (newv2.normalized + checkObj.character.ImpulseAddDir).normalized;

        //방향과 속력 업데이트 등록
        moveObj.SetUpdateForce(moveDir, moveSpeed);
        checkObj.SetUpdateForce(checkDir, checkSpeed);

        //충돌 데이터 기록
        var collisionData = new CollisionData(collisionDataList.Count,
            moveObj.PID, checkObj.PID, moveObj.pData, checkObj.pData);
        collisionDataList.Add(collisionData);

        //로깅        
        Debug.Log(moveObjTrans.name + " 충돌 전 속도 : " + moveObj.speed);
        Debug.Log(checkObjTrans.name + " 충돌 전 속도 : " + checkObj.speed);

        Debug.Log(string.Format("{0}의 공격력 : {1} ({2}*{3})",
            moveObjTrans.name, finalAttackValue, attackValue, speedRate));

        Debug.Log(string.Format("{0}의 방어력 : {1}\n방어율 : {2}",
            checkObjTrans.name, checkObj.character.finalDefence, defenceValue));

        Debug.Log(string.Format("{0}의 충격량 : {1} = ({2}+{3})*{4}",
            moveObjTrans.name, finalMoveImpulse, moveImpulse, moveObj.character.ImpulseAddValue, moveObj.character.ImpulsePercentValue));
        Debug.Log(string.Format("{0}의 충격량 : {1} = ({2}+{3})*{4}",
            checkObjTrans.name, finalCheckImpulse, checkImpulse, checkObj.character.ImpulseAddValue, checkObj.character.ImpulsePercentValue));

        Debug.Log(string.Format("[{0} final Vector]\n방향 : {1} 속도 : {2}",
            moveObjTrans.name, moveDir, moveSpeed));
        Debug.Log(string.Format("[{0} final Vector]\n방향 : {1} 속도 : {2}",
           checkObjTrans.name, checkDir, checkSpeed));

        //물리 연산 참고 사이트 -> https://brownsoo.github.io/2DVectors/moving_balls/
    }

    public int FindFirstCollision(int pid)
    {
        foreach (var data in collisionDataList)
        {
            if (data.collideObjPID == pid)
            {
                return data.beCollidedPID;
            }
            else if (data.beCollidedPID == pid)
            {
                return data.collideObjPID;
            }
        }

        //첫 충돌이 아직 발생하지 않았다.
        return -1;
    }

    public CharacterPhysicsData FindFirstCollisionPhysicsData(int pid)
    {
        foreach (var data in collisionDataList)
        {
            if (data.collideObjPID == pid)
            {
                return data.beCollidedObjData;
            }
            else if (data.beCollidedPID == pid)
            {
                return data.collideObjData;
            }
        }

        //첫 충돌이 아직 발생하지 않았다.
        return null;
    }

    public CharacterPhysics GetPhysicsObject(int pid)
    {
        var findObj = physicsObjectList.Find(v => v.PID == pid);
        return findObj;
    }

    public CharacterPhysics GetFirstCollisionObject(int pid)
    {
        var findPID = FindFirstCollision(pid);
        var findObj = GetPhysicsObject(findPID);
        return findObj;
    }

    private List<int> GetRemoveObjList()
    {
        List<int> removeList = null;
        foreach (var obj in physicsObjectList)
        {
            if (!GameManager.Instance.IsInIcePlate(obj.characterTransform.position) ||
                obj.characterTransform.position.x < hoglineLine.position.x ||
                obj.characterTransform.position.x > backLine.position.x)
            {
                if (removeList == null)
                    removeList = new List<int>();

                obj.isInActive = true;
                obj.characterTransform.gameObject.SetActive(false);
                removeList.Add(obj.PID);
            }
        }
        return removeList;
    }

    private void RemoveObj(List<int> removeList)
    {
        if (removeList != null)
        {
            foreach (var pid in removeList)
            {
                GameManager.Instance.RemoveCharacter(pid);
            }

            removeList.Clear();
            removeList = null;
        }
    }

    public IEnumerator StartPhysicsCoroutine(IEnumerator enumerator)
    {
        yield return StartCoroutine(enumerator);
    }

    public void ClearList()
    {
        physicsObjectList.Clear();
        collisionDataList.Clear();
    }

    public void StopPhysicsObj(CharacterPhysics obj)
    {
        obj.speed = 0;
        obj.dir = Vector3.zero;

        obj.character.InitState();
    }

    private void OnDestroy()
    {
        physicsObjectList.Clear();
        collisionDataList.Clear();

        physicsObjectList = null;
        collisionDataList = null;

        instance = null;
    }
}
