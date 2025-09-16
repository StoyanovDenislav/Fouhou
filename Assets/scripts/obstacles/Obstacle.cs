using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Vector2 direction;
    public float Speed { get; set; }
    public float Damage { get; set; }
    private SpriteRenderer sr;
    
    [HideInInspector] public BulletPool bulletPool;

    public void Init(Vector2 dir, float spd, float damage, Sprite sprite, BulletPool poolRef)
    {
        direction = dir.normalized;
        Speed = spd;
        bulletPool = poolRef;
        Damage = damage;
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
    }

    void Update()
    {
        transform.Translate(direction * Speed * Time.deltaTime, Space.Self);

        // Check if bullet is outside play area using boundary manager
        if (PlayAreaBounds.Instance != null && PlayAreaBounds.Instance.IsOutsideBounds(transform.position))
        {
            ScoreManager.Instance.AddBulletScore(1);
            bulletPool.Despawn(gameObject);
        }
        // Fallback to default bounds if no boundary manager exists
        else if (PlayAreaBounds.Instance == null)
        {
            if (Mathf.Abs(transform.position.x) > 8f || Mathf.Abs(transform.position.y) > 4.5f)
            {
                ScoreManager.Instance.AddBulletScore(1);
                bulletPool.Despawn(gameObject);
            }
        }
    }

    public void FreezeAndConvert()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddBulletScore(1);
        }
        else
        {
            Debug.LogWarning("ScoreManager is missing!");
        }

        bulletPool.Despawn(gameObject);
    }
}