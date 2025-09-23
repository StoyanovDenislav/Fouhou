using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BulletHell/Patterns/ArcSpray")]
public class ArcSprayPattern : ObstaclePattern
{
    [SerializeField] private int bulletsPerShot = 9;
    [SerializeField] private float arcAngle = 120f; // total arc width

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

    public int BulletsPerShot
    {
        get { return Mathf.Max(1, bulletsPerShot); }
        set { bulletsPerShot = Mathf.Max(1, value); }
    }
    public float ArcAngle
    {
        get { return Mathf.Max(0f, arcAngle); }
        set { arcAngle = Mathf.Max(0f, value); }
    }

    public override void Fire(Transform origin, BulletPool pool, float time)
    {
        int count = BulletsPerShot;
        float centerAngle = RotationSpeed * time;

        if (count == 1)
        {
            Vector2 dirSingle = new Vector2(Mathf.Cos(centerAngle * Mathf.Deg2Rad), Mathf.Sin(centerAngle * Mathf.Deg2Rad));
            pool.Spawn(origin.position, dirSingle, Speed, Damage, Sprite);
            return;
        }

        float step = ArcAngle / (count - 1);
        float start = centerAngle - ArcAngle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angle = start + step * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            pool.Spawn(origin.position, dir, Speed, Damage, Sprite);
        }
    }
}