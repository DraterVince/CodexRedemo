# Numbered Animation Support - Enhancement

## ? What's New

Both the **Animation Folder Organizer** and **Auto Multi-Animation Setup** tools now support **numbered animations**!

---

## ?? Supported Animation Naming

### Base Animation Names
All standard animation types are supported:
- `idle`, `attack`, `walk`, `run`, `jump`, `fall`, `death`
- `hurt`, `hit`, `damage`, `dodge`, `roll`, `block`
- `cast`, `spell`, `shoot`, `reload`, `climb`, `crouch`
- `slide`, `dash`, `skill`, `charge`, `defend`, `heal`, `surf`
- `tumble`, `take_hit`, `air_atk`

### Numbered Variations ? NEW!
Now you can have multiple versions of the same animation:

#### Format 1: Direct Number (Preferred)
- `idle1`, `idle2`, `idle3`
- `attack1`, `attack2`, `attack3`
- `walk1`, `walk2`
- `jump1`, `jump2`

#### Format 2: Underscore + Number
- `idle_1`, `idle_2`, `idle_3`
- `attack_1`, `attack_2`, `attack_3`
- `walk_1`, `walk_2`

Both formats are automatically detected and normalized!

### Special Patterns
- `2_atk`, `3_atk`, `sp_atk`
- `j_up`, `j_down`
- `air_atk`, `take_hit`

---

## ?? Example Project Structures

### Example 1: Multiple Attack Animations

**Before:**
```
Assets/AssestsForGame/Player Sprites/Knight/
  idle.png
  attack.png          ? Only 1 attack
  walk.png
```

**Now Supported:**
```
Assets/AssestsForGame/Player Sprites/Knight/
  idle.png
  attack1.png    ? First attack
  attack2.png         ? Second attack  
  attack3.png         ? Third attack
  walk.png
```

**Organized Result:**
```
Assets/Animations/Knight/
  Idle/
    ??? (idle sprites)
  Attack1/
    ??? (attack1 sprites)
  Attack2/
    ??? (attack2 sprites)
  Attack3/
    ??? (attack3 sprites)
  Walk/
    ??? (walk sprites)
```

### Example 2: Multiple Idle Variations

```
Assets/AssestsForGame/Player Sprites/MartialHero/
  idle_1.png       ? Relaxed idle
  idle_2.png     ? Battle-ready idle
  idle_3.png    ? Tired idle
  attack.png
```

**Organized Result:**
```
Assets/Animations/MartialHero/
  Idle1/
  Idle2/
  Idle3/
  Attack/
```

### Example 3: Mixed Naming

```
Assets/AssestsForGame/Player Sprites/WaterPriestess/
  Sprites/
    ??? Idle/              ? Base idle
    ??? Attack/   ? Base attack
    ??? 2_atk/            ? Second attack (special pattern)
    ??? 3_atk/        ? Third attack (special pattern)
    ??? sp_atk/           ? Special attack
    ??? j_up/             ? Jump up
    ??? j_down/    ? Jump down
    ??? air_atk/          ? Air attack
```

---

## ?? Detection Logic

The enhanced detection system:

1. **Exact Matches**: `idle`, `attack`, `walk` ? Base animations
2. **Direct Numbers**: `idle1`, `attack2` ? Numbered variations
3. **Underscore Numbers**: `idle_1`, `attack_2` ? Normalized to `idle1`, `attack2`
4. **Contains Pattern**: Files/folders containing animation names with numbers
5. **Special Patterns**: `2_atk`, `sp_atk`, `j_up` ? Special cases

### Normalization

The system automatically normalizes names:
- `idle_1` ? `idle1`
- `attack_2` ? `attack2`
- `walk_3` ? `walk3`

This ensures consistent naming in your organized folders!

---

## ?? Using in Code

### MultiAnimationController

