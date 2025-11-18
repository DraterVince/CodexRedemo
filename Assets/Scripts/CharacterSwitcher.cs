using UnityEngine;
using System.Collections.Generic;

public class CharacterSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        [Header("Basic Info")]
        public string characterName;
        
        [TextArea(3, 6)]
        [Tooltip("Description shown in character selection screen")]
        public string characterDescription = "A mysterious warrior...";
        
        [Header("Visual")]
        public Sprite characterSprite;
        public RuntimeAnimatorController animatorController;
        public GameObject characterPrefab;
        
        [Header("Unlock Settings")]
        public bool isUnlocked = true;
        public int unlockCost = 0;
        
        [Header("Scale Settings")]
        public Vector3 characterScale = Vector3.one;
        public bool useCustomScale = false;
    }
    
    public List<CharacterData> characters = new List<CharacterData>();
    public int currentCharacterIndex = 0;
    
    public SpriteRenderer characterSpriteRenderer;
    public Animator characterAnimator;
    public CharacterJumpAttack jumpAttackComponent;
    public Transform characterPrefabContainer;
    
    public string saveKey = "SelectedCharacter";
    public bool autoSave = true;
    
    private GameObject currentCharacterInstance;

    void Start()
    {
        if (characterSpriteRenderer == null)
            characterSpriteRenderer = GetComponent<SpriteRenderer>();
        
        if (characterAnimator == null)
            characterAnimator = GetComponent<Animator>();
        
        if (jumpAttackComponent == null)
            jumpAttackComponent = GetComponent<CharacterJumpAttack>();
        
        if (characterPrefabContainer == null)
            characterPrefabContainer = transform;
        
        if (autoSave)
        {
            LoadUnlockStates();
        }
        
        if (autoSave && PlayerPrefs.HasKey(saveKey))
        {
            currentCharacterIndex = PlayerPrefs.GetInt(saveKey, 0);
        }
        
        ApplyCharacter(currentCharacterIndex);
    }

    public void SwitchToCharacter(int index)
    {
        if (index < 0 || index >= characters.Count)
        {
            return;
        }
        
        if (!characters[index].isUnlocked)
        {
            return;
        }
        
        currentCharacterIndex = index;
        ApplyCharacter(currentCharacterIndex);
        
        if (autoSave)
        {
            PlayerPrefs.SetInt(saveKey, currentCharacterIndex);
            PlayerPrefs.Save();
            
            if (NewAndLoadGameManager.Instance != null)
            {
                NewAndLoadGameManager.Instance.AutoSave();
            }
        }
    }
    
    public void SwitchToCharacter(string characterName)
    {
        int index = characters.FindIndex(c => c.characterName == characterName);
        if (index >= 0)
        {
            SwitchToCharacter(index);
        }
    }

    public void NextCharacter()
    {
        int nextIndex = (currentCharacterIndex + 1) % characters.Count;
        
        int attempts = 0;
        while (!characters[nextIndex].isUnlocked && attempts < characters.Count)
        {
            nextIndex = (nextIndex + 1) % characters.Count;
            attempts++;
        }
        
        if (characters[nextIndex].isUnlocked)
        {
            SwitchToCharacter(nextIndex);
        }
    }
    
    public void PreviousCharacter()
    {
        int prevIndex = currentCharacterIndex - 1;
        if (prevIndex < 0) prevIndex = characters.Count - 1;
        
        int attempts = 0;
        while (!characters[prevIndex].isUnlocked && attempts < characters.Count)
        {
            prevIndex--;
            if (prevIndex < 0) prevIndex = characters.Count - 1;
            attempts++;
        }
        
        if (characters[prevIndex].isUnlocked)
        {
            SwitchToCharacter(prevIndex);
        }
    }
    
    public bool UnlockCharacter(int index, int playerMoney = -1)
    {
        if (index < 0 || index >= characters.Count)
            return false;
        
        if (characters[index].isUnlocked)
        {
            return true;
        }
        
        if (playerMoney >= 0 && playerMoney < characters[index].unlockCost)
        {
            Debug.Log($"Not enough money to unlock {characters[index].characterName}. Need {characters[index].unlockCost}, have {playerMoney}");
            return false;
        }
        
        characters[index].isUnlocked = true;
        
        if (autoSave)
        {
            PlayerPrefs.SetInt($"Character_{characters[index].characterName}_Unlocked", 1);
            PlayerPrefs.Save();
        }
        
        return true;
    }
    
    public bool UnlockCharacter(string characterName, int playerMoney = -1)
    {
        int index = characters.FindIndex(c => c.characterName == characterName);
        return index >= 0 && UnlockCharacter(index, playerMoney);
    }
    
    private void ApplyCharacter(int index)
    {
        if (index < 0 || index >= characters.Count)
            return;
        
        CharacterData character = characters[index];
        
        if (currentCharacterInstance != null)
        {
            Destroy(currentCharacterInstance);
        }
        
        if (character.useCustomScale)
        {
            transform.localScale = character.characterScale;
            
            if (jumpAttackComponent != null)
            {
                jumpAttackComponent.SetCharacterScale(character.characterScale);
            }
        }
        
        if (characterSpriteRenderer != null && character.characterSprite != null)
        {
            characterSpriteRenderer.sprite = character.characterSprite;
        }
        
        if (characterAnimator != null && character.animatorController != null)
        {
            characterAnimator.runtimeAnimatorController = character.animatorController;
            
            if (jumpAttackComponent != null)
            {
                jumpAttackComponent.SetCharacterAnimator(characterAnimator);
            }
        }
        
        if (character.characterPrefab != null)
        {
            currentCharacterInstance = Instantiate(character.characterPrefab, characterPrefabContainer);
            currentCharacterInstance.transform.localPosition = Vector3.zero;
            currentCharacterInstance.transform.localRotation = Quaternion.identity;
            
            Animator prefabAnimator = currentCharacterInstance.GetComponent<Animator>();
            if (prefabAnimator == null)
            {
                prefabAnimator = currentCharacterInstance.GetComponentInChildren<Animator>();
            }
            
            if (prefabAnimator != null && jumpAttackComponent != null)
            {
                jumpAttackComponent.SetCharacterAnimator(prefabAnimator);
            }
            
            if (character.useCustomScale)
            {
                currentCharacterInstance.transform.localScale = character.characterScale;
            }
        }
    }
    
    public CharacterData GetCurrentCharacter()
    {
        if (currentCharacterIndex >= 0 && currentCharacterIndex < characters.Count)
        {
            return characters[currentCharacterIndex];
        }
        return null;
    }
    
    public bool IsCharacterUnlocked(int index)
    {
        return index >= 0 && index < characters.Count && characters[index].isUnlocked;
    }
    
    public bool IsCharacterUnlocked(string characterName)
    {
        int index = characters.FindIndex(c => c.characterName == characterName);
        return IsCharacterUnlocked(index);
    }
    
    public void LoadUnlockStates()
    {
        foreach (var character in characters)
        {
            if (PlayerPrefs.HasKey($"Character_{character.characterName}_Unlocked"))
            {
                character.isUnlocked = PlayerPrefs.GetInt($"Character_{character.characterName}_Unlocked") == 1;
            }
        }
    }
}
