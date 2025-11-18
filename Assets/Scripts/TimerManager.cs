using UnityEngine;
using TMPro;
using System.Collections;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] public float remainingTime;
    [SerializeField] GameObject GameOverScreen;
    [SerializeField] GameObject ExpectedOutput;

    private bool isTimerActive = false;
    private PlayCardButton playCardButton;
    private bool isHandlingTimerExpiration = false;

    private void Start()
    {
        isTimerActive = false;
        // Find PlayCardButton to access player health
        playCardButton = FindObjectOfType<PlayCardButton>();
    }

    void Update()
    {
        if (isTimerActive && remainingTime > 0 && !isHandlingTimerExpiration)
        {
            // Use unscaledDeltaTime so timer continues even when game is paused or player tabs out
            remainingTime -= Time.unscaledDeltaTime;
        }
        else if (isTimerActive && remainingTime <= 0 && !isHandlingTimerExpiration)
        {
            // Timer reached 0 - clamp to 0 and pause timer, then trigger enemy attack animation
            remainingTime = 0;
            isHandlingTimerExpiration = true;
            PauseTimer();
            StartCoroutine(HandleTimerExpiration());
        }

        // Clamp remainingTime to prevent negative display values
        float displayTime = Mathf.Max(0, remainingTime);
        int minutes = Mathf.FloorToInt(displayTime / 60);
        int seconds = Mathf.FloorToInt(displayTime % 60);

        // Use string interpolation instead of string.Format to reduce allocations
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private IEnumerator HandleTimerExpiration()
    {
        // Wait one frame to ensure timer is paused
        yield return null;

        if (playCardButton != null && playCardButton.enemyManager != null)
        {
            // Get the current enemy
            if (playCardButton.enemyManager.counter < playCardButton.enemyManager.enemies.Count)
            {
                GameObject currentEnemy = playCardButton.enemyManager.enemies[playCardButton.enemyManager.counter];
                EnemyJumpAttack enemyJumpAttack = currentEnemy.GetComponent<EnemyJumpAttack>();

                if (enemyJumpAttack != null && playCardButton.playerCharacter != null)
                {
                    // Calculate total attack animation duration
                    // Animation sequence: jumpToPlayerDuration + 0.05f wait + attackPauseDuration + jumpBackDuration
                    float totalAnimationDuration = enemyJumpAttack.jumpToPlayerDuration + 0.05f + enemyJumpAttack.attackPauseDuration + enemyJumpAttack.jumpBackDuration;
                    
                    // Perform enemy attack animation
                    // Note: callback fires early, but we wait for full animation duration
                    enemyJumpAttack.PerformJumpAttack(playCardButton.playerCharacter.transform, () => {
                        // Callback fires early, but we handle damage after full animation
                    });
                    
                    // Wait for the full attack animation duration while timer stays at 00:00
                    yield return new WaitForSeconds(totalAnimationDuration);
                    
                    // Apply damage after full animation completes
                    playCardButton.PlayerTakeDamage(1);
                    playCardButton.UpdatePlayerHealthUI();
                    
                    // Reset timer to 60 seconds
                    remainingTime = 60;
                    
                    // Resume timer
                    isHandlingTimerExpiration = false;
                    StartTimer();
                }
                else
                {
                    // No attack animation - apply damage directly
                    playCardButton.PlayerTakeDamage(1);
                    playCardButton.UpdatePlayerHealthUI();
                    
                    // Reset timer to 60 seconds
                    remainingTime = 60;
                    
                    // Resume timer
                    isHandlingTimerExpiration = false;
                    StartTimer();
                }
            }
            else
            {
                // No enemy available - just reset timer
                remainingTime = 60;
                isHandlingTimerExpiration = false;
                StartTimer();
            }
        }
        else
        {
            // PlayCardButton or EnemyManager not found - just reset timer
            remainingTime = 60;
            isHandlingTimerExpiration = false;
            StartTimer();
        }
    }

    public void ResetTimer()
    {
        float resetTime = 60;
        remainingTime = resetTime;
    }

    public void StartTimer()
    {
        isTimerActive = true;
    }

    public void PauseTimer()
    {
        isTimerActive = false;
    }

    public void DisableScreen()
    {
        if (GameOverScreen.activeInHierarchy)
            ExpectedOutput.SetActive(false);
    }
}