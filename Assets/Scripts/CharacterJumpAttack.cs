using System.Collections;
using UnityEngine;

public class CharacterJumpAttack : MonoBehaviour
{
public Vector3 characterScale = Vector3.one;
    public bool applyScaleOnStart = true;
    
    public float jumpToEnemyDuration = 0.5f;
    public float jumpBackDuration = 0.5f;
    public float jumpHeight = 2f;
    public float attackDistance = 1.5f;
    public float attackPauseDuration = 0.2f;
    
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
    [Tooltip("High damage sound effect that plays with the ultimate attack charge-up (damage > 3)")]
    public AudioClip highDamageSoundEffect;
    [Tooltip("AudioSource to play sword sound (auto-created if not assigned)")]
    public AudioSource audioSource;
    
    [Header("Visual Effects")]
    [Tooltip("Blood splatter effect that plays on every attack hit")]
    public GameObject bloodSplatterEffect;
    [Tooltip("Special visual effect for attacks dealing more than 3 damage")]
    public GameObject highDamageEffect;
    [Tooltip("Offset for visual effects position")]
    public Vector3 visualEffectOffset = Vector3.zero;
    
    // Whether to return to original position after attack (works in both singleplayer and multiplayer)
    public bool returnToStartPositionAfterAttack = true;
    
    private Vector3 originalPosition;
    private bool isAnimating = false;
    private bool hasShownIdleWarning = false;

    void Start()
    {
        if (applyScaleOnStart)
   {
            transform.localScale = characterScale;
        }
        
        originalPosition = transform.position;
        
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
 Debug.LogWarning($"[CharacterJumpAttack] {gameObject.name}: No Animator found. Sprite animations will not play. Either add an Animator component or disable 'Use Sprite Animation'.");
            }
 }
        
        if (useSpriteAnimation && characterAnimator != null)
     {
        if (characterAnimator.runtimeAnimatorController == null)
     {
    Debug.LogWarning($"[CharacterJumpAttack] {gameObject.name}: Animator found but no Animator Controller assigned! Please assign an Animator Controller or disable 'Use Sprite Animation'.");
                useSpriteAnimation = false;
            }
            else
         {
      characterAnimator.speed = 1f;
           PlayIdleAnimation();
  }
        }
    }

    void Update()
    {
        if (!isAnimating && useSpriteAnimation && characterAnimator != null && characterAnimator.runtimeAnimatorController != null)
        {
      if (!string.IsNullOrEmpty(idleAnimationName))
      {
    try
       {
          var currentState = characterAnimator.GetCurrentAnimatorStateInfo(0);
          if (!currentState.IsName(idleAnimationName) && currentState.normalizedTime >= 1f)
     {
         PlayIdleAnimation();
      }
        }
    catch
        {
       }
  }
        }
    }

    public void SetCharacterAnimator(Animator newAnimator)
    {
        characterAnimator = newAnimator;
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
         
        try
    {
              characterAnimator.Play(idleAnimationName, 0, 0f);
            }
            catch (System.Exception)
        {
         if (useSpriteAnimation && !hasShownIdleWarning)
      {
     Debug.LogWarning($"[CharacterJumpAttack] {gameObject.name}: Idle state '{idleAnimationName}' not found in Animator Controller. Disabling sprite animations. Please add the state or uncheck 'Use Sprite Animation'.");
   hasShownIdleWarning = true;
           useSpriteAnimation = false;
       }
            }
}
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
     
        Vector3 startPosition = transform.position;
        Vector3 directionToTarget = (targetPosition - startPosition).normalized;
        Vector3 attackPosition = targetPosition - (directionToTarget * attackDistance);
        
        // Spawn ultimate attack effect (charge-up) at player position BEFORE jumping (only if damage > 3)
        if (damage > 3f && highDamageEffect != null)
        {
            GameObject chargeEffect = Instantiate(highDamageEffect, startPosition + visualEffectOffset, Quaternion.identity);
            
            // Play high damage sound effect
            PlayHighDamageSound();
            
            // Ensure animation/particle system doesn't loop
            Animator animator = chargeEffect.GetComponent<Animator>();
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
            ParticleSystem particles = chargeEffect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                // Ensure particle system doesn't loop
                var main = particles.main;
                main.loop = false;
                float duration = main.duration + main.startLifetime.constantMax;
                Destroy(chargeEffect, duration);
            }
            else if (animator != null)
            {
                // Get animation length from animator
                AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
                if (clips.Length > 0)
                {
                    float animationLength = clips[0].length;
                    Destroy(chargeEffect, animationLength);
                }
                else
                {
                    Destroy(chargeEffect, 2f);
                }
            }
            else
            {
                Destroy(chargeEffect, 2f);
            }
            
            // Wait a bit for the charge-up effect to be visible
            yield return new WaitForSeconds(1f);
        }
        
    yield return StartCoroutine(JumpToPosition(startPosition, attackPosition, jumpToEnemyDuration));
        
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
    
    // Spawn blood splatter effect at ENEMY position (target position) when correct answer
    Vector3 enemyHitPosition = targetPosition + visualEffectOffset;
    SpawnBloodEffect(enemyHitPosition);
    
    if (onAttackHit != null)
        {
  onAttackHit.Invoke();
        }
    
        if (attackEffectPrefab != null)
        {
     Vector3 effectPosition = enemyHitPosition + attackEffectOffset;
     GameObject effect = Instantiate(attackEffectPrefab, effectPosition, Quaternion.identity);
            Destroy(effect, 2f);
     }
        
     yield return new WaitForSeconds(attackPauseDuration);
        
        // Return to start position if enabled (works in both singleplayer and multiplayer)
        if (returnToStartPositionAfterAttack)
        {
            yield return StartCoroutine(JumpToPosition(transform.position, startPosition, jumpBackDuration));
            PlayIdleAnimation();
        }
        
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
     }
    }

    public void StopAnimation()
    {
  StopAllCoroutines();
      transform.position = originalPosition;
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
    /// Play high damage sound effect (for ultimate attacks)
    /// </summary>
    private void PlayHighDamageSound()
    {
        if (highDamageSoundEffect != null && audioSource != null)
        {
            float volume = 1f;
            if (SettingsManager.Instance != null)
            {
                volume = SettingsManager.GetEffectiveVolume(false, 1f);
            }
            audioSource.PlayOneShot(highDamageSoundEffect, volume);
        }
    }
    
    /// <summary>
    /// Spawn blood splatter effect at target position (enemy position for correct answers)
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

