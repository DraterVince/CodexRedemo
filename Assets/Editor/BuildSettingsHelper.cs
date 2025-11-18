#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Helper script to configure build settings for production builds
/// Removes development build text, console, and configures splash screen
/// </summary>
public class BuildSettingsHelper : EditorWindow
{
    [MenuItem("Tools/Configure Production Build Settings")]
    public static void ConfigureProductionSettings()
    {
        // Disable development build (removes "Development Build" text and console)
        EditorUserBuildSettings.development = false;
      
        // Disable autoconnect profiler
        EditorUserBuildSettings.connectProfiler = false;
        
     // Configure Player Settings for WebGL
        PlayerSettings.SplashScreen.show = false; // Requires Unity Pro/Plus
     PlayerSettings.SplashScreen.showUnityLogo = false; // Requires Unity Pro/Plus
        
   // Alternative: Minimize splash screen if you don't have Pro
        // PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;
        
        // Configure WebGL-specific settings
        ConfigureWebGLSettings();
        
        Debug.Log("? Production build settings applied:");
        Debug.Log("  - Development Build: DISABLED");
        Debug.Log("  - Development Console: DISABLED");
        Debug.Log("- Profiler Auto-connect: DISABLED");
        Debug.Log("  - Splash Screen: DISABLED (requires Unity Pro/Plus)");
        Debug.Log("  - WebGL Template: SupabaseTemplate");
        Debug.Log("\nNote: If you don't have Unity Pro/Plus, the splash screen will still show.");
        Debug.Log("Consider using File ? Build Settings ? Player Settings to adjust splash screen manually.");
    }
    
    [MenuItem("Tools/Configure Development Build Settings")]
    public static void ConfigureDevelopmentSettings()
    {
        // Enable development build
        EditorUserBuildSettings.development = true;
  
     // Enable autoconnect profiler
        EditorUserBuildSettings.connectProfiler = true;
        
        Debug.Log("? Development build settings applied:");
        Debug.Log("  - Development Build: ENABLED");
        Debug.Log("  - Development Console: ENABLED");
   Debug.Log("  - Profiler Auto-connect: ENABLED");
    }
    
    [MenuItem("Tools/Configure WebGL Template with Google Auth Fix")]
    public static void ConfigureWebGLSettings()
    {
  // Set custom WebGL template
        PlayerSettings.WebGL.template = "PROJECT:SupabaseTemplate";

        // Disable compression warnings
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        
        // Enable exception support for better error messages
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;
        
        Debug.Log("? WebGL settings configured:");
        Debug.Log("  - Template: SupabaseTemplate");
        Debug.Log("  - Compression: Disabled (for faster testing)");
      Debug.Log("  - Exception Support: Full with Stacktrace");
    }
    
    [MenuItem("Tools/Fix WebGL Template for Google OAuth Popup")]
    public static void InjectGoogleAuthFix()
    {
        string templatePath = "Assets/WebGLTemplates/SupabaseTemplate/index.html";

        if (!File.Exists(templatePath))
        {
            Debug.LogError($"Template not found: {templatePath}");
            return;
        }
        
   string content = File.ReadAllText(templatePath);
        
        // Check if fix is already injected
 if (content.Contains("google-auth-fix.js"))
{
        Debug.Log("? Google Auth fix already injected in template");
    return;
        }
   
        // Find the </head> tag and inject the script before it
        string scriptTag = "\n    <script src=\"google-auth-fix.js\"></script>\n";
   content = content.Replace("</head>", scriptTag + "</head>");
 
        File.WriteAllText(templatePath, content);
        AssetDatabase.Refresh();
        
        Debug.Log("? Google Auth popup fix injected into WebGL template!");
   Debug.Log("  Next build will use popup-based Google authentication");
    Debug.Log("  This prevents the game from reloading during Google login");
    }
    
    [MenuItem("Tools/Show Current Build Settings")]
    public static void ShowCurrentSettings()
  {
        Debug.Log("=== CURRENT BUILD SETTINGS ===");
    Debug.Log($"Development Build: {(EditorUserBuildSettings.development ? "ENABLED" : "DISABLED")}");
  Debug.Log($"Autoconnect Profiler: {(EditorUserBuildSettings.connectProfiler ? "ENABLED" : "DISABLED")}");
        Debug.Log($"Splash Screen Show: {PlayerSettings.SplashScreen.show}");
        Debug.Log($"Show Unity Logo: {PlayerSettings.SplashScreen.showUnityLogo}");
        Debug.Log($"Active Build Target: {EditorUserBuildSettings.activeBuildTarget}");
        Debug.Log($"WebGL Template: {PlayerSettings.WebGL.template}");
  Debug.Log($"WebGL Compression: {PlayerSettings.WebGL.compressionFormat}");
  }

    [MenuItem("Tools/Remove 'Made with Unity' Watermark (WebGL CSS Fix)")]
    public static void HideUnityWatermark()
    {
      string stylePath = "Assets/WebGLTemplates/SupabaseTemplate/TemplateData/style.css";
        
        if (!Directory.Exists("Assets/WebGLTemplates/SupabaseTemplate/TemplateData"))
{
          Directory.CreateDirectory("Assets/WebGLTemplates/SupabaseTemplate/TemplateData");
        }
     
     string cssContent = @"/* Hide Unity WebGL Logo and Watermark */
#unity-webgl-logo {
    display: none !important;
    visibility: hidden !important;
    opacity: 0 !important;
    width: 0 !important;
    height: 0 !important;
}

#unity-footer {
    display: none !important;
}

/* Hide fullscreen button text */
#unity-fullscreen-button {
    display: none !important;
}

/* Hide build title */
#unity-build-title {
    display: none !important;
}

/* Make canvas take full container */
#unity-canvas {
    width: 100%;
    height: 100%;
}

#unity-container {
 width: 100%;
    height: 100%;
}

/* Alternative: Keep footer but hide just the logo */
/*
#unity-footer {
    display: flex !important;
}

#unity-webgl-logo {
display: none !important;
}
*/
";
        
        File.WriteAllText(stylePath, cssContent);
        AssetDatabase.Refresh();
     
 Debug.Log("? CSS fix applied to hide Unity watermark!");
        Debug.Log($"  Created/Updated: {stylePath}");
        Debug.Log("  Note: This hides the bottom footer with 'Made with Unity' logo");
        Debug.Log("  Unity Pro/Plus is still required to fully remove the splash screen");
    }
}
#endif
