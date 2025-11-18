using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute (fileName = "New Item", menuName = "Item/Create New Item")]

public class Item : ScriptableObject
{
    public int id;
    public string cardName;
    public Sprite artwork;

    [Header("Hint Settings")]
    [Tooltip("Optional hint text shown when this card is hovered in the card holder. Leave empty for no hint.")]
    [TextArea(2, 4)]
    public string hintText;
}