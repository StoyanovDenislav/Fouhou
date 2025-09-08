using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BulletHell/Patterns/RotatingFlower")]
public class RotatingFlowerPattern : ObstaclePattern
{
    
    public float Petals {
        get
        {
            return petals;
        }
        set { petals = value; }
    }
    public float RotationSpeed {
        get
        {
            return rotationSpeed;
        }
        set { rotationSpeed = value; }
    }
    public float Speed {
        get
        {
            return speed;
        }
        set { speed = value; }
    }
    public float Damage {
        get
        {
            return damage;  
        }
        set { damage = value; }
    }

    public Sprite Sprite {
        get { return sprite; }
        set { sprite = value; }

    }

    public override void Fire(Transform origin, BulletPool pool, float time)
    {
        
        float baseAngle = RotationSpeed * time;

        for (int i = 0; i < Petals; i++)
        {
            float angle = baseAngle + (360f / Petals) * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            pool.Spawn(origin.position, dir, Speed, Damage, Sprite);
        }
    }
}