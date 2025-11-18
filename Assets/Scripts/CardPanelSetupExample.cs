using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Example script showing how to use CardPanelManager to set card values programmatically
/// </summary>
public class CardPanelSetupExample : MonoBehaviour
{
    [Header("Card Panel Manager")]
    public CardPanelManager cardPanelManager;

    [Header("Example Card Data")]
    public Sprite exampleCardBackground;
    public Color exampleCardColor = Color.white;

    private void Start()
    {
        // Example: Setup cards programmatically
        SetupExampleCards();
    }

    /// <summary>
    /// Example: Setup cards with data
    /// </summary>
    private void SetupExampleCards()
    {
        if (cardPanelManager == null)
        {
            cardPanelManager = FindObjectOfType<CardPanelManager>();
        }

        if (cardPanelManager == null)
        {
            Debug.LogError("[CardPanelSetupExample] CardPanelManager not found!");
            return;
        }

        // Example 1: Set card data for a specific card in a panel
        cardPanelManager.SetCardData(
            panelName: "CardsPanel1",
            cardIndex: 0,
            name: "Example Card 1",
            description: "This is an example card description that shows how the card system works.",
            example: "Example: This card can be used to demonstrate functionality.",
            backgroundSprite: exampleCardBackground,
            backgroundColor: exampleCardColor
        );

        // Example 2: Set multiple cards
        for (int i = 0; i < 12; i++)
        {
            cardPanelManager.SetCardData(
                panelName: "CardsPanel1",
                cardIndex: i,
                name: $"Card {i + 1}",
                description: $"Description for card {i + 1}. This card has some interesting properties.",
                example: $"Example {i + 1}: Use this card to see how it works.",
                backgroundSprite: exampleCardBackground,
                backgroundColor: exampleCardColor
            );
        }
    }

    /// <summary>
    /// Example: Add a new card to a panel
    /// </summary>
    public void AddNewCard()
    {
        if (cardPanelManager == null) return;

        // Create card data
        CardData newCard = ScriptableObject.CreateInstance<CardData>();
        newCard.cardName = "New Card";
        newCard.cardDescription = "A newly added card";
        newCard.cardExample = "Example: This card was added dynamically";
        newCard.cardImage = exampleCardBackground;
        newCard.cardColor = exampleCardColor;

        // Add to panel
        cardPanelManager.AddCardToPanel("CardsPanel1", newCard);
    }

    /// <summary>
    /// Example: Update a card's data
    /// </summary>
    public void UpdateCard(int cardIndex, string name, string description, string example)
    {
        if (cardPanelManager == null) return;

        cardPanelManager.SetCardData(
            panelName: "CardsPanel1",
            cardIndex: cardIndex,
            name: name,
            description: description,
            example: example,
            backgroundSprite: exampleCardBackground,
            backgroundColor: exampleCardColor
        );
    }
}
