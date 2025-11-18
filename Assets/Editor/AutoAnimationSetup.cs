using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// Auto-detects sprite animations from folder structure
/// Looks for folders named "Idle" and "Attack" containing sprite sheets
/// </summary>
public class AutoAnimationSetup : EditorWindow
{
    private string spriteFolderPath = "Assets";
    private GameObject targetCharacter;
    private bool searchComplete = false;
    private List<Sprite> idleSprites = new List<Sprite>();
    private List<Sprite> attackSprites = new List<Sprite>();
    private string idleFolderFound = "";
    private string attackFolderFound = "";
    
 [Header("Animation Settings")]
    private int idleFPS = 8;
    private int attackFPS = 24;
 private bool autoSetup = true;
    
    [MenuItem("Tools/Auto Setup Character Animations")]
    public static void ShowWindow()
    {
        var window = GetWindow<AutoAnimationSetup>("Auto Animation Setup");
     window.minSize = new Vector2(500, 600);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Auto Character Animation Setup", EditorStyles.boldLabel);
EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool automatically finds sprite animations in your project.\n\n" +
"Expected folder structure:\n" +
  "  YourFolder/\n" +
        "    ??? Idle/       (contains idle sprites)\n" +
    "    ??? Attack/     (contains attack sprites)\n\n" +
   "OR sprite sheets named:\n" +
     "  ??? idle.png / Idle.png     (sliced sprite sheet)\n" +
     "  ??? attack.png / Attack.png (sliced sprite sheet)\n\n" +
 "It will find folders OR sprite sheets and automatically setup animations!", 
MessageType.Info);
        
        EditorGUILayout.Space();
        
    // Folder path selection
        EditorGUILayout.LabelField("1. Select Sprite Folder", EditorStyles.boldLabel);
   EditorGUILayout.BeginHorizontal();
     spriteFolderPath = EditorGUILayout.TextField("Search Path:", spriteFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Sprite Folder", "Assets", "");
        if (!string.IsNullOrEmpty(path))
       {
        if (path.StartsWith(Application.dataPath))
      {
   spriteFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
    }
 }
  }
    EditorGUILayout.EndHorizontal();
 
        // Search button
        if (GUILayout.Button("?? Search for Animations", GUILayout.Height(35)))
  {
            SearchForAnimations();
      }
        
        EditorGUILayout.Space();
        
        // Show search results
        if (searchComplete)
        {
  EditorGUILayout.LabelField("Search Results", EditorStyles.boldLabel);
            
     // Idle sprites
      EditorGUILayout.BeginVertical("box");
         EditorGUILayout.LabelField($"Idle Sprites: {idleSprites.Count} found", EditorStyles.boldLabel);
            if (!string.IsNullOrEmpty(idleFolderFound))
         {
           EditorGUILayout.LabelField($"?? {idleFolderFound}", EditorStyles.miniLabel);
        }
         if (idleSprites.Count > 0)
 {
   GUI.color = Color.green;
     EditorGUILayout.LabelField($"? Found {idleSprites.Count} idle sprites");
          GUI.color = Color.white;
      
   // Show first few sprite names
                for (int i = 0; i < Mathf.Min(3, idleSprites.Count); i++)
     {
  EditorGUILayout.LabelField($"  • {idleSprites[i].name}", EditorStyles.miniLabel);
      }
            if (idleSprites.Count > 3)
      {
         EditorGUILayout.LabelField($"  ... and {idleSprites.Count - 3} more", EditorStyles.miniLabel);
   }
          }
   else
          {
     GUI.color = Color.red;
                EditorGUILayout.LabelField("? No idle sprites found!");
          GUI.color = Color.white;
       }
            EditorGUILayout.EndVertical();
     
        EditorGUILayout.Space();
       
            // Attack sprites
       EditorGUILayout.BeginVertical("box");
    EditorGUILayout.LabelField($"Attack Sprites: {attackSprites.Count} found", EditorStyles.boldLabel);
     if (!string.IsNullOrEmpty(attackFolderFound))
        {
  EditorGUILayout.LabelField($"?? {attackFolderFound}", EditorStyles.miniLabel);
          }
            if (attackSprites.Count > 0)
  {
         GUI.color = Color.green;
         EditorGUILayout.LabelField($"? Found {attackSprites.Count} attack sprites");
    GUI.color = Color.white;
  
      // Show first few sprite names
       for (int i = 0; i < Mathf.Min(3, attackSprites.Count); i++)
  {
          EditorGUILayout.LabelField($"  • {attackSprites[i].name}", EditorStyles.miniLabel);
           }
        if (attackSprites.Count > 3)
      {
          EditorGUILayout.LabelField($"  ... and {attackSprites.Count - 3} more", EditorStyles.miniLabel);
                }
  }
            else
       {
      GUI.color = Color.red;
             EditorGUILayout.LabelField("? No attack sprites found!");
 GUI.color = Color.white;
            }
      EditorGUILayout.EndVertical();
    
          EditorGUILayout.Space();
            
            // Setup section (only show if sprites found)
            if (idleSprites.Count > 0 || attackSprites.Count > 0)
          {
    EditorGUILayout.LabelField("2. Configure & Setup", EditorStyles.boldLabel);
           
 EditorGUILayout.BeginVertical("box");
 targetCharacter = (GameObject)EditorGUILayout.ObjectField(
     "Target Character:", 
    targetCharacter, 
        typeof(GameObject), 
                true);
                
 if (targetCharacter == null)
      {
            EditorGUILayout.HelpBox(
       "Leave empty to create a new GameObject, or drag an existing character here.", 
        MessageType.Info);
    }
    
    EditorGUILayout.Space();
    
     idleFPS = EditorGUILayout.IntSlider("Idle FPS:", idleFPS, 1, 60);
      attackFPS = EditorGUILayout.IntSlider("Attack FPS:", attackFPS, 1, 60);
         
                autoSetup = EditorGUILayout.Toggle("Auto-add Input Handler", autoSetup);
         
     EditorGUILayout.EndVertical();
         
      EditorGUILayout.Space();
    
           // Setup button
       GUI.backgroundColor = Color.green;
          bool canSetup = (idleSprites.Count > 0 || attackSprites.Count > 0);
       GUI.enabled = canSetup;
         
      if (GUILayout.Button("? Setup Character Animations!", GUILayout.Height(40)))
          {
             SetupCharacterAnimations();
     }
    
  GUI.enabled = true;
           GUI.backgroundColor = Color.white;
}
        }
   
