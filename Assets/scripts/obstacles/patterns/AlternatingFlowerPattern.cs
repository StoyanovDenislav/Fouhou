using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BulletHell/Patterns/AlternatingFlower")]
public class AlternatingFlowerPattern : ObstaclePattern
{
    [SerializeField] private float alternationRate = 1f; // toggles per second

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

    public float AlternationRate
    {
        get { return Mathf.Max(0f, alternationRate); }
        set { alternationRate = Mathf.Max(0f, value); }
    }

    public override void Fire(Transform origin, BulletPool pool, float time)
    {
        bool odd = (Mathf.FloorToInt(time * Mathf.Max(0.0001f, AlternationRate)) % 2) == 1;
        float spin = (odd ? -RotationSpeed : RotationSpeed) * time;

        for (int i = 0; i < Petals; i++)
        {
            float angle = spin + (360f / Petals) * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            pool.Spawn(origin.position, dir, Speed, Damage, Sprite);
        }
    }
}