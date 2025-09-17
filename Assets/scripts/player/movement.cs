using UnityEngine;

public class movement : MonoBehaviour
{
    private GameObject player;
    private Transform playerTransform;
    public float speed;
    
    [Header("Boundary Settings")]
    public bool usePlayAreaBounds = true;
    
    // Fallback bounds if PlayAreaBounds is not available
    [Header("Fallback Bounds (if PlayAreaBounds not found)")]
    public float leftBound = -8f;
    public float rightBound = 8f;
    public float topBound = 4f;
    public float bottomBound = -4f;
    
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerTransform = player.GetComponent<Transform>();
    }

    void Update()
    {
        Vector3 currentPosition = playerTransform.position;
        Vector3 newPosition = currentPosition;
        
        // Handle input and calculate new position
        if (Input.GetKey(KeyCode.D))
        {
            newPosition += speed * Vector3.right * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            newPosition += Vector3.left * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.W))
        {
            newPosition += Vector3.up * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            newPosition += Vector3.down * speed * Time.deltaTime;
        }
        
        // Clamp position to bounds
        if (usePlayAreaBounds)
        {
            newPosition = ClampToPlayArea(newPosition);
        }
        
        // Apply the clamped position
        playerTransform.position = newPosition;
    }
    
    private Vector3 ClampToPlayArea(Vector3 position)
    {
        if (PlayAreaBounds.Instance != null)
        {
            // Use PlayAreaBounds if available
            position.x = Mathf.Clamp(position.x, PlayAreaBounds.Instance.leftBound, PlayAreaBounds.Instance.rightBound);
            position.y = Mathf.Clamp(position.y, PlayAreaBounds.Instance.bottomBound, PlayAreaBounds.Instance.topBound);
        }
        else
        {
            // Use fallback bounds
            position.x = Mathf.Clamp(position.x, leftBound, rightBound);
            position.y = Mathf.Clamp(position.y, bottomBound, topBound);
            
            Debug.LogWarning("PlayAreaBounds not found! Using fallback bounds.");
        }
        
        return position;
    }
}