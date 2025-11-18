using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Editor utility to auto-generate AnimationClips and an AnimatorController from a sprite-sheet (multiple sprites in a single texture)
public class AutoSpriteSheetAnimator : EditorWindow
{
 private UnityEngine.Object spriteSource;
 private string saveFolder = "Assets/Animations";
 private string controllerName = "AutoSpriteController";
 private float frameRate =12f;
 private bool loopClips = true;
 private bool createController = true;
 private Vector2 scroll;

 // Speed multipliers
 private float globalSpeedMultiplier =1f; //1 = normal, <1 = slower, >1 = faster
 private float idleSpeedMultiplier =1f; // specific override for idle animation

 [MenuItem("Tools/Animation/Auto SpriteSheet Animator")]
 public static void ShowWindow()
 {
 GetWindow<AutoSpriteSheetAnimator>("Auto SpriteSheet Animator");
 }

 private void OnGUI()
 {
 scroll = EditorGUILayout.BeginScrollView(scroll);

 EditorGUILayout.LabelField("Auto SpriteSheet Animator", EditorStyles.boldLabel);
 EditorGUILayout.HelpBox("Select a Texture2D or a Sprite asset that has multiple sliced sprites. This tool will group sprites by name (prefix) and create AnimationClips and an Animator Controller.", MessageType.Info);

 spriteSource = EditorGUILayout.ObjectField("Sprite / Texture", spriteSource, typeof(UnityEngine.Object), false);
 saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);
 controllerName = EditorGUILayout.TextField("Controller Name", controllerName);
 frameRate = EditorGUILayout.FloatField("Base Frame Rate", frameRate);
 loopClips = EditorGUILayout.Toggle("Loop Clips", loopClips);
 createController = EditorGUILayout.Toggle("Create Animator Controller", createController);

 EditorGUILayout.Space();
 EditorGUILayout.LabelField("Speed Adjustments", EditorStyles.boldLabel);
 globalSpeedMultiplier = EditorGUILayout.FloatField(new GUIContent("Global Speed Multiplier", "1 = normal, <1 = slower, >1 = faster"), globalSpeedMultiplier);
 idleSpeedMultiplier = EditorGUILayout.FloatField(new GUIContent("Idle Speed Multiplier", "Multiplier applied to the Idle animation (1 = normal). Use <1 to slow down."), idleSpeedMultiplier);

 if (GUILayout.Button("Generate Animations from Sprite Sheet"))
 {
 GenerateFromSelection();
 }

