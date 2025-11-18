# Hint Button System Setup Guide

## Overview

The Hint Button system allows you to add hint buttons to any level that display tooltips when clicked. The tooltips automatically disappear after a few seconds.

**NEW: Answer-Linked Hint Buttons** - Hint buttons can now be linked to specific answers. When an answer is marked as correct, the corresponding hint button automatically disappears!

## Components

### 1. HintButton.cs
- Main component that handles hint button functionality
- Attach to any Button GameObject
- Shows tooltip when button is clicked
- Auto-dismisses tooltip after configurable duration
- **NEW**: Can be linked to answer GameObjects - automatically hides when answer is correct

### 2. AnswerHintManager.cs
- Manager component that automatically sets up hint buttons for all answers
- Links hint buttons to their corresponding answers
- Positions hint buttons on top of answers
- Handles all hint button lifecycle automatically

### 3. HintTooltip.cs (Optional)
- Standalone tooltip component with fade animations
- Can be used with custom tooltip prefabs
- Handles fade in/out and auto-dismiss

## Quick Setup

### Method 1: Automatic Setup with AnswerHintManager (Recommended)

This is the easiest way to set up hint buttons for all answers in a level:

1. **Add AnswerHintManager:**
   - In your level scene, create an empty GameObject (or use an existing manager)
   - Add Component > Scripts > AnswerHintManager
   - In the Inspector:
     - **Output Manager**: Drag your OutputManager component (or leave null to auto-find)
     - **Hint Button Prefab**: (Optional) Create a prefab with a Button + HintButton component, or leave null for default
     - **Default Hint Text Template**: Set default text (use {0} for output index, {1} for answer index)
     - **Position On Answers**: Enable to position hint buttons on top of answers
     - **Position Offset**: Adjust offset if needed

2. **Set Custom Hints (Optional):**
   - In AnswerHintManager, expand "Custom Hints"
   - Click "+" to add hint entries
   - Set Output Index, Answer Index, and Hint Text for each answer

3. **Test:**
   - Play the scene
   - Hint buttons should appear on all answers
   - Click hint buttons to see tooltips
   - When you get an answer correct, its hint button should disappear!

### Method 2: Manual Setup (Individual Hint Buttons)

### Method 2a: Simple Setup (No Prefab Required)

1. **Create a Hint Button:**
   - In your level scene, create a Button GameObject (UI > Button)
   - Position it where you want the hint button to appear
   - Optionally change the button text/image to a "?" icon or "Hint" text

2. **Add HintButton Component:**
   - Select the button GameObject
   - Add Component > Scripts > HintButton
   - Configure the settings in the Inspector:
     - **Hint Text**: Enter the hint text you want to display
     - **Tooltip Duration**: How long the tooltip stays visible (2-10 seconds)
     - **Appear Above Button**: If true, tooltip appears above button
     - **Tooltip Offset**: Position offset if not appearing above
     - **Visual Settings**: Customize colors, font size, padding

3. **Test:**
   - Play the scene and click the hint button
   - Tooltip should appear and auto-dismiss after the set duration

### Method 2b: Linking to Answers (Manual)

To manually link hint buttons to answers:

1. **Create Hint Button:**
   - Create a Button GameObject
   - Add HintButton component
   - Set hint text

2. **Link to Answer:**
   - In HintButton component, find "Answer Linking" section
   - **Linked Answer**: Drag the answer GameObject from OutputManager
   - **Hide When Answer Correct**: Enable this (default: true)
   - **Check Interval**: How often to check (default: 0.2 seconds)

3. **Position on Answer:**
   - Position the hint button on top of the answer GameObject
   - Or enable "Parent To Answers" in AnswerHintManager

4. **Test:**
   - When the answer is marked as correct (SetActive(true)), the hint button will automatically hide

### Method 3: Using a Custom Tooltip Prefab

1. **Create Tooltip Prefab:**
   - Create a Panel GameObject (UI > Panel)
   - Add a TextMeshProUGUI child for the hint text
   - Optionally add HintTooltip component for fade animations
   - Style it as desired (background, borders, etc.)
   - Save as a Prefab

2. **Setup Hint Button:**
   - Create button as in Method 1
   - Add HintButton component
   - Assign your tooltip prefab to the **Tooltip Prefab** field
   - Enter hint text and configure other settings

3. **Test:**
   - Play the scene and click the hint button
   - Your custom tooltip will appear

## Configuration Options

