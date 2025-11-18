# Quick Start: Multi-Animation System

## ?? Get Started in 5 Minutes!

### ?? Important: Fix Compilation First

If you see red errors about `SpriteAnimator` not found:

**Quick Fix:**
1. In Unity: **Tools ? Fix Compilation Issues**
2. Click **"Force Reimport All Animation Scripts"**
3. Wait for Unity to recompile (bottom-right corner)

OR manually:
1. Right-click `Assets\Scripts\SpriteAnimator.cs` in Project window
2. Select **"Reimport"**
3. Wait for compilation

? **Errors gone?** Continue below!

---

## ?? Step 1: Organize Your Sprites

Choose **ONE** of these structures:

### Option A: Sprite Sheets (Easiest)
```
Assets/Characters/YourCharacter/
  idle.png      ? Name your sprite sheets exactly!
  attack.png
  walk.png
  run.png
  jump.png
```

### Option B: Folders
```
Assets/Characters/YourCharacter/
  Idle/   ? Name your folders exactly!
    frame_0.png
frame_1.png
  Attack/
    frame_0.png
    frame_1.png
  Walk/
frame_0.png
```

**Important:** 
- ? Names must be **exact**: `idle`, `attack`, `walk`, `run`, `jump`, etc.
- ? Case doesn't matter: `Idle`, `idle`, `IDLE` all work
- ? Don't use prefixes: `knight_idle.png` won't work

---

## ?? Step 2: Auto-Detect Animations

1. In Unity menu: **Tools ? Auto Multi-Animation Setup**
2. Click **"Browse"** button
3. Select your character's folder (e.g., `Assets/Characters/Knight/`)
4. Click **"?? Search for ALL Animations"**
5. Review the list - you should see all detected animations!

**Example result:**
```
? IDLE    4 frames  @ 8 fps  ?? Loop
? ATTACK      6 frames  @ 24 fps
? WALK        8 frames  @ 12 fps  ?? Loop
? RUN       8 frames  @ 16 fps  ?? Loop
? JUMP        6 frames  @ 12 fps
```

---

## ? Step 3: Setup Character

1. (Optional) Drag your character GameObject to **"Target Character"** field
   - Leave empty to create a new GameObject
2. Click **"? Setup Multi-Animation Controller!"**
3. Done! ?

---

## ?? Step 4: Test Animations

Enter **Play Mode** and press:

| Key | Animation |
|-----|-----------|
| **1** | Idle |
| **Space** | Attack |
| **Left Click** | Attack |
| **W** | Walk |
| **Shift** | Run |
| **Ctrl** | Dodge |
| **2, 3, 4** | Special animations |

---

## ?? Step 5: Use in Code

### Play Animation
```csharp
// Get the controller
MultiAnimationController anim = GetComponent<MultiAnimationController>();

// Play any animation
anim.PlayAnimation("attack");
anim.PlayAnimation("walk");
anim.PlayAnimation("jump");
```

### Check if Animation Exists
```csharp
if (anim.HasAnimation("death"))
{
    anim.PlayAnimation("death");
}
```

### List All Animations
```csharp
List<string> animations = anim.GetAvailableAnimations();
foreach (string name in animations)
{
    Debug.Log($"Available: {name}");
}
```

---

## ?? What Animations Are Supported?

The system automatically detects these 25+ animation types:

**Combat:** idle, attack, block, dodge, hurt, death
**Movement:** walk, run, jump, fall, climb, crouch, slide, dash
**Special:** cast, spell, shoot, reload, charge, skill

**...and more!** Full list in `MULTI_ANIMATION_SYSTEM_GUIDE.md`

---

## ? Common Issues

### "No animations found!"
- ? Check folder names are **exact**: `idle`, `attack`, etc.
- ? Make sure sprite sheets are **sliced** (Sprite Editor)
- ? Try searching in parent folder

### "SpriteAnimator not found" error
- ? Use: **Tools ? Fix Compilation Issues**
- ? Or reimport `SpriteAnimator.cs` manually
- ? See `SPRITEANIMATOR_TYPE_RESOLUTION_FIX.md`

### Animation not playing
- ? Check animation exists: `HasAnimation("name")`
- ? Verify sprites assigned
- ? Check SpriteRenderer attached

---

## ?? Full Documentation

- **Complete Guide:** `MULTI_ANIMATION_SYSTEM_GUIDE.md`
- **Implementation Details:** `MULTI_ANIMATION_IMPLEMENTATION_SUMMARY.md`
- **Troubleshooting:** `SPRITEANIMATOR_TYPE_RESOLUTION_FIX.md`

---

## ?? Old vs New System

### Old System
```csharp
CharacterAnimationController old;
old.PlayIdle();    // Only idle
old.PlayAttack();  // Only attack
// That's all! ?
```

### New System  
```csharp
MultiAnimationController new;
new.PlayAnimation("idle");    // ?
new.PlayAnimation("attack");  // ?
new.PlayAnimation("walk");    // ?
new.PlayAnimation("run");     // ?
new.PlayAnimation("jump");    // ?
new.PlayAnimation("death");   // ?
// ...unlimited animations! ?
```

---

## ? Checklist

- [ ] Fixed compilation errors
- [ ] Organized sprites (folders OR sprite sheets)
- [ ] Ran auto-detection tool
- [ ] Reviewed detected animations
- [ ] Setup character
- [ ] Tested in Play Mode
- [ ] Can play animations from code

---

## ?? You're Ready!

Your character now supports **unlimited animations** with **auto-detection**!

**Need help?** Check the full guides:
- `MULTI_ANIMATION_SYSTEM_GUIDE.md` - Complete documentation
- `MULTI_ANIMATION_IMPLEMENTATION_SUMMARY.md` - Technical details

**Happy animating!** ???
