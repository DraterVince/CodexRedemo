# ?? Quick Reference: Numbered Animations

## ? Supported Formats

| You Name It | We Detect It As |
|-------------|-----------------|
| `idle` | `idle` |
| `idle1` | `idle1` |
| `idle_1` | `idle1` (normalized) |
| `attack2` | `attack2` |
| `attack_2` | `attack2` (normalized) |
| `2_atk` | `2_atk` (special) |

---

## ??? Tools

### Organize Animations
```
Tools ? Organize Animations into Folders
```
- Scans for all numbered variations
- Creates organized folder structure
- Normalizes names automatically

### Setup Controller
```
Tools ? Auto Multi-Animation Setup
```
- Detects all numbered animations
- Configures Multi-Animation Controller
- Inherits settings from base animations

---

## ?? Code Usage

### Basic
```csharp
controller.PlayAnimation("attack1");
controller.PlayAnimation("attack2");
controller.PlayAnimation("attack3");
```

### Check Existence
```csharp
if (controller.HasAnimation("attack2"))
{
    controller.PlayAnimation("attack2");
}
```

### Get All Animations
```csharp
List<string> anims = controller.GetAvailableAnimations();
// ["idle", "idle1", "idle2", "attack1", "attack2", "attack3", ...]
```

---

## ?? Common Patterns

### Attack Combo
```csharp
int combo = 1;

void Attack()
{
    controller.PlayAnimation("attack" + combo);
    combo = (combo % 3) + 1; // Loop: 1, 2, 3, 1, 2, 3...
}
```

### Random Idle
```csharp
void RandomIdle()
{
    int variant = Random.Range(1, 4); // 1, 2, or 3
    controller.PlayAnimation("idle" + variant);
}
```

### Power-Based Jump
```csharp
void Jump(float power)
{
    int level = Mathf.CeilToInt(power / 33f); // 1-3
    controller.PlayAnimation("jump" + level);
}
```

---

## ?? Naming Rules

### ? Good
- `idle.png`, `idle1.png`, `idle2.png`
- `attack1.png`, `attack2.png`
- `walk.png`, `walk1.png`

### ?? OK (normalized)
- `idle_1.png` ? `idle1`
- `attack_2.png` ? `attack2`

### ? Avoid
- `idleOne.png` (use `idle1.png`)
- `attack-1.png` (use `attack1.png`)

---

## ?? Smart Defaults

Numbered animations inherit from base:

| Base | FPS | Loop | Numbered Inherit |
|------|-----|------|------------------|
| `idle` | 8 | ? | Same |
| `attack` | 24 | ? | Same |
| `walk` | 12 | ? | Same |

---

## ?? Result Structure

```
Assets/Animations/
  Knight/
    Idle/
    Idle1/
    Idle2/
    Attack/
    Attack1/
    Attack2/
    Attack3/
    Walk/
    Walk1/
```

---

## ?? Quick Start

1. **Name your sprites**: `idle1.png`, `attack1.png`, etc.
2. **Organize**: `Tools ? Organize Animations`
3. **Setup**: `Tools ? Auto Multi-Animation Setup`
4. **Use**: `controller.PlayAnimation("attack1")`

**Done!** ??
