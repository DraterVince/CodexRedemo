using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Auto-detects ALL sprite animations from folder structure or sprite sheet names
/// Looks for any common animation names: idle, attack, walk, run, jump, death, etc.
/// Supports numbered animations: idle1, idle_1, attack2, attack_2, etc.
/// </summary>
public class AutoMultiAnimationSetup : EditorWindow
{
    private string spriteFolderPath = "Assets";
    private GameObject targetCharacter;
    private bool searchComplete = false;
  
  // Dictionary to hold all found animations
    private Dictionary<string, AnimationInfo> foundAnimations = new Dictionary<string, AnimationInfo>();
    
  [System.Serializable]
    public class AnimationInfo
    {
        public string name;
     public List<Sprite> sprites;
        public string sourcePath;
        public int defaultFPS;
   public bool loop;
     public bool pingPong;
  }
    
  // Base animation names to search for (without numbers)
    private static readonly string[] BASE_ANIMATION_NAMES = new string[]
    {
        "idle", "attack", "walk", "run", "jump", "fall", "death", "die",
"hurt", "hit", "damage", "dodge", "roll", "block", "cast", "spell",
        "shoot", "reload", "climb", "crouch", "slide", "dash", "skill",
        "charge", "defend", "heal", "surf", "tumble", "take_hit", "air_atk"
    };
    
    // Special animation patterns
    private static readonly string[] SPECIAL_PATTERNS = new string[]
    {
     "atk", "j_up", "j_down", "sp_atk"
    };
    
    // Default FPS settings for different animation types (base names)
    private Dictionary<string, int> defaultFPS = new Dictionary<string, int>
    {
      {"idle", 8}, {"walk", 12}, {"run", 16}, {"attack", 24},
        {"jump", 12}, {"fall", 10}, {"death", 12}, {"hurt", 16},
 {"dodge", 20}, {"roll", 18}, {"cast", 15}, {"shoot", 20},
        {"heal", 12}, {"surf", 14}, {"tumble", 18}
  };
    
    // Which animations should loop (base names)
    private static readonly HashSet<string> LOOPING_ANIMATIONS = new HashSet<string>
  {
   "idle", "walk", "run", "climb"
    };
    
    // Which animations should ping-pong (base names)
    private static readonly HashSet<string> PINGPONG_ANIMATIONS = new HashSet<string>
    {
        "idle"
    };
    
    private Vector2 scrollPos;
    
  [MenuItem("Tools/Auto Multi-Animation Setup")]
    public static void ShowWindow()
    {
        var window = GetWindow<AutoMultiAnimationSetup>("Multi-Animation Setup");
   window.minSize = new Vector2(550, 700);
      window.Show();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
  
        EditorGUILayout.LabelField("Auto Multi-Animation Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Automatically finds ALL animations in your project!\n\n" +
     "Searches for:\n" +
        "  • Folders: Idle/, Attack/, Walk/, Run/, Jump/, Death/, etc.\n" +
 "  • Sprite Sheets: idle.png, attack.png, walk.png, etc.\n\n" +
"Supports: " + string.Join(", ", BASE_ANIMATION_NAMES.Take(10)) + "\n" +
   "...and " + (BASE_ANIMATION_NAMES.Length - 10) + " more!\n\n" +
   "All case-insensitive!", 
            MessageType.Info);
     
        EditorGUILayout.Space();
        
        // Folder path selection
  EditorGUILayout.LabelField("1. Select Search Folder", EditorStyles.boldLabel);
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
     GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("?? Search for ALL Animations", GUILayout.Height(35)))
   {
     SearchForAllAnimations();
     }
        GUI.backgroundColor = Color.white;
 
        EditorGUILayout.Space();
        