### HintButton Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Hint Text** | The text to display in the tooltip | "This is a hint!" |
| **Tooltip Prefab** | Custom tooltip prefab (optional) | null (creates default) |
| **Parent Canvas** | Canvas to spawn tooltip on | Auto-finds Canvas |
| **Tooltip Duration** | How long tooltip stays visible (seconds) | 4 seconds |
| **Tooltip Offset** | Position offset from button | (0, 100) |
| **Appear Above Button** | If true, positions tooltip above button | true |
| **Tooltip Background Color** | Background color for default tooltip | Dark gray (0.1, 0.1, 0.1, 0.95) |
| **Tooltip Text Color** | Text color for hint | White |
| **Font Size** | Font size for hint text | 18 |
| **Padding** | Padding around text | 15 |
| **Linked Answer** | Answer GameObject to link to | null |
| **Hide When Answer Correct** | Auto-hide when answer is correct | true |
| **Check Interval** | How often to check answer status | 0.2 seconds |
| **Dismiss On Panel Minimize** | Auto-dismiss tooltip when expected output panel is minimized | true |
| **Use Default Hint Template** | Fall back to template text when no custom hint exists | false |
| **Hide Buttons Without Hints** | Hide buttons if they have no hint text | false |

### AnswerHintManager Settings

| Setting | Description | Default |
|---------|-------------|---------|
| **Output Manager** | Reference to OutputManager | Auto-finds |
| **Hint Button Prefab** | Prefab for hint buttons | null (creates default) |
| **Default Hint Text Template** | Template for hint text | "Hint for answer {1}" |
| **Custom Hints** | List of custom hints per answer | Empty |
| **Position On Answers** | Position buttons on answers | true |
| **Position Offset** | Offset from answer position | (0, 0) |
| **Parent To Answers** | Parent buttons to answers | false |

### HintTooltip Settings (if using prefab)

| Setting | Description | Default |
|---------|-------------|---------|
| **Fade In Duration** | Fade in animation time | 0.3 seconds |
| **Fade Out Duration** | Fade out animation time | 0.3 seconds |
| **Display Duration** | How long tooltip stays visible | 4 seconds |
| **Auto Dismiss** | Automatically dismiss after duration | true |

## Usage Examples

### Example 1: Automatic Setup with AnswerHintManager

```csharp
// In Unity Inspector:
// 1. Add AnswerHintManager to a GameObject
// 2. Assign OutputManager (or leave null to auto-find)
// 3. Set Default Hint Text Template: "Hint: Try card {1}!"
// 4. Enable "Position On Answers"
// 5. Play scene - hint buttons automatically created for all answers!
```

### Example 2: Basic Hint Button (Manual)

```csharp
// In Unity Inspector:
// 1. Create Button
// 2. Add HintButton component
// 3. Set Hint Text: "Try using the red card first!"
// 4. Set Tooltip Duration: 5 seconds
```

### Example 3: Answer-Linked Hint Buttons

Link hint buttons to specific answers so they disappear when correct:

```csharp
// Method 1: Using AnswerHintManager (Automatic)
// - Add AnswerHintManager component
// - It automatically creates and links hint buttons for all answers

// Method 2: Manual Linking
HintButton hintButton = GetComponent<HintButton>();
GameObject answerObject = outputManager.answerListContainer[0].answers[0];
hintButton.LinkToAnswer(answerObject);
// Hint button will hide when answerObject.SetActive(true) is called
```

### Example 4: Custom Hints Per Answer

Use AnswerHintManager's Custom Hints list:

1. In AnswerHintManager, expand "Custom Hints"
2. Add entry: Output Index = 0, Answer Index = 0, Hint Text = "Use the attack card!"
3. Add entry: Output Index = 0, Answer Index = 1, Hint Text = "Try the defense card next!"
4. All other answers use the default template

### Example 5: Programmatic Setup

```csharp
// Get hint button component
HintButton hintButton = GetComponent<HintButton>();

// Set hint text programmatically
hintButton.SetHintText("This is a dynamic hint!");

// Set tooltip duration
hintButton.SetTooltipDuration(6f);

// Link to answer
GameObject answer = outputManager.answerListContainer[0].answers[0];
hintButton.LinkToAnswer(answer);

// Check if answer is correct
if (hintButton.IsAnswerCorrect())
{
    Debug.Log("Answer is correct!");
}
```

## Tips

1. **Answer-Linked Hint Buttons:**
   - Use AnswerHintManager for automatic setup - it handles everything!
   - Hint buttons automatically hide when answers are correct
   - Each hint button corresponds to one answer
   - Number of hint buttons = number of correct answers needed

2. **Button Design:**
   - Use a "?" icon or lightbulb icon for hint buttons
   - Make buttons small and unobtrusive
   - Position them on top of answers (AnswerHintManager does this automatically)

3. **Tooltip Positioning:**
   - Enable "Appear Above Button" to prevent tooltips from going off-screen
   - Adjust Tooltip Offset if you need custom positioning
   - Tooltip automatically adjusts to stay within screen bounds

