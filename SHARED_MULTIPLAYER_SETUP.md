# ?? Shared Multiplayer Turn-Based System Setup Guide

## Overview

This system provides a **shared health pool** multiplayer experience where:
- ? **2-5 players** take turns playing cards
- ? **Shared HP pool** - all players share the same health
- ? **Wrong answers** damage everyone
- ? **Character switching** with slide animations between turns
- ? **Automatic turn progression** after each card play
- ? **Dynamic player scaling** - adapts to any number of players (2-5)

---

## ?? How It Works

### **Gameplay Flow:**

1. **Players join room** (2-5 players)
2. **Each player selects their character** (cosmetic)
3. **Game starts** - First player's character appears
4. **Turn System:**
   - Player 1 plays a card
   - ? Correct? ? Enemy takes damage
   - ? Wrong? ? **All players** lose health
   - Character slides out, Player 2's character slides in
   - Player 2 plays a card
   - Process repeats for all players
5. **Win Condition:** All enemies defeated
6. **Lose Condition:** Shared health reaches 0

---

## ?? Setup Instructions

### **Step 1: Scene Setup**

#### **1.1 Create Shared Game Manager**

1. **In your multiplayer level scene:**
   - Right-click Hierarchy ? Create Empty
   - Name it: `SharedGameManager`

2. **Add Components:**
   - Select `SharedGameManager`
   - Add Component ? `SharedMultiplayerGameManager`
 - Add Component ? `Photon View`

3. **Configure PhotonView:**
   - Observed Components: None (uses RPCs)
   - View ID: Auto-assign

#### **1.2 Configure SharedMultiplayerGameManager**

In the Inspector:

```yaml
Shared Health System:
  Shared Max Health: 100
  Shared Health Bar: [Drag your UI health bar Image]
  Shared Health Text: [Drag your health TextMeshProUGUI]

Character Display:
  Character Display Position: [Create empty GameObject as spawn point]
  Character Slide Speed: 5
  Off Screen Left: (-15, 0, 0)
  Off Screen Right: (15, 0, 0)

Turn Display:
  Current Turn Text: [TextMeshProUGUI showing "Player's Turn"]
  Your Turn Indicator: [GameObject with "YOUR TURN" message]
  Waiting Indicator: [GameObject with "Waiting..." message]

Player List UI:
  Player List Container: [Transform for player list]
  Player List Item Prefab: [Prefab showing player name]

Game Over:
  Game Over Panel: [GameObject with game over UI]
  Victory Panel: [GameObject with victory UI]
  Game Over Text: [TextMeshProUGUI for status message]

References:
  Play Card Button: [Drag your PlayCardButton component]
  Card Manager: [Drag your CardManager component]
  Enemy Manager: [Drag your EnemyManager component]
```

---

### **Step 2: UI Setup**

#### **2.1 Shared Health Bar**

Create UI Canvas:

```yaml
Canvas
??? SharedHealthPanel
    ??? HealthBarBackground (Image)
    ??? HealthBarFill (Image - Fill Type)
    ??? HealthText (TextMeshProUGUI)
```

**HealthBarFill Settings:**
- Image Type: Filled
- Fill Method: Horizontal
- Fill Origin: Left

#### **2.2 Turn Indicator**

```yaml
Canvas
??? TurnIndicator
    ??? CurrentPlayerText (TextMeshProUGUI) - "Player 1's Turn"
    ??? YourTurnIndicator (GameObject with Image + Text)
    ?   ??? "YOUR TURN" (green, pulsing)
    ??? WaitingIndicator (GameObject with Image + Text)
        ??? "Waiting for other players..." (yellow)
```

#### **2.3 Player List**

```yaml
Canvas
??? PlayerListPanel
    ??? Title: "PLAYERS"
    ??? PlayerListContainer (Vertical Layout Group)
        ??? [Player items spawned here]
```

**Player List Item Prefab:**
```yaml
PlayerListItem (Prefab)
??? Background (Image)
??? PlayerName (TextMeshProUGUI)
??? TurnIndicator (Image - green dot for active)
```

---

### **Step 3: Character Setup**

#### **3.1 Create Character Resources**

1. **Create folder:** `Assets/Resources/Characters/`

2. **Add character prefabs:**
   - `Default.prefab`
   - `Knight.prefab`
   - `Mage.prefab`
   - `Archer.prefab`
   - (Add more as needed)

