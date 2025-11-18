using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Auto-detects Idle and Attack sprite sheets in a folder (or entire project) and generates
// AnimationClips + an AnimatorController with Idle/Attack states.
public class AutoJumpAttackAnimatorSetup : EditorWindow
{
 private string searchFolder = "Assets";
 private bool searchEntireProject = true;
 private string controllerName = "AutoJumpAttackController";
 private string saveFolder = "Assets/Animations";
 private float baseFrameRate =12f;

 // Speed/misc options
 private float globalSpeedMultiplier =1f;
 private float idleSpeedMultiplier =1f;

 // Import settings to force for pixel-art
 private bool forcePointFilter = true;
 private bool disableMipMaps = true;
 private bool forceNoCompression = true;
 private int maxTextureSize =2048;

 private float transitionDuration =0.08f;
 private bool attackHasExitTime = true;
 private float attackExitTime =0.9f;

 private Vector2 scroll;

 [MenuItem("Tools/Animation/Auto Jump-Attack Setup")]
 public static void ShowWindow() => GetWindow<AutoJumpAttackAnimatorSetup>("Auto Jump-Attack");

 private void OnGUI()
 {
 scroll = EditorGUILayout.BeginScrollView(scroll);
 EditorGUILayout.LabelField("Auto Jump-Attack Animator Setup", EditorStyles.boldLabel);
 EditorGUILayout.HelpBox("Scans the project (or a selected folder) for textures whose path or filename contains 'idle' or 'attack', generates clips and an AnimatorController.", MessageType.Info);

 searchEntireProject = EditorGUILayout.Toggle("Search Entire Project", searchEntireProject);
 using (new EditorGUI.DisabledScope(searchEntireProject))
 {
 EditorGUILayout.BeginHorizontal();
 searchFolder = EditorGUILayout.TextField("Search Folder", searchFolder);
 if (GUILayout.Button("Browse", GUILayout.Width(60)))
 {
 string p = EditorUtility.OpenFolderPanel("Select Folder to Search", Application.dataPath, "");
 if (!string.IsNullOrEmpty(p) && p.StartsWith(Application.dataPath))
 searchFolder = "Assets" + p.Substring(Application.dataPath.Length);
 }
 EditorGUILayout.EndHorizontal();
 }

 EditorGUILayout.Space();
 controllerName = EditorGUILayout.TextField("Controller Name", controllerName);
 saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);
 baseFrameRate = EditorGUILayout.FloatField("Base Frame Rate", baseFrameRate);

 EditorGUILayout.Space();
 EditorGUILayout.LabelField("Speed Options", EditorStyles.boldLabel);
 globalSpeedMultiplier = EditorGUILayout.FloatField(new GUIContent("Global Speed Multiplier", "1 = normal, <1 slower, >1 faster"), globalSpeedMultiplier);
 idleSpeedMultiplier = EditorGUILayout.FloatField(new GUIContent("Idle Speed Multiplier", "Multiplier applied only to idle animations"), idleSpeedMultiplier);

 EditorGUILayout.Space();
 EditorGUILayout.LabelField("Import Settings to Force", EditorStyles.boldLabel);
 forcePointFilter = EditorGUILayout.Toggle("Force Point Filter", forcePointFilter);
 disableMipMaps = EditorGUILayout.Toggle("Disable MipMaps", disableMipMaps);
 forceNoCompression = EditorGUILayout.Toggle("Force No Compression", forceNoCompression);
 maxTextureSize = EditorGUILayout.IntField("Max Texture Size", maxTextureSize);

 EditorGUILayout.Space();
 EditorGUILayout.LabelField("Transition Settings", EditorStyles.boldLabel);
 transitionDuration = EditorGUILayout.Slider("Transition Duration", transitionDuration,0f,1f);
 attackHasExitTime = EditorGUILayout.Toggle("Attack Has Exit Time", attackHasExitTime);
 if (attackHasExitTime) attackExitTime = EditorGUILayout.Slider("Attack Exit Time", attackExitTime,0f,1f);

 EditorGUILayout.Space();
 if (GUILayout.Button("Scan and Generate (Idle + Attack)"))
 {
 ScanAndGenerate();
 }

