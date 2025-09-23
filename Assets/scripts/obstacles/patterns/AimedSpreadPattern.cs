using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BulletHell/Patterns/AimedSpread")]
public class AimedSpreadPattern : ObstaclePattern
{
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private int bullets = 5;          // >= 1
    [SerializeField] private float totalSpread = 25f;  // degrees across full spread

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

    public string TargetTag
    {
        get { return targetTag; }
        set { targetTag = value; }
    }
    public int Bullets
    {
        get { return Mathf.Max(1, bullets); }
        set { bullets = Mathf.Max(1, value); }
    }
    public float TotalSpread
    {
        get { return Mathf.Max(0f, totalSpread); }
        set { totalSpread = Mathf.Max(0f, value); }
    }

    public override void Fire(Transform origin, BulletPool pool, float time)
    {
        int count = Bullets;

        // Aim at target (fallback to up)
        Vector2 baseDir = Vector2.up;
        var go = GameObject.FindGameObjectWithTag(TargetTag);
        if (go != null)
        {
            baseDir = ((Vector2)(go.transform.position - origin.position)).normalized;
        }

        if (count == 1)
        {
            pool.Spawn(origin.position, baseDir, Speed, Damage, Sprite);
            return;
        }

        float half = TotalSpread * 0.5f;
        float step = TotalSpread / (count - 1);
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < count; i++)
        {
            float offset = -half + step * i;
            float ang = baseAngle + offset;
            Vector2 dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
            pool.Spawn(origin.position, dir, Speed, Damage, Sprite);
        }
    }
}