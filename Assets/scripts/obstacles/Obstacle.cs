using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Vector2 direction;
    private BulletPool pool;
    public float Damage { get; set; }
    public float Speed { get; set; }

    
    public void Init(Vector2 dir, float spd, float damage, Sprite sprite, BulletPool poolRef)
    {
        direction = dir.normalized;
        pool = poolRef;
        Damage = damage;
        Speed = spd;
        gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
        
    }

    void Update()
    {
        transform.Translate(direction * Speed * Time.deltaTime, Space.World);

        if (Mathf.Abs(transform.position.x) > 10f || Mathf.Abs(transform.position.y) > 10f)
            pool.Despawn(gameObject); // ♻️ recycle instead of Destroy
    }
}