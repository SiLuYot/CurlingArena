using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseSkillEvent
{
    private SkillData.ConditionType type;
    private CharacterSkill characterSkill;
    private List<SkillData> skillList;

    public BaseSkillEvent(SkillData.ConditionType type, CharacterSkill characterSkill)
    {
        this.type = type;
        this.characterSkill = characterSkill;
        this.skillList = characterSkill.skillList;
    }

    public void Event(CharacterPhysics otherObj = null, List<CharacterPhysics> physicsObjectList = null)
    {
        foreach (var skill in skillList)
        {
            if (skill.cType != type)
                continue;

            var occurStandardObject = characterSkill.GetOccurStandardObject(skill, otherObj);

            var applyObjList = characterSkill.GetApplyObject(skill, occurStandardObject, otherObj, physicsObjectList);
            if (applyObjList == null)
            {
                Debug.Log("적용될 오브젝트가 없다.");
                continue;
            }

            //충돌된 오브젝트 체크 후 발동 되는지 판단
            if (characterSkill.CheckSkill(skill, otherObj))
            {
                //적용될 오브젝트들 모두 체크
                var checkObjList = characterSkill.CheckSkill(skill, applyObjList);
                foreach (var obj in checkObjList)
                {
                    //선발된 오브젝트들만 스킬 적용
                    characterSkill.ApplySkill(skill, occurStandardObject, obj);
                }
            }
        }
    }

    public IEnumerator EventCoroutine(CharacterPhysics otherObj = null, List<CharacterPhysics> physicsObjectList = null)
    {
        foreach (var skill in skillList)
        {
            if (skill.cType != type)
                continue;

            yield return PhysicsManager.Instance.StartPhysicsCoroutine(
                characterSkill.GetOccurStandardObjectCoroutine(skill, otherObj));

            var occurStandardObject = characterSkill.occurStandardObject;

            var applyObjList = characterSkill.GetApplyObject(skill, occurStandardObject, otherObj, physicsObjectList);
            if (applyObjList == null)
            {
                Debug.Log("적용될 오브젝트가 없다.");
                continue;
            }

            //충돌된 오브젝트가 있을수도 있고 없을수도 있다.
            var checkObjList = characterSkill.CheckSkill(skill, applyObjList);
            foreach (var obj in checkObjList)
            {
                characterSkill.ApplySkill(skill, occurStandardObject, obj);
            }
        }
    }

}

public class CharacterSkill
{
    public Character character;
    public List<SkillData> skillList;
    private Transform characterTransform;

    private BaseSkillEvent initEvent;
    private BaseSkillEvent firstShootEvent;
    private BaseSkillEvent collideEvent;
    private BaseSkillEvent beCollidedEvent;
    private BaseSkillEvent allStopEvent;

    public CharacterPhysics occurStandardObject;

    public CharacterSkill(Character character)
    {
        this.character = character;
        this.skillList = character.Data.skillDataArray.ToList();
        this.characterTransform = character.transform;

        initEvent = new BaseSkillEvent(SkillData.ConditionType.Init, this);
        firstShootEvent = new BaseSkillEvent(SkillData.ConditionType.FirstShoot, this);
        collideEvent = new BaseSkillEvent(SkillData.ConditionType.Collide, this);
        beCollidedEvent = new BaseSkillEvent(SkillData.ConditionType.BeCollide, this);
        allStopEvent = new BaseSkillEvent(SkillData.ConditionType.AllStop, this);
    }

    public void InitEvent()
    {
        initEvent.Event();
    }

    public void FirstShootEvent()
    {
        firstShootEvent.Event();
    }

    public void CollideEvent(CharacterPhysics otherObj, List<CharacterPhysics> physicsObjectList)
    {
        collideEvent.Event(otherObj, physicsObjectList);
    }

    public void BeCollidedEvent(CharacterPhysics otherObj, List<CharacterPhysics> physicsObjectList)
    {
        beCollidedEvent.Event(otherObj, physicsObjectList);
    }

    public IEnumerator AllStopEvent(CharacterPhysics firstCollisionObj, List<CharacterPhysics> physicsObjectList)
    {
        yield return PhysicsManager.Instance.StartPhysicsCoroutine(allStopEvent.EventCoroutine(firstCollisionObj, physicsObjectList));
    }

