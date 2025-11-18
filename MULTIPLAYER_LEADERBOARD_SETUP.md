# Multiplayer Leaderboard Setup Guide

## Overview

This guide explains how to set up the multiplayer leaderboard system that tracks the best single-run scores (levels beaten in one session) and displays the top 10 players in the main menu.

## Features

- ✅ Tracks levels beaten per player in each multiplayer session
- ✅ Saves best single-run scores to Supabase
- ✅ Shows levels beaten in victory/defeat screens
- ✅ Displays top 10 best scores in main menu leaderboard
- ✅ Syncs leaderboard data across all players

---

## Step 1: Supabase Database Setup

### Create the Leaderboard Table

1. **Open your Supabase Dashboard**
   - Go to your Supabase project
   - Navigate to **SQL Editor**

2. **Run this SQL to create the table:**

```sql
CREATE TABLE IF NOT EXISTS multiplayer_leaderboard (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username TEXT NOT NULL,
    user_id TEXT,
    levels_beaten INTEGER NOT NULL DEFAULT 0,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(username)
);

-- Create index for faster queries
CREATE INDEX IF NOT EXISTS idx_leaderboard_levels_beaten ON multiplayer_leaderboard(levels_beaten DESC);

-- Enable Row Level Security (optional, but recommended)
ALTER TABLE multiplayer_leaderboard ENABLE ROW LEVEL SECURITY;

-- Create policy to allow all reads (so everyone can see leaderboard)
CREATE POLICY "Allow public read access" ON multiplayer_leaderboard
    FOR SELECT USING (true);

-- Create policy to allow authenticated users to insert/update
CREATE POLICY "Allow authenticated insert/update" ON multiplayer_leaderboard
    FOR ALL USING (auth.role() = 'authenticated');
```

3. **Verify the table was created:**
   - Go to **Table Editor** in Supabase
   - You should see `multiplayer_leaderboard` table with columns:
     - `id` (uuid)
     - `username` (text)
     - `user_id` (text, nullable)
     - `levels_beaten` (integer)
     - `updated_at` (timestamp)

---

## Step 2: Unity Scene Setup

### 2.1 Create MultiplayerLeaderboardManager GameObject

**IMPORTANT:** This manager uses `DontDestroyOnLoad`, so you only need to place it **ONCE** in **ONE** scene. It will persist across all scenes automatically.

**Recommended Placement:**
- **MultiplayerLobby scene** (recommended) - Available when entering multiplayer
- **OR Main Menu scene** - Available from the start, works everywhere

1. **In your chosen scene (MultiplayerLobby recommended):**
   - Right-click in Hierarchy → **Create Empty**
   - Name it: `MultiplayerLeaderboardManager`

2. **Add Components:**
   - Select `MultiplayerLeaderboardManager`
   - Add Component → `MultiplayerLeaderboardManager` (script)
   - Add Component → `Photon View`

3. **Configure PhotonView:**
   - View ID: `999` (or auto-assign)
   - Observed Components: None (uses RPCs)

**Note:** The singleton pattern ensures only one instance exists. If you place it in multiple scenes, duplicates will be automatically destroyed.

### 2.2 Assign UI References (Optional)

**IMPORTANT:** Since the manager is in the Main Menu scene, you **don't need to assign** the `Levels Beaten Text` reference. The manager will automatically find it at runtime when the victory/defeat panels are shown.

**Optional Setup (if you want to assign it manually):**
- If you prefer, you can leave `Levels Beaten Text` empty - it will be found automatically
- Or name a TextMeshProUGUI GameObject `"LevelsBeatenText"` in your victory/defeat panels, and it will be found by name

**In the MultiplayerLeaderboardManager Inspector:**
```
UI References:
  - Levels Beaten Text: [Leave empty - will find at runtime]
  - Levels Beaten Text GameObject Name: "LevelsBeatenText" (optional - change if your text has a different name)
  - Leaderboard Container: [Leave empty - used by LeaderboardPanel]
  - Player Entry Prefab: [Leave empty - used by LeaderboardPanel]
```

**How it finds the text at runtime:**
1. First, searches for a GameObject named `"LevelsBeatenText"` (or the name you set)
2. Then searches inside `VictoryPanel` or `GameOverPanel` for TextMeshProUGUI components
3. Finally, searches all TextMeshProUGUI objects for ones with "LevelsBeaten" or "Leaderboard" in the name

**Note:** The `levelsBeatenText` should be a TextMeshProUGUI component in your victory/defeat panels.

---

## Step 3: Victory/Defeat Panel Setup

**IMPORTANT:** Since `MultiplayerLeaderboardManager` is in the Main Menu scene, you **don't need to manually assign** the text reference. The manager will automatically find it at runtime.

### 3.1 Add Levels Beaten Text to Victory Panel

1. **Open your Multiplayer Victory Panel** (in multiplayer level scenes)
2. **Add a TextMeshProUGUI:**
   - Right-click on Victory Panel → **UI** → **Text - TextMeshPro**
   - **Name it: `LevelsBeatenText`** (important - this is how it will be found)
   - Position it below the victory message
   - Set initial text: `"Levels Beaten in This Lobby: 0"` (will be updated automatically)

