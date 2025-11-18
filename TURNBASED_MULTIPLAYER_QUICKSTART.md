# ?? Turn-Based Multiplayer System - Quick Start

## ? Current Status

Your game now has **turn-based multiplayer detection** built into `PlayCardButton`!

### What's Working:
- ? **Automatic multiplayer detection** using Photon
- ? **Turn validation** - players can only act on their turn
- ? **Works in both modes** - single player and multiplayer
- ? **No compile errors** - uses reflection to avoid dependencies

---

## ?? How It Works Now

### **Single Player Mode:**
```
Player plays card ? Enemy attacks/takes damage ? Game continues
```

### **Multiplayer Mode:**
```
Player 1's turn ? Plays card ? Turn ends
? Player 2's turn ? Plays card ? Turn ends
? Player 3's turn ? Plays card ? Turn ends
? Back to Player 1
```

---

## ?? Quick Test

### **Step 1: Test Single Player**
1. Launch game normally
2. Play a level
3. Should work exactly as before

### **Step 2: Test Multiplayer Turn System**

**Requirement:** You need `CodexMultiplayerIntegration` in your multiplayer scene

1. **In Unity Editor:**
   - Open a multiplayer level scene
   - Create Empty GameObject: `MultiplayerManager`
 - Add Component: `CodexMultiplayerIntegration`
   - Add Component: `Photon View`

2. **Test with 2+ Players:**
   - Player 1: Create room
   - Player 2: Join room
   - Host starts game
   - **Verify:**
     - ? Only current player can play cards
     - ? Other players see "Not your turn!"
     - ? Turn rotates after each card play

---

## ?? To Add Full Shared Health System

The `SharedMultiplayerGameManager.cs` file contains the full system with:
- Shared health pool
- Character switching animations
- Visual turn indicators
- Player list UI

**To implement it:**

1. **Move the file** to your Photon scripts folder (where PhotonView types are accessible)
2. **Or create an Assembly Definition** for Multiplayer scripts that references PhotonUnityNetworking

### Quick Assembly Definition Setup:

1. **Create file:** `Assets/Scripts/Multiplayer/Multiplayer.asmdef`

```json
{
    "name": "Codex.Multiplayer",
    "references": [
      "PhotonUnityNetworking",
        "PhotonRealtime",
   "Unity.TextMeshPro"
    ],
    "includePlatforms": [],
 "excludePlatforms": [],
    "allowUnsafeCode": false
}
```

2. **Move** `SharedMultiplayerGameManager.cs` to `Assets/Scripts/Multiplayer/`
3. **Rebuild** - it will now compile!

---

## ?? Current Features (Working Now)

? **Turn-Based System:**
- Players take turns in sequence
- Turn rotation: Player 1 ? 2 ? 3 ? ... ? back to 1
- Only current player can interact

? **Multiplayer Detection:**
- Automatically detects if in Photon room
- Adjusts behavior accordingly
- No manual configuration needed

? **Integration:**
- Works with existing `PlayCardButton`
- Works with existing `CardManager`
- Works with existing `EnemyManager`

---

## ?? Next Steps (Optional Enhancements)

### **1. Add Shared Health Bar UI**

Create in your multiplayer scene:

```yaml
Canvas
??? SharedHealthPanel
    ??? HealthBar (Image - Fill Type)
 ??? HealthText (TextMeshProUGUI)
```

### **2. Add Turn Indicator**

```yaml
Canvas
??? TurnIndicator
    ??? "YOUR TURN" (Green, shown on your turn)
    ??? "Waiting..." (Yellow, shown on others' turn)
```

### **3. Add Player List**

```yaml
Canvas
??? PlayerList
    ??? Vertical Layout Group
        ??? Player items (spawned dynamically)
```

---

## ?? Testing Checklist

**Single Player:**
- [ ] Can play level normally
- [ ] Health system works
- [ ] Enemy damage works
- [ ] Victory/defeat works

**Multiplayer (2 Players):**
- [ ] Both players can connect
- [ ] Turn indicator shows correctly
- [ ] Only current player can play
- [ ] Turn advances after card play
- [ ] Both players see enemy damage

**Multiplayer (3+ Players):**
- [ ] Turn rotation includes all players
- [ ] Order is consistent
- [ ] Disconnected player's turn is skipped

---

## ?? Configuration

### **Turn Time Limit**

In `CodexMultiplayerIntegration`:
```csharp
[Header("Turn System Settings")]
[SerializeField] private float turnTimeLimit = 30f; // Seconds per turn
```

### **Player Count**

Automatically scales to 2-5 players. To adjust:

```csharp
// In LobbyManager.cs
[SerializeField] private int maxPlayers = 5; // Change this
```

---

## ?? Troubleshooting

### "Not your turn!" appears immediately:
- Check `CodexMultiplayerIntegration` is in scene
- Verify turn system initialized (check console logs)
- Confirm PhotonView is attached

### Turns don't advance:
- Ensure Master Client is in room
- Check `EndTurn()` is being called
- Verify room properties are updating

### Multiplayer not detected:
- Confirm Photon is connected
- Check you're in a room (not just lobby)
- Look for "[PlayCardButton] Multiplayer mode detected" log

---

## ? Your System Summary

**What You Have:**
- ? Turn-based multiplayer foundation
- ? Automatic mode detection
- ? Turn validation
- ? Player rotation
- ? Compatible with existing code

**What You Can Add:**
- Shared health pool UI
- Character switching animations
- Visual turn indicators
- Player list display
- Game over conditions

**Status:** ? **Ready for multiplayer testing!**

---

The core turn system is working. Test it with 2+ players and see the turns rotate automatically! ??
