using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterData data;
    public CharacterData Data { get => data; }

    private CharacterPhysics physics;
    public CharacterPhysics Physics { get => physics; }

    public float finalAttack;
    public float finalDefence;
    public float ImpulseAddValue;
    public float ImpulsePercentValue;
    public float ImmuneCount;
    public float lockCount;
    public int team;

    public void Init(CharacterData data, int team)
    {
        this.data = data;
        this.team = team;

        var pData = new CharacterPhysicsData(1, 1.5f);
        this.physics = new CharacterPhysics(
            data, pData, this,
            CollideEvent,
            BeCollidedEvent,
            AllStopEvent);

        InitState();
        InitEvent();
    }

    public void InitState()
    {
        finalAttack = data.attack;
        finalDefence = data.defence;
        ImpulseAddValue = 0;
        ImpulsePercentValue = 1.0f;
        ImmuneCount = 0;
        lockCount = 0;
    }

    public float GetDefenceValue()
    {
        return finalDefence * 100 / (finalDefence + 100);
    }

    public float GetImpulseValue(float attack)
    {
        return attack * (100 - GetDefenceValue()) / 100;
    }

    public void InitEvent()
    {
        foreach (var skill in data.skillDataArray)
        {
            if (skill.conditionType != (int)SkillData.ConditionType.Init)
                continue;

            var applyObjList = GetApplyObject(skill, null, null);
            if (applyObjList == null)
            {
                Debug.LogError("ApplyObjList is NULL");
                continue;
            }

            foreach (var obj in applyObjList)
            {
                ApplySkill(skill, obj, true);
            }
        }
    }

    public void CollideEvent(CharacterPhysics otherObj)
    {
        foreach (var skill in data.skillDataArray)
        {
            if (skill.conditionType != (int)SkillData.ConditionType.Collide)
                continue;

            var applyObjList = GetApplyObject(skill, otherObj, null);
            if (applyObjList == null)
            {
                Debug.LogError("ApplyObjList is NULL");
                continue;
            }

            if (CheckSkill(skill, otherObj))
            {
                foreach (var obj in applyObjList)
                {
                    ApplySkill(skill, obj, false);
                }
            }
        }
    }

    public void BeCollidedEvent(CharacterPhysics otherObj)
    {
        foreach (var skill in data.skillDataArray)
        {
            if (skill.conditionType != (int)SkillData.ConditionType.BeCollide)
                continue;

            var applyObjList = GetApplyObject(skill, otherObj, null);
            if (applyObjList == null)
            {
                Debug.LogError("ApplyObjList is NULL");
                continue;
            }

            if (CheckSkill(skill, otherObj))
            {
                foreach (var obj in applyObjList)
                {
                    ApplySkill(skill, obj, false);
                }
            }
        }
    }

    public void AllStopEvent(List<CharacterPhysics> physicsObjectList)
    {
        foreach (var skill in data.skillDataArray)
        {
            if (skill.conditionType != (int)SkillData.ConditionType.AllStop)
                continue;

            var applyObjList = GetApplyObject(skill, null, physicsObjectList);
            if (applyObjList == null)
            {
                Debug.Log(string.Format("{0}스킬 '{1}'\n발동 무시",
                    transform.name, skill.desc));
                continue;
            }

            var checkObjList = CheckSkill(skill, applyObjList);
            foreach (var obj in checkObjList)
            {
                ApplySkill(skill, obj, true);
            }
        }
    }

    public List<CharacterPhysics> GetApplyObject(SkillData skill, CharacterPhysics collisionObj, List<CharacterPhysics> physicsObjectList)
    {
        switch (skill.applyObject)
        {
            case 0: //없음
                break;
            case 1: //본인
                return new List<CharacterPhysics>() { this.physics };
            case 2: //충돌된 대상
                return new List<CharacterPhysics>() { collisionObj };
            case 3: //가장 가까운 아군
                break;
            case 4: //가장 가까운 적군
                break;
            case 5: //가장 가까운 아군/적군
                {
                    var obj = GetNearCharacter(physicsObjectList);
                    if (obj != null)
                    {
                        return new List<CharacterPhysics>() { obj };
                    }
                    break;
                }
            case 6: //범위 안 아군
                break;
            case 7: //범위 안 적군
                break;
            case 8: //범위 안 아군/적군
                return GetCharacterInRange(skill.applyRange, physicsObjectList);
            case 9: //랜덤한 아군
                break;
            case 10: //랜덤한 적군
                break;
            case 11: //랜덤한 아군/적군
                {
                    var obj = GetRandomCharacter(physicsObjectList);
                    if (obj != null)
                    {
                        return new List<CharacterPhysics>() { obj };
                    }
                    break;
                }
            case 12: //모든 대상
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
            if (obj.pid == this.Physics.pid)
                continue;

            var dis = Vector3.Distance(
                this.transform.localPosition,
                obj.characterTransform.localPosition);

            if (dis < nearDistance || nearDistance == -1)
            {
                nearDistance = dis;
                findObj = obj;
            }
        }

        return findObj;
    }

    public List<CharacterPhysics> GetCharacterInRange(float range, List<CharacterPhysics> physicsObjectList)
    {
        List<CharacterPhysics> findObjList = null;

        foreach (var obj in physicsObjectList)
        {
            if (obj.pid == this.Physics.pid)
                continue;

            var dis = Vector3.Distance(
                this.transform.localPosition,
                obj.characterTransform.localPosition);

            range *= GameManager.DISTACNE;
            if (dis <= range)
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
            if (obj.pid == this.Physics.pid)
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
        var otherCharacter = otherObj.character;

        if (skillData.id == 0)
            return false;

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

        //자신에게 스킬 봉인 횟수가 남아 있다면  (-1 = 무한)
        if (this.lockCount > 0 || this.lockCount == -1)
        {
            if (this.lockCount != -1)
                this.lockCount -= 1;

            Debug.Log(string.Format("스킬 '{0} 발동 무시'\n{1} - 'lockCount'에서 제외 {2}의 남은 스킬 봉인 횟수 {3}",
                skillData.desc, otherCharacter.name, this.name, this.lockCount));
            return false;
        }

        //상대에게 스킬 면역 횟수가 남아 있다면 (-1 = 무한)
        if (otherCharacter.ImmuneCount > 0 || otherCharacter.ImmuneCount == -1)
        {
            //상대 기준으로 면역 스킬 조건 체크
            if (otherCharacter.CheckImmuneSkill(this))
            {
                if (otherCharacter.ImmuneCount != -1)
                    otherCharacter.ImmuneCount -= 1;

                Debug.Log(string.Format("스킬 '{0} 발동 무시'\n{1} - 'ImmuneCount'에서 제외 {2}의 남은 스킬 면역 횟수 {3}",
                    skillData.desc, otherCharacter.name, otherCharacter.name, otherCharacter.ImmuneCount));
                return false;
            }
        }

        return true;
    }

    public bool CheckImmuneSkill(Character otherCharacter)
    {
        foreach (var skill in Data.skillDataArray)
        {
            if (!CheckRarity(skill, otherCharacter))
            {
                return false;
            }
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

    public bool CheckDefanceOver(SkillData skillData, Character otherCharacter)
    {
        //공격력이 방어력을 넘는지 체크 안한다면 (체크 안함)
        if (skillData.isDefanceOver != 1)
            return true;

        //상대의 공격력이 자신의 방어력을 넘지 못한 경우 (발동)
        if (otherCharacter.finalAttack < this.finalDefence) return true;
        //상대의 공격력이 자신의 방어력을 넘은 경우 (발동 안함)
        else return false;
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

    //첫 충돌인지 체크
    public bool CheckFirstCollide(SkillData skillData)
    {
        //첫 충돌 체크를 안한다면 (체크 안함)
        if (skillData.isFirstCollide != 1)
            return true;

        //첫 충돌이 이미 발생한 뒤라면 (발동 안함)
        if (this.physics.isFirstCollide)
            return false;
        //아직 첫 충돌이 발생하지 않은 경우 (발동)
        else return true;
    }

    public bool CheckObjectTeam(SkillData skillData, Character otherCharacter)
    {
        //조건이 없거나 아군/적군 둘다 일때
        if (skillData.conditionObjectType == 0 ||
            skillData.conditionObjectType == 3)
            return true;

        //서로 같은 팀인지 체크
        bool isSameTeam = this.team == otherCharacter.team;
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

    public void ApplySkill(SkillData skillData, CharacterPhysics applyObj, bool isAllStop)
    {
        Debug.Log(string.Format("스킬 발동 {0} -> {1}\n{2}",
           transform.name, applyObj.characterTransform.name, skillData.desc));

        var applyCharacter = applyObj.character;
        switch (skillData.applyType)
        {
            case 1: //공격력
                {
                    var value = GetApplyValueType(skillData.applyValueType, applyCharacter.finalAttack, this);
                    var finalValue = value * skillData.applyValue;

                    applyCharacter.finalAttack = finalValue;
                    break;
                }
            case 2: //방어력
                {
                    var value = GetApplyValueType(skillData.applyValueType, applyCharacter.finalDefence, this);
                    var finalValue = value * skillData.applyValue;

                    applyCharacter.finalDefence = finalValue;
                    break;
                }
            case 3: //충격량
                {
                    var value = GetApplyValueType(skillData.applyValueType, applyCharacter.ImpulseAddValue, this);
                    var finalValue = value * skillData.applyValue;

                    applyCharacter.ImpulsePercentValue = skillData.applyPercentValue;

                    if (isAllStop)
                    {
                        var dir = Vector3.Normalize(applyCharacter.transform.localPosition - this.transform.localPosition);
                        var speed = applyCharacter.Physics.GetQuadraticEquationValue((finalValue * GameManager.DISTACNE) * 2);

                        applyCharacter.Physics.ApplyForce(dir, speed);

                        Debug.Log(string.Format("준 충격량 : {0}\n방향 : {1} 속도 : {2}",
                            finalValue, dir, speed));
                    }
                    else
                    {
                        applyCharacter.ImpulseAddValue = applyCharacter.GetImpulseValue(finalValue);
                    }
                    break;
                }
            case 4: //스킬 면역 횟수
                {
                    var finalValue = skillData.applyValue;
                    applyCharacter.ImmuneCount = finalValue;
                }
                break;
            case 5: //스킬 봉인 횟수
                {
                    var finalValue = skillData.applyValue;
                    applyCharacter.lockCount = finalValue;
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

    public float GetShootSpeed(float attackBouns = 1f)
    {
        return Mathf.Sqrt(100f * ((0.6f * (Data.attack * attackBouns)) + 30f)) + 15f;
    }

    public void RefreshData(CharacterData data)
    {
        this.data = data;
        physics.RefreshData(data);

        InitState();
        InitEvent();
    }

    public void RemoveCharacterPhysics()
    {
        physics.RemovePhysicsObject();
        physics = null;
    }

    private void OnDestroy()
    {
        physics = null;
    }
}
