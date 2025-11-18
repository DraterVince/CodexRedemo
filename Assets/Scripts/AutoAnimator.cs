using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

#if UNITY_TEXTMESHPRO || TMP_PRESENT
using TMPro;
#endif

/// <summary>
/// Automatic Animation System
/// Easily animate any GameObject with presets or custom parameters
/// </summary>
public class AutoAnimator : MonoBehaviour
{
    [Header("Animation Type")]
    [Tooltip("Select the type of animation to play")]
    public AnimationType animationType = AnimationType.FadeIn;
    
    [Header("Timing")]
  [Tooltip("Delay before animation starts")]
    public float startDelay = 0f;
    
    [Tooltip("Duration of the animation")]
    public float duration = 0.5f;
    
    [Tooltip("Play animation on Start")]
    public bool playOnStart = true;
    
    [Tooltip("Loop the animation")]
    public bool loop = false;

    [Tooltip("Play animation in reverse after completing")]
    public bool pingPong = false;
  
    [Header("Easing")]
    [Tooltip("Easing curve for smooth animation")]
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Scale Settings")]
    [Tooltip("Start scale (for scale animations)")]
    public Vector3 startScale = Vector3.zero;
    
    [Tooltip("End scale (for scale animations)")]
    public Vector3 endScale = Vector3.one;
    
 [Header("Position Settings")]
    [Tooltip("Movement offset (for slide animations)")]
    public Vector3 moveOffset = new Vector3(100f, 0f, 0f);
  
    [Tooltip("Use local space for position")]
    public bool useLocalSpace = true;
    
    [Header("Rotation Settings")]
    [Tooltip("Start rotation (for rotation animations)")]
    public Vector3 startRotation = Vector3.zero;
    
    [Tooltip("End rotation (for rotation animations)")]
    public Vector3 endRotation = new Vector3(0f, 0f, 360f);
    
    [Header("Alpha Settings")]
    [Tooltip("Start alpha (for fade animations)")]
    [Range(0f, 1f)]
 public float startAlpha = 0f;
    
    [Tooltip("End alpha (for fade animations)")]
    [Range(0f, 1f)]
    public float endAlpha = 1f;
    
    [Header("Events")]
    [Tooltip("Trigger an event when animation completes")]
  public UnityEngine.Events.UnityEvent onAnimationComplete;
  
    // Internal state
    private Vector3 originalPosition;
  private Vector3 originalScale;
  private Quaternion originalRotation;
    private CanvasGroup canvasGroup;
    private Image image;
    private TextMeshProUGUI tmpText;
    private SpriteRenderer spriteRenderer;
    // Reserved for future use
    #pragma warning disable CS0414
    private bool isAnimating = false;
    #pragma warning restore CS0414
    private Coroutine currentAnimation;
    
    public enum AnimationType
    {
        FadeIn,
        FadeOut,
        ScaleIn,
        ScaleOut,
        ScalePulse,
        SlideFromLeft,
  SlideFromRight,
        SlideFromTop,
        SlideFromBottom,
        RotateIn,
        RotateOut,
        Bounce,
      Shake,
  PopIn,
        Custom
    }
    
