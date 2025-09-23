using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BulletHell/Patterns/SpiralStream")]
public class SpiralStreamPattern : ObstaclePattern
{
    [SerializeField] private int arms = 1;

    public float Petals
    {
        get { return petals; }
        set { petals = value; }
    }
    public float RotationSpeed
    {
        get { return rotationSpeed; }
        set { rotationSpeed = value; }
    }
    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }
    public float Damage
    {
        get { return damage; }
        set { damage = value; }
    }
    public Sprite Sprite
    {
        get { return sprite; }
        set { sprite = value; }
    }

    public int Arms
    {
        get { return Mathf.Max(1, arms); }
        set { arms = Mathf.Max(1, value); }
    }

    public override void Fire(Transform origin, BulletPool pool, float time)
    {
        float baseAngle = RotationSpeed * time;
        float step = 360f / Arms;

        for (int i = 0; i < Arms; i++)
        {
            float angle = baseAngle + step * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            pool.Spawn(origin.position, dir, Speed, Damage, Sprite);
        }
    }
}