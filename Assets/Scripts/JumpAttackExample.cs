using UnityEngine;

public class JumpAttackExample : MonoBehaviour
{
    [Header("Example References")]
    public CharacterJumpAttack playerAttack;
    public EnemyJumpAttack enemyAttack;
    public Transform targetEnemy;
    public Transform targetPlayer;

    void Start()
    {
        if (playerAttack == null)
   {
  playerAttack = FindObjectOfType<CharacterJumpAttack>();
        }
        
        if (enemyAttack == null)
        {
            enemyAttack = FindObjectOfType<EnemyJumpAttack>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && playerAttack != null && targetEnemy != null)
  {
            TestPlayerAttack();
        }

  if (Input.GetKeyDown(KeyCode.E) && enemyAttack != null && targetPlayer != null)
        {
            TestEnemyAttack();
        }
    }

    void TestPlayerAttack()
    {
        Debug.Log("Player attacking enemy!");
     
        playerAttack.PerformJumpAttack(targetEnemy, () => {
            Debug.Log("Player hit the enemy! Apply damage here.");
        });
    }

    void TestEnemyAttack()
    {
Debug.Log("Enemy attacking player!");
        
        enemyAttack.PerformJumpAttack(targetPlayer, () => {
            Debug.Log("Enemy hit the player! Apply damage here.");
        });
    }

    void AttackToPosition(Vector3 targetPosition)
    {
        if (playerAttack != null)
     {
            playerAttack.PerformJumpAttack(targetPosition, () => {
 Debug.Log($"Reached position: {targetPosition}");
            });
        }
    }

    void CheckIfAttacking()
    {
        if (playerAttack != null && playerAttack.IsAnimating())
        {
    Debug.Log("Player is currently performing an attack!");
        }
    }

    void CancelAttack()
    {
        if (playerAttack != null)
  {
            playerAttack.StopAnimation();
     Debug.Log("Attack cancelled, player returned to original position");
     }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
  style.normal.textColor = Color.white;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label("Jump Attack Example Controls:", style);
        GUILayout.Label("Press P - Player Attack", style);
        GUILayout.Label("Press E - Enemy Attack", style);
        GUILayout.EndArea();
    }
}
