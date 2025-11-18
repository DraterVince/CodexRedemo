using System.Collections;
using UnityEngine;

public class EnemyJumpAttack : MonoBehaviour
{
    public Vector3 characterScale = Vector3.one;
    public bool applyScaleOnStart = true;
    
    public float jumpToPlayerDuration = 0.4f;
    public float jumpBackDuration = 0.4f;
    public float jumpHeight = 1.5f;
public float attackDistance = 1.5f;
    public float attackPauseDuration = 0.15f;
    
    public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public Animator characterAnimator;
    public bool useSpriteAnimation = true;
  public string idleAnimationName = "Idle";
    public string attackAnimationTrigger = "Attack";
    public bool useAttackTrigger = true;
    public string attackAnimationState = "Attack";
    
  public GameObject attackEffectPrefab;
    public Vector3 attackEffectOffset = Vector3.zero;
    
    [Header("Sound Effects")]
    [Tooltip("Sword sound effect that plays when the attack hits")]
    public AudioClip swordSoundEffect;
    [Tooltip("AudioSource to play sword sound (auto-created if not assigned)")]
    public AudioSource audioSource;
    
    [Header("Visual Effects")]
    [Tooltip("Blood splatter effect that plays on every attack hit")]
    public GameObject bloodSplatterEffect;
    [Tooltip("Special visual effect for attacks dealing more than 3 damage")]
    public GameObject highDamageEffect;
    [Tooltip("Offset for visual effects position")]
    public Vector3 visualEffectOffset = Vector3.zero;
    
  private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isAnimating = false;
    private bool hasShownIdleWarning = false;
    private bool? idleStateExists = null; // null = not checked, true = exists, false = doesn't exist
    // isVerifyingIdleState is used in coroutine to prevent multiple verifications
    // Suppress warning - field is used but compiler may not detect it due to #if directives
    #pragma warning disable CS0414
    private bool isVerifyingIdleState = false;
    #pragma warning restore CS0414
    private float lastStateCheckTime = 0f;
    private const float STATE_CHECK_INTERVAL = 0.5f; // Only check state every 0.5 seconds to reduce allocations

    void Start()
    {
        if (applyScaleOnStart)
{
     transform.localScale = characterScale;
      }
        
        // Initialize original position and rotation - this will be updated before each attack
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D sound
            }
        }
        
        // Register audio source with SettingsManager for volume control
        if (audioSource != null && SettingsManager.Instance != null)
        {
            SettingsManager.RegisterSFXSource(audioSource);
        }
        
        if (characterAnimator == null && useSpriteAnimation)
        {
      characterAnimator = GetComponent<Animator>();
            
      if (characterAnimator == null)
        {
            characterAnimator = GetComponentInChildren<Animator>();
      }
        
  if (characterAnimator == null)
  {
       Debug.LogWarning($"[EnemyJumpAttack] {gameObject.name}: No Animator found. Sprite animations will not play. Either add an Animator component or disable 'Use Sprite Animation'.");
       }
  }
        
