using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Helper script to quickly setup jump attack animations on characters
/// Right-click on a GameObject and select "Setup Jump Attack" from the context menu
/// </summary>
public class JumpAttackSetupHelper : MonoBehaviour
{
    [MenuItem("GameObject/Battle System/Setup Player Jump Attack", false, 0)]
    static void SetupPlayerJumpAttack(MenuCommand menuCommand)
    {
        GameObject selected = menuCommand.context as GameObject;
        if (selected != null)
        {
            var componentType = System.Type.GetType("CharacterJumpAttack, Assembly-CSharp");
            if (componentType != null && selected.GetComponent(componentType) == null)
            {
                selected.AddComponent(componentType);
                Debug.Log($"CharacterJumpAttack component added to {selected.name}");
                EditorUtility.SetDirty(selected);
            }
            else if (selected.GetComponent(componentType) != null)
            {
                Debug.LogWarning($"{selected.name} already has a CharacterJumpAttack component.");
            }
        }
    }

    [MenuItem("GameObject/Battle System/Setup Enemy Jump Attack", false, 0)]
    static void SetupEnemyJumpAttack(MenuCommand menuCommand)
    {
        GameObject selected = menuCommand.context as GameObject;
        if (selected != null)
        {
            var componentType = System.Type.GetType("EnemyJumpAttack, Assembly-CSharp");
            if (componentType != null && selected.GetComponent(componentType) == null)
            {
                selected.AddComponent(componentType);
                Debug.Log($"EnemyJumpAttack component added to {selected.name}");
                EditorUtility.SetDirty(selected);
            }
            else if (selected.GetComponent(componentType) != null)
            {
                Debug.LogWarning($"{selected.name} already has an EnemyJumpAttack component.");
            }
        }
    }

    [MenuItem("GameObject/Battle System/Setup All Enemies in Scene", false, 0)]
    static void SetupAllEnemies()
    {
        var enemyManager = Object.FindObjectOfType(System.Type.GetType("EnemyManager, Assembly-CSharp"));
        if (enemyManager != null)
        {
            var enemiesField = enemyManager.GetType().GetField("enemies");
            if (enemiesField != null)
            {
                var enemies = enemiesField.GetValue(enemyManager) as System.Collections.IList;
                if (enemies != null)
                {
                    int count = 0;
                    var componentType = System.Type.GetType("EnemyJumpAttack, Assembly-CSharp");
                    foreach (var enemy in enemies)
                    {
                        GameObject enemyGO = enemy as GameObject;
                        if (enemyGO != null && componentType != null && enemyGO.GetComponent(componentType) == null)
                        {
                            enemyGO.AddComponent(componentType);
                            EditorUtility.SetDirty(enemyGO);
                            count++;
                        }
                    }
                    Debug.Log($"Added EnemyJumpAttack component to {count} enemies.");
                    return;
                }
            }
        }
        Debug.LogWarning("No EnemyManager found in scene or enemies list is empty.");
    }
}
#endif