    public void AddSynergySkill(List<SkillData> skillDataList)
    {
        foreach (var data in skillDataList)
        {
            skillList.Add(data);
        }
    }

    public IEnumerator GetOccurStandardObjectCoroutine(SkillData skill, CharacterPhysics collisionObj)
    {
        occurStandardObject = null;
        switch (skill.occurStandard)
        {
            case 0:
                {
                    Debug.Log(string.Format("{0}의 OccurStandardObject : {1} (본인)", characterTransform.name, skill.occurStandard));
                    occurStandardObject = character.Physics;
                    break;
                }
            case 1:
                {
                    Debug.Log(string.Format("{0}의 OccurStandardObject : {1} (충돌 된 상대)", characterTransform.name, skill.occurStandard));
                    occurStandardObject = collisionObj;
                    break;
                }
            case 2:
                {
                    Debug.Log(string.Format("{0}의 OccurStandardObject : {1} (위치 직접 지정)", characterTransform.name, skill.occurStandard));

                    var findPID = PhysicsManager.Instance.FindFirstCollision(character.Physics.PID);
                    //첫 충돌한 기록은 있지만
                    if (findPID != -1)
                    {
                        //그 오브젝트가 비활성화(삭제대기) 상태라면
                        var findObj = PhysicsManager.Instance.GetPhysicsObject(findPID);
                        if (findObj.isInActive)
                        {
                            //오픈된 모든 UI들을 숨기고
                            UIManager.Instance.SetAllActive(false);

                            //위치 지정 UI 오픈
                            var selectUI = UIManager.Instance.Get<PositionSelectUI>() as PositionSelectUI;
                            selectUI.Init(skill.desc, findObj);

                            //위치를 지정할때 까지 기다린다.
                            yield return PhysicsManager.Instance.StartPhysicsCoroutine(
                                selectUI.WaitSelectPosition());

                            //위치가업데이트 된 오브젝트를 받아온다
                            occurStandardObject = selectUI.newPosObj;

                            //위치 지정 UI를 닫고
                            selectUI.Close();

                            //다시 오픈 된 UI들을 활성화
                            UIManager.Instance.SetAllActive(true);
                        }
                    }

                    break;
                }
        }
    }

    public CharacterPhysics GetOccurStandardObject(SkillData skill, CharacterPhysics collisionObj)
    {
        occurStandardObject = null;
        switch (skill.occurStandard)
        {
            case 0:
                {
                    Debug.Log(string.Format("{0}의 OccurStandardObject : {1} (본인)", characterTransform.name, skill.occurStandard));
                    occurStandardObject = character.Physics;
                    break;
                }
            case 1:
                {
                    Debug.Log(string.Format("{0}의 OccurStandardObject : {1} (충돌 된 상대)", characterTransform.name, skill.occurStandard));
                    occurStandardObject = collisionObj;
                    break;
                }
            case 2:
                {
                    Debug.Log(string.Format("{0}의 OccurStandardObject : {1} (위치 직접 지정)", characterTransform.name, skill.occurStandard));
                    break;
                }
        }

        return occurStandardObject;
    }