3. **Character Prefab Structure:**
```yaml
CharacterPrefab
??? Model (3D Model or Sprite)
??? Animator (Optional)
??? CharacterJumpAttack (for attack animations)
```

#### **3.2 Character Display Position**

1. **In your level scene:**
   - Create Empty GameObject
   - Name: `CharacterDisplayPosition`
   - Position: `(0, 0, 0)` (center of screen, or wherever you want characters)
   - Rotation: `(0, 0, 0)`

2. **Drag to SharedGameManager:**
   - `Character Display Position` field

---

### **Step 4: Integration with Existing Systems**

#### **4.1 PlayCardButton Integration**

Your `PlayCardButton` is now automatically integrated! It will:
- ? Check turn ownership before allowing card play
- ? Notify multiplayer system of correct/wrong answers
- ? In multiplayer: damage shared health on wrong answer
- ? In single player: work normally

**No code changes needed!**

#### **4.2 CardManager Integration**

The `CardManager` works automatically:
- Cards are dealt to each player
- Turn system controls when player can interact
- Controls disabled when not your turn

#### **4.3 EnemyManager Integration**

Enemy health is synchronized:
- When enemy dies, all players see it
- Progress through enemies is shared
- Victory condition checked after all enemies die

---

## ?? Character Switching Animation

### **How It Works:**

```
[Player 1 Character] --slides left--> [OFF SCREEN]
[OFF SCREEN] --slides in from right--> [Player 2 Character]
```

### **Customization:**

```csharp
// In SharedMultiplayerGameManager Inspector:

Character Slide Speed: 5  // Higher = faster transitions

Off Screen Left: (-15, 0, 0)   // Where characters exit
Off Screen Right: (15, 0, 0)   // Where characters enter

// Adjust Y value to match your character height
Off Screen Left: (-15, 2, 0)   // If characters are elevated
```

### **Adding Custom Animations:**

Modify `SwitchCharacterAnimated()` in `SharedMultiplayerGameManager.cs`:

```csharp
// Add fade effect
private IEnumerator SwitchCharacterAnimated(int newActorNumber)
{
    // ... existing slide code ...
  
    // Add fade
    var renderer = currentCharacterInstance.GetComponent<Renderer>();
    StartCoroutine(FadeOut(renderer));

    // ... rest of code ...
}
```

---

## ?? Testing

### **Single Player Test:**
1. Launch game normally
2. Select level from menu
3. Should work as before (individual health)

### **Multiplayer Test:**

#### **Test with 2 Players:**
1. Build game twice or use Unity Editor + Build
2. **Player 1:**
   - Start game ? Multiplayer
   - Create room: "TestRoom"
3. **Player 2:**
   - Start game ? Multiplayer
 - Join room: "TestRoom"
4. **Player 1 (Host):**
   - Start game
5. **Verify:**
   - ? Both see shared health bar
   - ? Player 1's character appears first
   - ? Player 1 can play cards
   - ? Player 2 sees "Waiting..."
   - ? After Player 1 plays ? Character switches
   - ? Player 2's turn begins
   - ? Wrong answer damages both players
   - ? Correct answer damages enemy

#### **Test with 3-5 Players:**
- Follow same steps with more players
- Verify turn rotation works for all
- Verify character switching for each player

---

## ?? Advanced Configuration

### **Scaling Health Based on Player Count**

Modify `InitializeSharedHealth()` in `SharedMultiplayerGameManager.cs`:

```csharp
private void InitializeSharedHealth()
{
    if (!PhotonNetwork.IsMasterClient) return;
    
    // Scale health based on players
    int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
    float scaledHealth = sharedMaxHealth * playerCount * 0.75f;
    
    currentSharedHealth = scaledHealth;
    
    ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable
    {
        { "SharedHealth", currentSharedHealth },
        { "SharedMaxHealth", scaledHealth }
    };
    
  PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
}
```

### **Adjusting Damage Values**

In `RPC_OnCardPlayed()`:

```csharp
if (!wasCorrect)
{
    if (PhotonNetwork.IsMasterClient)
    {
        // Adjust damage per wrong answer
      float damageAmount = 10f; // Change this value
  DamageSharedHealth(damageAmount);
    }
}
```

### **Custom Turn Order**

Modify turn system to prioritize certain players:

