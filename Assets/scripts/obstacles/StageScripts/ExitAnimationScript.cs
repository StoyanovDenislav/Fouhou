using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "BulletHell/StageScripts/ExitAnimation")]
public class ExitAnimationScript : StageScript
{
    [Header("Player Exit")]
    public bool animatePlayer = false; // Usually we don't move player out
    public string playerTag = "Player";
    public Vector3 playerExitOffset = new Vector3(0, -10, 0); // Where to move player relative to current position
    public float playerAnimationDuration = 2f;
    public AnimationCurve playerEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Enemy Exit")]
    public bool animateEnemy = true;
    public string enemyTag = "Enemy";
    public Vector3 enemyExitOffset = new Vector3(0, 10, 0); // Where to move enemy relative to current position
    public float enemyAnimationDuration = 2f;
    public AnimationCurve enemyEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Timing")]
    public float delayBetweenAnimations = 0.2f;
    public bool playerFirst = false; // Usually enemy exits first
    
    [Header("Cleanup")]
    public bool destroyAfterAnimation = false; // Usually keep objects for next stage
    
    public override IEnumerator Execute(StageSpawner stageSpawner, int stageIndex)
    {
        Debug.Log($"🎭 Starting exit animation for stage {stageIndex + 1}");
        
        // Find player and enemy
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        GameObject enemy = GameObject.FindGameObjectWithTag(enemyTag);
        
        if (playerFirst)
        {
            // Animate player first (rare)
            if (animatePlayer && player != null)
            {
                yield return AnimateExit(player, playerExitOffset, playerAnimationDuration, playerEaseCurve, "Player");
                yield return new WaitForSeconds(delayBetweenAnimations);
            }
            
            // Then animate enemy
            if (animateEnemy && enemy != null)
            {
                yield return AnimateExit(enemy, enemyExitOffset, enemyAnimationDuration, enemyEaseCurve, "Enemy");
            }
        }
        else
        {
            // Animate enemy first (typical)
            if (animateEnemy && enemy != null)
            {
                yield return AnimateExit(enemy, enemyExitOffset, enemyAnimationDuration, enemyEaseCurve, "Enemy");
                yield return new WaitForSeconds(delayBetweenAnimations);
            }
            
            // Then animate player (if needed)
            if (animatePlayer && player != null)
            {
                yield return AnimateExit(player, playerExitOffset, playerAnimationDuration, playerEaseCurve, "Player");
            }
        }
        
        // Cleanup if requested
        if (destroyAfterAnimation)
        {
            if (animatePlayer && player != null)
            {
                Destroy(player);
                Debug.Log("🗑️ Player destroyed after exit animation");
            }
            if (animateEnemy && enemy != null)
            {
                Destroy(enemy);
                Debug.Log("🗑️ Enemy destroyed after exit animation");
            }
        }
        
        Debug.Log($"✅ Exit animation complete for stage {stageIndex + 1}");
    }
    
    private IEnumerator AnimateExit(GameObject target, Vector3 exitOffset, float duration, AnimationCurve easeCurve, string objectName)
    {
        if (target == null) yield break;
        
        Vector3 startPosition = target.transform.position;
        Vector3 endPosition = startPosition + exitOffset;
        
        Debug.Log($"🚪 Starting {objectName} exit animation from {startPosition} to {endPosition}");
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // Apply easing curve
            float easedT = easeCurve.Evaluate(t);
            
            // Interpolate position
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, easedT);
            target.transform.position = currentPosition;
            
            yield return null;
        }
        
        // Ensure final position is exact
        target.transform.position = endPosition;
        Debug.Log($"👋 {objectName} exit complete at {endPosition}");
    }
}