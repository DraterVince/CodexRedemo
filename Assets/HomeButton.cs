using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButton : MonoBehaviour
{
    [Header("Cleanup Settings")]
    [Tooltip("Reset Time.timeScale when going home")]
    public bool resetTimeScale = true;
    
    [Tooltip("Stop all timers before leaving")]
    public bool stopTimers = true;
    
    [Header("Save Settings")]
    [Tooltip("Auto-save game progress before returning to menu")]
    public bool autoSaveOnExit = true;
    
    [Header("WebGL Save Settings")]
    [Tooltip("Wait time for IndexedDB to flush in WebGL (seconds)")]
    public float webglSaveDelay = 5.0f;

    private bool isTransitioning = false;

    public void LoadHome()
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[HomeButton] Already transitioning, ignoring duplicate call");
            return;
        }
        
        isTransitioning = true;
        Debug.Log("[HomeButton] Loading home - performing cleanup...");
        
        // Reset time scale first
        if (resetTimeScale)
        {
            Time.timeScale = 1f;
            Debug.Log("[HomeButton] Time.timeScale reset to 1");
        }
        
        // Stop timers
        if (stopTimers)
        {
            var timerType = System.Type.GetType("Timer");
            if (timerType != null)
            {
                var timer = FindObjectOfType(timerType);
                if (timer != null)
                {
                    var pauseMethod = timerType.GetMethod("PauseTimer");
                    if (pauseMethod != null)
                    {
                        pauseMethod.Invoke(timer, null);
                        Debug.Log("[HomeButton] Timer paused");
                    }
                }
            }
        }
      
        CharacterSelectionRefresher.RefreshOnSceneLoad();
        
        // Start the transition coroutine
        StartCoroutine(SaveAndLoadSceneCoroutine());
    }
    
    private IEnumerator SaveAndLoadSceneCoroutine()
    {
        // Auto-save before leaving
        if (autoSaveOnExit && NewAndLoadGameManager.Instance != null)
        {
            // Only save if there's an active slot
            if (NewAndLoadGameManager.Instance.CurrentSlot == 0)
            {
                Debug.LogWarning("[HomeButton] No active save slot - skipping auto-save");
            }
            else
            {
                Debug.Log("[HomeButton] Auto-saving game progress...");
                NewAndLoadGameManager.Instance.AutoSave();
       
#if UNITY_WEBGL && !UNITY_EDITOR
                // WebGL: Wait for save to complete + extra buffer for IndexedDB
                Debug.Log("[HomeButton] WebGL: Waiting for save operation to complete...");
                yield return StartCoroutine(NewAndLoadGameManager.Instance.WaitForSaveCompletion());
   
                Debug.Log($"[HomeButton] WebGL: Adding {webglSaveDelay}s buffer for IndexedDB flush...");
                yield return new WaitForSecondsRealtime(webglSaveDelay);
                Debug.Log("[HomeButton] WebGL: Save should be fully persisted");
#else
                // Desktop: Small delay to ensure save completes
                yield return new WaitForSecondsRealtime(0.1f);
#endif
    
                Debug.Log("[HomeButton] Game progress saved!");
            }
        }
   
        Debug.Log("[HomeButton] Loading LevelSelect scene...");
        SceneManager.LoadSceneAsync("LevelSelect");
    }
}
