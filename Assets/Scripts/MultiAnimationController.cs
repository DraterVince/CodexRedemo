using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// NOTE: Requires SpriteAnimator.cs to be compiled first
// If you get compile errors, reimport SpriteAnimator.cs in Unity Editor

/// <summary>
/// Multi-Animation Controller - Manages multiple animation states dynamically
/// Automatically detects and plays any named animation (idle, attack, walk, run, jump, etc.)
/// </summary>
public class MultiAnimationController : MonoBehaviour
{
    [System.Serializable]
    public class AnimationData
    {
  public string animationName;
   public List<Sprite> sprites = new List<Sprite>();
      public int frameRate = 12;
        public bool loop = true;
      public bool pingPong = false;
        [HideInInspector]
        public SpriteAnimator animator;
    }
    
[Header("Animation Library")]
    [Tooltip("All available animations")]
    public List<AnimationData> animations = new List<AnimationData>();
  
    [Header("Settings")]
    [Tooltip("Default animation to play on start")]
    public string defaultAnimation = "idle";
  
    [Tooltip("Auto-return to default after non-looping animation")]
    public bool autoReturnToDefault = true;
    
    [Tooltip("Delay before returning to default")]
    public float returnDelay = 0.1f;
    
    [Header("Debug")]
    [SerializeField]
    private string currentAnimationName = "";
    
    [SerializeField]
    private bool isPlaying = false;
    
    private SpriteRenderer spriteRenderer;
    private Dictionary<string, AnimationData> animationDict = new Dictionary<string, AnimationData>();
    private AnimationData currentAnimation;
    
    // Common animation names that auto-detection looks for
    private static readonly string[] COMMON_ANIMATIONS = new string[]
    {
   "idle", "attack", "walk", "run", "jump", "fall", "death", "die",
        "hurt", "hit", "damage", "dodge", "roll", "block", "cast", "spell",
  "shoot", "reload", "climb", "crouch", "slide", "dash", "skill"
    };
    
    private void Awake()
    {
   spriteRenderer = GetComponent<SpriteRenderer>();
        SetupAnimators();
        BuildAnimationDictionary();
    }
    
    private void Start()
    {
        PlayAnimation(defaultAnimation);
    }
  
    /// <summary>
    /// Setup sprite animators for each animation
    /// </summary>
    private void SetupAnimators()
    {
    foreach (var animData in animations)
      {
      if (animData.sprites.Count == 0)
            {
              Debug.LogWarning($"[MultiAnimationController] Animation '{animData.animationName}' has no sprites!");
       continue;
          }
            
            // Create a sprite animator for this animation
   SpriteAnimator animator = gameObject.AddComponent<SpriteAnimator>();
  animator.sprites = animData.sprites;
        animator.frameRate = animData.frameRate;
            animator.loop = animData.loop;
            animator.pingPong = animData.pingPong;
            animator.playOnStart = false;
      
  // Setup completion callback
            if (animator.onAnimationComplete == null)
 {
          animator.onAnimationComplete = new UnityEngine.Events.UnityEvent();
            }
  animator.onAnimationComplete.AddListener(() => OnAnimationComplete(animData.animationName));
      
       animData.animator = animator;
    }
        
        Debug.Log($"[MultiAnimationController] Setup {animations.Count} animations on {gameObject.name}");
    }
    
    /// <summary>
/// Build dictionary for fast lookup
    /// </summary>
    private void BuildAnimationDictionary()
    {
        animationDict.Clear();
        
        foreach (var animData in animations)
        {
     string key = animData.animationName.ToLower();
            if (!animationDict.ContainsKey(key))
    {
         animationDict[key] = animData;
            }
  else
   {
      Debug.LogWarning($"[MultiAnimationController] Duplicate animation name: {animData.animationName}");
            }
        }
    }
    
