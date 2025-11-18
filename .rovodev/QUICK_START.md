# CodexRedemo - Rovo Dev Quick Start Guide

## ✅ Integration Complete!

Rovo Dev is now integrated with your CodexRedemo Unity project. This guide will help you get started.

## What Was Set Up

1. **AGENTS.md** - Project context file that helps Rovo Dev understand:
   - Project structure and organization
   - Coding conventions and patterns
   - Key technologies and integrations
   - Common tasks and workflows
   - Unity-specific considerations

2. **Project Configuration** - Local Rovo Dev settings for:
   - File patterns to watch (C# scripts, Unity assets, etc.)
   - Files to ignore (Unity cache, build outputs)
   - Project-specific context and hints

## How to Use Rovo Dev with CodexRedemo

### Starting a Session

From the **workspace root** (where `.rovodev/` is located), run:
```powershell
# Navigate to workspace root if not already there
cd C:\Users\Vince

# Start Rovo Dev - it will work across your entire workspace
rovodev
```

Or navigate to your project directory:
```powershell
cd CodexRedemo\CodexWebGL-main
rovodev
```

### Working with Your Unity Project

Rovo Dev can help you with:

#### 🎮 Game Development
- "Add a new card type with X effect"
- "Create a new character with Y animations"
- "Implement a new game mechanic"
- "Fix the bug in the multiplayer lobby"

#### 🎨 Unity-Specific Tasks
- "Analyze the character animation system"
- "Help me set up a new scene for level 7"
- "Create a new ScriptableObject for card data"
- "Debug why the card display isn't working"

#### 📝 Code Improvements
- "Refactor the CardManager to be more modular"
- "Add error handling to the save system"
- "Optimize the character loading logic"
- "Review the multiplayer synchronization code"

#### 📚 Documentation
- "Document the card system architecture"
- "Create a guide for adding new levels"
- "Explain how the Supabase integration works"

### Example Workflow

```
> Help me add a new card for level 3

[Rovo Dev will:]
1. Check AGENTS.md for card system patterns
2. Look at existing cards in Assets/Items/Level 3 Cards/
3. Create a new ScriptableObject asset
4. Follow naming conventions (e.g., 3015newCard.asset)
5. Guide you on integrating it into the game

> Fix the character not animating in multiplayer

[Rovo Dev will:]
1. Reference multiplayer documentation
2. Check CharacterAnimationController and Multiplayer.cs
3. Identify the issue
4. Propose and implement a fix
5. Suggest testing steps
```

## Best Practices

### 1. Be Specific About Unity Context
Instead of: "Fix the animation"
Better: "Fix the character idle animation not playing in the multiplayer scene"

### 2. Reference Documentation
Rovo Dev has access to all your project docs:
- "Following the pattern in CARD_PANEL_SETUP_GUIDE.md, create..."
- "Based on the multiplayer documentation, implement..."

### 3. Test in Unity
After Rovo Dev makes changes:
1. Open Unity Editor
2. Let Unity recompile scripts
3. Test in play mode
4. Report back any issues to Rovo Dev

### 4. Leverage Project Knowledge
Rovo Dev understands your:
- Naming conventions (PascalCase for public, camelCase for private)
- File organization (Scripts/, Prefabs/, Items/)
- Existing patterns (CardData, CharacterLoader, etc.)

## Tips & Tricks

### Multi-File Changes
Rovo Dev can modify multiple files at once:
```
> Add a new card type that requires changes to CardManager, 
  CardDisplay, and create a new ScriptableObject
```

### Code Review
```
> Review the SaveLoadUI.cs script for potential issues
> Check if the multiplayer synchronization follows best practices
```

### Architecture Questions
```
> Explain how the card system works
> What's the best way to add a new animation type?
> How does Supabase authentication integrate with Unity?
```

### Bug Investigation
```
> Debug why cards aren't displaying in level 5
> Investigate the null reference error in CharacterSelectionManager
```

## Useful Commands

Within Rovo Dev interactive mode:

- `/help` - Show available commands
- `/clear` - Clear conversation history
- `/exit` - Exit Rovo Dev
- `/mode` - Switch between agent/ask modes

## Project-Specific Notes

### Unity Editor Required
Some changes need Unity Editor to:
- Recompile C# scripts
- Generate .meta files
- Update scene/prefab references
- Test gameplay changes

### WebGL Builds
When building for WebGL:
1. Rovo Dev can modify scripts and assets
2. You must use Unity Editor to build
3. Test in a web server (not file:// protocol)

### Multiplayer Testing
For multiplayer features:
1. Build and test with multiple clients
2. Check Photon dashboard for connection issues
3. Report findings back to Rovo Dev for fixes

## Getting Help

### From Rovo Dev
- Ask about the project: "Explain the card system"
- Request documentation: "Document this feature"
- Get guidance: "What's the best approach to..."

### Documentation
Your project has extensive docs:
- Start with **MASTER_INDEX.md**
- Check **AGENTS.md** for patterns
- Specific guides for features (CARD_*, CHARACTER_*, MULTIPLAYER_*)

## Next Steps

1. **Start Rovo Dev**: `rovodev` from your workspace
2. **Try a simple task**: "Explain the project structure"
3. **Make a change**: "Add a TODO comment to the main menu script"
4. **Build something**: "Create a new card for the tutorial level"

## Troubleshooting

### Rovo Dev not seeing project files
- Ensure you're in the correct directory
- Check that AGENTS.md exists in the project root

### Changes not reflected in Unity
- Let Unity recompile (check bottom-right corner)
- Refresh assets (Ctrl+R in Unity)

### Permission issues
- Check .rovodev/config.yml tool permissions
- Allow file operations when prompted

---

**Ready to start developing with Rovo Dev!** 🚀

Try asking: "Analyze the CodexRedemo project structure and suggest improvements"
