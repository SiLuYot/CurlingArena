using System;
using System.Collections;
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
    private int pid;
    private float attackBouns;
    private float firstSpeed;
    private float friction;

    public float speed;
    public Vector3 dir;
    public float sweepValue;

    public bool isInActive;

    public int PID { get => pid; }
    public float AttackBouns => attackBouns;
    public float FirstSpeed => firstSpeed;
    public float Mass => pData.mass;
    public float Radius => pData.radius;
    public float Friction
    {
        get
        {
            return friction - sweepValue;
        }
    }

    public Character character;
    public CharacterPhysicsData pData;

    public Transform characterTransform;
    public Vector3 prevPostion;

    public Action updateForce;
    public Action<CharacterPhysics, List<CharacterPhysics>> collideEvent;
    public Action<CharacterPhysics, List<CharacterPhysics>> beCollidedEvent;
    public Func<CharacterPhysics, List<CharacterPhysics>, IEnumerator> allStopEvent;

    public CharacterPhysics(Character character, CharacterPhysicsData pData,
        Action<CharacterPhysics, List<CharacterPhysics>> collideEvent,
        Action<CharacterPhysics, List<CharacterPhysics>> beCollidedEvent,
        Func<CharacterPhysics, List<CharacterPhysics>, IEnumerator> allStopEvent)
    {
        this.pid = 0;
        this.speed = 0;
        this.dir = Vector3.zero;
        this.friction = GameManager.BASE_FRICTION;
        this.sweepValue = 0;
        this.attackBouns = 1.0f;
        this.firstSpeed = 0;
        this.isInActive = false;

        this.character = character;
        this.pData = pData;

        this.characterTransform = character.transform;
        this.prevPostion = character.transform.localPosition;

        this.collideEvent = collideEvent;
        this.beCollidedEvent = beCollidedEvent;
        this.allStopEvent = allStopEvent;

        PhysicsManager.Instance.AddPhysicsObject(this);
    }

    public void RecoverySweepFriction(float deltaTime)
    {
        if (sweepValue <= 0)
            return;

        var tempFriction = friction - sweepValue;
        tempFriction = tempFriction <= 0 ? GameManager.SWEEP_MAX : tempFriction;

        var tempSweepValue = sweepValue - GameManager.FRICTION * deltaTime;
        if (tempSweepValue <= 0)
        {
            tempSweepValue = 0;
        }

        //Debug.Log(string.Format("마찰력 회복 값 : {0}\n{1} -> {2}",
        //    GameManager.FRICTION * deltaTime,
        //    tempFriction, 
        //    friction - tempSweepValue));

        sweepValue = tempSweepValue;
    }

    public void SetPID(int pid)
    {
        this.pid = pid;
    }

    public void RefreshData(Character character)
    {
        this.character = character;
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

        Debug.Log(string.Format("ApplyForce -> {0}\ndir : {1} speed : {2} attackBouns : {3}",
            characterTransform.name, dir, speed, attackBouns));
    }

    public void ApplyDir(Vector3 dir)
    {
        this.dir += dir;
        this.dir.Normalize();
    }

    public void Sweep(float value)
    {
        var tempSweepValue = sweepValue + value;
        var tempFriction = friction - tempSweepValue;

        if (tempFriction <= GameManager.SWEEP_MAX)
        {
            tempSweepValue = friction - GameManager.SWEEP_MAX;
            tempFriction = friction - tempSweepValue;
        }

        Debug.Log(string.Format("현재 마찰력 : {0}\n= {1} - {2} (기본 마찰력 - 스윕으로 감소시킬 마찰력)",
            tempFriction, 
            friction, 
            tempSweepValue));

        sweepValue = tempSweepValue;
    }

    //충격량을 속도로 변환
    //등차수열의 합과 이차방정식으로 속도를 알아낸다
    public float GetQuadraticEquationValue(float impulse)
    {
        impulse = (impulse * GameManager.DISTACNE) * 3;

        var d = Friction;

        var a = (1 / d);
        var b = (d * a);
        var c = -(2 * impulse);

        var D = -b + Mathf.Sqrt((b * b) - (4 * a * c));
        var x = D / (2 * a);

        return x;
    }
}
