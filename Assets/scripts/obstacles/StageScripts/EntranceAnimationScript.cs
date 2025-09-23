using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "BulletHell/StageScripts/EntranceAnimation")]
public class EntranceAnimationScript : StageScript
{  
    [Header("Player Entrance")]
    public bool animatePlayer = true;
    public string playerTag = "Player";
    public Vector3 playerStartOffset = new Vector3(0, -10, 0); // Start position relative to final position
    public Vector3 playerFinalPosition = new Vector3(0, -4, 0); // Where player should end up
    public float playerAnimationDuration = 2f;
    public AnimationCurve playerEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Enemy Entrance")]
    public bool animateEnemy = true;
    public string enemyTag = "Enemy";
    public Vector3 enemyStartOffset = new Vector3(0, 10, 0); // Start position relative to final position
    public Vector3 enemyFinalPosition = new Vector3(0, 4, 0); // Where enemy should end up
    public float enemyAnimationDuration = 2f;
    public AnimationCurve enemyEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Timing")]
    public float delayBetweenAnimations = 0.5f; // Delay between enemy and player animations
    public bool enemyFirst = true; // If true, enemy animates first, then player
    
    [Header("Fallback")]
    public GameObject playerPrefab; // Fallback if no player found in scene
    public GameObject enemyPrefab; // Fallback if no enemy found in scene
    
    public override IEnumerator Execute(StageSpawner stageSpawner, int stageIndex)
    {
        Debug.Log($"🎬 Starting entrance animation for stage {stageIndex + 1}");
        
        // Find or create player and enemy
        GameObject player = FindOrCreateGameObject(playerTag, playerPrefab, "Player");
        GameObject enemy = FindOrCreateGameObject(enemyTag, enemyPrefab, "Enemy");
        
        if (enemyFirst)
        {
            // Animate enemy first
            if (animateEnemy && enemy != null)
            {
                yield return AnimateEntrance(enemy, enemyStartOffset, enemyFinalPosition, enemyAnimationDuration, enemyEaseCurve, "Enemy");
                yield return new WaitForSeconds(delayBetweenAnimations);
            }
            
            // Then animate player
            if (animatePlayer && player != null)
            {
                yield return AnimateEntrance(player, playerStartOffset, playerFinalPosition, playerAnimationDuration, playerEaseCurve, "Player");
            }
        }
        else
        {
            // Animate player first
            if (animatePlayer && player != null)
            {
                yield return AnimateEntrance(player, playerStartOffset, playerFinalPosition, playerAnimationDuration, playerEaseCurve, "Player");
                yield return new WaitForSeconds(delayBetweenAnimations);
            }
            
            // Then animate enemy
            if (animateEnemy && enemy != null)
            {
                yield return AnimateEntrance(enemy, enemyStartOffset, enemyFinalPosition, enemyAnimationDuration, enemyEaseCurve, "Enemy");
            }
        }
        
        Debug.Log($"✅ Entrance animation complete for stage {stageIndex + 1}");
    }
    
    private GameObject FindOrCreateGameObject(string tag, GameObject prefab, string fallbackName)
    {
        // Try to find existing object by tag
        GameObject obj = GameObject.FindGameObjectWithTag(tag);
        
        if (obj == null && prefab != null)
        {
            // Create from prefab if no existing object found
            obj = Instantiate(prefab);
            obj.tag = tag;
            Debug.Log($"📦 Created {fallbackName} from prefab: {prefab.name}");
        }
        
        if (obj == null)
        {
            Debug.LogWarning($"⚠️ No {fallbackName} found and no prefab assigned for entrance animation");
        }
        
        return obj;
    }
    
    private IEnumerator AnimateEntrance(GameObject target, Vector3 startOffset, Vector3 finalPosition, float duration, AnimationCurve easeCurve, string objectName)
    {
        if (target == null) yield break;
        
        Vector3 startPosition = finalPosition + startOffset;
        
        // Set starting position
        target.transform.position = startPosition;
        Debug.Log($"🎭 Starting {objectName} entrance animation from {startPosition} to {finalPosition}");
        
        // Animate to final position
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // Apply easing curve
            float easedT = easeCurve.Evaluate(t);
            
            // Interpolate position
            Vector3 currentPosition = Vector3.Lerp(startPosition, finalPosition, easedT);
            target.transform.position = currentPosition;
            
            yield return null;
        }
        
        // Ensure final position is exact
        target.transform.position = finalPosition;
        Debug.Log($"✨ {objectName} entrance complete at {finalPosition}");
    }
}