4. **Answer Positioning:**
   - Enable "Position On Answers" in AnswerHintManager to automatically position hint buttons
   - Use "Parent To Answers" if you want hint buttons to move with answers
   - Adjust Position Offset to fine-tune placement

5. **Custom Hints:**
   - Use AnswerHintManager's Custom Hints list for level-specific hints
   - Default template is used for answers without custom hints
   - Custom hints override the template

6. **Performance:**
   - Check Interval controls how often hint buttons check answer status
   - Lower values = more responsive but more CPU usage
   - Default 0.2 seconds is usually sufficient

7. **Single Tooltip Display:**
   - Only one tooltip is visible at a time
   - Clicking a hint button automatically dismisses any previously open tooltip from another button
   - This keeps the UI clean and prevents tooltip overlap

8. **Panel Integration:**
   - Tooltips automatically dismiss when the expected output panel is minimized
   - This prevents tooltips from staying visible when the panel is hidden
   - Enable "Dismiss On Panel Minimize" in HintButton to use this feature (enabled by default)
   - Use "Hide Buttons Without Hints" in AnswerHintManager if you only want buttons when real hint text exists

9. **Custom Styling:**
   - Create a custom hint button prefab for consistent styling
   - Create a custom tooltip prefab for consistent tooltip styling
   - Use HintTooltip component for smooth fade animations
   - Default tooltip text now uses a larger 24pt font and yellow color for readability

## Troubleshooting

### Tooltip doesn't appear
- Check that the button has a Button component
- Ensure there's a Canvas in the scene
- Check console for error messages

### Tooltip appears in wrong position
- Adjust "Appear Above Button" setting
- Modify "Tooltip Offset" values
- Tooltip should auto-adjust to stay on screen

### Tooltip doesn't dismiss
- Check "Tooltip Duration" is set correctly
- If using custom prefab, ensure HintTooltip component is added
- Check that auto-dismiss is enabled

### Hint button doesn't hide when answer is correct
- Check that "Linked Answer" is assigned in HintButton
- Verify "Hide When Answer Correct" is enabled
- Ensure the answer GameObject is being set to active (SetActive(true)) when correct
- Check Check Interval - try lowering it if answer is correct but button doesn't hide
- Verify OutputManager structure matches expected format
- If the tooltip only shows the placeholder text, add a Custom Hint entry for that output/answer or enable "Use Default Hint Template" if you want placeholders

### Tooltip text is cut off
- Increase padding in Visual Settings
- Make tooltip prefab larger
- Reduce font size if needed

## Advanced Usage

### Using AnswerHintManager Programmatically

```csharp
// Get AnswerHintManager
AnswerHintManager hintManager = FindObjectOfType<AnswerHintManager>();

// Get hint button for specific answer
HintButton hintButton = hintManager.GetHintButton(outputIndex: 0, answerIndex: 0);

// Manually hide/show hint button
hintManager.HideHintButton(0, 0);
hintManager.ShowHintButton(0, 0);

// Clear all hint buttons and recreate
hintManager.ClearHintButtons();
hintManager.SetupHintButtons();
```

### Creating Custom Hint Button Prefab

1. Create a Button GameObject with your desired styling
2. Add HintButton component
3. Configure default settings (hint text, tooltip duration, etc.)
4. Save as Prefab
5. Assign to AnswerHintManager's "Hint Button Prefab" field
6. All hint buttons will use this prefab

### Manual Answer Linking

```csharp
// Link hint button to answer manually
HintButton hintButton = GetComponent<HintButton>();
OutputManager outputManager = FindObjectOfType<OutputManager>();

// Link to first answer of first output
GameObject answer = outputManager.answerListContainer[0].answers[0];
hintButton.LinkToAnswer(answer);

// The hint button will now hide when answer.SetActive(true) is called
```

## Notes

- Tooltips are automatically destroyed when dismissed
- **Only one tooltip is visible at a time** - clicking a hint button automatically dismisses any previously open tooltip from another button
- Tooltips work in both single-player and multiplayer modes
- Tooltips respect Canvas sorting order (appear on top)
- **Answer-linked hint buttons automatically hide when their answer is correct**
- **AnswerHintManager automatically creates hint buttons for all answers in OutputManager**
- **Each hint button corresponds to one answer - number of hint buttons = number of correct answers needed**
- Hint buttons check answer status periodically (configurable Check Interval)
- When an answer is marked as correct (SetActive(true)), the linked hint button hides immediately
- **Tooltips automatically dismiss when the expected output panel is minimized** (prevents tooltips from staying visible when panel is hidden)

