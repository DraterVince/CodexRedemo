using UnityEngine;

// NOTE: Requires MultiAnimationController.cs and SpriteAnimator.cs
// If you get compile errors, reimport those files in Unity Editor first

/// <summary>
/// Input handler for Multi-Animation Controller
/// Supports keyboard and mouse input to trigger different animations
/// </summary>
[RequireComponent(typeof(MultiAnimationController))]
public class MultiAnimationInput : MonoBehaviour
{
 [Header("References")]
    public MultiAnimationController animController;
    
    [Header("Key Bindings")]
    [Tooltip("Key to play idle animation")]
    public KeyCode idleKey = KeyCode.Alpha1;
    
    [Tooltip("Key to play attack animation")]
    public KeyCode attackKey = KeyCode.Space;

    [Tooltip("Key to play walk animation")]
    public KeyCode walkKey = KeyCode.W;
    
    [Tooltip("Key to play run animation")]
    public KeyCode runKey = KeyCode.LeftShift;
    
    [Tooltip("Key to play jump animation")]
    public KeyCode jumpKey = KeyCode.Space;
    
    [Tooltip("Key to play dodge animation")]
    public KeyCode dodgeKey = KeyCode.LeftControl;
    
    [Header("Mouse Input")]
    [Tooltip("Enable left-click to trigger attack")]
    public bool enableMouseAttack = true;
    
    [Tooltip("Right-click animation")]
    public string rightClickAnimation = "block";
    
    [Header("Movement Detection")]
    [Tooltip("Auto-play walk when moving")]
    public bool autoWalkOnMovement = false;
    
    [Tooltip("Movement threshold to trigger walk")]
    public float movementThreshold = 0.1f;
    
 [Header("Animation Overrides")]
    [Tooltip("Custom animation key bindings")]
    public AnimationKeyBinding[] customBindings = new AnimationKeyBinding[0];
    
    [System.Serializable]
    public class AnimationKeyBinding
    {
        public KeyCode key;
  public string animationName;
    }
    
    [Header("Debug")]
    [SerializeField]
    private bool showDebugInfo = true;
    
    private Vector3 lastPosition;
    
    private void Awake()
    {
        if (animController == null)
        {
         animController = GetComponent<MultiAnimationController>();
        }
        
        lastPosition = transform.position;
    }
    
    private void Update()
    {
        if (animController == null) return;
        
        // Handle keyboard input
    HandleKeyboardInput();
        
        // Handle mouse input
        if (enableMouseAttack)
        {
   HandleMouseInput();
        }
        
        // Handle movement detection
        if (autoWalkOnMovement)
  {
       HandleMovementDetection();
   }
        
        // Handle custom bindings
     HandleCustomBindings();
    }
    
