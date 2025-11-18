# Multiplayer UI Setup Guide

## Overview

This guide explains the multiplayer-specific UI panels and synchronization features that have been implemented.

## Features Implemented

### 1. Multiplayer Victory Panel
- **Separate from singleplayer**: Uses `victoryPanel` in `SharedMultiplayerGameManager`
- **Next Level Button**: Only visible to the host
- **Random Level Selection**: Loads a random level from `availableLevels` array in `LobbyManager`
- **Return to Lobby Button**: Available for all players

### 2. Multiplayer Game Over Panel
- **Separate from singleplayer**: Uses `gameOverPanel` in `SharedMultiplayerGameManager`
- **Return to Lobby Only**: No level select option (multiplayer-specific)
- **All players see the same panel**

### 3. Multiplayer Pause Panel
- **Separate from singleplayer**: Uses `pausePanel` in `SharedMultiplayerGameManager`
- **No Level Select**: Cannot go back to level select (only accessible in singleplayer)
- **Synchronized**: When one player pauses, everyone pauses
- **Resume Button**: Resumes for all players
- **Return to Lobby Button**: Available from pause menu

### 4. Synchronized Pause System
- **ESC Key**: Press ESC to pause/unpause (synchronized across all players)
- **RPC-Based**: Uses Photon RPCs to sync pause state
- **Time Scale**: Sets `Time.timeScale = 0` when paused, `1` when resumed

### 5. Synchronized Expected Output Panel
- **Minimize/Maximize Sync**: When one player minimizes/maximizes the expected output panel, it syncs for everyone
- **RPC-Based**: Uses Photon RPCs to sync panel state
- **Automatic Detection**: `AnimatedPanel` automatically detects multiplayer mode and syncs

## Unity Setup Instructions

### Step 1: Create Multiplayer UI Panels

In your multiplayer level scenes, create separate UI panels:

#### Victory Panel (Multiplayer)
1. Create a new Panel GameObject: `MultiplayerVictoryPanel`
2. Add UI elements:
   - Text: "VICTORY! All enemies defeated!"
   - Button: "Next Level" (host only)
   - Button: "Return to Lobby"
3. Assign to `SharedMultiplayerGameManager.victoryPanel`

#### Game Over Panel (Multiplayer)
1. Create a new Panel GameObject: `MultiplayerGameOverPanel`
2. Add UI elements:
   - Text: "GAME OVER - Shared health depleted!"
   - Button: "Return to Lobby" (no level select)
3. Assign to `SharedMultiplayerGameManager.gameOverPanel`

#### Pause Panel (Multiplayer)
1. Create a new Panel GameObject: `MultiplayerPausePanel`
2. Add UI elements:
   - Text: "PAUSED"
   - Button: "Resume"
   - Button: "Return to Lobby" (no level select option)
3. Assign to `SharedMultiplayerGameManager.pausePanel`

### Step 2: Assign References in SharedMultiplayerGameManager

In the Inspector, assign the following:

```
Game Over (Multiplayer):
  - Game Over Panel: [Drag MultiplayerGameOverPanel]
  - Victory Panel: [Drag MultiplayerVictoryPanel]
  - Game Over Text: [Drag TextMeshProUGUI component]
  - Next Level Button: [Drag Next Level button from Victory Panel]
  - Return To Lobby Button: [Drag Return to Lobby button]

Pause Panel (Multiplayer):
  - Pause Panel: [Drag MultiplayerPausePanel]
  - Resume Button: [Drag Resume button]
  - Return To Lobby From Pause Button: [Drag Return to Lobby button from Pause Panel]

Expected Output Panel Sync:
  - Expected Output Panel: [Drag AnimatedPanel component from ExpectedOutputPanel GameObject]
```

### Step 3: Configure Available Levels

In `LobbyManager`, the `availableLevels` array defines which levels can be randomly selected:

```csharp
[SerializeField] private string[] availableLevels = { "Level_1", "Level_2", "Level_3" };
```

**Important**: Update these scene names to match your actual multiplayer level scene names!

### Step 4: Hide Singleplayer UI in Multiplayer

Make sure singleplayer UI panels (YouWinScreen, GameOverScreen, etc.) are:
- Hidden when in multiplayer mode
- Or disabled in multiplayer scenes

## How It Works

### Victory Flow
1. All enemies defeated → `RPC_GameOver(true)` called
2. Victory panel shows for all players
3. **Host sees**: Next Level button + Return to Lobby button
4. **Other players see**: Return to Lobby button only
5. Host clicks "Next Level" → Random level from `availableLevels` loads

### Game Over Flow
1. Shared health reaches 0 → `RPC_GameOver(false)` called
2. Game Over panel shows for all players
3. **All players see**: Return to Lobby button only
4. Clicking "Return to Lobby" loads `MultiplayerLobby` scene

### Pause Flow
1. Any player presses ESC → `PauseGame()` called
2. RPC sent to all players → `RPC_PauseGame()` executed
3. `Time.timeScale = 0` for all players
4. Pause panel shows for all players
5. Any player presses ESC or clicks Resume → `ResumeGame()` called
6. RPC sent to all players → `RPC_ResumeGame()` executed
7. `Time.timeScale = 1` for all players
8. Pause panel hidden for all players

### Expected Output Panel Sync Flow
1. Any player clicks Expected Output Panel → `OnPanelClicked()` called
2. `AnimatedPanel` detects multiplayer mode
3. Calls `SharedMultiplayerGameManager.SyncExpectedOutputPanel(isExpanded)`
4. RPC sent to all players → `RPC_SyncExpectedOutputPanel()` executed
5. All players' panels minimize/maximize to match

## Code Changes Summary

### SharedMultiplayerGameManager.cs
- Added multiplayer-specific UI panel references
- Added pause system with RPC synchronization
- Added expected output panel synchronization
- Updated `RPC_GameOver()` to show/hide Next Level button based on host status
- Added `LoadNextRandomLevel()` to load random level from `availableLevels`
- Added `Update()` method to handle ESC key for pause

### AnimatedPanel.cs
- Made `isExpanded` public for synchronization
- Added `SyncPanelState()` method to sync with SharedMultiplayerGameManager
- Added `SetPanelState()` method to force panel state (called by RPC)

### LobbyManager.cs
- Added `GetAvailableLevels()` public method for SharedMultiplayerGameManager to access available levels

## Testing Checklist

- [ ] Victory panel shows Next Level button only for host
- [ ] Victory panel shows Return to Lobby button for all players
- [ ] Next Level button loads random level from availableLevels
- [ ] Game Over panel shows Return to Lobby button only (no level select)
- [ ] Pause panel shows Resume and Return to Lobby buttons (no level select)
- [ ] ESC key pauses game for all players
- [ ] ESC key or Resume button resumes game for all players
- [ ] Expected output panel minimize/maximize syncs across all players
- [ ] All panels are separate from singleplayer UI

## Notes

- The pause system uses `Time.timeScale` which affects all time-based operations
- The expected output panel sync uses the existing `AnimatedPanel` component
- All RPCs use `RpcTarget.All` to ensure all players see the same state
- The Next Level button only appears for the host to prevent conflicts
- Level names in `availableLevels` must match actual scene names in Build Settings

