using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns character sprites on level select that play idle animations
/// For WORLD SPACE sprites (not UI Canvas)
/// </summary>
public class LevelSelectSpriteSpawner : MonoBehaviour
{
    [Header("Sprite Setup")]
    [Tooltip("Prefab with SpriteRenderer (SpriteAnimator will be added automatically)")]
    public GameObject spritePrefab;
    
    [Tooltip("Idle animation sprites for each character")]
    public List<IdleCharacterData> characters = new List<IdleCharacterData>();
    
    [Header("Spawn Settings")]
    [Tooltip("Parent transform for spawned sprites (optional)")]
    public Transform spawnParent;
    
    [Tooltip("Spacing between sprites in world units")]
    public float spacing = 2f;
 
    [Tooltip("Start position for first sprite")]
    public Vector3 startPosition = Vector3.zero;
    
    [Tooltip("Spawn horizontally (true) or vertically (false)")]
    public bool horizontalLayout = true;
    
    [Tooltip("Scale multiplier for spawned sprites")]
    public float spriteScale = 1f;
    
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
        SpawnAllCharacters();
    }
    
    /// <summary>
    /// Spawn all characters with their idle animations
    /// </summary>
    public void SpawnAllCharacters()
    {
      ClearSpawnedSprites();
        
        for (int i = 0; i < characters.Count; i++)
{
            if (characters[i].idleSprites.Count == 0)
       {
         Debug.LogWarning($"[LevelSelectSpriteSpawner] Character '{characters[i].characterName}' has no idle sprites!");
     continue;
     }
      
      SpawnCharacter(i);
     }
    }
    
    /// <summary>
 /// Spawn a single character at the specified index
    /// </summary>
    private void SpawnCharacter(int index)
    {
        // Calculate spawn position
  Vector3 offset = horizontalLayout 
  ? new Vector3(spacing * index, 0, 0) 
            : new Vector3(0, -spacing * index, 0);
   Vector3 spawnPosition = startPosition + offset;
        
    // Create sprite object
        GameObject spriteObj;
        if (spritePrefab != null)
    {
      spriteObj = Instantiate(spritePrefab, spawnPosition, Quaternion.identity);
    }
     else
        {
     spriteObj = new GameObject($"Character_{characters[index].characterName}");
       spriteObj.transform.position = spawnPosition;
     spriteObj.AddComponent<SpriteRenderer>();
      }
        
        // Set parent
        if (spawnParent != null)
        {
            spriteObj.transform.SetParent(spawnParent);
   spriteObj.transform.localPosition = spawnPosition;
 }
        
        // Get or add SpriteRenderer
        SpriteRenderer spriteRenderer = spriteObj.GetComponent<SpriteRenderer>();
   if (spriteRenderer == null)
        {
      spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();
     }
        
        // Set initial sprite
 spriteRenderer.sprite = characters[index].idleSprites[0];
        
        // Apply scale
 spriteObj.transform.localScale = Vector3.one * spriteScale;
     
   // Add and configure SpriteAnimator
        SpriteAnimator animator = spriteObj.GetComponent<SpriteAnimator>();
     if (animator == null)
        {
            animator = spriteObj.AddComponent<SpriteAnimator>();
      }
        
  animator.sprites = characters[index].idleSprites;
        animator.frameRate = idleFrameRate;
        animator.loop = true;
      animator.playOnStart = autoPlay;
        
   if (autoPlay)
 {
            animator.Play();
        }
        
        spawnedSprites.Add(spriteObj);
  
      Debug.Log($"[LevelSelectSpriteSpawner] Spawned '{characters[index].characterName}' with {characters[index].idleSprites.Count} idle frames");
    }
  
    /// <summary>
  /// Clear all spawned sprites
    /// </summary>
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
    
    /// <summary>
    /// Play all idle animations
    /// </summary>
    public void PlayAllAnimations()
    {
   foreach (GameObject sprite in spawnedSprites)
{
        if (sprite != null)
{
    SpriteAnimator animator = sprite.GetComponent<SpriteAnimator>();
   if (animator != null)
     {
       animator.Play();
           }
  }
      }
    }
  
    /// <summary>
    /// Stop all idle animations
    /// </summary>
    public void StopAllAnimations()
    {
        foreach (GameObject sprite in spawnedSprites)
        {
     if (sprite != null)
      {
     SpriteAnimator animator = sprite.GetComponent<SpriteAnimator>();
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
