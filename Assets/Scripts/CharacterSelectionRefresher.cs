using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectionRefresher : MonoBehaviour
{
    [Header("Auto Refresh Settings")]
    [Tooltip("Automatically refresh character selection when this script starts")]
    [SerializeField] private bool autoRefreshOnStart = true;
    
  [Tooltip("Delay before refreshing (seconds)")]
    [SerializeField] private float refreshDelay = 0.3f;
    
    private void Start()
  {
  if (autoRefreshOnStart)
{
 Invoke(nameof(RefreshCharacterSelection), refreshDelay);
  }
    }
    
    public void RefreshCharacterSelection()
{
     // Find CharacterSelectionUI using dynamic lookup
        var uiType = System.Type.GetType("CharacterSelectionUI");
  if (uiType == null)
   {
     Debug.LogWarning("[CharacterSelectionRefresher] CharacterSelectionUI type not found!");
         return;
 }
        
var characterSelectionUI = FindObjectOfType(uiType);
        
        if (characterSelectionUI != null)
        {
   // Call RefreshDisplay method using reflection
            var refreshMethod = uiType.GetMethod("RefreshDisplay");
if (refreshMethod != null)
 {
     refreshMethod.Invoke(characterSelectionUI, null);
         Debug.Log("[CharacterSelectionRefresher] Character selection refreshed successfully!");
        }
  else
 {
      Debug.LogWarning("[CharacterSelectionRefresher] RefreshDisplay method not found!");
   }
 }
        else
        {
   Debug.LogWarning("[CharacterSelectionRefresher] Could not find CharacterSelectionUI to refresh!");
        }
    }
    
    /// <summary>
    /// Call this when loading level select scene from code
  /// </summary>
public static void RefreshOnSceneLoad()
{
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
  if (scene.name == "LevelSelect")
    {
          // Find and refresh after a short delay
   var refresher = FindObjectOfType<CharacterSelectionRefresher>();
      if (refresher != null)
{
    refresher.Invoke(nameof(RefreshCharacterSelection), 0.3f);
   }
            else
     {
    // No refresher found, create a temporary one
 var tempObj = new GameObject("TempCharacterSelectionRefresher");
    var tempRefresher = tempObj.AddComponent<CharacterSelectionRefresher>();
      tempRefresher.refreshDelay = 0.3f;
    tempRefresher.autoRefreshOnStart = true;
     Destroy(tempObj, 1f); // Clean up after 1 second
       }
        }
        
        // Unsubscribe after one use
 SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