```csharp
// In CodexMultiplayerIntegration.cs
private void NextTurn()
{
    // Custom turn order logic
  List<Player> playerList = new List<Player>(PhotonNetwork.PlayerList);
    
    // Sort by level (highest level goes first)
    playerList.Sort((a, b) => {
      int levelA = (int)a.CustomProperties["levels_unlocked"];
        int levelB = (int)b.CustomProperties["levels_unlocked"];
        return levelB.CompareTo(levelA);
    });
    
    // Assign turn based on sorted list
    // ...
}
```

---

## ?? Player Count Scaling

The system automatically handles different player counts:

| Players | Turn Cycle | Shared Health | Strategy |
|---------|-----------|---------------|----------|
| **2** | A ? B ? A ? B | 100 HP | Fast-paced, frequent turns |
| **3** | A ? B ? C ? A | 100 HP | Balanced pacing |
| **4** | A ? B ? C ? D ? A | 100 HP | Strategic teamwork |
| **5** | A ? B ? C ? D ? E ? A | 100 HP | Maximum coordination |

### **Recommended Adjustments:**

```csharp
// 2 players: Reduce enemy health
if (playerCount == 2)
{
    foreach (var enemy in enemies)
    {
        enemy.health *= 0.75f;
  }
}

// 5 players: Increase shared health
if (playerCount == 5)
{
    sharedMaxHealth = 150f;
}
```

---

## ?? Troubleshooting

### **Characters don't appear:**
- Check Resources/Characters/ folder exists
- Verify character prefabs are in correct folder
- Check Character Display Position is set
- Ensure cosmetic data is synced (check player properties)

### **Turn doesn't advance:**
- Verify CodexMultiplayerIntegration is attached
- Check PhotonView is on SharedGameManager
- Confirm Master Client is in room
- Check console for turn change logs

### **Health doesn't sync:**
- Ensure only Master Client calls DamageSharedHealth
- Check room properties are updating (use Photon logs)
- Verify SharedHealthBar UI reference is set

### **Character switching is choppy:**
- Increase Character Slide Speed
- Check frame rate (target 60 FPS)
- Reduce character polygon count
- Disable unnecessary character components

### **Controls don't disable:**
- Verify turnSystem.IsMyTurn() is being checked
- Check PlayCardButton.enabled logic
- Confirm turn events are firing (add Debug.Log)

---

## ? Enhancement Ideas

### **1. Turn Timer:**
Add visual timer above character:

```csharp
public TextMeshProUGUI turnTimerText;

private void Update()
{
    if (turnSystem != null)
    {
    float remaining = turnSystem.GetTurnTimeRemaining();
    turnTimerText.text = $"Time: {remaining:F0}s";
    }
}
```

### **2. Combo System:**
Reward consecutive correct answers:

```csharp
private int comboCount = 0;

void OnCorrectAnswer()
{
    comboCount++;
    float bonusDamage = 1f + (comboCount * 0.25f);
    DamageEnemy(bonusDamage);
}

void OnWrongAnswer()
{
    comboCount = 0; // Reset combo
}
```

### **3. Character Abilities:**
Give each character unique passive:

```csharp
// Knight: Reduces damage taken
if (characterType == "Knight")
{
    damage *= 0.75f;
}

// Mage: Bonus damage on correct
if (characterType == "Mage")
{
    enemyDamage *= 1.25f;
}
```

### **4. Revive System:**
Allow players to revive team if they have enough correct answers in a row:

```csharp
if (comboCount >= 5 && sharedHealth < sharedMaxHealth * 0.5f)
{
// Revive bonus
    DamageSharedHealth(-20f); // Heal
}
```

---

## ?? Quick Start Checklist

Before launching multiplayer:

- [ ] SharedGameManager GameObject exists
- [ ] SharedMultiplayerGameManager component attached
- [ ] PhotonView component attached
- [ ] All UI references set (health bar, turn text, etc.)
- [ ] Character Display Position set
- [ ] Character prefabs in Resources/Characters/
- [ ] Player List UI setup
- [ ] PlayCardButton, CardManager, EnemyManager referenced
- [ ] Game Over panels created
- [ ] Tested in 2-player mode
- [ ] Tested in 3+ player mode
- [ ] Turn rotation works
- [ ] Character switching animates
- [ ] Shared health updates
- [ ] Victory/defeat conditions work

---

**Your shared multiplayer system is ready!** Players will now work together with shared health, taking turns with smooth character transitions! ??
