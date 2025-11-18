using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Card", menuName = "Card Collection/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Card Information")]
    public string cardName;
    public int cardID;
    public int unlockLevel; // Which level unlocks this card
    
    [Header("Visual")]
    public Sprite cardImage;
    public Color cardColor = Color.white;
    
    [Header("Description")]
    [TextArea(3, 6)]
    public string cardDescription;
    
    [Header("Example")]
    [TextArea(2, 4)]
    public string cardExample;
    
    [Header("Stats (Optional)")]
    public string cardType; // e.g., "Attack", "Defense", "Special"
    public int cardValue;
    
    [Header("Unlock Status")]
    public bool isUnlocked = false;
}
