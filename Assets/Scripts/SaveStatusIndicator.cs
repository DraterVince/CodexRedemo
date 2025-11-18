using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_TEXTMESHPRO || TMPRO
using TMPro;
#endif

/// <summary>
/// Displays save status feedback to the user
/// Can be used as a singleton or placed on individual UI panels
/// </summary>
public class SaveStatusIndicator : MonoBehaviour
{
    public static SaveStatusIndicator Instance { get; private set; }
    
    [Header("UI References")]
    [Tooltip("Text to display save status messages (supports both Text and TextMeshProUGUI)")]
    public Component statusTextComponent;
    
    [Tooltip("Optional panel/image to show/hide")]
    public GameObject statusPanel;
  
  [Tooltip("Optional loading spinner/animation")]
 public GameObject loadingSpinner;
    
    [Header("Settings")]
 [Tooltip("How long to show status messages (0 = stay visible)")]
    public float messageDisplayDuration = 3f;
    
    [Tooltip("Use this as a singleton instance")]
    public bool useSingleton = true;
  
  private Coroutine hideCoroutine;
    
    private void Awake()
 {
        if (useSingleton)
     {
       if (Instance == null)
    {
   Instance = this;
       DontDestroyOnLoad(gameObject);
            }
  else
  {
     Destroy(gameObject);
         return;
   }
        }
   
     // Start hidden
        Hide();
    }
    
  /// <summary>
    /// Show a saving message with optional spinner
    /// </summary>
  public void ShowSaving(string message = "Saving...")
 {
        if (hideCoroutine != null)
{
     StopCoroutine(hideCoroutine);
   hideCoroutine = null;
  }
   
        SetText(message);
   
        if (statusPanel != null)
        {
      statusPanel.SetActive(true);
  }
     
     if (loadingSpinner != null)
        {
        loadingSpinner.SetActive(true);
        }
    }
    
    /// <summary>
    /// Show a success message
    /// </summary>
    public void ShowSuccess(string message = "Save Complete!")
  {
 if (hideCoroutine != null)
   {
       StopCoroutine(hideCoroutine);
        }
      
        SetText(message);
        
if (statusPanel != null)
  {
     statusPanel.SetActive(true);
        }
      
   if (loadingSpinner != null)
    {
  loadingSpinner.SetActive(false);
   }
 
        // Auto-hide after delay
        if (messageDisplayDuration > 0)
        {
   hideCoroutine = StartCoroutine(HideAfterDelay(messageDisplayDuration));
  }
  }
    
    /// <summary>
    /// Show an error message
    /// </summary>
  public void ShowError(string message = "Save Failed!")
    {
        if (hideCoroutine != null)
  {
  StopCoroutine(hideCoroutine);
   }
     
      SetText(message);
        
 if (statusPanel != null)
        {
   statusPanel.SetActive(true);
        }
   
        if (loadingSpinner != null)
     {
            loadingSpinner.SetActive(false);
   }
   
   // Auto-hide after delay
        if (messageDisplayDuration > 0)
  {
       hideCoroutine = StartCoroutine(HideAfterDelay(messageDisplayDuration));
        }
    }
    
    /// <summary>
    /// Hide the status indicator
    /// </summary>
    public void Hide()
    {
        if (hideCoroutine != null)
  {
     StopCoroutine(hideCoroutine);
       hideCoroutine = null;
        }
        
        if (statusPanel != null)
  {
            statusPanel.SetActive(false);
      }
   
        if (loadingSpinner != null)
     {
 loadingSpinner.SetActive(false);
     }
  }
    
 private IEnumerator HideAfterDelay(float delay)
  {
        yield return new WaitForSecondsRealtime(delay);
    Hide();
    }
    
    private void SetText(string text)
    {
        if (statusTextComponent == null) return;
        
#if UNITY_TEXTMESHPRO || TMPRO
     // Try TextMeshProUGUI first
        var tmpText = statusTextComponent as TextMeshProUGUI;
     if (tmpText != null)
     {
  tmpText.text = text;
  return;
        }
#endif
        
        // Fallback to Unity Text
      var unityText = statusTextComponent as Text;
        if (unityText != null)
        {
            unityText.text = text;
        }
    }
}