        // Show search results
        if (searchComplete)
        {
         EditorGUILayout.LabelField($"Search Results: {foundAnimations.Count} Animations Found", EditorStyles.boldLabel);

            if (foundAnimations.Count == 0)
            {
   EditorGUILayout.HelpBox("No animations found! Try a different folder.", MessageType.Warning);
}
 else
    {
           // Display each found animation
   EditorGUILayout.BeginVertical("box");
       
         foreach (var kvp in foundAnimations.OrderBy(x => x.Key))
   {
         AnimationInfo info = kvp.Value;
    
                  EditorGUILayout.BeginHorizontal();
  
        // Animation name with color coding
 GUI.color = info.sprites.Count > 0 ? Color.green : Color.red;
            EditorGUILayout.LabelField($"? {info.name.ToUpper()}", EditorStyles.boldLabel, GUILayout.Width(100));
            GUI.color = Color.white;
    
      EditorGUILayout.LabelField($"{info.sprites.Count} frames", GUILayout.Width(80));
        EditorGUILayout.LabelField($"@ {info.defaultFPS} fps", GUILayout.Width(70));
   
        if (info.loop)
          EditorGUILayout.LabelField("?? Loop", GUILayout.Width(60));
    if (info.pingPong)
          EditorGUILayout.LabelField("? Ping-Pong", GUILayout.Width(80));
         
        EditorGUILayout.EndHorizontal();
        
          EditorGUILayout.LabelField($"   ?? {info.sourcePath}", EditorStyles.miniLabel);
      
        // Show sprite names (first few)
            if (info.sprites.Count > 0)
          {
              string spriteNames = " Sprites: ";
     for (int i = 0; i < Mathf.Min(3, info.sprites.Count); i++)
          {
             spriteNames += info.sprites[i].name;
   if (i < info.sprites.Count - 1 && i < 2)
    spriteNames += ", ";
               }
             if (info.sprites.Count > 3)
  spriteNames += $"... +{info.sprites.Count - 3} more";
   
      EditorGUILayout.LabelField(spriteNames, EditorStyles.miniLabel);
                    }
               
  EditorGUILayout.Space(5);
     }
       
    EditorGUILayout.EndVertical();
          
     EditorGUILayout.Space();
   
           // Setup section
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
   
           EditorGUILayout.EndVertical();
            
    EditorGUILayout.Space();
  
     // Setup button
      GUI.backgroundColor = Color.green;
                if (GUILayout.Button("? Setup Multi-Animation Controller!", GUILayout.Height(40)))
    {
             SetupMultiAnimationController();
     }
       GUI.backgroundColor = Color.white;
          }
        }
   
        EditorGUILayout.Space();
        
    // Statistics box
      EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("?? Animation Type Support:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Searching for {BASE_ANIMATION_NAMES.Length} animation types:", EditorStyles.miniLabel);
      
        string animList = "  ";
        for (int i = 0; i < BASE_ANIMATION_NAMES.Length; i++)
        {
        animList += BASE_ANIMATION_NAMES[i];
            if (i < BASE_ANIMATION_NAMES.Length - 1)
       animList += ", ";
       if ((i + 1) % 8 == 0)
     animList += "\n  ";
        }
        EditorGUILayout.LabelField(animList, EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void SearchForAllAnimations()
    {
        foundAnimations.Clear();
    searchComplete = false;
        
  Debug.Log($"[AutoMultiAnimationSetup] Searching in: {spriteFolderPath}");
        
        // Search recursively
        SearchDirectory(spriteFolderPath);

        searchComplete = true;
   
   Debug.Log($"[AutoMultiAnimationSetup] Search complete! Found {foundAnimations.Count} animations");
    }
    
 private void SearchDirectory(string directory)
  {
  // Check current directory name for animation match
     string folderName = Path.GetFileName(directory).ToLower();
  string detectedAnim = DetectAnimationName(folderName);
       
        if (!string.IsNullOrEmpty(detectedAnim) && !foundAnimations.ContainsKey(detectedAnim))
        {
    List<Sprite> sprites = LoadSpritesFromFolder(directory);
       if (sprites.Count > 0)
            {
  AddAnimation(detectedAnim, sprites, directory);
      }
        }
      
        // Check for sprite sheets in this directory
    SearchForSpriteSheets(directory);
        
 // Recursively search subdirectories
      string[] subFolders = AssetDatabase.GetSubFolders(directory);
        foreach (string subFolder in subFolders)
     {
 SearchDirectory(subFolder);
      }
    }
    
    private void SearchForSpriteSheets(string directory)
    {
 string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { directory });
    
        foreach (string guid in textureGuids)
     {
     string path = AssetDatabase.GUIDToAssetPath(guid);
      string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
     
      // Try to detect animation name (with number support)
       string detectedAnim = DetectAnimationName(fileName);
       
            if (!string.IsNullOrEmpty(detectedAnim) && !foundAnimations.ContainsKey(detectedAnim))
       {
    List<Sprite> sprites = LoadSpritesFromSpriteSheet(path);
     if (sprites.Count > 0)
             {
          AddAnimation(detectedAnim, sprites, path);
      }
         }
    }
    }
    
