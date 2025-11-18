# Card Data Assignment Fix

## Problem
Cards are being clicked but they have no CardData assigned, so the detail panel can't show any information.

## Error Message
```
[CardHoverDisplay] Card data is null on Button. Cannot show details. Make sure card has CardData assigned.
```

## Solution: Assign CardData to Cards

### Method 1: Using CardPanelManager (Recommended)

1. **Select the CardPanelManager** in your scene
2. **Find the Card Panel entry** (e.g., "CardsPanel1")
3. **Check the Cards list**:
   - If the list is empty, you need to add CardData ScriptableObjects
   - Click the "+" button to add entries
   - Drag CardData ScriptableObjects from your Project window into the list
4. **Make sure Card Prefab is assigned**:
   - The Card Prefab should be assigned in the Card Panel entry
   - This is the prefab that will be instantiated for each CardData
5. **Run the game**:
   - The CardPanelManager will automatically create cards from the CardData list
   - Each card will have its CardData assigned automatically

### Method 2: Assign CardData Manually (For Existing Cards)

If you have existing cards in the scene (like Buttons) that aren't being created by CardPanelManager:

1. **Create CardData ScriptableObjects**:
   - Right-click in Project window
   - Create → Card Collection → Card Data
   - Set the name, description, and example for each card
   - Save the ScriptableObjects

2. **Assign CardData to existing cards**:
   - Select each card GameObject in the scene
   - Find the CardHoverDisplay component
   - Either:
     - Add a CardData field and assign it manually (if you add a public CardData field)
     - Or use code to assign it (see Method 3)

### Method 3: Assign CardData via Code

If you need to assign CardData programmatically:

```csharp
// Option 1: Use CardPanelManager
CardPanelManager manager = FindObjectOfType<CardPanelManager>();
CardData myCardData = // ... get your CardData
manager.AddCardToPanel("CardsPanel1", myCardData);

// Option 2: Assign to existing card
CardHoverDisplay cardDisplay = cardGameObject.GetComponent<CardHoverDisplay>();
CardData myCardData = // ... get your CardData
cardDisplay.SetupCard(myCardData);

// Option 3: Set card data directly
CardHoverDisplay cardDisplay = cardGameObject.GetComponent<CardHoverDisplay>();
cardDisplay.SetCardData(
    name: "Card Name",
    description: "Card description",
    example: "Card example"
);
```

## Quick Fix Steps

### If Using CardPanelManager (Cards Created Automatically):

1. **Check CardPanelManager**:
   - Select CardPanelManager in scene
   - Check if Cards list has any CardData assigned
   - If empty, add CardData ScriptableObjects to the list

2. **Check Card Prefab**:
   - Make sure Card Prefab is assigned in CardPanelManager
   - The prefab should have CardHoverDisplay component

3. **Check Console**:
   - Run the game and check Console for errors
   - Look for: `[CardPanelManager] Panel 'PanelName' has no CardData assigned!`
   - This will tell you which panel is missing CardData

### If Cards are Manually Placed in Scene (Like Buttons):

1. **Select Each Card Button**:
   - Select each Button GameObject in the scene that has CardHoverDisplay component
   - In the Inspector, find the CardHoverDisplay component
   - Look for the "Card Data" section

2. **Assign CardData ScriptableObject**:
   - Find the "Card Data Asset" field in the CardHoverDisplay component
   - Drag your CardData ScriptableObject (like "Boolean (Card Data)") from the Project window into this field
   - Repeat for each card in the scene

3. **Verify Assignment**:
   - Run the game and check Console
   - You should see: `[CardHoverDisplay] Card 'Button' has CardData: boolean`
   - If you see errors, the CardData wasn't assigned correctly

4. **Create CardData ScriptableObjects** (if needed):
   - Right-click in Project → Create → Card Collection → Card Data
   - Set name, description, example for each card
   - Save the ScriptableObject
   - Assign it to the Card Data Asset field in each card's CardHoverDisplay component

## Verification

### For Manually Placed Cards:

1. **In Inspector**:
   - Select each card Button in the scene
   - Check that "Card Data Asset" field is assigned in CardHoverDisplay component
   - The field should show your CardData ScriptableObject (e.g., "Boolean (Card Data)")

2. **In Console** (when game starts):
   - You should see: `[CardHoverDisplay] Card 'Button' has CardData: boolean`
   - If you see warnings, the CardData wasn't assigned correctly

3. **When Clicking a Card**:
   - You should see:
   ```
   [CardHoverDisplay] Card clicked on Button
   [CardHoverDisplay] Selecting card: boolean
   [CardSelectedDetailPanel] ShowCardDetails called with card: boolean
   [CardSelectedDetailPanel] Detail panel activated. Panel active after: True
   ```

## Common Issues

### Issue: Cards list is empty
**Fix**: Add CardData ScriptableObjects to the Cards list in CardPanelManager

### Issue: Card Prefab is null
**Fix**: Assign a card prefab in the Card Panel entry in CardPanelManager

### Issue: Cards exist but have no CardData
**Fix**: Either use CardPanelManager to create cards, or assign CardData manually to existing cards

### Issue: CardData is null after SetupCard
**Fix**: Make sure CardData ScriptableObject is not null when calling SetupCard()

## Still Not Working?

1. Check Console for specific error messages
2. Verify CardPanelManager is in the scene
3. Verify CardData ScriptableObjects exist and are assigned
4. Verify Card Prefab is assigned and has CardHoverDisplay component
5. Check if cards are being created (look for logs like `[CardPanelManager] Creating card with data: CardName`)

