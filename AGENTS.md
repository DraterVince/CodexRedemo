# Rovo Dev Agent Guide - CodexRedemo Project

## Project Overview
**CodexRedemo** is a Unity WebGL game project featuring:
- Turn-based card gameplay mechanics
- Multiplayer support via Photon
- Character animation system with sprite sheets
- Supabase integration for authentication and data persistence
- Educational coding-themed card game elements

## Project Structure

### Key Directories
- **Assets/Scripts/** - All C# game scripts
  - `Scripts/Multiplayer/` - Multiplayer-specific logic
  - `Scripts/Settings/` - Game settings management
  - `Scripts/LevelLoader/` - Level loading systems
  - `Scripts/Input/` - Input handling
  - `Scripts/Editor/` - Unity Editor tools and automation scripts

- **Assets/Scenes/** - Unity scene files
  - `MainMenu.unity` - Main menu scene
  - `LoginScene.unity` - Authentication scene
  - `Multiplayer.unity` - Multiplayer game scene
  - `Singleplayer.unity` - Single player game scene
  - `TutorialLevel.unity` - Tutorial level
  - `Part 1 Levels/` & `Part 2 Levels/` - Game levels
  - `Levels with Hint/` - Levels with hint system

- **Assets/Prefabs/** - Reusable Unity prefabs
  - `Card Prefabs/` - Card-related prefabs
  - `Multiplayer/` - Multiplayer UI/gameplay prefabs

- **Assets/Items/** - ScriptableObject card data
  - `Level X Cards/` - Cards organized by level
  - `TutorialCards/` - Tutorial-specific cards

- **Assets/Editor/** - Editor automation tools for animations, sprite sheets, etc.

### Important Files
- **MASTER_INDEX.md** - Comprehensive documentation index
- **BuildWebGL/** - WebGL build output directory
- **Packages/manifest.json** - Unity package dependencies

## Technologies & Integrations

### Core Technologies
- **Unity 2022.x+** (WebGL target)
- **C# .NET** scripting
- **Photon Unity Networking (PUN)** - Multiplayer
- **Supabase** - Backend (Auth, Database, Storage)
- **TextMesh Pro** - UI text rendering

### External Dependencies
- Supabase client libraries (via NuGet packages in Assets/Packages/)
- Photon realtime and chat SDKs
- Various sprite/animation assets

## Development Guidelines

### Coding Conventions
1. **Naming**:
   - PascalCase for public methods and properties
   - camelCase for private fields
   - Use descriptive names (e.g., `CardDisplayUI`, `CharacterSelectionManager`)

2. **ScriptableObjects**:
   - Card data stored as ScriptableObject assets
   - Located in `Assets/Items/` organized by level
   - Follow naming pattern: `{ID}{name}Card.asset` (e.g., `1001classCard.asset`)

3. **Prefabs**:
   - UI prefabs should be self-contained with required components
   - Character prefabs stored in `Assets/Resources/Characters/`
   - Use prefab variants for character customization

4. **Animation System**:
   - Automated setup via Editor scripts in `Assets/Editor/`
   - Support for sprite sheet animations
   - Multi-animation controller for characters
   - Jump attack animation system

### Important Patterns

#### Card System
```csharp
// Cards use CardData ScriptableObject
// CardManager handles card logic
// CardDisplay/CardDisplayUI handle rendering
```

#### Save System
- Uses `PlayerDataManager` and `PlayerData` classes
- Integrates with Supabase for cloud saves
- Local save slots via `SaveLoadSlot` and `SaveLoadUI`

#### Character System
- `CharacterLoader` - Loads character prefabs from Resources
- `CharacterSelectionManager` - Handles character selection UI
- `CharacterAnimationController` - Controls character animations
- `MultiAnimationController` - Supports multiple animation sets

#### Multiplayer
- Photon PUN for networking
- `Multiplayer.cs` - Main multiplayer game logic
- `PlayerListEntry.cs` - Player list UI
- `MultiplayerLeaderboardEntry.cs` - Leaderboard entries

### WebGL Specific Notes
- Custom WebGL templates in `Assets/WebGLTemplates/`
- `SupabaseJSBridge.cs` bridges Unity and JavaScript
- Google OAuth integration with special handling for WebGL
- External eval fixes documented in `BUILD_FIX_EXTERNALEVAL.md`

## Common Tasks

### Adding a New Card
1. Create ScriptableObject asset in appropriate `Assets/Items/Level X Cards/` folder
2. Set card ID, name, description, sprite
3. Add to CardManager's card list in the scene
4. Test in appropriate level scene

### Adding a New Character
1. Create character sprite sheets in `Assets/Resources/Characters/`
2. Use Editor tools to auto-generate animations
3. Create character prefab with required components
4. Register in CharacterSelectionManager

### Building for WebGL
1. Use Unity Build Settings → WebGL
2. Select custom WebGL template (SupabaseTemplate)
3. Build to `BuildWebGL/` directory
4. Test locally or deploy to hosting service

## Documentation References
The project has extensive documentation in markdown files at the root:
- **MASTER_INDEX.md** - Main documentation hub
- **COMPILATION_FIX_GUIDE.md** - Troubleshooting compilation issues
- **MULTIPLAYER_CHARACTER_SPAWN_INFO.md** - Multiplayer character setup
- **TUTORIAL_LORE_DIALOGUE_SYSTEM_GUIDE.md** - Tutorial system docs
- **CARD_PANEL_SETUP_GUIDE.md** - Card UI setup
- Many other specific feature guides (see MASTER_INDEX.md)

## Known Issues & Fixes
- **Google OAuth 403 errors**: See `GOOGLE_OAUTH_403_ITCHIO_URL_FIX.md`
- **Card timing issues**: See `CARD_TIMING_FIX.md`
- **Button click blocking**: See `FIX_BUTTON_CHILD_BLOCKING_CLICKS.md`
- **Character loading**: See `CHARACTER_LOADING_FIX.md`

## Testing
- Test in Unity Editor first before WebGL builds
- WebGL builds require a web server (don't open index.html directly)
- Use Unity's Build and Run for quick testing
- Test multiplayer with multiple browser tabs/windows

## Best Practices for Rovo Dev Agent

### When Making Changes
1. **Read relevant documentation** from the markdown files first
2. **Check MASTER_INDEX.md** for related documentation
3. **Preserve existing patterns** in the codebase
4. **Test changes** in Unity Editor when possible
5. **Update documentation** if adding significant features

### File Organization
- Keep scripts organized in appropriate subdirectories
- Use `.meta` files (Unity requirement - don't delete)
- Follow existing naming conventions
- Place Editor scripts only in `Assets/Editor/` folders

### Unity-Specific Considerations
- Scripts must inherit from `MonoBehaviour` for Unity components
- Use `[SerializeField]` for inspector-visible private fields
- Coroutines for async operations (not async/await for MonoBehaviour)
- Use Unity's lifecycle methods (Start, Update, OnEnable, etc.)

## Quick Start for Common Requests

**"Add a new level"**: Check scenes in `Assets/Scenes/Part X Levels/`, duplicate existing level, modify

**"Fix a bug in multiplayer"**: Check `Assets/Scripts/Multiplayer.cs` and related multiplayer docs

**"Add new card type"**: Create ScriptableObject in `Assets/Items/`, follow existing card pattern

**"Update character animations"**: Use Editor tools in Unity menu or check `Assets/Editor/` scripts

**"Build issues"**: Consult `COMPILATION_FIX_GUIDE.md` and related fix documentation

---

*This file helps Rovo Dev understand your project. Keep it updated as the project evolves!*
