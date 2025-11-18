# ?? QUICK START: Tutorial Lore Dialogue

## **? FILES CREATED:**
- `Assets/Scripts/TutorialLoreDialogue.cs` - Main system
- `TUTORIAL_LORE_DIALOGUE_SYSTEM_GUIDE.md` - Full guide

## **? 5-MINUTE SETUP:**

### **1. Create UI (Tutorial Scene):**
```
Canvas
??? LoreDialoguePanel (full screen, dark background)
    ??? DialogueText (TMP, centered, white)
    ??? ContinueIndicator (Image, bottom-right, arrow)
```

### **2. Add Script:**
- Canvas ? Add Component ? **TutorialLoreDialogue**
- Assign: LoreDialoguePanel, DialogueText, ContinueIndicator, TutorialPanel

### **3. Add Dialogue:**
In Inspector ? Dialogue Content ? Click "+"

**Example:**
```
Entry 1:
  Text: "Welcome, young warrior. You stand at the threshold of a great journey."

Entry 2:
  Text: "Long ago, this land was filled with magic and wonder. But darkness crept in..."

Entry 3:
  Text: "The ancient cards hold the power to restore balance. Master them to succeed."

Entry 4:
  Text: "Let me teach you the ways of combat. Pay close attention!"
```

### **4. Test:**
- Play Tutorial scene
- Lore dialogue shows automatically
- Click to advance through dialogues
- After last dialogue ? Tutorial panel appears

---

## **?? HOW IT WORKS:**

```
Scene Loads ? Lore Starts ? Letter-by-letter ? Click ? Next ? ... ? Last ? Tutorial Panel
```

---

## **?? CONTROLS:**
- **Click** = Advance dialogue
- **Click during typing** = Skip animation
- **Space/Enter** = Also advance

---

## **?? CUSTOMIZE:**

### **Speed:**
- Letter Delay: 0.05 (adjust in Inspector)

### **Content:**
- Add more dialogue entries in Inspector
- Write your own lore text

### **Visual:**
- Change font size (DialogueText)
- Change colors
- Add continue indicator animation

---

## **?? FULL GUIDE:**
See `TUTORIAL_LORE_DIALOGUE_SYSTEM_GUIDE.md` for complete instructions!

---

**Status:** ? Ready to use!  
**Time:** 5 minutes to setup  
**Result:** Cinematic story intro! ??
