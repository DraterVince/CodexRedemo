using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// Auto-creates Unity Animator Controller with Idle and Attack states
/// Matches your existing animation structure with parameters and transitions
/// </summary>
public class AutoAnimatorControllerSetup : EditorWindow
{
    private string searchPath = "Assets";
    private GameObject targetCharacter;
    private string characterName = "NewCharacter";
    
    // Found animations
    private AnimationClip idleClip;
    private AnimationClip attackClip;
    
    // Manual sprite assignment
    private List<Sprite> idleSprites = new List<Sprite>();
    private List<Sprite> attackSprites = new List<Sprite>();
    
    // Settings
    private int idleFPS = 8;
  private int attackFPS = 24;
    private float attackTransitionDuration = 0.1f;
    private bool hasExitTime = true;
    
    private bool searchComplete = false;
  private Vector2 scrollPos;
    
    [MenuItem("Tools/Auto Animator Controller Setup")]
    public static void ShowWindow()
    {
        var window = GetWindow<AutoAnimatorControllerSetup>("Animator Controller Setup");
        window.minSize = new Vector2(500, 700);
        window.Show();
    }
    
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        EditorGUILayout.LabelField("Unity Animator Controller Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Creates a Unity Animator Controller matching your existing structure:\n\n" +
      "? Animator Controller with Idle/Attack states\n" +
       "? Attack trigger parameter\n" +
       "? Automatic transitions\n" +
      "? Animation clips from sprites\n" +
"? Detects sprite sheets named 'idle' or 'attack'\n\n" +
     "Same style as your KnightAnimatorController!", 
      MessageType.Info);
        
      EditorGUILayout.Space();
        
   // Character name
     EditorGUILayout.LabelField("1. Character Setup", EditorStyles.boldLabel);
 EditorGUILayout.BeginVertical("box");
 characterName = EditorGUILayout.TextField("Character Name:", characterName);
 targetCharacter = (GameObject)EditorGUILayout.ObjectField(
    "Target GameObject (optional):", 
          targetCharacter, 
 typeof(GameObject), 
            true);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        // Search for animations or manual sprite assignment
   EditorGUILayout.LabelField("2. Animation Setup", EditorStyles.boldLabel);
        
