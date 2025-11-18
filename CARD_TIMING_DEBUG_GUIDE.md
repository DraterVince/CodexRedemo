# ?? Debugging Guide: Card Progression Issues

## Quick Diagnosis

### **Issue: Correct answers showing wrong marker**

**Check Console Logs:**
```
[SharedMultiplayerGameManager] Syncing card state - Card: X, PlayButton: Y, Output: Z, Answer: ?
```

**What to look for:**
- `Answer` value should match the question that was JUST answered
- If Answer = 1 when you answered question 0, the fix didn't work

**Solution:**
- Verify `currentAnswerIndex = counter;` happens BEFORE `counter++` in PlayCardButton.cs
- Check line where correct answer is detected

---

### **Issue: Turn not advancing after wrong answer**

**Check Console Logs:**
```
[SharedMultiplayerGameManager] Wrong answer - advancing turn after delay
[SharedMultiplayerGameManager] Advancing turn after delay
```

**If you DON'T see these logs:**
- The `RPC_OnCardPlayed` wasn't called or MasterClient check failed
- Check if `PhotonNetwork.IsMasterClient` is true for one player

**If you see the logs but turn doesn't change:**
- Check if `turnSystem` is null
- Check if `turnSystem.EndTurn()` is working

---

### **Issue: Cards stuck in PlayedCard holder**

**Check Console Logs:**
```
[CardManager] Found X cards in PlayedCard holder, moving back to grid
```

**If you DON'T see this log:**
- PlayedCard GameObject name might be different
- Use Unity Hierarchy to find exact name (case-sensitive!)
- Update line: `GameObject.Find("PlayedCard")` with correct name

**If you see the log but cards still stuck:**
- Check if cards have DraggableItem component
- Verify grid Transform is assigned in CardManager

---

### **Issue: Second player can't see cards**

**Check Console Logs:**
```
[SharedMultiplayerGameManager] Card grid activated
[SharedMultiplayerGameManager] Card grid deactivated
```

**What should happen:**
- Grid activated when it's YOUR turn
- Grid deactivated when it's OTHER player's turn

**If grid stays hidden:**
- Check if `cardManager.grid` is assigned in Inspector
- Verify `UpdateCardVisibility()` is being called in Update()

---

## Step-by-Step Debug Process

### **1. Test Correct Answer Flow (Solo)**
```
Start Solo in Multiplayer Room:
1. Play Question 1 (correct)
2. Check console: "Syncing card state - Answer: 0"
3. Check UI: Answer marker for Question 1 visible
4. Cards should reset and show Question 2
```

### **2. Test Wrong Answer Flow (Solo)**
```
1. Play Question 1 (wrong)
2. Check console: "Wrong answer - advancing turn after delay"
3. Wait 1 second
4. Check console: "Advancing turn after delay"
5. Cards should reset and show SAME Question 1
```

### **3. Test with 2 Players**
```
Player 1:
1. Play Question 1 (correct)
2. See answer marker for Q1
3. See turn change to Player 2

Player 2:
1. See answer marker for Q1 (synced!)
2. See Question 2 cards
3. Play Question 2 (correct)
4. See answer marker for Q2
5. See turn change to Player 1

Player 1:
1. See answer marker for Q2 (synced!)
2. See Question 3 cards
```

---

## Common Issues & Solutions

### **"Answer marker shows 1 ahead"**
**Cause:** Counter was incremented before storing answerIndex  
**Fix:** Move `int currentAnswerIndex = counter;` BEFORE the `counter++` lines

### **"Turn doesn't advance after wrong answer"**
**Cause:** AdvanceTurnAfterDelay not being called  
**Fix:** Verify `RPC_OnCardPlayed` has the StartCoroutine line for wrong answers

### **"Cards stuck in PlayedCard holder"**
**Cause:** GameObject.Find("PlayedCard") returns null  
**Fix:** Check exact GameObject name in Unity Hierarchy

### **"Counter keeps advancing even on wrong answer"**
**Cause:** Counter increment happens even in wrong answer branch  
**Fix:** Verify `cardManager.counter++` and `counter++` are ONLY in the correct answer branch

### **"Second player sees same cards as first player"**
**Expected!** Both players see the same cards (shared card pool)  
**Issue:** If you don't want this, you need individual card decks per player

---

