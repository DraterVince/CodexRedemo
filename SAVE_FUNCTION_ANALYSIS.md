# ?? SAVE FUNCTION ANALYSIS - NewAndLoadGameManager.cs

## ? Overall Assessment: **MOSTLY WORKING** with Some Improvements Needed

The save system is well-implemented with WebGL-specific handling, but there are a few areas that could be improved.

---

## ?? Current Save Flow Analysis

### **SaveToCurrentSlot() ? SaveToSlot() ? VerifySaveAfterDelay() (WebGL)**

```
User triggers save
    ?
SaveToCurrentSlot() - Collects data from PlayerPrefs
    ?
SaveToSlot() - Writes to PlayerPrefs (3x immediate writes)
    ?
VerifySaveAfterDelay() - Coroutine verification (5 save cycles)
  ?
isSaveInProgress flag cleared
```

---

## ? What's Working Well

### 1. **WebGL-Specific Handling**
```csharp
#if UNITY_WEBGL && !UNITY_EDITOR
    // Aggressive multi-save approach (3x immediate + 5x in coroutine)
    for (int i = 0; i < 3; i++)
    {
     PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }
```
? **Good:** Handles WebGL's IndexedDB persistence issues

### 2. **Save Progress Tracking**
```csharp
public bool IsSaveInProgress => isSaveInProgress;
```
? **Good:** Prevents overlapping saves

### 3. **Auto-Save on Quit/Pause**
```csharp
private void OnApplicationQuit()
private void OnApplicationPause(bool pauseStatus)
```
? **Good:** Catches unexpected exits

### 4. **Dual Storage (PlayerPrefs + Supabase)**
```csharp
SaveToSlot(CurrentSlot, data);        // Local
SaveSlotToSupabase(CurrentSlot, data); // Cloud
```
? **Good:** Redundancy for data safety

---

## ?? Potential Issues Found

### **Issue 1: Coroutine Could Be Interrupted**

**Location:** `SaveToSlot()` Line ~705
```csharp
if (currentSaveCoroutine != null)
{
    StopCoroutine(currentSaveCoroutine);  // ?? Stops previous save!
}
currentSaveCoroutine = StartCoroutine(VerifySaveAfterDelay(slot, key, json));
```

**Problem:**
- If user saves twice quickly, first save's verification is cancelled
- First save might not complete verification
- Could leave `isSaveInProgress` flag stuck

**Fix:**
```csharp
// Don't stop previous save - wait for it instead
if (currentSaveCoroutine != null)
{
  Debug.LogWarning("[NewAndLoadGameManager] Save already in progress, queuing...");
  yield return StartCoroutine(WaitForSaveCompletion());
}
currentSaveCoroutine = StartCoroutine(VerifySaveAfterDelay(slot, key, json));
```

---

### **Issue 2: PlayerDataManager Sync Could Fail**

**Location:** `SaveToCurrentSlot()` Line ~405
```csharp
if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
{
    unlockedCosmetics = PlayerDataManager.Instance.GetCurrentPlayerData().unlocked_cosmetics;
}
else
{
    // Fallback to PlayerPrefs
    unlockedCosmetics = PlayerPrefs.GetString("unlockedCosmetics", "[]");
}
```

**Problem:**
- If PlayerDataManager is null, falls back to potentially stale PlayerPrefs data
- No warning logged that fallback occurred
- Could save incorrect cosmetics

**Fix:**
```csharp
if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetCurrentPlayerData() != null)
{
    unlockedCosmetics = PlayerDataManager.Instance.GetCurrentPlayerData().unlocked_cosmetics;
    Debug.Log($"[NewAndLoadGameManager] Using unlocked cosmetics from PlayerDataManager: {unlockedCosmetics}");
}
else
{
    unlockedCosmetics = PlayerPrefs.GetString("unlockedCosmetics", "[]");
    Debug.LogWarning($"[NewAndLoadGameManager] ?? PlayerDataManager unavailable, using PlayerPrefs fallback: {unlockedCosmetics}");
    Debug.LogWarning("[NewAndLoadGameManager] This may not reflect latest unlocked cosmetics!");
}
```

---

### **Issue 3: No Save Queue System**

**Problem:**
- User could trigger multiple saves in quick succession
- Only last save is guaranteed to complete
- Previous saves might be abandoned mid-verification

**Current Behavior:**
```
Save 1 starts ? User saves again ? Save 1 coroutine stopped ? Save 2 starts
```

