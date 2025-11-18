using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public int counter;
    public List<CardListContainer> cardContainer = new List<CardListContainer>();
    public List<CardDisplayContainer> cardDisplayContainer = new List<CardDisplayContainer>();

    private List<Item> chosenCards = new List<Item>();
    private bool isRandomizing = false; // Prevent multiple simultaneous randomizations
    private Coroutine currentRandomizeCoroutine = null; // Track active coroutine

    [System.Serializable]
    public class CardListContainer
    {
        public List<Item> cards = new List<Item>();
    }
    [System.Serializable]
    public class CardDisplayContainer
    {
        public List<CardDisplay> cardDisplay = new List<CardDisplay>();
    }

    public GameObject grid;
    private void Start()
    {
        Time.timeScale = 1f;
        
        // In multiplayer mode, don't auto-randomize
        // The turn system will handle randomization via RPC_SyncAllCounters
        bool isMultiplayer = Photon.Pun.PhotonNetwork.IsConnected && Photon.Pun.PhotonNetwork.InRoom;
        
        if (!isMultiplayer)
        {
            currentRandomizeCoroutine = StartCoroutine(Randomize());
        }
        else
        {
            Debug.Log("[CardManager] Multiplayer mode detected - skipping auto-randomization (turn system will handle it)");
        }
    }

    /// <summary>
    /// Force stop any ongoing randomization (useful for multiplayer sync)
    /// </summary>
    public void CancelRandomization()
    {
        if (isRandomizing && currentRandomizeCoroutine != null)
        {
            Debug.Log("[CardManager] Canceling ongoing randomization");
            StopCoroutine(currentRandomizeCoroutine);
            isRandomizing = false;
            currentRandomizeCoroutine = null;
        }
    }
    
    /// <summary>
    /// Start randomization and track the coroutine (for multiplayer)
    /// </summary>
    public void StartRandomization()
    {
        currentRandomizeCoroutine = StartCoroutine(Randomize());
    }
    
    public IEnumerator Randomize()
    {
        // Prevent multiple simultaneous randomizations
        if (isRandomizing)
        {
            Debug.LogWarning($"[CardManager] Randomize already in progress - skipping duplicate call");
            yield break;
        }
        
        isRandomizing = true;
        
        if (counter < 0 || counter >= cardContainer.Count || counter >= cardDisplayContainer.Count)
        {
            Debug.LogWarning($"CardManager counter {counter} is out of range. Cannot randomize cards.");
            isRandomizing = false;
            currentRandomizeCoroutine = null;
            yield break;
        }
        
        // Simply use all cards from the container - filter out duplicate object references
        chosenCards = new List<Item>();
        HashSet<Item> seenObjects = new HashSet<Item>(); // Track object references to detect duplicates
        
        for (int idx = 0; idx < cardContainer[counter].cards.Count; idx++)
        {
            var card = cardContainer[counter].cards[idx];
            if (card != null)
            {
                if (seenObjects.Add(card))
                {
                    chosenCards.Add(card);
                }
                else
                {
                    Debug.LogWarning($"[CardManager] Duplicate object reference detected at index {idx}: '{card.cardName}' (Object: {card.name}) - skipping duplicate");
                }
            }
        }
        
        // Check if we have enough cards
        int cardsNeeded = cardDisplayContainer[counter].cardDisplay.Count;
        if (chosenCards.Count < cardsNeeded)
        {
            Debug.LogError($"[CardManager] NOT ENOUGH CARDS! Need {cardsNeeded}, but only have {chosenCards.Count} unique card objects in cardContainer[{counter}]");
            isRandomizing = false;
            currentRandomizeCoroutine = null;
            yield break;
        }
        
        for (int i = 0; i < cardDisplayContainer[counter].cardDisplay.Count; i++)
        {
            if (chosenCards.Count == 0)
            {
                Debug.LogError($"[CardManager] Ran out of cards at index {i}! This shouldn't happen.");
                break;
            }
            
            // Double-check bounds before accessing
            if (i >= cardDisplayContainer[counter].cardDisplay.Count)
            {
                Debug.LogError($"[CardManager] Index {i} out of range for cardDisplayContainer[{counter}]! Count: {cardDisplayContainer[counter].cardDisplay.Count}");
                break;
            }
            
            int rand = Random.Range(0, chosenCards.Count);
            Item selectedCard = chosenCards[rand];
            string selectedCardName = selectedCard.cardName;

            // Safely set card data with null checks (no try-catch needed - we already have bounds checks)
            if (cardDisplayContainer[counter].cardDisplay[i] == null)
            {
                Debug.LogError($"[CardManager] Card display {i} is null at counter {counter}! Skipping...");
                continue;
            }
            
            cardDisplayContainer[counter].cardDisplay[i].cardName = selectedCardName;
            
            // Safely get text component with null checks
            if (cardDisplayContainer[counter].cardDisplay[i].transform.childCount > 0)
            {
                Transform child = cardDisplayContainer[counter].cardDisplay[i].transform.GetChild(0);
                if (child != null)
                {
                    TextMeshProUGUI textComponent = child.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        textComponent.text = selectedCardName;
                    }
                }
            }
            
            // Set sprite artwork
            if (cardDisplayContainer[counter].cardDisplay[i].cardDesign != null && selectedCard.artwork != null)
            {
                cardDisplayContainer[counter].cardDisplay[i].cardDesign.sprite = selectedCard.artwork;
            }

            // Store reference for hover hints
            cardDisplayContainer[counter].cardDisplay[i].SetAssignedItem(selectedCard);

            // Yield must be outside try-catch (C# limitation)
            yield return new WaitForSeconds(0.15f);
            
            // Activate card display
            if (cardDisplayContainer[counter].cardDisplay[i] != null)
            {
                cardDisplayContainer[counter].cardDisplay[i].gameObject.SetActive(true);
                cardDisplayContainer[counter].cardDisplay[i].gameObject.transform.localScale = Vector3.one;
            }

            // Remove the selected card from the pool so it can't be selected again
            chosenCards.RemoveAt(rand);
        }
        
        isRandomizing = false;
        currentRandomizeCoroutine = null;
    }

    public void ResetCards()
    {
        chosenCards.Clear();

        // CRITICAL: Validate counter BEFORE accessing arrays
        if (counter < 0 || counter >= cardDisplayContainer.Count)
        {
            Debug.LogWarning($"[CardManager] ResetCards: Counter {counter} is out of range. CardDisplayContainer count: {cardDisplayContainer.Count}. Clamping counter to valid range.");
            // Clamp counter to valid range to prevent errors
            counter = Mathf.Clamp(counter, 0, Mathf.Max(0, cardDisplayContainer.Count - 1));
            if (cardDisplayContainer.Count == 0)
            {
                Debug.LogError("[CardManager] ResetCards: No card display containers available! Cannot reset cards.");
                return;
            }
        }
        
        // Double-check bounds before accessing
        if (counter < 0 || counter >= cardDisplayContainer.Count || cardDisplayContainer[counter] == null)
        {
            Debug.LogError($"[CardManager] ResetCards: Cannot access cardDisplayContainer[{counter}]. Counter: {counter}, Count: {cardDisplayContainer.Count}");
            return;
        }
        
        // Reset cards in the display container (grid area)
        int cardCount = cardDisplayContainer[counter].cardDisplay.Count;
        for (int i = 0; i < cardCount; i++)
        {
            // Validate card display object exists
            if (cardDisplayContainer[counter].cardDisplay[i] == null)
            {
                Debug.LogWarning($"[CardManager] ResetCards: Card display {i} is null at counter {counter}! Skipping...");
                continue;
            }
            
            // Clear card data to prevent stale values
            cardDisplayContainer[counter].cardDisplay[i].cardName = "";
            cardDisplayContainer[counter].cardDisplay[i].SetAssignedItem(null);
            
            // Clear text display
            try
            {
                if (cardDisplayContainer[counter].cardDisplay[i].transform.childCount > 0)
                {
                    Transform child = cardDisplayContainer[counter].cardDisplay[i].transform.GetChild(0);
                    if (child != null)
                    {
                        TextMeshProUGUI textComponent = child.GetComponentInChildren<TextMeshProUGUI>();
                        if (textComponent != null)
                        {
                            textComponent.text = "";
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[CardManager] Could not clear text for card {i}: {ex.Message}");
            }
            
            cardDisplayContainer[counter].cardDisplay[i].gameObject.SetActive(false);
            if (grid != null)
            {
                cardDisplayContainer[counter].cardDisplay[i].gameObject.transform.SetParent(grid.transform);
            }
            cardDisplayContainer[counter].cardDisplay[i].gameObject.transform.localScale = Vector3.one;
        }

        // Also check and clear ANY cards stuck in PlayedCard holders
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "PlayedCard" && obj.transform.childCount > 0)
            {
                // Move all children back to grid
                while (obj.transform.childCount > 0)
                {
                    Transform child = obj.transform.GetChild(0);
                    child.SetParent(grid.transform);
                    child.gameObject.SetActive(false);
                    child.localScale = Vector3.one;
                }
            }
        }
    }
}
