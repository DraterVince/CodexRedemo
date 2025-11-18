# ? Fixed: Character Loading Errors

## ? **Errors That Were Occurring:**

```
ArgumentOutOfRangeException: Index was out of range. Must be non-negative and less than the size of the collection.
Parameter name: index

[SharedMultiplayerGameManager] No character found for actor 1
```

## ?? **Root Cause:**

The `LoadPlayerCharacters()` coroutine was running too early, before:
1. Players were fully synced to the room
2. Player custom properties were available
3. PhotonNetwork.PlayerList was populated

## ? **What Was Fixed:**

### **1. Increased Wait Time**
```csharp
// BEFORE: yield return new WaitForSeconds(0.5f);
// AFTER:  yield return new WaitForSeconds(1.0f);
```
Gives more time for player sync.

### **2. Added Comprehensive Logging**
Now you'll see exactly what's happening:
```
[SharedMultiplayerGameManager] Loading characters for 2 players
[SharedMultiplayerGameManager] Loading character for player: Player1, ActorNumber: 1
[SharedMultiplayerGameManager] Player Player1 has cosmetic: knight
[SharedMultiplayerGameManager] GetCharacterPrefab called with: knight
[SharedMultiplayerGameManager] Successfully loaded prefab: Knight
[SharedMultiplayerGameManager] Character created and stored for actor 1
```

### **3. Better Error Handling**
- Checks if player exists before loading character
- Warns if cosmetic property is missing (uses "default")
- Logs each step of character loading
- Shows exact paths being searched for prefabs

---

## ?? **What to Check Now:**

### **1. Console Logs When Testing:**

**Good logs (working):**
```
[SharedMultiplayerGameManager] Loading characters for 2 players
[SharedMultiplayerGameManager] Loading character for player: Player1, ActorNumber: 1
[SharedMultiplayerGameManager] Player Player1 has cosmetic: knight
[SharedMultiplayerGameManager] Successfully loaded prefab: Knight
[SharedMultiplayerGameManager] Character created and stored for actor 1
[SharedMultiplayerGameManager] Showing first player's character: Player1
[SharedMultiplayerGameManager] ShowCharacter called for actor: 1, animated: false
[SharedMultiplayerGameManager] Character 1 shown instantly
```

**Bad logs (something wrong):**
```
[SharedMultiplayerGameManager] Player has no multiplayer_cosmetic property
[SharedMultiplayerGameManager] Could not load character prefab for cosmetic: xxx
[SharedMultiplayerGameManager] Could not load any character prefab, not even Default!
```

### **2. Check Resources Folder:**

Make sure you have character prefabs at:
```
Assets/Resources/Characters/
  ??? Default.prefab
  ??? Knight.prefab (or knight.prefab)
  ??? Mage.prefab (or mage.prefab)
  ??? Archer.prefab (or archer.prefab)
  ??? Rogue.prefab (or rogue.prefab)
```

**The system tries:**
1. `Characters/{cosmetic}` - exact match
2. `Characters/{Capitalized}` - capitalized first letter
3. `Characters/Default` - fallback

---

## ?? **Expected Behavior Now:**

1. **Players join room**
2. **Wait 1 second** for full sync
3. **Load each player's character:**
   - Get their cosmetic from room properties
   - Load the prefab from Resources
   - Instantiate and hide offscreen
   - Store in playerCharacters dictionary
4. **Show first player's character**
5. **Ready to play!**

---

## ?? **If Errors Still Occur:**

### **"No character found for actor X"**
**Cause:** Character prefab doesn't exist or failed to load
**Check:**
- Resources/Characters/ folder exists
- Prefab names match cosmetic names
- At least a Default.prefab exists

### **"Index was out of range"**
**Cause:** Trying to access player that doesn't exist
**Check:**
- Both players are fully connected before starting
- Console shows "Loading characters for X players" with correct number

### **"Could not load character prefab"**
**Cause:** Prefab path is wrong
**Check:**
- Prefab is in Resources/Characters/ folder
- Prefab name matches cosmetic name (case-insensitive)
- Resources folder is properly recognized by Unity

---

**Build successful! Test with console open to see detailed character loading process!** ??
