using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Utility to automatically setup the Confetti sprite sheet for animation
/// </summary>
public class ConfettiSpriteSheetSetup : EditorWindow
{
    private Texture2D confettiTexture;
    private int rows = 1;
    private int columns = 8;
    private int padding = 10;
    private bool createGameObject = true;
    private int frameRate = 24;
    
    [MenuItem("Tools/Setup Confetti Animation")]
    public static void ShowWindow()
    {
        var window = GetWindow<ConfettiSpriteSheetSetup>("Confetti Setup");
     window.Show();
    }
    
    private void OnEnable()
    {
     // Try to auto-load the confetti texture
        confettiTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AssestsForGame/Confetti.png");
    }
    
    private void OnGUI()
    {
  EditorGUILayout.LabelField("Confetti Sprite Sheet Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
confettiTexture = (Texture2D)EditorGUILayout.ObjectField("Confetti Texture", confettiTexture, typeof(Texture2D), false);
   
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Sprite Sheet Layout", EditorStyles.boldLabel);
     rows = EditorGUILayout.IntField("Rows", rows);
   columns = EditorGUILayout.IntField("Columns", columns);
        padding = EditorGUILayout.IntField("Padding (pixels)", padding);
        
      EditorGUILayout.Space();
     EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
  frameRate = EditorGUILayout.IntSlider("Frame Rate", frameRate, 1, 60);
        createGameObject = EditorGUILayout.Toggle("Create GameObject", createGameObject);
        
        EditorGUILayout.Space();
        
  GUI.enabled = confettiTexture != null;

        if (GUILayout.Button("Setup Confetti Animation", GUILayout.Height(40)))
        {
    SetupConfettiSpriteSheet();
     }
        
        GUI.enabled = true;
     
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This tool will:\n" +
    "1. Configure the sprite sheet with proper padding\n" +
         "2. Slice it into individual sprites\n" +
            "3. Optionally create a GameObject with SpriteAnimator\n" +
       "4. Setup the animation ready to play!", 
            MessageType.Info);
    }
    
    private void SetupConfettiSpriteSheet()
    {
    if (confettiTexture == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a confetti texture first!", "OK");
       return;
        }
        
        string path = AssetDatabase.GetAssetPath(confettiTexture);
     
        // Step 1: Configure texture import settings
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
    EditorUtility.DisplayDialog("Error", "Could not get texture importer!", "OK");
 return;
        }
        
        // Configure import settings
        importer.textureType = TextureImporterType.Sprite;
    importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        
     // Step 2: Slice the sprite sheet with padding
        int spriteWidth = (confettiTexture.width - (padding * (columns + 1))) / columns;
      int spriteHeight = (confettiTexture.height - (padding * (rows + 1))) / rows;
        
        List<SpriteMetaData> spriteSheet = new List<SpriteMetaData>();
     
  int spriteIndex = 0;
        for (int row = 0; row < rows; row++)
        {
  for (int col = 0; col < columns; col++)
       {
        SpriteMetaData sprite = new SpriteMetaData();
sprite.name = $"Confetti_{spriteIndex:D2}";

    // Calculate position with padding
       int x = padding + (col * (spriteWidth + padding));
                int y = confettiTexture.height - padding - ((row + 1) * (spriteHeight + padding));
    
                sprite.rect = new Rect(x, y, spriteWidth, spriteHeight);
 sprite.alignment = (int)SpriteAlignment.Center;
                sprite.pivot = new Vector2(0.5f, 0.5f);
       
  spriteSheet.Add(sprite);
     spriteIndex++;
       }
    }
        
        // Suppress obsolete API warning - this is an editor-only script
        #pragma warning disable CS0618
        importer.spritesheet = spriteSheet.ToArray();
        #pragma warning restore CS0618
        
        // Apply import settings
      EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
  
        // Wait for import to complete
        AssetDatabase.Refresh();
     
        Debug.Log($"[Confetti Setup] Sprite sheet configured with {spriteSheet.Count} sprites!");
        
    // Step 3: Create GameObject with animation (if requested)
        if (createGameObject)
        {
 CreateConfettiGameObject(path);
    }
        
        EditorUtility.DisplayDialog("Success", 
            $"Confetti sprite sheet setup complete!\n\n" +
         $"Sprites: {spriteSheet.Count}\n" +
 $"Frame Rate: {frameRate} FPS\n" +
         (createGameObject ? "GameObject created in scene!" : ""),
"OK");
    }
    
    private void CreateConfettiGameObject(string texturePath)
    {
  // Load all sprites from the texture
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath);
        List<Sprite> spriteList = sprites.OfType<Sprite>()
   .OrderBy(s => s.name)
     .ToList();
        
   if (spriteList.Count == 0)
        {
  Debug.LogWarning("[Confetti Setup] No sprites found after slicing!");
          return;
        }
        
// Create GameObject
        GameObject confettiObj = new GameObject("ConfettiAnimation");
   
        // Add SpriteRenderer
        SpriteRenderer spriteRenderer = confettiObj.AddComponent<SpriteRenderer>();
     spriteRenderer.sprite = spriteList[0];
        
        // Add SpriteAnimator
        SpriteAnimator animator = confettiObj.AddComponent<SpriteAnimator>();
        animator.sprites = spriteList;
        animator.frameRate = frameRate;
        animator.playOnStart = true;
        animator.loop = true;
        animator.pingPong = false;
        
        // Position in scene
        confettiObj.transform.position = Vector3.zero;
    
// Select the new object
        Selection.activeGameObject = confettiObj;
        
        Debug.Log($"[Confetti Setup] Created GameObject with {spriteList.Count} sprites at {frameRate} FPS!");
    }
}
