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

    public float Mass { get => data.mass; }
    public float Radius { get => data.radius; }
    public float Friction
    {
        get
        {
            var vaule = friction - sweepValue;            
            return vaule <= 0 ? 0.01f : vaule;
        }
    }

    public CharacterPhysicsData data;
    public Transform characterTransform;
    public Action UpdateForce;

    public CharacterPhysics(CharacterPhysicsData data, Transform trans)
    {
        this.index = 0;
        this.speed = 0;
        this.dir = Vector3.zero;
        this.friction = 0.1f;
        this.sweepValue = 0;

        this.data = data;
        this.characterTransform = trans;

        PhysicsManager.Instance.AddPhysicsObject(this);
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

    public void Sweep(float value)
    {
        sweepValue += Math.Abs(value);

        AddDir(new Vector3(0, 0, value));
    }
}
