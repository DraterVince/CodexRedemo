# Sprite Sheet Detection Update

## Summary
Updated the animation setup tools to detect sprite sheets named "attack" and "idle" in addition to folders with those names.

## Changes Made

### 1. AutoAnimationSetup.cs
- **Added**: `LoadSpritesFromSpriteSheet()` method to load sprites from individual sprite sheet files
- **Modified**: `SearchDirectory()` method to check for:
  - Folders named "idle" or "attack" (existing functionality)
  - **NEW**: Sprite sheet files named "idle.png", "attack.png", etc.
- **Updated**: Help text and tips to mention sprite sheet detection

### 2. AutoAnimatorControllerSetup.cs
- **Added**: `SearchForSpriteSheets()` method to find sprite sheets named "idle" or "attack"
- **Modified**: `SearchForAnimations()` method to:
  - Search for existing animation clips (existing functionality)
  - **NEW**: Search for sprite sheets if animations not found
- **Updated**: Help text to mention sprite sheet detection

## How It Works

### Detection Priority
1. First checks for **folders** named "idle" or "attack"
2. If not found, checks for **sprite sheet files** named "idle" or "attack"
3. Case-insensitive matching (works with "Idle", "idle", "IDLE", etc.)

### Supported File Names
- `idle.png`, `Idle.png`, `IDLE.png`
- `attack.png`, `Attack.png`, `ATTACK.png`
- Works with any image format (.png, .jpg, .jpeg, etc.)

### Requirements
- Sprite sheets must be properly sliced in Unity
- Sprites will be sorted alphabetically by name

## Usage

### Tool: "Tools > Auto Setup Character Animations"
1. Click "Search for Animations"
2. Tool will now find:
   - Folders named "idle" or "attack" containing sprites
   - **OR** sprite sheets named "idle" or "attack"
3. Click "Setup Character Animations!" to apply

### Tool: "Tools > Auto Animator Controller Setup"
1. Click "Search for Animations"
2. Tool will find existing animation clips, **OR** sprite sheets
3. Click "Create Animator Controller!" to generate the controller

## Benefits
- More flexible sprite organization
- Works with single sprite sheet files
- No need to create separate folders for simple animations
- Maintains backward compatibility with folder-based structure

## Examples

### Example 1: Sprite Sheet Files
```
Assets/
  Characters/
    Knight/
      idle.png      ? Tool will find this!
      attack.png    ? Tool will find this!
```

### Example 2: Folder Structure (Still Supported)
```
Assets/
  Characters/
  Knight/
      Idle/      ? Tool will find this!
        frame_0.png
        frame_1.png
      Attack/     ? Tool will find this!
     frame_0.png
        frame_1.png
```

### Example 3: Mixed Structure
```
Assets/
  Characters/
    Knight/
      idle.png      ? Tool will find sprite sheet first
      Attack/       ? Will search for this if no attack.png found
        frame_0.png
```

## Technical Details

### New Methods

#### `LoadSpritesFromSpriteSheet(string texturePath)`
- Loads all sprites from a single texture file
- Handles multi-sprite textures (sprite sheets)
- Sorts sprites alphabetically

#### `SearchForSpriteSheets(string directory)`
- Searches for texture files in directory
- Checks if filename matches "idle" or "attack"
- Recursively searches subdirectories
- Only in `AutoAnimatorControllerSetup.cs`

### Modified Methods

#### `SearchDirectory(string directory)` - AutoAnimationSetup.cs
- Added sprite sheet detection after folder check
- Uses `AssetDatabase.FindAssets("t:Texture2D")` to find textures
- Checks filename matches "idle" or "attack"

#### `SearchForAnimations()` - AutoAnimatorControllerSetup.cs
- Calls new `SearchForSpriteSheets()` if animations not found
- Maintains existing animation clip search functionality

## Testing Checklist
- [x] Compiles without errors
- [x] Backward compatible with folder structure
- [x] Detects sprite sheets named "idle"
- [x] Detects sprite sheets named "attack"
- [x] Case-insensitive matching works
- [x] Help text updated
- [x] Tips updated
