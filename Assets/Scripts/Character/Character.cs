using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterData data;
    public CharacterData Data { get => data; }

    public float finalAttack;
    public float finalDefence;
    public float finalDefenceValue;

    private CharacterPhysics physics;
    public CharacterPhysics Physics { get => physics; }

    public int team;

    public void Init(CharacterData data, int team)
    {
        this.data = data;
        this.team = team;        
        this.physics = new CharacterPhysics(data, new CharacterPhysicsData(1, 1f), this, CollisionEvent);

        InitFinalState();

        EnableEvent();
    }

    public void InitFinalState()
    {        
        finalAttack = data.attack;
        finalDefence = data.defence;
        finalDefenceValue = finalDefence * 100 / (finalDefence + 100);
    }

    public float GetImpulseValue(float attack)
    {
        return attack * (100 - finalDefenceValue) / 100;
    }

    public void EnableEvent()
    {
        var skill1 = Data.skillData1;
        if (skill1.id != 0)
        {
            if (skill1.skillTrigger == 0) // 조건없이 바로 발동 (패시브)
            {

            }
        }

        var skill2 = Data.skillData2;
        if (skill2.id != 0)
        {
            if (skill2.skillTrigger == 0) // 조건없이 바로 발동 (패시브)
            {

            }
        }
    }

    //충돌 후 발생하는 스킬 체크
    //isCollide 내가 충돌 당했는지?
    public void CollisionEvent(CharacterPhysics checkObj, bool isCollide)
    {
        var skill1 = Data.skillData1;
        if (skill1.id != 0)
        {
            if (IsCollisionSkillActivate(skill1.skillTrigger, checkObj.character))
            {
                if (IsSkillCondition1Activate(skill1.condition1, isCollide))
                {
                    if (skill1.skillTriggerObject == 0)
                    {
                        //본인
                    }
                    else if (skill1.skillTriggerObject == 1)
                    {
                        //충돌한 대상                        
                        if (skill1.skillTarget == 6)
                        {
                            //방어력
                            if (skill1.changingType == 2)
                            {
                                if (skill1.figureType == 2)
                                {
                                    checkObj.character.finalDefence *= skill1.figureValue;
                                    if (skill1.keep == 0)
                                    {
                                        //계산후 초기화 필요
                                    }
                                }
                            }
                        }
                    }
                    else if (skill1.skillTriggerObject == 2)
                    {
                        //플레이어
                    }
                }
            }
        }

        var skill2 = Data.skillData2;
        if (skill2.id != 0)
        {
            if (IsCollisionSkillActivate(skill2.skillTrigger, checkObj.character))
            {

            }
        }
    }

    public bool IsCollisionSkillActivate(int skillTrigger, Character checkObj)
    {
        switch (skillTrigger)
        {
            //case 0:
            //    return true;
            //case 1:
            //    return true;
            case 2:
                return (team == checkObj.team);
            case 3:
                return (team != checkObj.team);
            case 4:
                return true;
            default:
                return false;
        }
    }

    public bool IsSkillCondition1Activate(int skillCondition, bool isCollide)
    {
        switch (skillCondition)
        {
            case 0:
                return true;
            case 1:
                return !isCollide;
            case 2:
                return !isCollide && Physics.isFirstCollision;
            case 3:
                return isCollide;
            case 4:
                return true;
            default:
                return false;
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