```csharp
MultiAnimationController controller = GetComponent<MultiAnimationController>();

// Play base animations
controller.PlayAnimation("idle");
controller.PlayAnimation("attack");

// Play numbered variations ? NEW!
controller.PlayAnimation("idle1");
controller.PlayAnimation("idle2");
controller.PlayAnimation("attack1");
controller.PlayAnimation("attack2");
controller.PlayAnimation("attack3");

// Check availability
if (controller.HasAnimation("attack2"))
{
    Debug.Log("Second attack available!");
    controller.PlayAnimation("attack2");
}

// Get all animations
List<string> allAnims = controller.GetAvailableAnimations();
// Returns: ["idle", "idle1", "idle2", "attack1", "attack2", "attack3", "walk", ...]
```

### Example: Combo System

```csharp
public class ComboSystem : MonoBehaviour
{
    private MultiAnimationController animator;
    private int comboCount = 0;
    
    void Start()
    {
   animator = GetComponent<MultiAnimationController>();
    }
    
    public void Attack()
    {
        comboCount++;
        
 // Cycle through attack animations
        string attackAnim = "attack" + comboCount;
        
        if (animator.HasAnimation(attackAnim))
        {
       animator.PlayAnimation(attackAnim);
        }
        else
        {
            // Reset to first attack
 comboCount = 1;
 animator.PlayAnimation("attack1");
        }
        
        // Reset combo after delay
        StartCoroutine(ResetCombo());
    }
    
  IEnumerator ResetCombo()
    {
        yield return new WaitForSeconds(1.5f);
        comboCount = 0;
}
}
```

### Example: Idle Variation System

```csharp
public class IdleVariation : MonoBehaviour
{
    private MultiAnimationController animator;
    
    void Start()
    {
        animator = GetComponent<MultiAnimationController>();
    InvokeRepeating("SwitchIdle", 5f, 5f); // Every 5 seconds
    }
    
 void SwitchIdle()
 {
        // Count available idle animations
        List<string> allAnims = animator.GetAvailableAnimations();
        List<string> idles = allAnims.FindAll(a => a.StartsWith("idle"));
        
        if (idles.Count > 1)
        {
            // Pick a random idle variation
            string randomIdle = idles[Random.Range(0, idles.Count)];
 animator.PlayAnimation(randomIdle);
            Debug.Log($"Switched to: {randomIdle}");
     }
    }
}
```

---

## ??? Tool Usage

### Animation Folder Organizer

1. Open: `Tools ? Organize Animations into Folders`
2. Click **"?? Scan for Characters & Animations"**
3. The tool will now show:
 ```
   ?? Knight (5 animations)
      Animations: IDLE, ATTACK1, ATTACK2, ATTACK3, WALK
      Frames: idle(4), attack1(6), attack2(8), attack3(10), walk(8)
   ```
4. Click **"?? Copy X Character(s) to Organized Folders"**

### Auto Multi-Animation Setup

1. Open: `Tools ? Auto Multi-Animation Setup`
2. Browse to your character folder
3. Click **"?? Search for ALL Animations"**
4. You'll see all detected animations:
   ```
   ? IDLE - 4 frames @ 8 fps ?? Loop
   ? ATTACK1 - 6 frames @ 24 fps
   ? ATTACK2 - 8 frames @ 24 fps
   ? ATTACK3 - 10 frames @ 24 fps
   ? WALK - 8 frames @ 12 fps ?? Loop
   ```
5. Click **"? Setup Multi-Animation Controller!"**

---

## ?? Smart Settings

### FPS Assignment
Numbered variations inherit settings from their base animation:

- `idle`, `idle1`, `idle2` ? All get **8 FPS**
- `attack`, `attack1`, `attack2`, `attack3` ? All get **24 FPS**
- `walk`, `walk1`, `walk2` ? All get **12 FPS**

### Loop Settings
Numbered variations also inherit loop settings:

- `idle`, `idle1`, `idle2` ? All **LOOP**
- `attack`, `attack1`, `attack2` ? All **NO LOOP**
- `walk`, `walk1`, `walk2` ? All **LOOP**

