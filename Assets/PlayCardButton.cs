using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PlayCardButton : MonoBehaviour
{
    public int counter = 0;

    public int newMoney;
    public int nextLevel;

    public Timer timer;
    public CardManager cardManager;
    public EnemyManager enemyManager;
    public OutputManager outputManager;
    public MoneyManager moneyManager;

    [SerializeField] GameObject GameOverScreen;
    [SerializeField] GameObject YouWinScreen;

    [SerializeField] GameObject Reward;

    [SerializeField] TextMeshProUGUI playerHP;
    [SerializeField] TextMeshProUGUI enemyHP;

    public Item card;
    public GameObject playerCharacter;
    public bool useJumpAttackAnimation = true;

    public CharacterJumpAttack playerJumpAttack;

    public List<CorrectAnswerContainer> correctAnswersContainer = new List<CorrectAnswerContainer>();
    [System.Serializable]
    public class CorrectAnswerContainer
    {
        public List<string> correctAnswers = new List<string>();
        [Tooltip("Damage dealt to enemy for each correct answer (index matches correctAnswers). Defaults to 1 if not specified.")]
        public List<float> correctAnswerDamage = new List<float>();
        [Tooltip("Damage dealt to player for wrong answers. If empty, defaults to 1.")]
        public float wrongAnswerDamage = 1f;
    }

    public Image playerHealthBar;
    public List<GameObject> enemyHealthBarObject = new List<GameObject>();
    public List<Image> enemyHealthBar = new List<Image>();

    public List<float> enemyHealthAmount = new List<float>();
    public List<float> enemyHealthTotal = new List<float>();

    [SerializeField] public float playerHealthAmount;
    [SerializeField] public float playerHealthTotal;

    Image cardDesign;

    Transform parent;
    [SerializeField] Transform playedCard;
    private Button playButton;
    private bool isMultiplayerMode = false;
    private bool delayHealthUIUpdate = false;
    // Reserved for future use
    #pragma warning disable CS0414
    private float _pendingEnemyHealth = -1f;
    private float _pendingPlayerHealth = -1f;
    #pragma warning restore CS0414

    private void Start()
    {
        cardManager = FindAnyObjectByType<CardManager>();
        playButton = GetComponent<Button>();

        DetectMultiplayerMode();
        
        // Setup player character for both singleplayer and multiplayer
        // In multiplayer, all players share control of the same character
        if (playerCharacter != null)
        {
            playerCharacter.SetActive(true);
            playerJumpAttack = playerCharacter.GetComponent<CharacterJumpAttack>();
        }
        
        // Start battle music for singleplayer mode
        if (!isMultiplayerMode)
        {
            StartBattleMusic();
        }
    }

    private void DetectMultiplayerMode()
    {
        try
        {
            System.Type photonNetworkType = System.Type.GetType("Photon.Pun.PhotonNetwork, PhotonUnityNetworking");

            if (photonNetworkType != null)
            {
                var isConnectedProp = photonNetworkType.GetProperty("IsConnected");
                var inRoomProp = photonNetworkType.GetProperty("InRoom");

                if (isConnectedProp != null && inRoomProp != null)
                {
                    bool isConnected = (bool)isConnectedProp.GetValue(null);
                    bool inRoom = (bool)inRoomProp.GetValue(null);

                    isMultiplayerMode = isConnected && inRoom;
                }
            }
        }
        catch (System.Exception)
        {
            isMultiplayerMode = false;
        }
    }

    private void Update()
    {
 if (playButton != null)
        {
        parent = transform.Find("PlayedCard");
            bool hasCard = parent != null && parent.childCount > 0;
          playButton.interactable = hasCard;
        }
        if (!isMultiplayerMode)
 {
  if (playerHealthAmount <= 0f)
            {
     GameOverScreen.SetActive(true);
                // Play defeat music
                PlayDefeatMusic();
  }

   playerHP.text = playerHealthAmount.ToString() + " / " + playerHealthTotal.ToString();
            playerHealthBar.fillAmount = playerHealthAmount / playerHealthTotal;
     }
        else
   {
  UpdateCardVisibility();
        }
  if (!delayHealthUIUpdate && enemyManager.counter < enemyHealthAmount.Count)
   {
      enemyHP.text = enemyHealthAmount[enemyManager.counter].ToString() + " / " + enemyHealthTotal[enemyManager.counter].ToString();
         enemyHealthBar[enemyManager.counter].fillAmount = enemyHealthAmount[enemyManager.counter] / enemyHealthTotal[enemyManager.counter];
  }
  if (enemyManager.counter >= enemyManager.enemies.Count)
{
     if (!isMultiplayerMode)
          {
         nextLevel = SceneManager.GetActiveScene().buildIndex + 1;
       if (nextLevel > PlayerPrefs.GetInt("levelAt"))
     {
       PlayerPrefs.SetInt("levelAt", nextLevel);
      }
    }
   YouWinScreen.SetActive(true);
                // Play victory music
                PlayVictoryMusic();
 }
    }
    private void UpdateCardVisibility()
    {
        if (!isMultiplayerMode)
        {
            return; 
        }

        bool isMyTurn = IsMyTurn();
        if (cardManager != null && cardManager.grid != null)
        {
            cardManager.grid.SetActive(isMyTurn);
        }
    }

    public void PlayButton()
    {
        if (isMultiplayerMode && !IsMyTurn())
        {
     return;
        }
    parent = transform.Find("PlayedCard");
   if (parent == null)
        {
      return;
        }
    if (parent.childCount == 0)
   {
  return;
        }
        
      playedCard = parent.GetChild(0);
  if (playedCard == null)
 {
       return;
     }
        string playedCardName = playedCard.name;

  for (int i = 0; i < correctAnswersContainer[outputManager.counter].correctAnswers.Count; i++)
        {
            if (counter == i)
            {
                if (playedCard.name == correctAnswersContainer[outputManager.counter].correctAnswers[counter])
                {
        int currentAnswerIndex = counter;
        
        // Get damage value for this answer (defaults to 1 if not specified)
        float damage = GetCorrectAnswerDamage(outputManager.counter, currentAnswerIndex);
        
        // In multiplayer, don't increment counters locally - let server control via RPC
        if (!isMultiplayerMode)
        {
            // cardManager.counter should NOT increment here - it only increments when enemy dies (next question)
            // Only playButton.counter increments (tracks which answer within current question)
            counter++;
        }
        
        if (isMultiplayerMode)
       {
            // Send to server with incremented playButton.counter and damage value
            // cardManager.counter stays the same (same question)
            // counter + 1 = next answer index for this question
            NotifyMultiplayerCardPlayed(true, currentAnswerIndex, damage);
         }

       if (useJumpAttackAnimation && playerJumpAttack != null && enemyManager.counter < enemyManager.enemies.Count)
     {
       GameObject currentEnemy = enemyManager.enemies[enemyManager.counter];
       delayHealthUIUpdate = true;

    playerJumpAttack.PerformJumpAttack(currentEnemy.transform, () => {
        if (!isMultiplayerMode)
        {
            EnemyTakeDamage(damage);
            outputManager.answerListContainer[outputManager.counter].answers[currentAnswerIndex].SetActive(true);
            UpdateEnemyHealthUI();
            delayHealthUIUpdate = false;
            
            // Check if enemy is defeated
            if (enemyHealthAmount[enemyManager.counter] <= 0f)
            {
                // Enemy defeated - move to next enemy/question
                CheckEnemyDefeat(currentAnswerIndex);
            }
            else
            {
                // Enemy still alive - increment card counter and randomize new set (like multiplayer)
                if (cardManager != null)
                {
                    // Only increment if we have more card sets available
                    if (cardManager.counter + 1 < cardManager.cardContainer.Count && 
                        cardManager.counter + 1 < cardManager.cardDisplayContainer.Count)
                    {
                        cardManager.counter++;
                        Debug.Log($"[PlayCardButton] Correct answer - enemy alive, incrementing card counter to {cardManager.counter}");
                        
                        // Reset and randomize cards for new set
                        cardManager.ResetCards();
                        cardManager.StartRandomization();
                        
                        // Reset timer for next question
                        if (timer != null)
                        {
                            timer.ResetTimer();
                            timer.StartTimer();
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[PlayCardButton] Cannot increment card counter - reached max card sets. Current: {cardManager.counter}, Max: {cardManager.cardContainer.Count}");
                    }
                }
            }
        }
        else
        {
            // In multiplayer, just update UI locally - server handles damage/logic
            outputManager.answerListContainer[outputManager.counter].answers[currentAnswerIndex].SetActive(true);
            delayHealthUIUpdate = false;
        }
       }, damage);
    }
        else
    {
        if (!isMultiplayerMode)
        {
            // Use the damage value already calculated above
            EnemyTakeDamage(damage);
            outputManager.answerListContainer[outputManager.counter].answers[currentAnswerIndex].SetActive(true);
            UpdateEnemyHealthUI();
            
            // Check if enemy is defeated
            if (enemyHealthAmount[enemyManager.counter] <= 0f)
            {
                // Enemy defeated - move to next enemy/question
                CheckEnemyDefeat(currentAnswerIndex);
            }
            else
            {
                // Enemy still alive - increment card counter and randomize new set (like multiplayer)
                if (cardManager != null)
                {
                    // Only increment if we have more card sets available
                    if (cardManager.counter + 1 < cardManager.cardContainer.Count && 
                        cardManager.counter + 1 < cardManager.cardDisplayContainer.Count)
                    {
                        cardManager.counter++;
                        Debug.Log($"[PlayCardButton] Correct answer - enemy alive, incrementing card counter to {cardManager.counter}");
                        
                        // Reset and randomize cards for new set
                        cardManager.ResetCards();
                        cardManager.StartRandomization();
                        
                        // Reset timer for next question
                        if (timer != null)
                        {
                            timer.ResetTimer();
                            timer.StartTimer();
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[PlayCardButton] Cannot increment card counter - reached max card sets. Current: {cardManager.counter}, Max: {cardManager.cardContainer.Count}");
                    }
                }
            }
        }
        else
        {
            // In multiplayer, just update UI locally - server handles damage/logic
            outputManager.answerListContainer[outputManager.counter].answers[currentAnswerIndex].SetActive(true);
        }
   }
   }
         else
                {
                    // Wrong answer - get damage value for wrong answers
                    float wrongDamage = GetWrongAnswerDamage(outputManager.counter);
                    
         if (isMultiplayerMode)
      {
         NotifyMultiplayerCardPlayed(false, -1, wrongDamage);
            }
          else
                 {
          if (useJumpAttackAnimation && playerCharacter != null && enemyManager.counter < enemyManager.enemies.Count)
     {
     GameObject currentEnemy = enemyManager.enemies[enemyManager.counter];
EnemyJumpAttack enemyJumpAttack = currentEnemy.GetComponent<EnemyJumpAttack>();

               if (enemyJumpAttack != null)
        {
         delayHealthUIUpdate = true;
    
        enemyJumpAttack.PerformJumpAttack(playerCharacter.transform, () => {
     PlayerTakeDamage(wrongDamage);
  UpdatePlayerHealthUI();
       delayHealthUIUpdate = false;
     }, wrongDamage);
          }
         else
  {
      PlayerTakeDamage(wrongDamage);
      UpdatePlayerHealthUI();
  }
   }
  else
          {
PlayerTakeDamage(wrongDamage);
         UpdatePlayerHealthUI();
         }
     }

         // In singleplayer, wrong answers also reset and randomize cards
         if (!isMultiplayerMode && cardManager != null)
         {
             cardManager.ResetCards();
             cardManager.StartRandomization();
         }
     }
             if (isMultiplayerMode && parent != null && parent.childCount > 0)
                {
    Transform cardToMove = parent.GetChild(0);
   cardToMove.SetParent(cardManager.grid.transform);
        cardToMove.gameObject.SetActive(false);
    cardToMove.localScale = Vector3.one;
      }
      
    break;
            }
        }
    }
    private void NotifyMultiplayerCardPlayed(bool wasCorrect, int answerIndex, float damage)
    {

        try
        {
            var manager = GameObject.FindObjectOfType<SharedMultiplayerGameManager>();
            if (manager != null)
            {
                manager.OnCardPlayed(wasCorrect, damage);
                if (wasCorrect)
                {
                    // Sync counter values to all players
                    // cardManager.counter stays same (same question until enemy dies)
                    // counter + 1 = next answer index within current question
                    manager.SyncCardState(cardManager.counter, counter + 1, outputManager.counter, answerIndex);
                }
                return;
            }
            var managerType = System.Type.GetType("SharedMultiplayerGameManager");
            if (managerType != null)
            {
                var managerObj = GameObject.FindObjectOfType(managerType);
                if (managerObj != null)
                {
                    var method = managerType.GetMethod("OnCardPlayed", new System.Type[] { typeof(bool), typeof(float) });
                    if (method != null)
                    {
                        method.Invoke(managerObj, new object[] { wasCorrect, damage });
                        if (wasCorrect)
                        {
                            var syncMethod = managerType.GetMethod("SyncCardState");
                            if (syncMethod != null)
                            {
                                // cardManager.counter stays same, only increment playButton.counter
                                syncMethod.Invoke(managerObj, new object[] { cardManager.counter, counter + 1, outputManager.counter, answerIndex });
                            }
                        }
                        return;
                    }
                }
            }
        }
        catch (System.Exception)
        {
        }
    }
    private bool IsMyTurn()
    {
        if (!isMultiplayerMode)
        {
            return true;
        }

        try
        {
            System.Type photonNetworkType = System.Type.GetType("Photon.Pun.PhotonNetwork, PhotonUnityNetworking");
            if (photonNetworkType != null)
            {
                var currentRoomProp = photonNetworkType.GetProperty("CurrentRoom");
                if (currentRoomProp != null)
                {
                    var currentRoom = currentRoomProp.GetValue(null);
                    if (currentRoom != null)
                    {
                        var playerCountProp = currentRoom.GetType().GetProperty("PlayerCount");
                        if (playerCountProp != null)
                        {
                            int playerCount = (int)playerCountProp.GetValue(currentRoom);
                            if (playerCount == 1)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            var integration = GameObject.FindObjectOfType(System.Type.GetType("CodexMultiplayerIntegration"));
            if (integration != null)
            {
                var method = integration.GetType().GetMethod("IsMyTurn");
                if (method != null)
                {
                    return (bool)method.Invoke(integration, null);
                }
            }
        }
        catch (System.Exception)
        {
        }
        return true;
    }

    public void PlayerTakeDamage(float damage)
    {
        playerHealthAmount -= damage;
        playerHealthBar.fillAmount = playerHealthAmount / playerHealthTotal;
    }

    public void EnemyTakeDamage(float damage)
    {
        enemyHealthAmount[enemyManager.counter] -= damage;
    }
    public void UpdateEnemyHealthUI()
    {
        if (enemyManager.counter < enemyHealthAmount.Count)
 {
enemyHP.text = enemyHealthAmount[enemyManager.counter].ToString() + " / " + enemyHealthTotal[enemyManager.counter].ToString();
     enemyHealthBar[enemyManager.counter].fillAmount = enemyHealthAmount[enemyManager.counter] / enemyHealthTotal[enemyManager.counter];
      }
    }
    public void UpdatePlayerHealthUI()
    {
   if (!isMultiplayerMode)
        {
       playerHP.text = playerHealthAmount.ToString() + " / " + playerHealthTotal.ToString();
         playerHealthBar.fillAmount = playerHealthAmount / playerHealthTotal;
        }
    }

    public void DeactivateEnemy()
    {
        enemyManager.enemies[enemyManager.counter].SetActive(false);
        enemyHealthBarObject[enemyManager.counter].SetActive(false);
    }

    public void ActivateEnemy()
    {
        enemyManager.enemies[enemyManager.counter].SetActive(true);
        enemyHealthBarObject[enemyManager.counter].SetActive(true);
    }

    public void DeactivateOutput()
    {
        outputManager.codes[outputManager.counter].SetActive(false);
        outputManager.outputs[outputManager.counter].SetActive(false);
    }

    public void ActivateOutput()
    {
        outputManager.codes[outputManager.counter].SetActive(true);
        outputManager.outputs[outputManager.counter].SetActive(true);
    }

    public void DeactivateAnswer()
    {
        if (enemyManager.counter < enemyHealthAmount.Count)
        {
            outputManager.answerList[outputManager.counter].SetActive(false);
        }
    }

    public void ActivateAnswer()
    {
        outputManager.answerList[outputManager.counter].SetActive(true);
    }

    private string GetLevelRewardLockKey()
    {
        int currentSlot = 0;
        if (NewAndLoadGameManager.Instance != null)
        {
            currentSlot = NewAndLoadGameManager.Instance.CurrentSlot;
            string userId = "";

            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
            {
                userId = PlayerDataManager.Instance.GetCurrentPlayerData().user_id;
            }

            if (!string.IsNullOrEmpty(userId))
            {
                return $"{userId}_rewardLock_Slot{currentSlot}_Level_" + SceneManager.GetActiveScene().buildIndex;
            }
        }
        return $"rewardLock_Slot{currentSlot}_Level_" + SceneManager.GetActiveScene().buildIndex;
    }

    private void CheckEnemyDefeat(int currentAnswerIndex)
    {
        if (enemyHealthAmount[enemyManager.counter] <= 0f)
        {
            // Enemy defeated - move to next enemy/question (like multiplayer AdvanceToNextEnemyOrEnd)
            DeactivateEnemy();
            DeactivateOutput();
            DeactivateAnswer();
            
            // Increment counters for next question/enemy
            enemyManager.counter++;
            outputManager.counter++;
            counter = 0; // Reset answer index for new question
            
            // Increment cardManager.counter to match the new question/enemy
            if (cardManager != null)
            {
                cardManager.counter++;
                Debug.Log($"[PlayCardButton] Enemy defeated - incrementing card counter to {cardManager.counter} for next question");
            }

            if (enemyManager.counter >= enemyManager.enemies.Count)
            {
                // All enemies defeated
                Invoke("ShowWinScreen", 1.0f);
            }
            else if (enemyManager.counter < enemyHealthAmount.Count)
            {
                // Activate next enemy
                ActivateEnemy();
                ActivateOutput();
                ActivateAnswer();

                if (isMultiplayerMode)
                {
                    NotifyStartTimer();
                    NotifyAdvanceTurn();
                }
                else
                {
                    // Reset and randomize cards for new enemy/question
                    if (cardManager != null)
                    {
                        cardManager.ResetCards();
                        cardManager.StartRandomization();
                    }
                    if (timer != null)
                    {
                        timer.ResetTimer();
                        timer.StartTimer();
                    }
                }
            }
        }
        // Note: If enemy is still alive, card counter increment is handled in PlayButton() above
    }
    private void NotifyStartTimer()
    {
        try
        {
            var manager = GameObject.FindObjectOfType<SharedMultiplayerGameManager>();
            if (manager != null)
            {
                manager.StartTimerForAllPlayers();
                return;
            }
            var managerType = System.Type.GetType("SharedMultiplayerGameManager");
            if (managerType != null)
            {
                var managerObj = GameObject.FindObjectOfType(managerType);
                if (managerObj != null)
                {
                    var method = managerType.GetMethod("StartTimerForAllPlayers");
                    if (method != null)
                    {
                        method.Invoke(managerObj, null);
                        return;
                    }
                }
            }
        }
        catch (System.Exception)
        {
        }
    }
    private void NotifyAdvanceTurn()
    {
        try
        {
            var manager = GameObject.FindObjectOfType<SharedMultiplayerGameManager>();
            if (manager != null)
            {
                manager.AdvanceTurn();
                return;
            }
            var managerType = System.Type.GetType("SharedMultiplayerGameManager");
            if (managerType != null)
            {
                var managerObj = GameObject.FindObjectOfType(managerType);
                if (managerObj != null)
                {
                    var method = managerType.GetMethod("AdvanceTurn");
                    if (method != null)
                    {
                        method.Invoke(managerObj, null);
                        return;
                    }
                }
            }
        }
        catch (System.Exception)
        {
        }
    }

    private void ShowWinScreen()
    {
        if (!isMultiplayerMode)
        {
       nextLevel = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextLevel > PlayerPrefs.GetInt("levelAt"))
            {
 PlayerPrefs.SetInt("levelAt", nextLevel);

            if (PlayerDataManager.Instance != null)
            {
                _ = PlayerDataManager.Instance.UpdateLevelsUnlocked(nextLevel);
            }
  }
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
 bool isTutorialLevel = (currentSceneIndex == 5);
       
   if (!isTutorialLevel)
            {
   string lockKey = GetLevelRewardLockKey();
     if (PlayerPrefs.GetInt(lockKey, 0) == 0)
            {
    newMoney = moneyManager.moneyCount + moneyManager.rewardAmount;
        PlayerPrefs.SetInt("moneyCount", newMoney);
    PlayerPrefs.SetInt(lockKey, 1);
            Reward.SetActive(true);

        if (PlayerDataManager.Instance != null)
        {
            _ = PlayerDataManager.Instance.UpdateMoney(newMoney);
        }
                }
            }
    else
  {
     }

  if (NewAndLoadGameManager.Instance != null)
            {
  NewAndLoadGameManager.Instance.AutoSave();
            }

      if (CardCollectionManager.Instance != null)
            {
    int currentLevel = SceneManager.GetActiveScene().buildIndex;
                CardCollectionManager.Instance.UnlockCardsForLevel(currentLevel);
            }
 }
        else
        {
        }
        if (YouWinScreen != null)
        {
            YouWinScreen.SetActive(true);
        }
    }
    
    #region Music
    
    /// <summary>
    /// Start playing battle music (for singleplayer mode)
    /// </summary>
    private void StartBattleMusic()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayBattleMusic();
        }
    }
    
    /// <summary>
    /// Play victory music (for singleplayer mode)
    /// </summary>
    private void PlayVictoryMusic()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayVictoryMusic();
        }
    }
    
    /// <summary>
    /// Play defeat music (for singleplayer mode)
    /// </summary>
    private void PlayDefeatMusic()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayDefeatMusic();
        }
    }
    
    #endregion
    
    #region Damage Helpers
    
    /// <summary>
    /// Get damage value for a correct answer at the specified index
    /// </summary>
    private float GetCorrectAnswerDamage(int questionIndex, int answerIndex)
    {
        if (questionIndex < 0 || questionIndex >= correctAnswersContainer.Count)
        {
            return 1f; // Default damage
        }
        
        var container = correctAnswersContainer[questionIndex];
        
        // If damage list is empty or doesn't have enough entries, default to 1
        if (container.correctAnswerDamage == null || 
            answerIndex < 0 || 
            answerIndex >= container.correctAnswerDamage.Count)
        {
            return 1f; // Default damage
        }
        
        float damage = container.correctAnswerDamage[answerIndex];
        
        // Ensure damage is at least 0.1 (prevent negative or zero damage)
        return Mathf.Max(0.1f, damage);
    }
    
    /// <summary>
    /// Get damage value for wrong answers
    /// </summary>
    private float GetWrongAnswerDamage(int questionIndex)
    {
        if (questionIndex < 0 || questionIndex >= correctAnswersContainer.Count)
        {
            return 1f; // Default damage
        }
        
        var container = correctAnswersContainer[questionIndex];
        float damage = container.wrongAnswerDamage;
        
        // Ensure damage is at least 0.1 (prevent negative or zero damage)
        return Mathf.Max(0.1f, damage);
    }
    
    #endregion
}