      EditorGUILayout.BeginVertical("box");
 EditorGUILayout.LabelField("Option A: Search for Existing Animations", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        searchPath = EditorGUILayout.TextField("Search Path:", searchPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
   string path = EditorUtility.OpenFolderPanel("Select Animation Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
      {
                searchPath = "Assets" + path.Substring(Application.dataPath.Length);
            }
  }
 EditorGUILayout.EndHorizontal();
 
        if (GUILayout.Button("?? Search for Animations", GUILayout.Height(30)))
        {
            SearchForAnimations();
        }
     EditorGUILayout.EndVertical();
        
  EditorGUILayout.Space();
        
        // Manual sprite assignment
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Option B: Assign Sprites Manually", EditorStyles.boldLabel);
        
EditorGUILayout.LabelField($"Idle Sprites ({idleSprites.Count}):", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Idle Sprites"))
        {
       AddSprites(ref idleSprites);
        }
        if (idleSprites.Count > 0 && GUILayout.Button("Clear"))
        {
            idleSprites.Clear();
        }
        EditorGUILayout.EndHorizontal();
 
        if (idleSprites.Count > 0)
        {
            EditorGUILayout.LabelField($"  • {idleSprites.Count} sprites assigned", EditorStyles.miniLabel);
}
        
        EditorGUILayout.Space();
        
  EditorGUILayout.LabelField($"Attack Sprites ({attackSprites.Count}):", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Attack Sprites"))
        {
        AddSprites(ref attackSprites);
        }
 if (attackSprites.Count > 0 && GUILayout.Button("Clear"))
        {
            attackSprites.Clear();
        }
        EditorGUILayout.EndHorizontal();
        
        if (attackSprites.Count > 0)
    {
            EditorGUILayout.LabelField($"  • {attackSprites.Count} sprites assigned", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        // Show found animations
        if (searchComplete)
        {
        EditorGUILayout.LabelField("Found Animations:", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
  
       if (idleClip != null)
            {
      GUI.color = Color.green;
      EditorGUILayout.LabelField($"? Idle: {idleClip.name}");
     GUI.color = Color.white;
        }
  else
        {
    GUI.color = Color.yellow;
   EditorGUILayout.LabelField("? No Idle animation found");
      GUI.color = Color.white;
     }
 
            if (attackClip != null)
     {
           GUI.color = Color.green;
        EditorGUILayout.LabelField($"? Attack: {attackClip.name}");
       GUI.color = Color.white;
 }
            else
       {
      GUI.color = Color.yellow;
      EditorGUILayout.LabelField("? No Attack animation found");
 GUI.color = Color.white;
            }
          
            EditorGUILayout.EndVertical();
        }
        
  EditorGUILayout.Space();
        
        // Settings
        EditorGUILayout.LabelField("3. Animation Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        idleFPS = EditorGUILayout.IntSlider("Idle FPS:", idleFPS, 1, 60);
   attackFPS = EditorGUILayout.IntSlider("Attack FPS:", attackFPS, 1, 60);
     attackTransitionDuration = EditorGUILayout.Slider("Transition Duration:", attackTransitionDuration, 0f, 1f);
 hasExitTime = EditorGUILayout.Toggle("Has Exit Time (Attack->Idle):", hasExitTime);
EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
 // Create button
        GUI.backgroundColor = Color.green;
 bool canCreate = (!string.IsNullOrEmpty(characterName)) && 
         ((idleClip != null || attackClip != null) || 
    (idleSprites.Count > 0 || attackSprites.Count > 0));
        GUI.enabled = canCreate;
        
        if (GUILayout.Button("? Create Animator Controller!", GUILayout.Height(40)))
    {
 CreateAnimatorController();
        }
        
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        
   if (!canCreate)
        {
 EditorGUILayout.HelpBox(
   "Need: Character name + (Animations OR Sprites)", 
 MessageType.Warning);
   }
     
        EditorGUILayout.EndScrollView();
    }
    
    private void SearchForAnimations()
    {
idleClip = null;
        attackClip = null;
        searchComplete = false;
      
    // Search for existing animation clips
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { searchPath });
        
    foreach (string guid in guids)
        {
         string path = AssetDatabase.GUIDToAssetPath(guid);
         AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
         
            if (clip == null) continue;
            
    string clipName = clip.name.ToLower();
      
         // Look for idle
            if (clipName.Contains("idle") && idleClip == null)
            {
                idleClip = clip;
       }

            // Look for attack
     if (clipName.Contains("attack") && attackClip == null)
    {
 attackClip = clip;
            }
        }
        
   // NEW: If animations not found, search for sprite sheets named "idle" or "attack"
   if (idleSprites.Count == 0 || attackSprites.Count == 0)
        {
            SearchForSpriteSheets(searchPath);
        }
        
        searchComplete = true;
        Debug.Log($"[AutoAnimatorController] Search complete. Idle: {(idleClip != null ? idleClip.name : "Not found")}, Attack: {(attackClip != null ? attackClip.name : "Not found")}");
        Debug.Log($"[AutoAnimatorController] Sprites found - Idle: {idleSprites.Count}, Attack: {attackSprites.Count}");
    }
    
    private void SearchForSpriteSheets(string directory)
    {
        // Search for texture files named "idle" or "attack"
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { directory });
        
 foreach (string guid in textureGuids)
        {
       string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
  
            // Check for "idle" sprite sheet
   if (fileName == "idle" && idleSprites.Count == 0)
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
         foreach (Object asset in assets)
  {
     if (asset is Sprite sprite)
     {
            idleSprites.Add(sprite);
  }
          }
    
              if (idleSprites.Count > 0)
         {
   idleSprites = idleSprites.OrderBy(s => s.name).ToList();
     Debug.Log($"[AutoAnimatorController] Found Idle sprite sheet: {path} ({idleSprites.Count} sprites)");
             }
      }
            
            // Check for "attack" sprite sheet
            if (fileName == "attack" && attackSprites.Count == 0)
        {
          Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
   foreach (Object asset in assets)
     {
         if (asset is Sprite sprite)
       {
      attackSprites.Add(sprite);
          }
                }
         
  if (attackSprites.Count > 0)
   {
         attackSprites = attackSprites.OrderBy(s => s.name).ToList();
       Debug.Log($"[AutoAnimatorController] Found Attack sprite sheet: {path} ({attackSprites.Count} sprites)");
        }
 }
        }
        
      // Recursively search subdirectories
        string[] subFolders = AssetDatabase.GetSubFolders(directory);
        foreach (string subFolder in subFolders)
  {
            SearchForSpriteSheets(subFolder);
        }
    }
    
    private void AddSprites(ref List<Sprite> spriteList)
    {
        string path = EditorUtility.OpenFilePanel("Select Sprite Sheet", "Assets", "png,jpg,jpeg");
   
        if (string.IsNullOrEmpty(path)) return;
        
 if (path.StartsWith(Application.dataPath))
        {
    path = "Assets" + path.Substring(Application.dataPath.Length);
    }
        
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object asset in assets)
        {
        if (asset is Sprite sprite)
            {
spriteList.Add(sprite);
    }
        }
        
   spriteList = spriteList.OrderBy(s => s.name).ToList();
        
  Debug.Log($"Added {spriteList.Count} sprites");
    }
    
    private void CreateAnimatorController()
    {
        string animPath = "Assets/Animations";
        
    // Create Animations folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(animPath))
        {
        AssetDatabase.CreateFolder("Assets", "Animations");
        }
      
    string charPath = $"{animPath}/{characterName}";
  if (!AssetDatabase.IsValidFolder(charPath))
        {
            string[] parts = characterName.Split('/');
      string currentPath = animPath;
         foreach (string part in parts)
            {
       string newPath = $"{currentPath}/{part}";
     if (!AssetDatabase.IsValidFolder(newPath))
             {
       AssetDatabase.CreateFolder(currentPath, part);
                }
 currentPath = newPath;
        }
     charPath = currentPath;
        }
     
    // Create animation clips from sprites if needed
        if (idleClip == null && idleSprites.Count > 0)
        {
   idleClip = CreateAnimationClipFromSprites(idleSprites, $"{characterName}_Idle", charPath, idleFPS);
        }
        
    if (attackClip == null && attackSprites.Count > 0)
        {
       attackClip = CreateAnimationClipFromSprites(attackSprites, $"{characterName}_Attack", charPath, attackFPS);
        }
      
        // Create Animator Controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(
    $"{charPath}/{characterName}AnimatorController.controller");
        
        // Add "Attack" trigger parameter (matching your existing structure)
controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        
      // Get base layer
    AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine stateMachine = baseLayer.stateMachine;
        
        // Create Idle state
 AnimatorState idleState = null;
    if (idleClip != null)
        {
            idleState = stateMachine.AddState("Idle", new Vector3(330, 110, 0));
  idleState.motion = idleClip;
        }
        
        // Create Attack state
 AnimatorState attackState = null;
        if (attackClip != null)
        {
     attackState = stateMachine.AddState("Attack", new Vector3(600, 100, 0));
 attackState.motion = attackClip;
        }
        
        // Set default state
        if (idleState != null)
        {
     stateMachine.defaultState = idleState;
        }
        
    // Create transitions (matching your existing structure)
      if (idleState != null && attackState != null)
        {
          // Idle -> Attack (triggered by Attack parameter)
            AnimatorStateTransition idleToAttack = idleState.AddTransition(attackState);
 idleToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
       idleToAttack.hasExitTime = false;
            idleToAttack.duration = attackTransitionDuration;
  
     // Attack -> Idle (automatic after attack finishes)
  AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = hasExitTime;
     attackToIdle.exitTime = 0.9f;
  attackToIdle.duration = attackTransitionDuration;
        }
      
    // Save controller
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
      // Setup character GameObject
  GameObject character = targetCharacter;
        if (character == null)
        {
            character = new GameObject(characterName);
   character.transform.position = Vector3.zero;
        }
        
        // Add/Get SpriteRenderer
        SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = character.AddComponent<SpriteRenderer>();
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
        
  // Add/Get Animator
     Animator animator = character.GetComponent<Animator>();
        if (animator == null)
        {
    animator = character.AddComponent<Animator>();
        }
        
        animator.runtimeAnimatorController = controller;
        
        // Select objects
   Selection.activeObject = controller;
      Selection.activeGameObject = character;
        
        EditorUtility.DisplayDialog(
            "Success!", 
            $"Animator Controller created!\n\n" +
            $"Location: {charPath}/{characterName}AnimatorController.controller\n\n" +
        $"Idle: {(idleClip != null ? "?" : "?")}\n" +
            $"Attack: {(attackClip != null ? "?" : "?")}\n\n" +
            $"Use animator.SetTrigger(\"Attack\") to trigger attacks!", 
        "Awesome!");
      
      Debug.Log($"[AutoAnimatorController] ? Created: {charPath}/{characterName}AnimatorController.controller");
    }
    
    private AnimationClip CreateAnimationClipFromSprites(List<Sprite> sprites, string clipName, string path, int fps)
    {
        AnimationClip clip = new AnimationClip();
  clip.frameRate = fps;
        
 EditorCurveBinding spriteBinding = new EditorCurveBinding();
    spriteBinding.type = typeof(SpriteRenderer);
     spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";
        
   ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
   spriteKeyFrames[i] = new ObjectReferenceKeyframe();
            spriteKeyFrames[i].time = i / (float)fps;
      spriteKeyFrames[i].value = sprites[i];
        }
    
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyFrames);
        
     // Set loop
        AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = clipName.ToLower().Contains("idle");
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
      
        string clipPath = $"{path}/{clipName}.anim";
        AssetDatabase.CreateAsset(clip, clipPath);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"[AutoAnimatorController] Created animation clip: {clipPath}");
        
        return clip;
    }
}