    private void Awake()
    {
        // Cache original values
        originalPosition = useLocalSpace ? transform.localPosition : transform.position;
     originalScale = transform.localScale;
        originalRotation = transform.localRotation;
        
        // Get or add CanvasGroup for UI fade animations
        canvasGroup = GetComponent<CanvasGroup>();
     if (canvasGroup == null && (animationType == AnimationType.FadeIn || animationType == AnimationType.FadeOut))
        {
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
   // Cache image/text components
  image = GetComponent<Image>();
        tmpText = GetComponent<TextMeshProUGUI>();
   spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Start()
    {
        if (playOnStart)
        {
    PlayAnimation();
        }
    }
    
    /// <summary>
    /// Play the configured animation
    /// </summary>
    public void PlayAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
   
        currentAnimation = StartCoroutine(AnimateCoroutine());
    }
    
    /// <summary>
    /// Stop the current animation
  /// </summary>
    public void StopAnimation()
    {
        if (currentAnimation != null)
        {
 StopCoroutine(currentAnimation);
  currentAnimation = null;
     }
        isAnimating = false;
    }
    
    /// <summary>
    /// Reset to original state
    /// </summary>
    public void ResetToOriginal()
 {
        StopAnimation();
        
   if (useLocalSpace)
            transform.localPosition = originalPosition;
        else
         transform.position = originalPosition;
            
        transform.localScale = originalScale;
        transform.localRotation = originalRotation;
    
      if (canvasGroup != null)
       canvasGroup.alpha = 1f;
    }
    
    private IEnumerator AnimateCoroutine()
    {
     // Wait for start delay
     if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
      }
     
        do
        {
            isAnimating = true;
   
     // Execute the animation based on type
    switch (animationType)
            {
  case AnimationType.FadeIn:
 yield return FadeAnimation(startAlpha, endAlpha);
              break;
         
         case AnimationType.FadeOut:
             yield return FadeAnimation(endAlpha, startAlpha);
           break;
           
    case AnimationType.ScaleIn:
       yield return ScaleAnimation(startScale, endScale);
         break;
           
      case AnimationType.ScaleOut:
            yield return ScaleAnimation(endScale, startScale);
   break;
       
     case AnimationType.ScalePulse:
             yield return ScaleAnimation(originalScale, originalScale * 1.2f);
   yield return ScaleAnimation(originalScale * 1.2f, originalScale);
 break;
        
     case AnimationType.SlideFromLeft:
          yield return SlideAnimation(new Vector3(-moveOffset.x, 0f, 0f));
     break;
    
    case AnimationType.SlideFromRight:
            yield return SlideAnimation(new Vector3(moveOffset.x, 0f, 0f));
    break;
    
       case AnimationType.SlideFromTop:
    yield return SlideAnimation(new Vector3(0f, moveOffset.y, 0f));
break;
             
     case AnimationType.SlideFromBottom:
           yield return SlideAnimation(new Vector3(0f, -moveOffset.y, 0f));
    break;
     
          case AnimationType.RotateIn:
         yield return RotateAnimation(startRotation, endRotation);
        break;
     
       case AnimationType.RotateOut:
        yield return RotateAnimation(endRotation, startRotation);
   break;
    
        case AnimationType.Bounce:
      yield return BounceAnimation();
          break;
  
                case AnimationType.Shake:
   yield return ShakeAnimation();
      break;
   
                case AnimationType.PopIn:
            yield return PopInAnimation();
   break;
        
                case AnimationType.Custom:
   yield return CustomAnimation();
         break;
       }
       
            // Ping pong - play in reverse
       if (pingPong && !loop)
        {
       yield return ReverseAnimation();
            }
   
        } while (loop);
        
        isAnimating = false;
        onAnimationComplete?.Invoke();
  }
    
    private IEnumerator FadeAnimation(float from, float to)
    {
        float elapsed = 0f;
        
while (elapsed < duration)
        {
  elapsed += Time.deltaTime;
            float t = easingCurve.Evaluate(elapsed / duration);
    float alpha = Mathf.Lerp(from, to, t);
 
            if (canvasGroup != null)
         canvasGroup.alpha = alpha;
            else if (image != null)
       {
            Color color = image.color;
                color.a = alpha;
         image.color = color;
            }
         else if (tmpText != null)
 {
              Color color = tmpText.color;
    color.a = alpha;
         tmpText.color = color;
        }
          else if (spriteRenderer != null)
      {
      Color color = spriteRenderer.color;
       color.a = alpha;
       spriteRenderer.color = color;
        }
  
       yield return null;
        }
    }
    
    private IEnumerator ScaleAnimation(Vector3 from, Vector3 to)
  {
        float elapsed = 0f;
        
   while (elapsed < duration)
      {
            elapsed += Time.deltaTime;
            float t = easingCurve.Evaluate(elapsed / duration);
       transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
    }
        
        transform.localScale = to;
    }
    
    private IEnumerator SlideAnimation(Vector3 offset)
  {
  Vector3 startPos = originalPosition + offset;
        Vector3 endPos = originalPosition;
     
     if (useLocalSpace)
            transform.localPosition = startPos;
        else
        transform.position = startPos;
        
        float elapsed = 0f;
     
        while (elapsed < duration)
        {
         elapsed += Time.deltaTime;
       float t = easingCurve.Evaluate(elapsed / duration);
          Vector3 newPos = Vector3.Lerp(startPos, endPos, t);
    
            if (useLocalSpace)
       transform.localPosition = newPos;
     else
 transform.position = newPos;
    
            yield return null;
      }
        
if (useLocalSpace)
  transform.localPosition = endPos;
        else
            transform.position = endPos;
    }
    
    private IEnumerator RotateAnimation(Vector3 from, Vector3 to)
    {
    float elapsed = 0f;
        Quaternion startRot = Quaternion.Euler(from);
        Quaternion endRot = Quaternion.Euler(to);
        
        while (elapsed < duration)
        {
      elapsed += Time.deltaTime;
       float t = easingCurve.Evaluate(elapsed / duration);
      transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
         yield return null;
    }
  
        transform.localRotation = endRot;
    }
    