    /// <summary>
    /// Detects animation name from folder/file name
    /// Supports: idle, idle1, idle_1, idle_2, attack, attack1, attack_1, etc.
    /// </summary>
    private string DetectAnimationName(string name)
    {
   name = name.ToLower().Trim();
        
 // First check for exact matches with base animation names
      foreach (string baseAnim in BASE_ANIMATION_NAMES)
        {
    // Check for exact match (e.g., "idle", "attack")
       if (name == baseAnim)
   return baseAnim;
         
// Check for numbered variations
   // Pattern: idle1, idle_1, idle_2, attack1, attack_1, etc.
       if (Regex.IsMatch(name, $"^{baseAnim}_?\\d+$"))
 {
     // Extract the full name including number
      Match match = Regex.Match(name, $"^({baseAnim}_?\\d+)");
   if (match.Success)
          return match.Groups[1].Value.Replace("_", ""); // Normalize: idle_1 -> idle1
  }
          
      // Check if name contains the animation type anywhere
if (name.Contains(baseAnim))
       {
     // Check if it's followed by a number
      string pattern = baseAnim + @"_?(\d+)";
    Match match = Regex.Match(name, pattern);
           if (match.Success)
         {
     return baseAnim + match.Groups[1].Value; // e.g., attack1, idle2
  }
    
       // If no number, return base animation name
       return baseAnim;
     }
        }
        
        // Check for special patterns (2_atk, 3_atk, sp_atk, etc.)
  foreach (string special in SPECIAL_PATTERNS)
 {
      if (name.Contains(special))
   {
         return name; // Return the full special pattern name
  }
        }
        
 return null; // No match found
    }
    
  /// <summary>
    /// Gets the base animation name from a potentially numbered animation
    /// e.g., "attack1" -> "attack", "idle_2" -> "idle"
    /// </summary>
  private string GetBaseAnimationName(string animName)
    {
        foreach (string baseAnim in BASE_ANIMATION_NAMES)
      {
   if (animName.ToLower().StartsWith(baseAnim.ToLower()))
return baseAnim;
        }
   return animName; // Return as-is if no base found
    }
    
    private void AddAnimation(string name, List<Sprite> sprites, string sourcePath)
    {
  string key = name.ToLower();
        
   if (foundAnimations.ContainsKey(key))
          return;
      
    // Get base animation name for settings lookup
     string baseName = GetBaseAnimationName(name);
    
   int fps = defaultFPS.ContainsKey(baseName) ? defaultFPS[baseName] : 12;
     bool loop = LOOPING_ANIMATIONS.Contains(baseName);
        bool pingPong = PINGPONG_ANIMATIONS.Contains(baseName);
      
        AnimationInfo info = new AnimationInfo
 {
  name = name,
      sprites = sprites,
      sourcePath = sourcePath,
    defaultFPS = fps,
   loop = loop,
   pingPong = pingPong
 };
        
 foundAnimations[key] = info;
        
        Debug.Log($"[AutoMultiAnimationSetup] Found {name}: {sprites.Count} sprites @ {fps}fps (Loop: {loop})");
    }

