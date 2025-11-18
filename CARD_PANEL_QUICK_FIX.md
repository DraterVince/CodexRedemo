# Card Panel Quick Fix Guide

## Description Panel Not Showing - Quick Checklist

### Step 1: Check Console for Errors
When you run the game, check the Console (Window → General → Console). You should see errors like:
- `[CardSelectedDetailPanel] Detail Panel GameObject is not assigned in the inspector!`
- `[CardSelectedDetailPanel] Detail Card Name TextMeshProUGUI is not assigned in the inspector!`

These errors will tell you exactly what's missing.

### Step 2: Assign Detail Panel GameObject
1. Select the GameObject with `CardSelectedDetailPanel` component (probably "CardDescPanel")
2. In the Inspector, find the `CardSelectedDetailPanel` component
3. **Most Important**: Assign the "Detail Panel" GameObject field
   - This should be the GameObject that contains your panel UI (the one that shows/hides)
   - Usually this is a parent GameObject with an Image component (the panel background)
   - It should contain child GameObjects with the TextMeshProUGUI components

### Step 3: Assign TextMeshProUGUI Components
In the `CardSelectedDetailPanel` component, assign:
- **Detail Card Name** - TextMeshProUGUI for the card name
- **Detail Card Description** - TextMeshProUGUI for the card description
- **Detail Card Example** - TextMeshProUGUI for the card example

These are usually child GameObjects of the Detail Panel GameObject.

### Step 4: Test Click Detection
1. Make sure an EventSystem exists in the scene (GameObject → UI → Event System)
2. Make sure card prefab has Image component with "Raycast Target" enabled
3. Click a card in Play mode
4. Check Console for: `[CardHoverDisplay] Card clicked on...`

### Step 5: Verify Panel Shows
After clicking a card, you should see logs like:
```
[CardHoverDisplay] Card clicked on CardName
[CardHoverDisplay] Selecting card: CardName
[CardSelectedDetailPanel] ShowCardDetails called with card: CardName
[CardSelectedDetailPanel] Detail panel activated. Panel active after: True
```

## Common Issues

### Issue: No click logs in console
**Fix**: 
- Make sure EventSystem exists in scene
- Make sure card Image has "Raycast Target" enabled
- Make sure card is visible and not blocked by other UI

### Issue: Click logs appear but panel doesn't show
**Fix**: 
- Check if Detail Panel GameObject is assigned in CardSelectedDetailPanel
- Check if Detail Panel GameObject is active (not disabled)
- Check if parent GameObjects are active
- Check Console for errors about missing TextMeshProUGUI components

### Issue: Panel shows but is empty
**Fix**: 
- Make sure TextMeshProUGUI components are assigned in CardSelectedDetailPanel
- Make sure CardData has name, description, and example values set

### Issue: Panel shows but text is wrong
**Fix**: 
- Make sure you're assigning the correct TextMeshProUGUI components
- Check that CardData has the correct values

## Quick Test
1. Run the game
2. Click on a card
3. Check Console for errors
4. If you see errors, fix them based on the error messages
5. If no errors but panel doesn't show, check if Detail Panel GameObject is assigned and active