    private IEnumerator BounceAnimation()
    {
      // Bounce effect with multiple bounces
 float elapsed = 0f;
        float bounceHeight = moveOffset.y;
        Vector3 startPos = useLocalSpace ? transform.localPosition : transform.position;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
          
            // Bouncing motion
       float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 3f)) * bounceHeight * (1f - t);
          Vector3 newPos = startPos + new Vector3(0f, bounce, 0f);
    
    if (useLocalSpace)
    transform.localPosition = newPos;
       else
    transform.position = newPos;
     
      yield return null;
        }
        
        if (useLocalSpace)
      transform.localPosition = startPos;
        else
      transform.position = startPos;
    }
    
    private IEnumerator ShakeAnimation()
    {
        float elapsed = 0f;
    Vector3 startPos = useLocalSpace ? transform.localPosition : transform.position;
        float intensity = moveOffset.magnitude;
        
    while (elapsed < duration)
 {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / duration);
      
            Vector3 randomOffset = Random.insideUnitSphere * intensity * t;
            Vector3 newPos = startPos + randomOffset;
            
            if (useLocalSpace)
      transform.localPosition = newPos;
            else
         transform.position = newPos;
           
            yield return null;
   }
        
        if (useLocalSpace)
            transform.localPosition = startPos;
        else
     transform.position = startPos;
    }
    
private IEnumerator PopInAnimation()
    {
        // Combined scale and fade
        transform.localScale = Vector3.zero;
        if (canvasGroup != null)
         canvasGroup.alpha = 0f;
    
   float elapsed = 0f;
        
  while (elapsed < duration)
        {
          elapsed += Time.deltaTime;
            float t = easingCurve.Evaluate(elapsed / duration);
            
     // Overshoot scale
       float scaleValue = t < 0.7f ? t / 0.7f * 1.1f : 1.1f - ((t - 0.7f) / 0.3f * 0.1f);
            transform.localScale = originalScale * scaleValue;
            
  if (canvasGroup != null)
                canvasGroup.alpha = t;
            
  yield return null;
 }
   
        transform.localScale = originalScale;
  if (canvasGroup != null)
   canvasGroup.alpha = 1f;
    }
    
    private IEnumerator CustomAnimation()
    {
        // Custom animation combining scale, position, and rotation
        Vector3 startPos = originalPosition + moveOffset;
        
        if (useLocalSpace)
            transform.localPosition = startPos;
        else
            transform.position = startPos;
            
    transform.localScale = startScale;
        transform.localRotation = Quaternion.Euler(startRotation);
        
      float elapsed = 0f;
        
        while (elapsed < duration)
        {
      elapsed += Time.deltaTime;
       float t = easingCurve.Evaluate(elapsed / duration);
            
        // Interpolate position
       Vector3 newPos = Vector3.Lerp(startPos, originalPosition, t);
     if (useLocalSpace)
          transform.localPosition = newPos;
  else
           transform.position = newPos;
            
       // Interpolate scale
        transform.localScale = Vector3.Lerp(startScale, endScale, t);
   
          // Interpolate rotation
       transform.localRotation = Quaternion.Lerp(
         Quaternion.Euler(startRotation),
    Quaternion.Euler(endRotation),
         t
     );
            
yield return null;
        }
        
        if (useLocalSpace)
            transform.localPosition = originalPosition;
      else
 transform.position = originalPosition;

        transform.localScale = endScale;
        transform.localRotation = Quaternion.Euler(endRotation);
    }
    
    private IEnumerator ReverseAnimation()
    {
        // Play animation in reverse
        AnimationType originalType = animationType;

        // Swap start/end values temporarily
        Vector3 tempScale = startScale;
        startScale = endScale;
        endScale = tempScale;
 
        Vector3 tempRot = startRotation;
        startRotation = endRotation;
        endRotation = tempRot;
     
      float tempAlpha = startAlpha;
        startAlpha = endAlpha;
 endAlpha = tempAlpha;
        
    // Execute reversed animation
        yield return AnimateCoroutine();
        
        // Restore original values
        startScale = endScale;
        endScale = tempScale;
 startRotation = endRotation;
        endRotation = tempRot;
        startAlpha = endAlpha;
        endAlpha = tempAlpha;
    }
  
    /// <summary>
    /// Quick helper: Fade in this object
    /// </summary>
    public static void FadeIn(GameObject obj, float duration = 0.5f)
    {
    AutoAnimator anim = obj.GetComponent<AutoAnimator>();
 if (anim == null)
anim = obj.AddComponent<AutoAnimator>();
   
   anim.animationType = AnimationType.FadeIn;
        anim.duration = duration;
     anim.PlayAnimation();
    }
  
    /// <summary>
    /// Quick helper: Scale in this object
    /// </summary>
  public static void ScaleIn(GameObject obj, float duration = 0.5f)
    {
        AutoAnimator anim = obj.GetComponent<AutoAnimator>();
   if (anim == null)
         anim = obj.AddComponent<AutoAnimator>();
        
        anim.animationType = AnimationType.ScaleIn;
        anim.duration = duration;
        anim.PlayAnimation();
    }
  
    /// <summary>
    /// Quick helper: Pop in this object
    /// </summary>
    public static void PopIn(GameObject obj, float duration = 0.5f)
    {
        AutoAnimator anim = obj.GetComponent<AutoAnimator>();
        if (anim == null)
       anim = obj.AddComponent<AutoAnimator>();
        
        anim.animationType = AnimationType.PopIn;
        anim.duration = duration;
        anim.PlayAnimation();
    }
}
