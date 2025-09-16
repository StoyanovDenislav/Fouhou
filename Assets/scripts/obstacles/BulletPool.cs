using UnityEngine;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public int poolSize = 50000;   // Preload 50k bullets

    private Queue<GameObject> pool = new();
    private List<GameObject> active = new(); // track active bullets

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
        if (pool.Count == 0) return null;

        GameObject b = pool.Dequeue();
        b.transform.position = pos;
        b.SetActive(true);

        var o = b.GetComponent<Obstacle>();
        o.Init(dir, speed, damage, sprite, this);

        active.Add(b); // track active
        return b;
    }

    public void Despawn(GameObject b)
    {
        b.SetActive(false);
        active.Remove(b);
        pool.Enqueue(b);
    }

    public List<GameObject> GetActive()
    {
        return active;
    }
}