**Better Approach:**
```
Save 1 starts ? User saves again ? Save 1 completes ? Save 2 starts
```

---

### **Issue 4: Save Timeout Might Be Too Short**

**Location:** `WaitForSaveCompletion()` Line ~365
```csharp
float timeout = 30f; // Increased timeout for WebGL save verification (was 15f)
```

**Analysis:**
- 30 seconds is good for WebGL
- But `VerifySaveAfterDelay()` itself can take up to ~4 seconds:
  - 5 cycles × 0.3s = 1.5s
  - + 0.5s wait
  - + 3 retries × (3 cycles × 0.2s + 0.5s) = ~2.7s
  - **Total: ~4.7 seconds**

? **This is actually fine** - 30s timeout is sufficient

---

### **Issue 5: Cosmetics Could Be Lost**

**Location:** `SaveToCurrentSlot()` Lines 405-425

**Scenario:**
1. User unlocks cosmetic in PlayerDataManager
2. Save is triggered
3. PlayerDataManager is somehow null at that moment
4. Falls back to PlayerPrefs (which might not have the new cosmetic yet)
5. **New cosmetic is not saved!**

**Risk Level:** ?? Medium
- Happens if save is triggered during scene transition
- PlayerDataManager might be destroyed/not ready

---

## ?? Critical Bug Found: Race Condition

**Location:** `LoadGame()` ? `LoadGameSceneAfterDelay()` Lines 340-365

**The Bug:**
```csharp
private IEnumerator LoadGameSceneAfterDelay()
{
    // Force multiple save cycles
    for (int i = 0; i < 3; i++)
    {
        PlayerPrefs.Save();
    yield return new WaitForSeconds(0.2f);
    }
    
    // ...then load scene
 LoadGameScene();  // ? Loads LevelSelect scene
}
```

**Problem:**
- If user loads a game while a save is in progress
- `isSaveInProgress` flag might still be true
- New scene loads with incomplete save
- Previous slot data might corrupt current slot

**Fix:**
```csharp
private IEnumerator LoadGameSceneAfterDelay()
{
    // CRITICAL: Wait for any in-progress saves!
    if (isSaveInProgress)
    {
        Debug.Log("[NewAndLoadGameManager] Waiting for in-progress save before loading...");
        yield return StartCoroutine(WaitForSaveCompletion());
    }
    
    // Force multiple save cycles
 for (int i = 0; i < 3; i++)
    {
PlayerPrefs.Save();
        yield return new WaitForSeconds(0.2f);
    }
    
    // Verify data is actually saved
    int levelAt = PlayerPrefs.GetInt("levelAt", 5);
    Debug.Log($"[NewAndLoadGameManager] PlayerPrefs verification - levelAt: {levelAt}");
    
    yield return new WaitForSeconds(0.5f);
    
    Debug.Log("[NewAndLoadGameManager] Loading LevelSelect scene now");
    LoadGameScene();
}
```

---

## ?? Recommended Fixes

### **Priority 1: Fix Race Condition (Critical)**

Add wait for save completion before loading:

```csharp
private IEnumerator LoadGameSceneAfterDelay()
{
    // WAIT FOR SAVE TO COMPLETE FIRST!
    if (isSaveInProgress)
    {
  Debug.LogWarning("[NewAndLoadGameManager] Save in progress, waiting before load...");
        yield return StartCoroutine(WaitForSaveCompletion());
    }
    
    // ...existing code...
}
```

---

### **Priority 2: Add Save Queue**

Instead of cancelling previous saves, queue them:

```csharp
private Queue<SaveRequest> saveQueue = new Queue<SaveRequest>();
private bool isProcessingQueue = false;

private class SaveRequest
{
    public int slot;
    public GameData data;
    public string key;
    public string json;
}

private void SaveToSlot(int slot, GameData data)
{
    string json = JsonUtility.ToJson(data);
    string key = GetSlotKey(slot);
    
    SaveRequest request = new SaveRequest
  {
    slot = slot,
        data = data,
        key = key,
    json = json
    };
    
    saveQueue.Enqueue(request);
    
    if (!isProcessingQueue)
    {
 StartCoroutine(ProcessSaveQueue());
  }
}

private IEnumerator ProcessSaveQueue()
{
    isProcessingQueue = true;
    
    while (saveQueue.Count > 0)
    {
        SaveRequest request = saveQueue.Dequeue();
 
        // Process save...
  isSaveInProgress = true;
        
        // Write data
        for (int i = 0; i < 3; i++)
        {
   PlayerPrefs.SetString(request.key, request.json);
       PlayerPrefs.Save();
     }
        
  // Verify
yield return StartCoroutine(VerifySaveAfterDelay(request.slot, request.key, request.json));
 
        isSaveInProgress = false;
    }
    
    isProcessingQueue = false;
}
```

