using System;
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
    public float addImpulse;
    public int team;
    public event Action TempEvent;

    public void Init(CharacterData data, int team)
    {
        this.data = data;
        this.team = team;

        var pData = new CharacterPhysicsData(1, 1f);
        this.physics = new CharacterPhysics(
            data, pData, this,
            CollideEvent,
            BeCollidedEvent,
            AllStopEvent);

        InitFinalState();
    }

    public void InitFinalState()
    {
        finalAttack = data.attack;
        finalDefence = data.defence;
        addImpulse = 0;
    }

    public float GetDefenceValue()
    {
        return finalDefence * 100 / (finalDefence + 100);
    }

    public float GetImpulseValue(float attack)
    {
        return attack * (100 - GetDefenceValue()) / 100;
    }

    public bool CheckSize(SkillData skillData, Character otherCharacter)
    {
        switch (skillData.checkSize)
        {
            case 0:
                return true;
            case 1:
                return otherCharacter.Data.sizeData.id == 0;
            case 2:
                return otherCharacter.Data.sizeData.id >= 0;
            case 3:
                return otherCharacter.Data.sizeData.id <= 1;
            case 4:
                return otherCharacter.Data.sizeData.id >= 1;
            case 5:
                return otherCharacter.Data.sizeData.id <= 2;
            case 6:
                return otherCharacter.Data.sizeData.id == 2;
            default:
                return false;
        }
    }

    public bool CheckRarity(SkillData skillData, Character otherCharacter)
    {
        switch (skillData.checkRarity)
        {
            case 0:
                return true;
            case 1:
                return otherCharacter.Data.rarityData.id == 0;
            case 2:
                return otherCharacter.Data.rarityData.id <= 1;
            case 3:
                return otherCharacter.Data.rarityData.id >= 1;
            case 4:
                return otherCharacter.Data.rarityData.id <= 2;
            case 5:
                return otherCharacter.Data.rarityData.id >= 2;
            case 6:
                return otherCharacter.Data.rarityData.id <= 3;
            case 7:
                return otherCharacter.Data.rarityData.id >= 3;
            case 8:
                return otherCharacter.Data.rarityData.id <= 4;
            case 9:
                return otherCharacter.Data.rarityData.id >= 4;
            case 10:
                return otherCharacter.Data.rarityData.id <= 5;
            case 11:
                return otherCharacter.Data.rarityData.id == 5;
            default:
                return false;
        }
    }

    public bool CheckDefenceOver(SkillData skillData, Character otherCharacter)
    {
        //1인 경우만 체크
        if (skillData.checkDefenceOver == 1)
        {
            //충돌된 대상이 나의 방어력을 못넘긴 경우
            if (otherCharacter.finalAttack < this.finalDefence) return true;
            else return false;
        }
        else return true;

    }

    public void CheckOneShot(SkillData skillData)
    {
        if (skillData.isOneShot != 1)
            return;

        Action tempAction = null;
        tempAction = () =>
        {
            InitFinalState();
            TempEvent -= tempAction;
        };

        TempEvent += tempAction;
    }

    public bool CheckFirstCollide(SkillData skillData)
    {
        if (skillData.isFirstCollide == 1)
        {
            //첫 충돌이 이미 발생한 뒤라면
            if (this.physics.isFirstCollide) return false;
            else return true;
        }
        else return true;
    }

    public bool CheckObjectTeam(SkillData skillData, Character otherCharacter)
    {
        //조건이 없거나 둘 다 일때
        if (skillData.conditionObjectType == 0 ||
            skillData.conditionObjectType == 3)
            return true;

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

        return false;
    }

    public void ShootEvent()
    {
        TempEvent?.Invoke();
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
                Debug.Log(string.Format("{0}스킬 '{1}'\n발동 무시 원인 : ApplyObjList is NULL", 
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

        if (!CheckFirstCollide(skillData))
            return false;

        if (!CheckSize(skillData, otherCharacter))
            return false;

        if (!CheckRarity(skillData, otherCharacter))
            return false;

        if (!CheckDefenceOver(skillData, otherCharacter))
            return false;

        if (!CheckObjectTeam(skillData, otherCharacter))
            return false;

        return true;
    }

    public List<CharacterPhysics> CheckSkill(SkillData skillData, List<CharacterPhysics> objList)
    {
        var checkObjList = new List<CharacterPhysics>();

        if (skillData.id == 0)
            return checkObjList;

        if (!CheckFirstCollide(skillData))
            return checkObjList;

        foreach (var obj in objList)
        {
            var otherCharacter = obj.character;

            if (!CheckSize(skillData, otherCharacter))
            {
                Debug.LogError(string.Format("'CheckSize'에서 제외됨 : {0}", otherCharacter.name));
                continue;
            }

            if (!CheckRarity(skillData, otherCharacter))
            {
                Debug.LogError(string.Format("'CheckRarity'에서 제외됨 : {0}", otherCharacter.name));
                continue;
            }

            if (!CheckDefenceOver(skillData, otherCharacter))
            {
                Debug.LogError(string.Format("'CheckDefenceOver'에서 제외됨 : {0}", otherCharacter.name));
                continue;
            }

            if (!CheckObjectTeam(skillData, otherCharacter))
            {
                Debug.LogError(string.Format("'CheckObjectTeam'에서 제외됨 : {0}", otherCharacter.name));
                continue;
            }

            checkObjList.Add(obj);
        }

        return checkObjList;
    }

    public void ApplySkill(SkillData skillData, CharacterPhysics applyObj, bool isAllStop)
    {
        Debug.Log(string.Format("스킬 발동 {0} -> {1}\n{2}",
           transform.name, applyObj.characterTransform.name, skillData.desc));

        var applyCharacter = applyObj.character;
        switch (skillData.applyType)
        {
            case 1:
                {
                    var value = GetApplyValueType(skillData.applyValueType, applyCharacter.finalAttack, this);
                    applyCharacter.finalAttack = value * skillData.applyValue;
                    break;
                }
            case 2:
                {
                    var value = GetApplyValueType(skillData.applyValueType, applyCharacter.finalDefence, this);
                    applyCharacter.finalDefence = value * skillData.applyValue;
                    break;
                }
            case 3:
                {
                    var value = GetApplyValueType(skillData.applyValueType, applyCharacter.addImpulse, this);
                    var finalValue = value * skillData.applyValue;

                    if (isAllStop)
                    {
                        var dir = Vector3.Normalize(applyCharacter.transform.localPosition - this.transform.localPosition);
                        var speed = applyCharacter.Physics.GetQuadraticEquationValue(finalValue);

                        applyCharacter.Physics.ApplyForce(dir, speed);

                        Debug.Log(string.Format("준 충격량 : {0}\n방향 : {1} 속도 : {2}",
                            finalValue, dir, speed));
                    }
                    else
                    {
                        applyCharacter.addImpulse = applyCharacter.GetImpulseValue(finalValue);
                    }
                    break;
                }
            case 4:
                break;
            case 5:
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

    public void RefreshData(CharacterData data)
    {
        this.data = data;
        physics.RefreshData(data);
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
