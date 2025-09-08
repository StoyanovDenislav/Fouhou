using UnityEngine;

public abstract class ObstaclePattern : ScriptableObject
{
    public float fireRate;
    public Sprite sprite;
    public float damage;
    public float speed;
    public float rotationSpeed;
    public float petals;// Seconds between shots

    public abstract void Fire(Transform origin, BulletPool pool, float time);
}
