# Prefab Detection Troubleshooting Guide

## ✅ Your Path is CORRECT!

The path `Assets/Resources/Characters/Daimyo.prefab` is **correct**. The file exists in the right location.

---

## 🔍 Why It Might Not Be Detected

### Issue 1: Unity Resources Folder Not Recognized
**Problem**: Unity hasn't recognized the Resources folder properly.

**Solution**:
1. In Unity, go to **Assets** → **Refresh** (or press `Ctrl+R`)
2. Wait for Unity to reimport all assets
3. Check if the prefab appears in the Project window
4. Try building the project (Resources.Load only works in builds or Play mode)

### Issue 2: Prefab Not Properly Imported
**Problem**: The prefab file exists but Unity hasn't imported it correctly.

**Solution**:
1. Select the prefab in Unity Project window
2. Check the Inspector - it should show prefab settings
3. If it shows as a broken prefab, try:
   - Right-click the prefab → **Reimport**
   - Delete the `.meta` file and let Unity recreate it
   - Recreate the prefab from scratch

### Issue 3: Case Sensitivity
**Problem**: The cosmetic name doesn't match the prefab name exactly.

**Solution**:
- Check `LobbyManager.cs` - the cosmetic name should be exactly `"Daimyo"` (capital D, lowercase rest)
- The code tries:
  1. `Characters/Daimyo` (exact match)
  2. `Characters/daimyo` (lowercase)
  3. `Characters/Default` (fallback)

### Issue 4: Prefab Missing Required Components
**Problem**: The prefab exists but is missing required components.

**Solution**:
- Open the prefab in Unity
- Verify it has:
  - ✅ SpriteRenderer component
  - ✅ CharacterJumpAttack component
  - ✅ Animator component
  - ✅ Animator Controller assigned

### Issue 5: Resources Folder Not Built
**Problem**: Resources.Load only works in Play mode or builds, not in Editor scripts at compile time.

**Solution**:
- Test in Play mode (not just in Editor)
- Resources.Load works at runtime, not at edit time
- Make sure you're testing in a build or Play mode

---

## 🧪 Diagnostic Steps

### Step 1: Verify Prefab Exists
1. In Unity, open the Project window
2. Navigate to `Assets/Resources/Characters/`
3. Verify `Daimyo.prefab` is visible
4. Select it and check the Inspector shows prefab settings

### Step 2: Check Cosmetic Name
1. Open `Assets/Scripts/Multiplayer/LobbyManager.cs`
2. Check line 62: `"Daimyo"` should be in the `positionBasedCosmetics` array
3. Verify the spelling and capitalization match exactly

### Step 3: Test Resources.Load Directly
Add this test code to verify Resources.Load works:
```csharp
// Test in a script's Start() method
void Start()
{
    GameObject testPrefab = Resources.Load<GameObject>("Characters/Daimyo");
    if (testPrefab != null)
    {
        Debug.Log("✅ Daimyo prefab found!");
    }
    else
    {
        Debug.LogError("❌ Daimyo prefab NOT found!");
    }
}
```

### Step 4: Check Console Logs
When the game runs, check the console for:
- `[SharedMultiplayerGameManager] Searching for: 'Daimyo'`
- `[SharedMultiplayerGameManager] ✓ Found prefab with exact match: Characters/Daimyo`
- Or error messages indicating why it's not found

### Step 5: Verify Prefab Components
1. Open `Daimyo.prefab` in Unity
2. Check the root GameObject has:
   - SpriteRenderer (with a sprite assigned)
   - CharacterJumpAttack component
   - Animator component (with Animator Controller assigned)
3. If any are missing, add them

---

## 🛠️ Quick Fixes

### Fix 1: Reimport Resources Folder
1. In Unity, right-click `Assets/Resources/` folder
2. Select **Reimport**
3. Wait for Unity to reimport all assets
4. Test again

### Fix 2: Recreate the Prefab
1. Create a new empty GameObject
2. Add required components:
   - SpriteRenderer
   - CharacterJumpAttack
   - Animator
3. Assign the Animator Controller
4. Drag it to `Assets/Resources/Characters/` folder
5. Name it exactly `Daimyo` (Unity will add `.prefab`)

### Fix 3: Check Resources Folder Location
Make sure the folder structure is:
```
Assets/
  Resources/          ← Must be named exactly "Resources" (capital R)
    Characters/       ← Can be any name
      Daimyo.prefab   ← Must match cosmetic name
```

### Fix 4: Verify Prefab is Not Broken
1. Select the prefab in Unity
2. Check if it shows as a broken prefab (red X icon)
3. If broken, try:
   - Right-click → **Reimport**
   - Or recreate the prefab

---

## 📋 Checklist

Before testing, verify:
- [ ] Prefab exists at `Assets/Resources/Characters/Daimyo.prefab`
- [ ] Prefab is visible in Unity Project window
- [ ] Prefab has all required components
- [ ] Cosmetic name in `LobbyManager.cs` is exactly `"Daimyo"`
- [ ] Resources folder is named exactly `"Resources"` (capital R)
- [ ] Unity has refreshed/reimported assets
- [ ] Testing in Play mode (Resources.Load doesn't work in Editor scripts at compile time)
- [ ] Console shows search logs when game runs

---

## 🐛 Common Errors and Solutions

### Error: "Could not load character prefab for cosmetic: 'Daimyo'"
**Cause**: Resources.Load couldn't find the prefab
**Solution**:
1. Verify prefab exists in `Assets/Resources/Characters/`
2. Check spelling and capitalization
3. Reimport Resources folder
4. Test in Play mode (not Editor)

### Error: "Even Default prefab not found!"
**Cause**: Resources folder not recognized or wrong location
**Solution**:
1. Verify `Assets/Resources/Characters/Default.prefab` exists
2. Check Resources folder is named exactly `"Resources"`
3. Reimport Resources folder
4. Restart Unity

### Error: Prefab shows as broken in Unity
**Cause**: Prefab file is corrupted or missing components
**Solution**:
1. Right-click prefab → **Reimport**
2. If still broken, recreate the prefab
3. Check prefab has all required components

---

## 🎯 Testing

1. **Start the game in Play mode**
2. **Join a multiplayer room**
3. **Check the console** for logs like:
   - `[SharedMultiplayerGameManager] Searching for: 'Daimyo'`
   - `[SharedMultiplayerGameManager] ✓ Found prefab with exact match: Characters/Daimyo`
4. **If prefab is found**, you should see:
   - `[SharedMultiplayerGameManager] Creating character instance for {player} with cosmetic 'Daimyo'`
   - `[SharedMultiplayerGameManager] ✓ Character created and stored for actor {number}`

---

## 💡 Pro Tips

1. **Resources.Load only works at runtime** - Test in Play mode or builds
2. **Case sensitivity matters** - `"Daimyo"` ≠ `"daimyo"` (but code tries both)
3. **Resources folder must be named exactly "Resources"** - Unity is case-sensitive
4. **Prefab must have .prefab extension** - Unity adds this automatically when saving
5. **Check console logs** - They show exactly what path is being searched

---

## 🆘 Still Not Working?

If the prefab still isn't detected:
1. Check the Unity console for specific error messages
2. Verify the prefab is not broken in Unity
3. Try recreating the prefab from scratch
4. Check if other prefabs (like Default.prefab) load correctly
5. Verify Resources folder structure is correct
6. Test Resources.Load with a simple test script

If Default.prefab loads but Daimyo.prefab doesn't:
- Check spelling and capitalization
- Verify Daimyo.prefab has the same structure as Default.prefab
- Check if Daimyo.prefab is corrupted

