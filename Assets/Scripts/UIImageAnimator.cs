using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Force recompile - type resolution fix v2
// Last updated: fixing LevelSelectUISpriteSpawner compilation issues

/// <summary>
/// UI Image Sprite Animator - Animates UI Images through multiple sprites
/// Perfect for UI animations, icons, and effects
/// </summary>
[RequireComponent(typeof(Image))]
public class UIImageAnimator : MonoBehaviour
{
    [Header("Animation Sprites")]
    [Tooltip("List of sprites to animate through")]
    public List<Sprite> sprites = new List<Sprite>();
    
    [Header("Animation Settings")]
    [Tooltip("Frames per second")]
    [Range(1, 60)]
    public int frameRate = 12;
 
    [Tooltip("Play animation on start")]
 public bool playOnStart = true;
    
    [Tooltip("Loop the animation")]
    public bool loop = true;
    
    [Tooltip("Play animation in reverse after completing")]
    public bool pingPong = false;
    
    [Tooltip("Delay before starting animation (seconds)")]
 public float startDelay = 0f;
    
    [Header("Playback Control")]
    [Tooltip("Current frame index")]
    [SerializeField]
    private int currentFrame = 0;
    
    [Tooltip("Is animation playing?")]
    [SerializeField]
 private bool isPlaying = false;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent onAnimationComplete;
    public UnityEngine.Events.UnityEvent onLoopComplete;
 
    // Private fields
    private Image image;
    private float frameTime;
    private float timer;
    private bool isReversing = false;
private Coroutine animationCoroutine;
    
    private void Awake()
    {
      image = GetComponent<Image>();
  frameTime = 1f / frameRate;
    }
    
    private void Start()
    {
   if (playOnStart && sprites.Count > 0)
        {
      Play();
        }
    }
 
    /// <summary>
    /// Play the sprite animation
    /// </summary>
    public void Play()
    {
    if (sprites.Count == 0)
   {
    Debug.LogWarning("[UIImageAnimator] No sprites assigned!");
            return;
        }
        
      if (animationCoroutine != null)
        {
       StopCoroutine(animationCoroutine);
        }
  
        animationCoroutine = StartCoroutine(PlayAnimation());
    }
    
    /// <summary>
    /// Stop the animation
    /// </summary>
    public void Stop()
    {
   if (animationCoroutine != null)
        {
    StopCoroutine(animationCoroutine);
        animationCoroutine = null;
        }
        isPlaying = false;
    }
    
    /// <summary>
    /// Pause the animation
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
    }
    
    /// <summary>
    /// Resume the animation
    /// </summary>
    public void Resume()
    {
        if (!isPlaying && sprites.Count > 0)
        {
  isPlaying = true;
      }
    }
    
    /// <summary>
    /// Reset to first frame
  /// </summary>
    public void ResetToStart()
    {
        currentFrame = 0;
        isReversing = false;
        SetFrame(currentFrame);
    }
    
    /// <summary>
    /// Set specific frame
 /// </summary>
    public void SetFrame(int frameIndex)
    {
        if (frameIndex >= 0 && frameIndex < sprites.Count)
   {
            currentFrame = frameIndex;
            image.sprite = sprites[currentFrame];
        }
    }
    
    /// <summary>
    /// Change animation speed
    /// </summary>
    public void SetFrameRate(int fps)
    {
        frameRate = Mathf.Clamp(fps, 1, 60);
     frameTime = 1f / frameRate;
    }
    
    /// <summary>
    /// Get total frame count
 /// </summary>
    public int GetFrameCount()
    {
   return sprites.Count;
    }
    
    /// <summary>
    /// Get current frame index
    /// </summary>
    public int GetCurrentFrame()
    {
        return currentFrame;
    }
    
    /// <summary>
    /// Check if animation is playing
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying;
    }
    
    /// <summary>
    /// Replace sprite list at runtime
    /// </summary>
    public void SetSprites(List<Sprite> newSprites)
  {
  sprites = newSprites;
        ResetToStart();
    }
    
    private IEnumerator PlayAnimation()
    {
        // Start delay
     if (startDelay > 0f)
   {
      yield return new WaitForSeconds(startDelay);
      }
        
        isPlaying = true;
        currentFrame = 0;
   timer = 0f;
        
      while (isPlaying)
  {
      // Update timer
            timer += Time.deltaTime;
          
            // Check if it's time for next frame
            if (timer >= frameTime)
   {
              timer -= frameTime;
        
          // Advance frame
 if (!isReversing)
          {
       currentFrame++;
       
    // Check if reached end
        if (currentFrame >= sprites.Count)
       {
      if (pingPong)
     {
    isReversing = true;
  currentFrame = sprites.Count - 2;
       }
            else if (loop)
     {
          currentFrame = 0;
     onLoopComplete?.Invoke();
     }
   else
            {
          currentFrame = sprites.Count - 1;
   isPlaying = false;
           onAnimationComplete?.Invoke();
      }
    }
       }
          else // Reversing (ping pong)
         {
        currentFrame--;
          
         // Check if reached start
           if (currentFrame < 0)
           {
    if (loop)
  {
      isReversing = false;
    currentFrame = 1;
    onLoopComplete?.Invoke();
  }
            else
         {
      currentFrame = 0;
isPlaying = false;
          onAnimationComplete?.Invoke();
       }
           }
 }
          
    // Update sprite
     SetFrame(currentFrame);
   }
      
   yield return null;
        }
    }
}
