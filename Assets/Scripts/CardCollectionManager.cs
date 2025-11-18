using System.Collections.Generic;
using UnityEngine;

public class CardCollectionManager : MonoBehaviour
{
    public static CardCollectionManager Instance;
    
    [Header("All Cards in Game")]
    public List<CardData> allCards = new List<CardData>();
    
    private const string UNLOCKED_CARDS_KEY = "UnlockedCards";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadUnlockedCards();
    }

    public void UnlockCardsForLevel(int levelCompleted)
    {
        bool anyNewCards = false;
        
        foreach (CardData card in allCards)
        {
            if (card.unlockLevel == levelCompleted && !IsCardUnlocked(card.cardID))
            {
                UnlockCard(card.cardID);
                anyNewCards = true;
            }
        }
        
        if (anyNewCards)
        {
            SaveUnlockedCards();
        }
    }

    public void UnlockCard(int cardID)
    {
        CardData card = GetCardByID(cardID);
        if (card != null)
        {
            card.isUnlocked = true;
            SaveUnlockedCards();
        }
    }

    public bool IsCardUnlocked(int cardID)
    {
        CardData card = GetCardByID(cardID);
        return card != null && card.isUnlocked;
    }

    public CardData GetCardByID(int cardID)
    {
        return allCards.Find(c => c.cardID == cardID);
    }

    public List<CardData> GetUnlockedCards()
    {
        return allCards.FindAll(c => c.isUnlocked);
    }

    public List<CardData> GetCardsFromLevel(int level)
    {
        return allCards.FindAll(c => c.unlockLevel == level && c.isUnlocked);
    }

    private void SaveUnlockedCards()
    {
        List<int> unlockedIDs = new List<int>();
        
        foreach (CardData card in allCards)
        {
            if (card.isUnlocked)
            {
                unlockedIDs.Add(card.cardID);
            }
        }
        
        string json = JsonUtility.ToJson(new UnlockedCardsList { cardIDs = unlockedIDs });
        PlayerPrefs.SetString(UNLOCKED_CARDS_KEY, json);
        PlayerPrefs.Save();
    }

    private void LoadUnlockedCards()
    {
        if (PlayerPrefs.HasKey(UNLOCKED_CARDS_KEY))
        {
            string json = PlayerPrefs.GetString(UNLOCKED_CARDS_KEY);
            UnlockedCardsList loadedData = JsonUtility.FromJson<UnlockedCardsList>(json);
            
            if (loadedData != null && loadedData.cardIDs != null)
            {
                foreach (CardData card in allCards)
                {
                    card.isUnlocked = false;
                }
                
                foreach (int cardID in loadedData.cardIDs)
                {
                    CardData card = GetCardByID(cardID);
                    if (card != null)
                    {
                        card.isUnlocked = true;
                    }
                }
            }
        }
    }

    public void ResetAllCards()
    {
        foreach (CardData card in allCards)
        {
            card.isUnlocked = false;
        }
        
        PlayerPrefs.DeleteKey(UNLOCKED_CARDS_KEY);
        PlayerPrefs.Save();
    }

    [System.Serializable]
    private class UnlockedCardsList
    {
        public List<int> cardIDs = new List<int>();
    }
}
