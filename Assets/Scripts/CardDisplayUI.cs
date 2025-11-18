using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplayUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image cardImage;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescriptionText;
    public TextMeshProUGUI cardTypeText;
    public TextMeshProUGUI cardValueText;
    public GameObject lockedOverlay;
    public Image background;
    
    [Header("Locked State")]
    public Sprite lockedSprite;
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    
    private CardData cardData;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnCardClicked);
        }
    }

    /// <summary>
    /// Setup the card display with data
    /// </summary>
    public void SetupCard(CardData data)
    {
        cardData = data;
        
        if (cardData == null)
        {
            return;
        }

        // Update visuals based on unlock status
        if (cardData.isUnlocked)
        {
            ShowUnlockedCard();
        }
        else
        {
            ShowLockedCard();
        }
    }

    /// <summary>
    /// Show card in unlocked state
    /// </summary>
    private void ShowUnlockedCard()
    {
        // Show card image
        if (cardImage != null)
        {
            cardImage.sprite = cardData.cardImage;
            cardImage.color = Color.white;
        }
        
        // Show card name
        if (cardNameText != null)
        {
            cardNameText.text = cardData.cardName;
        }
        
        // Show description
        if (cardDescriptionText != null)
        {
            cardDescriptionText.text = cardData.cardDescription;
        }
        
        // Show type
        if (cardTypeText != null)
        {
            cardTypeText.text = cardData.cardType;
        }
        
        // Show value
        if (cardValueText != null)
        {
            cardValueText.text = cardData.cardValue.ToString();
        }
        
        // Set background color
        if (background != null)
        {
            background.color = cardData.cardColor;
        }
        
        // Hide locked overlay
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(false);
        }
    }

    /// <summary>
    /// Show card in locked state
    /// </summary>
    private void ShowLockedCard()
    {
        // Show locked image
        if (cardImage != null)
        {
            cardImage.sprite = lockedSprite;
            cardImage.color = lockedColor;
        }
        
        // Show "???" for locked cards
        if (cardNameText != null)
        {
            cardNameText.text = "???";
        }
        
        if (cardDescriptionText != null)
        {
            cardDescriptionText.text = "Complete Level " + cardData.unlockLevel + " to unlock";
        }
        
        if (cardTypeText != null)
        {
            cardTypeText.text = "Locked";
        }
        
        if (cardValueText != null)
        {
            cardValueText.text = "?";
        }
        
        // Darken background
        if (background != null)
        {
            background.color = lockedColor;
        }
        
        // Show locked overlay
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(true);
        }
    }

    /// <summary>
    /// Handle card click
    /// </summary>
    private void OnCardClicked()
    {
        if (cardData != null && cardData.isUnlocked)
        {
            // Open detailed view
            CardDetailPanel detailPanel = FindObjectOfType<CardDetailPanel>();
            if (detailPanel != null)
            {
                detailPanel.ShowCard(cardData);
            }
        }
    }
}
