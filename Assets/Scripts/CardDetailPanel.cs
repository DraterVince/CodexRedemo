using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDetailPanel : MonoBehaviour
{
    [Header("Detail UI Elements")]
    public GameObject detailPanel;
    public Image detailCardImage;
    public TextMeshProUGUI detailCardName;
    public TextMeshProUGUI detailCardDescription;
    public TextMeshProUGUI detailCardType;
    public TextMeshProUGUI detailCardValue;
    public TextMeshProUGUI detailUnlockLevel;
    public Button closeButton;

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideCard);
        }
        
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Show card details
    /// </summary>
    public void ShowCard(CardData card)
    {
        if (card == null || !card.isUnlocked)
        {
            return;
        }
        
        // Populate detail UI
        if (detailCardImage != null)
        {
            detailCardImage.sprite = card.cardImage;
        }
        
        if (detailCardName != null)
        {
            detailCardName.text = card.cardName;
        }
        
        if (detailCardDescription != null)
        {
            detailCardDescription.text = card.cardDescription;
        }
        
        if (detailCardType != null)
        {
            detailCardType.text = "Type: " + card.cardType;
        }
        
        if (detailCardValue != null)
        {
            detailCardValue.text = "Value: " + card.cardValue;
        }
        
        if (detailUnlockLevel != null)
        {
            detailUnlockLevel.text = "Unlocked from Level " + card.unlockLevel;
        }
        
        // Show panel
        if (detailPanel != null)
        {
            detailPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hide card details
    /// </summary>
    public void HideCard()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }
}