**That's it!** The manager will automatically find this text when the victory panel is shown.

### 3.2 Add Levels Beaten Text to Defeat Panel

1. **Open your Multiplayer Game Over Panel**
2. **Add a TextMeshProUGUI:**
   - Right-click on Game Over Panel → **UI** → **Text - TextMeshPro**
   - **Name it: `LevelsBeatenText`** (or any name containing "LevelsBeaten" or "Leaderboard")
   - Position it below the game over message
   - Set initial text: `"Levels Beaten in This Lobby: 0"` (will be updated automatically)

**Note:** If you name both texts `LevelsBeatenText`, the manager will find whichever panel is active. Alternatively, you can use different names and the manager will search for both.

---

## Step 4: Multiplayer Lobby Leaderboard Panel Setup

### 4.1 Create Leaderboard Panel UI

1. **In your MultiplayerLobby scene:**
   - Right-click on Canvas → **UI** → **Panel**
   - Name it: `LeaderboardPanel`

2. **Add UI Elements:**
   - **Title Text:**
     - Right-click on LeaderboardPanel → **UI** → **Text - TextMeshPro**
     - Name: `TitleText`
     - Text: `"Multiplayer Leaderboard\n(Top 10 Best Single-Run Scores)"`
     - Position at top of panel
   
   - **Container:**
     - Right-click on LeaderboardPanel → **UI** → **Panel**
     - Name: `LeaderboardContainer`
     - Add **Vertical Layout Group** component:
       - Spacing: 10
       - Child Force Expand: Width = true, Height = false
       - Child Control Size: Width = true, Height = true
   
   - **Close Button:**
     - Right-click on LeaderboardPanel → **UI** → **Button - TextMeshPro**
     - Name: `CloseButton`
     - Text: `"Close"`
     - Position at bottom of panel

3. **Hide panel initially:**
   - Uncheck the `LeaderboardPanel` GameObject (set inactive)

### 4.2 Add LeaderboardPanel Script

1. **Select LeaderboardPanel GameObject**
2. **Add Component → `LeaderboardPanel` (script)**
3. **Assign References in Inspector:**

```
UI References:
  - Leaderboard Panel: [Drag LeaderboardPanel GameObject]
  - Leaderboard Container: [Drag LeaderboardContainer Transform]
  - Leaderboard Entry Prefab: [Optional - create a prefab for entries]
  - Close Button: [Drag CloseButton]
  - Title Text: [Drag TitleText]

Leaderboard Entry UI (if using prefab):
  - Rank Text Index: 0
  - Name Text Index: 1
  - Score Text Index: 2
```

### 4.3 Create Leaderboard Entry Prefab (Optional)

If you want custom styling for leaderboard entries:

1. **Create Entry Prefab:**
   - Right-click in Project → **UI** → **Panel**
   - Name: `LeaderboardEntryPrefab`
   - Add **Horizontal Layout Group**:
     - Spacing: 10
     - Child Force Expand: Width = false, Height = true
   
2. **Add Text Elements:**
   - **Rank Text:** TextMeshProUGUI (e.g., "#1")
   - **Name Text:** TextMeshProUGUI (e.g., "PlayerName")
   - **Score Text:** TextMeshProUGUI (e.g., "10 levels")
   
3. **Assign to LeaderboardPanel:**
   - Drag `LeaderboardEntryPrefab` to **Leaderboard Entry Prefab** field

**Note:** If you don't create a prefab, the system will create simple text entries automatically.

### 4.4 Add Leaderboard Button to Multiplayer Lobby

1. **In MultiplayerLobby scene:**
   - Find or create a button for the leaderboard (can be in the lobby panel or room panel)
   - Name it: `LeaderboardButton`

2. **Connect Button to LeaderboardPanel:**

   **Option A: Add to LobbyManager (Recommended)**
   
   - Open `LobbyManager.cs`
   - Add these fields in the `[Header("Room Panel")]` section or create a new `[Header("Leaderboard")]` section:
     ```csharp
     [Header("Leaderboard")]
     [SerializeField] private Button leaderboardButton;
     [SerializeField] private LeaderboardPanel leaderboardPanel;
     ```
   - In `SetupButtons()` method, add:
     ```csharp
     if (leaderboardButton != null && leaderboardPanel != null)
     {
         leaderboardButton.onClick.AddListener(() => leaderboardPanel.OpenLeaderboard());
     }
     ```
   - In Unity Inspector, assign:
     - **Leaderboard Button:** Drag `LeaderboardButton`
     - **Leaderboard Panel:** Drag the GameObject with `LeaderboardPanel` script
   
   **Option B: Create a Simple Handler Script (Alternative)**
   
   - Create a new GameObject in MultiplayerLobby scene: `LeaderboardButtonHandler`
   - Add Component → **New Script** → `LeaderboardButtonHandler`
   - Copy this script:
     ```csharp
     using UnityEngine;
     using UnityEngine.UI;
     
     public class LeaderboardButtonHandler : MonoBehaviour
     {
         [SerializeField] private Button leaderboardButton;
         [SerializeField] private LeaderboardPanel leaderboardPanel;
         
         private void Start()
         {
             if (leaderboardButton != null && leaderboardPanel != null)
             {
                 leaderboardButton.onClick.AddListener(() => leaderboardPanel.OpenLeaderboard());
             }
         }
     }
     ```
   - In Unity Inspector, assign:
     - **Leaderboard Button:** Drag `LeaderboardButton`
     - **Leaderboard Panel:** Drag the GameObject with `LeaderboardPanel` script

