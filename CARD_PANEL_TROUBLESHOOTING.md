# Card Panel Troubleshooting Guide

## Description Panel Not Showing

If the description panel is not showing when you click on a card, check the following:

### 1. Check if CardSelectedDetailPanel Exists in Scene
- **Problem**: CardSelectedDetailPanel component doesn't exist in the scene
- **Solution**: 
  - Create a GameObject in your scene
  - Add the `CardSelectedDetailPanel` component to it
  - Make sure the GameObject is active (checkbox checked in Inspector)

### 2. Check if Detail Panel GameObject is Assigned
- **Problem**: Detail Panel GameObject is not assigned in CardSelectedDetailPanel inspector
- **Solution**:
  - Select the GameObject with CardSelectedDetailPanel component
  - In the Inspector, assign the Detail Panel GameObject to the "Detail Panel" field
  - This should be the GameObject that contains the panel UI (usually the parent of the text elements)

### 3. Check if TextMeshProUGUI Components are Assigned
- **Problem**: TextMeshProUGUI components are not assigned
- **Solution**:
  - In CardSelectedDetailPanel inspector, assign:
    - Detail Card Name (TextMeshProUGUI)
    - Detail Card Description (TextMeshProUGUI)
    - Detail Card Example (TextMeshProUGUI)

### 4. Check if EventSystem Exists
- **Problem**: No EventSystem in the scene (required for click detection)
- **Solution**:
  - Go to GameObject → UI → Event System
  - Or check if an EventSystem already exists in the scene
  - Unity automatically creates one when you create UI elements, but it might be missing

### 5. Check if Card Image has Raycast Target Enabled
- **Problem**: Card background Image doesn't have "Raycast Target" enabled
- **Solution**:
  - Select the card prefab or card GameObject
  - Find the Image component (card background)
  - Make sure "Raycast Target" checkbox is enabled
  - This is required for the card to receive click events

### 6. Check if Card Has CardData
- **Problem**: Card doesn't have card data assigned
- **Solution**:
  - Make sure the card has CardData assigned (via CardPanelManager or directly)
  - Check that cardData is not null in the CardHoverDisplay component
  - Verify that the card has a name, description, and example set

### 7. Check if Panel is Hidden Behind Other UI
- **Problem**: Panel is showing but hidden behind other UI elements
- **Solution**:
  - Check the Canvas Sort Order
  - Make sure the detail panel is on a Canvas with higher sort order
  - Check the RectTransform of the panel - make sure it's positioned correctly
  - Verify the panel's Canvas Group doesn't have Alpha set to 0

### 8. Check Console for Errors
- **Problem**: Errors in the console preventing the panel from showing
- **Solution**:
  - Open the Console window (Window → General → Console)
  - Look for errors or warnings related to:
    - CardSelectedDetailPanel
    - CardHoverDisplay
    - Missing references
  - The debug logs will tell you exactly what's missing

## Debug Steps

1. **Enable Debug Logging**: The code now has debug logs that will tell you:
   - When a card is clicked
   - If the detail panel is found
   - If card data is null
   - If the panel is activated

2. **Check Console**: When you click a card, you should see logs like:
   ```
   [CardHoverDisplay] Card clicked on CardName
   [CardHoverDisplay] Selecting card: CardName
   [CardSelectedDetailPanel] ShowCardDetails called with card: CardName
   [CardSelectedDetailPanel] Detail panel activated. Panel active: True
   ```

3. **Manual Test**: Try calling the panel directly:
   ```csharp
   CardSelectedDetailPanel panel = FindObjectOfType<CardSelectedDetailPanel>();
   if (panel != null)
   {
       CardData testCard = ScriptableObject.CreateInstance<CardData>();
       testCard.cardName = "Test Card";
       testCard.cardDescription = "Test Description";
       testCard.cardExample = "Test Example";
       panel.ShowCardDetails(testCard);
   }
   ```

## Common Setup Issues

### Issue: Panel shows but text is empty
- **Cause**: TextMeshProUGUI components not assigned or card data is empty
- **Fix**: Assign text components in inspector and make sure card data has values

### Issue: Panel doesn't appear when clicking
- **Cause**: Detail Panel GameObject is not assigned or is inactive
- **Fix**: Assign the panel GameObject and make sure it's active

### Issue: Click doesn't work
- **Cause**: Raycast Target not enabled or no EventSystem
- **Fix**: Enable Raycast Target on card Image and ensure EventSystem exists

### Issue: Panel appears but disappears immediately
- **Cause**: Another script might be hiding it, or panel is being deactivated
- **Fix**: Check if any other scripts are controlling the panel's active state

## Quick Checklist

- [ ] CardSelectedDetailPanel component exists in scene
- [ ] Detail Panel GameObject is assigned in CardSelectedDetailPanel
- [ ] Detail Panel GameObject is active
- [ ] TextMeshProUGUI components are assigned (Name, Description, Example)
- [ ] EventSystem exists in scene
- [ ] Card Image has Raycast Target enabled
- [ ] Card has CardData with name, description, and example
- [ ] Console shows no errors when clicking card
- [ ] Panel Canvas has correct sort order (not behind other UI)

## Still Not Working?

If the panel still doesn't show after checking all the above:

1. Check the Console for specific error messages
2. Verify the panel GameObject hierarchy is correct
3. Make sure the panel is not disabled by a parent GameObject
4. Check if the panel's Canvas has "Render Mode" set correctly
5. Verify the panel's RectTransform is not set to 0 size
6. Check if any UI animations or scripts are interfering