You can override these settings in the Inspector or code!

---

## ?? Benefits

### 1. **Richer Animations**
Create multiple variations of the same action:
- Different attack styles
- Various idle poses
- Multiple walk cycles

### 2. **Combo Systems**
Easily implement attack combos:
```csharp
attack1 ? attack2 ? attack3 ? special_attack
```

### 3. **Variety & Polish**
Add visual variety without complex code:
- Random idle animations
- Contextual attacks
- Different movement styles

### 4. **Organized Structure**
Everything stays organized:
```
Animations/
  Knight/
  Idle1/
    Idle2/
    Attack1/
    Attack2/
    Attack3/
```

---

## ?? Technical Details

### Regular Expression Patterns

The system uses these regex patterns for detection:

```regex
^{baseAnim}_?\d+$        # Matches: idle1, idle_1, attack2, attack_2
{baseAnim}_?(\d+)        # Extracts number: idle1 ? 1, attack_2 ? 2
```

### Normalization Process

```csharp
Input: "idle_1"  ? Output: "idle1"
Input: "attack_2" ? Output: "attack2"
Input: "walk_3" ? Output: "walk3"
```

### Priority Order

1. Exact match (`idle`)
2. Direct number (`idle1`)
3. Underscore number (`idle_1`)
4. Contains pattern (`my_idle_1_anim`)
5. Special patterns (`2_atk`, `sp_atk`)

---

## ?? Naming Recommendations

### ? Best Practices

```
? idle.png, idle1.png, idle2.png
? attack1.png, attack2.png, attack3.png
? walk.png, walk1.png
```

### ?? Acceptable

```
?? idle_1.png, idle_2.png (will be normalized)
?? Idle1.png, IDLE2.png (case doesn't matter)
```

### ? Avoid

```
? idleOne.png (use idle1.png)
? attack_first.png (use attack1.png)
? idle-1.png (use idle1.png or idle_1.png)
```

---

## ?? Summary

### What Changed

| Feature | Before | Now |
|---------|--------|-----|
| **Animation Names** | `idle`, `attack` | `idle`, `idle1`, `idle2`, `attack1`, `attack2` |
| **Detection** | Exact match only | Exact + numbered variations |
| **Flexibility** | 1 animation per type | Unlimited variations per type |
| **Naming** | Strict | Flexible (numbers, underscores) |

### Files Updated

1. ? `Assets/Editor/AnimationFolderOrganizer.cs`
   - Added `DetectAnimationName()` method
   - Added regex pattern matching
   - Added normalization logic

2. ? `Assets/Editor/AutoMultiAnimationSetup.cs`
   - Added `DetectAnimationName()` method
   - Added `GetBaseAnimationName()` method
   - Updated search logic

---

## ?? Get Started

1. **Organize your animations:**
   ```
   Tools ? Organize Animations into Folders
   ```

2. **Set up your character:**
   ```
   Tools ? Auto Multi-Animation Setup
   ```

3. **Use in code:**
   ```csharp
   controller.PlayAnimation("attack1");
   controller.PlayAnimation("attack2");
   controller.PlayAnimation("attack3");
 ```

**Now you have full support for numbered animation variations!** ??

---

## ?? Example Use Cases

### Fighting Game
```csharp
// Light, medium, heavy attacks
controller.PlayAnimation("attack1"); // Light punch
controller.PlayAnimation("attack2"); // Medium punch
controller.PlayAnimation("attack3"); // Heavy punch
```

### RPG Character
```csharp
// Different idle states
controller.PlayAnimation("idle");   // Standing
controller.PlayAnimation("idle1");  // Breathing
controller.PlayAnimation("idle2");  // Looking around
controller.PlayAnimation("idle3");  // Stretching
```

### Platformer
```csharp
// Multiple jump animations
controller.PlayAnimation("jump1");  // Small hop
controller.PlayAnimation("jump2");  // Normal jump
controller.PlayAnimation("jump3");  // Double jump
```

---

**Happy Animating!** ???
