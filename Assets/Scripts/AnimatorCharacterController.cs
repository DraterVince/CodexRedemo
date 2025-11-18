using UnityEngine;

/// <summary>
/// Character controller using Unity Animator (matches your existing setup)
/// Triggers Attack parameter like your KnightAnimatorController
/// </summary>
[RequireComponent(typeof(Animator))]
public class AnimatorCharacterController : MonoBehaviour
{
    private Animator animator;
    
    [Header("Input Settings")]
    [Tooltip("Key to trigger attack")]
    public KeyCode attackKey = KeyCode.Space;

    [Tooltip("Enable mouse click to attack")]
    public bool enableMouseAttack = true;
    
    [Tooltip("Mouse button for attack (0=Left, 1=Right, 2=Middle)")]
    public int mouseButton = 0;
    
    [Header("Debug")]
    [SerializeField]
    private bool isAttacking = false;
    
    [SerializeField]
    private int attackCount = 0;
    
    private void Start()
    {
   animator = GetComponent<Animator>();
      
     if (animator == null)
    {
          Debug.LogError($"[AnimatorCharacterController] No Animator found on {gameObject.name}!");
        }
      
        if (animator.runtimeAnimatorController == null)
      {
            Debug.LogWarning($"[AnimatorCharacterController] No Animator Controller assigned to {gameObject.name}!");
 }
    }
    
    private void Update()
    {
        if (animator == null) return;
        
        // Check for attack input
    bool attackInput = false;
   
        // Keyboard input
      if (Input.GetKeyDown(attackKey))
        {
 attackInput = true;
            Debug.Log($"[{gameObject.name}] Attack key pressed: {attackKey}");
      }
        
        // Mouse input
        if (enableMouseAttack && Input.GetMouseButtonDown(mouseButton))
      {
      attackInput = true;
      Debug.Log($"[{gameObject.name}] Mouse button {mouseButton} clicked");
        }
        
   // Trigger attack
 if (attackInput)
        {
            TriggerAttack();
    }
    
        // Update attacking state from animator
        UpdateAttackingState();
    }
    
    /// <summary>
    /// Trigger an attack (same as your existing setup)
    /// </summary>
    public void TriggerAttack()
    {
      if (animator == null) return;
     
      attackCount++;
        Debug.Log($"[{gameObject.name}] Triggering attack #{attackCount}");
     
// Trigger the Attack parameter (same as your KnightAnimatorController)
        animator.SetTrigger("Attack");
    }
    
  /// <summary>
    /// Check current animator state
    /// </summary>
    private void UpdateAttackingState()
    {
    if (animator == null) return;
        
        // Check if in attack state
 AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    isAttacking = stateInfo.IsName("Attack");
    }
    
    /// <summary>
    /// Check if currently attacking
 /// </summary>
  public bool IsAttacking()
    {
        return isAttacking;
    }
    
    /// <summary>
    /// Get reference to animator
    /// </summary>
    public Animator GetAnimator()
    {
        return animator;
    }
    
    /// <summary>
    /// Set animator parameter (for advanced use)
    /// </summary>
    public void SetTrigger(string parameterName)
    {
        if (animator != null)
        {
    animator.SetTrigger(parameterName);
        }
 }
    
    public void SetBool(string parameterName, bool value)
    {
        if (animator != null)
        {
       animator.SetBool(parameterName, value);
        }
    }
    
    public void SetFloat(string parameterName, float value)
    {
        if (animator != null)
        {
      animator.SetFloat(parameterName, value);
        }
    }
    
    public void SetInteger(string parameterName, int value)
    {
        if (animator != null)
  {
    animator.SetInteger(parameterName, value);
        }
    }
    
    // Context menu for testing
    [ContextMenu("Test Attack")]
    private void TestAttack()
    {
        TriggerAttack();
    }
}
