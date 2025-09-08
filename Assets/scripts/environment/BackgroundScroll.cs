using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    [Header("Settings")]
    public float scrollSpeed = 2f;          // Downward units/sec
    public int poolSize = 3;                // 3 tiles total
    public bool alignToCameraOnStart = true;

    [Header("Debug")]
    [SerializeField] private bool isLeader = true; // clones become false

    private Transform[] tiles;              // the pool (leader + clones)
    private float tileHeight;               // world height of a tile
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Start()
    {
        // Clones do nothing (their script is disabled right after Instantiate)
        if (!isLeader) return;

        var sr = GetComponent<SpriteRenderer>();
        if (!sr) { Debug.LogError("[BackgroundScroll] Needs a SpriteRenderer."); enabled = false; return; }
        tileHeight = sr.bounds.size.y;

        // Build pool: this object + (poolSize-1) clones ABOVE it
        tiles = new Transform[poolSize];
        tiles[0] = transform;

        for (int i = 1; i < poolSize; i++)
        {
            Vector3 pos = transform.position + Vector3.up * (tileHeight * i);
            GameObject clone = Instantiate(gameObject, pos, Quaternion.identity);
            clone.transform.SetParent(transform.parent, true);

            // Mark clone as non-leader and disable its script to prevent spawning/updates
            var bs = clone.GetComponent<BackgroundScroll>();
            bs.isLeader = false;
            bs.alignToCameraOnStart = false;
            bs.enabled = false;

            tiles[i] = clone.transform;
        }

        // Optional: center middle tile on camera and place neighbors above/below
        if (alignToCameraOnStart && cam && cam.orthographic && tiles.Length >= 3)
        {
            float cy = cam.transform.position.y;
            float cx = tiles[0].position.x;
            float cz = tiles[0].position.z;

            tiles[0].position = new Vector3(cx, cy - tileHeight, cz); // bottom (green)
            tiles[1].position = new Vector3(cx, cy,            cz);   // middle (black, visible)
            tiles[2].position = new Vector3(cx, cy + tileHeight, cz); // top (red)
        }
    }

    void Update()
    {
        if (!isLeader || tiles == null) return;

        // 1) Move all tiles downward together
        float dy = scrollSpeed * Time.deltaTime;
        for (int i = 0; i < tiles.Length; i++)
            tiles[i].Translate(Vector3.down * dy);

        if (!(cam && cam.orthographic)) return;

        // 2) Compute camera bottom and highest top edge among tiles
        float camBottom = cam.transform.position.y - cam.orthographicSize;

        float highestTopEdge = float.NegativeInfinity;
        for (int i = 0; i < tiles.Length; i++)
        {
            float topEdge = tiles[i].position.y + tileHeight * 0.5f;
            if (topEdge > highestTopEdge) highestTopEdge = topEdge;
        }

        // 3) Any tile fully below the screen? Move it above the current highest
        for (int i = 0; i < tiles.Length; i++)
        {
            float topEdge = tiles[i].position.y + tileHeight * 0.5f;
            if (topEdge < camBottom) // fully off-screen below
            {
                float newCenterY = highestTopEdge + tileHeight * 0.5f;
                Vector3 p = tiles[i].position;
                tiles[i].position = new Vector3(p.x, newCenterY, p.z);

                highestTopEdge = newCenterY + tileHeight * 0.5f; // update for chains
            }
        }
    }
}
