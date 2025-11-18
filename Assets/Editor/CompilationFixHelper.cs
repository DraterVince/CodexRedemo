using UnityEditor;
using UnityEngine;

/// <summary>
/// Helper tool to fix SpriteAnimator type resolution issues
/// </summary>
public class CompilationFixHelper : EditorWindow
{
    [MenuItem("Tools/Fix Compilation Issues")]
    public static void ShowWindow()
    {
        GetWindow<CompilationFixHelper>("Compilation Fix");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Compilation Fix Helper", EditorStyles.boldLabel);
   EditorGUILayout.Space();
        
 EditorGUILayout.HelpBox(
            "If you see errors like:\n" +
            "\"The type or namespace name 'SpriteAnimator' could not be found\"\n\n" +
            "Click the button below to force Unity to reimport and recompile.", 
        MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Force Reimport SpriteAnimator", GUILayout.Height(40)))
        {
            ForceReimport("Assets/Scripts/SpriteAnimator.cs");
  }
        
    EditorGUILayout.Space();
     
      if (GUILayout.Button("Force Reimport All Animation Scripts", GUILayout.Height(40)))
      {
            ForceReimportAll();
     }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Refresh Asset Database", GUILayout.Height(30)))
        {
            AssetDatabase.Refresh();
   EditorUtility.DisplayDialog("Success", "Asset database refreshed!", "OK");
        }
     
      EditorGUILayout.Space();
      EditorGUILayout.Space();
        
   EditorGUILayout.HelpBox(
 "Still having issues?\n\n" +
            "1. Close Unity\n" +
            "2. Delete the 'Library' folder\n" +
        "3. Reopen Unity\n\n" +
   "See SPRITEANIMATOR_TYPE_RESOLUTION_FIX.md for more solutions.", 
        MessageType.Warning);
    }
    
    private void ForceReimport(string path)
    {
        Debug.Log($"[CompilationFix] Reimporting: {path}");
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
     AssetDatabase.Refresh();
   EditorUtility.DisplayDialog("Success", $"Reimported {path}\n\nWait for compilation to complete.", "OK");
    }
    
    private void ForceReimportAll()
    {
        string[] scripts = new string[]
        {
    "Assets/Scripts/SpriteAnimator.cs",
            "Assets/Scripts/CharacterAnimationController.cs",
            "Assets/Scripts/MultiAnimationController.cs",
            "Assets/Scripts/MultiAnimationInput.cs"
 };
      
        int count = 0;
      foreach (string script in scripts)
        {
  if (System.IO.File.Exists(script))
      {
            Debug.Log($"[CompilationFix] Reimporting: {script}");
           AssetDatabase.ImportAsset(script, ImportAssetOptions.ForceUpdate);
  count++;
            }
        }
  
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"Reimported {count} scripts\n\nWait for compilation to complete.", "OK");
    }
}
