using UnityEngine;

public class HeartMechanics : MonoBehaviour
{
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    private float timeToExpireShield;
    private bool hasShield;
    private SpriteRenderer sr;
    private float alpha;
    

    [Header("🔊 Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSound;
    [Range(0f, 1f)]
    [SerializeField] private float damageVolume = 0.7f;
    

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        
        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Configure audio source for UI/player sounds
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
    }

    private bool Shield(Collision2D collision)
    {
        GameObject obstacleGameObject = collision.gameObject;
        
        // Get the Obstacle component to access the BulletPool reference
        Obstacle obstacle = obstacleGameObject.GetComponent<Obstacle>();
        if (obstacle != null && obstacle.bulletPool != null)
        {
            // Use the pool's Despawn method instead of Destroy
            obstacle.bulletPool.Despawn(obstacleGameObject);
        }
        else
        {
            // Fallback to Destroy if no pool reference found
            Destroy(obstacleGameObject);
        }
        
        if (sr != null)
        {
            alpha = 0.5f;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
        
        timeToExpireShield = Time.time + .5f;
        
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
        
        if (obstacleScript == null)
        {
            Debug.LogWarning("No object found");
            return;
        }

        unsafe
        {
            // pin health in memory so GC won't move it
            fixed (float* pHealth = &health)
            {
                if (hasShield) 
                {
                    HeartMechanicsFunctions.TakeDamage(0, pHealth);
                }
                else
                {
                    float damageAmount = obstacleScript.Damage;
                    HeartMechanicsFunctions.TakeDamage(damageAmount, pHealth);
                    
                    // 💔 Play damage sound only when actually taking damage
                    if (damageAmount > 0)
                    {
                        PlayDamageSound();
                    }
                    
                    hasShield = Shield(bullet);
                }
            }
        }
    }

    private void PlayDamageSound()
    {
        if (damageSound != null && audioSource != null)
        {
            audioSource.volume = damageVolume;
            audioSource.pitch = Random.Range(0.9f, 1.1f); // Slight pitch variation
            audioSource.PlayOneShot(damageSound);
            Debug.Log("💔 Damage sound played!");
        }
    }

    // Public methods for external volume control
    public void SetDamageVolume(float volume)
    {
        damageVolume = Mathf.Clamp01(volume);
    }

    // Public getters for UI/debugging
    public float GetCurrentHealth() => health;
    public float GetMaxHealth() => maxHealth;
    public bool HasShield() => hasShield;
    public float GetShieldTimeRemaining() => Mathf.Max(0f, timeToExpireShield - Time.time);
}

static class HeartMechanicsFunctions
{
    public static unsafe void TakeDamage(float damage, float* currentHealthPtr)
    {
        *currentHealthPtr -= damage;

        ScoreManager.Instance.hitTimes++;
        
        if (*currentHealthPtr <= 0)
        {
            *currentHealthPtr = 0;
        }
    }
}