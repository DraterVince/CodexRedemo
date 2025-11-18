# Multiplayer Character Spawn Information

## Where Multiplayer Characters Spawn

### Initial Spawn Position
- **Position**: `offScreenLeft` (default: `Vector3(-15f, 0f, 0f)`)
- **Location**: In `SharedMultiplayerGameManager.cs`, line 555
- **When**: Characters are instantiated here when players join the room
- **State**: Characters are created as **inactive** (`SetActive(false)`) at this position

### Display Position (When It's Their Turn)
- **Position**: `characterDisplayPosition` (Transform set in Unity Inspector)
- **Location**: Characters are moved here when it's their turn
- **How**: Characters slide in from `offScreenRight` (default: `Vector3(15f, 0f, 0f)`) to `characterDisplayPosition`

## Character Flow

1. **Spawn**: Character instantiated at `offScreenLeft` position (off-screen left)
2. **Storage**: Character stored in `playerCharacters` dictionary (inactive)
3. **Turn Start**: When it's a player's turn:
   - Character slides in from `offScreenRight` (off-screen right)
   - Character moves to `characterDisplayPosition` (visible position)
   - Character becomes active and plays idle animation
4. **Turn End**: Character slides out to `offScreenLeft` (off-screen left) and becomes inactive

## Setup in Unity Inspector

### SharedMultiplayerGameManager Component
- **characterDisplayPosition**: Assign a Transform/GameObject where you want characters to appear during their turn
  - This is the visible position where characters are displayed
  - Example: Create an empty GameObject at the desired position and assign it here

- **offScreenLeft**: Vector3 position for initial spawn (default: `(-15, 0, 0)`)
  - Characters spawn here initially (off-screen left)

- **offScreenRight**: Vector3 position for slide-in animation (default: `(15, 0, 0)`)
  - Characters slide in from here when switching turns

## Singleplayer vs Multiplayer

### Singleplayer
- Uses `PlayCardButton.playerCharacter` GameObject
- Character is always visible and active
- **Now automatically disabled in multiplayer mode**

### Multiplayer
- Uses `SharedMultiplayerGameManager` character switching system
- Characters spawn at `offScreenLeft`, then move to `characterDisplayPosition` when it's their turn
- Only the current player's character is visible
- Singleplayer character is automatically disabled when in multiplayer mode

## Code References

- **Spawn Location**: `SharedMultiplayerGameManager.cs`, line 555
- **Display Location**: `SharedMultiplayerGameManager.cs`, line 716 (`characterDisplayPosition.position`)
- **Slide Animation**: `SharedMultiplayerGameManager.cs`, `SwitchCharacterAnimated()` method
- **Singleplayer Disable**: `PlayCardButton.cs`, line 75

