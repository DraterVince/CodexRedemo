# Multi-Animation System - Complete Guide

## Overview
The **Multi-Animation System** automatically detects and manages **ALL** your character animations, not just idle and attack!

### What's New
- ? Detects **25+ animation types** automatically
- ? Works with both **folders** and **sprite sheets**
- ? Single controller manages all animations
- ? Easy to add custom animations
- ? Backward compatible with existing setups

## Supported Animation Types

### Combat Animations
- `idle` - Default resting animation
- `attack` / `attack1` / `attack2` / `attack3` - Attack variations
- `block` / `defend` - Defensive stance
- `dodge` / `roll` - Evasive maneuvers
- `hurt` / `hit` / `damage` - Taking damage
- `death` / `die` - Death animation

### Movement Animations
- `walk` - Walking movement
- `run` - Running movement
- `jump` - Jumping
- `fall` - Falling
- `climb` - Climbing
- `crouch` - Crouching
- `slide` - Sliding
- `dash` - Dashing

### Special Animations
- `cast` / `spell` - Magic casting
- `shoot` - Ranged attack
- `reload` - Reloading weapon
- `charge` - Charging attack
- `skill` - Special skill

**...and more! The system searches for all common animation names.**

## File Structure Examples

### Option 1: Folder Structure
```
Assets/
  Characters/
    Knight/
      Idle/   ? Detected!
        frame_0.png
        frame_1.png
      Attack/       ? Detected!
  frame_0.png
  frame_1.png
      Walk/         ? Detected!
        frame_0.png
      Run/ ? Detected!
        frame_0.png
      Jump/  ? Detected!
        frame_0.png
```

### Option 2: Sprite Sheets
```
Assets/
  Characters/
  Knight/
      idle.png      ? Detected!
      attack.png    ? Detected!
      walk.png      ? Detected!
      run.png       ? Detected!
      jump.png      ? Detected!
```

### Option 3: Mixed (Folders + Sprite Sheets)
```
Assets/
  Characters/
 Knight/
      idle.png      ? Sprite sheet
      attack.png    ? Sprite sheet
      Walk/         ? Folder
        frame_0.png
      Run/   ? Folder
        frame_0.png
```

## How to Use

### Step 1: Prepare Your Sprites
1. Import all sprite sheets/images into Unity
2. Slice sprite sheets if needed (Sprite Editor)
3. Organize into folders OR name sprite sheets appropriately
   - Folders: `Idle/`, `Attack/`, `Walk/`, etc.
   - Files: `idle.png`, `attack.png`, `walk.png`, etc.

### Step 2: Run Auto-Setup Tool
1. In Unity: **Tools ? Auto Multi-Animation Setup**
2. Click **"Browse"** and select your character's folder
3. Click **"?? Search for ALL Animations"**
4. Review detected animations in the list
5. (Optional) Drag an existing GameObject to "Target Character"
6. Click **"? Setup Multi-Animation Controller!"**

### Step 3: Test Your Animations
1. Select your character in Hierarchy
2. Enter Play Mode
3. Try these keys:
   - **1** = Idle
   - **Space** or **Left Click** = Attack
   - **W** = Walk
   - **Shift** = Run
   - **Ctrl** = Dodge
   - **2, 3, 4** = Special animations

## Components

### 1. MultiAnimationController
The main controller that manages all animations.

**Key Features:**
- Stores all animation data
- Switches between animations
- Auto-returns to idle after non-looping animations
- Add animations at runtime

**Public Methods:**
```csharp
// Play an animation by name
PlayAnimation(string animationName);

// Play with custom FPS
PlayAnimation(string animationName, int customFPS);

// Check if animation exists
bool HasAnimation(string animationName);

// Get all available animations
List<string> GetAvailableAnimations();

// Get current animation
string GetCurrentAnimation();

// Add animation at runtime
AddAnimation(string name, List<Sprite> sprites, int fps, bool loop, bool pingPong);
```

**Example Usage:**
```csharp
MultiAnimationController controller = GetComponent<MultiAnimationController>();

// Play attack
controller.PlayAnimation("attack");

// Play walk
controller.PlayAnimation("walk");

// Check if has jump animation
if (controller.HasAnimation("jump"))
{
    controller.PlayAnimation("jump");
}

// List all animations
var animations = controller.GetAvailableAnimations();
foreach (string anim in animations)
{
    Debug.Log($"Available: {anim}");
}
```

