using UnityEngine;

/// <summary>
/// Simple input handler for character animations
/// Press Space or Click to attack
/// </summary>
public class CharacterAnimationInput : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The character animation controller")]
    public CharacterAnimationController animController;
    
    [Header("Input Settings")]
    [Tooltip("Key to trigger attack")]
    public KeyCode attackKey = KeyCode.Space;
    
    [Tooltip("Enable mouse click to attack")]
    public bool enableMouseAttack = true;
    
    [Tooltip("Mouse button for attack (0=Left, 1=Right, 2=Middle)")]
    public int mouseButton = 0;
    
    [Header("Debug")]
    [SerializeField]
    private float lastAttackTime = 0f;
    
    [SerializeField]
    private int attackCount = 0;
    
    private void Start()
 {
        // Try to find controller if not assigned
        if (animController == null)
  {
    animController = GetComponent<CharacterAnimationController>();
     }
     
      if (animController == null)
      {
            Debug.LogError($"[CharacterAnimationInput] No CharacterAnimationController found on {gameObject.name}!");
        }
  }
    
    private void Update()
    {
        if (animController == null) return;
        
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
    }
    
    /// <summary>
    /// Trigger an attack (can be called from other scripts or UI buttons)
    /// </summary>
    public void TriggerAttack()
    {
        if (animController == null) return;
      
   // Don't attack if already attacking
        if (animController.IsAttacking())
        {
    Debug.Log($"[{gameObject.name}] Cannot attack - already attacking!");
    return;
        }
        
        lastAttackTime = Time.time;
        attackCount++;
        
      Debug.Log($"[{gameObject.name}] Triggering attack #{attackCount}");
        animController.PlayAttack();
    }
    
    /// <summary>
    /// Force return to idle (can be called from UI or other scripts)
    /// </summary>
    public void ForceIdle()
    {
      if (animController != null)
        {
    animController.PlayIdle();
        }
    }
}