        EditorGUILayout.Space();
        
        // Tips
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("?? Tips:", EditorStyles.boldLabel);
  EditorGUILayout.LabelField("• Folder names can be 'Idle', 'idle', 'IDLE', etc.", EditorStyles.miniLabel);
        EditorGUILayout.LabelField("• Same for 'Attack', 'attack', 'ATTACK', etc.", EditorStyles.miniLabel);
        EditorGUILayout.LabelField("• Sprite sheets can be named 'idle.png', 'attack.png', etc.", EditorStyles.miniLabel);
        EditorGUILayout.LabelField("• Sprites will be sorted alphabetically", EditorStyles.miniLabel);
        EditorGUILayout.LabelField("• Make sure sprites are sliced before searching!", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
    }
    
    private void SearchForAnimations()
    {
   idleSprites.Clear();
        attackSprites.Clear();
     idleFolderFound = "";
        attackFolderFound = "";
   searchComplete = false;
        
        Debug.Log($"[AutoAnimationSetup] Searching in: {spriteFolderPath}");
        
        // Search for folders
        string[] allFolders = AssetDatabase.GetSubFolders(spriteFolderPath);
        
    // Recursively search all subdirectories
        SearchDirectory(spriteFolderPath);
        
  searchComplete = true;
        
        Debug.Log($"[AutoAnimationSetup] Search complete! Idle: {idleSprites.Count}, Attack: {attackSprites.Count}");
    }
    
    private void SearchDirectory(string directory)
    {
        // Check current directory
        string folderName = Path.GetFileName(directory);
   
     // Check if this is an Idle folder
        if (folderName.ToLower() == "idle" && idleSprites.Count == 0)
        {
  idleSprites = LoadSpritesFromFolder(directory);
            idleFolderFound = directory;
       Debug.Log($"[AutoAnimationSetup] Found Idle folder: {directory} ({idleSprites.Count} sprites)");
        }
     
      // Check if this is an Attack folder
        if (folderName.ToLower() == "attack" && attackSprites.Count == 0)
        {
attackSprites = LoadSpritesFromFolder(directory);
            attackFolderFound = directory;
            Debug.Log($"[AutoAnimationSetup] Found Attack folder: {directory} ({attackSprites.Count} sprites)");
        }
        
      // NEW: Check for sprite sheets named "idle" or "attack" in this directory
     if (idleSprites.Count == 0 || attackSprites.Count == 0)
    {
string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { directory });
    
            foreach (string guid in textureGuids)
 {
  string path = AssetDatabase.GUIDToAssetPath(guid);
    string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
            
         // Check for "idle" sprite sheet
         if (fileName == "idle" && idleSprites.Count == 0)
      {
      List<Sprite> sprites = LoadSpritesFromSpriteSheet(path);
          if (sprites.Count > 0)
        {
idleSprites = sprites;
 idleFolderFound = path;
 Debug.Log($"[AutoAnimationSetup] Found Idle sprite sheet: {path} ({idleSprites.Count} sprites)");
    }
           }
        
          // Check for "attack" sprite sheet
            if (fileName == "attack" && attackSprites.Count == 0)
       {
        List<Sprite> sprites = LoadSpritesFromSpriteSheet(path);
        if (sprites.Count > 0)
      {
   attackSprites = sprites;
               attackFolderFound = path;
           Debug.Log($"[AutoAnimationSetup] Found Attack sprite sheet: {path} ({attackSprites.Count} sprites)");
   }
        }
    }
  }
        