### 2. MultiAnimationInput
Handles keyboard and mouse input for triggering animations.

**Features:**
- Keyboard key bindings
- Mouse button support
- Movement detection (auto walk/run)
- Custom key bindings
- External triggering support

**Key Bindings (Default):**
- `1` - Idle
- `Space` - Attack
- `W` - Walk
- `Shift` - Run
- `Ctrl` - Dodge
- `Left Click` - Attack
- `Right Click` - Block (configurable)
- `2, 3, 4` - Special animations

**Custom Bindings:**
You can add custom bindings in the Inspector:
1. Expand "Custom Bindings"
2. Set size (e.g., 3)
3. Assign keys and animation names

**Example:**
```csharp
// Trigger animation from code
MultiAnimationInput input = GetComponent<MultiAnimationInput>();
input.TriggerAnimation("death");

// Enable auto-walk on movement
input.autoWalkOnMovement = true;
```

### 3. AutoMultiAnimationSetup (Editor Tool)
The editor tool that auto-detects all animations.

**Location:** `Tools ? Auto Multi-Animation Setup`

**Features:**
- Searches recursively through folders
- Detects 25+ animation types
- Shows all found animations with frame counts
- Auto-configures FPS and loop settings
- One-click setup

## Animation Settings

### Default FPS (Auto-Configured)
- **Idle:** 8 fps
- **Walk:** 12 fps
- **Run:** 16 fps
- **Attack:** 24 fps
- **Jump:** 12 fps
- **Hurt:** 16 fps
- **Dodge:** 20 fps
- **Default:** 12 fps

### Loop Settings (Auto-Configured)
**Looping animations:**
- Idle, Walk, Run, Climb

**Non-looping animations:**
- Attack, Jump, Death, Hurt, Dodge, etc.

**Ping-Pong animations:**
- Idle (creates breathing effect)

## Advanced Usage

### Adding Animations at Runtime
```csharp
MultiAnimationController controller = GetComponent<MultiAnimationController>();

// Load sprites
List<Sprite> newSprites = new List<Sprite>();
// ... load sprites ...

// Add new animation
controller.AddAnimation("victory", newSprites, 15, false, false);

// Play it
controller.PlayAnimation("victory");
```

### Changing Animation Speed
```csharp
// Play attack at double speed
controller.PlayAnimation("attack", 48); // 24 fps * 2
```

### Custom Animation Flow
```csharp
// Chain animations
public IEnumerator AttackCombo()
{
    controller.PlayAnimation("attack1");
    yield return new WaitForSeconds(0.5f);
    
    controller.PlayAnimation("attack2");
    yield return new WaitForSeconds(0.5f);
    
    controller.PlayAnimation("attack3");
}
```

### Disable Auto-Return to Idle
```csharp
MultiAnimationController controller = GetComponent<MultiAnimationController>();
controller.autoReturnToDefault = false;

// Manually return to idle when needed
controller.PlayAnimation("idle");
```

## Comparison: Old vs New System

### Old System (CharacterAnimationController)
? Only supports **2 animations** (idle, attack)
? Requires manual setup for each animation
? Hard-coded animation types
? Difficult to add new animations

### New System (MultiAnimationController)
? Supports **25+ animations** out of the box
? **Auto-detects** all animations
? **Dynamic** - add animations at runtime
? **Extensible** - easy to add custom types

## Migration Guide

### From CharacterAnimationController to MultiAnimationController

**Option 1: Keep Both (Recommended for Testing)**
1. Don't remove old component yet
2. Add `MultiAnimationController` to same object
3. Test new system
4. Remove old component when satisfied

**Option 2: Fresh Setup**
1. Remove `CharacterAnimationController`
2. Run `Tools ? Auto Multi-Animation Setup`
3. Test animations

**Option 3: Manual Conversion**
```csharp
// Old code
CharacterAnimationController oldController;
oldController.PlayAttack();
oldController.PlayIdle();

// New code
MultiAnimationController newController;
newController.PlayAnimation("attack");
newController.PlayAnimation("idle");
```

