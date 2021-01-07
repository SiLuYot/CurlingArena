using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterData data;
    public CharacterData Data { get => data; }

    private CharacterPhysics physics;
    public CharacterPhysics Physics { get => physics; }

    private CharacterSkill skill;
    public CharacterSkill Skill { get => skill; }

    private GameCharacterNameUI gameCharacterNameUI;

    private Team team;
    public Team Team { get => team; }

    public float synergyAtkValue;
    public float synergyDefValue;

    public float finalAttack;
    public float finalDefence;
    public float ImpulseAddValue;
    public float ImpulsePercentValue;
    public Vector3 ImpulseAddDir;
    public float immuneCount;
    public float lockCount;

    public void Init(CharacterData data, Team team)
    {
        this.data = data;

        var pData = new CharacterPhysicsData(GameManager.MASS, data.sizeData.GetCharacterRadius());

        this.physics = new CharacterPhysics(this ,pData,
            CollideEvent, BeCollidedEvent, AllStopEvent);

        this.skill = new CharacterSkill(this);

        this.team = team;

        this.synergyAtkValue = 0;
        this.synergyDefValue = 0;

        InitState();
        //InitEvent();
    }

    public void InitState()
    {
        finalAttack = data.attack + synergyAtkValue;
        finalDefence = data.defence + synergyDefValue;
        ImpulseAddValue = 0;
        ImpulsePercentValue = 1.0f;
        ImpulseAddDir = Vector3.zero;
        immuneCount = 0;
        lockCount = 0;
    }

    public void InitNameUI()
    {
        if (gameCharacterNameUI != null)
            return;

        gameCharacterNameUI = UIManager.Instance.Get<GameCharacterNameUI>(true) as GameCharacterNameUI;
        gameCharacterNameUI.Init(data, Physics.characterTransform);        
    }

    public void SetSynergyValue(float atk, float def, List<SkillData> skillDataList)
    {
        this.synergyAtkValue = finalAttack * atk;
        this.synergyDefValue = finalDefence * def;

        finalAttack = data.attack + this.synergyAtkValue;
        finalDefence = data.defence + this.synergyDefValue;

        Skill.AddSynergySkill(skillDataList);
    }

    public void InitEvent()
    {
        skill.InitEvent();
    }

    public void FirstShootEvent()
    {
        skill.FirstShootEvent();
    }

    public void CollideEvent(CharacterPhysics otherObj, List<CharacterPhysics> physicsObjectList)
    {
        skill.CollideEvent(otherObj, physicsObjectList);
    }

    public void BeCollidedEvent(CharacterPhysics otherObj, List<CharacterPhysics> physicsObjectList)
    {
        skill.BeCollidedEvent(otherObj, physicsObjectList);
    }

    public IEnumerator AllStopEvent(CharacterPhysics firstCollisionObj, List<CharacterPhysics> physicsObjectList)
    {        
        yield return PhysicsManager.Instance.StartPhysicsCoroutine(skill.AllStopEvent(firstCollisionObj, physicsObjectList));
    }

    public float GetDefenceValue()
    {
        return finalDefence * 100 / (finalDefence + 100);
    }

    public float GetImpulseValue(float attack)
    {
        return attack * (100 - GetDefenceValue()) / 100;
    }

    public float GetShootSpeed(float attackBouns = 1f)
    {
        //return Mathf.Sqrt(100f * ((0.6f * (Data.attack * attackBouns)) + 30f)) + 15f;
        return attackBouns * GameManager.SPEED;
    }

    public void RefreshData(CharacterData data)
    {
        this.data = data;
        physics.RefreshData(this);

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
        if (gameCharacterNameUI != null)
        {
            gameCharacterNameUI.Close();
        }

        physics = null;
    }
}