## Unity Inspector Checklist

### **SharedMultiplayerGameManager:**
```
? PhotonView component attached
? Play Card Button assigned
? Card Manager assigned
? Enemy Manager assigned
? Turn system component (CodexMultiplayerIntegration) on same GameObject
```

### **CardManager:**
```
? grid GameObject assigned
? cardContainer list populated
? cardDisplayContainer list populated
```

### **PlayCardButton:**
```
? cardManager assigned
? enemyManager assigned
? outputManager assigned
? correctAnswersContainer list populated
```

---

## Expected Behavior Summary

| Event | Counter Change | Turn Change | Cards Reset | Cards Randomize |
|-------|---------------|-------------|-------------|-----------------|
| **Correct Answer (Enemy Alive)** | +1 | Yes ? Next Player | Yes | Yes (new counter) |
| **Wrong Answer** | No change | Yes ? Next Player | Yes | Yes (same counter) |
| **Enemy Defeated** | Reset to 0 | Yes ? Next Player | Yes | Yes (counter=0) |

---

## Test Script

Run this sequence to verify all fixes:

```
1. START GAME (2 Players)
   Counter: 0
   Turn: Player 1

2. Player 1: CORRECT
   Expected: Answer 0 marker shows ?
   Counter: 0 ? 1
   Turn: Player 1 ? Player 2
   Cards: Reset & Randomize (counter=1)

3. Player 2: WRONG
   Expected: No marker shows ?
   Counter: Stays 1
   Turn: Player 2 ? Player 1
   Cards: Reset & Randomize (counter=1, SAME)

4. Player 1: CORRECT (Retry)
   Expected: Answer 1 marker shows ?
   Counter: 1 ? 2
   Turn: Player 1 ? Player 2
   Cards: Reset & Randomize (counter=2)

5. Player 2: CORRECT
   Expected: Answer 2 marker shows ?
   Counter: 2 ? 3
   Enemy: Defeated (if 3 HP enemy)
   Counter: Reset to 0
   Turn: Player 2 ? Player 1
   Cards: Reset & Randomize (counter=0, NEW ENEMY)

? If all these work correctly, all issues are fixed!
```

---

## Quick Fixes

### **If answer markers are still wrong:**
```csharp
// In PlayCardButton.cs, around line 158
if (playedCard.name == correctAnswersContainer[outputManager.counter].correctAnswers[counter])
{
    // Show marker
    outputManager.answerListContainer[outputManager.counter].answers[i].SetActive(true);

    // ?? THIS MUST BE HERE:
    int currentAnswerIndex = counter;

    // ?? BEFORE THESE:
  cardManager.counter++;
    counter++;

    // ?? PASS currentAnswerIndex HERE:
    NotifyMultiplayerCardPlayed(true, currentAnswerIndex);
}
```

### **If turn doesn't advance on wrong answer:**
```csharp
// In SharedMultiplayerGameManager.cs RPC_OnCardPlayed
if (!wasCorrect)
{
    if (PhotonNetwork.IsMasterClient)
    {
        DamageSharedHealth(1f);
    
        // ?? THIS LINE MUST BE HERE:
   StartCoroutine(AdvanceTurnAfterDelay(1.0f));
    }
}
```

### **If cards stick in PlayedCard holder:**
```csharp
// In CardManager.cs ResetCards(), after the for loop:

// ?? ADD THIS CODE:
GameObject playedCardHolder = GameObject.Find("PlayedCard");
if (playedCardHolder != null && playedCardHolder.transform.childCount > 0)
{
    Debug.Log($"[CardManager] Found {playedCardHolder.transform.childCount} cards in PlayedCard holder, moving back to grid");
    
    while (playedCardHolder.transform.childCount > 0)
  {
        Transform child = playedCardHolder.transform.GetChild(0);
        child.SetParent(grid.transform);
        child.gameObject.SetActive(false);
        child.localScale = Vector3.one;
    }
}
```

---

**If all else fails:**
1. Rebuild the project
2. Check all Inspector assignments
3. Verify PhotonView is on SharedMultiplayerGameManager
4. Check that CodexMultiplayerIntegration is on same GameObject as SharedMultiplayerGameManager
5. Look for any exceptions in Console (red errors)

**Still broken?** Check if changes were saved and built correctly. Sometimes Unity caches old code!
