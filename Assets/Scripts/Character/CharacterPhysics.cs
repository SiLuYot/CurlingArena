using System;
using System.Collections.Generic;
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
    public int pid;
    public float speed;
    public Vector3 dir;
    private float friction;
    public float sweepValue;
    public float attackBouns;
    public float firstSpeed;
    public bool isFirstCollide;

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

    public CharacterData data;
    public CharacterPhysicsData pData;
    public Character character;
    public Transform characterTransform;

    public Action updateForce;
    public Action<CharacterPhysics> collideEvent;
    public Action<CharacterPhysics> beCollidedEvent;
    public Action<List<CharacterPhysics>> allStopEvent;

    public CharacterPhysics(CharacterData data, CharacterPhysicsData pData, Character character,
        Action<CharacterPhysics> collideEvent, Action<CharacterPhysics> beCollidedEvent, Action<List<CharacterPhysics>> allStopEvent)
    {
        this.pid = 0;
        this.speed = 0;
        this.dir = Vector3.zero;
        this.friction = 0.1f;
        this.sweepValue = 0;
        this.attackBouns = 1.0f;
        this.firstSpeed = 0;
        this.isFirstCollide = false;

        this.data = data;
        this.pData = pData;
        this.character = character;
        this.characterTransform = character.transform;
        this.collideEvent = collideEvent;
        this.beCollidedEvent = beCollidedEvent;
        this.allStopEvent = allStopEvent;

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

    public void ApplyForce(Vector3 dir, float force, float attackBouns = 1.0f)
    {        
        this.dir = dir;
        this.speed = force;
        this.firstSpeed = force;
        this.attackBouns = attackBouns;

        Debug.Log(string.Format("ApplyForce -> {0}\ndir : {1} speed : {2} ab : {3}",
            characterTransform.name, dir, speed, attackBouns));
    }

    public void ApplyDir(Vector3 dir)
    {
        this.dir += dir;
    }

    public void Sweep(float value)
    {
        sweepValue += Math.Abs(value);

        ApplyDir(new Vector3(0, 0, value));
    }

    //충격량을 속도로 변환
    //등차수열의 합과 이차방정식으로 속도를 알아낸다
    public float GetQuadraticEquationValue(float impulse)
    {
        var d = Friction;

        var a = (1 / d);
        var b = (d * a);
        var c = -(2 * impulse);

        var D = -b + Mathf.Sqrt((b * b) - (4 * a * c));
        var x = D / (2 * a);

        return x;
    }
}
