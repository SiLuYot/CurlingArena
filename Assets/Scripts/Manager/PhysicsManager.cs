using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class CollisionData
{
    public int index;
    public int collideObjPID;
    public int beCollidedPID;

    public CollisionData(int index, int collideObjPID, int beCollidedPID)
    {
        this.index = index;
        this.collideObjPID = collideObjPID;
        this.beCollidedPID = beCollidedPID;
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
                instance = FindObjectOfType(typeof(PhysicsManager)) as PhysicsManager;

            return instance;
        }
    }

    //물리 영향을 받는 모든 오브젝트들의 리스트
    private List<CharacterPhysics> physicsObjectList = new List<CharacterPhysics>();
    //충돌 발생 시 순서대로 기록되는 리스트
    private List<CollisionData> collisionDataList = new List<CollisionData>();

    public bool isAllStop = false;
    public bool isAllStopEventEnd = false;

    public void Init()
    {
        isAllStop = false;
        isAllStopEventEnd = false;

        physicsObjectList.Clear();
        collisionDataList.Clear();
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
        //모두 멈춘 경우 이벤트 발생
        AllStopEvent();
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

            //속도를 마찰력만큼 감소 시킨다.
            moveObj.speed -= moveObj.Friction;

            //0보다 작을경우 0으로 처리
            if (moveObj.speed < 0)
            {
                moveObj.speed = 0;
                moveObj.dir = Vector3.zero;
            }

            //아직 변하는 중이라면
            if (moveObj.speed > 0 &&
                moveObj.dir != Vector3.zero)
            {
                allStop = false;
                //체크할 오브젝트 리스트 순환
                foreach (var checkObj in physicsObjectList)
                {
                    //자신은 제외한다.
                    if (moveObj.PID == checkObj.PID)
                        continue;

                    //둘 다 움직이고 있을때 더 빠른 오브젝트를 기준으로 판단
                    if (moveObj.speed < checkObj.speed)
                        continue;

                    //충돌하기에 충분히 가까운 거리라면
                    if (CheckCollision(moveObj, checkObj))
                    {
                        //반동 계산
                        CalculateBouncing(moveObj, checkObj);
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
            if (moveObj != null &&
                moveObj.updateForce != null)
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

        //단 한번만 발생한다.
        if (isAllStopEventEnd)
        {
            GameManager.Instance.End();
            return;
        }

        isAllStopEventEnd = true;

        List<int> removeList = null;
        foreach (var obj in physicsObjectList)
        {
            if (obj.characterTransform.position.x < hoglineLine.position.x ||
                obj.characterTransform.position.x > backLine.position.x)
            {
                if (removeList == null)
                    removeList = new List<int>();

                removeList.Add(obj.PID);
            }
        }

        if (removeList != null)
        {
            foreach (var pid in removeList)
            {
                GameManager.Instance.RemoveCharacter(pid);
            }

            removeList.Clear();
            removeList = null;
        }           

        foreach (var moveObj in physicsObjectList)
        {            
            var findObj = GetFirstCollisionObject(moveObj.PID);
            moveObj?.allStopEvent(findObj, physicsObjectList);
        }
    }

    private bool CheckCollision(CharacterPhysics moveObj, CharacterPhysics checkObj)
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
        var difference = sumRadius - vc.magnitude;

        //현재 오브젝트의 방향과 두 오브젝트 사이의 벡터의 내적
        var dot = Vector3.Dot(moveObj.dir, vc);

        //충분히 가깝거나 두 벡터의 내적이 0인 경우 (자신의 앞에 있는 경우)
        bool isCollision = (difference > 0) && (dot > 0);
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
                vc, moveObjDir, sumRadius);

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
        //현재 오브젝트 벡터에 vc의 방향 벡터를 투영
        var proj11 = Vector3.Project(v1, vcd);
        //현재 오브젝트 벡터에 vc의 노말 벡터를 투영
        var proj12 = Vector3.Project(v1, vcn);

        //충돌된 오브젝트 벡터
        var v2 = checkObjTrans.localPosition - checkObj.prevPostion;
        //충돌된 오브젝트 벡터에 vc의 방향 벡터를 투영
        var proj21 = Vector3.Project(v2, vcd);
        //충돌된 오브젝트 벡터에 vc의 노말 벡터를 투영
        var proj22 = Vector3.Project(v2, vcn);

        //서로의 운동벡터 교환 (x축)
        float P = moveObj.Mass * proj11.x + checkObj.Mass * proj21.x;
        float V = proj11.x - proj21.x;
        float v2fx = (P + V * moveObj.Mass) / (moveObj.Mass + checkObj.Mass);
        float v1fx = v2fx - V;

        //서로의 운동벡터 교환 (z축)
        P = moveObj.Mass * proj11.z + checkObj.Mass * proj21.z;
        V = proj11.z - proj21.z;
        float v2fz = (P + V * moveObj.Mass) / (moveObj.Mass + checkObj.Mass);
        float v1fz = v2fz - V;
        //참조 -> https://brownsoo.github.io/2DVectors/moving_balls/

        //반동이 계산된 현재 오브젝트 벡터
        var newv1 = new Vector3(proj12.x + v1fx, 0, proj12.z + v1fz);
        //반동이 계산된 충돌된 오브젝트 벡터
        var newv2 = new Vector3(proj22.x + v2fx, 0, proj22.z + v2fz);

        //충돌 이벤트 발생
        moveObj?.collideEvent(checkObj, physicsObjectList);
        //충돌 된 이벤트 발생
        checkObj?.beCollidedEvent(moveObj, physicsObjectList);

        //공격력과 공격보너스를 합친 값 (발사단계에서 게이지 퍼센트 만큼 50%~120% 보너스)
        var attackValue = moveObj.character.finalAttack * moveObj.AttackBouns;

        //발사 할때와 충돌 했을때의 속도 감소율
        var speedRate = GameManager.Instance.IsCurrentPlayer(moveObj.character) ?
            (moveObj.speed / moveObj.FirstSpeed) : 1.0f;

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
        moveObj.updateForce = () =>
        {
            moveObj.dir = moveDir;
            moveObj.speed = moveSpeed;
        };

        //방향과 속력 업데이트 등록
        checkObj.updateForce = () =>
        {
            checkObj.dir = checkDir;
            checkObj.speed = checkSpeed;
        };

        //충돌 데이터 기록
        var collisionData = new CollisionData(collisionDataList.Count, moveObj.PID, checkObj.PID);
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

    private void OnDestroy()
    {
        physicsObjectList.Clear();
        collisionDataList.Clear();

        physicsObjectList = null;
        collisionDataList = null;

        instance = null;
    }
}
