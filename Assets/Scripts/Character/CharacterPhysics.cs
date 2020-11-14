using System;
using UnityEngine;

public class CharacterPhysicsData
{
    public float mass = 0;
    public float radius = 0;

    public CharacterPhysicsData(float mass, float radius)
    {
        this.mass = mass;
        this.radius = radius;
    }
}

public class CharacterPhysics
{
    public int index;
    public float speed;
    public Vector3 dir;
    public float friction;
    public float sweepValue;
    public float attackBouns;
    public bool isFirstCollision;

    public float Mass => pData.mass;
    public float Radius => pData.radius;
    public float Friction
    {
        get
        {
            var vaule = friction - sweepValue;            
            return vaule <= 0 ? 0.001f : vaule;
        }
    }

    //public float DefenceValue => data.defence * 100 / (data.defence + 100);

    public CharacterData data;
    public CharacterPhysicsData pData;
    public Character character;
    public Transform characterTransform;
    public Action updateForce;
    public Action<CharacterPhysics, bool> collisionEvent;    

    public CharacterPhysics(CharacterData data, CharacterPhysicsData pData, Character character, 
        Action<CharacterPhysics, bool> collisionEvent)
    {
        this.index = 0;
        this.speed = 0;
        this.dir = Vector3.zero;
        this.friction = 0.1f;
        this.sweepValue = 0;
        this.attackBouns = 1.0f;
        this.isFirstCollision = false;

        this.data = data;
        this.pData = pData;
        this.character = character;
        this.characterTransform = character.transform;
        this.collisionEvent = collisionEvent;

        PhysicsManager.Instance.AddPhysicsObject(this);
    }

    public void RefreshData(CharacterData data)
    {
        this.data = data;
    }

    public void RemovePhysicsObject()
    {
        PhysicsManager.Instance.RemovePhysicsObject(this);
    }

    public void AddForce(Vector3 dir, float force)
    {
        this.dir = dir;
        this.speed = force;
    }

    public void AddDir(Vector3 dir)
    {
        this.dir += dir;
    }

    //public float GetImpulseValue(float attack)
    //{
    //    return attack * (100 - DefenceValue) / 100;
    //}

    public void SetAttackBonus(float bouns)
    {
        attackBouns = bouns;
    }

    public void Sweep(float value)
    {
        sweepValue += Math.Abs(value);

        AddDir(new Vector3(0, 0, value));
    }
}