 EditorGUILayout.EndScrollView();
 }

 private void GenerateFromSelection()
 {
 if (spriteSource == null)
 {
 EditorUtility.DisplayDialog("Error", "Please assign a Sprite or Texture2D asset.", "OK");
 return;
 }

 string path = AssetDatabase.GetAssetPath(spriteSource);
 if (string.IsNullOrEmpty(path))
 {
 EditorUtility.DisplayDialog("Error", "Could not determine asset path for the selected object.", "OK");
 return;
 }

 // Ensure the texture importer uses Point filtering (no bilinear)
 TextureImporter texImporter = AssetImporter.GetAtPath(path) as TextureImporter;
 if (texImporter != null)
 {
 // Only change if necessary to avoid unnecessary reimports
 if (texImporter.filterMode != FilterMode.Point)
 {
 texImporter.filterMode = FilterMode.Point;
 texImporter.SaveAndReimport();
 }
 }

 UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
 Sprite[] sprites = assets.OfType<Sprite>().ToArray();

 if (sprites.Length ==0)
 {
 EditorUtility.DisplayDialog("Error", "No sprites found in the selected asset. Make sure the texture type is set to 'Sprite (2D and UI)' and is sliced.", "OK");
 return;
 }

 // Ensure save folder exists
 if (!AssetDatabase.IsValidFolder(saveFolder))
 {
 string parent = "Assets";
 string[] parts = saveFolder.Split('/');
 for (int i =0; i < parts.Length; i++)
 {
 string sub = string.Join("/", parts.Take(i +1).ToArray());
 if (!AssetDatabase.IsValidFolder(sub))
 {
 AssetDatabase.CreateFolder(parent, parts[i]);
 }
 parent = sub;
 }
 }

 // Group sprites by prefix (before an underscore) or by stripping trailing numbers
 var groups = GroupSpritesByPrefix(sprites);

 List<AnimationClip> createdClips = new List<AnimationClip>();

 foreach (var kv in groups)
 {
 string stateName = kv.Key;
 Sprite[] groupSprites = kv.Value.OrderBy(s => ExtractNumberSuffix(s.name)).ThenBy(s => s.name).ToArray();

 // Determine effective frame rate for this clip
 float effectiveMultiplier = globalSpeedMultiplier;
 if (stateName.ToLower().Contains("idle"))
 {
 // apply idle-specific multiplier
 effectiveMultiplier *= idleSpeedMultiplier;
 }
 float effectiveFrameRate = Mathf.Max(1f, frameRate * effectiveMultiplier);

 AnimationClip clip = CreateClipFromSprites(groupSprites, effectiveFrameRate, loopClips);
 string clipPath = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(saveFolder, stateName + ".anim"));
 AssetDatabase.CreateAsset(clip, clipPath);
 createdClips.Add(clip);
 Debug.Log($"Created clip: {clipPath} ({groupSprites.Length} frames) @ {effectiveFrameRate} fps");
 }

 AnimatorController controller = null;
 if (createController && createdClips.Count >0)
 {
 string controllerPath = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(saveFolder, controllerName + ".controller"));
 controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
 var root = controller.layers[0].stateMachine;

 for (int i =0; i < createdClips.Count; i++)
 {
 var clip = createdClips[i];
 var state = root.AddState(clip.name, new Vector3(300 + i *200,0,0));
 state.motion = clip;
 if (i ==0) root.defaultState = state;

 // Add a trigger parameter for non-looping clips
 if (!IsLoopingClip(clip))
 {
 if (!controller.parameters.Any(p => p.name == clip.name))
 controller.AddParameter(clip.name, AnimatorControllerParameterType.Trigger);
 }
 }

 AssetDatabase.SaveAssets();
 Debug.Log($"Created Animator Controller at: {controllerPath}");
 }

 AssetDatabase.SaveAssets();
 AssetDatabase.Refresh();

 string message = $"Created {createdClips.Count} clip(s)." + (controller != null ? $" Animator Controller created: {controller.name}" : "");
 EditorUtility.DisplayDialog("Done", message, "OK");
 }

 private static Dictionary<string, List<Sprite>> GroupSpritesByPrefix(Sprite[] sprites)
 {
 Dictionary<string, List<Sprite>> groups = new Dictionary<string, List<Sprite>>();

 foreach (var s in sprites)
 {
 string name = s.name;
 string key = null;

 // If contains underscore, use prefix before first underscore
 if (name.Contains("_"))
 {
 key = name.Split(new[] { '_' },2)[0];
 }
 else
 {
 // Strip trailing numbers, e.g., run01 run02 -> run
 var m = Regex.Match(name, "^(.*?)(\\d+)$");
 if (m.Success)
 key = m.Groups[1].Value;
 else
 key = name; // single frame animation
 }

 if (!groups.ContainsKey(key)) groups[key] = new List<Sprite>();
 groups[key].Add(s);
 }

 return groups;
 }

 private static int ExtractNumberSuffix(string name)
 {
 var m = Regex.Match(name, "(\\d+)$");
 if (m.Success && int.TryParse(m.Groups[1].Value, out int v))
 return v;
 return 0;
 }

 private static AnimationClip CreateClipFromSprites(Sprite[] frames, float frameRate, bool loop)
 {
 AnimationClip clip = new AnimationClip();
 // Allow very small frame rates (<1) so animations can be slowed below1 fps if needed.
 clip.frameRate = Mathf.Max(0.0001f, frameRate);
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

 // Set loop time in clip settings
 var settings = AnimationUtility.GetAnimationClipSettings(clip);
 settings.loopTime = loop;
 AnimationUtility.SetAnimationClipSettings(clip, settings);

 return clip;
 }

 private static bool IsLoopingClip(AnimationClip clip)
 {
 var settings = AnimationUtility.GetAnimationClipSettings(clip);
 return settings.loopTime;
 }
}
