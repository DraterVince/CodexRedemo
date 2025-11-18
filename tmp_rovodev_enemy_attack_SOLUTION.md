# 🔴 CRITICAL ISSUE FOUND: Enemy Attack Animation Not Playing

## Root Cause #1: Idle State Name Mismatch ⚠️

### The Problem:
The **LightBanditController** (and likely other enemy controllers) have different idle state names than what `EnemyJumpAttack` expects:

- **LightBanditController** has state named: `"LightBanditIdle"`
- **EnemyJumpAttack.cs** expects: `"Idle"` (line 18, default value)

### What Happens:
1. When enemy spawns, `Start()` calls `PlayIdleAnimation()` (line 107)
2. `PlayIdleAnimation()` tries to play state named `"Idle"` (line 252)
3. State doesn't exist (actual name is `"LightBanditIdle"`)
4. In Unity Editor: `HasAnimatorState()` returns `false` (lines 284-300)
5. Script sets `useSpriteAnimation = false` (line 270) **← ANIMATIONS DISABLED!**
6. Later when attack happens, animations are skipped because `useSpriteAnimation` is now `false`

### Evidence from Code:
```csharp
// Line 268-270 in EnemyJumpAttack.cs
Debug.LogWarning($"[EnemyJumpAttack] {gameObject.name}: Idle state '{idleAnimationName}' not found in Animator Controller. Disabling sprite animations...");
hasShownIdleWarning = true;
useSpriteAnimation = false;  // ← THIS DISABLES ALL ANIMATIONS!
```

---

## Root Cause #2: Attack State Name Also Wrong! ⚠️

### The Problem:
- **LightBanditController** attack state is named: `"LightBanditAttack"`
- **EnemyJumpAttack.cs** expects: `"Attack"` (line 21, default value)

When checking at line 500:
```csharp
bool isInAttackState = stateInfo.IsName("Attack");
```
This will ALWAYS return `false` because the actual state name is `"LightBanditAttack"`!

---

## 💡 THE SOLUTION

There are **TWO ways** to fix this:

### Option A: Fix in Unity Inspector (Per-Prefab) ⭐ RECOMMENDED
For each enemy prefab, set the correct state names in the Inspector:

**For LightBandit:**
1. Select the LightBandit prefab
2. Find `EnemyJumpAttack` component
3. Set `Idle Animation Name` = `"LightBanditIdle"`
4. Set `Attack Animation State` = `"LightBanditAttack"`

**For Other Enemies:**
- DemonSlime: `"Idle"` / `"DemonSlime_Attack"`
- HeavyBandit: Needs verification
- etc.

### Option B: Fix All Animator Controllers (Global) 
Rename all idle states to `"Idle"` and attack states to `"Attack"` in every Animator Controller.

**Pros:** Works with default script settings
**Cons:** Time-consuming, affects all controllers

---

## 🔍 How to Verify the Issue

### Step 1: Check Console Logs
Run the game and look for this warning when enemy spawns:
```
[EnemyJumpAttack] LightBandit: Idle state 'Idle' not found in Animator Controller. Disabling sprite animations.
```

If you see this, **animations are disabled** and won't play!

### Step 2: Verify State Names
In Unity:
1. Open `LightBanditController` in Animator window
2. Check state names:
   - Default state: `LightBanditIdle` (NOT "Idle")
   - Attack state: `LightBanditAttack` (NOT "Attack")

---

## 🛠️ Quick Fix Script

I can create an auto-fix script that:
1. Scans all enemy prefabs
2. Detects the actual state names in their Animator Controllers
3. Automatically sets the correct `idleAnimationName` and `attackAnimationState` values

Would you like me to create this?

---

## Why This Happened

Looking at the controllers:
- **DemonSlime.controller**: States are `"Idle"` and `"DemonSlime_Attack"` 
- **LightBanditController**: States are `"LightBanditIdle"` and `"LightBanditAttack"`

There's **no consistency** in naming conventions across different enemy controllers!

The `EnemyJumpAttack.cs` script defaults to `"Idle"` and `"Attack"`, which works for some enemies but NOT for LightBandit (and possibly others).

---

## Testing After Fix

After applying the fix:

1. **Start the game**
2. **Check Console** - Should see:
   ```
   [EnemyJumpAttack] LightBandit: PerformJumpAttack called
   [EnemyJumpAttack] LightBandit: Attack trigger 'Attack' set successfully
   ```
3. **Watch enemy** - Should see attack animation play
4. **No warnings** about missing states or disabled animations

---

## Next Steps

**Choose one:**

1. **Manual Fix (Fast)** - I'll tell you exactly which prefabs to fix
2. **Auto-Fix Script** - I'll create a Unity Editor script to fix all enemies
3. **Check Other Enemies** - Scan all enemy controllers for the same issue

Which would you prefer?
