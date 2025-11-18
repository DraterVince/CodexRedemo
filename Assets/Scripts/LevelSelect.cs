using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    [Header("Character Selection Refresh")]
    [Tooltip("Automatically refresh character selection when loading this scene")]
    public bool autoRefreshCharacterSelection = true;

    [Header("Scene Cleanup")]
    [Tooltip("Reset Time.timeScale when loading this scene")]
    public bool resetTimeScale = true;
    
    [Tooltip("Stop all coroutines when loading this scene")]
    public bool stopAllCoroutines = true;

    private void Awake()
    {
        if (resetTimeScale)
      {
            Time.timeScale = 1f;
        }
    }
    
    private void Start()
    {
        if (autoRefreshCharacterSelection)
  {
            StartCoroutine(ForceRefreshCharacterSelection());
        }
 }
    
    private IEnumerator ForceRefreshCharacterSelection()
    {
        
        yield return new WaitForSeconds(0.5f);
        
    var uiType = System.Type.GetType("CharacterSelectionUI");
    if (uiType != null)
   {
 var ui = FindObjectOfType(uiType);
            
       if (ui != null)
            {
        
         var refreshMethod = uiType.GetMethod("RefreshDisplay");
   if (refreshMethod != null)
        {
        refreshMethod.Invoke(ui, null);
    }
            }
      else
            {
           Debug.LogWarning("[LevelSelect] CharacterSelectionUI not found in scene");
            }
   }
        else
        {
            Debug.LogWarning("[LevelSelect] CharacterSelectionUI type not found");
        }
    
        var switcherType = System.Type.GetType("CharacterSwitcher");
        if (switcherType != null)
     {
            var switcher = FindObjectOfType(switcherType);
    if (switcher != null)
   {
          Debug.Log("[LevelSelect] Found CharacterSwitcher");
         
         var loadMethod = switcherType.GetMethod("LoadUnlockStates");
       if (loadMethod != null)
    {
            loadMethod.Invoke(switcher, null);
Debug.Log("[LevelSelect] CharacterSwitcher unlock states reloaded");
    }
    }
        }
    }

    public void LoadLevelSelect()
    {
        
        if (stopAllCoroutines)
     {
            StopAllCoroutines();
        }
        
        if (resetTimeScale)
        {
            Time.timeScale = 1f;
        }
        
   if (autoRefreshCharacterSelection)
        {
CharacterSelectionRefresher.RefreshOnSceneLoad();
        }

    SceneManager.LoadSceneAsync("LevelSelect");
    }
}