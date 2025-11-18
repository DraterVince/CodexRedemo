using UnityEngine;

public class CharacterLoader : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Save key used by CharacterSelectionUI (default: SelectedCharacter)")]
    [SerializeField] private string saveKey = "SelectedCharacter";
    
    [Tooltip("Load character on Start")]
  [SerializeField] private bool loadOnStart = true;
    
    [Tooltip("Default character index if no save found")]
    [SerializeField] private int defaultCharacterIndex = 0;

    private void Start()
    {
        if (loadOnStart)
 {
LoadSelectedCharacter();
        }
    }

    public void LoadSelectedCharacter()
    {
   var characterSwitcher = GetComponent(System.Type.GetType("CharacterSwitcher"));
        
if (characterSwitcher == null)
        {
   Debug.LogError("[CharacterLoader] Cannot load character - CharacterSwitcher component not found on this GameObject!");
            return;
   }

        try
    {
     var charactersField = characterSwitcher.GetType().GetField("characters");
        if (charactersField == null)
 {
     Debug.LogError("[CharacterLoader] Could not find 'characters' field in CharacterSwitcher!");
     return;
}

var characters = charactersField.GetValue(characterSwitcher) as System.Collections.IList;
 
       if (characters == null || characters.Count == 0)
     {
             Debug.LogWarning("[CharacterLoader] No characters available in CharacterSwitcher!");
              return;
   }

     int savedIndex = PlayerPrefs.GetInt(saveKey, defaultCharacterIndex);
  
            if (savedIndex < 0 || savedIndex >= characters.Count)
            {
      Debug.LogWarning($"[CharacterLoader] Saved character index {savedIndex} is invalid. Using default index {defaultCharacterIndex}.");
     savedIndex = defaultCharacterIndex;
 }

         var characterData = characters[savedIndex];
   var isUnlockedField = characterData.GetType().GetField("isUnlocked");
            bool isUnlocked = isUnlockedField != null && (bool)isUnlockedField.GetValue(characterData);
            
   if (!isUnlocked)
     {
          Debug.LogWarning($"[CharacterLoader] Saved character at index {savedIndex} is locked! Using first unlocked character.");
      savedIndex = FindFirstUnlockedCharacter(characters);
       }

  var switchMethod = characterSwitcher.GetType().GetMethod("SwitchToCharacter", new System.Type[] { typeof(int) });
            if (switchMethod != null)
  {
       switchMethod.Invoke(characterSwitcher, new object[] { savedIndex });
       
         var characterNameField = characters[savedIndex].GetType().GetField("characterName");
      string charName = characterNameField != null ? (string)characterNameField.GetValue(characters[savedIndex]) : "Unknown";
         
                Debug.Log($"[CharacterLoader] Loaded character: {charName} (Index: {savedIndex})");
    }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[CharacterLoader] Error loading character: {ex.Message}");
 }
    }

    private int FindFirstUnlockedCharacter(System.Collections.IList characters)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            var characterData = characters[i];
   var isUnlockedField = characterData.GetType().GetField("isUnlocked");
 
      if (isUnlockedField != null && (bool)isUnlockedField.GetValue(characterData))
 {
     return i;
 }
        }
        
  if (characters.Count > 0)
 {
   var firstChar = characters[0];
        var isUnlockedField = firstChar.GetType().GetField("isUnlocked");
       if (isUnlockedField != null)
  {
                isUnlockedField.SetValue(firstChar, true);
         }
            return 0;
  }
      
return 0;
    }
}
