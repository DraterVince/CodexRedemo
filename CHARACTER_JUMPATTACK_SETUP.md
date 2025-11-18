# CharacterJumpAttack Component Setup Guide

## Overview
The `CharacterJumpAttack` component is used to make player characters perform jump attack animations when they deal damage to enemies in multiplayer mode.

## Setup Steps

### 1. Add the Component to Your Character Prefab

1. Open your character prefab in Unity (e.g., `Characters/Default`)
2. Select the root GameObject of the character
3. Click "Add Component" in the Inspector
4. Search for "CharacterJumpAttack" and add it

### 2. Configure the Component Settings

#### Basic Settings:
- **Character Scale**: Set to match your character's scale (e.g., `(1, 1, 1)`)
- **Apply Scale On Start**: Check this if you want the scale applied automatically
- **Jump To Enemy Duration**: How long the jump takes (default: 0.5 seconds)
- **Jump Back Duration**: How long to return to original position (default: 0.5 seconds)
- **Jump Height**: How high the character jumps (default: 2.0)
- **Attack Distance**: Distance from enemy when attacking (default: 1.5)
- **Attack Pause Duration**: How long to pause after attack (default: 0.2 seconds)

#### Animation Settings:
- **Use Sprite Animation**: Check this if your character uses Animator animations
- **Character Animator**: Drag your character's Animator component here (or leave empty to auto-detect)
- **Idle Animation Name**: Name of the idle animation state (e.g., "Idle")
- **Attack Animation Trigger**: Name of the attack trigger parameter (e.g., "Attack")
- **Use Attack Trigger**: Check this if you use a trigger, uncheck if you use an animation state name
- **Attack Animation State**: Name of the attack animation state (if not using triggers)

#### Optional Settings:
- **Attack Effect Prefab**: (Optional) Prefab to spawn when attacking
- **Attack Effect Offset**: (Optional) Offset for the effect position

### 3. Set Up the Animator Controller

1. Create or open your character's Animator Controller
2. Add the following states:
   - **Idle**: The character's idle animation
   - **Attack**: The character's attack animation

3. If using triggers:
   - Add a trigger parameter named "Attack" (or whatever you set in "Attack Animation Trigger")
   - Create a transition from "Idle" to "Attack" with the trigger condition
   - Create a transition from "Attack" back to "Idle" (set to "Has Exit Time" = true)

4. If using animation state names:
   - Just make sure the state name matches "Attack Animation State" in the component

### 4. Verify the Setup

1. Make sure your character prefab has an Animator component
2. Make sure the Animator has an Animator Controller assigned
3. Make sure the Animator Controller has the required states/triggers
4. Save the prefab

### 5. Test in Game

1. Start a multiplayer game
2. Play a card with a correct answer
3. Your character should perform a jump attack animation towards the enemy

## Troubleshooting

### "Character prefab doesn't have an Animator component"
- Add an Animator component to your character prefab
- Assign an Animator Controller to the Animator

### "Character has no CharacterJumpAttack component"
- Add the CharacterJumpAttack component to your character prefab
- Follow the setup steps above

### Attack animation doesn't play
- Check that "Use Sprite Animation" is checked
- Check that the Animator Controller is assigned
- Check that the attack trigger/state name matches your Animator Controller
- Check that the transition conditions are set up correctly

### Character doesn't jump
- Check that the CharacterJumpAttack component is on the character prefab
- Check that the component settings are correct
- Check the console for error messages

## Notes

- The CharacterJumpAttack component is optional - the game will work without it, but there won't be jump attack animations
- The component automatically finds the Animator component if not assigned
- The component automatically updates its position when the character moves
- The jump attack animation is triggered when a correct answer is played in multiplayer mode