    public List<CharacterPhysics> GetApplyObject(SkillData skill, CharacterPhysics occurStandardObj, CharacterPhysics collisionObj, List<CharacterPhysics> physicsObjectList)
    {
        Debug.Log(string.Format("{0}의 스킬 발동 준비\n{1}", characterTransform.name, skill.desc));

        switch (skill.applyObject)
        {
            case 0: //없음
                Debug.Log(string.Format("applyObject : {0} (적용할 오브젝트 없음)", skill.applyObject));
                break;
            case 1: //본인
                Debug.Log(string.Format("applyObject : {0} (본인에게 적용)", skill.applyObject));
                return new List<CharacterPhysics>() { occurStandardObj };
            case 2: //충돌된 대상
                Debug.Log(string.Format("applyObject : {0} (충돌된 대상에게 적용)", skill.applyObject));
                return new List<CharacterPhysics>() { collisionObj };
            case 3: //가장 가까운 아군
                Debug.Log(string.Format("applyObject : {0} (가장 가까운 아군에게 적용)", skill.applyObject));
                break;
            case 4: //가장 가까운 적군
                Debug.Log(string.Format("applyObject : {0} (가장 가까운 적군에게 적용)", skill.applyObject));
                break;
            case 5: //가장 가까운 아군/적군
                {
                    Debug.Log(string.Format("applyObject : {0} (가장 가까운 아군/적군에게 적용)", skill.applyObject));
                    var obj = GetNearCharacter(physicsObjectList);
                    if (obj != null)
                    {
                        return new List<CharacterPhysics>() { obj };
                    }
                    break;
                }
            case 6: //범위 안 아군
                Debug.Log(string.Format("applyObject : {0} (범위 안 아군에게 적용)", skill.applyObject));
                break;
            case 7: //범위 안 적군
                Debug.Log(string.Format("applyObject : {0} (범위 안 적군에게 적용)", skill.applyObject));
                break;
            case 8: //범위 안 아군/적군
                Debug.Log(string.Format("applyObject : {0} (범위 안 아군/적군에게 적용)", skill.applyObject));
                return GetCharacterInRange(skill.applyAngle, skill.applyRange, occurStandardObj, collisionObj, physicsObjectList);
            case 9: //랜덤한 아군
                Debug.Log(string.Format("applyObject : {0} (랜덤한 아군에게 적용)", skill.applyObject));
                break;
            case 10: //랜덤한 적군
                Debug.Log(string.Format("applyObject : {0} (랜덤한 적군에게 적용)", skill.applyObject));
                break;
            case 11: //랜덤한 아군/적군
                Debug.Log(string.Format("applyObject : {0} (랜덤한 아군/적군에게 적용)", skill.applyObject));
                {
                    var obj = GetRandomCharacter(physicsObjectList);
                    if (obj != null)
                    {
                        return new List<CharacterPhysics>() { obj };
                    }
                    break;
                }
            case 12: //모든 대상
                Debug.Log(string.Format("applyObject : {0} (모든 대상에게 적용)", skill.applyObject));
                break;
        }

        return null;
    }

    public CharacterPhysics GetNearCharacter(List<CharacterPhysics> physicsObjectList)
    {
        float nearDistance = -1;
        CharacterPhysics findObj = null;

        foreach (var obj in physicsObjectList)
        {
            if (obj.PID == character.Physics.PID)
                continue;

            var dis = Vector3.Distance(
                characterTransform.localPosition,
                obj.characterTransform.localPosition);

            if (dis < nearDistance || nearDistance == -1)
            {
                nearDistance = dis;
                findObj = obj;
            }
        }

        return findObj;
    }

    public List<CharacterPhysics> GetCharacterInRange(float angleValue, float rangeValue, CharacterPhysics occurStandardObj, CharacterPhysics collisionObj, List<CharacterPhysics> physicsObjectList)
    {
        List<CharacterPhysics> findObjList = null;
        Vector3 dir = Vector3.zero;

        if (occurStandardObj == null)
            return findObjList;

        if (collisionObj != null)
            dir = Vector3.Normalize(
                collisionObj.characterTransform.localPosition -
                characterTransform.localPosition);

        rangeValue = (rangeValue * GameManager.DISTACNE) + occurStandardObj.Radius;

        foreach (var obj in physicsObjectList)
        {
            if (obj.PID == occurStandardObj.PID)
                continue;

            var dis = Vector3.Distance(
                occurStandardObj.characterTransform.localPosition,
                obj.characterTransform.localPosition);

            var targetDir = Vector3.Normalize(
                obj.characterTransform.localPosition -
                occurStandardObj.characterTransform.localPosition);

            var angle = Vector3.Angle(dir, targetDir);

            if (angle <= angleValue && dis <= rangeValue)
            {
                if (findObjList == null)
                    findObjList = new List<CharacterPhysics>();

                findObjList.Add(obj);
            }
        }

        return findObjList;
    }

