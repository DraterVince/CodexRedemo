# Card Panel Setup Guide

This guide explains how to set up the card panel system with click/select functionality.

## Overview

The card panel system allows you to:
- Create card panels with lists of cards
- Display only card name on the card itself (with background image)
- Display full card details (name, description, example) in a detail panel when a card is clicked/selected
- Set card values programmatically
- Manage multiple card panels with different card lists
- Click a card to select it and display details in the panel

## Components

### 1. CardData (ScriptableObject)
- Contains card information: name, description, example, image, color
- Can be created in Unity: Right-click → Create → Card Collection → Card Data

### 2. CardHoverDisplay (Component)
- Handles individual card display and click/select detection
- Displays only the card name on the card itself
- Attaches to each card GameObject in the card list
- Triggers detail panel to show when clicked/selected
- Highlights selected card with a different color

### 3. CardSelectedDetailPanel (Component)
- Displays card details when a card is clicked/selected
- Shows name, description, and example (description and example are ONLY shown here, not on the card)
- Panel remains visible until another card is selected or cleared

### 4. CardPanelManager (Component)
- Manages all card panels
- Allows setting card values programmatically
- Handles card creation and panel setup

## Setup Instructions

### Step 1: Create Card Prefab

1. Create a new GameObject (e.g., "CardPrefab")
2. Add an Image component (this will be the card background)
   - Enable "Raycast Target" so it can receive click events
3. Add a Button component (recommended, for click detection)
   - OR use Image with Raycast Target enabled (CardHoverDisplay implements IPointerClickHandler)
4. Add a child GameObject for text:
   - CardNameText (TextMeshProUGUI) - **Only the name is displayed on the card itself**
   - **Note**: Description and Example are NOT displayed on the card, only in the detail panel when selected
5. Add CardHoverDisplay component to the root GameObject
6. In CardHoverDisplay inspector:
   - Assign Card Background (Image)
   - Assign Card Name Text (TextMeshProUGUI)
   - **Do NOT assign description or example text** - these are only shown in the detail panel
   - Set Selected Color (color to highlight selected card)
   - Optional: Assign Selection Indicator (Image to show when selected)
   - **Note**: Card Data Asset field is optional - CardData is assigned automatically by CardPanelManager
7. **Important**: Do NOT add a Button child GameObject - the parent CardPrefab handles clicks
8. Save as a prefab

### Step 2: Create Detail Panel

1. Create a new GameObject (e.g., "CardDetailPanel")
2. Add an Image component for the panel background
3. Add child GameObjects for text:
   - DetailCardName (TextMeshProUGUI)
   - DetailCardDescription (TextMeshProUGUI)
   - DetailCardExample (TextMeshProUGUI)
4. Add CardSelectedDetailPanel component
5. In CardSelectedDetailPanel inspector:
   - Assign Detail Panel (GameObject)
   - Assign Detail Card Name (TextMeshProUGUI)
   - Assign Detail Card Description (TextMeshProUGUI)
   - Assign Detail Card Example (TextMeshProUGUI)
   - **Note**: Card background image is not displayed - the panel uses its own background
6. Position the panel where you want it to appear (it will display when a card is selected)

### Step 3: Create Card Panel

1. Create a new GameObject (e.g., "CardsPanel1")
2. Add a ScrollRect component (optional, for scrolling)
3. Create a child GameObject (e.g., "CardListContainer")
   - Add ContentSizeFitter component
   - Add GridLayoutGroup component (optional, for grid layout)
4. Add CardPanelManager component to the scene (can be on any GameObject)
5. In CardPanelManager inspector:
   - Add a new Card Panel entry
   - Set Panel Name (e.g., "CardsPanel1")
   - Assign Card List Container (the container with GridLayoutGroup)
   - Assign Card Prefab (the prefab you created)
   - Add Card Data ScriptableObjects to the Cards list (optional, can be set programmatically)
   - Assign Selected Detail Panel (the CardSelectedDetailPanel component)

### Step 4: Setup Card Values Programmatically

```csharp
// Get the CardPanelManager
CardPanelManager cardPanelManager = FindObjectOfType<CardPanelManager>();

// Set card data for a specific card
cardPanelManager.SetCardData(
    panelName: "CardsPanel1",
    cardIndex: 0,
    name: "Card Name",
    description: "Card description text",
    example: "Example: This is how the card works",
    backgroundSprite: cardSprite,  // Optional
    backgroundColor: Color.white    // Optional
);

// Add a new card to a panel
CardData newCard = ScriptableObject.CreateInstance<CardData>();
newCard.cardName = "New Card";
newCard.cardDescription = "Description";
newCard.cardExample = "Example text";
cardPanelManager.AddCardToPanel("CardsPanel1", newCard);
```

## Usage Examples

### Example 1: Setup 12 Cards with Data

```csharp
CardPanelManager cardPanelManager = FindObjectOfType<CardPanelManager>();

for (int i = 0; i < 12; i++)
{
    cardPanelManager.SetCardData(
        panelName: "CardsPanel1",
        cardIndex: i,
        name: $"Card {i + 1}",
        description: $"Description for card {i + 1}",
        example: $"Example {i + 1}: Use this card to..."
    );
}
```

