using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BulletHell/Patterns/RandomBurst")]
public class RandomBurstPattern : ObstaclePattern
{
    [SerializeField] private int bullets = 12;
    [SerializeField] private float minSpeed = 6f;
    [SerializeField] private float maxSpeed = 12f;

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

    public int Bullets
    {
        get { return Mathf.Max(1, bullets); }
        set { bullets = Mathf.Max(1, value); }
    }
    public float MinSpeed
    {
        get { return Mathf.Max(0f, minSpeed); }
        set { minSpeed = Mathf.Max(0f, value); }
    }
    public float MaxSpeed
    {
        get { return Mathf.Max(MinSpeed, maxSpeed); }
        set { maxSpeed = Mathf.Max(MinSpeed, value); }
    }

    public override void Fire(Transform origin, BulletPool pool, float time)
    {
        int count = Bullets;
        float lo = MinSpeed;
        float hi = Mathf.Max(lo, MaxSpeed);

        for (int i = 0; i < count; i++)
        {
            float angle = UnityEngine.Random.Range(0f, 360f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            float spd = UnityEngine.Random.Range(lo, hi);
            pool.Spawn(origin.position, dir, spd, Damage, Sprite);
        }
    }
}