 // Recursively search subdirectories
        string[] subFolders = AssetDatabase.GetSubFolders(directory);
        foreach (string subFolder in subFolders)
        {
        SearchDirectory(subFolder);
}
    }
    
    private List<Sprite> LoadSpritesFromSpriteSheet(string texturePath)
    {
        List<Sprite> sprites = new List<Sprite>();
        
        // Load all sprites from this texture/sprite sheet
      Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
        foreach (Object asset in assets)
 {
   if (asset is Sprite sprite)
          {
         sprites.Add(sprite);
 }
    }
     
  // Sort by name
        sprites = sprites.OrderBy(s => s.name).ToList();
    
        return sprites;
    }
    
    private List<Sprite> LoadSpritesFromFolder(string folderPath)
    {
        List<Sprite> sprites = new List<Sprite>();
        
   // Get all assets in folder
  string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
        
        foreach (string guid in guids)
        {
       string path = AssetDatabase.GUIDToAssetPath(guid);

      // Load all sprites from this asset (handles sprite sheets)
      Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
     foreach (Object asset in assets)
      {
    if (asset is Sprite sprite)
       {
          sprites.Add(sprite);
      }
      }
        }
        
        // Sort by name
        sprites = sprites.OrderBy(s => s.name).ToList();
        
   return sprites;
    }

    private void SetupCharacterAnimations()
    {
      // Create or use existing GameObject
        GameObject character = targetCharacter;
        
        if (character == null)
     {
            character = new GameObject("Character");
          character.transform.position = Vector3.zero;
            Debug.Log("[AutoAnimationSetup] Created new GameObject: Character");
      }
      
        // Ensure SpriteRenderer exists
        SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = character.AddComponent<SpriteRenderer>();
   Debug.Log("[AutoAnimationSetup] Added SpriteRenderer");
        }
  
        // Set initial sprite
        if (idleSprites.Count > 0)
    {
       spriteRenderer.sprite = idleSprites[0];
        }
        else if (attackSprites.Count > 0)
        {
        spriteRenderer.sprite = attackSprites[0];
        }
    
    // Add CharacterAnimationController
        CharacterAnimationController animController = character.GetComponent<CharacterAnimationController>();
        if (animController == null)
{
            animController = character.AddComponent<CharacterAnimationController>();
         Debug.Log("[AutoAnimationSetup] Added CharacterAnimationController");
        }
 
        // Configure controller
        animController.idleSprites = idleSprites;
        animController.attackSprites = attackSprites;
animController.idleFrameRate = idleFPS;
        animController.attackFrameRate = attackFPS;
        animController.autoReturnToIdle = true;
  animController.returnToIdleDelay = 0.1f;
  
   // Add input handler if requested
        if (autoSetup)
        {
   CharacterAnimationInput inputHandler = character.GetComponent<CharacterAnimationInput>();
      if (inputHandler == null)
            {
             inputHandler = character.AddComponent<CharacterAnimationInput>();
       Debug.Log("[AutoAnimationSetup] Added CharacterAnimationInput");
            }
         
     inputHandler.animController = animController;
       inputHandler.attackKey = KeyCode.Space;
  inputHandler.enableMouseAttack = true;
        }
        
        // Select the character
        Selection.activeGameObject = character;
        
  // Mark as dirty to save changes
        EditorUtility.SetDirty(character);
      
     // Show success dialog
        EditorUtility.DisplayDialog(
            "Setup Complete!", 
            $"Character animations configured!\n\n" +
       $"Idle Sprites: {idleSprites.Count}\n" +
            $"Attack Sprites: {attackSprites.Count}\n\n" +
      $"Press Space or Click to attack in Play Mode!", 
            "Awesome!");
   
        Debug.Log($"[AutoAnimationSetup] ? Setup complete for: {character.name}");
    }
}
