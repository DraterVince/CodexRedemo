using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Organizes animations into Assets/Animations/[CharacterName]/ folders
/// Creates proper folder structure for each character's animations
/// Supports numbered animations like attack1, attack_1, idle2, etc.
/// </summary>
public class AnimationFolderOrganizer : EditorWindow
{
    private string sourceFolder = "Assets/AssestsForGame/Player Sprites";
    private string destinationFolder = "Assets/Animations";
    private bool copyInsteadOfMove = true;
    private bool createSubfolders = true;
    private Vector2 scrollPos;
    
    private Dictionary<string, CharacterAnimationSet> detectedCharacters = new Dictionary<string, CharacterAnimationSet>();
    private bool scanComplete = false;
    
    [System.Serializable]
    public class CharacterAnimationSet
    {
        public string characterName;
        public string sourcePath;
        public Dictionary<string, List<Sprite>> animations = new Dictionary<string, List<Sprite>>();
        public bool selected = true;
    }
 
    // Base animation names to search for (without numbers)
    private static readonly string[] BASE_ANIMATION_NAMES = new string[]
    {
        "idle", "attack", "walk", "run", "jump", "fall", "death", "die",
        "hurt", "hit", "damage", "dodge", "roll", "block", "cast", "spell",
        "shoot", "reload", "climb", "crouch", "slide", "dash", "skill",
   "charge", "defend", "heal", "surf", "tumble", "take_hit", "air_atk"
    };
    
    // Special animation names that might have numbers or underscores
    private static readonly string[] SPECIAL_PATTERNS = new string[]
    {
"atk", "j_up", "j_down", "sp_atk"
    };
 
    [MenuItem("Tools/Organize Animations into Folders")]
    public static void ShowWindow()
    {
        var window = GetWindow<AnimationFolderOrganizer>("Animation Organizer");
        window.minSize = new Vector2(600, 700);
        window.Show();
    }
    
    private void OnGUI()
    {
   scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
  
        EditorGUILayout.LabelField("Animation Folder Organizer", EditorStyles.boldLabel);
   EditorGUILayout.Space();
        
   EditorGUILayout.HelpBox(
   "This tool will:\n" +
    "1. Scan for all character sprite folders\n" +
      "2. Detect all animations for each character\n" +
    "3. Create organized folders: Assets/Animations/[CharacterName]/[AnimationType]/\n" +
            "4. Copy or move sprites to the organized structure\n\n" +
            "Example result:\n" +
            "  Assets/Animations/Knight/Idle/\n" +
       "  Assets/Animations/Knight/Attack/\n" +
  "  Assets/Animations/MartialHero/Idle/\n" +
            "  Assets/Animations/MartialHero/Attack/\n\n" +
"? Now supports numbered animations:\n" +
            "  attack1, attack_1, idle2, idle_2, etc.", 
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        // Settings
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
   sourceFolder = EditorGUILayout.TextField("Source Folder:", sourceFolder);
 if (GUILayout.Button("Browse", GUILayout.Width(80)))
      {
    string path = EditorUtility.OpenFolderPanel("Select Source Folder", "Assets", "");
       if (!string.IsNullOrEmpty(path))
       {
                if (path.StartsWith(Application.dataPath))
                {
        sourceFolder = "Assets" + path.Substring(Application.dataPath.Length);
}
            }
 }
        EditorGUILayout.EndHorizontal();
        
    EditorGUILayout.BeginHorizontal();
        destinationFolder = EditorGUILayout.TextField("Destination Folder:", destinationFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
     string path = EditorUtility.OpenFolderPanel("Select Destination Folder", "Assets", "");
 if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
     {
      destinationFolder = "Assets" + path.Substring(Application.dataPath.Length);
      }
      }
  }
        EditorGUILayout.EndHorizontal();
      
   copyInsteadOfMove = EditorGUILayout.Toggle("Copy (keep originals)", copyInsteadOfMove);
        createSubfolders = EditorGUILayout.Toggle("Create Animation Subfolders", createSubfolders);
        
        EditorGUILayout.EndVertical();
   
   EditorGUILayout.Space();
    
        // Scan button
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("?? Scan for Characters & Animations", GUILayout.Height(35)))
        {
    ScanForCharacters();
     }
        GUI.backgroundColor = Color.white;
      
        EditorGUILayout.Space();
        
