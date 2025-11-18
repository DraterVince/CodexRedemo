# Character Prefab Setup Guide

## ✅ Your Setup is Correct!

Your character prefab setup is **correct**. Here's what you have and what's needed:

### Current Setup (Correct):
1. ✅ **Root GameObject** - Empty GameObject
2. ✅ **SpriteRenderer Component** - For displaying the character sprite
3. ✅ **CharacterJumpAttack Component** - For jump attack animations
4. ✅ **Animator Component** - For playing animations
5. ✅ **Animator Controller** - Assigned to the Animator

---

## 📋 Required Components Checklist

### 1. Root GameObject
- **Name**: Should match your character name (e.g., "Knight", "Ronin", "Daimyo")
- **Position**: Can be anywhere (will be repositioned by game manager)
- **Scale**: Usually (1, 1, 1) unless you want to scale the character

### 2. SpriteRenderer Component
- ✅ **Sprite**: Must have a sprite assigned (your character's sprite)
- ✅ **Sorting Layer**: Set appropriately (e.g., "Characters", "Player")
- ✅ **Order in Layer**: Set to ensure correct rendering order
- ✅ **Color**: Usually white (1, 1, 1, 1) unless you want tinting

### 3. CharacterJumpAttack Component
- ✅ **Character Scale**: Set to match your character's scale
- ✅ **Apply Scale On Start**: Check if you want automatic scaling
- ✅ **Jump To Enemy Duration**: 0.5 seconds (default)
- ✅ **Jump Back Duration**: 0.5 seconds (default)
- ✅ **Jump Height**: 2.0 (default)
- ✅ **Attack Distance**: 1.5 (default)
- ✅ **Use Sprite Animation**: ✅ **MUST BE CHECKED**
- ✅ **Character Animator**: Should auto-detect, but you can drag the Animator here
- ✅ **Idle Animation Name**: "Idle" (must match your Animator Controller state name)
- ✅ **Attack Animation Trigger**: "Attack" (must match your Animator Controller trigger parameter)
- ✅ **Use Attack Trigger**: ✅ **CHECKED** if using triggers, **UNCHECKED** if using state names
- ✅ **Attack Animation State**: "Attack" (only used if `Use Attack Trigger` is unchecked)

### 4. Animator Component
- ✅ **Controller**: Must have an Animator Controller assigned
- ✅ **Avatar**: Not needed for 2D sprites (leave as None)
- ✅ **Update Mode**: Normal (default)
- ✅ **Culling Mode**: Always Animate (default)

### 5. Animator Controller
- ✅ **Idle State**: Must exist with name matching "Idle Animation Name"
- ✅ **Attack State**: Must exist with name matching "Attack Animation State"
- ✅ **Attack Trigger**: Must exist with name matching "Attack Animation Trigger" (if using triggers)
- ✅ **Transitions**:
  - Idle → Attack (triggered by "Attack" trigger)
  - Attack → Idle (has exit time = true)

---

## 🔍 Common Issues and Fixes

### Issue 1: Character Not Visible
**Problem**: SpriteRenderer has no sprite assigned
**Fix**: 
1. Select your character prefab
2. In the SpriteRenderer component, assign a sprite to the "Sprite" field
3. Make sure the sprite is imported correctly in Unity

### Issue 2: Animations Not Playing
**Problem**: Animator Controller not assigned or incorrect setup
**Fix**:
1. Select your character prefab
2. In the Animator component, assign your Animator Controller
3. Verify the Animator Controller has "Idle" and "Attack" states
4. Verify the "Attack" trigger parameter exists (if using triggers)

### Issue 3: CharacterJumpAttack Not Working
**Problem**: Component settings don't match Animator Controller
**Fix**:
1. Check "Use Sprite Animation" is checked
2. Verify "Idle Animation Name" matches your Animator Controller's idle state name
3. Verify "Attack Animation Trigger" matches your Animator Controller's trigger parameter name
4. Verify "Use Attack Trigger" is checked if using triggers, unchecked if using state names

### Issue 4: Character Doesn't Jump
**Problem**: CharacterJumpAttack component not found or not configured
**Fix**:
1. Verify CharacterJumpAttack component is on the root GameObject
2. Check the component settings are correct
3. Check the console for error messages

---

## 🎯 Recommended Setup Steps

1. **Create the Prefab**:
   - Create an empty GameObject
   - Name it after your character (e.g., "Knight")

2. **Add SpriteRenderer**:
   - Add SpriteRenderer component
   - Assign your character's sprite
   - Set Sorting Layer and Order in Layer

3. **Add Animator**:
   - Add Animator component
   - Create or assign an Animator Controller
   - Configure the Animator Controller with Idle and Attack states

4. **Add CharacterJumpAttack**:
   - Add CharacterJumpAttack component
   - Configure the settings:
     - Check "Use Sprite Animation"
     - Set "Idle Animation Name" to match your Animator Controller
     - Set "Attack Animation Trigger" to match your Animator Controller
     - Set "Use Attack Trigger" to true (if using triggers)

5. **Save as Prefab**:
   - Drag the GameObject into your `Assets/Resources/Characters/` folder
   - Name it to match your cosmetic name (e.g., "Knight.prefab")

---

## ✅ Verification Checklist

Before testing, verify:
- [ ] SpriteRenderer has a sprite assigned
- [ ] Animator has an Animator Controller assigned
- [ ] Animator Controller has "Idle" state
- [ ] Animator Controller has "Attack" state
- [ ] Animator Controller has "Attack" trigger parameter (if using triggers)
- [ ] CharacterJumpAttack "Use Sprite Animation" is checked
- [ ] CharacterJumpAttack "Idle Animation Name" matches Animator Controller
- [ ] CharacterJumpAttack "Attack Animation Trigger" matches Animator Controller
- [ ] CharacterJumpAttack "Use Attack Trigger" matches your setup (checked = triggers, unchecked = state names)
- [ ] Prefab is saved in `Assets/Resources/Characters/` folder
- [ ] Prefab name matches your cosmetic name (e.g., "Knight.prefab")

---

## 🚀 Testing

1. Start a multiplayer game
2. Play a card with a correct answer
3. Your character should:
   - Jump towards the enemy
   - Play the attack animation
   - Stay at the attack position (in multiplayer)
   - Slide off screen (handled by game manager)

---

## 📝 Notes

- The CharacterJumpAttack component can auto-detect the Animator component
- The SpriteRenderer is only needed for visual display - CharacterJumpAttack doesn't directly use it
- The root GameObject will be moved by CharacterJumpAttack during jump attacks
- In multiplayer, characters slide off screen after attacking (handled by SharedMultiplayerGameManager)
- In singleplayer, characters return to their original position after attacking (if `returnToStartPositionAfterAttack` is true)

---

## 🆘 Still Having Issues?

Check the console for error messages:
- `"No Animator found"` → Add Animator component
- `"No Animator Controller assigned"` → Assign Animator Controller
- `"Idle state 'Idle' not found"` → Add Idle state to Animator Controller
- `"Character has no CharacterJumpAttack component"` → Add CharacterJumpAttack component
- `"Could not load character prefab"` → Check prefab is in Resources/Characters/ folder

If you're still having issues, check:
1. Console logs for specific error messages
2. Animator Controller setup (states, transitions, parameters)
3. CharacterJumpAttack component settings
4. Prefab location and naming

