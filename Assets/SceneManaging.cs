using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManaging : MonoBehaviour
{
    [Header("Save Settings")]
    [Tooltip("Auto-save game progress before changing scenes")]
    public bool autoSaveBeforeChange = true;
  
    [Header("WebGL Save Settings")]
    [Tooltip("Wait time for IndexedDB to flush in WebGL (seconds)")]
    public float webglSaveDelay = 3.0f;

    private bool isTransitioning = false;

  public void ChangeScene(int sceneIndex)
  {
   if (isTransitioning)
{
   Debug.LogWarning("[SceneManaging] Already transitioning, ignoring duplicate call");
     return;
        }
     
     isTransitioning = true;
        StartCoroutine(SaveAndLoadSceneCoroutine(sceneIndex));
    }
    
    private IEnumerator SaveAndLoadSceneCoroutine(int sceneIndex)
 {
        // Auto-save before changing scenes
      if (autoSaveBeforeChange && NewAndLoadGameManager.Instance != null)
  {
  Debug.Log($"[SceneManaging] Auto-saving before loading scene {sceneIndex}...");
     NewAndLoadGameManager.Instance.AutoSave();
       
#if UNITY_WEBGL && !UNITY_EDITOR
       // WebGL: Wait for save to complete + extra buffer for IndexedDB
   Debug.Log("[SceneManaging] WebGL: Waiting for save operation to complete...");
  yield return StartCoroutine(NewAndLoadGameManager.Instance.WaitForSaveCompletion());
     
 Debug.Log($"[SceneManaging] WebGL: Adding {webglSaveDelay}s buffer for IndexedDB flush...");
    yield return new WaitForSecondsRealtime(webglSaveDelay);
  Debug.Log("[SceneManaging] WebGL: Save should be fully persisted");
#else
   // Desktop: Small delay to ensure save completes
            yield return new WaitForSecondsRealtime(0.1f);
#endif
          
            Debug.Log("[SceneManaging] Game progress saved!");
 }
     
  Debug.Log($"[SceneManaging] Loading scene {sceneIndex}...");
        SceneManager.LoadScene(sceneIndex);
    }
}