    private void HandleKeyboardInput()
    {
        // Check each predefined key
        if (Input.GetKeyDown(idleKey) && animController.HasAnimation("idle"))
        {
   animController.PlayAnimation("idle");
         DebugLog("Triggered: Idle");
 }
        
   if (Input.GetKeyDown(attackKey) && animController.HasAnimation("attack"))
        {
 animController.PlayAnimation("attack");
      DebugLog("Triggered: Attack");
        }
   
        if (Input.GetKeyDown(walkKey) && animController.HasAnimation("walk"))
     {
            animController.PlayAnimation("walk");
          DebugLog("Triggered: Walk");
        }
        
        if (Input.GetKeyDown(runKey) && animController.HasAnimation("run"))
        {
     animController.PlayAnimation("run");
       DebugLog("Triggered: Run");
        }
        
        if (Input.GetKeyDown(jumpKey) && animController.HasAnimation("jump"))
        {
            animController.PlayAnimation("jump");
            DebugLog("Triggered: Jump");
   }
  
        if (Input.GetKeyDown(dodgeKey) && animController.HasAnimation("dodge"))
        {
            animController.PlayAnimation("dodge");
      DebugLog("Triggered: Dodge");
      }
        
     // Number keys for quick access
     if (Input.GetKeyDown(KeyCode.Alpha2) && animController.HasAnimation("attack"))
        {
            animController.PlayAnimation("attack");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3) && animController.HasAnimation("skill"))
        {
   animController.PlayAnimation("skill");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4) && animController.HasAnimation("cast"))
 {
    animController.PlayAnimation("cast");
        }
    }
    
    private void HandleMouseInput()
    {
      // Left click - attack
if (Input.GetMouseButtonDown(0) && animController.HasAnimation("attack"))
        {
   animController.PlayAnimation("attack");
       DebugLog("Mouse Attack");
  }
        
  // Right click - custom animation
        if (Input.GetMouseButtonDown(1) && !string.IsNullOrEmpty(rightClickAnimation))
        {
     if (animController.HasAnimation(rightClickAnimation))
  {
              animController.PlayAnimation(rightClickAnimation);
       DebugLog($"Right Click: {rightClickAnimation}");
            }
        }
        
        // Middle click - special
        if (Input.GetMouseButtonDown(2) && animController.HasAnimation("special"))
    {
            animController.PlayAnimation("special");
            DebugLog("Special Attack");
        }
    }
    
    private void HandleMovementDetection()
    {
        float distance = Vector3.Distance(transform.position, lastPosition);
        
        if (distance > movementThreshold)
        {
// Check if running (shift held)
         if (Input.GetKey(runKey) && animController.HasAnimation("run"))
            {
   if (animController.GetCurrentAnimation() != "run")
   {
             animController.PlayAnimation("run");
     }
        }
     // Otherwise walk
       else if (animController.HasAnimation("walk"))
            {
          if (animController.GetCurrentAnimation() != "walk")
       {
        animController.PlayAnimation("walk");
        }
            }
        }
        else
        {
            // Not moving - idle
            if (animController.GetCurrentAnimation() == "walk" || 
    animController.GetCurrentAnimation() == "run")
        {
     if (animController.HasAnimation("idle"))
        {
      animController.PlayAnimation("idle");
     }
    }
        }
    
        lastPosition = transform.position;
    }
    
    private void HandleCustomBindings()
    {
foreach (var binding in customBindings)
        {
  if (Input.GetKeyDown(binding.key) && animController.HasAnimation(binding.animationName))
   {
    animController.PlayAnimation(binding.animationName);
       DebugLog($"Custom: {binding.animationName}");
            }
        }
    }
    
    private void DebugLog(string message)
    {
     if (showDebugInfo)
        {
   Debug.Log($"[MultiAnimationInput] {message}");
        }
    }
    
    // Public methods for external triggering
    public void TriggerAnimation(string animationName)
 {
   if (animController != null && animController.HasAnimation(animationName))
        {
        animController.PlayAnimation(animationName);
        }
    }
    
    // Context menu helpers
    [ContextMenu("List Key Bindings")]
 private void ListKeyBindings()
    {
        Debug.Log("=== Animation Key Bindings ===");
        Debug.Log($"Idle: {idleKey}");
        Debug.Log($"Attack: {attackKey}");
        Debug.Log($"Walk: {walkKey}");
        Debug.Log($"Run: {runKey}");
        Debug.Log($"Jump: {jumpKey}");
        Debug.Log($"Dodge: {dodgeKey}");
    
        if (customBindings.Length > 0)
        {
    Debug.Log("Custom Bindings:");
       foreach (var binding in customBindings)
            {
   Debug.Log($"  {binding.animationName}: {binding.key}");
            }
        }
    }
    
    [ContextMenu("List Available Animations")]
    private void ListAvailableAnimations()
    {
        if (animController != null)
        {
            var anims = animController.GetAvailableAnimations();
    Debug.Log($"=== Available Animations ({anims.Count}) ===");
          foreach (var anim in anims)
    {
                Debug.Log($"  • {anim}");
            }
        }
    }
}
