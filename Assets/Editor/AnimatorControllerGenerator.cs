using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class AnimatorControllerGenerator : EditorWindow
{
    private string controllerName = "NewCharacterAnimator";
    private string savePath = "Assets/Animators";
    private AnimationType animationType = AnimationType.PlayerCharacter;
    
    // Animation Clips
    private AnimationClip idleClip;
    private AnimationClip attackClip;
 private AnimationClip walkClip;
    private AnimationClip runClip;
    private AnimationClip jumpClip;
    private AnimationClip deathClip;
    private AnimationClip hurtClip;
    
    // Transition Settings
    private float transitionDuration = 0.1f;
    private bool hasExitTime = false;
    private float exitTime = 0.75f;
    
    // Advanced Options
    private bool createSubStateMachines = false;
    private bool addBlendTrees = false;
    private bool autoSetupTriggers = true;
    private bool createLayeredAnimator = false;
    
    private Vector2 scrollPosition;
    private bool showAdvancedOptions = false;
    private bool showTransitionSettings = true;
    
    private enum AnimationType
    {
      PlayerCharacter,
        Enemy,
        NPC,
        UI,
        Custom
    }
    
    [MenuItem("Tools/Animation/Animator Controller Generator")]
    public static void ShowWindow()
    {
        var window = GetWindow<AnimatorControllerGenerator>("Animator Generator");
  window.minSize = new Vector2(400, 600);
        window.Show();
    }
    
    private void OnGUI()
    {
 scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        DrawHeader();
     DrawBasicSettings();
        DrawAnimationClips();
     DrawTransitionSettings();
        DrawAdvancedOptions();
        DrawActionButtons();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
  GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
   {
         fontSize = 16,
     alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("Animator Controller Generator", headerStyle);
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("Automatically create Animator Controllers with states, transitions, and parameters.", MessageType.Info);
        EditorGUILayout.Space(10);
    }
    
    private void DrawBasicSettings()
  {
        EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
        
        controllerName = EditorGUILayout.TextField("Controller Name", controllerName);
        
        EditorGUILayout.BeginHorizontal();
  savePath = EditorGUILayout.TextField("Save Path", savePath);
    if (GUILayout.Button("Browse", GUILayout.Width(60)))
   {
     string path = EditorUtility.OpenFolderPanel("Select Save Location", "Assets", "");
if (!string.IsNullOrEmpty(path))
 {
      savePath = "Assets" + path.Substring(Application.dataPath.Length);
}
        }
        EditorGUILayout.EndHorizontal();
        
        animationType = (AnimationType)EditorGUILayout.EnumPopup("Animation Type", animationType);
        
   EditorGUILayout.Space(10);
    }
    
    private void DrawAnimationClips()
    {
        EditorGUILayout.LabelField("Animation Clips", EditorStyles.boldLabel);
     EditorGUILayout.HelpBox("Assign animation clips. Only assigned clips will be added to the controller.", MessageType.None);
        
        idleClip = (AnimationClip)EditorGUILayout.ObjectField("Idle Clip", idleClip, typeof(AnimationClip), false);
        attackClip = (AnimationClip)EditorGUILayout.ObjectField("Attack Clip", attackClip, typeof(AnimationClip), false);
        walkClip = (AnimationClip)EditorGUILayout.ObjectField("Walk Clip", walkClip, typeof(AnimationClip), false);
        runClip = (AnimationClip)EditorGUILayout.ObjectField("Run Clip", runClip, typeof(AnimationClip), false);
        jumpClip = (AnimationClip)EditorGUILayout.ObjectField("Jump Clip", jumpClip, typeof(AnimationClip), false);
      hurtClip = (AnimationClip)EditorGUILayout.ObjectField("Hurt Clip", hurtClip, typeof(AnimationClip), false);
    deathClip = (AnimationClip)EditorGUILayout.ObjectField("Death Clip", deathClip, typeof(AnimationClip), false);
        
      EditorGUILayout.Space(10);
    }
    
    private void DrawTransitionSettings()
    {
        showTransitionSettings = EditorGUILayout.Foldout(showTransitionSettings, "Transition Settings", true);
      if (showTransitionSettings)
     {
          EditorGUI.indentLevel++;
            transitionDuration = EditorGUILayout.Slider("Transition Duration", transitionDuration, 0f, 1f);
        hasExitTime = EditorGUILayout.Toggle("Has Exit Time", hasExitTime);
  if (hasExitTime)
     {
                exitTime = EditorGUILayout.Slider("Exit Time", exitTime, 0f, 1f);
            }
 EditorGUI.indentLevel--;
 }
        EditorGUILayout.Space(10);
    }
    
    private void DrawAdvancedOptions()
    {
        showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options", true);
        if (showAdvancedOptions)
        {
            EditorGUI.indentLevel++;
            autoSetupTriggers = EditorGUILayout.Toggle("Auto Setup Triggers", autoSetupTriggers);
          createSubStateMachines = EditorGUILayout.Toggle("Create Sub-State Machines", createSubStateMachines);
    addBlendTrees = EditorGUILayout.Toggle("Add Blend Trees (Movement)", addBlendTrees);
createLayeredAnimator = EditorGUILayout.Toggle("Create Layered Animator", createLayeredAnimator);
  EditorGUI.indentLevel--;
        }
    EditorGUILayout.Space(10);
  }
    
    private void DrawActionButtons()
    {
        EditorGUILayout.Space(10);
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Generate Animator Controller", GUILayout.Height(40)))
        {
       GenerateAnimatorController();
     }
      GUI.backgroundColor = Color.white;
   
        EditorGUILayout.Space(5);
  
        if (GUILayout.Button("Load Template Settings"))
     {
 LoadTemplateSettings();
        }
        
  if (GUILayout.Button("Clear All Fields"))
        {
            ClearFields();
 }
    }
    
    private void GenerateAnimatorController()
    {
        if (string.IsNullOrEmpty(controllerName))
    {
          EditorUtility.DisplayDialog("Error", "Please enter a controller name.", "OK");
       return;
     }
        
        if (!HasAnyClip())
   {
            EditorUtility.DisplayDialog("Error", "Please assign at least one animation clip.", "OK");
       return;
        }
    
        // Create directory if it doesn't exist
        if (!Directory.Exists(savePath))
     {
        Directory.CreateDirectory(savePath);
      }
        
   string fullPath = Path.Combine(savePath, controllerName + ".controller");
        
        // Check if file already exists
        if (File.Exists(fullPath))
        {
            if (!EditorUtility.DisplayDialog("File Exists", 
          $"An animator controller already exists at {fullPath}. Do you want to overwrite it?", 
       "Yes", "No"))
     {
  return;
     }
        }
        
        // Create the animator controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(fullPath);
        
        // Setup parameters
   SetupParameters(controller);
   
        // Get root state machine
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
        
   // Create states
        Dictionary<string, AnimatorState> states = new Dictionary<string, AnimatorState>();
        
      if (idleClip != null)
   {
            states["Idle"] = CreateState(rootStateMachine, "Idle", idleClip, new Vector3(300, 0, 0));
            rootStateMachine.defaultState = states["Idle"];
     }
      
        if (attackClip != null)
     {
            states["Attack"] = CreateState(rootStateMachine, "Attack", attackClip, new Vector3(300, 100, 0));
        }
 
        if (walkClip != null && !addBlendTrees)
        {
            states["Walk"] = CreateState(rootStateMachine, "Walk", walkClip, new Vector3(500, 0, 0));
        }
   
      if (runClip != null && !addBlendTrees)
     {
       states["Run"] = CreateState(rootStateMachine, "Run", runClip, new Vector3(500, 100, 0));
   }
        
        if (jumpClip != null)
        {
            states["Jump"] = CreateState(rootStateMachine, "Jump", jumpClip, new Vector3(300, 200, 0));
   }
        
if (hurtClip != null)
        {
        states["Hurt"] = CreateState(rootStateMachine, "Hurt", hurtClip, new Vector3(500, 200, 0));
        }

        if (deathClip != null)
        {
   states["Death"] = CreateState(rootStateMachine, "Death", deathClip, new Vector3(300, 300, 0));
        }
  
        // Create blend trees if requested
        if (addBlendTrees && (walkClip != null || runClip != null))
      {
         CreateMovementBlendTree(rootStateMachine, states);
        }
        
// Setup transitions
        SetupTransitions(controller, states);
        
        // Create additional layers if requested
     if (createLayeredAnimator)
        {
     CreateAdditionalLayers(controller);
        }
        
// Save the controller
    AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
  
        EditorUtility.DisplayDialog("Success", 
     $"Animator Controller '{controllerName}' has been created successfully at:\n{fullPath}", 
     "OK");
        
    // Select the created controller
    Selection.activeObject = controller;
        EditorGUIUtility.PingObject(controller);
    }
    
    private bool HasAnyClip()
    {
        return idleClip != null || attackClip != null || walkClip != null || 
   runClip != null || jumpClip != null || hurtClip != null || deathClip != null;
    }
    
 private void SetupParameters(AnimatorController controller)
  {
        if (!autoSetupTriggers) return;
        
     // Add common parameters based on assigned clips
        if (attackClip != null)
controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        
        if (jumpClip != null)
 controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        
    if (hurtClip != null)
            controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
        
        if (deathClip != null)
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);
      
        if (walkClip != null || runClip != null)
        {
         controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
   }
  
        if (addBlendTrees)
        {
            controller.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
      }
    }
    
    private AnimatorState CreateState(AnimatorStateMachine stateMachine, string name, AnimationClip clip, Vector3 position)
    {
        AnimatorState state = stateMachine.AddState(name, position);
        state.motion = clip;
        return state;
    }
    
    private void SetupTransitions(AnimatorController controller, Dictionary<string, AnimatorState> states)
    {
        if (!autoSetupTriggers || states.Count == 0) return;
        
        AnimatorState idleState = states.ContainsKey("Idle") ? states["Idle"] : states.Values.First();
     
        // Attack transitions
    if (states.ContainsKey("Attack"))
        {
    CreateTransition(idleState, states["Attack"], "Attack");
            CreateTransition(states["Attack"], idleState, null, true); // Return to idle
   }
      
        // Jump transitions
        if (states.ContainsKey("Jump"))
        {
CreateTransition(idleState, states["Jump"], "Jump");
       CreateTransition(states["Jump"], idleState, null, true);
            
            if (states.ContainsKey("Walk"))
    CreateTransition(states["Walk"], states["Jump"], "Jump");
        }
        
      // Hurt transitions
        if (states.ContainsKey("Hurt"))
        {
            foreach (var state in states.Values)
        {
         if (state.name != "Hurt" && state.name != "Death")
         {
    CreateTransition(state, states["Hurt"], "Hurt");
            }
    }
         CreateTransition(states["Hurt"], idleState, null, true);
        }
    
        // Death transitions (one-way)
   if (states.ContainsKey("Death"))
     {
   foreach (var state in states.Values)
         {
    if (state.name != "Death")
      {
         CreateTransition(state, states["Death"], "Death");
        }
       }
        }
        
  // Movement transitions
        if (states.ContainsKey("Walk"))
        {
          var toWalk = CreateTransition(idleState, states["Walk"], null);
            toWalk.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
    
            var toIdle = CreateTransition(states["Walk"], idleState, null);
    toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
        }
 
      if (states.ContainsKey("Run"))
        {
            if (states.ContainsKey("Walk"))
   {
              var toRun = CreateTransition(states["Walk"], states["Run"], null);
         toRun.AddCondition(AnimatorConditionMode.Greater, 1.5f, "Speed");
        
var toWalk = CreateTransition(states["Run"], states["Walk"], null);
  toWalk.AddCondition(AnimatorConditionMode.Less, 1.5f, "Speed");
            }
            else
            {
    var toRun = CreateTransition(idleState, states["Run"], null);
        toRun.AddCondition(AnimatorConditionMode.Greater, 1.5f, "Speed");
         
      var toIdle = CreateTransition(states["Run"], idleState, null);
         toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
          }
        }
    }
    
    private AnimatorStateTransition CreateTransition(AnimatorState from, AnimatorState to, string triggerName, bool autoReturnToIdle = false)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
      transition.duration = transitionDuration;
        transition.hasExitTime = autoReturnToIdle ? true : hasExitTime;
        
        if (transition.hasExitTime)
        {
        transition.exitTime = exitTime;
        }
        
        if (!string.IsNullOrEmpty(triggerName))
        {
            transition.AddCondition(AnimatorConditionMode.If, 0, triggerName);
}
        
        return transition;
    }
    
    private void CreateMovementBlendTree(AnimatorStateMachine stateMachine, Dictionary<string, AnimatorState> states)
    {
        BlendTree blendTree;
        AnimatorState blendState = stateMachine.AddState("Movement", new Vector3(500, 0, 0));
     blendTree = new BlendTree();
        blendTree.name = "MovementBlendTree";
     blendTree.blendParameter = "MoveSpeed";
        blendTree.blendType = BlendTreeType.Simple1D;
        
        if (idleClip != null)
      blendTree.AddChild(idleClip, 0f);
        if (walkClip != null)
         blendTree.AddChild(walkClip, 1f);
        if (runClip != null)
blendTree.AddChild(runClip, 2f);
  
      blendState.motion = blendTree;
        
      // Transition from idle to blend tree
        if (states.ContainsKey("Idle"))
        {
            var toMovement = CreateTransition(states["Idle"], blendState, null);
            toMovement.AddCondition(AnimatorConditionMode.Greater, 0.01f, "MoveSpeed");
         
            var toIdle = CreateTransition(blendState, states["Idle"], null);
          toIdle.AddCondition(AnimatorConditionMode.Less, 0.01f, "MoveSpeed");
     }
        
      states["Movement"] = blendState;
    }
    
    private void CreateAdditionalLayers(AnimatorController controller)
    {
        // Create upper body layer for attacks/actions
  AnimatorControllerLayer upperBodyLayer = new AnimatorControllerLayer
        {
      name = "UpperBody",
            defaultWeight = 1f,
    blendingMode = AnimatorLayerBlendingMode.Additive,
            stateMachine = new AnimatorStateMachine()
        };
        
        upperBodyLayer.stateMachine.name = "UpperBody";
      upperBodyLayer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
   
   if (AssetDatabase.GetAssetPath(controller) != "")
        {
  AssetDatabase.AddObjectToAsset(upperBodyLayer.stateMachine, AssetDatabase.GetAssetPath(controller));
     }
        
        controller.AddLayer(upperBodyLayer);
    }
    
    private void LoadTemplateSettings()
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Player Character Template"), false, () => ApplyTemplate(AnimationType.PlayerCharacter));
  menu.AddItem(new GUIContent("Enemy Template"), false, () => ApplyTemplate(AnimationType.Enemy));
        menu.AddItem(new GUIContent("NPC Template"), false, () => ApplyTemplate(AnimationType.NPC));
        menu.ShowAsContext();
  }
    
    private void ApplyTemplate(AnimationType type)
    {
  animationType = type;
        
        switch (type)
        {
            case AnimationType.PlayerCharacter:
             transitionDuration = 0.1f;
 hasExitTime = false;
                autoSetupTriggers = true;
         addBlendTrees = true;
             createLayeredAnimator = true;
 break;
    
    case AnimationType.Enemy:
 transitionDuration = 0.15f;
        hasExitTime = true;
     exitTime = 0.8f;
   autoSetupTriggers = true;
 addBlendTrees = false;
  createLayeredAnimator = false;
    break;
      
   case AnimationType.NPC:
      transitionDuration = 0.2f;
          hasExitTime = false;
autoSetupTriggers = true;
     addBlendTrees = false;
      createLayeredAnimator = false;
          break;
        }

     Repaint();
}
    
    private void ClearFields()
    {
        controllerName = "NewCharacterAnimator";
        idleClip = null;
        attackClip = null;
  walkClip = null;
        runClip = null;
        jumpClip = null;
        hurtClip = null;
        deathClip = null;
        Repaint();
    }
}
