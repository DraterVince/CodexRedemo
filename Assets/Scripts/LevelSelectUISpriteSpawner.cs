using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Spawns character sprites with idle animations on UI Canvas
/// Specifically for level select screens
/// </summary>
public class LevelSelectUISpriteSpawner : MonoBehaviour
{
    [Header("Sprite Setup")]
    public List<IdleCharacterData> characters = new List<IdleCharacterData>();
    
    [Header("UI Spawn Settings")]
    [Tooltip("Parent RectTransform (usually inside a Canvas)")]
    public RectTransform spawnParent;
    
    [Tooltip("Spacing between sprites in pixels")]
    public float spacing = 150f;
    
    [Tooltip("Start position for first sprite")]
    public Vector2 startPosition = new Vector2(-300, 0);
    
    [Tooltip("Spawn horizontally (true) or vertically (false)")]
    public bool horizontalLayout = true;
    
    [Header("Sprite Settings")]
    [Tooltip("Size of each character sprite")]
    public Vector2 spriteSize = new Vector2(100, 100);
 
    [Header("Animation Settings")]
  [Tooltip("Frame rate for idle animations")]
    public int idleFrameRate = 8;
    
    [Tooltip("Auto-start animations on spawn")]
    public bool autoPlay = true;
 
    private List<GameObject> spawnedSprites = new List<GameObject>();
    
    [System.Serializable]
  public class IdleCharacterData
    {
        public string characterName;
        public List<Sprite> idleSprites = new List<Sprite>();
    }
    
    private void Start()
    {
        // Auto-set spawn parent to this RectTransform if not set
        if (spawnParent == null)
        {
         spawnParent = GetComponent<RectTransform>();
   }
        
  SpawnAllCharacters();
    }
    
    public void SpawnAllCharacters()
 {
        ClearSpawnedSprites();
        
        for (int i = 0; i < characters.Count; i++)
        {
         if (characters[i].idleSprites.Count == 0)
            {
 Debug.LogWarning($"[LevelSelectUISpriteSpawner] Character '{characters[i].characterName}' has no idle sprites!");
     continue;
            }
            
            SpawnCharacter(i);
     }
    }
    
    private void SpawnCharacter(int index)
    {
 // Calculate UI position
 Vector2 offset = horizontalLayout 
            ? new Vector2(spacing * index, 0) 
  : new Vector2(0, -spacing * index);
        Vector2 spawnPosition = startPosition + offset;
        
   // Create UI GameObject
        GameObject spriteObj = new GameObject($"Character_{characters[index].characterName}");
    spriteObj.transform.SetParent(spawnParent, false);
   
        // Add RectTransform
        RectTransform rectTransform = spriteObj.AddComponent<RectTransform>();
    rectTransform.anchoredPosition = spawnPosition;
        rectTransform.sizeDelta = spriteSize;
        
        // Add Image component (UI version of SpriteRenderer)
 Image image = spriteObj.AddComponent<Image>();
        image.sprite = characters[index].idleSprites[0];
image.preserveAspect = true;
        
        // Add UIImageAnimator for UI-based sprite animation
        UIImageAnimator animator = spriteObj.AddComponent<UIImageAnimator>();
        animator.sprites = characters[index].idleSprites;
        animator.frameRate = idleFrameRate;
        animator.loop = true;
        animator.playOnStart = autoPlay;
      
        if (autoPlay)
        {
     animator.Play();
        }
        
        spawnedSprites.Add(spriteObj);
        
        Debug.Log($"[LevelSelectUISpriteSpawner] Spawned UI sprite '{characters[index].characterName}' with {characters[index].idleSprites.Count} idle frames");
    }
    
    public void ClearSpawnedSprites()
    {
        foreach (GameObject sprite in spawnedSprites)
        {
         if (sprite != null)
      {
      Destroy(sprite);
            }
  }
        spawnedSprites.Clear();
    }
    
    public void PlayAllAnimations()
    {
        foreach (GameObject sprite in spawnedSprites)
        {
   if (sprite != null)
            {
          UIImageAnimator animator = sprite.GetComponent<UIImageAnimator>();
 if (animator != null)
  {
        animator.Play();
       }
    }
        }
    }
 
    public void StopAllAnimations()
    {
        foreach (GameObject sprite in spawnedSprites)
        {
        if (sprite != null)
   {
      UIImageAnimator animator = sprite.GetComponent<UIImageAnimator>();
                if (animator != null)
          {
          animator.Stop();
     }
  }
        }
    }
    
    private void OnDestroy()
    {
        ClearSpawnedSprites();
    }
}
