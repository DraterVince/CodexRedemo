# How to Assign CardData to Cards

## Overview

There are two ways to assign CardData, depending on whether you're using CardPanelManager or manually placed cards.

## Method 1: Using CardPanelManager (Recommended - Cards Created Automatically)

If you're using CardPanelManager to automatically create cards, you assign CardData to the **CardPanelManager**, NOT to the prefab itself.

### Steps:

1. **Create Your Card Prefab** (if you haven't already):
   - Create a GameObject with Image, TextMeshProUGUI, and CardHoverDisplay component
   - Save it as a prefab (drag from Hierarchy to Project window)
   - **Don't assign CardData to this prefab** - it will be assigned automatically

2. **Set Up CardPanelManager**:
   - Select the GameObject with CardPanelManager component in your scene
   - In the Inspector, find the Card Panel entry (e.g., "CardsPanel1")

3. **Assign Card Prefab**:
   - In the Card Panel entry, find "Card Prefab" field
   - Drag your card prefab from the Project window into this field

4. **Assign CardData ScriptableObjects**:
   - In the same Card Panel entry, find the "Cards" list
   - Click the "+" button to add entries to the list
   - Drag your CardData ScriptableObjects from the Project window into the list
   - Add one CardData for each card you want to display
   - Example: If you have 12 cards, add 12 CardData ScriptableObjects to the list

5. **Run the Game**:
   - CardPanelManager will automatically:
     - Instantiate the prefab for each CardData in the list
     - Assign the CardData to each card instance
     - Display the cards in the panel

### Example Setup:

```
CardPanelManager (in scene)
  └── Card Panel Entry: "CardsPanel1"
      ├── Card Prefab: [YourCardPrefab] ← ONE prefab
      └── Cards List:
          ├── [0] Boolean (Card Data) ← CardData ScriptableObject
          ├── [1] String (Card Data) ← CardData ScriptableObject
          ├── [2] Integer (Card Data) ← CardData ScriptableObject
          ├── [3] Float (Card Data) ← CardData ScriptableObject
          └── ... (more CardData ScriptableObjects)
```

**Result**: CardPanelManager creates 12 card instances from ONE prefab, each with different CardData assigned automatically.

---

## Method 2: Manually Placed Cards (Cards Already in Scene)

If you have cards already placed in the scene (like Buttons), you assign CardData directly to each card instance.

### Steps:

1. **Select Each Card in the Scene**:
   - In the Hierarchy, select each card GameObject (Button)
   - Each card should have a CardHoverDisplay component

2. **Assign CardData in Inspector**:
   - In the Inspector, find the CardHoverDisplay component
   - Find the "Card Data" section
   - Find the "Card Data Asset" field
   - Drag your CardData ScriptableObject from the Project window into this field
   - Repeat for each card in the scene

3. **Optional: Assign to Prefab**:
   - If you want all instances of a prefab to have the same CardData:
     - Select the prefab in the Project window
     - In the Inspector, find CardHoverDisplay component
     - Assign CardData to "Card Data Asset" field
   - Note: This sets a default CardData for the prefab, but you can override it per instance

### Example Setup:

```
Scene Hierarchy:
  ├── Button 1 (Card)
  │   └── CardHoverDisplay
  │       └── Card Data Asset: [Boolean (Card Data)] ← Assign here
  ├── Button 2 (Card)
  │   └── CardHoverDisplay
  │       └── Card Data Asset: [String (Card Data)] ← Assign here
  └── Button 3 (Card)
      └── CardHoverDisplay
          └── Card Data Asset: [Integer (Card Data)] ← Assign here
```

---

## Quick Comparison

| Method | Card Prefab | CardData Assignment | Use Case |
|--------|-------------|---------------------|----------|
| **CardPanelManager** | Assign to CardPanelManager | Assign to CardPanelManager's Cards list | Cards created automatically, dynamic cards |
| **Manual Cards** | Not needed (or assign default) | Assign to each card instance in scene | Cards already placed in scene, static cards |

---

## Common Questions

### Q: Do I assign CardData to the prefab itself?

**A**: 
- **Using CardPanelManager**: No, assign CardData to the CardPanelManager's Cards list, NOT to the prefab
- **Manual Cards**: You can assign CardData to the prefab as a default, but usually assign it to each instance in the scene

### Q: How many prefabs do I need?

**A**: Only ONE prefab is needed. CardPanelManager will create multiple instances of the same prefab, each with different CardData assigned.

### Q: How many CardData ScriptableObjects do I need?

**A**: One CardData ScriptableObject for each unique card. If you have 12 different cards, create 12 CardData ScriptableObjects.

### Q: Can I assign the same CardData to multiple cards?

**A**: Yes! You can assign the same CardData ScriptableObject to multiple card instances. All cards will show the same information.

### Q: What if I want different cards to have different data?

**A**: Create multiple CardData ScriptableObjects (one for each card type) and assign them to different cards.

---

## Step-by-Step: Using CardPanelManager

1. **Create CardData ScriptableObjects**:
   - Right-click in Project → Create → Card Collection → Card Data
   - Name them (e.g., "Boolean (Card Data)", "String (Card Data)")
   - Set name, description, example for each

2. **Create Card Prefab**:
   - Create GameObject with Image, TextMeshProUGUI, CardHoverDisplay
   - Save as prefab
   - **Don't assign CardData to prefab**

3. **Set Up CardPanelManager**:
   - Select CardPanelManager in scene
   - Assign Card Prefab to "Card Prefab" field
   - Add CardData ScriptableObjects to "Cards" list

4. **Run Game**:
   - Cards are created automatically with CardData assigned

---

## Troubleshooting

### Issue: Cards created but have no data
**Fix**: Make sure CardData ScriptableObjects are assigned to CardPanelManager's Cards list

### Issue: Cards not showing
**Fix**: Make sure Card Prefab is assigned in CardPanelManager

### Issue: Wrong card data showing
**Fix**: Check the order of CardData in the Cards list - cards are created in the same order

### Issue: Manual cards have no data
**Fix**: Assign CardData to each card's CardHoverDisplay component in the Inspector