    private List<Sprite> LoadSpritesFromFolder(string folderPath)
    {
        List<Sprite> sprites = new List<Sprite>();
    string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
 
        foreach (string guid in guids)
        {
      string path = AssetDatabase.GUIDToAssetPath(guid);
       Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
          {
     if (asset is Sprite sprite)
    {
           sprites.Add(sprite);
         }
        }
        }
        
 return sprites.OrderBy(s => s.name).ToList();
    }
    
    private List<Sprite> LoadSpritesFromSpriteSheet(string texturePath)
    {
        List<Sprite> sprites = new List<Sprite>();
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
   
        foreach (Object asset in assets)
 {
          if (asset is Sprite sprite)
        {
      sprites.Add(sprite);
        }
        }
        
        return sprites.OrderBy(s => s.name).ToList();
    }
    
    private void SetupMultiAnimationController()
    {
 // Create or use existing GameObject
        GameObject character = targetCharacter;
    
        if (character == null)
  {
       character = new GameObject("Character");
            character.transform.position = Vector3.zero;
            Debug.Log("[AutoMultiAnimationSetup] Created new GameObject: Character");
        }
        
     // Ensure SpriteRenderer exists
        SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
    if (spriteRenderer == null)
  {
  spriteRenderer = character.AddComponent<SpriteRenderer>();
            Debug.Log("[AutoMultiAnimationSetup] Added SpriteRenderer");
        }
        
        // Set initial sprite (prefer idle)
        if (foundAnimations.ContainsKey("idle") && foundAnimations["idle"].sprites.Count > 0)
        {
            spriteRenderer.sprite = foundAnimations["idle"].sprites[0];
   }
        else if (foundAnimations.Count > 0)
    {
            var firstAnim = foundAnimations.Values.First();
 if (firstAnim.sprites.Count > 0)
   spriteRenderer.sprite = firstAnim.sprites[0];
        }
        
   // Add MultiAnimationController
      MultiAnimationController controller = character.GetComponent<MultiAnimationController>();
        if (controller == null)
        {
       controller = character.AddComponent<MultiAnimationController>();
 Debug.Log("[AutoMultiAnimationSetup] Added MultiAnimationController");
        }
        
  // Clear existing animations
        controller.animations.Clear();
        
        // Add all found animations
        foreach (var kvp in foundAnimations)
      {
            var info = kvp.Value;
   
          var animData = new MultiAnimationController.AnimationData
         {
        animationName = info.name,
        sprites = info.sprites,
frameRate = info.defaultFPS,
          loop = info.loop,
pingPong = info.pingPong
      };
            
            controller.animations.Add(animData);
        }
        
        // Set default animation (prefer idle)
 controller.defaultAnimation = foundAnimations.ContainsKey("idle") ? "idle" : foundAnimations.Keys.First();
        controller.autoReturnToDefault = true;
    controller.returnDelay = 0.1f;
  
        // Select the character
        Selection.activeGameObject = character;
        
        // Mark as dirty to save changes
        EditorUtility.SetDirty(character);
        
    // Show success dialog
        string animList = string.Join("\n", foundAnimations.Keys.Select(k => $"  • {k.ToUpper()}"));
        
EditorUtility.DisplayDialog(
            "Setup Complete!", 
 $"Multi-Animation Controller configured!\n\n" +
       $"Found {foundAnimations.Count} animations:\n{animList}\n\n" +
        $"Use PlayAnimation(\"name\") to switch animations!", 
 "Awesome!");
        
        Debug.Log($"[AutoMultiAnimationSetup] ? Setup complete for: {character.name}");
  }
}