    /// <summary>
    /// Play animation by name
    /// </summary>
    public void PlayAnimation(string animationName)
    {
      if (string.IsNullOrEmpty(animationName))
        {
         Debug.LogWarning("[MultiAnimationController] Animation name is empty!");
  return;
        }
 
        string key = animationName.ToLower();
        
        if (!animationDict.ContainsKey(key))
        {
            Debug.LogWarning($"[MultiAnimationController] Animation '{animationName}' not found! Available: {string.Join(", ", animationDict.Keys)}");
            return;
        }
        
        AnimationData animData = animationDict[key];
        
        // Don't restart if already playing
        if (currentAnimation == animData && isPlaying)
      {
   return;
        }
        
        // Stop current animation
  if (currentAnimation != null && currentAnimation.animator != null)
    {
          currentAnimation.animator.Stop();
        }
      
        // Play new animation
      currentAnimation = animData;
        currentAnimationName = animData.animationName;
        isPlaying = true;
        
        if (animData.animator != null)
        {
 animData.animator.Play();
     Debug.Log($"[{gameObject.name}] Playing animation: {animationName}");
        }
    }
    
    /// <summary>
    /// Play animation with custom settings
    /// </summary>
    public void PlayAnimation(string animationName, int customFPS)
    {
    PlayAnimation(animationName);
        
  if (currentAnimation != null && currentAnimation.animator != null)
        {
     currentAnimation.animator.SetFrameRate(customFPS);
        }
    }

    /// <summary>
    /// Stop current animation
    /// </summary>
    public void StopCurrentAnimation()
    {
    if (currentAnimation != null && currentAnimation.animator != null)
      {
    currentAnimation.animator.Stop();
   }
        
        isPlaying = false;
    }
    
    /// <summary>
  /// Called when any animation completes
    /// </summary>
    private void OnAnimationComplete(string animationName)
    {
        Debug.Log($"[{gameObject.name}] Animation complete: {animationName}");
        isPlaying = false;
   
        if (autoReturnToDefault && animationName.ToLower() != defaultAnimation.ToLower())
      {
            if (returnDelay > 0)
        {
                Invoke("ReturnToDefault", returnDelay);
            }
     else
            {
         ReturnToDefault();
            }
        }
    }
    
    private void ReturnToDefault()
    {
  PlayAnimation(defaultAnimation);
    }
    
 /// <summary>
    /// Check if animation exists
    /// </summary>
 public bool HasAnimation(string animationName)
    {
        return animationDict.ContainsKey(animationName.ToLower());
    }
    
    /// <summary>
    /// Get all available animation names
    /// </summary>
    public List<string> GetAvailableAnimations()
    {
        return animations.Select(a => a.animationName).ToList();
    }
    
    /// <summary>
    /// Get current animation name
    /// </summary>
    public string GetCurrentAnimation()
    {
      return currentAnimationName;
    }
    
    /// <summary>
    /// Check if currently playing
    /// </summary>
    public bool IsPlaying()
    {
   return isPlaying;
    }
    
    /// <summary>
    /// Add animation at runtime
    /// </summary>
    public void AddAnimation(string name, List<Sprite> sprites, int fps = 12, bool loop = true, bool pingPong = false)
    {
      AnimationData newAnim = new AnimationData
        {
        animationName = name,
            sprites = sprites,
            frameRate = fps,
        loop = loop,
     pingPong = pingPong
        };
        
        animations.Add(newAnim);
        
        // Setup animator for new animation
        SpriteAnimator animator = gameObject.AddComponent<SpriteAnimator>();
        animator.sprites = sprites;
        animator.frameRate = fps;
        animator.loop = loop;
 animator.pingPong = pingPong;
        animator.playOnStart = false;
        
        if (animator.onAnimationComplete == null)
   {
            animator.onAnimationComplete = new UnityEngine.Events.UnityEvent();
  }
        animator.onAnimationComplete.AddListener(() => OnAnimationComplete(name));
   
        newAnim.animator = animator;
        animationDict[name.ToLower()] = newAnim;

      Debug.Log($"[MultiAnimationController] Added animation: {name}");
    }
    
    /// <summary>
  /// Get common animation names for auto-detection
 /// </summary>
  public static string[] GetCommonAnimationNames()
    {
      return COMMON_ANIMATIONS;
    }
  
    // Context menu helpers
    [ContextMenu("List All Animations")]
    private void ListAllAnimations()
    {
        Debug.Log($"[{gameObject.name}] Available Animations:");
      foreach (var anim in animations)
  {
         Debug.Log($"  - {anim.animationName}: {anim.sprites.Count} frames @ {anim.frameRate}fps (Loop: {anim.loop})");
        }
    }
    
    [ContextMenu("Play Default Animation")]
    private void TestDefaultAnimation()
    {
   PlayAnimation(defaultAnimation);
    }
}
