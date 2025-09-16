using UnityEngine;

public class PlayAreaBounds : MonoBehaviour
{
    [Header("Play Area Boundaries")]
    public float leftBound = -8f;
    public float rightBound = 8f;
    public float topBound = 4.5f;
    public float bottomBound = -4.5f;
    
    [Header("Visual Debug")]
    public bool showBounds = true;
    public Color boundsColor = Color.cyan;
    
    private static PlayAreaBounds instance;
    public static PlayAreaBounds Instance => instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public bool IsOutsideBounds(Vector3 position)
    {
        return position.x < leftBound || position.x > rightBound || 
               position.y > topBound || position.y < bottomBound;
    }
    
    void OnDrawGizmos()
    {
        if (showBounds)
        {
            Gizmos.color = boundsColor;
            
            // Draw the play area rectangle
            Vector3 topLeft = new Vector3(leftBound, topBound, 0);
            Vector3 topRight = new Vector3(rightBound, topBound, 0);
            Vector3 bottomLeft = new Vector3(leftBound, bottomBound, 0);
            Vector3 bottomRight = new Vector3(rightBound, bottomBound, 0);
            
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }
    }
}