        // Display results
        if (scanComplete)
        {
            EditorGUILayout.LabelField($"?? Scan Results: {detectedCharacters.Count} Characters Found", EditorStyles.boldLabel);
            
            if (detectedCharacters.Count == 0)
            {
                EditorGUILayout.HelpBox("No characters found! Try a different source folder.", MessageType.Warning);
    }
     else
     {
     EditorGUILayout.BeginVertical("box");
    
         foreach (var kvp in detectedCharacters.OrderBy(x => x.Key))
     {
     CharacterAnimationSet character = kvp.Value;
    
   EditorGUILayout.BeginVertical("box");
       
    // Character header
    EditorGUILayout.BeginHorizontal();
             character.selected = EditorGUILayout.Toggle(character.selected, GUILayout.Width(20));
 
         GUI.color = character.selected ? Color.white : Color.gray;
  EditorGUILayout.LabelField($"?? {character.characterName}", EditorStyles.boldLabel);
              GUI.color = Color.white;
    
         EditorGUILayout.LabelField($"({character.animations.Count} animations)", GUILayout.Width(120));
         EditorGUILayout.EndHorizontal();
          
     EditorGUILayout.LabelField($"   ?? {character.sourcePath}", EditorStyles.miniLabel);
    
   // Show animations
             if (character.animations.Count > 0)
      {
       string animList = "   Animations: ";
     animList += string.Join(", ", character.animations.Keys.Select(k => k.ToUpper()));
   EditorGUILayout.LabelField(animList, EditorStyles.miniLabel);
        
      // Show sprite counts
              string spriteCounts = "   Frames: ";
             spriteCounts += string.Join(", ", character.animations.Select(a => $"{a.Key}({a.Value.Count})"));
       EditorGUILayout.LabelField(spriteCounts, EditorStyles.miniLabel);
         }
               
        // Show destination preview
            string destPath = $"{destinationFolder}/{SanitizeFolderName(character.characterName)}";
     EditorGUILayout.LabelField($"   ? Destination: {destPath}/", EditorStyles.miniLabel);
        
              EditorGUILayout.EndVertical();
    EditorGUILayout.Space(5);
      }
         
      EditorGUILayout.EndVertical();
    
         EditorGUILayout.Space();
  
                // Organization controls
          EditorGUILayout.BeginVertical("box");
   EditorGUILayout.LabelField("Organization Options", EditorStyles.boldLabel);
          
       EditorGUILayout.BeginHorizontal();
       if (GUILayout.Button("Select All"))
  {
       foreach (var character in detectedCharacters.Values)
   character.selected = true;
    }
           if (GUILayout.Button("Deselect All"))
   {
       foreach (var character in detectedCharacters.Values)
   character.selected = false;
        }
                EditorGUILayout.EndHorizontal();
   
  EditorGUILayout.EndVertical();
            
EditorGUILayout.Space();
       
     // Organize button
      int selectedCount = detectedCharacters.Values.Count(c => c.selected);
      GUI.enabled = selectedCount > 0;
                
    GUI.backgroundColor = Color.green;
 string buttonText = copyInsteadOfMove 
      ? $"?? Copy {selectedCount} Character(s) to Organized Folders" 
: $"?? Move {selectedCount} Character(s) to Organized Folders";
       
      if (GUILayout.Button(buttonText, GUILayout.Height(40)))
            {
                 OrganizeAnimations();
                }
           GUI.backgroundColor = Color.white;
         GUI.enabled = true;
            }
        }
   
        EditorGUILayout.Space();
    