### Example 2: Update Card on Button Click

```csharp
public void OnUpdateCardButtonClicked()
{
    CardPanelManager cardPanelManager = FindObjectOfType<CardPanelManager>();
    cardPanelManager.SetCardData(
        panelName: "CardsPanel1",
        cardIndex: 0,
        name: "Updated Card Name",
        description: "Updated description",
        example: "Updated example"
    );
}
```

## Features

- **Minimal Card Display**: Only card name and background are shown on the card itself
- **Click/Select Detection**: Cards can be clicked to select them and display details
- **Detail Panel**: Description and example are ONLY shown in the detail panel when selected, not on the card
- **Selection Highlighting**: Selected cards are highlighted with a different color
- **Multiple Panels**: Support for multiple card panels with different card lists
- **Programmatic Control**: Set card values at runtime via code
- **Auto-Setup**: Automatically finds and connects UI elements if not manually assigned
- **Toggle Selection**: Clicking a selected card again will deselect it and hide the detail panel

## Tips

1. **Card Display**: Only add the card name TextMeshProUGUI to the card prefab - description and example are not needed
2. **Click Detection**: CardPrefab should have either:
   - Button component (recommended) - easier click detection
   - Image component with "Raycast Target" enabled - works with IPointerClickHandler
3. **Important**: Do NOT add a Button child GameObject - it will block clicks from reaching the parent CardPrefab
4. Make sure card prefab has an Image component with "Raycast Target" enabled (for click detection if not using Button)
5. Card values (including description and example) can be set programmatically - they will appear in the detail panel when selected
6. Use GridLayoutGroup on the card list container for automatic card layout
7. The card itself only shows the name and background - keep it simple and clean
8. Selected cards are highlighted with the selected color - customize this in the CardHoverDisplay inspector
9. Click a selected card again to deselect it and hide the detail panel

## Troubleshooting

### Description Panel Not Showing

If the description panel is not showing when you click a card, check these in order:

1. **Check Console for Errors**: 
   - Open Console (Window → General → Console)
   - Look for errors related to CardSelectedDetailPanel or CardHoverDisplay
   - The debug logs will tell you exactly what's missing

2. **CardSelectedDetailPanel Exists in Scene**:
   - Make sure a GameObject with `CardSelectedDetailPanel` component exists in your scene
   - The GameObject should be active (checkbox checked)

3. **Detail Panel GameObject is Assigned**:
   - Select the GameObject with CardSelectedDetailPanel component
   - In the Inspector, make sure "Detail Panel" is assigned (the GameObject that contains the panel UI)
   - This is the GameObject that will be shown/hidden

4. **TextMeshProUGUI Components are Assigned**:
   - In CardSelectedDetailPanel inspector, assign:
     - Detail Card Name (TextMeshProUGUI)
     - Detail Card Description (TextMeshProUGUI)
     - Detail Card Example (TextMeshProUGUI)

5. **EventSystem Exists**:
   - Make sure an EventSystem exists in the scene (required for click detection)
   - Go to GameObject → UI → Event System if missing
   - Unity usually creates one automatically when creating UI

6. **Card Has Raycast Target Enabled**:
   - Select the card prefab
   - Make sure the Image component (card background) has "Raycast Target" enabled
   - This is required for click detection

7. **Card Has CardData**:
   - Make sure the card has CardData assigned (via CardPanelManager or directly)
   - Verify the card has name, description, and example values set

8. **Panel is Not Hidden**:
   - Check if the panel GameObject is active
   - Check if parent GameObjects are active
   - Verify the panel is not behind other UI (check Canvas sort order)

### Other Issues

- **Click not working**: Make sure the card has an Image component with "Raycast Target" enabled and EventSystem exists
- **Cards not displaying**: Verify that Card Prefab and Card List Container are assigned correctly
- **Card name not showing**: Make sure Card Name Text (TextMeshProUGUI) is assigned in CardHoverDisplay
- **Description/Example not showing**: These only appear in the detail panel when a card is selected - make sure the detail panel has these TextMeshProUGUI components assigned
- **Card not highlighting when selected**: Check that Selected Color is set in CardHoverDisplay inspector

### Debug Information

The system now includes debug logging. When you click a card, check the Console for:
- `[CardHoverDisplay] Card clicked on...` - Confirms click is detected
- `[CardHoverDisplay] Selecting card:...` - Confirms card selection
- `[CardSelectedDetailPanel] ShowCardDetails called...` - Confirms panel method is called
- `[CardSelectedDetailPanel] Detail panel activated...` - Confirms panel is activated

If you see errors instead, they will tell you exactly what's missing.

## Notes

- **Card Display**: Only the card name is displayed on the card itself - description and example are stored but only shown in the detail panel
- **Click to Select**: Cards are selected by clicking on them - only one card can be selected at a time
- **Detail Panel**: The detail panel shows the selected card's name, description, and example
- Cards are automatically instantiated when the panel is set up
- The system supports multiple card panels with different names
- Card data (name, description, example) can be set via ScriptableObjects or programmatically
- The detail panel remains visible until another card is selected or the selection is cleared
- Keep card prefabs simple - they only need a background image and name text
- Clicking a selected card again will deselect it and hide the detail panel