    public CharacterPhysics GetRandomCharacter(List<CharacterPhysics> physicsObjectList)
    {
        var randomList = new List<CharacterPhysics>();
        foreach (var obj in physicsObjectList)
        {
            if (obj.PID == character.Physics.PID)
                continue;

            randomList.Add(obj);
        }

        if (randomList.Count <= 0)
            return null;

        var randomID = UnityEngine.Random.Range(0, randomList.Count);
        var randomObj = randomList.ElementAt(randomID);

        return randomObj;
    }

    public bool CheckSkill(SkillData skillData, CharacterPhysics otherObj)
    {
        //스킬이 없다면 false
        if (skillData.id == 0)
            return false;

        //체크할 상대가 없다면 true
        if (otherObj == null)
            return true;

        var otherCharacter = otherObj.character;

        //첫 충돌이 아니라면
        if (!CheckFirstCollide(skillData))
        {
            Debug.Log(string.Format("스킬 '{0}'\n발동 무시 {1} - 'CheckFirstCollide'에서 제외",
                skillData.desc, otherCharacter.name));

            return false;
        }

        //스킬이 발동되는 사이즈가 아니라면
        if (!CheckSize(skillData, otherCharacter))
        {
            Debug.Log(string.Format("스킬 '{0}'\n발동 무시 {1} - 'CheckSize'에서 제외",
               skillData.desc, otherCharacter.name));

            return false;
        }

        //상대의 공격력이 자신의 방어력을 넘겼다면
        if (!CheckDefanceOver(skillData, otherCharacter))
        {
            Debug.Log(string.Format("스킬 '{0}'\n발동 무시 {1} - 'CheckDefanceOver'에서 제외",
               skillData.desc, otherCharacter.name));

            return false;
        }

        //발동되는 팀 조건이 아니라면
        if (!CheckObjectTeam(skillData, otherCharacter))
        {
            Debug.Log(string.Format("스킬 '{0}'\n발동 무시 {1} - 'CheckObjectTeam'에서 제외",
               skillData.desc, otherCharacter.name));

            return false;
        }

        //자신에게 스킬 봉인 횟수가 남아 있다면
        if (!CheckSkillLockCount())
        {
            Debug.Log(string.Format("스킬 '{0} 발동 무시'\n{1} - 'lockCount'에서 제외 {2}의 남은 스킬 봉인 횟수 {3}",
                skillData.desc, otherCharacter.name, character.name, character.lockCount));

            return false;
        }

        //상대에게 스킬 면역 횟수가 남아 있다면
        if (!CheckSkillImmuneCount(skillData, otherCharacter))
        {
            Debug.Log(string.Format("스킬 '{0} 발동 무시'\n{1} - 'ImmuneCount'에서 제외 {2}의 남은 스킬 면역 횟수 {3}",
                   skillData.desc, otherCharacter.name, otherCharacter.name, otherCharacter.immuneCount));

            return false;
        }

        return true;
    }

    public List<CharacterPhysics> CheckSkill(SkillData skillData, List<CharacterPhysics> objList)
    {
        var checkObjList = new List<CharacterPhysics>();

        foreach (var obj in objList)
        {
            if (CheckSkill(skillData, obj))
            {
                checkObjList.Add(obj);
            }
        }

        return checkObjList;
    }

    public bool CheckFirstCollide(SkillData skillData)
    {
        //첫 충돌 체크를 안한다면 (체크 안함)
        if (skillData.isFirstCollide != 1)
            return true;

        //이 오브젝트와 한번이라도 충돌된 오브젝트의 pid를 받아온다
        var firstCollisionPID = PhysicsManager.Instance.FindFirstCollision(character.Physics.PID);

        //첫 충돌이 이미 발생한 뒤라면 (발동 안함)
        if (firstCollisionPID != -1)
            return false;
        //아직 첫 충돌이 발생하지 않은 경우 (발동)
        else return true;
    }

    public bool CheckSize(SkillData skillData, Character otherCharacter)
    {
        switch (skillData.checkSize)
        {
            case 0: //조건 없음
                return true;
            case 1: //0 한정
                return otherCharacter.Data.sizeData.id == 0;
            case 2: //0 이상
                return otherCharacter.Data.sizeData.id >= 0;
            case 3: //1 이하
                return otherCharacter.Data.sizeData.id <= 1;
            case 4: //1 이상
                return otherCharacter.Data.sizeData.id >= 1;
            case 5: //2 이하
                return otherCharacter.Data.sizeData.id <= 2;
            case 6: //2 한정
                return otherCharacter.Data.sizeData.id == 2;
            default: //그 외 false
                return false;
        }
    }

