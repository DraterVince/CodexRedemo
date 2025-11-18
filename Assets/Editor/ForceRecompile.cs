using UnityEngine;
using UnityEditor;

/// <summary>
/// Forces Unity to reimport and recompile scripts to fix type resolution issues
/// </summary>
public class ForceRecompile : EditorWindow
{
    [MenuItem("Tools/Fix Compilation Issues/Force Reimport All Scripts")]
    public static void ReimportAllScripts()
    {
        if (EditorUtility.DisplayDialog(
 "Force Reimport Scripts",
            "This will reimport all C# scripts in the project. This may take a few moments.\n\nContinue?",
      "Yes", "Cancel"))
        {
    Debug.Log("[ForceRecompile] Starting reimport of all scripts...");
     
         // Reimport all C# scripts
     string[] scripts = AssetDatabase.FindAssets("t:Script");
            int count = 0;
            
        foreach (string guid in scripts)
    {
      string path = AssetDatabase.GUIDToAssetPath(guid);
           if (path.EndsWith(".cs"))
         {
             AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
           count++;
  }
  }
            
          AssetDatabase.Refresh();
            
    Debug.Log($"[ForceRecompile] ? Reimported {count} scripts. Unity will now recompile.");
         EditorUtility.DisplayDialog("Success", $"Reimported {count} scripts!\n\nUnity is now recompiling...", "OK");
        }
    }
    
    [MenuItem("Tools/Fix Compilation Issues/Reimport SpriteAnimator")]
    public static void ReimportSpriteAnimator()
    {
        string path = "Assets/Scripts/SpriteAnimator.cs";
        
      if (System.IO.File.Exists(path))
        {
       Debug.Log("[ForceRecompile] Reimporting SpriteAnimator.cs...");
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
  AssetDatabase.Refresh();
   Debug.Log("[ForceRecompile] ? SpriteAnimator.cs reimported successfully!");
     EditorUtility.DisplayDialog("Success", "SpriteAnimator.cs has been reimported!", "OK");
        }
        else
        {
            Debug.LogError("[ForceRecompile] ? SpriteAnimator.cs not found at expected path!");
  EditorUtility.DisplayDialog("Error", "SpriteAnimator.cs not found at:\n" + path, "OK");
        }
    }
    
    [MenuItem("Tools/Fix Compilation Issues/Clear Library and Reimport")]
    public static void ClearLibraryFolder()
    {
        if (EditorUtility.DisplayDialog(
   "Clear Library Folder",
"This will close Unity, delete the Library folder, and require you to reopen the project.\n\n" +
"This forces Unity to completely rebuild all cached data.\n\n" +
    "IMPORTANT: Save your work before continuing!\n\nContinue?",
          "Yes, Clear Library", "Cancel"))
        {
   string projectPath = System.IO.Path.GetDirectoryName(Application.dataPath);
       string libraryPath = System.IO.Path.Combine(projectPath, "Library");
   
    Debug.Log("[ForceRecompile] Creating cleanup batch file...");
  
        // Create a batch file to delete Library and reopen Unity
            string batchPath = System.IO.Path.Combine(projectPath, "CleanupLibrary.bat");
    string batchContent = $@"@echo off
echo Waiting for Unity to close...
timeout /t 3 /nobreak > nul
echo Deleting Library folder...
rmdir /s /q ""{libraryPath}""
echo Library folder deleted!
echo Please reopen Unity project manually.
pause
del ""%~f0""
";
      
            System.IO.File.WriteAllText(batchPath, batchContent);
            
      // Start the batch file
   System.Diagnostics.Process.Start(batchPath);
     
            // Quit Unity
            EditorApplication.Exit(0);
        }
    }
}