---

### **Priority 3: Improve Logging**

Add more diagnostic logs:

```csharp
private void SaveToCurrentSlot()
{
    Debug.Log($"[NewAndLoadGameManager] ??? SAVE START ???");
    Debug.Log($"[NewAndLoadGameManager] Slot: {CurrentSlot}");
    Debug.Log($"[NewAndLoadGameManager] User: {currentUserId}");
    Debug.Log($"[NewAndLoadGameManager] isSaveInProgress: {isSaveInProgress}");
    
    if (CurrentSlot == 0)
    {
        Debug.LogError("[NewAndLoadGameManager] ? SAVE ABORTED: No active slot!");
        return;
    }
    
    if (isSaveInProgress)
    {
        Debug.LogWarning("[NewAndLoadGameManager] ?? Save already in progress!");
    }
    
    // ...existing save logic...
    
    Debug.Log($"[NewAndLoadGameManager] ??? SAVE END ???");
}
```

---

## ?? Test Scenarios

### **Scenario 1: Rapid Saves**
```
1. Trigger AutoSave()
2. Immediately trigger AutoSave() again
3. Check: Does first save complete?
4. Check: Does second save start?
```

**Expected:** Both saves complete successfully
**Current:** First save might be abandoned

---

### **Scenario 2: Save During Scene Load**
```
1. Trigger AutoSave()
2. Immediately load a game (LoadGame())
3. Check: Does save complete before scene loads?
```

**Expected:** Save completes, then scene loads
**Current:** ?? Potential race condition

---

### **Scenario 3: Cosmetic Unlock + Save**
```
1. Unlock cosmetic in PlayerDataManager
2. Immediately trigger AutoSave()
3. Check: Is cosmetic saved?
```

**Expected:** Cosmetic should be saved
**Current:** ? Works if PlayerDataManager is available

---

## ?? Quick Fix Checklist

### Immediate Fixes (Do Now):

- [ ] **Add wait for save in `LoadGameSceneAfterDelay()`**
```csharp
if (isSaveInProgress)
{
    yield return StartCoroutine(WaitForSaveCompletion());
}
```

- [ ] **Add warning log in cosmetics fallback**
```csharp
Debug.LogWarning("[NewAndLoadGameManager] ?? PlayerDataManager unavailable!");
```

- [ ] **Add save start/end logs**
```csharp
Debug.Log($"[NewAndLoadGameManager] ??? SAVE START ???");
```

### Future Improvements (Optional):

- [ ] Implement save queue system
- [ ] Add save throttling (max 1 save per 0.5s)
- [ ] Add save retry count to UI
- [ ] Add "Saving..." indicator during verification

---

## ?? Summary

### Overall Grade: **B+ (85%)**

**Strengths:**
- ? WebGL persistence handling is robust
- ? Dual storage (PlayerPrefs + Supabase)
- ? Save verification with retries
- ? Auto-save on quit/pause
- ? Progress tracking

**Weaknesses:**
- ?? Race condition during scene load
- ?? No save queue (rapid saves might conflict)
- ?? Previous save coroutine cancellation
- ?? Could lose cosmetics if PlayerDataManager unavailable

**Critical Issue:**
?? **Race condition** - Save might not complete before scene load

**Recommended Action:**
Apply Priority 1 fix immediately (add wait in `LoadGameSceneAfterDelay()`)

---

## ?? Testing Commands

Add these to test save system:

```csharp
// In Unity Console or DeveloperTools:

// Test 1: Trigger rapid saves
for (int i = 0; i < 5; i++)
{
  NewAndLoadGameManager.Instance.AutoSave();
}

// Test 2: Check save status
Debug.Log($"Save in progress: {NewAndLoadGameManager.Instance.IsSaveInProgress}");

// Test 3: Force retry
NewAndLoadGameManager.Instance.RetryCurrentSlotSave();
```

---

**Need me to implement any of these fixes?** Let me know which priority level you want to address! ??