    public void CheckOneShot(SkillData skillData)
    {
        //1회성 스킬이 아니라면 리턴
        if (skillData.isOneShot != 1)
            return;

        //스텟 초기화 이벤트 등록 해야함...
        //Action tempAction = null;
        //tempAction = () =>
        //{
        //    InitFinalState();
        //    TempEvent -= tempAction;
        //};

        //TempEvent += tempAction;
    }

    public bool CheckDefanceOver(SkillData skillData, Character otherCharacter)
    {
        //공격력이 방어력을 넘는지 체크 안한다면 (체크 안함)
        if (skillData.isDefanceOver != 1)
            return true;

        //상대의 공격력이 자신의 방어력을 넘지 못한 경우 (발동)
        if (otherCharacter.finalAttack < character.finalDefence) return true;
        //상대의 공격력이 자신의 방어력을 넘은 경우 (발동 안함)
        else return false;
    }

    public bool CheckObjectTeam(SkillData skillData, Character otherCharacter)
    {
        //조건이 없거나 아군/적군 둘다 일때
        if (skillData.conditionObjectType == 0 ||
            skillData.conditionObjectType == 3)
            return true;

        //서로 같은 팀인지 체크
        bool isSameTeam = character.Team == otherCharacter.Team;
        if (isSameTeam)
        {
            //서로 같은 팀이고 조건도 아군일때
            if (skillData.conditionObjectType == 1)
                return true;
        }
        else
        {
            //서로 다른 팀이고 조건도 적군일때
            if (skillData.conditionObjectType == 2)
                return true;
        }

        //그 외에는 false
        return false;
    }

    public bool CheckSkillLockCount()
    {
        // (-1 = 무한)
        if (character.lockCount > 0 || character.lockCount == -1)
        {
            DecreaseLockCount();
            return false;
        }

        return true;
    }

    public bool CheckSkillImmuneCount(SkillData skillData, Character otherCharacter)
    {
        //적용되려는 효과가 면역 횟수라면 리턴
        if (skillData.applyType == 4)
            return true;

        // (-1 = 무한)
        if (otherCharacter.immuneCount > 0 || otherCharacter.immuneCount == -1)
        {
            //상대 기준으로 면역 스킬 조건 체크
            if (otherCharacter.Skill.CheckImmuneSkill(character))
            {
                DecreaseImmuneCount();
                return false;
            }
        }

        return true;
    }

    public bool CheckImmuneSkill(Character otherCharacter)
    {
        foreach (var skill in skillList)
        {
            if (!CheckRarity(skill, otherCharacter))
            {
                return false;
            }
        }

        return true;
    }

    public bool CheckRarity(SkillData skillData, Character otherCharacter)
    {
        switch (skillData.checkOtherRarity)
        {
            case 0: //모두 허용
                return true;
            case 1: //베이직 한정
                return otherCharacter.Data.rarityData.id == 0;
            case 2: //노말 이하
                return otherCharacter.Data.rarityData.id <= 1;
            case 3: //노말 이상
                return otherCharacter.Data.rarityData.id >= 1;
            case 4: //레어 이하
                return otherCharacter.Data.rarityData.id <= 2;
            case 5: //레어 이상
                return otherCharacter.Data.rarityData.id >= 2;
            case 6: //슈퍼레어 이하
                return otherCharacter.Data.rarityData.id <= 3;
            case 7: //슈퍼레어 이상
                return otherCharacter.Data.rarityData.id >= 3;
            case 8: //유니크 이하
                return otherCharacter.Data.rarityData.id <= 4;
            case 9: //유니크 이상
                return otherCharacter.Data.rarityData.id >= 4;
            case 10: //레전드 이하
                return otherCharacter.Data.rarityData.id <= 5;
            case 11: //레전드 한정
                return otherCharacter.Data.rarityData.id >= 5;
            default: //그 외
                return false;
        }
    }