## Troubleshooting

### No Animations Found
**Problem:** Tool finds 0 animations

**Solutions:**
1. Check folder path is correct
2. Verify sprite sheet is sliced
3. Check folder/file names match supported types
4. Names must be **exact match** (case-insensitive)
   - ? `Idle/`, `idle.png`, `IDLE.png`
   - ? `IdleAnimation/`, `char_idle.png`

### Animation Not Playing
**Problem:** `PlayAnimation()` doesn't work

**Solutions:**
1. Check animation exists: `HasAnimation("name")`
2. Verify sprite count > 0
3. Check SpriteRenderer is attached
4. Enable debug in MultiAnimationInput

### Wrong Animation Playing
**Problem:** Different animation plays than expected

**Check:**
1. Current animation: `GetCurrentAnimation()`
2. Auto-return enabled: `autoReturnToDefault`
3. Loop settings: non-looping animations return to idle

### Performance Issues
**Problem:** Too many SpriteAnimator components

**Solution:**
This is normal! Each animation needs its own `SpriteAnimator`. Unity handles this efficiently. If concerned:
- Limit animations to only what you need
- Don't add unused animation folders

## Best Practices

### Naming Conventions
? **Do:**
- Use lowercase names: `idle.png`, `attack.png`
- Use exact animation names: `walk`, `run`, `jump`
- Keep names simple and clear

? **Don't:**
- Add prefixes: `knight_idle.png`
- Use spaces: `idle animation.png`
- Use special characters: `idle@2x.png`

### Organization
? **Do:**
- Group all animations in character folder
- Use consistent structure (all folders OR all sprite sheets)
- Name sprite sheets descriptively

? **Don't:**
- Mix character animations across multiple folders
- Use inconsistent naming
- Forget to slice sprite sheets

### Performance
? **Do:**
- Use texture atlases when possible
- Compress sprites appropriately
- Remove unused animations

? **Don't:**
- Include duplicate animations
- Use uncompressed textures
- Add animations you don't need

## Examples

### Example 1: Simple Character
```
Assets/Characters/Hero/
  idle.png      (4 frames)
  attack.png    (6 frames)
  walk.png  (8 frames)
  
Result: 3 animations detected!
```

### Example 2: Complex Character
```
Assets/Characters/Warrior/
  Idle/     (10 frames)
  Attack/       (8 frames)
  Attack2/      (10 frames)
  Walk/         (12 frames)
  Run/     (14 frames)
  Jump/    (6 frames)
  Dodge/        (8 frames)
  Death/     (12 frames)
  
Result: 8 animations detected!
```

### Example 3: Mixed Setup
```
Assets/Characters/Mage/
  idle.png      (sprite sheet)
  cast.png      (sprite sheet)
  Walk/         (folder with frames)
  Run/          (folder with frames)
  
Result: 4 animations detected!
```

## FAQ

**Q: Can I add custom animation types?**
A: Yes! Just use `AddAnimation()` at runtime or edit `ANIMATION_NAMES` in `AutoMultiAnimationSetup.cs`.

**Q: Does this work with the old CharacterAnimationController?**
A: They're separate systems. You can use both on different characters, but not on the same character.

**Q: How many animations can I have?**
A: No practical limit! The system supports 25+ built-in types, plus unlimited custom animations.

**Q: Can I change animation speeds at runtime?**
A: Yes! Use `PlayAnimation(name, customFPS)`.

**Q: Does it work with Animator Controller?**
A: This is a separate system that doesn't use Unity's Animator. It's simpler and more direct.

**Q: What if my animation name isn't in the list?**
A: Add it to `ANIMATION_NAMES` array in `AutoMultiAnimationSetup.cs`, or use `AddAnimation()` at runtime.

## Summary

The **Multi-Animation System** is a complete upgrade that:

? Auto-detects **ALL** your animations
? Supports **25+ animation types** out of the box
? Works with **folders** and **sprite sheets**
? **One-click setup** via editor tool
? **Easy to extend** with custom animations
? **Backward compatible** with existing projects

**Get started in 3 steps:**
1. `Tools ? Auto Multi-Animation Setup`
2. Select character folder
3. Click "Setup"!

?? **Ready to animate!**