 EditorGUILayout.EndScrollView();
 }

 private void ScanAndGenerate()
 {
 // Ensure save folder
 if (!EnsureFolder(saveFolder))
 {
 EditorUtility.DisplayDialog("Error", $"Could not create or access save folder: {saveFolder}", "OK");
 return;
 }

 string[] guids;
 if (searchEntireProject)
 {
 guids = AssetDatabase.FindAssets("t:Texture2D");
 }
 else
 {
 if (string.IsNullOrEmpty(searchFolder) || !AssetDatabase.IsValidFolder(searchFolder))
 {
 EditorUtility.DisplayDialog("Error", "Search folder is invalid. Please choose a valid Assets sub-folder.", "OK");
 return;
 }
 guids = AssetDatabase.FindAssets("t:Texture2D", new[] { searchFolder });
 }

 // Find candidate textures where path or filename contains keywords
 var idleCandidates = new List<string>();
 var attackCandidates = new List<string>();
 foreach (var g in guids)
 {
 string path = AssetDatabase.GUIDToAssetPath(g);
 string lower = path.ToLowerInvariant();
 string filename = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();

 if (lower.Contains("idle") || filename.Contains("idle"))
 idleCandidates.Add(path);
 if (lower.Contains("attack") || filename.Contains("attack") || lower.Contains("atk") || filename.Contains("atk"))
 attackCandidates.Add(path);
 }

 // Choose best candidate: prefer textures inside a folder named exactly 'idle' or 'attack'
 string idlePath = PickBestCandidate(idleCandidates, "idle");
 string attackPath = PickBestCandidate(attackCandidates, "attack");

 if (string.IsNullOrEmpty(idlePath) && string.IsNullOrEmpty(attackPath))
 {
 EditorUtility.DisplayDialog("Not Found", "No idle or attack textures were found in the search scope.", "OK");
 return;
 }

 // Process import settings and create clips
 AnimationClip idleClip = null;
 AnimationClip attackClip = null;

 if (!string.IsNullOrEmpty(idlePath))
 {
 ApplyImporterSettings(idlePath);
 var sprites = LoadSpritesFromPath(idlePath);
 if (sprites.Length >0)
 {
 float effFPS = baseFrameRate * globalSpeedMultiplier * idleSpeedMultiplier;
 idleClip = CreateClipFromSprites(sprites, Math.Max(0.0001f, effFPS), true);
 string idleClipPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(saveFolder, controllerName + "_Idle.anim"));
 AssetDatabase.CreateAsset(idleClip, idleClipPath);
 }
 }

 if (!string.IsNullOrEmpty(attackPath))
 {
 ApplyImporterSettings(attackPath);
 var sprites = LoadSpritesFromPath(attackPath);
 if (sprites.Length >0)
 {
 float effFPS = baseFrameRate * globalSpeedMultiplier;
 attackClip = CreateClipFromSprites(sprites, Math.Max(0.0001f, effFPS), false);
 string attackClipPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(saveFolder, controllerName + "_Attack.anim"));
 AssetDatabase.CreateAsset(attackClip, attackClipPath);
 }
 }

 // Create controller
 string controllerPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(saveFolder, controllerName + ".controller"));
 AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
 if (!controller.parameters.Any(p => p.name == "Attack"))
 controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

 var root = controller.layers[0].stateMachine;
 root.name = controllerName + "_Root";

 AnimatorState idleState = null;
 if (idleClip != null)
 {
 idleState = root.AddState("Idle", new Vector3(200,0,0));
 idleState.motion = idleClip;
 root.defaultState = idleState;
 }

 AnimatorState attackState = null;
 if (attackClip != null)
 {
 attackState = root.AddState("Attack", new Vector3(200, -150,0));
 attackState.motion = attackClip;
 }

 if (idleState != null && attackState != null)
 {
 var toAttack = idleState.AddTransition(attackState);
 toAttack.duration = transitionDuration;
 toAttack.hasExitTime = false;
 toAttack.AddCondition(AnimatorConditionMode.If,0, "Attack");

 var toIdle = attackState.AddTransition(idleState);
 toIdle.duration = transitionDuration;
 toIdle.hasExitTime = attackHasExitTime;
 if (attackHasExitTime) toIdle.exitTime = attackExitTime;
 }

 AssetDatabase.SaveAssets();
 AssetDatabase.Refresh();

 EditorUtility.DisplayDialog("Done", $"Created controller at: {controllerPath}\nIdle clip: {(idleClip != null ? "yes" : "no")}\nAttack clip: {(attackClip != null ? "yes" : "no")}", "OK");
 Selection.activeObject = controller;
 EditorGUIUtility.PingObject(controller);
 }

 private static string PickBestCandidate(List<string> candidates, string keyword)
 {
 if (candidates == null || candidates.Count ==0) return null;
 // prefer files in a folder named exactly the keyword
 foreach (var p in candidates)
 {
 var parts = p.Split('/');
 if (parts.Any(part => string.Equals(part, keyword, StringComparison.OrdinalIgnoreCase)))
 return p;
 }
 // otherwise prefer filenames that start with the keyword
 var first = candidates.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p).StartsWith(keyword, StringComparison.OrdinalIgnoreCase));
 if (first != null) return first;
 // fallback: return first
 return candidates[0];
 }

 private static void ApplyImporterSettings(string assetPath)
 {
 var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
 if (importer == null) return;

 bool changed = false;
 if (importer.filterMode != FilterMode.Point)
 {
 importer.filterMode = FilterMode.Point;
 changed = true;
 }
 if (importer.mipmapEnabled != false)
 {
 importer.mipmapEnabled = false;
 changed = true;
 }
 if (importer.textureCompression != TextureImporterCompression.Uncompressed)
 {
 importer.textureCompression = TextureImporterCompression.Uncompressed;
 changed = true;
 }
 if (importer.maxTextureSize <32 || importer.maxTextureSize <0 || importer.maxTextureSize < maxExpectedSize())
 {
 // leave existing size if it's larger than requested, otherwise set to desired maxTextureSize
 importer.maxTextureSize = Math.Max(importer.maxTextureSize,2048);
 changed = true;
 }

 if (changed)
 importer.SaveAndReimport();

 int maxExpectedSize() =>2048; // keep simple for now
 }

 private static Sprite[] LoadSpritesFromPath(string path)
 {
 // Try to load sprites directly from the asset (works for sliced sprite sheets)
 var assets = AssetDatabase.LoadAllAssetsAtPath(path);
 var sprites = assets.OfType<Sprite>().ToList();
 if (sprites.Count >0)
 {
 return sprites.OrderBy(s => ExtractNumberSuffix(s.name)).ThenBy(s => s.name).ToArray();
 }

 // If no sprites found on the asset itself, check the asset's folder for multiple single-image sprites
 try
 {
 string folder = Path.GetDirectoryName(path)?.Replace("\\", "/");
 if (string.IsNullOrEmpty(folder)) return new Sprite[0];

 // Get all common image files in the folder
 var files = Directory.GetFiles(folder)
 .Where(f => {
 var ext = Path.GetExtension(f).ToLowerInvariant();
 return ext == ".png" || ext == ".psd" || ext == ".jpg" || ext == ".jpeg";
 })
 .OrderBy(f => f)
 .ToArray();

 List<Sprite> folderSprites = new List<Sprite>();
 foreach (var file in files)
 {
 // Convert system path to asset path
 string assetPath = file;
 if (assetPath.StartsWith(Application.dataPath))
 assetPath = "Assets" + assetPath.Substring(Application.dataPath.Length);
 assetPath = assetPath.Replace("\\", "/");

 // Try to load sprite directly (single-sprite texture will often give a Sprite)
 var sp = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
 if (sp != null)
 {
 folderSprites.Add(sp);
 continue;
 }

 // Fallback: load all assets at path and pick the first Sprite if present
 var all = AssetDatabase.LoadAllAssetsAtPath(assetPath);
 var sp2 = all.OfType<Sprite>().FirstOrDefault();
 if (sp2 != null)
 {
 folderSprites.Add(sp2);
 }
 }

 if (folderSprites.Count >0)
 return folderSprites.OrderBy(s => ExtractNumberSuffix(s.name)).ThenBy(s => s.name).ToArray();
 }
 catch (Exception)
 {
 // Ignore any IO errors and fall through to return empty
 }

 return new Sprite[0];
 }

 private static AnimationClip CreateClipFromSprites(Sprite[] frames, double frameRate, bool loop)
 {
 AnimationClip clip = new AnimationClip();
 clip.frameRate = (float)Math.Max(0.0001, frameRate);
 EditorCurveBinding spriteBinding = new EditorCurveBinding();
 spriteBinding.type = typeof(SpriteRenderer);
 spriteBinding.path = "";
 spriteBinding.propertyName = "m_Sprite";

 ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[frames.Length];
 for (int i =0; i < frames.Length; i++)
 {
 var k = new ObjectReferenceKeyframe();
 k.time = i / clip.frameRate;
 k.value = frames[i];
 keyFrames[i] = k;
 }

 AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyFrames);
 var settings = AnimationUtility.GetAnimationClipSettings(clip);
 settings.loopTime = loop;
 AnimationUtility.SetAnimationClipSettings(clip, settings);
 return clip;
 }

 private static int ExtractNumberSuffix(string name)
 {
 var m = Regex.Match(name, "(\\d+)$");
 if (m.Success && int.TryParse(m.Groups[1].Value, out int v)) return v;
 return 0;
 }

 private static bool EnsureFolder(string folder)
 {
 if (AssetDatabase.IsValidFolder(folder)) return true;
 string parent = "Assets";
 var parts = folder.Split('/');
 foreach (var part in parts)
 {
 string sub = parent + "/" + part;
 if (!AssetDatabase.IsValidFolder(sub))
 {
 var created = AssetDatabase.CreateFolder(parent, part);
 if (string.IsNullOrEmpty(created)) return false;
 }
 parent = sub;
 }
 return true;
 }
}