        // Info box
        EditorGUILayout.BeginVertical("box");
     EditorGUILayout.LabelField("?? Tips:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("• Use 'Copy' to keep original files safe", EditorStyles.miniLabel);
   EditorGUILayout.LabelField("• Use 'Move' to clean up and reorganize", EditorStyles.miniLabel);
     EditorGUILayout.LabelField("• Subfolders organize by animation type (Idle/, Attack/, etc.)", EditorStyles.miniLabel);
        EditorGUILayout.LabelField("• Existing files will be skipped (no overwrites)", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
        
      EditorGUILayout.EndScrollView();
    }
    
  private void ScanForCharacters()
    {
        detectedCharacters.Clear();
        scanComplete = false;
      
        if (!AssetDatabase.IsValidFolder(sourceFolder))
        {
EditorUtility.DisplayDialog("Error", $"Source folder does not exist:\n{sourceFolder}", "OK");
            return;
        }
        
        Debug.Log($"[AnimationOrganizer] Scanning: {sourceFolder}");
        
        // Get all subdirectories in the source folder
        string[] characterFolders = AssetDatabase.GetSubFolders(sourceFolder);
        
 foreach (string characterFolder in characterFolders)
        {
            string characterName = Path.GetFileName(characterFolder);
            
   // Skip non-character folders
          if (characterName.StartsWith(".") || characterName == "Sprites")
                continue;
 
         // Create character animation set
  CharacterAnimationSet character = new CharacterAnimationSet
   {
    characterName = characterName,
      sourcePath = characterFolder,
    selected = true
            };
    
            // Scan for animations in this character folder
      ScanCharacterFolder(characterFolder, character);
            
   if (character.animations.Count > 0)
          {
     detectedCharacters[characterName] = character;
    Debug.Log($"[AnimationOrganizer] Found character: {characterName} with {character.animations.Count} animations");
            }
  }

        scanComplete = true;
        Debug.Log($"[AnimationOrganizer] Scan complete! Found {detectedCharacters.Count} characters");
    }
    
    private void ScanCharacterFolder(string folderPath, CharacterAnimationSet character)
    {
        // Check for animation subfolders first
        string[] subFolders = AssetDatabase.GetSubFolders(folderPath);
        foreach (string subFolder in subFolders)
     {
 string folderName = Path.GetFileName(subFolder).ToLower();
  
            // Try to match animation name (with number support)
      string detectedAnimName = DetectAnimationName(folderName);
    if (!string.IsNullOrEmpty(detectedAnimName) && !character.animations.ContainsKey(detectedAnimName))
            {
 List<Sprite> sprites = LoadSpritesFromFolder(subFolder);
       if (sprites.Count > 0)
  {
          character.animations[detectedAnimName] = sprites;
    }
      }
    }

        // Also check for sprite sheets in the main folder
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
        foreach (string guid in textureGuids)
        {
       string path = AssetDatabase.GUIDToAssetPath(guid);
      string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
 
     // Try to match animation name (with number support)
            string detectedAnimName = DetectAnimationName(fileName);
            if (!string.IsNullOrEmpty(detectedAnimName) && !character.animations.ContainsKey(detectedAnimName))
  {
                List<Sprite> sprites = LoadSpritesFromSpriteSheet(path);
       if (sprites.Count > 0)
     {
     character.animations[detectedAnimName] = sprites;
     }
 }
        }
      
 // Recursively scan subfolders
        foreach (string subFolder in subFolders)
        {
 ScanCharacterFolder(subFolder, character);
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
    
    private void OrganizeAnimations()
    {
        int totalCopied = 0;
        int totalFolders = 0;
        List<string> createdFolders = new List<string>();
        
        // Ensure destination folder exists
     if (!AssetDatabase.IsValidFolder(destinationFolder))
        {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, destinationFolder.Substring("Assets/".Length)));
     AssetDatabase.Refresh();
        }
        
      foreach (var kvp in detectedCharacters)
        {
            CharacterAnimationSet character = kvp.Value;
            if (!character.selected) continue;
       
          string characterFolderName = SanitizeFolderName(character.characterName);
            string characterDestPath = $"{destinationFolder}/{characterFolderName}";
      
            // Create character folder
            if (!AssetDatabase.IsValidFolder(characterDestPath))
        {
        AssetDatabase.CreateFolder(destinationFolder, characterFolderName);
      createdFolders.Add(characterDestPath);
                totalFolders++;
    }
          
    // Process each animation
  foreach (var animKvp in character.animations)
    {
       string animName = animKvp.Key;
         List<Sprite> sprites = animKvp.Value;
                
       string animFolderName = char.ToUpper(animName[0]) + animName.Substring(1);
          string animDestPath = createSubfolders 
 ? $"{characterDestPath}/{animFolderName}" 
 : characterDestPath;
            
          // Create animation subfolder if needed
   if (createSubfolders && !AssetDatabase.IsValidFolder(animDestPath))
          {
         AssetDatabase.CreateFolder(characterDestPath, animFolderName);
   createdFolders.Add(animDestPath);
  totalFolders++;
         }
           
   // Copy/move sprites
         foreach (Sprite sprite in sprites)
             {
     string sourcePath = AssetDatabase.GetAssetPath(sprite);
  string fileName = Path.GetFileName(sourcePath);
        string destPath = $"{animDestPath}/{fileName}";
        
    // Skip if already exists
       if (File.Exists(Path.Combine(Application.dataPath, destPath.Substring("Assets/".Length))))
        {
    continue;
  }
            
       if (copyInsteadOfMove)
         {
        AssetDatabase.CopyAsset(sourcePath, destPath);
           }
           else
    {
              AssetDatabase.MoveAsset(sourcePath, destPath);
 }
     
             totalCopied++;
          }
     }
        }
  
        AssetDatabase.Refresh();
        
        // Show success message
        string action = copyInsteadOfMove ? "copied" : "moved";
        string message = $"Organization complete!\n\n" +
    $"? Created {totalFolders} folders\n" +
                 $"? {action.ToUpper()} {totalCopied} sprite files\n\n" +
   $"Organized folders:\n";
        
 foreach (string folder in createdFolders.Take(5))
        {
      message += $"  • {folder}\n";
        }
        
   if (createdFolders.Count > 5)
        {
            message += $"  ... and {createdFolders.Count - 5} more";
        }
        
   EditorUtility.DisplayDialog("Success!", message, "Awesome!");
        
 Debug.Log($"[AnimationOrganizer] ? Organization complete! Created {totalFolders} folders, {action} {totalCopied} files");
        
        // Rescan to show new structure
        ScanForCharacters();
    }
 
    private string SanitizeFolderName(string name)
    {
     // Remove invalid characters and clean up name
        name = name.Trim();
        
        // Remove common suffixes
        name = name.Replace("(Default skin)", "")
            .Replace("(200 gems)", "")
.Replace("(300 gems)", "")
     .Replace("(600 gems)", "")
.Replace("FREE_v1.1", "")
       .Replace("Pack 2", "")
                   .Trim();
        
      // Remove special characters
   foreach (char c in Path.GetInvalidFileNameChars())
        {
       name = name.Replace(c.ToString(), "");
        }
     
        // Replace spaces with underscores
        name = name.Replace(" ", "_");
        
   // Remove multiple underscores
     while (name.Contains("__"))
{
      name = name.Replace("__", "_");
        }
        
        return name.Trim('_');
    }
}
