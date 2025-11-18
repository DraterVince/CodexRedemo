# Player List Prefab Setup Guide

## Overview
The player list prefab is already created and configured. You just need to set it up in your Unity scene.

## Prefab Location
- **Prefab Path**: `Assets/Prefabs/Multiplayer/PlayerListEntry.prefab`
- **Script**: `Assets/Scripts/PlayerListEntry.cs`

## Prefab Components
The prefab includes:
- ✅ **PlayerNameText** - TextMeshProUGUI showing player name
- ✅ **ReadyStatusText** - TextMeshProUGUI showing "[READY]" or "[Not Ready]"
- ✅ **KickButton** - Button for host to kick players (only visible to host)
- ✅ **HostIndicator** - Image/GameObject showing host badge
- ✅ **PlayerListEntry Script** - All references are already assigned

## Setup Steps in Unity

### Step 1: Create Player List Container
1. Open your **Multiplayer** scene
2. Find the **RoomPanel** GameObject in the hierarchy
3. Right-click on **RoomPanel** → **UI** → **Panel** (or use an existing container)
4. Rename it to **"PlayerListContainer"**
5. Add a **Vertical Layout Group** component to it:
   - **Spacing**: 10
   - **Child Force Expand**: Width = true, Height = false
   - **Child Control Size**: Width = true, Height = true

### Step 2: Assign References in LobbyManager
1. Select the GameObject with the **LobbyManager** component
2. In the Inspector, find the **"Player List UI"** section
3. Assign the references:
   - **Player List Container**: Drag the **PlayerListContainer** GameObject you created
   - **Player Entry Prefab**: Drag `Assets/Prefabs/Multiplayer/PlayerListEntry.prefab` from the Project window

### Step 3: Verify Setup
The prefab should automatically work once assigned. When you join a room:
- Player names will appear in the list
- Ready status will show for each player
- Host indicator will show for the host
- Kick button will only appear for the host (and only for non-host players)

## How It Works

### For Host:
- Sees all players with their names
- Sees ready status for each player
- Sees host indicator on their own entry
- Sees kick buttons next to all other players (not themselves)

### For Regular Players:
- Sees all players with their names
- Sees ready status for each player
- Sees host indicator on the host's entry
- **Does NOT see kick buttons** (they're hidden)

## Prefab Structure
```
PlayerListEntry (Root)
└── Canvas
    ├── HostIndicator (Image - shows crown/badge for host)
    ├── PlayerNameText (TextMeshProUGUI - player name)
    ├── ReadyStatusText (TextMeshProUGUI - ready status)
    └── KickButton (Button - only visible to host)
        └── Text (TMP) - "Kick" label
```

## Troubleshooting

### Players not showing up?
- Check that `playerListContainer` is assigned in LobbyManager
- Check that `playerEntryPrefab` is assigned in LobbyManager
- Make sure you're in a room (not just lobby)
- Check the Console for error messages

### Kick button not appearing?
- Make sure you're the host (Master Client)
- Kick buttons only appear for non-host players
- Check that the prefab has the KickButton assigned in PlayerListEntry script

### Ready status not updating?
- Players need to click the "READY" button to set their ready status
- The status updates automatically when players change their ready state

## Customization

### Colors
You can customize colors in the PlayerListEntry prefab:
- **Ready Color**: Green (default)
- **Not Ready Color**: Yellow (default)
- **Kick Button Normal Color**: Red (default)
- **Kick Button Disabled Color**: Gray (default)

### Layout
The prefab uses a Horizontal Layout Group. You can adjust:
- Spacing between elements
- Sizes of individual elements
- Overall prefab size

## Notes
- The prefab is already fully configured with all script references
- The kick functionality is already implemented in LobbyManager
- The ready system is already integrated
- All you need to do is assign the references in the Unity Inspector!