if (useSpriteAnimation && characterAnimator != null)
        {
            if (characterAnimator.runtimeAnimatorController == null)
       {
        Debug.LogWarning($"[EnemyJumpAttack] {gameObject.name}: Animator found but no Animator Controller assigned! Please assign an Animator Controller or disable 'Use Sprite Animation'.");
                useSpriteAnimation = false;
         }
  else
            {
 characterAnimator.speed = 1f;
   PlayIdleAnimation();
   }
        }
    }
    
    void OnEnable()
    {
        // Update original position and rotation when enemy is enabled (in case it was repositioned)
        // CRITICAL: Only update if not currently animating to prevent overwriting during attacks
        if (!isAnimating)
        {
            originalPosition = transform.position;
            originalRotation = transform.rotation;
        }
    }

    void Update()
    {
        if (!isAnimating && useSpriteAnimation && characterAnimator != null && characterAnimator.runtimeAnimatorController != null)
        {
            if (!string.IsNullOrEmpty(idleAnimationName))
            {
                // Only try to play idle if we know the state exists or haven't checked yet
                if (idleStateExists != false)
                {
                    // Throttle state checks to reduce allocations - only check every 0.5 seconds
                    float currentTime = Time.time;
                    if (currentTime - lastStateCheckTime >= STATE_CHECK_INTERVAL)
                    {
                        lastStateCheckTime = currentTime;
                        
                        try
                        {
                            var currentState = characterAnimator.GetCurrentAnimatorStateInfo(0);
                            // Check if we're not in the idle state and the current animation has finished
                            if (!currentState.IsName(idleAnimationName) && currentState.normalizedTime >= 1f)
                            {
                                PlayIdleAnimation();
                            }
                        }
                        catch
                        {
                            // If we can't get state info, mark state as non-existent
                            if (idleStateExists == null)
                            {
                                idleStateExists = false;
                            }
                        }
                    }
                }
            }
        }
    }
    
    private System.Collections.IEnumerator VerifyIdleStateAfterPlay()
    {
        yield return null; // Wait one frame
        
        if (characterAnimator != null && !string.IsNullOrEmpty(idleAnimationName))
        {
            try
            {
                var currentState = characterAnimator.GetCurrentAnimatorStateInfo(0);
                // If we tried to play idle but we're not in the idle state, it doesn't exist
                if (!currentState.IsName(idleAnimationName))
                {
                    idleStateExists = false;
                    if (!hasShownIdleWarning)
                    {
                        Debug.LogWarning($"[EnemyJumpAttack] {gameObject.name}: Idle state '{idleAnimationName}' not found in Animator Controller. Disabling sprite animations.");
                        hasShownIdleWarning = true;
                        useSpriteAnimation = false;
                    }
                }
                else
                {
                    // State exists and we're in it
                    idleStateExists = true;
                }
            }
            catch
            {
                // Can't verify, assume it doesn't exist to be safe
                if (idleStateExists == null)
                {
                    idleStateExists = false;
                }
            }
        }
    }

    public void SetCharacterAnimator(Animator newAnimator)
    {
        characterAnimator = newAnimator;
        // Reset state existence check when animator changes
        idleStateExists = null;
        if (characterAnimator != null)
        {
            characterAnimator.speed = 1f;
        }
        PlayIdleAnimation();
    }
    
    public void SetCharacterScale(Vector3 scale)
    {
        characterScale = scale;
        transform.localScale = characterScale;
    }
    
    public void SetCharacterScale(float uniformScale)
    {
        SetCharacterScale(new Vector3(uniformScale, uniformScale, uniformScale));
    }
    
    public Vector3 GetCharacterScale()
    {
        return characterScale;
    }
    
    public void PlayIdleAnimation()
    {
        if (useSpriteAnimation && characterAnimator != null && !string.IsNullOrEmpty(idleAnimationName))
        {
            if (characterAnimator.runtimeAnimatorController == null)
            {
                return;
            }
            
            // Check if we've already determined the state doesn't exist
            if (idleStateExists == false)
            {
                // State doesn't exist, don't try to play it
                return;
            }
            
            // Check if the state exists (cache the result)
            if (idleStateExists == null)
            {
                idleStateExists = HasAnimatorState(characterAnimator, idleAnimationName);
            }
            
            if (idleStateExists == true)
            {
                // State exists (verified in editor) or we're assuming it exists (runtime)
                // At runtime, we'll verify after playing via coroutine
                characterAnimator.Play(idleAnimationName, 0, 0f);
                
                // At runtime, if we haven't verified yet, start verification coroutine (only once)
                #if !UNITY_EDITOR
                if (!isVerifyingIdleState)
                {
                    isVerifyingIdleState = true;
                    StartCoroutine(VerifyIdleStateAfterPlay());
                }
                #endif
            }
            else
            {
                // State doesn't exist - disable sprite animation to prevent repeated warnings
                if (!hasShownIdleWarning)
                {
                    Debug.LogWarning($"[EnemyJumpAttack] {gameObject.name}: Idle state '{idleAnimationName}' not found in Animator Controller. Disabling sprite animations. Please add the state or uncheck 'Use Sprite Animation'.");
                    hasShownIdleWarning = true;
                    useSpriteAnimation = false;
                }
            }
        }
    }
    
    private bool HasAnimatorState(Animator animator, string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return false;
        }
        
        // In editor, we can check the AnimatorController directly
        #if UNITY_EDITOR
        UnityEditor.Animations.AnimatorController controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
        if (controller != null)
        {
            // Check all layers for the state
            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    if (state.state.name == stateName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endif
        
        // At runtime, we can't check without playing the state
        // Return true to allow attempting to play - we'll verify via coroutine
        // If it doesn't exist, we'll mark it as false and never try again
        return true;
    }
    
    public void PlayAttackAnimation()
    {
        if (useSpriteAnimation && characterAnimator != null)
        {
   if (characterAnimator.runtimeAnimatorController == null)
       {
        return;
            }
       
  if (useAttackTrigger && !string.IsNullOrEmpty(attackAnimationTrigger))
   {
              if (HasParameter(characterAnimator, attackAnimationTrigger))
           {
   characterAnimator.Update(0f);
        characterAnimator.SetTrigger(attackAnimationTrigger);
             characterAnimator.Update(0f);
      }
          }
    else if (!string.IsNullOrEmpty(attackAnimationState))
   {
     try
                {
      characterAnimator.Play(attackAnimationState, 0, 0f);
         characterAnimator.Update(0f);
     }
           catch (System.Exception)
     {
    }
       }
        }
    }
    
    public void PerformJumpAttack(Vector3 targetPosition, System.Action onAttackHit = null, float damage = 1f)
    {
        if (!isAnimating)
        {
         StartCoroutine(JumpAttackCoroutine(targetPosition, onAttackHit, damage));
        }
    }

    public void PerformJumpAttack(Transform target, System.Action onAttackHit = null, float damage = 1f)
    {
      if (target != null)
        {
   PerformJumpAttack(target.position, onAttackHit, damage);
        }
    }

    private IEnumerator JumpAttackCoroutine(Vector3 targetPosition, System.Action onAttackHit, float damage)
    {
        isAnimating = true;
 
        // CRITICAL: Store original position and rotation BEFORE any movement
        // This must be the enemy's current position when attack starts
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        
        // Update stored original position for return journey
        originalPosition = startPosition;
        originalRotation = startRotation;
        
        // Calculate direction from enemy to player
        Vector3 directionToTarget = (targetPosition - startPosition);
        
        // Check if distance is valid (avoid division by zero)
        float distanceToTarget = directionToTarget.magnitude;
        if (distanceToTarget < 0.01f)
        {
            Debug.LogWarning($"[EnemyJumpAttack] {gameObject.name}: Target position is too close to enemy position! Enemy: {startPosition}, Target: {targetPosition}");
            // Just play attack animation in place
            if (useSpriteAnimation && characterAnimator != null && characterAnimator.runtimeAnimatorController != null)
            {
                if (useAttackTrigger && !string.IsNullOrEmpty(attackAnimationTrigger))
                {
                    if (HasParameter(characterAnimator, attackAnimationTrigger))
                    {
                        characterAnimator.Update(0f);
                        characterAnimator.ResetTrigger(attackAnimationTrigger);
                        characterAnimator.SetTrigger(attackAnimationTrigger);
                        characterAnimator.Update(0f);
                        characterAnimator.Update(0.001f);
                    }
                }
            }
            
            yield return new WaitForSeconds(0.05f);
            if (onAttackHit != null) { onAttackHit.Invoke(); }
            yield return new WaitForSeconds(attackPauseDuration);
            isAnimating = false;
            yield break;
        }
        
        directionToTarget = directionToTarget.normalized;
        
        // CRITICAL: Calculate attack position - enemy should jump TOWARDS player
        // The attack position should be closer to the player than the enemy's start position
        // Use a fixed percentage (e.g., 90%) of the distance to ensure forward movement
        Vector3 attackPosition;
        
        // Calculate how far to jump (90% of distance to player, ensuring we get close but don't overlap)
        // This guarantees forward movement and prevents backwards jumps
        float jumpPercentage = 0.9f; // Jump 90% of the way to the player
        float jumpDistance = distanceToTarget * jumpPercentage;
        
        // CRITICAL: Ensure we're moving FORWARD by verifying direction
        // Calculate attack position: start position + (normalized direction * jump distance)
        attackPosition = startPosition + (directionToTarget * jumpDistance);
        
        // Verify attack position is actually closer to target than start position
        float distanceFromStartToTarget = Vector3.Distance(startPosition, targetPosition);
        float distanceFromAttackToTarget = Vector3.Distance(attackPosition, targetPosition);
        
        // Safety check: If attack position is NOT closer to target than start, something is wrong
        if (distanceFromAttackToTarget >= distanceFromStartToTarget)
        {
            // This should never happen with the calculation above, but if it does, recalculate
            Debug.LogError($"[EnemyJumpAttack] {gameObject.name}: Attack position calculation failed! Attack distance ({distanceFromAttackToTarget:F2}) >= Start distance ({distanceFromStartToTarget:F2}). Recalculating...");
            // Force a smaller jump (70% of distance)
            jumpDistance = distanceToTarget * 0.7f;
            attackPosition = startPosition + (directionToTarget * jumpDistance);
            distanceFromAttackToTarget = Vector3.Distance(attackPosition, targetPosition);
        }
        
        // Additional safety: Ensure direction is valid (should point towards target)
        Vector3 calculatedDirection = (attackPosition - startPosition).normalized;
        float dotProduct = Vector3.Dot(calculatedDirection, directionToTarget);
        
        // Dot product should be close to 1.0 if directions match (within tolerance)
        if (dotProduct < 0.9f)
        {
            Debug.LogWarning($"[EnemyJumpAttack] {gameObject.name}: Direction mismatch! Dot product: {dotProduct:F2}. Recalculating...");
            // Recalculate with simpler approach
            jumpDistance = distanceToTarget * 0.8f;
            attackPosition = startPosition + (directionToTarget * jumpDistance);
        }
        
        // Make enemy face the player before jumping - use sprite flipping for 2D sprites
        // For 2D sprites, flip horizontally instead of rotating to avoid upside-down sprites
        // CRITICAL: Always reset rotation to originalRotation to prevent unwanted rotations
        transform.rotation = originalRotation;
        
        // CRITICAL: Don't flip sprites or change rotation - keep same behavior as singleplayer
        // This ensures enemy attack pattern matches singleplayer exactly
        // Just keep original rotation to prevent any unwanted rotations or flips
        
        yield return StartCoroutine(JumpToPosition(startPosition, attackPosition, jumpToPlayerDuration));
        
        if (useSpriteAnimation && characterAnimator != null && characterAnimator.runtimeAnimatorController != null)
        {
    if (useAttackTrigger && !string.IsNullOrEmpty(attackAnimationTrigger))
      {
                if (HasParameter(characterAnimator, attackAnimationTrigger))
          {
              characterAnimator.Update(0f);
       characterAnimator.ResetTrigger(attackAnimationTrigger);
             characterAnimator.SetTrigger(attackAnimationTrigger);
                characterAnimator.Update(0f);
 characterAnimator.Update(0.001f);
           }
       }
  else if (!string.IsNullOrEmpty(attackAnimationState))
            {
    characterAnimator.Play(attackAnimationState, 0, 0f);
    characterAnimator.Update(0f);
         }
 
            yield return null;
  }
     
        yield return new WaitForSeconds(0.05f);
        
        // Small delay before sword sound effect
        yield return new WaitForSeconds(0.2f);
        
        // Play sword sound effect when attack hits
        PlaySwordSound();
        
        // Spawn blood splatter effect at PLAYER position (target position) when wrong answer
        Vector3 playerHitPosition = targetPosition + visualEffectOffset;
        SpawnBloodEffect(playerHitPosition);
        
   if (onAttackHit != null)
      {
            onAttackHit.Invoke();
        }
        
     if (attackEffectPrefab != null)
        {
            Vector3 effectPosition = playerHitPosition + attackEffectOffset;
      GameObject effect = Instantiate(attackEffectPrefab, effectPosition, Quaternion.identity);
            Destroy(effect, 2f);
        }
   
     yield return new WaitForSeconds(attackPauseDuration);
     
      // CRITICAL FIX: Return to originalPosition, not startPosition, to prevent jumping backwards
      yield return StartCoroutine(JumpToPosition(transform.position, originalPosition, jumpBackDuration));
        
        // CRITICAL: Reset rotation to originalRotation after returning to prevent upside-down or rotated sprites
        transform.rotation = originalRotation;
        
        PlayIdleAnimation();
  
        isAnimating = false;
    }

    private IEnumerator JumpToPosition(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
     elapsed += Time.deltaTime;
  float t = Mathf.Clamp01(elapsed / duration);
            
         float curvedT = jumpCurve.Evaluate(t);
            
    Vector3 currentPos = Vector3.Lerp(from, to, curvedT);
            
            float heightOffset = jumpHeight * Mathf.Sin(t * Mathf.PI);
    currentPos.y += heightOffset;
            
    transform.position = currentPos;
    
        yield return null;
        }
        
        transform.position = to;
    }

    public bool IsAnimating()
  {
    return isAnimating;
    }

    public void UpdateOriginalPosition()
    {
  if (!isAnimating)
  {
 originalPosition = transform.position;
            originalRotation = transform.rotation;
        }
        else
        {
            // Cannot update original position while animating
        }
    }

    public void StopAnimation()
    {
        StopAllCoroutines();
     transform.position = originalPosition;
        transform.rotation = originalRotation;
        isAnimating = false;
        PlayIdleAnimation();
    }
    
    private bool HasParameter(Animator animator, string paramName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return false;
        
      foreach (AnimatorControllerParameter param in animator.parameters)
    {
        if (param.name == paramName)
              return true;
        }
        return false;
    }
    
    /// <summary>
    /// Play sword sound effect
    /// </summary>
    private void PlaySwordSound()
    {
        if (swordSoundEffect != null && audioSource != null)
        {
            float volume = 1f;
            if (SettingsManager.Instance != null)
            {
                volume = SettingsManager.GetEffectiveVolume(false, 1f);
            }
            audioSource.PlayOneShot(swordSoundEffect, volume);
        }
    }
    
    /// <summary>
    /// Spawn blood splatter effect at target position (player position for wrong answers)
    /// </summary>
    private void SpawnBloodEffect(Vector3 position)
    {
        if (bloodSplatterEffect != null)
        {
            GameObject bloodEffect = Instantiate(bloodSplatterEffect, position, Quaternion.identity);
            
            // Ensure animation doesn't loop
            Animator animator = bloodEffect.GetComponent<Animator>();
            if (animator != null)
            {
                // Disable looping on all animation clips
                AnimatorControllerParameter[] parameters = animator.parameters;
                foreach (AnimatorControllerParameter param in parameters)
                {
                    if (param.type == AnimatorControllerParameterType.Bool)
                    {
                        animator.SetBool(param.name, false);
                    }
                }
            }
            
            // Auto-destroy after animation/particle system finishes
            ParticleSystem particles = bloodEffect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                // Ensure particle system doesn't loop
                var main = particles.main;
                main.loop = false;
                float duration = main.duration + main.startLifetime.constantMax;
                Destroy(bloodEffect, duration);
            }
            else if (animator != null)
            {
                // Get animation length from animator
                AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
                if (clips.Length > 0)
                {
                    float animationLength = clips[0].length;
                    Destroy(bloodEffect, animationLength);
                }
                else
                {
                    Destroy(bloodEffect, 2f);
                }
            }
            else
            {
                Destroy(bloodEffect, 2f);
            }
        }
    }
}
