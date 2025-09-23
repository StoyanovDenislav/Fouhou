using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BulletHell/Patterns/SineSpeedFlower")]
public class SineSpeedFlowerPattern : ObstaclePattern
{
    [SerializeField] private float speedAmplitude = 3f;  // +/- variation
    [SerializeField] private float waveFrequency = 1.5f; // Hz

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

    public float SpeedAmplitude
    {
        get { return speedAmplitude; }
        set { speedAmplitude = Mathf.Max(0f, value); }
    }
    public float WaveFrequency
    {
        get { return waveFrequency; }
        set { waveFrequency = Mathf.Max(0f, value); }
    }

    public override void Fire(Transform origin, BulletPool pool, float time)
    {
        float baseAngle = RotationSpeed * time;
        for (int i = 0; i < Petals; i++)
        {
            float angle = baseAngle + (360f / Petals) * i;
            float phase = (i / Mathf.Max(1f, Petals)) * Mathf.PI * 2f; // per-petal phase
            float dynSpeed = Speed + SpeedAmplitude * Mathf.Sin(phase + time * WaveFrequency * Mathf.PI * 2f);
            dynSpeed = Mathf.Max(0.1f, dynSpeed);

            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            pool.Spawn(origin.position, dir, dynSpeed, Damage, Sprite);
        }
    }
}