    public void ApplySkill(SkillData skillData, CharacterPhysics occurStandardObj, CharacterPhysics applyObj)
    {
        Debug.Log(string.Format("{0}의 스킬 발동 {1} -> {2}\n{3}",
           characterTransform.name, occurStandardObj.characterTransform.name,
           applyObj.characterTransform.name, skillData.desc));

        var applyCharacter = applyObj.character;
        switch (skillData.applyType)
        {
            case 1: //공격력
                {
                    var value = GetApplyValueType(skillData.applyValueType, applyCharacter.finalAttack, character);
                    var finalValue = value * skillData.applyValue;

                    applyCharacter.finalAttack = finalValue;
                    break;
                }
            case 2: //방어력
                {
                    var value = GetApplyValueType(skillData.applyValueType, applyCharacter.finalDefence, character);
                    var finalValue = value * skillData.applyValue;

                    applyCharacter.finalDefence = finalValue;
                    break;
                }
            case 3: //충격량
                {
                    var dir = Vector3.Normalize(
                        applyCharacter.transform.localPosition -
                        occurStandardObj.characterTransform.localPosition);

                    var value = GetApplyValueType(skillData.applyValueType, applyCharacter.ImpulseAddValue, character);
                    var finalValue = value * skillData.applyValue;

                    applyCharacter.ImpulsePercentValue = skillData.applyPercentValue;

                    //적용될 오브젝트가 멈취있다면
                    if (applyCharacter.Physics.dir == Vector3.zero ||
                        applyCharacter.Physics.speed <= 0)
                    {
                        var speed = applyCharacter.Physics.GetQuadraticEquationValue(finalValue);
                        applyCharacter.Physics.ApplyForce(dir, speed);

                        Debug.Log(string.Format("준 충격량 : {0}\n방향 : {1} 속도 : {2}",
                            finalValue, dir, speed));
                    }
                    else
                    {
                        applyCharacter.ImpulseAddDir = dir;
                        applyCharacter.ImpulseAddValue = applyCharacter.GetImpulseValue(finalValue);

                        Debug.Log(string.Format("준 충격량 : {0}\n방향 : {1}",
                            finalValue, dir));
                    }
                    break;
                }
            case 4: //스킬 면역 횟수
                {
                    var finalValue = skillData.applyValue;

                    //이미 무한인 경우 제외
                    if (applyCharacter.immuneCount != -1)
                    {
                        if (finalValue == -1)
                            applyCharacter.immuneCount = finalValue;
                        else
                            applyCharacter.immuneCount += finalValue;
                    }
                }
                break;
            case 5: //스킬 봉인 횟수
                {
                    var finalValue = skillData.applyValue;

                    //이미 무한인 경우 제외
                    if (applyCharacter.lockCount != -1)
                    {
                        if (finalValue == -1)
                            applyCharacter.lockCount = finalValue;
                        else
                            applyCharacter.lockCount += finalValue;
                    }
                }
                break;
            case 6: //공격력 방어력
                {
                    var attackValue = GetApplyValueType(skillData.applyValueType, applyCharacter.finalAttack, character);
                    var finalAttackValue = attackValue * skillData.applyValue;
                    
                    var defenceValue = GetApplyValueType(skillData.applyValueType, applyCharacter.finalDefence, character);
                    var finalDefenceValue = defenceValue * skillData.applyValue;

                    applyCharacter.finalAttack = finalAttackValue;
                    applyCharacter.finalDefence = finalDefenceValue;
                }
                break;
        }

        CheckOneShot(skillData);
    }

    public float GetApplyValueType(int valueType, float type, Character character)
    {
        switch (valueType)
        {
            case 1:
                return character.finalAttack;
            case 2:
                return character.finalDefence;
            default:
                return type;
        }
    }

    public void DecreaseImmuneCount(float count = 1)
    {
        if (character.immuneCount == 0)
            return;

        if (character.immuneCount != -1)
            character.immuneCount -= count;
    }

    public void DecreaseLockCount(float count = 1)
    {
        if (character.lockCount == 0)
            return;

        if (character.lockCount != -1)
            character.lockCount -= count;
    }
}
