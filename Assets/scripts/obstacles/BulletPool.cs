using UnityEngine;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public int poolSize = 50000;   // 🔥 Preload 50k bullets

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject b = Instantiate(obstaclePrefab, transform);
            b.SetActive(false);
            pool.Enqueue(b);
        }
    }

    public GameObject Spawn(Vector3 pos, Vector2 dir, float speed, float damage, Sprite sprite)
    {
        if (pool.Count == 0) return null; // all bullets are in use

        GameObject b = pool.Dequeue();
        b.transform.position = pos;
        b.SetActive(true);

        Obstacle o = b.GetComponent<Obstacle>();
        o.Init(dir, speed, damage, sprite, this);

        return b;
    }
    
    public void Despawn(GameObject b)
    {
        b.SetActive(false);
        pool.Enqueue(b);
    }
}