 using UnityEngine;

public class HeartMechanics : MonoBehaviour
{
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    private float timeToExpireShield;
    private bool hasShield ;
    private SpriteRenderer sr;
    private float alpha;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private bool Shield(Collision2D collision)
    {
        GameObject obstacleGameObject = collision.gameObject;
        
        Destroy(obstacleGameObject);
        
        if (sr != null)
        {
            alpha = 0.5f;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
        
        timeToExpireShield = Time.time + 2f;
        
        return true;
    }

    private void Update()
    {
        if (Time.time >= timeToExpireShield)
        { 
            hasShield = false;
            alpha = 1f;
            
            if (sr != null)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D bullet)
    {
       Obstacle obstacleScript = bullet.gameObject.GetComponent<Obstacle>();
        
        if (obstacleScript ==null)
        {
            Debug.LogWarning("No object found");
            return;
        }

        unsafe
        {
            // pin health in memory so GC won’t move it
            fixed (float* pHealth = &health)
            {
                if (hasShield) HeartMechanicsFunctions.TakeDamage(0, pHealth);
                else
                {
                    HeartMechanicsFunctions.TakeDamage(obstacleScript.Damage, pHealth);
                    hasShield = Shield(bullet);
                }

                
            }
        }
    }
    
    
}

static class HeartMechanicsFunctions
{
    public static unsafe void TakeDamage(float damage, float* currentHealthPtr)
    {
        *currentHealthPtr -= damage;
        
        if (*currentHealthPtr <= 0)
        {
            *currentHealthPtr = 0;
        }
                    
    }
}
