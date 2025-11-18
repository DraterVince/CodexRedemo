using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Character Animation Controller - Manages Idle and Attack animations
/// Automatically switches between animation states
/// </summary>
public class CharacterAnimationController : MonoBehaviour
{
  [Header("Animation References")]
    [Tooltip("Sprite animator for idle animation")]
    public SpriteAnimator idleAnimator;
    
    [Tooltip("Sprite animator for attack animation")]
    public SpriteAnimator attackAnimator;
    
    [Header("Animation Sprites")]
    [Tooltip("Sprites for idle animation")]
    public List<Sprite> idleSprites = new List<Sprite>();
    
    [Tooltip("Sprites for attack animation")]
    public List<Sprite> attackSprites = new List<Sprite>();
    
    [Header("Settings")]
    [Tooltip("Idle animation frame rate")]
    [Range(1, 60)]
    public int idleFrameRate = 8;
    
    [Tooltip("Attack animation frame rate")]
    [Range(1, 60)]
    public int attackFrameRate = 24;
    
    [Tooltip("Auto-return to idle after attack")]
    public bool autoReturnToIdle = true;
    
    [Tooltip("Delay before returning to idle (if auto-return enabled)")]
    public float returnToIdleDelay = 0.5f;
    
    [Header("Debug")]
    [SerializeField]
    private AnimationState currentState = AnimationState.Idle;
    
[SerializeField]
    private bool isAttacking = false;
    
    private SpriteRenderer spriteRenderer;
    
    public enum AnimationState
    {
   Idle,
        Attack
    }
    
    private void Awake()
    {
      spriteRenderer = GetComponent<SpriteRenderer>();
        
      // Create animators if they don't exist
        SetupAnimators();
    }
    
    private void Start()
    {
        // Start with idle animation
    PlayIdle();
    }
    
    /// <summary>
    /// Setup animator components if not already assigned
    /// </summary>
    private void SetupAnimators()
    {
    // If no idle animator assigned, try to find or create one
        if (idleAnimator == null)
        {
        // Check if one already exists
            SpriteAnimator[] existingAnimators = GetComponents<SpriteAnimator>();
      if (existingAnimators.Length > 0)
            {
    idleAnimator = existingAnimators[0];
      }
            else
         {
 // Create new one
     idleAnimator = gameObject.AddComponent<SpriteAnimator>();
            }
        }
        
     // If no attack animator, create a second one
     if (attackAnimator == null)
{
        attackAnimator = gameObject.AddComponent<SpriteAnimator>();
        }
        
        // Configure idle animator
      idleAnimator.sprites = idleSprites;
        idleAnimator.frameRate = idleFrameRate;
        idleAnimator.loop = true;
        idleAnimator.pingPong = true; // Breathing effect
        idleAnimator.playOnStart = false; // We'll control it
     
        // Configure attack animator
        attackAnimator.sprites = attackSprites;
        attackAnimator.frameRate = attackFrameRate;
        attackAnimator.loop = false; // Play once
        attackAnimator.playOnStart = false;
        
        // Setup event for attack completion
if (attackAnimator.onAnimationComplete == null)
{
            attackAnimator.onAnimationComplete = new UnityEngine.Events.UnityEvent();
        }
        attackAnimator.onAnimationComplete.AddListener(OnAttackComplete);
        
        Debug.Log($"[CharacterAnimationController] Setup complete on {gameObject.name}");
    }
    
    /// <summary>
    /// Play idle animation
    /// </summary>
    public void PlayIdle()
    {
        if (currentState == AnimationState.Idle && idleAnimator.IsPlaying())
            return; // Already playing idle
        
     currentState = AnimationState.Idle;
        isAttacking = false;
      
        // Stop attack animation
      if (attackAnimator != null)
        {
  attackAnimator.Stop();
        }
      
        // Play idle animation
        if (idleAnimator != null)
        {
    idleAnimator.Play();
            Debug.Log($"[{gameObject.name}] Playing IDLE animation");
        }
    }
    
    /// <summary>
    /// Play attack animation
    /// </summary>
    public void PlayAttack()
    {
        if (isAttacking)
        {
        Debug.Log($"[{gameObject.name}] Already attacking!");
      return; // Already attacking
   }
        
        currentState = AnimationState.Attack;
        isAttacking = true;
        
      // Stop idle animation
        if (idleAnimator != null)
        {
   idleAnimator.Stop();
    }
        
   // Play attack animation
    if (attackAnimator != null)
      {
            attackAnimator.Play();
            Debug.Log($"[{gameObject.name}] Playing ATTACK animation");
}
    }
    
    /// <summary>
    /// Called when attack animation completes
    /// </summary>
    private void OnAttackComplete()
    {
        Debug.Log($"[{gameObject.name}] Attack animation complete");
   isAttacking = false;
        
        if (autoReturnToIdle)
        {
        if (returnToIdleDelay > 0)
            {
  Invoke("PlayIdle", returnToIdleDelay);
     }
            else
   {
        PlayIdle();
            }
        }
    }
    
    /// <summary>
    /// Get current animation state
    /// </summary>
    public AnimationState GetCurrentState()
  {
        return currentState;
    }
    
    /// <summary>
    /// Check if currently attacking
    /// </summary>
 public bool IsAttacking()
    {
        return isAttacking;
    }
    
    /// <summary>
    /// Update sprites at runtime
  /// </summary>
    public void SetIdleSprites(List<Sprite> sprites)
    {
        idleSprites = sprites;
        if (idleAnimator != null)
        {
          idleAnimator.sprites = sprites;
        }
    }
  
 /// <summary>
    /// Update attack sprites at runtime
    /// </summary>
    public void SetAttackSprites(List<Sprite> sprites)
    {
        attackSprites = sprites;
      if (attackAnimator != null)
      {
            attackAnimator.sprites = sprites;
        }
    }
    
    /// <summary>
    /// Change animation speeds
    /// </summary>
    public void SetIdleSpeed(int fps)
    {
        idleFrameRate = fps;
        if (idleAnimator != null)
        {
       idleAnimator.SetFrameRate(fps);
}
    }
    
    public void SetAttackSpeed(int fps)
    {
  attackFrameRate = fps;
        if (attackAnimator != null)
    {
         attackAnimator.SetFrameRate(fps);
 }
    }
    
    // Context menu options for testing in editor
  [ContextMenu("Test Idle Animation")]
    private void TestIdle()
    {
        PlayIdle();
    }
    
    [ContextMenu("Test Attack Animation")]
    private void TestAttack()
    {
        PlayAttack();
    }
}
