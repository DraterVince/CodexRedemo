# ?? SAVE SYSTEM - LOCALHOST vs DEPLOYMENT

## ?? **CURRENT: Localhost Mode**

```
? Supabase: DISABLED
? PlayerPrefs: ENABLED
? Cloud Saves: DISABLED
? Fast Testing: YES
?? Save Persistence: UNRELIABLE (expected)
```

### **How It Works Now:**
1. Creates saves in PlayerPrefs (browser storage)
2. No cloud backup
3. Saves might disappear on scene change (WebGL bug)
4. **This is fine for testing game logic!**

---

## ?? **DEPLOYMENT: Production Mode**

When deploying to itch.io or your domain:

### **Step 1: Enable Supabase**

Find these lines in `NewAndLoadGameManager.cs` and **uncomment** them:

```csharp
// Line ~75: In Start() method
Debug.Log("[NewAndLoadGameManager] WebGL: Loading saves from Supabase...");
LoadAllSavesFromSupabase();
StartCoroutine(WaitForSupabaseLoad());

// Line ~350: In LoadAllSavesFromSupabase()
LoadAllSaveSlotsJS();

// Line ~356: In SaveSlotToSupabase()
SaveSlotToSupabaseJS(slot, data.username, data.levelsUnlocked, data.currentMoney, data.unlockedCosmetics);

// Line ~365: In DeleteSlotFromSupabase()
DeleteSlotFromSupabaseJS(slot);
```

Find this line in `SaveLoadUI.cs` and **uncomment** it:

```csharp
// Line ~20: In Start() method
Debug.Log("[SaveLoadUI] WebGL: Waiting for Supabase to load saves...");
StartCoroutine(WaitForSupabaseAndInit());
```

### **Step 2: Comment Out Localhost Code**

**Comment out** the "LOCALHOST MODE" debug logs:

```csharp
// NewAndLoadGameManager.cs Line ~81
// Debug.Log("[NewAndLoadGameManager] WebGL: LOCALHOST MODE - Using PlayerPrefs only...");

// SaveLoadUI.cs Line ~26
// Debug.Log("[SaveLoadUI] WebGL: LOCALHOST MODE - Initializing immediately...");
// InitializeUI();
```

### **Step 3: Test on Deployed URL**

- Build WebGL
- Upload to itch.io or your server
- Test saves work across browser refreshes
- Verify cloud backup is working

---

## ?? **Quick Comparison:**

| Feature | Localhost | Deployment |
|---------|-----------|------------|
| Speed | ? Instant | ?? 2s delay |
| Cloud Saves | ? No | ? Yes |
| Persistence | ?? Unreliable | ? Reliable |
| Cross-Device | ? No | ? Yes |
| Testing | ? Fast | ?? Needs setup |

---

## ? **Quick Toggle:**

### **Enable Localhost Mode** (current):
- Comment Supabase calls
- Uncomment "LOCALHOST MODE" logs
- Fast testing, no cloud

### **Enable Production Mode** (deployment):
- Uncomment Supabase calls
- Comment "LOCALHOST MODE" logs
- Reliable saves, cloud backup

---

**Current Status:** ?? Localhost Mode (fast testing)

**Next Step:** Test game, then switch to Production Mode for deployment! ??
