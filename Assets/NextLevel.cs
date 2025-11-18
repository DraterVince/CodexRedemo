using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
  public int nextLevel;
    public bool autoSaveBeforeNext = true;
 public float webglSaveDelay = 3.0f;

 private bool isTransitioning = false;

    private void Start()
    {
        nextLevel = SceneManager.GetActiveScene().buildIndex + 1;
 }

    public void ProgressLevel()
    {
  if (isTransitioning)
        {
            return;
     }
        
        isTransitioning = true;
      StartCoroutine(SaveAndLoadSceneCoroutine(nextLevel));
    }
    
 private IEnumerator SaveAndLoadSceneCoroutine(int sceneIndex)
  {
        if (autoSaveBeforeNext && NewAndLoadGameManager.Instance != null)
        {
   NewAndLoadGameManager.Instance.AutoSave();
   
#if UNITY_WEBGL && !UNITY_EDITOR
            yield return StartCoroutine(NewAndLoadGameManager.Instance.WaitForSaveCompletion());
        yield return new WaitForSecondsRealtime(webglSaveDelay);
#else
            yield return new WaitForSecondsRealtime(0.1f);
#endif
        }
     SceneManager.LoadScene(sceneIndex);
    }
}