---

## Step 5: Verify Setup

### Checklist

- [ ] Supabase table `multiplayer_leaderboard` created
- [ ] `MultiplayerLeaderboardManager` GameObject created in MultiplayerLobby scene
- [ ] `MultiplayerLeaderboardManager` script added with PhotonView
- [ ] Levels Beaten Text added to Victory Panel
- [ ] Levels Beaten Text added to Defeat Panel
- [ ] Levels Beaten Text assigned in MultiplayerLeaderboardManager
- [ ] Leaderboard Panel created in MultiplayerLobby scene
- [ ] `LeaderboardPanel` script added and references assigned
- [ ] Leaderboard Button added to MultiplayerLobby
- [ ] LobbyManager (or LeaderboardButtonHandler) has leaderboard button and panel assigned

---

## Step 6: Testing

### Test 1: Levels Beaten Tracking

1. **Start a multiplayer game:**
   - Join a room with 2+ players
   - Start a game
   - Beat a level (victory)
   - Check victory screen: Should show "Levels Beaten in This Lobby: 1"

2. **Beat another level:**
   - Beat another level
   - Should show "Levels Beaten in This Lobby: 2"

3. **Lose the game:**
   - Let shared health reach 0
   - Check defeat screen: Should show final count (e.g., "Levels Beaten in This Lobby: 5")
   - Check Supabase: Should see entries with the scores

### Test 2: Leaderboard Display

1. **Open Main Menu:**
   - Return to main menu
   - Click Leaderboard button
   - Should see top 10 players with their best scores

2. **Verify Top Scores:**
   - Check that scores are sorted highest to lowest
   - Check that only top 10 are shown

### Test 3: Best Score Saving

1. **First Run:**
   - Beat 5 levels, then lose
   - Check Supabase: Should show 5 levels

2. **Second Run (Better):**
   - Beat 10 levels, then lose
   - Check Supabase: Should update to 10 levels (not add 5+10)

3. **Third Run (Worse):**
   - Beat 3 levels, then lose
   - Check Supabase: Should still show 10 levels (best score)

---

## Troubleshooting

### Leaderboard not showing in main menu?

- **Check:** Is `MultiplayerLeaderboardManager` in the scene?
- **Check:** Does it have `DontDestroyOnLoad`? (It should persist)
- **Check:** Is `LeaderboardPanel` script finding the manager?
- **Solution:** Make sure `MultiplayerLeaderboardManager` is created in MultiplayerLobby scene and persists

### Scores not saving to Supabase?

- **Check:** Supabase table exists and has correct columns
- **Check:** RLS policies allow insert/update
- **Check:** Console for error messages
- **Check:** Only master client saves (to avoid duplicates)

### Levels beaten not showing in victory/defeat screen?

- **Check:** `levelsBeatenText` is assigned in MultiplayerLeaderboardManager
- **Check:** Text component is active and visible
- **Check:** Victory/defeat panels are calling `UpdateLevelsBeatenDisplay()`

### Leaderboard shows wrong scores?

- **Check:** Supabase table has correct data
- **Check:** Leaderboard is loading from Supabase on start
- **Check:** Scores are only saved when they're new bests

---

## Database Schema Reference

```sql
multiplayer_leaderboard:
  - id: UUID (Primary Key)
  - username: TEXT (Unique, NOT NULL)
  - user_id: TEXT (Nullable)
  - levels_beaten: INTEGER (NOT NULL, default 0)
  - updated_at: TIMESTAMP (Auto-updated)
```

---

## Code Files Reference

- **`Assets/Scripts/Multiplayer/MultiplayerLeaderboardManager.cs`** - Main leaderboard manager
- **`Assets/Scripts/Multiplayer/LeaderboardPanel.cs`** - UI panel for main menu
- **`Assets/Scripts/MultiplayerLeaderboardEntry.cs`** - Supabase model
- **`Assets/Scripts/MainMenu.cs`** - Main menu with leaderboard button
- **`Assets/Scripts/Multiplayer/SharedMultiplayerGameManager.cs`** - Calls leaderboard updates

---

## Notes

- The leaderboard tracks **best single-run scores**, not cumulative
- Scores are only saved when they beat the player's previous best
- Top 10 players are shown in the main menu
- All players in a lobby see the same leaderboard (synced via RPC)
- The system works even when not in a multiplayer room (shows persistent leaderboard)

