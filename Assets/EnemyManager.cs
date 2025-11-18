using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyData
    {
        public string enemyName;
        public GameObject enemyPrefab;
        public Vector3 enemyScale = Vector3.one;
        public bool useCustomScale = true;
        public int health = 100;
        public int attackDamage = 10;
    }

    public List<EnemyData> enemyTypes = new List<EnemyData>();
    public List<GameObject> enemies = new List<GameObject>();
    public int counter;

    public GameObject SpawnEnemy(string enemyName, Vector3 position, Transform parent = null)
    {
        EnemyData enemyData = enemyTypes.Find(e => e.enemyName == enemyName);
        if (enemyData == null)
        {
            Debug.LogWarning($"[EnemyManager] Enemy type '{enemyName}' not found!");
            return null;
        }

        return SpawnEnemy(enemyData, position, parent);
    }

    public GameObject SpawnEnemy(int index, Vector3 position, Transform parent = null)
    {
        if (index < 0 || index >= enemyTypes.Count)
        {
            Debug.LogWarning($"[EnemyManager] Invalid enemy index: {index}");
            return null;
        }

        return SpawnEnemy(enemyTypes[index], position, parent);
    }

    private GameObject SpawnEnemy(EnemyData enemyData, Vector3 position, Transform parent = null)
    {
        if (enemyData.enemyPrefab == null)
        {
            Debug.LogWarning($"[EnemyManager] Enemy '{enemyData.enemyName}' has no prefab assigned!");
            return null;
        }

        GameObject enemy = Instantiate(enemyData.enemyPrefab, position, Quaternion.identity, parent);

        if (enemyData.useCustomScale)
        {
            enemy.transform.localScale = enemyData.enemyScale;

            EnemyJumpAttack jumpAttack = enemy.GetComponent<EnemyJumpAttack>();
            if (jumpAttack != null)
            {
                jumpAttack.SetCharacterScale(enemyData.enemyScale);
            }
        }

        enemies.Add(enemy);
        counter = enemies.Count;

        Debug.Log($"[EnemyManager] Spawned enemy '{enemyData.enemyName}' at {position}");
        return enemy;
    }

    public void RemoveEnemy(GameObject enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
            counter = enemies.Count;
        }
    }

    public void ClearEnemies()
    {
        enemies.Clear();
        counter = 0;
    }

    public void DestroyAllEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        ClearEnemies();
    }

    public EnemyData GetEnemyData(string enemyName)
    {
        return enemyTypes.Find(e => e.enemyName == enemyName);
    }
}
