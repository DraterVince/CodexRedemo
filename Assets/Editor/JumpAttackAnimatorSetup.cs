using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Simple editor utility to create an AnimatorController with Idle and Attack states.
// It can generate AnimationClips from two sprite sheet assets (sliced textures) or use existing clips.
public class JumpAttackAnimatorSetup : EditorWindow
{
 private Texture2D idleTexture;
 private Texture2D attackTexture;
 private AnimationClip idleClip;
 private AnimationClip attackClip;

 private string saveFolder = "Assets/Animations";
 private string controllerName = "JumpAttackController";
 private float baseFrameRate =12f;
 private bool loopIdle = true;
 private bool loopAttack = false;

 private float transitionDuration =0.08f;
 private bool attackHasExitTime = true;
 private float attackExitTime =0.9f;

 [MenuItem("Tools/Animation/Jump-Attack Animator Setup")]
 public static void ShowWindow() => GetWindow<JumpAttackAnimatorSetup>("JumpAttack Setup");

 private void OnGUI()
 {
 EditorGUILayout.LabelField("Jump-Attack Animator Setup", EditorStyles.boldLabel);
 EditorGUILayout.Space();

 idleTexture = (Texture2D)EditorGUILayout.ObjectField("Idle Texture", idleTexture, typeof(Texture2D), false);
 attackTexture = (Texture2D)EditorGUILayout.ObjectField("Attack Texture", attackTexture, typeof(Texture2D), false);

 EditorGUILayout.Space();
 idleClip = (AnimationClip)EditorGUILayout.ObjectField("Idle Clip (optional)", idleClip, typeof(AnimationClip), false);
 attackClip = (AnimationClip)EditorGUILayout.ObjectField("Attack Clip (optional)", attackClip, typeof(AnimationClip), false);

 EditorGUILayout.Space();
 saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);
 controllerName = EditorGUILayout.TextField("Controller Name", controllerName);
 baseFrameRate = EditorGUILayout.FloatField("Base Frame Rate", baseFrameRate);

 EditorGUILayout.Space();
 loopIdle = EditorGUILayout.Toggle("Loop Idle", loopIdle);
 loopAttack = EditorGUILayout.Toggle("Loop Attack", loopAttack);

 EditorGUILayout.Space();
 EditorGUILayout.LabelField("Transition Settings", EditorStyles.boldLabel);
 transitionDuration = EditorGUILayout.Slider("Transition Duration", transitionDuration,0f,1f);
 attackHasExitTime = EditorGUILayout.Toggle("Attack Has Exit Time", attackHasExitTime);
 if (attackHasExitTime) attackExitTime = EditorGUILayout.Slider("Attack Exit Time", attackExitTime,0f,1f);

 EditorGUILayout.Space();
 if (GUILayout.Button("Generate Controller and Clips"))
 {
 Generate();
 }
 }

 private void Generate()
 {
 if (string.IsNullOrEmpty(controllerName))
 {
 EditorUtility.DisplayDialog("Error", "Please provide a controller name.", "OK");
 return;
 }

 if (!DirectoryExistsOrCreate(saveFolder))
 {
 EditorUtility.DisplayDialog("Error", $"Could not create or find folder: {saveFolder}", "OK");
 return;
 }

 // If textures provided and clips not provided explicitly, generate clips from sprite sheets
 if (idleClip == null && idleTexture != null)
 {
 var sprites = LoadSpritesFromTexture(idleTexture);
 if (sprites != null && sprites.Length >0)
 {
 idleClip = CreateClipFromSprites(sprites, baseFrameRate, loopIdle);
 string idlePath = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(saveFolder, controllerName + "_Idle.anim"));
 AssetDatabase.CreateAsset(idleClip, idlePath);
 }
 }

 if (attackClip == null && attackTexture != null)
 {
 var sprites = LoadSpritesFromTexture(attackTexture);
 if (sprites != null && sprites.Length >0)
 {
 attackClip = CreateClipFromSprites(sprites, baseFrameRate, loopAttack);
 string attackPath = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(saveFolder, controllerName + "_Attack.anim"));
 AssetDatabase.CreateAsset(attackClip, attackPath);
 }
 }

 if (idleClip == null && attackClip == null)
 {
 EditorUtility.DisplayDialog("Error", "No clips or source textures provided.", "OK");
 return;
 }

 // Create AnimatorController
 string controllerPath = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(saveFolder, controllerName + ".controller"));
 AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

 // Add parameters
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

 // Transitions
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

 Selection.activeObject = controller;
 EditorGUIUtility.PingObject(controller);

 EditorUtility.DisplayDialog("Done", $"Created Animator Controller at: {controllerPath}", "OK");
 }

 private static bool DirectoryExistsOrCreate(string folder)
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

 private static Sprite[] LoadSpritesFromTexture(Texture2D tex)
 {
 if (tex == null) return null;
 string path = AssetDatabase.GetAssetPath(tex);
 if (string.IsNullOrEmpty(path)) return null;
 var assets = AssetDatabase.LoadAllAssetsAtPath(path);
 return assets.OfType<Sprite>().ToArray();
 }

 private static AnimationClip CreateClipFromSprites(Sprite[] frames, float frameRate, bool loop)
 {
 AnimationClip clip = new AnimationClip();
 clip.frameRate = Mathf.Max(0.0001f, frameRate);
 EditorCurveBinding spriteBinding = new EditorCurveBinding();
 spriteBinding.type = typeof(SpriteRenderer);
 spriteBinding.path = "";
 spriteBinding.propertyName = "m_Sprite";

 var ordered = frames.OrderBy(s => ExtractNumberSuffix(s.name)).ThenBy(s => s.name).ToArray();

 ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[ordered.Length];
 for (int i =0; i < ordered.Length; i++)
 {
 var k = new ObjectReferenceKeyframe();
 k.time = i / clip.frameRate;
 k.value = ordered[i];
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
}
